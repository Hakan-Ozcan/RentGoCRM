using Microsoft.Crm.Sdk.Messages;
using RntCar.BusinessLibrary.Repository;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.SendInvoicestoLogo
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            LoggerHelper loggerHelper = new LoggerHelper();
            InvoiceRepository invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
            var results = invoiceRepository.getErrorInvoices();
            var resultsDraft = invoiceRepository.getDraftInvoicesCompletedContracts();
            results.AddRange(resultsDraft);
            results = results.DistinctBy(p => p.Id).ToList();

            foreach (var item in results)
            {
                loggerHelper.traceInfo("item id " + item);
                try
                {
                    ExecuteWorkflowRequest executeWorkflowRequest = new ExecuteWorkflowRequest
                    {
                        WorkflowId = StaticHelper.createInvoiceWithLogoWorkflowId,
                        EntityId = item.Id
                    };
                    crmServiceHelper.IOrganizationService.Execute(executeWorkflowRequest);
                }
                catch (Exception ex)
                {
                    loggerHelper.traceInfo("ex " + ex.Message);
                    continue;
                }

            }
        }
    }
}
