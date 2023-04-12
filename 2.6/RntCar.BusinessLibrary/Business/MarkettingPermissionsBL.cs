using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MarkettingPermission;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class MarkettingPermissionsBL : BusinessHandler
    {
        public MarkettingPermissionsBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public MarkettingPermissionsBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public MarkettingPermissionsBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public Guid createMarkettingPermissions(MarketingPermission markettingPermission, Entity contact = null)
        {
            var markettingPermissionsEntity = this.buildMarkettingPermissions(markettingPermission, contact);
            return this.createEntity(markettingPermissionsEntity);
        }
        public void updateMarkettingPermissions(MarketingPermission markettingPermission)
        {
            var markettingPermissionsEntity = this.buildMarkettingPermissions(markettingPermission);
            this.updateEntity(markettingPermissionsEntity);
        }
        public void deactiveMarkettingPermissions(Guid markettingPermissionsId)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.setState("rnt_marketingpermissions", markettingPermissionsId, (int)GlobalEnums.StateCode.Passive, (int)ClassLibrary._Enums_1033.rnt_marketingpermissions_StatusCode.Inactive);
        }
        public Entity buildMarkettingPermissions(MarketingPermission markettingPermission, Entity contact = null)
        {
            Entity entity = new Entity("rnt_marketingpermissions");
            if(markettingPermission.contactId.HasValue && markettingPermission.contactId != Guid.Empty)
                entity["rnt_contactid"] = new EntityReference("contact", markettingPermission.contactId.Value);

            if (markettingPermission.marketingPermissionId.HasValue)
                entity.Id = markettingPermission.marketingPermissionId.Value;

            if (contact != null)
                entity["rnt_name"] = contact.GetAttributeValue<string>("fullname") + " - İzinleri";

            entity["rnt_operationtype"] = markettingPermission.operationType.HasValue ? new OptionSetValue(markettingPermission.operationType.Value) : null;
            entity["rnt_permissionchannelcode"] = markettingPermission.channelCode.HasValue ? new OptionSetValue(markettingPermission.channelCode.Value) : null;
            entity["rnt_etkpermission"] = markettingPermission.etkPermission.HasValue ? markettingPermission.etkPermission.Value : false;
            entity["rnt_kvkkpermission"] = markettingPermission.kvkkPermission.HasValue ? markettingPermission.kvkkPermission.Value : false;
            entity["rnt_allowemails"] = markettingPermission.emailPermission.HasValue ? markettingPermission.emailPermission.Value : false;
            entity["rnt_allownotification"] = markettingPermission.notificationPermission.HasValue ? markettingPermission.notificationPermission.Value : false;
            entity["rnt_allowsms"] = markettingPermission.smsPermission.HasValue ? markettingPermission.smsPermission.Value : false;
            entity["rnt_allowcall"] = markettingPermission.callPermission.HasValue ? markettingPermission.callPermission.Value : false;

            return entity;
        }

    }
}
