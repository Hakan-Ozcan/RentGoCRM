using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.KilometerLimit
{
    public class ExecuteGetKilometerLimit : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string kilometerLimitParameter;
                initializer.PluginContext.GetContextParameter<string>("KilometerLimitParameter", out kilometerLimitParameter);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

                var parameters = JsonConvert.DeserializeObject<KilometerLimitParameter>(kilometerLimitParameter);
                ContractHelper contractHelper = new ContractHelper(initializer.Service, initializer.TracingService);
                var totalDuration  = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(parameters.pickupDate, parameters.dropoffDate);
                //var totalDuration = Convert.ToInt32(CommonHelper.calculateTotalDurationInDays(parameters.pickupDate.converttoTimeStamp(), parameters.dropoffDate.converttoTimeStamp()));
                initializer.TraceMe("totalDuration : " + totalDuration);
                KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(initializer.Service, initializer.TracingService);
                List<KilometerLimitData> kilometerLimitData = new List<KilometerLimitData>();
                foreach (var item in parameters.gorupCodeList)
                {
                    var kilometer = kilometerLimitBL.getKilometerLimitForGivenDurationandGroupCode(totalDuration, item);
                    kilometerLimitData.Add(new KilometerLimitData
                    {
                        kilometerLimit = kilometer,
                        groupCodeInformationId = item
                    });
                }
               
                if (parameters.reservationId.HasValue)
                {
                    initializer.TraceMe("reservation update or contract create");
                    ReservationRepository reservationRepository = new ReservationRepository(initializer.Service);
                    var reservation = reservationRepository.getReservationById(parameters.reservationId.Value, new string[] { "rnt_pickupdatetime",
                                                                                                                                        "rnt_dropoffdatetime",
                                                                                                                                        "rnt_kilometerlimit",
                                                                                                                                        "rnt_groupcodeid" });
                    initializer.TraceMe("pickupdatetime - pickupdatetime parameter : " + reservation.GetAttributeValue<DateTime>("rnt_pickupdatetime") + "-" + parameters.pickupDate);
                    initializer.TraceMe("dropoffdatetime - dropoffdatetime parameter : " + reservation.GetAttributeValue<DateTime>("rnt_dropoffdatetime") + "-" + parameters.dropoffDate);
                    var isDateChanged = CommonHelper.isDateChanged(reservation.GetAttributeValue<DateTime>("rnt_pickupdatetime"), 
                                                                   reservation.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                                                                   parameters.pickupDate.AddMinutes(StaticHelper.offset), parameters.dropoffDate.AddMinutes(StaticHelper.offset));
                    initializer.TraceMe("is date changed : " + isDateChanged);
                    if (!isDateChanged)
                    {
                        var kilometerLimit = kilometerLimitData.Where(item => item.groupCodeInformationId == reservation.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id).FirstOrDefault();
                        kilometerLimit.kilometerLimit = reservation.GetAttributeValue<int>("rnt_kilometerlimit");
                    }
                }

                if (parameters.contractId.HasValue)
                {
                    initializer.TraceMe("contract update");
                    ContractRepository contractRepository = new ContractRepository(initializer.Service);
                    var contract = contractRepository.getContractById(parameters.contractId.Value, new string[] { "rnt_pickupdatetime",
                                                                                                                  "rnt_dropoffdatetime",
                                                                                                                  "rnt_kilometerlimit",
                                                                                                                  "rnt_groupcodeid" });
                    initializer.TraceMe("pickupdatetime - pickupdatetime parameter : " + contract.GetAttributeValue<DateTime>("rnt_pickupdatetime") + "-" + parameters.pickupDate);
                    initializer.TraceMe("dropoffdatetime - dropoffdatetime parameter : " + contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime") + "-" + parameters.dropoffDate);
                    var isDateChanged = CommonHelper.isDateChanged(contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                                   contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                                                                   parameters.pickupDate.AddMinutes(StaticHelper.offset), parameters.dropoffDate.AddMinutes(StaticHelper.offset));
                    initializer.TraceMe("is date changed : " + isDateChanged);
                    if (!isDateChanged)
                    {
                        var kilometerLimit = kilometerLimitData.Where(item => item.groupCodeInformationId == contract.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id).FirstOrDefault();
                        kilometerLimit.kilometerLimit = contract.GetAttributeValue<int>("rnt_kilometerlimit");
                    }
                }

                var response = new GetKilometerLimitResponse
                {
                    kilometerLimits = kilometerLimitData,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };

                initializer.PluginContext.OutputParameters["KilometerLimitResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
