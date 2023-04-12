using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository.WebAPI
{
    public class IndividiualCustomerWebAPIRepository : CrmWebAPIRepositoryHandler
    {
        public IndividiualCustomerWebAPIRepository(RestClient restClient) : base(restClient)
        {
        }

        public IndividiualCustomerWebAPIRepository(RestClient restClient, Guid userId) : base(restClient, userId)
        {
        }

        public IndividiualCustomerWebAPIRepository(RestClient restClient, string organizationName) : base(restClient, organizationName)
        {
        }

        public IndividiualCustomerWebAPIRepository(RestClient restClient, Guid userId, string organizationName) : base(restClient, userId, organizationName)
        {
        }

        public List<IndividualCustomerData> GetCustomerByGivenCriteriaWithGivenColumns(String searchText, String[] columns)
        {
            string queryFilters = String.Format(@"&$filter=contains(firstname,'{0}') 
                                                        or contains(lastname,'{0}')
                                                        or contains(governmentid,'{0}') 
                                                        or contains(mobilephone,'{0}')", searchText);
            string queryOptions = "?$select=" + String.Join(",", columns) + queryFilters;


            var request1 = new RestRequest(Method.GET);
            this.RestClient.BaseUrl = new Uri(this.RestClient.BaseUrl + "/contacts" + queryOptions);



            //request1.AddHeader("MSCRMCallerID", "E81C0BC0-07A2-E811-940E-000C293D97F8");
            IRestResponse response1 = this.RestClient.Execute(request1);
            var collection = JsonConvert.DeserializeObject<JObject>(response1.Content);

            JToken valArray;
            JArray array;
            if (collection.TryGetValue("value", out valArray))
            {
                array = (JArray)valArray;
            }
            else
            {
                array = new JArray(collection);
            }
            List<IndividualCustomerData> contactList = new List<IndividualCustomerData>();
            foreach (JObject entity in array)
            {
                contactList.Add(new IndividualCustomerData
                {
                    FirstName = entity["firstname"].ToString(),
                    LastName = entity["lastname"].ToString(),
                    GovernmentId = entity["governmentid"].ToString(),
                    MobilePhone = entity["mobilephone"].ToString()
                });
            }

            return contactList;
        }
        //public async Task<HttpResponseMessage> SendCrmRequestAsync(
        //    HttpMethod method, string query, Boolean formatted = false, int maxPageSize = 100, String userId = "")
        //{
        //    try

        //    {
        //        HttpRequestMessage request = new HttpRequestMessage(method, query);
        //        request.Headers.Add("Prefer", "odata.maxpagesize=" + maxPageSize.ToString());
        //        if (formatted)
        //            request.Headers.Add("Prefer",
        //                "odata.include-annotations=OData.Community.Display.V1.FormattedValue");
        //        //impersonation
        //        //request.Headers.Add("MSCRMCallerID", "E81C0BC0-07A2-E811-940E-000C293D97F8");
        //        return await this.HttpClient.SendAsync(request);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
    }
}
