using RestSharp;
using RntCar.ClassLibrary._Broker;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

namespace RntCar.BrokerServicesWebXml
{
    /// <summary>
    /// Summary description for BrokerServiceWebXML
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class BrokerServiceWebXML : System.Web.Services.WebService
    {

        public readonly AuthHeader authHeader;


        public string responseUrl;

        public string authorizationValue = "Basic YnJva2VyQ2xpZW50OlltVmphR0Z0Y0dsdmJn";

        [WebMethod]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public string testme()
        {
            ResponseBase responseBase=CheckCredentials();

            return responseBase.responseResult.exceptionDetail;
        }


        ///// <summary>
        ///// Ana veri servisi
        ///// </summary>
        ///// <param name="getMasterDataRequest_Broker"></param>
        ///// <returns></returns>
        //[WebMethod]
        //public LoginResponse_Broker login(LoginRequest_Broker LoginRequest_Broker)
        //{

        //}

        /// <summary>
        /// Ana veri servisi
        /// </summary>
        /// <param name="getMasterDataRequest_Broker"></param>
        /// <returns></returns>
        [WebMethod]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public GetMasterDataResponse_Broker getMasterData(GetMasterDataRequest_Broker getMasterDataRequest_Broker)
        {
            ResponseBase responseBase = CheckCredentials();
            if(!responseBase.responseResult.result)
            {
                var invalidResponse = new GetMasterDataResponse_Broker();
                invalidResponse.responseResult = new ResponseResult();
                invalidResponse.responseResult = responseBase.responseResult;
                return invalidResponse;
            }
            responseUrl = ApiServiceUrl();
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "getMasterData", Method.POST);
            Dictionary<string, string> headerParameterList = new Dictionary<string, string>();
            headerParameterList.Add("Authorization", authorizationValue);

            restSharpHelper.PrepareRequest(headerParameterList);
            restSharpHelper.AddJsonParameter<GetMasterDataRequest_Broker>(getMasterDataRequest_Broker);

            var response = restSharpHelper.Execute<GetMasterDataResponse_Broker>();
            return response;
        }

        /// <summary>
        /// Kullanılabilirlik hesapla
        /// </summary>
        /// <remarks></remarks>
        /// <param name="availabilityParameters_Broker"></param>
        /// <returns></returns>
        [WebMethod]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public AvailabilityResponse_Broker calculateAvailability(AvailabilityParameters_Broker availabilityParameters_Broker)
        {
            ResponseBase responseBase = CheckCredentials();
            if (!responseBase.responseResult.result)
            {
                var invalidResponse =new AvailabilityResponse_Broker();
                invalidResponse.responseResult = new ResponseResult();
                invalidResponse.responseResult = responseBase.responseResult;
                return invalidResponse;
            }
            responseUrl = ApiServiceUrl();
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "calculateAvailability", Method.POST);
            Dictionary<string, string> headerParameterList = new Dictionary<string, string>();
            headerParameterList.Add("Authorization", authorizationValue);

            restSharpHelper.PrepareRequest(headerParameterList);
            restSharpHelper.AddJsonParameter<AvailabilityParameters_Broker>(availabilityParameters_Broker);

