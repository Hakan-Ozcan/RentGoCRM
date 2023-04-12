using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.Enums;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmWorkflows
{
    public class CreateInvoiceForIyziLink : CodeActivity
    {
        [Input("Contract Reference")]
        [ReferenceTarget("rnt_contract")]
        public InArgument<EntityReference> _contract { get; set; }

        [Input("Invoice Reference")]
        [ReferenceTarget("rnt_invoice")]
        public InArgument<EntityReference> _invoice { get; set; }

        [Input("LangId")]
        public InArgument<int> _langId { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            initializer.TraceMe("process start");
            var contractRef = _contract.Get<EntityReference>(context);
            var invoiceRef = _invoice.Get<EntityReference>(context);
            initializer.TraceMe("getting parameters");
            try
            {
                InvoiceBL invoiceBL = new InvoiceBL(initializer.Service);
                QueryExpression getLastInvoiceQuery = new QueryExpression(invoiceRef.LogicalName);
                getLastInvoiceQuery.ColumnSet = new ColumnSet(true);
                getLastInvoiceQuery.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractRef.Id);
                getLastInvoiceQuery.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)InvoiceEnums.ActiveStatus.IntegratedWithLogo);
                getLastInvoiceQuery.Criteria.AddCondition("rnt_invoicetypecode", ConditionOperator.Equal, (int)rnt_invoice_rnt_invoicetypecode.Individual);
                Entity invoiceTemplate = initializer.Service.RetrieveMultiple(getLastInvoiceQuery).Entities.FirstOrDefault();

                if (invoiceTemplate != null)
                {
                    Entity updateInvoice = new Entity(invoiceRef.LogicalName, invoiceRef.Id);
                    updateInvoice.Attributes["rnt_cityid"] = invoiceTemplate.Contains("rnt_cityid") ? invoiceTemplate.GetAttributeValue<EntityReference>("rnt_cityid") : null;
                    updateInvoice.Attributes["rnt_districtid"] = invoiceTemplate.Contains("rnt_districtid") ? invoiceTemplate.GetAttributeValue<EntityReference>("rnt_districtid") : null;
                    updateInvoice.Attributes["rnt_countryid"] = invoiceTemplate.GetAttributeValue<EntityReference>("rnt_countryid");
                    updateInvoice.Attributes["rnt_addressdetail"] = invoiceTemplate.GetAttributeValue<string>("rnt_addressdetail");
                    updateInvoice.Attributes["rnt_email"] = invoiceTemplate.GetAttributeValue<string>("rnt_email");
                    updateInvoice.Attributes["rnt_companyname"] = invoiceTemplate.GetAttributeValue<string>("rnt_companyname");
                    updateInvoice.Attributes["rnt_firstname"] = invoiceTemplate.GetAttributeValue<string>("rnt_firstname");
                    updateInvoice.Attributes["rnt_lastname"] = invoiceTemplate.GetAttributeValue<string>("rnt_lastname");
                    updateInvoice.Attributes["rnt_govermentid"] = invoiceTemplate.GetAttributeValue<string>("rnt_govermentid");
                    updateInvoice.Attributes["rnt_mobilephone"] = invoiceTemplate.GetAttributeValue<string>("rnt_mobilephone");
                    updateInvoice.Attributes["rnt_invoicetypecode"] = invoiceTemplate.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode");
                    updateInvoice.Attributes["rnt_taxnumber"] = invoiceTemplate.GetAttributeValue<string>("rnt_taxnumber");
                    updateInvoice.Attributes["rnt_taxofficeid"] = invoiceTemplate.Contains("rnt_taxofficeid") ? invoiceTemplate.GetAttributeValue<EntityReference>("rnt_taxofficeid") : null;
                    initializer.Service.Update(updateInvoice);
                    invoiceBL.handleCreateInvoiceWithLogoProcessForContract(contractRef.Id, 1055, invoiceRef.Id);
                }
                else
                {
                    throw new Exception("Sözleşmeye ait ilk defa Bireysel Fatura kesilmektedir. Address Bilgilerini doldurunuz");
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
