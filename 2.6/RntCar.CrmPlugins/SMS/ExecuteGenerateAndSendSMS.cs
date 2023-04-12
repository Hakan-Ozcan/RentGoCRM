using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.SMS;
using RntCar.SDK.Common;
using System;

namespace RntCar.CrmPlugins.SMS
{
    public class ExecuteGenerateAndSendSMS : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            //GenerateAndSendSMSResponse response = new GenerateAndSendSMSResponse();
            try
            {
                int smsContentCode;
                initializer.PluginContext.GetContextParameter<int>("SMSContentCode", out smsContentCode);

                string pnrNumber;
                initializer.PluginContext.GetContextParameter<string>("PnrNumber", out pnrNumber);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

                string mobilePhone;
                initializer.PluginContext.GetContextParameter<string>("MobilePhone", out mobilePhone);

                string firstName;
                initializer.PluginContext.GetContextParameter<string>("FirstName", out firstName);

                string lastName;
                initializer.PluginContext.GetContextParameter<string>("LastName", out lastName);

                string verificationCode;
                initializer.PluginContext.GetContextParameter<string>("VerificationCode", out verificationCode);

                string amount;
                initializer.PluginContext.GetContextParameter<string>("amount", out amount);

                string email;
                initializer.PluginContext.GetContextParameter<string>("email", out email);

                string additionalProductName;
                initializer.PluginContext.GetContextParameter<string>("AdditionalProductName", out additionalProductName);

                initializer.TraceMe($@"MobilePhone: {mobilePhone} ,LangId: {langId} , PnrNumber: {pnrNumber},
                                   SMSContentCode: {smsContentCode}, VerificationCode: {verificationCode} , FirstName: {firstName}, LastName: {lastName}, AdditionalProduct: {additionalProductName}");

                initializer.TraceMe("amount " + amount);
                initializer.TraceMe("email " + email);

                //retrieve sms message by content code and langid
                SMSContentBL smsContentBl = new SMSContentBL(initializer, initializer.Service, initializer.TracingService);
                initializer.TraceMe("lets start");
                var message = smsContentBl.getSMSContentByCodeandLangId(smsContentCode, langId, mobilePhone, pnrNumber, verificationCode, firstName, lastName, amount, additionalProductName);
                if (!string.IsNullOrEmpty(message))
                {
                    initializer.TraceMe($@"{mobilePhone}");
                    initializer.TraceMe($@"{message}");
                    initializer.TraceMe($@"sms {message}");

                    initializer.TraceMe("sms");

                    SMSBL smsBl = new SMSBL(initializer.Service);
                    smsBl.sendSMS(mobilePhone, message);

                    initializer.TraceMe("SMS Sended.");

                    var response = new GenerateAndSendSMSResponse
                    {
                        ResponseResult = ResponseResult.ReturnSuccess()
                    };
                    initializer.TraceMe("response : " + JsonConvert.SerializeObject(response));

                    initializer.PluginContext.OutputParameters["GenerateAndSendSMSResponse"] = JsonConvert.SerializeObject(response);
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
                    initializer.PluginContext.OutputParameters["GenerateAndSendSMSResponse"] = JsonConvert.SerializeObject(response);
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
