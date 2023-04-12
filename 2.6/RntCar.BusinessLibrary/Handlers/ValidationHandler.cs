using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Handlers
{
    public class ValidationHandler : HandlerBase, IDisposable
    {

        public Guid? UserId { get; set; }
        public IOrganizationService OrgService { get; set; }
        public ITracingService TracingService { get; set; }
       

        public ValidationHandler(IOrganizationService orgService)
        {
            this.OrgService = orgService;
        }
        public ValidationHandler(IOrganizationService orgService, ITracingService tracingService)
        {
            this.OrgService = orgService;
            this.TracingService = tracingService;
        }
        public ValidationHandler(IOrganizationService orgService, Guid userId)
        {
            this.UserId = userId;
            this.OrgService = orgService;

        }

        public void Trace(string traceText)
        {
            if (this.TracingService != null)
            {
                this.TracingService.Trace(traceText);
            }
        }

        ~ValidationHandler()
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
                UserId = null;
                OrgService = null;
            }

        }
    }
}
