using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmWorkflows
{
    public class SendDigitalSignatureSMS : CodeActivity
    {
		[Input("Citizenship")]
		[ReferenceTarget("rnt_country")]
		public InArgument<EntityReference> _citizenshipId { get; set; }
		protected override void Execute(CodeActivityContext context)
        {
			PluginInitializer initializer = new PluginInitializer(context);
			try
			{
				SMSContentBL smsContentBl = new SMSContentBL(initializer, initializer.Service, initializer.TracingService);
				//var message = smsContentBl.getSMSContentByCodeandLangId(smsContentCode, langId, mobilePhone, pnrNumber, verificationCode, firstName, lastName);
			}
			catch (Exception ex)
			{
				initializer.TraceMe("SendDigitalSignatureSMS error : " + ex.Message);
			}
        }
    }
}
