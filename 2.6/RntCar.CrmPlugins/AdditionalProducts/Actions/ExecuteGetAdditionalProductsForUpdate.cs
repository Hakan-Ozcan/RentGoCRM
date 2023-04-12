using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.AdditionalProducts.Actions
{
    public class ExecuteGetAdditionalProductsForUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {

            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            String GroupCodeParameters;
            initializer.PluginContext.GetContextParameter<string>("GroupCodeParameters", out GroupCodeParameters);

            String CustomerParameters;
            initializer.PluginContext.GetContextParameter<string>("CustomerParameters", out CustomerParameters);

            String SelectedDateAndBranch;
            initializer.PluginContext.GetContextParameter<string>("SelectedDateAndBranch", out SelectedDateAndBranch);

            int TotalDuration;
            initializer.PluginContext.GetContextParameter<int>("TotalDuration", out TotalDuration);

            String ReservationId;
            initializer.PluginContext.GetContextParameter<string>("ReservationId", out ReservationId);

            int LangId;
            initializer.PluginContext.GetContextParameter<int>("LangId", out LangId);


            initializer.TraceMe("GroupCodeParameters : " + GroupCodeParameters);
            initializer.TraceMe("CustomerParameters : " + CustomerParameters);
            initializer.TraceMe("SelectedDateAndBranch : " + SelectedDateAndBranch);
            initializer.TraceMe("GroupCodeParameters : " + GroupCodeParameters);
            initializer.TraceMe("ReservationId : " + ReservationId);
            initializer.TraceMe("LangId : " + LangId);
            initializer.TraceMe("TotalDuration Parameter : " + TotalDuration);

            var groupCodeData = JsonConvert.DeserializeObject<GroupCodeInformationCRMData>(GroupCodeParameters);
            var individualCustomerData = JsonConvert.DeserializeObject<IndividualCustomerDetailData>(CustomerParameters, new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });
            var selectedDateAndBranchData = JsonConvert.DeserializeObject<ReservationDateandBranchParameters>(SelectedDateAndBranch);
            
            initializer.TraceMe("isAdditionalDriver  " + individualCustomerData.isAdditionalDriver);
            if (individualCustomerData.distributionChannelCode == (int)rnt_DistributionChannelCode.Branch && !individualCustomerData.isAdditionalDriver)
            {
                initializer.TraceMe("Channel is branch");
                AdditionalProductValidation additionalProductValidation = new AdditionalProductValidation(initializer.Service, initializer.TracingService);
                initializer.TraceMe("check findeks start");
                var r = additionalProductValidation.checkFindeks(new Guid(groupCodeData.rnt_groupcodeinformationsid),
                                                                 individualCustomerData.individualCustomerId,
                                                                 individualCustomerData.pricingType,
                                                                 new Guid(ReservationId),
                                                                 individualCustomerData.corporateCustomerId,
                                                                 groupCodeData.changeType,
                                                                 1055);
                if (!r.ResponseResult.Result)
                {
                    initializer.PluginContext.OutputParameters["AdditionalProductsResponse"] = JsonConvert.SerializeObject(r);
                    return;
                }
            }
            initializer.TraceMe("getIndividualCustomerByIdWithGivenColumns start");
            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(initializer.Service);
            var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(individualCustomerData.individualCustomerId, new string[] { "birthdate" });
            individualCustomerData.birthDate = customer.GetAttributeValue<DateTime>("birthdate");
            initializer.TraceMe("getIndividualCustomerByIdWithGivenColumns end");
            AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(initializer.Service, initializer.TracingService);
            initializer.TraceMe("satart get additionalProduct");
            var result = additionalProductsBL.GetAdditionalProductForUpdate(groupCodeData, individualCustomerData, selectedDateAndBranchData, ReservationId, LangId);
            initializer.TraceMe("end get additionalProduct");
            initializer.PluginContext.OutputParameters["AdditionalProductsResponse"] = JsonConvert.SerializeObject(result);
        }
    }
}
