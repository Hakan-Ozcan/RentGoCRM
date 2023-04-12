using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Models.IYS;
using RntCar.SDK.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RntCar.IYSServicesWebAPI.Controllers
{
    public class EmailEventController : ApiController
    {
        [HttpGet]
        [Route("api/iys/testme")]
        public string testme()
        {
            return "i am ok";
        }


        [HttpPost]
        [Route("api/iys/emailevents/groupunsubscribe")]
        public HttpResponseMessage UnsubscribeGroup([FromBody] EmailEvent emailEvent)
        {
            CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();
            try
            {
                var query = string.Format(@"<fetch version='1.0' output-format='xml-platform' mapping='logical' no-lock='true' distinct='false'>
                          <entity name='rnt_marketingpermissions'>
                            <attribute name='rnt_marketingpermissionsid' />
                            <attribute name='rnt_allowemails' />
                            <order attribute='rnt_name' descending='false' />
                            <link-entity name='contact' from='contactid' to='rnt_contactid' link-type='inner' alias='cnt'>
                              <filter type='and'>
                                <condition attribute='emailaddress1' operator='eq' value='{0}' />
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>", emailEvent.Email);
                Entity updateEntity = null;
                var result = crmServiceHelper_Web.IOrganizationService.RetrieveMultiple(new FetchExpression(query));
                if (result.Entities.Count == 0)
                {
                    return this.Request.CreateResponse(HttpStatusCode.NotFound);
                }
                else
                {
                    foreach (Entity marketingPermission in result.Entities)
                    {
                        if (marketingPermission.Attributes.Contains("rnt_allowemails") && Convert.ToBoolean(marketingPermission.Attributes["rnt_allowemails"]))
                        {
                            updateEntity = new Entity(marketingPermission.LogicalName, marketingPermission.Id);
                            updateEntity["rnt_allowemails"] = false;
                            crmServiceHelper_Web.IOrganizationService.Update(updateEntity);
                        }
                    }
                    return this.Request.CreateResponse(HttpStatusCode.OK);
                }
            }
            catch (Exception ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}
