using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System.Activities;

namespace RntCar.CrmWorkflows
{
    public class CreateInvoiceWithLogofromInvoiceEntity : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);

            var invoiceId = initializer.WorkflowContext.PrimaryEntityId;

            InvoiceRepository invoiceRepository = new InvoiceRepository(initializer.Service);
            var invoice = invoiceRepository.getInvoiceById(invoiceId);
            initializer.TraceMe("invoice.Id " + invoiceId);
            initializer.TraceMe(invoice == null ? "invoice is null" : "invoice is not null");
            var statusCode = invoice.GetAttributeValue<OptionSetValue>("statuscode");
            initializer.TraceMe("statusCode.Value : " + statusCode.Value);
            if (statusCode.Value == (int)RntCar.ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegratedWithLogo)
            {
                initializer.TraceMe("invoice is already send to logo");
                return;
            }
            initializer.TraceMe("invoice operation start");
            InvoiceBL invoiceBL = new InvoiceBL(initializer.Service, initializer.TracingService);
            var contract = invoice.GetAttributeValue<EntityReference>("rnt_contractid");

           if(contract != null)
            {
                initializer.TraceMe("contract.Id " + contract.Id);

                var contractResponse = invoiceBL.handleCreateInvoiceWithLogoProcessForContract(contract.Id, 1055, invoice.Id);
            }
            else
            {               
                var reservation = invoice.GetAttributeValue<EntityReference>("rnt_reservationid");
                initializer.TraceMe("reservation.Id " + reservation.Id);
                invoiceBL.handleCreateInvoiceWithLogoProcessForReservation(reservation.Id, 1055);
            }
            
        }
    }
}
