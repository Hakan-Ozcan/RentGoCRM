using RntCar.ClassLibrary;
using RntCar.IntegrationHelper;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    public class HGSController : ApiController
    {

        [HttpGet]
        [HttpPost]
        [Route("api/hgs/testme")]
        public string testme()
        {
            return "i am hgs ok";
        }

        [HttpPost]
        [Route("api/hgs/saleProduct")]
        public SaleProductResponse saleProduct([FromBody] SaleProductParameter saleProductParameter)
        {
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

                using (var hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService))
                {
                    return hgsHelper.saleProduct(saleProductParameter);
                }
            }
            catch (Exception ex)
            {
                return new SaleProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/hgs/cancelProduct")]
        public CancelProductResponse cancelProduct([FromBody] CancelProductParameter cancelProductParameter)
        {
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

                using (var hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService))
                {
                    return hgsHelper.cancelProduct(cancelProductParameter);
                }
            }
            catch (Exception ex)
            {
                return new CancelProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/hgs/updateDirectiveAmounts")]
        public UpdateDirectiveAmountsResponse updateDirectiveAmounts([FromBody] UpdateDirectiveAmountsParameter updateDirectiveAmountsParameter)
        {
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

                using (var hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService))
                {
                    return hgsHelper.updateDirectiveAmounts(updateDirectiveAmountsParameter);
                }
            }
            catch (Exception ex)
            {
                return new UpdateDirectiveAmountsResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/hgs/updateVehicleInfo")]
        public UpdateVehicleInfoResponse updateVehicleInfo([FromBody] UpdateVehicleInfoParameter updateVehicleInfoParameter)
        {
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

                using (var hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService))
                {
                    return hgsHelper.updateVehicleInfo(updateVehicleInfoParameter);
                }
            }
            catch (Exception ex)
            {
                return new UpdateVehicleInfoResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/hgs/getHgsTransactionList")]
        public GetHGSTransactionListResponse getHgsTransactionList([FromBody] GetHGSTransactionListParameter getHGSTransactionListParameter)
        {
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

                using (var hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService))
                {
                    return hgsHelper.getHgsTransactionList(getHGSTransactionListParameter);
                }
            }
            catch (Exception ex)
            {
                return new GetHGSTransactionListResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
    }
}