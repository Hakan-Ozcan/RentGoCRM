using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp.Extensions.MonoHttp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using HttpUtility = System.Web.HttpUtility;

namespace RntCar.BusinessLibrary.Business
{
    public class SMSBL : BusinessHandler
    {
        public SMSBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public SMSBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public LoginData login()
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var baseUrl = configurationBL.GetConfigurationByName("SMSApiUrl");//get api base url in crm
            var loginInfo = configurationBL.GetConfigurationByName("SMSApiLoginInfo").Split(';'); // get login info in crm

            RestSharpHelper helper = new RestSharpHelper(baseUrl, "core/loginUser", RestSharp.Method.POST);
            var obj = new LoginParameter
            {
                username = loginInfo[0],
                password = loginInfo[1]
            };
            helper.AddJsonParameter<LoginParameter>(obj);

            var response = helper.Execute<LoginData>();

            return response;
        }
        private string UTF8_to_ISO(string value)
        {

            Encoding isoEncoding = Encoding.GetEncoding("ISO-8859-9");
            Encoding utfEncoding = Encoding.UTF8;

            // Converte os bytes 
            byte[] bytesIso = utfEncoding.GetBytes(value);

            //  Obtém os bytes da string UTF 
            byte[] bytesUtf = Encoding.Convert(utfEncoding, isoEncoding, bytesIso);

            // Obtém a string ISO a partir do array de bytes convertido
            string textoISO = utfEncoding.GetString(bytesUtf);

            return textoISO;

        }
        public static string ToUnicodeString(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in str)
            {
                sb.Append("\\u" + ((int)c).ToString("X4"));
            }
            return sb.ToString();
        }
        public static string ConvertStringEncoding(string txt, Encoding srcEncoding, Encoding dstEncoding)
        {
            if (string.IsNullOrEmpty(txt))
            {
                return txt;
            }

            if (srcEncoding == null)
            {
                throw new System.ArgumentNullException(nameof(srcEncoding));
            }

            if (dstEncoding == null)
            {
                throw new System.ArgumentNullException(nameof(dstEncoding));
            }

            var srcBytes = srcEncoding.GetBytes(txt);
            var dstBytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);

            return dstEncoding.GetString(dstBytes);
        }
        public SendSMSData sendSMS(string phoneNumber, string message)
        {
            var responseSliptted = new string[] { };
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var baseUrl = configurationBL.GetConfigurationByName("SMSApiUrl");//get api base url in crm
            var loginInfo = configurationBL.GetConfigurationByName("SMSApiLoginInfo");//get api base url in crm
            var splitted = loginInfo.Split(';');

            string data = null;
            data += "user=" + System.Web.HttpUtility.UrlEncode(splitted[0], Encoding.GetEncoding("iso-8859-9"));
            data += "&password=" + System.Web.HttpUtility.UrlEncode(splitted[1], Encoding.GetEncoding("iso-8859-9"));
            data += "&gsm=" + System.Web.HttpUtility.UrlEncode(phoneNumber, Encoding.GetEncoding("iso-8859-9"));
            data += "&text=" + System.Web.HttpUtility.UrlEncode(message, Encoding.GetEncoding("iso-8859-9"));
            var postRequest = WebRequest.Create(baseUrl + "/sendsms.asp" + "?" + data);
            postRequest.Method = "POST";
            postRequest.ContentType = "application/x-www-form-urlencoded";
            postRequest.ContentLength = 0;
            var response = postRequest.GetResponse() as HttpWebResponse;
            using (Stream dataStream = response.GetResponseStream())
            {
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseSliptted = reader.ReadToEnd().Split('&');
                // Display the content.

            }
            this.Trace("responseSliptted : " + JsonConvert.SerializeObject(responseSliptted));
            return new SendSMSData
            {
                status = responseSliptted[0].Split('=')[1],
                msg = responseSliptted[2].Split('=')[1],
                errtext = responseSliptted[1].Split('=')[1],
            };

        }
    }
}
