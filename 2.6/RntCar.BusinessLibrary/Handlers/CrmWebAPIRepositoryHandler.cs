using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Handlers
{
    public class CrmWebAPIRepositoryHandler : IDisposable
    {

        public RestClient RestClient { get; set; }
        public Guid UserId { get; set; }
        public String OrganizationName { get; set; }

        public CrmWebAPIRepositoryHandler(RestClient restClient)
        {
            this.RestClient = restClient;
        }
        public CrmWebAPIRepositoryHandler(RestClient restClient, Guid userId)
        {
            this.RestClient = restClient;
            this.UserId = userId;
        }

        public CrmWebAPIRepositoryHandler(RestClient restClient, String organizationName)
        {
            this.RestClient = restClient;
            this.OrganizationName = organizationName;
        }
        public CrmWebAPIRepositoryHandler(RestClient restClient, Guid userId, String organizationName)
        {
            this.RestClient = restClient;
            this.OrganizationName = organizationName;
            this.UserId = userId;
        }
       
       
        
        ~CrmWebAPIRepositoryHandler()
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
                this.RestClient = null;
                this.OrganizationName = null;
                
            }

        }
    }
}
