using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections.Generic;

namespace RntCar.CrmWorkflows
{
    public class ManualPayment_Invoice : CodeActivity
    {
        
        protected override void Execute(CodeActivityContext context)
        {
           
            try
            {
                PluginInitializer initializer = new PluginInitializer(context);
                var contractId = initializer.WorkflowContext.PrimaryEntityId;

                initializer.TraceMe("started : " + contractId);

                ContractRepository contractRepository = new ContractRepository(initializer.Service);
                var c = contractRepository.getContractById(contractId);
          
                initializer.TraceMe(c.Id.ToString());
                initializer.TraceMe(c.GetAttributeValue<string>("rnt_invoiceid_manualpayment"));
                initializer.TraceMe(c.GetAttributeValue<string>("rnt_contractitems_manualpayment"));

                InvoiceRepository invoiceRepository = new InvoiceRepository(initializer.Service);
                var invoice = invoiceRepository.getInvoiceById(new Guid(c.GetAttributeValue<string>("rnt_invoiceid_manualpayment")));

                List<Guid> items = JsonConvert.DeserializeObject<List<Guid>>(c.GetAttributeValue<string>("rnt_contractitems_manualpayment"));              

                List<Entity> contractItemsLogo = new List<Entity>();

                foreach (var item in items)
                {
                    ContractItemRepository contractItemRepository = new ContractItemRepository(initializer.Service);
                    var contractItem = contractItemRepository.getContractItemId(item);
                    contractItemsLogo.Add(contractItem);
                }              
               
                InvoiceHelper invoiceHelper = new InvoiceHelper(initializer.Service, initializer.TracingService);
                invoiceHelper.sendInvoicetoLogo(invoice, contractItemsLogo, contractId, c.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id);

                initializer.TraceMe("end");

            }
            catch (Exception)
            {

                throw;
            }
          
        }
    }
}
