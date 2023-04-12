using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Payment.actions
{
    public class Execute3dPaymentReturn : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            string Payment3dReturnParameters;
            initializer.PluginContext.GetContextParameter<string>("Payment3dReturnParameters", out Payment3dReturnParameters);
            initializer.TracingService.Trace("Payment 3d Return Parameters : " + Payment3dReturnParameters);

            Payment3dReturnResponse response = new Payment3dReturnResponse();
            try
            {
                PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
                var result= paymentBL.Payment3dReturn(JsonConvert.DeserializeObject<Payment3dReturnParameters>(Payment3dReturnParameters));
                response.ResponseResult = new ResponseResult() { Result = result };
            }
            catch (Exception ex)
            {
                initializer.TraceMe($"Payment 3d Return exception : {ex.Message}");
                response.ResponseResult = new ResponseResult() { Result = false, ExceptionDetail = ex.Message };
            }

            initializer.PluginContext.OutputParameters["ReservationPaymentReturnResponse"] = JsonConvert.SerializeObject(response);
            initializer.TraceMe("Payment 3d Return finished");
        }
    }
}
