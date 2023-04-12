using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Contact.Actions
{
    public class ExecuteGetIndividualCustomerDetail : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            string IndividualCustomerParameters;
            initializer.PluginContext.GetContextParameter<string>("IndividualCustomerParameters", out IndividualCustomerParameters);

            try
            {
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(initializer.Service, initializer.TracingService);

                var parameters = JsonConvert.DeserializeObject<IndividualCustomerDetailInformationParameters>(IndividualCustomerParameters);

                var result = individualCustomerBL.getIndividualCustomerDetail(parameters);
                initializer.TraceMe(JsonConvert.SerializeObject(result));
                   
                initializer.PluginContext.OutputParameters["IndividualCustomerResponse"] = JsonConvert.SerializeObject(result);
            }
            catch (Exception ex)
            {
                //todo will replace more user friendly message
                throw new Exception(ex.Message);
            }
        }
    }
}
