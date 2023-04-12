using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RestSharp;
using RestSharp.Authenticators;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.OptimumIntegration
{
    public class OptimumIntegrationHelper
    {

        private IOrganizationService Service;
        public OptimumIntegrationHelper(IOrganizationService _service)
        {
            Service = _service;
        }

        public Entity GetTransferByLicensePlate(string licensePlate)
        {
            var fetchQuery = $@"<fetch version='1.0' top='1' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='rnt_transfer'>
                                    <attribute name='rnt_transferid' />
                                    <attribute name='rnt_maintenancekm' />
                                    <attribute name='rnt_ismaintained' />
                                    <attribute name='rnt_name' />
                                    <attribute name='createdon' />
                                    <order attribute='createdon' descending='true' />
                                    <filter type='and'>
                                      <condition attribute='rnt_transfertype' operator='eq' value='20' />   
                                    </filter>
                                    <link-entity name='rnt_equipment' from='rnt_equipmentid' to='rnt_equipmentid' link-type='inner' alias='aa'>
                                      <filter type='and'>
                                        <condition attribute='rnt_name' operator='eq' value='{licensePlate}' />
                                        <condition attribute='statecode' operator='eq' value='0' />
                                      </filter>
                                    </link-entity>
                                  </entity>
                                </fetch>";

            var transferRecords = Service.RetrieveMultiple(new FetchExpression(fetchQuery));

            if (transferRecords.Entities.Count > 0)
            {
                return transferRecords.Entities.FirstOrDefault();
            }

            return null;
        }

        public OptimumResponse GetMaintainedEquipmentsByDate(DateTime date)
        {
            try
            {
                Uri baseUrl = new Uri("https://orp.optimumcozum.com/integrations/web-service-apis/rent-go/claims");
                IRestClient client = new RestClient(baseUrl);
                IRestRequest request = new RestRequest("", Method.GET);
                client.Authenticator = new HttpBasicAuthenticator(StaticHelper.GetConfiguration("OptimumUsername"), StaticHelper.GetConfiguration("OptimumPassword"));
                request.AddParameter("createdDate", date.ToString("yyyy-MM-dd"));

                IRestResponse<OptimumResponse> response = client.Execute<OptimumResponse>(request);

                if (response.IsSuccessful)
                {
                    return response.Data;
                }
                else
                {
                    Console.WriteLine(response.ErrorMessage);
                    return null;
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public void Process()
        {
            var optimumRecords = GetMaintainedEquipmentsByDate(DateTime.Now.AddDays(-5));

            if (optimumRecords != null)
            {
                var maintanceList = optimumRecords.output.Where(x => x.claimType == "maintenance").DistinctBy(x => x.registrationNumber).ToList();
                foreach (var item in maintanceList)
                {
                    var transferRec = GetTransferByLicensePlate(item.registrationNumber);

                    if (transferRec != null)
                    {
                        try
                        {
                            bool isMaintained = transferRec.GetAttributeValue<bool>("rnt_ismaintained");
                            if (isMaintained == null || isMaintained == false)
                            {
                                transferRec["rnt_currentkm"] = int.Parse(item.currentKm);
                                transferRec["rnt_nextmaintenancekm"] = item.nextMaintenanceKm;
                                transferRec["rnt_maintenancekm"] = int.Parse(item.maintenanceKm);
                                transferRec["rnt_ismaintained"] = item.isMaintained;

                                Service.Update(transferRec);
                            }
                        }
                        catch (Exception ex)
                        {
                            Entity ent = new Entity(transferRec.LogicalName, transferRec.Id);
                            ent["rnt_optimumintegrationerror"] = ex.Message;
                            Service.Update(ent);
                        }
                    }

                }
            }
        }
    }
}
