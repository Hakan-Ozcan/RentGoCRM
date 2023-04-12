using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Validations
{
    public class ManualPaymentValidations : ValidationHandler
    {
        public ManualPaymentValidations(IOrganizationService orgService) : base(orgService)
        {
        }

        public ManualPaymentValidations(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public ManualPaymentValidations(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public CheckBeforeManualPaymentResponse checkBeforeManualPayment(ManualPaymentParameters parameters, decimal manualPaymentAmount, Entity contract, Entity reservation)
        {
            if (parameters.manualPaymentType == (int)ClassLibrary._Enums_1033.rnt_ManualPaymentTypeCode.OnlyPayment ||
                parameters.manualPaymentType == (int)ClassLibrary._Enums_1033.rnt_ManualPaymentTypeCode.AddAdditionalProductWithPayment)
            {
                return new CheckBeforeManualPaymentResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            else if (parameters.manualPaymentType == (int)ClassLibrary._Enums_1033.rnt_ManualPaymentTypeCode.OnlyRefund)
            {
                PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
                var payment = paymentRepository.getDepositRefund(parameters.reservationId, parameters.contractId, new string[] { });

                this.Trace("payment deposit " + payment);
                Entity entity = null;
                if (contract != null && contract.Id != Guid.Empty)
                {
                    entity = contract;
                }
                else
                {
                    entity = reservation;
                }
                if (entity == null)
                {
                    return new CheckBeforeManualPaymentResponse
                    {
                        ResponseResult = ResponseResult.ReturnError("Reservation or contract is null!")
                    };
                }

                var netPayment = entity.GetAttributeValue<Money>("rnt_netpayment").Value;
                var totalAmount = entity.GetAttributeValue<Money>("rnt_totalamount").Value;
                var depositAmount = entity.GetAttributeValue<Money>("rnt_depositamount").Value;

                if (parameters.reservationId.HasValue)
                {
                    depositAmount = decimal.Zero;
                }
                this.Trace("netPayment " + netPayment);
                this.Trace("totalAmount " + totalAmount);
                this.Trace("depositAmount " + depositAmount);

                if (parameters.contractId.HasValue)
                {
                    if (entity.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker &&
                      entity.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contract_StatusCode.Rental)
                    {
                        totalAmount = entity.GetAttributeValue<Money>("rnt_generaltotalamount").Value;
                    }
                }

                //teminat iadesi gerçekleşmemiş
                if (payment == null)
                {
                    var diff = netPayment - totalAmount - depositAmount;
                    if (manualPaymentAmount > diff)
                    {
                        //paybroker için kontrol 
                        this.Trace("manualpay in if " + manualPaymentAmount);
                        this.Trace("diff in if " + diff);
                        XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                        var message = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("ExceedManualPaymentRefund", parameters.langId), diff);
                        this.Trace("err message " + message);
                        return new CheckBeforeManualPaymentResponse
                        {
                            ResponseResult = ResponseResult.ReturnError(message)
                        };
                    }
                }
                else
                {
                    var diff = netPayment - totalAmount;
                    if (manualPaymentAmount > diff)
                    {
                        this.Trace("manualpay in else " + manualPaymentAmount);
                        this.Trace("diff in else " + diff);

                        XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                        var message = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("ExceedManualPaymentRefund", parameters.langId), diff);
                        this.Trace("err message " + message);
                        return new CheckBeforeManualPaymentResponse
                        {
                            ResponseResult = ResponseResult.ReturnError(message)
                        };
                    }
                }
            }

            return new CheckBeforeManualPaymentResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
    }
}
