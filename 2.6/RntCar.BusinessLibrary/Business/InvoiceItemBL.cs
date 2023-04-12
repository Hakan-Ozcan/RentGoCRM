using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class InvoiceItemBL : BusinessHandler
    {
        public InvoiceItemBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public InvoiceItemBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public InvoiceItemBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public InvoiceItemBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public Guid createInvoiceItem(Guid invoiceId,
                                      Guid? contractItemId,
                                      Guid? reservationItemId,
                                      Guid currencyId,
                                      string itemName,
                                      decimal itemAmount)
        {
            Guid baseItemId = contractItemId.HasValue ? contractItemId.Value : reservationItemId.Value;
            Entity entity = new Entity("rnt_invoiceitem");
            entity["rnt_invoiceid"] = new EntityReference("rnt_invoice", invoiceId);

            if (contractItemId.HasValue)
                entity["rnt_contractitemid"] = new EntityReference("rnt_contractitem", contractItemId.Value);
            if (reservationItemId.HasValue)
                entity["rnt_reservationitemid"] = new EntityReference("rnt_reservationitem", reservationItemId.Value);

            entity["transactioncurrencyid"] = new EntityReference("transactioncurrency", currencyId);

            entity["rnt_name"] = itemName;
            entity["rnt_totalamount"] = new Money(itemAmount);
            entity.Id = baseItemId;
            return this.createEntity(entity);
        }

        public void deactiveInvoiceItem(Guid invoiceItemId, int statusCode)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.setState("rnt_invoiceitem", invoiceItemId, (int)GlobalEnums.StateCode.Passive, statusCode);
        }

        public void updateInvoiceItemStatus(Guid invoiceItemId, int stateCode, int statusCode)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.setState("rnt_invoiceitem", invoiceItemId, stateCode, statusCode);
        }

        internal void updateInvoiceItemInvoiceDate(Guid invoiceItemId, DateTime invoiceDate)
        {
            Entity entity = new Entity("rnt_invoiceitem");
            entity.Id = invoiceItemId;
            entity.Attributes["rnt_invoicedate"] = invoiceDate.AddMinutes(StaticHelper.offset);
            this.OrgService.Update(entity);
        }
    }
}
