using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class PluginTraceRepository : RepositoryHandler
    {
        public PluginTraceRepository(IOrganizationService Service) : base(Service)
        {
        }

        public PluginTraceRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public PluginTraceRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public PluginTraceRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public PluginTraceRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public List<Entity> getAllPluginTraceLogs()
        {
            List<Entity> pluginLogs = new List<Entity>();
            // Set the number of records per page to retrieve.
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;

            string pagingCookie = null;
            while (true)
            {
                QueryExpression query = new QueryExpression("plugintracelog");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                query.ColumnSet = new ColumnSet(true);
                query.AddOrder("createdon", OrderType.Ascending);
                var results = this.retrieveMultiple(query);
                pluginLogs.AddRange(results.Entities.ToList());
               
                if (results.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = results.PagingCookie;
                }
                else
                {
                    break;
                }
            }
            return pluginLogs;
        }
    }
}
