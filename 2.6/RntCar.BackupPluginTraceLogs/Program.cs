using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.Logger;
using RntCar.MongoDBHelper.Entities;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BackupPluginTraceLogs
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            LoggerHelper loggerHelper = new LoggerHelper();

            PluginTraceRepository pluginTraceRepository = new PluginTraceRepository(crmServiceHelper.IOrganizationService);
            var logs = pluginTraceRepository.getAllPluginTraceLogs();
            //loggerHelper.traceInfo("retrieve end");

            PluginTraceBL pluginTraceBL = new PluginTraceBL(crmServiceHelper.IOrganizationService);
            var convertedLogs = pluginTraceBL.convertEntityToLogDetail(logs);

            LogBusiness logBusiness = new LogBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            List<EntityReference> entityReferences = new List<EntityReference>();
            List<LogDetail> subDetail = new List<LogDetail>();
            var counter = 0;
            foreach (var item in convertedLogs)
            {
                try
                {
                    subDetail.Add(item);
                    entityReferences.Add(new EntityReference { Id = new Guid(item.pluginTraceLogId), LogicalName = "plugintracelog" });
                    if(entityReferences.Count == 1000)
                    {
                        logBusiness.createLog(subDetail);
                        BulkDelete(crmServiceHelper.IOrganizationService, entityReferences);
                        counter += entityReferences.Count;
                        subDetail.Clear();
                        entityReferences.Clear();
                        
                        loggerHelper.traceInfo("totalCount " + counter);
                    }                  
                }
                catch (Exception ex)
                {
                    loggerHelper.traceInfo(ex.Message);
                    continue;
                }
            }        

       }
        public static void BulkDelete(IOrganizationService service, List<EntityReference> entityReferences)
        {
            // Create an ExecuteMultipleRequest object.
            var multipleRequest = new ExecuteMultipleRequest()
            {
                // Assign settings that define execution behavior: continue on error, return responses. 
                Settings = new ExecuteMultipleSettings()
                {
                    ContinueOnError = true,
                    ReturnResponses = true
                },
                // Create an empty organization request collection.
                Requests = new OrganizationRequestCollection()
            };

            // Add a DeleteRequest for each entity to the request collection.
            foreach (var entityRef in entityReferences)
            {
                DeleteRequest deleteRequest = new DeleteRequest { Target = entityRef };
                multipleRequest.Requests.Add(deleteRequest);
            }

            // Execute all the requests in the request collection using a single web method call.
            ExecuteMultipleResponse multipleResponse = (ExecuteMultipleResponse)service.Execute(multipleRequest);
        }
    }
}
