using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Activities;

namespace RntCar.CrmWorkflows
{
    public class CreateInvoiceWithLogo : CodeActivity
    {
        [Input("Contract Reference")]
        [ReferenceTarget("rnt_contract")]
        public InArgument<EntityReference> _contract { get; set; }

        [Input("Reservation Reference")]
        [ReferenceTarget("rnt_reservation")]
        public InArgument<EntityReference> _reservation { get; set; }

        [Input("LangId")]
        public InArgument<int> _langId { get; set; }

        [Output("ExecutionResult")]
        public OutArgument<string> ExecutionResult { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            initializer.TraceMe("process start");
            var contractRef = _contract.Get<EntityReference>(context);
            var reservationRef = _reservation.Get<EntityReference>(context);
            var langId = _langId.Get<int>(context);
            initializer.TraceMe("getting parameters");
            try
            {
                InvoiceBL invoiceBL = new InvoiceBL(initializer.Service, initializer.TracingService);

                if (contractRef != null)
                {
                    var contractResponse = invoiceBL.handleCreateInvoiceWithLogoProcessForContract(contractRef.Id, langId, null);
                    initializer.TraceMe(JsonConvert.SerializeObject(contractResponse));
                    ExecutionResult.Set(context, JsonConvert.SerializeObject(contractResponse));
                }
                else if (reservationRef != null)
                {
                    var reservationResponse = invoiceBL.handleCreateInvoiceWithLogoProcessForReservation(reservationRef.Id, langId);
                    ExecutionResult.Set(context, JsonConvert.SerializeObject(reservationResponse));
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
