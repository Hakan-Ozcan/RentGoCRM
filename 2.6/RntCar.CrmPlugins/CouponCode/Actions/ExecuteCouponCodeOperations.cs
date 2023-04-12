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

namespace RntCar.CrmPlugins.CouponCode.Actions
{
    public class ExecuteCouponCodeOperations : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                string couponCodeOperationsParameter;
                initializer.PluginContext.GetContextParameter<string>("CouponCodeOperationsParameter", out couponCodeOperationsParameter);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("langId", out langId);

                initializer.TraceMe("Coupon code parameter : " + couponCodeOperationsParameter);

                var parameter = JsonConvert.DeserializeObject<CouponCodeOperationsParameter>(couponCodeOperationsParameter);

                CouponCodeBL couponCodeBL = new CouponCodeBL(initializer.Service, initializer.TracingService);
                var response = couponCodeBL.executeCouponCodeOperations(parameter, false, langId);

                initializer.TraceMe("Coupon code process response : " + JsonConvert.SerializeObject(response));

                initializer.PluginContext.OutputParameters["CouponCodeOperationsResponse"] = JsonConvert.SerializeObject(response);

            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
