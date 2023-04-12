using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.CouponCodeDefinition.Actions
{
    public class ExecuteGetDefinitionDetailsByCouponCode : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                string couponCode;
                initializer.PluginContext.GetContextParameter<string>("CouponCode", out couponCode);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("langId", out langId);

                initializer.TraceMe("CouponCode: " + couponCode);

                CouponCodeDefinitionBL couponCodeDefinitionBL = new CouponCodeDefinitionBL(initializer.Service, initializer.TracingService);
                var response = couponCodeDefinitionBL.getCouponCodeDefinitionByCouponCode(couponCode);

                initializer.TraceMe("Coupon code response : " + JsonConvert.SerializeObject(response));

                initializer.PluginContext.OutputParameters["DefinitionResponse"] = JsonConvert.SerializeObject(response);

            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