            var response = restSharpHelper.Execute<AvailabilityResponse_Broker>();
            return response;
        }

        /// <summary>
        /// Rezervasyon oluştur
        /// </summary>
        /// <remarks></remarks>
        /// <param name="reservationCreateParameters_Broker"></param>
        /// <returns></returns>
        [WebMethod]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public ReservationCreateResponse_Broker createReservation(ReservationCreateParameters_Broker reservationCreateParameters_Broker)
        {
            ResponseBase responseBase = CheckCredentials();
            if (!responseBase.responseResult.result)
            {
                var invalidResponse = new ReservationCreateResponse_Broker();
                invalidResponse.responseResult = new ResponseResult();
                invalidResponse.responseResult = responseBase.responseResult;
                return invalidResponse;
            }
            responseUrl = ApiServiceUrl();
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "createReservation", Method.POST);
            Dictionary<string, string> headerParameterList = new Dictionary<string, string>();
            headerParameterList.Add("Authorization", authorizationValue);

            restSharpHelper.PrepareRequest(headerParameterList);
            restSharpHelper.AddJsonParameter<ReservationCreateParameters_Broker>(reservationCreateParameters_Broker);

            var response = restSharpHelper.Execute<ReservationCreateResponse_Broker>();
            return response;
        }

        /// <summary>
        /// Rezervasyon iptali
        /// </summary>
        /// <param name="cancelReservationParameters_Broker"></param>
        /// <returns></returns>
        [WebMethod]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public CancelReservationResponse_Broker cancelReservation(CancelReservationParameters_Broker cancelReservationParameters_Broker)
        {
            ResponseBase responseBase = CheckCredentials();
            if (!responseBase.responseResult.result)
            {
                var invalidResponse = new CancelReservationResponse_Broker();
                invalidResponse.responseResult = new ResponseResult();
                invalidResponse.responseResult = responseBase.responseResult;
                return invalidResponse;
            }
            responseUrl = ApiServiceUrl();
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "cancelReservation", Method.POST);
            Dictionary<string, string> headerParameterList = new Dictionary<string, string>();
            headerParameterList.Add("Authorization", authorizationValue);

            restSharpHelper.PrepareRequest(headerParameterList);
            restSharpHelper.AddJsonParameter<CancelReservationParameters_Broker>(cancelReservationParameters_Broker);

            var response = restSharpHelper.Execute<CancelReservationResponse_Broker>();
            return response;
        }

        /// <summary>
        /// Ek ürün listesi
        /// </summary>
        /// <param name="cancelReservationParameters_Broker"></param>
        /// <returns></returns>
        [WebMethod]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public AdditionalProductResponse_Broker getAdditionalProducts(AdditionalProductParameters_Broker additionalProductParameters_Broker)
        {
            ResponseBase responseBase = CheckCredentials();
            if (!responseBase.responseResult.result)
            {
                var invalidResponse = new AdditionalProductResponse_Broker();
                invalidResponse.responseResult = new ResponseResult();
                invalidResponse.responseResult = responseBase.responseResult;
                return invalidResponse;
            }
            responseUrl = ApiServiceUrl();
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "getAdditionalProducts", Method.POST);
            Dictionary<string, string> headerParameterList = new Dictionary<string, string>();
            headerParameterList.Add("Authorization", authorizationValue);

            restSharpHelper.PrepareRequest(headerParameterList);
            restSharpHelper.AddJsonParameter<AdditionalProductParameters_Broker>(additionalProductParameters_Broker);

            var response = restSharpHelper.Execute<AdditionalProductResponse_Broker>();
            return response;
        }

        [WebMethod]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public GetCustomerReservationsResponse_Broker getReservations(GetReservationsRequest_Broker getReservationsRequest_Broker)
        {
            ResponseBase responseBase = CheckCredentials();
            if (!responseBase.responseResult.result)
            {
                var invalidResponse = new GetCustomerReservationsResponse_Broker();
                invalidResponse.responseResult = new ResponseResult();
                invalidResponse.responseResult = responseBase.responseResult;
                return invalidResponse;
            }
            responseUrl = ApiServiceUrl();
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "getReservations", Method.POST);
            Dictionary<string, string> headerParameterList = new Dictionary<string, string>();
            headerParameterList.Add("Authorization", authorizationValue);

            restSharpHelper.PrepareRequest(headerParameterList);
            restSharpHelper.AddJsonParameter<GetReservationsRequest_Broker>(getReservationsRequest_Broker);

            var response = restSharpHelper.Execute<GetCustomerReservationsResponse_Broker>();
            return response;
        }

        private ResponseBase CheckCredentials()
        {
            var _userName = StaticHelper.GetConfiguration("xml_username");
            var _password = StaticHelper.GetConfiguration("xml_password");
            ResponseBase responseBase = new ResponseBase();
            responseBase.responseResult = new ResponseResult();
            responseBase.responseResult.result = true;
            responseBase.responseResult.exceptionDetail = "Success";
            if (authHeader.UserName != _userName ||
                authHeader.Password != _password)
            {
                responseBase.responseResult.result = false;
                responseBase.responseResult.exceptionDetail = "Invalid User";
            }

            return responseBase;
        }

        private string ApiServiceUrl()
        {
            var _brokerserviceurl = StaticHelper.GetConfiguration("brokerserviceurl");
           
            return _brokerserviceurl;
        }

    }
}
