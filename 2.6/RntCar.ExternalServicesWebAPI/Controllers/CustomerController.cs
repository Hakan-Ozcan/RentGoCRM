using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Web.Contract;
using RntCar.ClassLibrary._Web.IndividualCustomer;
using RntCar.ExternalServices.Security;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Linq;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "X-My-Header")]
    [BasicHttpAuthorizeAttribute("Web")]
    public class CustomerController : ApiController
    {
        [HttpPost]
        [Route("api/customer/get")]
        public IndividualCustomerMarketingPermissionResponse get(IndividualCustomerMarketingPermissionRequest individualCustomerMarketingPermissionDataParameters)
        {
            CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();
            try
            {
                var date = new DateTime(individualCustomerMarketingPermissionDataParameters.createdOnTimeStamp).ToString("yyyy-MM-dd");

                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper_Web.IOrganizationService);
                var data = individualCustomerRepository.getContactsByGiventDatePermissions(date);

                var transformed = new IndividualCustomerMapper().buildCustomerMarketingPermissionData(data);
                return new IndividualCustomerMarketingPermissionResponse
                {
                    individualCustomers = transformed,
                    responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new IndividualCustomerMarketingPermissionResponse
                {
                    responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/customer/getByPermissionDate")]
        public IndividualCustomerMarketingPermissionResponse getByPermissionDate(IndividualCustomerMarketingPermissionRequest individualCustomerMarketingPermissionDataParameters)
        {
            CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();
            try
            {
                var date = new DateTime(individualCustomerMarketingPermissionDataParameters.createdOnTimeStamp).ToString("yyyy-MM-dd");

                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper_Web.IOrganizationService);
                var data = individualCustomerRepository.getContactsByMarketingDatePermissions(date);

                var transformed = new IndividualCustomerMapper().buildCustomerMarketingPermissionData(data);
                return new IndividualCustomerMarketingPermissionResponse
                {
                    individualCustomers = transformed,
                    responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new IndividualCustomerMarketingPermissionResponse
                {
                    responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/customer/getLastRentalInformation")]
        public GetLastRentalInformationResponse getLastRentalInformation(GetLastRentalInformationRequest getLastRentalInformationRequest)
        {
            CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();
            try
            {
                var str = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='rnt_contract'>
                                <attribute name='rnt_dropoffdatetime' />
                                <attribute name='rnt_pickupdatetime' />    
                                <order attribute='rnt_dropoffdatetime' descending='true' />
                                <filter type='and'>                                  
                                  <condition attribute='statuscode' operator='eq' value='100000001' />
                                </filter>  
                                <link-entity name='contact' from='contactid' to='rnt_customerid' link-type='inner' alias='ad'>
                                      <filter type='and'>
                                        <condition attribute='rnt_customernumber' operator='eq' value='{0}' />
                                      </filter>
                                </link-entity>
                              </entity>
                            </fetch>", getLastRentalInformationRequest.individualCustomerId);

                var res = crmServiceHelper_Web.IOrganizationService.RetrieveMultiple(new FetchExpression(str));
                if(res.Entities.FirstOrDefault() == null)
                {
                    return new GetLastRentalInformationResponse
                    {
                        responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnSuccess()
                    };
                }
                return new GetLastRentalInformationResponse
                {
                    contractId = res.Entities.FirstOrDefault().Id,
                    dropffDateTime = res.Entities.FirstOrDefault().GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                    pickupDateTime = res.Entities.FirstOrDefault().GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                    responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {

                return new GetLastRentalInformationResponse
                {
                    responseResult = RntCar.ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
    }
}
