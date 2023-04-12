using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Campaign
{
    // Tolga AYKURT - 27.02.2019
    public class ExecuteCalculateCampaignPrices : IPlugin
    {
        // Tolga AYKURT - 28.02.2019
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            string campaignParameters;
            initializer.PluginContext.GetContextParameter<string>("CampaignParameters", out campaignParameters);

            int langId;
            initializer.PluginContext.GetContextParameter<int>("LangId", out langId);            
           
            var campParams = JsonConvert.DeserializeObject<CampaignParameters>(campaignParameters);

            var campaignBL = new CampaignBL(initializer.Service, initializer.TracingService);
            var result = campaignBL.GetCalculatedCampaignPrices(campParams);
            initializer.PluginContext.OutputParameters["CalculatedPricesResponse"] = JsonConvert.SerializeObject(result);
        }
    }
}
