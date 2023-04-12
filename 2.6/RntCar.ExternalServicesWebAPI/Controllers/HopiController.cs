using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary;
using RntCar.IntegrationHelper;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    public class HopiController : ApiController
    {
        CrmServiceHelper crmServiceHelper = new CrmServiceHelper();


        [HttpPost]
        [Route("api/hopi/startCoinTransaction")]
        public StartCoinTransactionResponse startCoinTransaction([FromBody] StartCoinTransactionRequest startCoinTransactionRequest)
        {
            HopiHelper hopiHelper = new HopiHelper(crmServiceHelper.IOrganizationService);

            var response = hopiHelper.startCoinTransaction(startCoinTransactionRequest);

            return new StartCoinTransactionResponse
            {
                otpNeeded = response.otpNeeded,
                provisionId = Convert.ToString(response.provisionId),
                ResponseResult = response.ResponseResult            
            };

        }

        [HttpPost]
        [Route("api/hopi/getBirdUserInfo")]
        public GetBirdUserInfoResponse getBirdUserInfo([FromBody] GetBirdUserInfoRequest getBirdUserInfoRequest)
        {
            HopiHelper hopiHelper = new HopiHelper(crmServiceHelper.IOrganizationService);
            var response = hopiHelper.getBirdUserInfo(getBirdUserInfoRequest);

            return new GetBirdUserInfoResponse
            {
                birdId = response.birdId,
                ResponseResult = response.ResponseResult
            };

        }

        [HttpPost]
        [Route("api/hopi/startReturnTransaction")]
        public StartReturnTransactionResponse startReturnTransaction([FromBody] StartReturnTransactionRequest startReturnTransactionRequest)
        {
            HopiHelper hopiHelper = new HopiHelper(crmServiceHelper.IOrganizationService);
            var response = hopiHelper.StartReturnTransaction(startReturnTransactionRequest);

            return new StartReturnTransactionResponse
            {
                residual = response.residual,
                returnTrxId = response.returnTrxId,
                ResponseResult = response.ResponseResult
            };

        }

        [HttpPost]
        [Route("api/hopi/completeCoinTransaction")]
        public void completeCoinTransaction([FromBody] CompleteCoinTransactionRequest completeCoinTransactionRequest)
        {
            HopiHelper hopiHelper = new HopiHelper(crmServiceHelper.IOrganizationService);
            hopiHelper.CompleteCoinTransaction(completeCoinTransactionRequest);

        }

        [HttpPost]
        [Route("api/hopi/cancelCoinTransaction")]
        public void cancelCoinTransaction([FromBody] CancelCoinTransactionRequest cancelCoinTransactionRequest)
        {
            HopiHelper hopiHelper = new HopiHelper(crmServiceHelper.IOrganizationService);
            hopiHelper.CancelCoinTransaction(cancelCoinTransactionRequest);

        }

        [HttpPost]
        [Route("api/hopi/cancelReturnTransaction")]
        public void cancelReturnTransaction([FromBody] CancelReturnTransactionRequest cancelReturnTransactionRequest)
        {
            HopiHelper hopiHelper = new HopiHelper(crmServiceHelper.IOrganizationService);
            hopiHelper.CancelReturnTransaction(cancelReturnTransactionRequest);

        }

        [HttpPost]
        [Route("api/hopi/notifyCheckout")]
        public NotifyCheckoutResponse notifyCheckout([FromBody] NotifyCheckoutRequest notifyCheckoutRequest)
        {
            HopiHelper hopiHelper = new HopiHelper(crmServiceHelper.IOrganizationService);
            var response = hopiHelper.NotifyCheckout(notifyCheckoutRequest);

            return new NotifyCheckoutResponse
            {
                ResponseResult = response.ResponseResult
            };
        }

    }
}