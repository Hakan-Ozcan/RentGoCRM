using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.IntegrationHelper
{
    public class HopiHelper : IDisposable
    {
        private static EndpointAddress myEndpointAddress { get; set; }
        private static BasicHttpBinding myBasicHttpBinding { get; set; }
        public IOrganizationService orgService { get; set; }
        public ITracingService tracingService { get; set; }

        private HopiService.PosPortClient PosPortClient { get; set; }
        private OperationContextScope scope { get; set; }
        private string[] loginInfo { get; set; }
        private string endpointUrl { get; set; }

        public HopiHelper(IOrganizationService _service)
        {
            orgService = _service;
            prepareServiceConfiguration();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            PosPortClient = new HopiService.PosPortClient(myBasicHttpBinding, myEndpointAddress);
            scope = new OperationContextScope(PosPortClient.InnerChannel);
            var sec = new SecurityHeader(loginInfo[0], loginInfo[1]);
            OperationContext.Current.OutgoingMessageHeaders.Add(sec);
        }
        public HopiHelper(IOrganizationService _service, ITracingService _tracingService, bool useLocalIP = false)
        {
            orgService = _service;
            tracingService = _tracingService;
            prepareServiceConfiguration();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            PosPortClient = new HopiService.PosPortClient(myBasicHttpBinding, myEndpointAddress);
            scope = new OperationContextScope(PosPortClient.InnerChannel);
            var sec = new SecurityHeader(loginInfo[0], loginInfo[1]);
            OperationContext.Current.OutgoingMessageHeaders.Add(sec);

        }
        private void prepareServiceConfiguration()
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.orgService);
            loginInfo = configurationRepository.GetConfigurationByKey("hopiServiceLoginInfo").Split(';');
            endpointUrl = configurationRepository.GetConfigurationByKey("hopiEndPointUrl");

            myBasicHttpBinding = new BasicHttpBinding();
            myBasicHttpBinding.Name = "HopiServiceSoap";
            myBasicHttpBinding.Security.Mode = BasicHttpSecurityMode.Transport;
            myBasicHttpBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            myBasicHttpBinding.OpenTimeout = TimeSpan.FromMinutes(10);
            myBasicHttpBinding.CloseTimeout = TimeSpan.FromMinutes(10);
            myBasicHttpBinding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            myBasicHttpBinding.SendTimeout = TimeSpan.FromMinutes(10);
            myBasicHttpBinding.MaxReceivedMessageSize = 2147483647;

            myEndpointAddress = new EndpointAddress(endpointUrl);

        }


        public GetBirdUserInfoResponse getBirdUserInfo(GetBirdUserInfoRequest getBirdUserInfoRequest)
        {

            var convertedParameter = new HopiService.GetBirdUserInfoRequest
            {
                merchantCode = getBirdUserInfoRequest.merchantCode,
                storeCode = getBirdUserInfoRequest.storeCode,
                ItemElementName = HopiService.ItemChoiceType1.token,
                Item = getBirdUserInfoRequest.token
            };

            try
            {
                var response = PosPortClient.GetBirdUserInfo(convertedParameter);

                return new GetBirdUserInfoResponse
                {
                    birdId = response.birdId,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new GetBirdUserInfoResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }

        }
        public StartCoinTransactionResponse startCoinTransaction(StartCoinTransactionRequest startCoinTransactionRequest)
        {

            var convertedParameter = new HopiService.StartCoinTransactionRequest
            {
                amount = startCoinTransactionRequest.amount,
                billAmount = startCoinTransactionRequest.billAmount,
                storeCode = startCoinTransactionRequest.storeCode,
                merchantCode = startCoinTransactionRequest.merchantCode,
                birdId = startCoinTransactionRequest.birdId,
            };

            try
            {
                var response = PosPortClient.StartCoinTransaction(convertedParameter);

                return new StartCoinTransactionResponse
                {
                    otpNeeded = response.otpNeeded,
                    provisionId = Convert.ToString(response.provisionId),
                    ResponseResult = ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new StartCoinTransactionResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }

        }
        public StartReturnTransactionResponse StartReturnTransaction(StartReturnTransactionRequest startReturnTransactionRequest)
        {

            var convertedParameter = new HopiService.StartReturnTransactionRequest
            {
                merchantCode = startReturnTransactionRequest.merchantCode,
                storeCode = startReturnTransactionRequest.storeCode,
                transactionId = startReturnTransactionRequest.transactionId,
                campaignFreeAmount = startReturnTransactionRequest.campaignFreeAmount

            };

            try
            {
                var response = PosPortClient.StartReturnTransaction(convertedParameter);

                return new StartReturnTransactionResponse
                {
                    residual = response.residual,
                    returnTrxId = response.returnTrxId,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new StartReturnTransactionResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        public void CompleteCoinTransaction(CompleteCoinTransactionRequest completeCoinTransactionRequest)
        {
            var convertedParameter = new HopiService.CompleteCoinTransactionRequest { };
            PosPortClient.CompleteCoinTransaction(convertedParameter);
        }
        public void CancelCoinTransaction(CancelCoinTransactionRequest cancelCoinTransactionRequest)
        {
            var convertedParameter = new HopiService.CancelCoinTransactionRequest { };
            PosPortClient.CancelCoinTransaction(convertedParameter);
        }
        public void CancelReturnTransaction(CancelReturnTransactionRequest cancelReturnTransactionRequest)
        {
            var convertedParameter = new HopiService.CancelReturnTransactionRequest { };
            PosPortClient.CancelReturnTransaction(convertedParameter);
        }
        public void RefundCoin(RefundCoinRequest refundCoinRequest)
        {
            var convertedParameter = new HopiService.RefundCoinRequest { };
            PosPortClient.RefundCoin(convertedParameter);
        }
        public NotifyCheckoutResponse NotifyCheckout(NotifyCheckoutRequest notifyCheckoutRequest)
        {
            HopiService.AmountDetail payment = new HopiService.AmountDetail();
            payment.amount = notifyCheckoutRequest.paymentDetails[0].amount;
            payment.Item = notifyCheckoutRequest.paymentDetails[0].amount - Math.Round(notifyCheckoutRequest.paymentDetails[0].amount / (1 + (notifyCheckoutRequest.paymentDetails[0].percent / 100)), 2);

            //HopiService.TransactionInfo transactionData = new HopiService.TransactionInfo();
            //transactionData.barcode = notifyCheckoutRequest.transactionInfos[0].barcode;
            //transactionData.amount = notifyCheckoutRequest.transactionInfos[0].amount;
            //transactionData.quantity = notifyCheckoutRequest.transactionInfos[0].quantity;
            //transactionData.campaign = notifyCheckoutRequest.transactionInfos[0].campaign;

            HopiService.AmountDetail subTotal = new HopiService.AmountDetail();
            subTotal.amount = notifyCheckoutRequest.subtotalDetails[0].amount;
            subTotal.Item = notifyCheckoutRequest.subtotalDetails[0].amount - Math.Round(notifyCheckoutRequest.subtotalDetails[0].amount / (1 + (notifyCheckoutRequest.subtotalDetails[0].percent / 100)), 2);

            HopiService.UsedCampaignDetail campaign = new HopiService.UsedCampaignDetail();

            HopiService.AmountDetail campaignPayment = new HopiService.AmountDetail();
            campaignPayment.amount = notifyCheckoutRequest.usedCampaignDetails[0].amountDetails[0].amount;
            campaignPayment.Item = notifyCheckoutRequest.usedCampaignDetails[0].amountDetails[0].amount - Math.Round(notifyCheckoutRequest.usedCampaignDetails[0].amountDetails[0].amount / (1 + (notifyCheckoutRequest.usedCampaignDetails[0].amountDetails[0].percent / 100)), 2);

            campaign.amountDetails = new HopiService.AmountDetail[]
            {
                    campaignPayment
            };
            campaign.campaignCode = notifyCheckoutRequest.usedCampaignDetails[0].campaignCode;

            campaign.benefit = new HopiService.BenefitDetail();
            campaign.benefit.Items = new object[] { notifyCheckoutRequest.usedCampaignDetails[0].benefit.coins };

            HopiService.AmountDetail discount = new HopiService.AmountDetail();
            discount.amount = 75;
            discount.Item = 1;


            //HopiService.AmountDetail campaignFreePayment = new HopiService.AmountDetail();
            //campaignFreePayment.amount = notifyCheckoutRequest.campaignFreePaymentDetails[0].amount;
            //campaignFreePayment.Item = notifyCheckoutRequest.campaignFreePaymentDetails[0].percent;

            var convertedParameter = new HopiService.Checkout
            {
                birdId = notifyCheckoutRequest.birdId,
                merchantCode = notifyCheckoutRequest.merchantCode,
                storeCode = notifyCheckoutRequest.storeCode,
                transactionId = notifyCheckoutRequest.transactionId,
                dateTime = notifyCheckoutRequest.dateTime,
                paymentDetails = new HopiService.AmountDetail[] { payment },
                //transactionInfos = new HopiService.TransactionInfo[] { transactionData },
                subtotalDetails = new HopiService.AmountDetail[] { subTotal },
                usedCampaignDetails = new HopiService.UsedCampaignDetail[] { campaign },
                //campaignFreePaymentDetails = new HopiService.AmountDetail[] { campaignFreePayment }
            };

            try
            {
                tracingService.Trace("request : " + JsonConvert.SerializeObject(convertedParameter));
                PosPortClient.NotifyCheckout(convertedParameter);

                return new NotifyCheckoutResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new NotifyCheckoutResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }


        ~HopiHelper()
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
                this.PosPortClient = null;
                this.orgService = null;
                this.tracingService = null;
                this.scope = null;
            }
        }

    }
}