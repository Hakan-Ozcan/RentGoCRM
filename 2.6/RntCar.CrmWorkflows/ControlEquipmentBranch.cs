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

namespace RntCar.CrmWorkflows
{
    public class ControlEquipmentBranch : CodeActivity
    {
        [Input("Equipment")]
        [ReferenceTarget("rnt_equipment")]
        public InArgument<EntityReference> _equipment { get; set; }

        [Input("Contract")]
        [ReferenceTarget("rnt_contract")]
        public InArgument<EntityReference> _contract { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            initializer.TraceMe("process start");
            var equipmentRef = _equipment.Get<EntityReference>(context);
            var contractRef = _contract.Get<EntityReference>(context);

            try
            {
                Entity equipment = initializer.Service.Retrieve(equipmentRef.LogicalName, equipmentRef.Id, new ColumnSet("rnt_currentbranchid"));
                Entity contact = initializer.Service.Retrieve(contractRef.LogicalName, contractRef.Id, new ColumnSet("rnt_pickupbranchid"));

                EntityReference currentBrachRef = equipment.GetAttributeValue<EntityReference>("rnt_currentbranchid");
                EntityReference pickupBranchRef = contact.GetAttributeValue<EntityReference>("rnt_pickupbranchid");


                initializer.TraceMe("equipmentRef Name" + equipmentRef.Name);
                initializer.TraceMe("currentBrachRef Name" + currentBrachRef.Name);
                initializer.TraceMe("pickupBranchRef Name" + pickupBranchRef.Name);
                if (currentBrachRef.Id != pickupBranchRef.Id)
                {
                    Entity updateEquipment = new Entity(equipment.LogicalName, equipment.Id);
                    updateEquipment.Attributes["rnt_currentbranchid"] = new EntityReference(pickupBranchRef.LogicalName, pickupBranchRef.Id);
                    initializer.Service.Update(updateEquipment);
                }
            }
            catch (Exception ex)
            {
                initializer.TraceMe("exception" + ex.Message);
            }
        }
    }
}
