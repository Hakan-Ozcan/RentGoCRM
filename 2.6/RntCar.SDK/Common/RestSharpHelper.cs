using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RntCar.SDK.Common
{
    public class RestSharpHelper
    {

        public String methodname { get; set; }

        public RestSharp.Method type { get; set; }

        private RestClient client { get; set; }

        private RestRequest request { get; set; }

        private IRestResponse response { get; set; }

        public String requestURL { get; set; }


        public RestSharpHelper(String ClientURL, String MethodName, RestSharp.Method Type)
        {
            this.methodname = MethodName;
            this.type = Type;
            this.requestURL = ClientURL;

            client = new RestClient(ClientURL);
            this.PrepareRequest();
        }
        public RestSharpHelper(String ClientURL, String MethodName, RestSharp.Method Type, String HeaderType)
        {
            this.methodname = MethodName;
            this.type = Type;
            this.requestURL = ClientURL;

            client = new RestClient(ClientURL);
            this.PrepareRequest(HeaderType);
        }

        public String MethodName
        {
            get { return methodname; }
            set { methodname = value; }
        }
        public RestSharp.Method Type
        {
            get { return type; }
            set { type = value; }
        }

        public RestClient RestClient
        {
            get { return client; }
        }

        public RestRequest RestRequest
        {
            get { return request; }
        }
        public IRestResponse RestResponse
        {
            get { return response; }
        }

        public void PrepareRequest()
        {
            this.request = new RestRequest(this.MethodName, this.Type);

            request.AddHeader("Accept", "application/json");
            //request.AddHeader("UserName", "cmsclient");
            //request.AddHeader("Password", "123");

        }
        public void PrepareRequest(String HeaderType)
        {
            this.request = new RestRequest(this.MethodName, this.Type);

            request.AddHeader("Accept", HeaderType);
        }

        public void PrepareRequest(Dictionary<string, string> headerParameterList)
        {
            this.request = new RestRequest(this.MethodName, this.Type);
            
            foreach (var headerParameter in headerParameterList)
            {
                request.AddHeader(headerParameter.Key, headerParameter.Value);
            }
        }

        public void AddQueryParameter(String Name, String Value)
        {
            this.RestRequest.AddQueryParameter(Name, Value);
        }

        public void AddJsonParameter<T>(T item) where T : class
        {
            T _newItem = (T)item;
            this.RestRequest.AddParameter("application/json; charset=utf-8", JsonConvert.SerializeObject(item), ParameterType.RequestBody);

        }
        public void AddJsonParameter<T>(string name, T item) where T : class
        {
            T _newItem = (T)item;
            this.RestRequest.AddParameter(name, JsonConvert.SerializeObject(item), ParameterType.RequestBody);

        }

        public void AddParameter(String Name, String Value)
        {
            this.RestRequest.AddParameter(Name, Value);
        }
        public void AddBody<T>(T item) where T : class
        {
            T _newItem = (T)item;
            request.AddBody(_newItem);

        }

        public RestResponseMembers Execute()
        {
            this.response = this.RestClient.Execute(request);

            return RestResponseMembers.GetResponse(this.response);
        }
        public void ExecuteAsync<T>() 
        {
            this.RestClient.ExecuteAsync(
               request,
               response =>
               this.response = response);

        }
        public T Execute<T>()
        {

            this.response = this.RestClient.Execute(request);
            if (string.IsNullOrEmpty(this.response.Content) || this.response.Content == "[]")
            {
                return default(T);
            }
            return RestSharpHelper.Deserialize<T>(this.response.Content);
        }
        public byte[] ExecuteFile()
        {

            this.response = this.RestClient.Execute(request);

            return this.response.RawBytes;
        }
        public static T Deserialize<T>(string Input)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(Input);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }

    public class RestResponseMembers
    {
        public ResponseStatus ResponseStatus { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public String Content { get; set; }
        public String ErrorMessage { get; set; }
        public byte[] RawBytes { get; set; }
        public static RestResponseMembers GetResponse(IRestResponse response)
        {
            return new RestResponseMembers
            {
                RawBytes = response.RawBytes,
                Content = response.Content,
                ResponseStatus = response.ResponseStatus,
                StatusCode = response.StatusCode,
                ErrorMessage = response.ErrorMessage
            };
        }
    }
}
