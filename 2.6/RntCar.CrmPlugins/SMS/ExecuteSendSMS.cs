using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.SMS;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.SMS
{
    public class ExecuteSendSMS : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string message;
                initializer.PluginContext.GetContextParameter<string>("Message", out message);

                string mobilePhone;
                initializer.PluginContext.GetContextParameter<string>("MobilePhone", out mobilePhone);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

                initializer.TraceMe("lets start");
                initializer.TraceMe(message);
                initializer.TraceMe(mobilePhone);
                initializer.TraceMe(Convert.ToString(langId));


                if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(mobilePhone))
                {
                    initializer.TraceMe("sms");
                    SMSBL smsBl = new SMSBL(initializer.Service);
                    smsBl.sendSMS(mobilePhone, message);

                    initializer.TraceMe($"Mobile Phone:{mobilePhone} Message:{message}");
                    initializer.TraceMe("SMS Sended.");

                    var response = new GenerateAndSendSMSResponse
                    {
                        ResponseResult = ResponseResult.ReturnSuccess()
                    };
                    initializer.TraceMe("response : " + JsonConvert.SerializeObject(response));

                    initializer.PluginContext.OutputParameters["SendSMSResponse"] = JsonConvert.SerializeObject(response);
                }
                else
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var errorMessge = xrmHelper.GetXmlTagContentByGivenLangId("NullCustomerOrMessageForSMS", langId);
                    var response = new GenerateAndSendSMSResponse
                    {
                        ResponseResult = ClassLibrary.ResponseResult.ReturnError(errorMessge)
                    };
                    initializer.TraceMe("error response object " + JsonConvert.SerializeObject(response));
                    initializer.PluginContext.OutputParameters["SendSMSResponse"] = JsonConvert.SerializeObject(response);
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
