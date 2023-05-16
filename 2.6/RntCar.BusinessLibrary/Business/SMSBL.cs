using Microsoft.Xrm.Sdk;
using MongoDB.Bson.IO;
using Newtonsoft.Json;
using RestSharp.Extensions.MonoHttp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using static Microsoft.IdentityModel.Protocols.WSFederation.WSFederationConstants;
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
        //Bu yöntem, bir UTF-8 kodlamasına sahip bir metni ISO-8859-9 kodlamasına dönüştürmek için kullanılır. ISO-8859-9, Türkçe karakterlerin bulunduğu Latin alfabesini destekleyen bir karakter kodlamasıdır.
        {

            Encoding isoEncoding = Encoding.GetEncoding("ISO-8859-9");//GetEncoding metodu kullanılarak "ISO-8859-9" karakter kodlamasına sahip bir Encoding nesnesi alınır.Bu karakter kodlaması, Türkçe karakterleri içeren bir metni temsil etmek için kullanılır.

            // Converte os bytes 
            byte[] bytesIso = utfEncoding.GetBytes(value);//Bu satırda, value metnini ISO-8859-9 karakter kodlamasına sahip bir byte dizisine dönüştürmek için utfEncoding.GetBytes yöntemi kullanılır.    utfEncoding.GetBytes metodu, metni verilen karakter kodlamasına göre baytlara dönüştürür.


            //  Obtém os bytes da string UTF 
            byte[] bytesUtf = Encoding.Convert(utfEncoding, isoEncoding, bytesIso);//Bu satırda, bytesIso dizisini UTF-8 karakter kodlamasından ISO-8859-9 karakter kodlamasına dönüştürmek için Encoding.Convert metodu kullanılır.Encoding.Convert metodu, bir karakter kodlamasından diğerine baytları dönüştürmek için kullanılır.


            // Obtém a string ISO a partir do array de bytes convertido
            string textoISO = utfEncoding.GetString(bytesUtf);//Bu satırda, bytesUtf dizisini ISO-8859-9 karakter kodlamasına göre metne dönüştürmek için utfEncoding.GetString yöntemi kullanılır. utfEncoding.GetString yöntemi, baytları verilen karakter kodlamasına göre metne dönüştürür.


            return textoISO;

        }
        public static string ToUnicodeString(string str)
        //Bu yöntem, bir metni Unicode kaçış dizisine dönüştürmek için kullanılır. Unicode kaçış dizileri, Unicode karakterlerin kod noktalarını \uXXXX formatında temsil eder, burada XXXX Unicode karakterin dört haneli onaltılık değeridir.
        //Özellikle, metni bir yerden başka bir yere taşırken veya depolarken, Unicode karakterlerin kaybolmaması için bu tür bir dönüşüm yapılabilir.
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in str)
            {
                sb.Append("\\u" + ((int)c).ToString("X4"));//Bu satır, sb'ye Unicode kaçış dizisini ekler.\\u bir kaçış dizisidir ve Unicode karakterlerin başlangıcını temsil eder. ((int)c).ToString("X4") ifadesi, karakterin Unicode değerini onaltılık bir dizeye dönüştürür. "X4" biçimlendirme belirtecine sahip olduğu için dört haneli bir onaltılık değer üretilir.   "\\u" + ((int)c).ToString("X4") ifadesi, Unicode kaçış dizisi oluşturulur ve sb'ye eklenir.
            }
            return sb.ToString();
        }
        public static string ConvertStringEncoding(string txt, Encoding srcEncoding, Encoding dstEncoding)
        {
            if (string.IsNullOrEmpty(txt))//Bu satır, txt parametresinin boş veya null olup olmadığını kontrol eder Eğer metin boş veya null ise, dönüştürme yapılacak bir metin olmadığı için aynı metin geri döndürülür.
            {
                return txt;
            }

            if (srcEncoding == null)//Bu satır, srcEncoding parametresinin null olup olmadığını kontrol eder.Eğer null ise, kaynak kodlama parametresi geçerli bir değer içermiyor demektir. Bu durumda ArgumentNullException fırlatılır ve istisna oluşur.

            {
                throw new System.ArgumentNullException(nameof(srcEncoding));
            }

            if (dstEncoding == null)//Bu satır, dstEncoding parametresinin null olup olmadığını kontrol eder.Eğer null ise, hedef kodlama parametresi geçerli bir değer içermiyor demektir.Bu durumda ArgumentNullException fırlatılır ve istisna oluşur.
 
            {
                throw new System.ArgumentNullException(nameof(dstEncoding));
            }

            var srcBytes = srcEncoding.GetBytes(txt);//Bu satır, srcEncoding tarafından belirtilen kodlama kullanılarak txt metnini bir byte dizisine dönüştürür.Dönüştürülen byte dizisi, srcBytes değişkenine atanır.

            var dstBytes = Encoding.Convert(srcEncoding, dstEncoding, srcBytes);//Bu satır, srcBytes byte dizisini srcEncoding ile belirtilen kodlamadan dstEncoding ile belirtilen kodlamaya dönüştürür.Dönüştürülen byte dizisi, dstBytes değişkenine atanır.


            return dstEncoding.GetString(dstBytes);//Bu satır, dstBytes byte dizisini dstEncoding ile belirtilen kodlamadan metne dönüştürür. Dönüştürülen metin, metot çağrıldığı yere geri döndürülür.

        }
        public SendSMSData sendSMS(string phoneNumber, string message)
        {
            var responseSliptted = new string[] { };
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var baseUrl = configurationBL.GetConfigurationByName("SMSApiUrl");//get api base url in crm
            var loginInfo = configurationBL.GetConfigurationByName("SMSApiLoginInfo");//get api base url in crm
            var splitted = loginInfo.Split(';');

            string data = null;
            data += "user=" + System.Web.HttpUtility.UrlEncode(splitted[0], Encoding.GetEncoding("iso-8859-9"));//Bu satırda, data metnine kullanıcı adı parametresini (splitted[0]) ve ISO-8859-9 karakter kodlamasına göre kodlanmış hali eklenir. UrlEncode yöntemi, metindeki özel karakterleri uygun şekilde kodlar.
            data += "&password=" + System.Web.HttpUtility.UrlEncode(splitted[1], Encoding.GetEncoding("iso-8859-9"));//Bu satırda, data metnine şifre parametresini (splitted[1]) ve ISO-8859-9 karakter kodlamasına göre kodlanmış hali eklenir.
            data += "&gsm=" + System.Web.HttpUtility.UrlEncode(phoneNumber, Encoding.GetEncoding("iso-8859-9"));
            data += "&text=" + System.Web.HttpUtility.UrlEncode(message, Encoding.GetEncoding("iso-8859-9"));
            var postRequest = WebRequest.Create(baseUrl + "/sendsms.asp" + "?" + data);//Bu satırda, baseUrl, "/sendsms.asp" ve data değişkenlerini birleştirerek bir istek (postRequest) oluşturulur. Bu, belirtilen URL'ye ve verilere bir POST isteği göndermek için kullanılır.
            postRequest.Method = "POST";//Bu satırda, postRequest nesnesinin HTTP isteği yöntemini "POST" olarak ayarlar. Bu, isteğin POST isteği olarak gönderileceğini belirtir.
            postRequest.ContentType = "application/x-www-form-urlencoded";// Bu satırda, postRequest nesnesinin içerik türünü "application/x-www-form-urlencoded" olarak ayarlar. Bu, isteğin form verilerini kodlamak için kullanılan standart bir türdür.
            postRequest.ContentLength = 0;//Bu satırda, postRequest nesnesinin içerik uzunluğunu 0 olarak ayarlar. Bu, isteğin gövdesinin boş olduğunu belirtir.
            var response = postRequest.GetResponse() as HttpWebResponse;// Bu satırda, postRequest nesnesi üzerinden bir istek gönderilir ve yanıt (response) alınır. GetResponse() yöntemi, isteğin yanıtını döndürür ve HttpWebResponse türünden bir nesne olarak alır.
            using (Stream dataStream = response.GetResponseStream())
            {// Bu satırda, response nesnesinden bir veri akışı (dataStream) elde edilir. 
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);//Bu satırda, dataStream veri akışını kullanarak bir StreamReader örneği (reader) oluşturulur. Bu, veri akışından kolay erişim için bir StreamReader oluşturur.
                // Read the content.
                responseSliptted = reader.ReadToEnd().Split('&');//Bu satırda, reader üzerinden tüm veriyi okur ve ReadToEnd() yöntemiyle dizeye dönüştürür. Sonra, dizeyi '&' karakterine göre böler ve sonuçları responseSliptted değişkenine yerleştirir.
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
