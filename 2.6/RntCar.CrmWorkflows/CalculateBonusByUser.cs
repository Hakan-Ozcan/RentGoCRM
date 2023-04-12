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
    public class CalculateBonusByUser : CodeActivity
    {
        [Input("Contract Reference")]
        [ReferenceTarget("rnt_contract")]
        public InArgument<EntityReference> _contract { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            var contractRef = _contract.Get<EntityReference>(context);
            try
            {
                BonusCalculationLogBL bonusCalculationLogBL = new BonusCalculationLogBL(initializer.Service, initializer.TracingService);
                bonusCalculationLogBL.calculateBonusByUser(contractRef);
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
