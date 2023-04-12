using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmWorkflows
{
    public class DeleteInvoice : CodeActivity
    {
        [Input("Invoice")]
        [ReferenceTarget("rnt_invoice")]
        public InArgument<EntityReference> _invoice { get; set; }
        protected override void Execute(CodeActivityContext context) 
        {
            PluginInitializer initializer = new PluginInitializer(context);
            var invoice = _invoice.Get<EntityReference>(context);

            initializer.Service.Delete(invoice.LogicalName, invoice.Id);
        }
    }
}
