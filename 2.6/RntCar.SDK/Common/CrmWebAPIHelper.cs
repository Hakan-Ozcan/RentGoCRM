using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Common
{
    public class CrmWebAPIHelper
    {
        /// <summary>
        /// Creates a Httpclient for CRM WebAPI operations
        /// </summary>
        /// <param name="timeout">Determines timeout of the connection</param>
        /// <returns></returns>
        public HttpClient CreateHttpClient(int timeout = 0)
        {
            //var _clientHandler = new HttpClientHandler()
            //{ Credentials = new NetworkCredential(StaticHelper.GetConfiguration("CrmUser"), StaticHelper.GetConfiguration("CrmPassword"), "") };
            var _clientHandler = new HttpClientHandler()
            { Credentials = new NetworkCredential("administrator", "Tunalar!2456", "tuna") };

            var httpClient = new HttpClient(_clientHandler, false);

            httpClient.BaseAddress = new Uri("http://localhost/RentgoCrmDev/api/data/v8.2/");// new Uri(StaticHelper.GetConfiguration("CrmWebAPIUrl"));
            var t = timeout == 0 ? 2 : timeout;
            httpClient.Timeout = new TimeSpan(0, t, 0);
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }

        public RestClient CreateRestClient()
        {

            var client = new RestClient("http://localhost/RentgoCrmDev/api/data/v8.2/");
           // var client = new RestClient("http://176.235.168.154/RentgoCrmDev/api/data/v8.2/");
            client.Authenticator = new NtlmAuthenticator("tunalar\\administrator", "Tunalar!2456");
            client.AddDefaultHeader("OData-MaxVersion", "4.0");
            client.AddDefaultHeader("OData-Version", "4.0");
            return client;
        }
    }
    

}
