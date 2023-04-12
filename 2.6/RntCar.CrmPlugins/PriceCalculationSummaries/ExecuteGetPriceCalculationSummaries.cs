using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.PriceCalculationSummaries
{
    public class ExecuteGetPriceCalculationSummaries : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            string groupCodeInformationId;
            initializer.PluginContext.GetContextParameter<string>("groupCodeInformationId", out groupCodeInformationId);

            string campaignId;
            initializer.PluginContext.GetContextParameter<string>("campaignId", out campaignId);

            string trackingNumber;
            initializer.PluginContext.GetContextParameter<string>("trackingNumber", out trackingNumber);

            string pickupBranchId;
            initializer.PluginContext.GetContextParameter<string>("pickupBranchId", out pickupBranchId);

            string documentId;
            initializer.PluginContext.GetContextParameter<string>("documentId", out documentId);
            try
            {
                initializer.TraceMe("groupCodeInformationId : " + groupCodeInformationId);
                initializer.TraceMe("campaignId : " + campaignId);
                initializer.TraceMe("trackingNumber : " + trackingNumber);
                initializer.TraceMe("pickupBranchId : " + pickupBranchId);
                initializer.TraceMe("documentId : " + documentId);

                AvailibilityBL availibilityBL = new AvailibilityBL(initializer.Service, initializer.TracingService);
                var response = availibilityBL.getPriceCalculationPriceSummaries(trackingNumber,
                                                                                groupCodeInformationId,
                                                                                campaignId,
                                                                                pickupBranchId,
                                                                                documentId);
                initializer.PluginContext.OutputParameters["getPriceCalculationSummariesResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
