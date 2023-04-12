using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.CrmPlugins.Contract.Action
{
    public class ExecuteCalculatePricesforUpdateContract : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string calculatePricesforUpdateContractParameters;
                initializer.PluginContext.GetContextParameter<string>("CalculatePricesforUpdateContractParameters", out calculatePricesforUpdateContractParameters);               

                var param = JsonConvert.DeserializeObject<CalculatePricesForUpdateContractParameters>(calculatePricesforUpdateContractParameters);

                initializer.TraceMe("retrieve contract start : " + JsonConvert.SerializeObject(param));

                ContractRepository contractRepository = new ContractRepository(initializer.Service);
                var contractInformation = contractRepository.getContractById(param.contractId,
                                                                             new string[] {  "rnt_contracttypecode",
                                                                                              "rnt_corporateid",
                                                                                              "rnt_pickupbranchid",
                                                                                              "rnt_pickupdatetime",
                                                                                              "rnt_dropoffdatetime",
                                                                                              "rnt_ismonthly",
                                                                                              "rnt_groupcodeid",
                                                                                              "rnt_offset",
                                                                                              "rnt_customerid",
                                                                                              "rnt_pricinggroupcodeid"});

                ContractUpdateValidation contractUpdateValidation = new ContractUpdateValidation(initializer.Service, initializer.TracingService);
                var updateRes = contractUpdateValidation.checkMonthlyValidations(contractInformation, param.dropoffDateTime.AddMinutes(StaticHelper.offset), param.passValidation,false);

                if (!updateRes.ResponseResult.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(updateRes.ResponseResult.ExceptionDetail);
                }

                AvailabilityValidation availabilityValidation = new AvailabilityValidation(initializer.Service);
                var r = availabilityValidation.checkBrokerReservation_Contract(new AvailabilityParameters
                {
                    contractId = contractInformation.Id.ToString(),
                    pickupDateTime = contractInformation.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                    dropoffDateTime = param.dropoffDateTime.AddMinutes(StaticHelper.offset)
                }, 1055);

                if (!r.ResponseResult.Result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(r.ResponseResult.ExceptionDetail);
                }
                ContractItemRepository contractItemRepository = new ContractItemRepository(initializer.Service);
                var equipment = contractItemRepository.getRentalandWaitingforDeliveryEquipmentContractItemsByContractId(param.contractId, new string[] {"rnt_dropoffdatetime",
                                                                                                                                                        "rnt_pickupdatetime" }).FirstOrDefault();

                //araç değişikliğinde saat uyusmazlıgı fix
                //araç değişikliğinde teslim ederken arada olusan dakika uyusmazlıgını gidermek için
                //ayrıca sayfa ilk yüklendiğinde araç değişikliği varsa fiyat farkı cıkarmaması için
                initializer.TraceMe("param.dropoffDateTime.AddMinutes(StaticHelper.offset) " + param.dropoffDateTime.AddMinutes(StaticHelper.offset));
                initializer.TraceMe("contractInformation.GetAttributeValue<DateTime>(rnt_dropoffdatetime) " + contractInformation.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));
                if (param.dropoffDateTime.AddMinutes(StaticHelper.offset) == contractInformation.GetAttributeValue<DateTime>("rnt_dropoffdatetime"))
                {
                    param.dropoffDateTime = equipment.GetAttributeValue<DateTime>("rnt_dropoffdatetime").AddMinutes(-StaticHelper.offset);
                }
               
                initializer.TraceMe("retrieve contract end " + param.dropoffDateTime);


                var utcpickupTime = equipment.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                //var utcDropoff = equipment.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                //these fields are timezone independent so we need to make them timezone
                utcpickupTime = utcpickupTime.AddMinutes(-StaticHelper.offset);
          
                initializer.TraceMe("utcpickupTime" + utcpickupTime);
                AvailabilityHelper availabilityHelper = new AvailabilityHelper(initializer.Service);
                var response = availabilityHelper.calculateAvailability_Contract(contractInformation,
                                                                                 param.dropoffBranchId,
                                                                                 param.dropoffDateTime,
                                                                                 utcpickupTime,
                                                                                 param.langId,
                                                                                 param.isMonthly);
               //initializer.TraceMe("CalculatePricesforUpdateContractParameters" + calculatePricesforUpdateContractParameters);
                initializer.PluginContext.OutputParameters["CalculatePricesforUpdateContractResponse"] = JsonConvert.SerializeObject(response);

            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
