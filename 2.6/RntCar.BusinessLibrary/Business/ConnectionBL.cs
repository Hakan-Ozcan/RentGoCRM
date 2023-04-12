using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class ConnectionBL : BusinessHandler
    {
        public ConnectionBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public ConnectionBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public ConnectionBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public Guid createConnection(Guid? contactId, Guid accountId, int relationCode)
        {
            Entity e = new Entity("rnt_connection");
            if (contactId.HasValue)
            {
                e.Attributes["rnt_contactid"] = new EntityReference("contact", contactId.Value);
            }
            e.Attributes["rnt_accountid"] = new EntityReference("account", accountId);
            e.Attributes["rnt_relationcode"] = new OptionSetValue(relationCode);
            return this.OrgService.Create(e);
        }

        public void deactivateConnection(Guid connectionId)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.setState("rnt_connection", connectionId, (int)GlobalEnums.StateCode.Passive, (int)ClassLibrary._Enums_1033.rnt_bonuscalculationlog_StatusCode.Inactive);
        }
    }
}
