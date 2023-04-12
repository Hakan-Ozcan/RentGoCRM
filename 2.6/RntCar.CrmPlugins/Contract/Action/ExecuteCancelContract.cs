using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Contract.Action
{
    public class ExecuteCancelContract : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                int langId;
                initializer.PluginContext.GetContextParameter<int>("langId", out langId);

                string contractId;
                initializer.PluginContext.GetContextParameter<string>("contractId", out contractId);

                string pnrNumber;
                initializer.PluginContext.GetContextParameter<string>("pnrNumber", out pnrNumber);

                int cancellationReason;
                initializer.PluginContext.GetContextParameter<int>("cancellationReason", out cancellationReason);

                string cancellationDescription;
                initializer.PluginContext.GetContextParameter<string>("cancellationDescription", out cancellationDescription);

                initializer.TraceMe("langId: " + langId);
                initializer.TraceMe("contractId: " + contractId);
                initializer.TraceMe("pnrNumber: " + pnrNumber);
                initializer.TraceMe("cancellationReason: " + cancellationReason);
                initializer.TraceMe("cancellationDescription: " + cancellationDescription);
                string cancellationBy = cancellationReason == (int)ReservationEnums.CancellationReason.ByCustomer ? "Customer" : "Rentgo";


                AnnotationBL annotationBL = new AnnotationBL(initializer.Service);
                var annotationId = annotationBL.createNewAnnotation(new ClassLibrary.AnnotationData
                {
                    Subject = "Cancelled By " + cancellationBy,
                    NoteText = cancellationDescription,
                    ObjectId = new Guid(contractId),
                    ObjectName = "rnt_contract"
                });

                ContractBL contractBL = new ContractBL(initializer.Service, initializer.TracingService);
                var validationResponse = contractBL.checkBeforeContractCancellation(new ClassLibrary.ContractCancellationParameters
                {
                    pnrNumber = pnrNumber,
                    contractId = new Guid(contractId)
                }, langId);

                initializer.TraceMe("validationResponse : " + JsonConvert.SerializeObject(validationResponse));
                if (!validationResponse.ResponseResult.Result)
                {
                    initializer.PluginContext.OutputParameters["ContractCancellationResponse"] = JsonConvert.SerializeObject(validationResponse);
                    return;
                }
                ContractItemRepository contractItemRepository = new ContractItemRepository(initializer.Service);
                var item = contractItemRepository.getDiscountContractItem(new Guid(contractId), new string[] { });
                if (item != null)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    xrmHelper.setState("rnt_contractitem", item.Id, 1, 2);
                }

                validationResponse = contractBL.calculateCancellationAmountForGivenContractByCancellationReason(validationResponse,
                                                                                                      new Guid(contractId),
                                                                                                      validationResponse.willChargeFromUser,
                                                                                                      langId,
                                                                                                      cancellationReason);
                initializer.TraceMe("validationResponse1 : " + JsonConvert.SerializeObject(validationResponse));

                if (!validationResponse.ResponseResult.Result)
                {
                    initializer.PluginContext.OutputParameters["ContractCancellationResponse"] = JsonConvert.SerializeObject(validationResponse);
                    return;
                }

                initializer.TraceMe("validations are done");
                initializer.TraceMe("refund amount " + validationResponse.refundAmount);
                initializer.TraceMe("fine amount " + validationResponse.fineAmount);
                initializer.TraceMe("deactivation start");


                
                ContractItemResponse contractItemResponse = new ContractItemResponse();
                contractBL.cancelContract(contractId, cancellationReason, validationResponse.fineAmount, contractItemResponse, validationResponse.isCorporateContract);

                initializer.TraceMe("deactivation end");

                if (validationResponse.refundAmount != decimal.Zero)
                {
                    initializer.TraceMe("refund start");
                    PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
                    paymentBL.createRefund(new CreateRefundParameters
                    {
                        refundAmount = validationResponse.refundAmount,
                        contractId = new Guid(contractId),
                        langId = langId
                    });
                    initializer.TraceMe("refund end");
                }

                ContractRepository contractRepository = new ContractRepository(initializer.Service);
                var contract = contractRepository.getContractById(new Guid(contractId), new string[] { "rnt_depositamount" });
                var depositAmount = contract.Attributes.Contains("rnt_depositamount") ? contract.GetAttributeValue<Money>("rnt_depositamount").Value : decimal.Zero;
                if (depositAmount != decimal.Zero)
                {
                    initializer.TraceMe("deposit refund start");
                    PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
                    paymentBL.createRefundForDeposit(new CreateRefundParameters
                    {
                        refundAmount = depositAmount,
                        contractId = new Guid(contractId),
                        langId = langId
                    });
                    initializer.TraceMe("deposit refund end");
                }

                ContractHelper contractHelper = new ContractHelper(initializer.Service);
                initializer.TraceMe("mongodb started");
                try
                {
                    //deactivate old item
                    initializer.TraceMe("mongodb Rental start");
                    initializer.RetryMethod<MongoDBResponse>(() => contractHelper.updateContractItemInMongoDB(contractItemResponse), StaticHelper.retryCount, StaticHelper.retrySleep);
                    initializer.TraceMe("mongodb Rental end");
                }
                catch (Exception ex)
                {
                    //will think a logic
                    initializer.TraceMe("mongodb integration error : " + ex.Message);
                }
                initializer.TraceMe("mongodb end");

                initializer.PluginContext.OutputParameters["ContractCancellationResponse"] = JsonConvert.SerializeObject(validationResponse);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
