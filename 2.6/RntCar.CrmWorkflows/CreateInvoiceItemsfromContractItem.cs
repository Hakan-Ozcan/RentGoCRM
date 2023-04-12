using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Linq;

namespace RntCar.CrmWorkflows
{
    public class CreateInvoiceItemsfromContractItem : CodeActivity
    {
        [Input("Contract Reference")]
        [ReferenceTarget("rnt_contract")]
        public InArgument<EntityReference> _contract { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            var contractRef = _contract.Get<EntityReference>(context);
            if (contractRef != null)
            {
                ContractItemRepository contractItemRepository = new ContractItemRepository(initializer.Service);
                var items = contractItemRepository.getCompletedContractItemsByContractId(contractRef.Id);

                ContractRepository contractRepository = new ContractRepository(initializer.Service);
                var contract = contractRepository.getContractById(contractRef.Id, new string[] { "rnt_contracttypecode", "rnt_paymentmethodcode", "rnt_corporateid", "transactioncurrencyid", "rnt_customerid" });
                var paymentMethodCode = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
                var contractType = contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value;

                XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                initializer.TraceMe("items found  : " + items.Count);
                initializer.TraceMe("paymentMethodCode  : " + paymentMethodCode);
                foreach (var item in items)
                {
                    InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(initializer.Service);

                    var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemId(item.Id);
                    initializer.TraceMe("Invoice Item: " + JsonConvert.SerializeObject(invoiceItem));

                    if (invoiceItem == null)
                    {
                        Entity createInvoiceItem = xrmHelper.initializeFromRequest("rnt_contractitem", item.Id, "rnt_invoiceitem");

                        Entity invoice = null;
                        rnt_BillingTypeCode invoiceBillingType = rnt_BillingTypeCode.Individual;
                        if (paymentMethodCode == (int)rnt_PaymentMethodCode.LimitedCredit)
                        {
                            if (item.GetAttributeValue<OptionSetValue>("rnt_billingtype").Value == (int)rnt_BillingTypeCode.Corporate)
                            {
                                invoiceBillingType = rnt_BillingTypeCode.Corporate;
                            }
                        }
                        else if (paymentMethodCode == (int)rnt_PaymentMethodCode.PayBroker)
                        {
                            if (item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.PriceDifference &&
                                    paymentMethodCode == (int)rnt_PaymentMethodCode.PayBroker)
                            {
                                invoiceBillingType = rnt_BillingTypeCode.Corporate;
                            }
                        }
                        else if (item.GetAttributeValue<OptionSetValue>("rnt_billingtype").Value == (int)rnt_BillingTypeCode.Corporate)
                        {
                            invoiceBillingType = rnt_BillingTypeCode.Corporate;
                        }

                        if (invoiceBillingType == rnt_BillingTypeCode.Individual)
                        {
                            invoice = individualOperations(initializer.Service, contract);
                        }
                        else
                        {
                            invoice = corporateOperations(initializer.Service, contract);
                        }
                        initializer.TraceMe("invoice: " + JsonConvert.SerializeObject(invoice));
                        if (invoice != null)
                        {
                            createInvoiceItem["rnt_invoiceid"] = new EntityReference(invoice.LogicalName, invoice.Id);
                            createInvoiceItem["rnt_name"] = item.GetAttributeValue<string>("rnt_name");
                            //createInvoiceItem["rnt_invoicedate"] = item.Attributes.Contains("rnt_invoicedate") ? item.GetAttributeValue<DateTime>("rnt_invoicedate").Date : DateTime.UtcNow.AddMinutes(StaticHelper.offset);
                            createInvoiceItem["rnt_netamount"] = item.GetAttributeValue<Money>("rnt_netamount").Value;
                            initializer.Service.Create(createInvoiceItem);
                        }
                    }
                }
            }
        }
        private Entity corporateOperations(IOrganizationService Service, Entity contract)
        {

            InvoiceRepository invoiceRepository = new InvoiceRepository(Service);
            var invoice = invoiceRepository.getCorporateNotIntegratedInvoiceByContractId(contract.Id);
            if (invoice == null)
            {
                CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(Service);
                var account = corporateCustomerRepository.getCorporateCustomerById(contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id);
                var invoiceData = InvoiceHelper.buildInvoiceAddressDataFromAccountEntity(account);
                InvoiceBL invoiceBL = new InvoiceBL(Service);
                var _id = invoiceBL.createInvoice(invoiceData,
                null,
                contract.Id,
                contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);

                Entity e = new Entity("rnt_invoice");
                e["rnt_defaultinvoice"] = true;
                e.Id = _id;
                Service.Update(e);
                invoice = new InvoiceRepository(Service).getInvoiceById(_id);
            }
            return invoice;
        }

        private Entity individualOperations(IOrganizationService Service, Entity contract)
        {

            InvoiceRepository invoiceRepository = new InvoiceRepository(Service);
            var invoice = invoiceRepository.getFirstAvailableInvoiceByContractId(contract.Id).FirstOrDefault();
            if (invoice == null)
            {
                InvoiceBL invoiceBL = new InvoiceBL(Service);
                InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(Service);
                var invoiceAddress = invoiceAddressRepository.getInvoiceAddressByCustomerIdByGivenColumns(contract.GetAttributeValue<EntityReference>("rnt_customerid").Id);
                if (invoiceAddress.Count > 0)
                {
                    var invoiceData = InvoiceHelper.buildInvoiceAddressDataFromInvoiceAddressEntity(invoiceAddress[0]);
                    var _id = invoiceBL.createInvoice(invoiceData,
                    null,
                    contract.Id,
                    contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);

                    Entity e = new Entity("rnt_invoice");
                    e["rnt_defaultinvoice"] = true;
                    e.Id = _id;
                    Service.Update(e);
                    invoice = new InvoiceRepository(Service).getInvoiceById(_id);
                }

            }
            else
            {
                invoice.GetAttributeValue<string>("");
            }
            return invoice;
        }
    }
}
