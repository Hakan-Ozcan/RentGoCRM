using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.AdditionalProducts.Actions
{
    public class ExecuteGetAdditionalProducts : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            initializer.TraceMe("lets start");
            String GroupCodeParameters;
            initializer.PluginContext.GetContextParameter<string>("GroupCodeParameters", out GroupCodeParameters);

            string CustomerParameters;
            initializer.PluginContext.GetContextParameter<string>("CustomerParameters", out CustomerParameters);

            string SelectedDateAndBranch;
            initializer.PluginContext.GetContextParameter<string>("SelectedDateAndBranch", out SelectedDateAndBranch);

            int TotalDuration;
            initializer.PluginContext.GetContextParameter<int>("TotalDuration", out TotalDuration);
            initializer.TraceMe("GroupCodeParameters" + GroupCodeParameters);
            initializer.TraceMe("CustomerParameters" + CustomerParameters);
            initializer.TraceMe("SelectedDateAndBranch" + SelectedDateAndBranch);
            initializer.TraceMe("TotalDuration" + TotalDuration);

            var groupCodeData = JsonConvert.DeserializeObject<GroupCodeInformationCRMData>(GroupCodeParameters);
            var individualCustomerData = JsonConvert.DeserializeObject<IndividualCustomerDetailData>(CustomerParameters);
            var selectedDateAndBranchData = JsonConvert.DeserializeObject<ReservationDateandBranchParameters>(SelectedDateAndBranch);
            if (individualCustomerData.distributionChannelCode == (int)rnt_DistributionChannelCode.Branch)
            {
                AdditionalProductValidation additionalProductValidation = new AdditionalProductValidation(initializer.Service, initializer.TracingService);
                //todo langId
                var r = additionalProductValidation.checkFindeks(new Guid(groupCodeData.rnt_groupcodeinformationsid), 
                                                                individualCustomerData.individualCustomerId,
                                                                individualCustomerData.pricingType,
                                                                null,
                                                                individualCustomerData.corporateCustomerId,
                                                                null,
                                                                1055);
                if (!r.ResponseResult.Result)
                {
                    initializer.PluginContext.OutputParameters["GroupCodeResponse"] = JsonConvert.SerializeObject(r);
                    return;
                }
            }
            AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(initializer.Service, initializer.TracingService);
            var response = additionalProductsBL.GetAdditionalProducts(groupCodeData, individualCustomerData, selectedDateAndBranchData, TotalDuration, new Guid(groupCodeData.currencyId));
            //initializer.TraceMe("response : " + JsonConvert.SerializeObject(response));
            initializer.PluginContext.OutputParameters["GroupCodeResponse"] = JsonConvert.SerializeObject(response);
        }
    }
}
