using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmWorkflows
{
    public class CheckMarketingPermission : CodeActivity
    {
        [Input("Contact Reference")]
        [ReferenceTarget("contact")]
        public InArgument<EntityReference> _contact { get; set; }

        [Output("SMS Permission")]
        public OutArgument<bool> SMSPermission { get; set; }
        [Output("Notification Permission")]
        public OutArgument<bool> NotificationPermission { get; set; }
        [Output("EMail Permission")]
        public OutArgument<bool> EMailPermission { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            initializer.TraceMe("process start");
            var contactRef = _contact.Get<EntityReference>(context);
            initializer.TraceMe("customer id" + contactRef.Id);

            bool smsPermission = false, emailPermission = false, notificationPermission = false;
            try
            {
                MarkettingPermissionsRepository markettingPermissionsRepository = new MarkettingPermissionsRepository(initializer.Service);
                var marketingPermissions = markettingPermissionsRepository.getMarkettingPermissionByContactId(contactRef.Id);

                if (marketingPermissions != null)
                {
                    emailPermission = marketingPermissions.Attributes.Contains("rnt_allowemails") ? marketingPermissions.GetAttributeValue<bool>("rnt_allowemails") : false;
                    notificationPermission = marketingPermissions.Attributes.Contains("rnt_allownotification") ? marketingPermissions.GetAttributeValue<bool>("rnt_allownotification") : false;
                    smsPermission = marketingPermissions.Attributes.Contains("rnt_allowsms") ? marketingPermissions.GetAttributeValue<bool>("rnt_allowsms") : false;
                }

                SMSPermission.Set(context, smsPermission);
                EMailPermission.Set(context, emailPermission);
                NotificationPermission.Set(context, notificationPermission);
            }
            catch (Exception ex)
            {
                initializer.TraceMe("exception" + ex.Message);
            }
        }
    }
}
