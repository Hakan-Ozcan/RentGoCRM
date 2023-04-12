using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Contract.Action
{
    public class ExecuteCheckBeforeContractCreation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                string ValidationParameters;
                initializer.PluginContext.GetContextParameter<string>("ValidationParameters", out ValidationParameters);

                initializer.TraceMe("Validation parameteres : " + ValidationParameters);

                var param = JsonConvert.DeserializeObject<CheckBeforeContractCreationParameters>(ValidationParameters);
                initializer.TraceMe("after Validation parameteres : " + JsonConvert.SerializeObject(param));

                ReservationRepository reservationRepository = new ReservationRepository(initializer.Service);
                var reservation = reservationRepository.getReservationById(param.reservationId, new string[] { "rnt_findeks",
                                                                                                               "rnt_paymentchoicecode",
                                                                                                               "createdon",
                                                                                                               "rnt_groupcodeid",
                                                                                                               "rnt_pickupdatetime",
                                                                                                               "statuscode",
                                                                                                               "rnt_corporateid",
                                                                                                               "statecode",
                                                                                                               "rnt_pickupbranchid",
                                                                                                               "rnt_customerid",
                                                                                                               "rnt_pricingtype",
                                                                                                               "rnt_corporateid",
                                                                                                               "rnt_reservationtypecode",
                                                                                                               "rnt_paymentmethodcode",
                                                                                                               "rnt_contractnumber"});



                var userId = initializer.PluginContext.UserId;
                initializer.TraceMe("userId : " + userId);

                //IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(initializer.Service);
                //var contact = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(reservation.GetAttributeValue<EntityReference>("rnt_customerid").Id,
                //                                                                                     new string[] { "rnt_findekspoint" });
                if (param.isQuickContract)
                {
                    AdditionalProductValidation additionalProductValidation = new AdditionalProductValidation(initializer.Service, initializer.TracingService);
                    var r = additionalProductValidation.checkFindeks(reservation.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id,
                                                                     reservation.GetAttributeValue<EntityReference>("rnt_customerid").Id,
                                                                     reservation.GetAttributeValue<string>("rnt_pricingtype"),
                                                                     reservation.Id,
                                                                     reservation.Contains("rnt_corporateid") ?  reservation.GetAttributeValue<EntityReference>("rnt_corporateid").Id.ToString() : null,
                                                                     null,
                                                                     param.langId);

                    if (!r.ResponseResult.Result)
                    {
                        throw new Exception(r.ResponseResult.ExceptionDetail);
                    }
                }

                ContractBL contractBL = new ContractBL(initializer.Service, initializer.TracingService);

                var response = contractBL.checkBeforeContractCreationWithParameters(reservation, param.contactId, userId, param.isQuickContract, param.langId);
                initializer.TraceMe("response : " + JsonConvert.SerializeObject(response));
                initializer.PluginContext.OutputParameters["ResponseResult"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
