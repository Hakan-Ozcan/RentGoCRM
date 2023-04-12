using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.CrmWorkflows
{
    public class UpdateHGSTransitRecord : CodeActivity
    {
        [Input("Contract Detail")]
        [ReferenceTarget("rnt_contractitem")]
        public InArgument<EntityReference> _contractitem { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            var contractItemRef = _contractitem.Get<EntityReference>(context);
            var primaryId = initializer.PrimaryId;
            initializer.TraceMe("process start!");

            try
            {
                QueryExpression getHGSTransitList = new QueryExpression("rnt_hgstransitlist");
                getHGSTransitList.ColumnSet = new ColumnSet("statuscode", "rnt_invoiceitemid");
                getHGSTransitList.Criteria.AddCondition("statuscode", ConditionOperator.NotEqual, (int)HGSTransitListStatusCode.Invoiced);
                getHGSTransitList.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemRef.Id);
                EntityCollection hgsTransitList = initializer.Service.RetrieveMultiple(getHGSTransitList);
                foreach (var hgsTransit in hgsTransitList.Entities)
                {
                    hgsTransit.Attributes["statuscode"] = new OptionSetValue((int)HGSTransitListStatusCode.Invoiced);
                    hgsTransit.Attributes["rnt_invoiceitemid"] = new EntityReference("rnt_invoiceitem", primaryId);
                    initializer.Service.Update(hgsTransit);
                }


            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
