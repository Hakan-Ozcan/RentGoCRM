using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.ExternalServicesWebAPI.Security;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    [LogoBasicHttpAuthorizeAttribute]
    public class LogoController : ApiController
    {
        [HttpPost]
        [HttpGet]
        [Route("api/logo/testme")]
        public string testme()
        {
            return "i am ok";
        }
        [HttpPost]
        [Route("api/logo/cancelInvoiceByLogoInvoiceNumber")]
        public CancelInvoiceResponse cancelInvoiceByLogoInvoiceNumber([FromBody]CancelInvoiceParameter cancelInvoiceParameter)
        {
            try
            {
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                InvoiceBL invoiceBL = new InvoiceBL(crmServiceHelper.IOrganizationService);
                var response = invoiceBL.cancelInvoiceByLogoInvoiceNumber(cancelInvoiceParameter.logoInvoiceNumber);
                return response;
            }
            catch (Exception ex)
            {
                return new CancelInvoiceResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
    }
}
