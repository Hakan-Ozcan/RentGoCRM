using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.PriceCalculation.Action
{
    public class ExecuteCreatePriceCalculation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                string priceListParameters;
                initializer.PluginContext.GetContextParameter<string>("PriceListParameters", out priceListParameters);

                string groupCodeParameters;
                initializer.PluginContext.GetContextParameter<string>("GroupCodeParameters", out groupCodeParameters);

                string groupCodeListPriceParameters;
                initializer.PluginContext.GetContextParameter<string>("GroupCodeLisPriceParameters", out groupCodeListPriceParameters);

                string availabilityPriceListParameters;
                initializer.PluginContext.GetContextParameter<string>("AvailabilityPriceListParameters", out availabilityPriceListParameters);

                PriceCalculationBL priceCalculationBL = new PriceCalculationBL(initializer.Service, initializer.TracingService);
                var response = priceCalculationBL.executeCreatePriceCalculation(priceListParameters, groupCodeParameters, groupCodeListPriceParameters, availabilityPriceListParameters);

                initializer.PluginContext.OutputParameters["ResponseResult"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
            
        }
    }
}
