using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.AdditionalProducts.Actions
{
    public class ExecuteGetReservationSelectedAdditionalProducts : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string ReservationId;
                initializer.PluginContext.GetContextParameter<string>("ReservationId", out ReservationId);

                int TotalDuration;
                initializer.PluginContext.GetContextParameter<int>("TotalDuration", out TotalDuration);

                Decimal ReservationPaidAmount;
                initializer.PluginContext.GetContextParameter<Decimal>("ReservationPaidAmount", out ReservationPaidAmount);

                int LangId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out LangId);

                initializer.TraceMe("ReservationId : " + ReservationId);
                initializer.TraceMe("TotalDuration : " + TotalDuration);
                initializer.TraceMe("ReservationPaidAmount : " + ReservationPaidAmount);
                initializer.TraceMe("LangId : " + LangId);

                AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(initializer.Service, initializer.TracingService);
                var response = additionalProductsBL.getReservationSelectedAdditionalProducts(ReservationId, ReservationPaidAmount);

                initializer.PluginContext.OutputParameters["AdditionalProductResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
