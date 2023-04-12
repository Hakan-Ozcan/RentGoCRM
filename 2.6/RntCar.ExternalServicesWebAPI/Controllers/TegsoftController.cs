using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Web.Tegsoft;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    public class TegsoftController : ApiController
    {
        [HttpPost]
        [Route("api/tegsoft/savecardinfo")]
        public CardInfoResponse SaveCardInfo(CardInfo cardInfo)
        {
            CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();
            try
            {

                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper_Web.IOrganizationService);
                var contactId = individualCustomerRepository.getExistingCustomerIdByGovernmentIdOrPassportNumber(cardInfo.GovernmentId, new string[] { "contactid" });

                if (contactId != null)
                {
                    var creditCardParams = new CreateCreditCardParameters
                    {
                        cardAlias = cardInfo.CardNumber,
                        cardHolderName = cardInfo.FullName,
                        creditCardNumber = cardInfo.CardNumber.removeEmptyCharacters(),
                        cvc = cardInfo.Cvv.ToString(),
                        expireMonth = cardInfo.ExpireDateMonth,
                        expireYear = cardInfo.ExpireDateYear,
                        individualCustomerId = Convert.ToString(contactId)
                    };
                    
                    OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CreateCustomerCreditCard");
                    organizationRequest["creditCardParameters"] = Convert.ToString(JsonConvert.SerializeObject(creditCardParams));
                    crmServiceHelper_Web.IOrganizationService.Execute(organizationRequest);


                    return new CardInfoResponse
                    {
                        responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnSuccess()
                    };
                }
                else
                {
                    return new CardInfoResponse
                    {
                        responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnError("User not found!")
                    };
                }
            }
            catch (Exception ex)
            {
                return new CardInfoResponse
                {
                    responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
    }
}
