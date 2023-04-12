using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Common
{
    public class SMSHelper : IDisposable
    {
        private string baseUrl { get; set; }
        private string[] apiTypes { get; set; }
        private string[] loginInfo { get; set; }
        private IOrganizationService Service { get; set; }
        public SMSHelper(IOrganizationService _service)
        {
            Service = _service;
            XrmHelper xrmHelper = new XrmHelper(this.Service);
            this.baseUrl = xrmHelper.getConfigurationValueByName("sahinSMSBaseUrl");
            this.apiTypes = xrmHelper.getConfigurationValueByName("sahinSMSApiTypes").Split(';');
            this.loginInfo = xrmHelper.getConfigurationValueByName("sahinSMSLoginInfo").Split(';');
        }
        public LoginResponse login()
        {
            try
            {

                // api types 0 means core
                RestSharpHelper restSharpHelper = new RestSharpHelper(this.baseUrl, this.apiTypes[0] + "/loginUser", RestSharp.Method.POST);
                var obj = new LoginParameter
                {
                    username = loginInfo[0],
                    password = loginInfo[1]
                };
                restSharpHelper.AddJsonParameter<LoginParameter>(obj);

                var response = restSharpHelper.Execute<LoginData>();

                return new LoginResponse
                {
                    loginData = response,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        public SendSMSResponse sendMultiSMS(List<MessagePacket> messagePackets)
        {
            var loginResponse = this.login();
            if (loginResponse.ResponseResult.Result)
            {
                if (loginResponse.loginData.status != "success")
                {
                    return new SendSMSResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(loginResponse.loginData.msg)
                    };
                }
                // api types 1 mean sms
                RestSharpHelper helper = new RestSharpHelper(this.baseUrl, this.apiTypes[1] + "/sendsms", RestSharp.Method.POST);

                var obj = new SendSMSParameter
                {
                    apikey = loginResponse.loginData.token,
                    type = "multi",
                    orjin = "SAHINSMS",
                    gonderimzamani = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    dil = "tr",
                    flashsms = "0",
                    mesajpaket = messagePackets
                };

                helper.AddJsonParameter<SendSMSParameter>(obj);

                var response = helper.Execute<SendSMSData>();

                return new SendSMSResponse
                {
                    sendSMSData = response,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }

            return new SendSMSResponse
            {
                ResponseResult = ResponseResult.ReturnError(loginResponse.ResponseResult.ExceptionDetail)
            };
        }
        ~SMSHelper()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Service = null;
                this.baseUrl = null;
                this.apiTypes = null;
                this.loginInfo = null;
            }
        }
    }
}
