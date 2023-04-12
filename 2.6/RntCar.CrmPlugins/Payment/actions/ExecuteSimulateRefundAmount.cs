using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.CrmPlugins.Payment.actions
{
    public class ExecuteSimulateRefundAmount : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            var simulateRefundResponse = new SimulateRefundAmountResponse();
            try
            {
                string simulateRefundRequest;
                initializer.PluginContext.GetContextParameter<string>("SimulateRefundAmountRequest", out simulateRefundRequest);

                var param = JsonConvert.DeserializeObject<SimulateRefundAmountRequest>(simulateRefundRequest);
                initializer.TraceMe("SimulateRefundAmountRequest : " + simulateRefundRequest);

                ConfigurationRepository configurationRepository = new ConfigurationRepository(initializer.Service);
                var maturityDifferenceCode = configurationRepository.GetConfigurationByKey("additionalProduct_MaturityDifference");
                var existingMaturityAdditionalProductCode = param.additionalProducts.Where(p => p.productCode == maturityDifferenceCode).FirstOrDefault();
                if (existingMaturityAdditionalProductCode == null)
                {
                    initializer.TraceMe("existingMaturityAdditionalProductCode == null");

                    simulateRefundResponse = new SimulateRefundAmountResponse
                    {
                        ResponseResult = ResponseResult.ReturnSuccess(),
                        totalAmount = param.additionalProducts.Sum(p => p.actualTotalAmount),
                        totaltobePaidAmount = param.additionalProducts.Sum(p => p.tobePaidAmount),
                        additionalProducts = param.additionalProducts
                    };
                    initializer.PluginContext.OutputParameters["SimulateRefundAmountResponse"] = JsonConvert.SerializeObject(simulateRefundResponse);
                    return;
                }
                PaymentRepository paymentRepository = new PaymentRepository(initializer.Service);
                var records = param.reservationId.HasValue ?
                                paymentRepository.getNotRefundedPayments_Reservation(param.reservationId.Value)
                              : param.contractId.HasValue ?
                                    paymentRepository.getNotRefundedPayments_Contract(param.contractId.Value, PaymentEnums.PaymentTransactionType.SALE)
                                    :
                                    null;

                List<PaymentObject> paymentObjects = new List<PaymentObject>();
                var amounttobeRefund = param.refundAmount * -1;
                var refundMaturity = decimal.Zero;
                initializer.TraceMe("amounttobeRefund: " + amounttobeRefund);

                //todo will make a generic  with paymentBL --> Createrefund parameters
                foreach (var item in records)
                {
                    var paymentAmount = decimal.Zero;
                    if (item.Attributes.Contains("rnt_refundamount"))
                    {
                        paymentAmount = item.GetAttributeValue<Money>("rnt_amount").Value - item.GetAttributeValue<Money>("rnt_refundamount").Value;
                    }
                    else
                    {
                        paymentAmount = item.GetAttributeValue<Money>("rnt_amount").Value;
                    }

                    var refundAmount = decimal.Zero;
                    //partially refund 
                    if (paymentAmount > amounttobeRefund)
                    {
                        initializer.TraceMe("payment is bigger than amounttobeRefund , payment amount : " + paymentAmount);
                        refundAmount = amounttobeRefund;
                        paymentObjects.Add(new PaymentObject
                        {
                            refundStatus = (int)ClassLibrary._Enums_1033.rnt_RefundStatus.PartiallyRefund,
                            paymentId = item.Id,
                            refundAmount = refundAmount
                        });

                    }
                    //totally refund
                    else if (paymentAmount == amounttobeRefund)
                    {
                        initializer.TraceMe("payment is equal amounttobeRefund");
                        refundAmount = amounttobeRefund;
                        paymentObjects.Add(new PaymentObject
                        {
                            refundStatus = (int)ClassLibrary._Enums_1033.rnt_RefundStatus.TotallyRefund,
                            paymentId = item.Id,
                            refundAmount = refundAmount
                        });
                    }
                    //if amount to be refund is bigger than the payment we need to iterate more
                    else if (paymentAmount < amounttobeRefund)
                    {
                        initializer.TraceMe("payment is less than amounttobeRefund");
                        refundAmount = paymentAmount;
                        paymentObjects.Add(new PaymentObject
                        {
                            refundStatus = (int)ClassLibrary._Enums_1033.rnt_RefundStatus.TotallyRefund,
                            paymentId = item.Id,
                            refundAmount = refundAmount
                        });
                    }

                    initializer.TraceMe("refundAmount : " + refundAmount);
                    initializer.TraceMe("payment amount : " + paymentAmount);

                    var refundAmount_WithoutMaturity = decimal.Zero;
                    var totalRefundRatio = (item.GetAttributeValue<decimal>("rnt_installmentratio") / 100) + 1;
                    if (paymentAmount > amounttobeRefund)
                    {
                        refundAmount_WithoutMaturity = refundAmount * (totalRefundRatio);
                        refundAmount_WithoutMaturity = decimal.Round(refundAmount_WithoutMaturity, 2);
                        initializer.TraceMe("payment amount after ratio division and rounding: " + refundAmount_WithoutMaturity);

                        amounttobeRefund = amounttobeRefund - refundAmount;
                        initializer.TraceMe("amounttobeRefund : " + amounttobeRefund);

                        refundMaturity += refundAmount_WithoutMaturity - refundAmount;
                        initializer.TraceMe("refundMaturity : " + refundMaturity);
                    }
                    else
                    {
                        refundAmount_WithoutMaturity = refundAmount /(totalRefundRatio);

                        refundAmount_WithoutMaturity = decimal.Round(refundAmount_WithoutMaturity, 2);
                        initializer.TraceMe("payment amount after ratio division and rounding: " + refundAmount_WithoutMaturity);

                        amounttobeRefund = amounttobeRefund - refundAmount_WithoutMaturity;
                        initializer.TraceMe("amounttobeRefund : " + amounttobeRefund);

                        refundMaturity += refundAmount - refundAmount_WithoutMaturity;
                        initializer.TraceMe("refundMaturity : " + refundMaturity);
                    }

                    
                    if (amounttobeRefund <= 0)
                    {
                        break;
                    }
                }

                var actualAmount = existingMaturityAdditionalProductCode.actualAmount.Value - refundMaturity;

                existingMaturityAdditionalProductCode.tobePaidAmount = refundMaturity * -1;
                existingMaturityAdditionalProductCode.actualAmount = actualAmount;
                existingMaturityAdditionalProductCode.actualTotalAmount = actualAmount;
                

                initializer.TraceMe("existingMaturityAdditionalProductCode.tobePaidAmount : " + existingMaturityAdditionalProductCode.tobePaidAmount);
                initializer.TraceMe("existingMaturityAdditionalProductCode.actualAmount : " + existingMaturityAdditionalProductCode.actualAmount);
                initializer.TraceMe("existingMaturityAdditionalProductCode.actualTotalAmount : " + existingMaturityAdditionalProductCode.actualTotalAmount);

                simulateRefundResponse = new SimulateRefundAmountResponse
                {
                    totalAmount = param.additionalProducts.Sum(p => p.actualTotalAmount),
                    totaltobePaidAmount = param.additionalProducts.Sum(p => p.tobePaidAmount),
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    additionalProducts = param.additionalProducts
                };
                initializer.PluginContext.OutputParameters["SimulateRefundAmountResponse"] = JsonConvert.SerializeObject(simulateRefundResponse);
            }
            catch (Exception ex)
            {
                simulateRefundResponse = new SimulateRefundAmountResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
                initializer.PluginContext.OutputParameters["SimulateRefundAmountResponse"] = JsonConvert.SerializeObject(simulateRefundResponse);
            }
        }
    }
}
