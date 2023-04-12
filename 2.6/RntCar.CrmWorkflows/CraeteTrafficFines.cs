using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Business;
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
    public class CraeteTrafficFines : CodeActivity
    {

        [Input("Traffic Fine Reference")]
        [ReferenceTarget("rnt_trafficfine")]
        public InArgument<EntityReference> _trafficfine { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            var _trafficfineRef = _trafficfine.Get<EntityReference>(context);
            initializer.TraceMe("process start!");

            try
            {
                TrafficFineRepository trafficFineRepository = new TrafficFineRepository(initializer.Service);
                var trafficFine = trafficFineRepository.getTrafficFineById(_trafficfineRef.Id);
                initializer.TraceMe("trafficFine id: " + trafficFine.Id);

                var chargeTrafficfines = trafficFine.Attributes.Contains("rnt_chargetrafficfines") ? trafficFine.GetAttributeValue<Boolean>("rnt_chargetrafficfines") : false;
                initializer.TraceMe("rnt_chargetrafficfines" + chargeTrafficfines);
                if(chargeTrafficfines)
                {
                    TrafficBL trafficBL = new TrafficBL(initializer.Service, initializer.TracingService);
                    trafficBL.processTrafficFineWF(trafficFine);
                }
                initializer.TraceMe("Process end!");
            }
            catch (Exception ex)
            {
                initializer.TraceMe("exception" + ex);//initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
