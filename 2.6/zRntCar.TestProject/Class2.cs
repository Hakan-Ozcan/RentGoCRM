using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;

namespace YourNamespace
{
    public class SMSWebService
    {
        private string baseUrl = "https://restapi.ttmesaj.com/ttmesajToken";

        public string GetToken()
        {
            var client = new RestClient(baseUrl + "/ttmesajToken");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "username=ttapiuser1&password=ttapiuser1123&grant_type=password", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);

            dynamic tokenResponse = JsonConvert.DeserializeObject(response.Content);
            string token = tokenResponse.access_token;
            return token;
        }

        public bool SendSMS(string username, string password, string numbers, string message, string origin)
        {
            string token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Token alınamadı.");
                return false;
            }

            var client = new RestClient(baseUrl + "/api/SendSms/SendSingle");
            var request = new RestRequest(Method.POST);
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", "Bearer " + token);

            var smsData = new
            {
                username = username,
                password = password,
                numbers = numbers,
                message = message,
                origin = origin,
                sd = "",
                ed = "",
                isNotification = (object)null,
                recipentType = "",
                brandCode = ""
            };

            request.AddJsonBody(smsData);

            IRestResponse response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic smsResponse = JsonConvert.DeserializeObject(response.Content);
                string status = smsResponse.status;
                string msg = smsResponse.msg;
                string errtext = smsResponse.errtext;

                Console.WriteLine("SMS gönderimi başarılı. Durum: " + status + ", Mesaj: " + msg + ", Hata Metni: " + errtext);
                return true;
            }
            else
            {
                Console.WriteLine("SMS gönderimi başarısız. Hata Kodu: " + response.StatusCode + ", Hata Mesajı: " + response.ErrorMessage);
                return false;
            }
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            SMSWebService smsService = new SMSWebService();
            string username = "XXX"; // Yeni sms firması kullanıcı adınız
            string password = "XXX"; // Yeni sms firması şifreniz
            string numbers = "XXX"; // Gönderilecek telefon numarası
            string message = "test"; // Gönderilecek mesaj
            string origin = "XXX"; // Gönderen bilgisi

            bool success = smsService.SendSMS(username, password, numbers, message, origin);

            if (success)
            {
                Console.WriteLine("SMS gönderimi başarılı!");
            }
            else
            {
                Console.WriteLine("SMS gönderimi başarısız!");
            }
        }
    }
}