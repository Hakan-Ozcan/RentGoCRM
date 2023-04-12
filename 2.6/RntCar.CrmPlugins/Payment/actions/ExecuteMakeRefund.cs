using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.PaymentHelper.iyzico;
using RntCar.PaymentHelper.NetTahsilat;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.Payment.actions
{
    public class ExecuteMakeRefund 
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            PaymentBL paymentBL = new PaymentBL(initializer.Service, initializer.TracingService);
            try
            {
                string createRefundParameters;
                initializer.PluginContext.GetContextParameter<string>("createRefundParameters", out createRefundParameters);
                initializer.TraceMe("createRefundParameters" + createRefundParameters);
                var param = JsonConvert.DeserializeObject<CreateRefundParameters>(createRefundParameters);

                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                //var provider = paymentBL.getProviderName()
                //initializer.TraceMe("paymentprovider_namespace : " + provider);
                
                //if (provider == GlobalEnums.PaymentProvider.nettahsilat.ToString())
                //{   
                //    var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");
                //    var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);
                //    //setting auth info
                //    param.userName = configs.nettahsilat_username;
                //    param.password = configs.nettahsilat_password;
                //}
                
                paymentBL.createRefund(param);
            }
            catch (Exception ex)
            {
                initializer.TraceMe("refund error : " + ex.Message);
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
