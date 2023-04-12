using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary;
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
    public class CheckAndUpdateDailyPrices: CodeActivity
    {
        [Input("Contract Pricing Group Code")]
        [ReferenceTarget("rnt_groupcodeinformations")]
        [RequiredArgument]
        public InArgument<EntityReference> _pricingGroupCodeRef { get; set; }

        [Input("Contract Campaign")]
        [ReferenceTarget("rnt_campaign")]
        [RequiredArgument]
        public InArgument<EntityReference> _campaignRef { get; set; }

        [Input("Tracking Number")]
        [RequiredArgument]
        public InArgument<string> _trackingNumber { get; set; }

        [Input("Total Duration")]
        [RequiredArgument]
        public InArgument<int> _totalDuration { get; set; }

        [Input("Total Amount")]
        [RequiredArgument]
        public InArgument<Money> _totalAmount { get; set; }

        
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            var pricingGroupCodeRef = _pricingGroupCodeRef.Get<EntityReference>(context);
            var campaignRef = _campaignRef.Get<EntityReference>(context);
            var trackingNumber = _trackingNumber.Get<string>(context);
            var totalAmount = _totalAmount.Get<Money>(context);
            var totalDuration = _totalDuration.Get<int>(context);
            Guid contractItemId = initializer.PrimaryId;

            PriceCalculationSummariesBL priceCalculationSummariesBL = new PriceCalculationSummariesBL(initializer.Service);
           

        }
    }
}
