using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.PaymentHelper;
using RntCar.PaymentHelper.iyzico;
using RntCar.PaymentHelper.NetTahsilat;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.CrmPlugins.Payment
{
    public class ExecuteMakePayment : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
            ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);

            var createPaymentResponse = new CreatePaymentResponse();
            //this parameter must be false after successfull payment 3rd party integration
            var rollBackSystem = true;
            string createPaymentParameters;
            initializer.PluginContext.GetContextParameter<string>("paymentParameters", out createPaymentParameters);
            initializer.TraceMe("paymentParameters" + createPaymentParameters);
            var param = JsonConvert.DeserializeObject<CreatePaymentParameters>(createPaymentParameters);
            try
            {
                createPaymentResponse = paymentBL.makePayment(param);
                createPaymentResponse.ResponseResult = ResponseResult.ReturnSuccess();
                initializer.PluginContext.OutputParameters["paymentResponse"] = JsonConvert.SerializeObject(createPaymentResponse);
                initializer.PluginContext.OutputParameters["ExecutionResult"] = string.Empty;

            }
            catch (Exception ex)
            {
                if (param.rollbackOperation.HasValue)
                {
                    initializer.TraceMe("rollbackOperation has value :" + param.rollbackOperation.Value);
                    rollBackSystem = param.rollbackOperation.Value;
                }
                else
                {
                    initializer.TraceMe("rollbackOperation doesnt has value");
                }
                initializer.TraceMe("will roll back : " + rollBackSystem);
                if (rollBackSystem)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
                }
                else
                {
                    initializer.PluginContext.OutputParameters["ExecutionResult"] = ex.Message;
                }

            }
        }
    }
}
