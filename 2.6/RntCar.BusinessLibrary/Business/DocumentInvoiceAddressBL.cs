using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class DocumentInvoiceAddressBL : BusinessHandler
    {
        public DocumentInvoiceAddressBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public DocumentInvoiceAddressBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public void createDocumentInvoiceAddress(InvoiceAddressData invoiceAddressData,
                                                 EntityReference reservationItem,
                                                 EntityReference contractItem,
                                                 decimal itemAmount)
        {
            var entity = this.buildDocumentInvoiceAddress(invoiceAddressData, reservationItem, contractItem, itemAmount);
            this.OrgService.Create(entity);
        }
        public void updateDocumentInvoiceAddress(InvoiceAddressData invoiceAddressData,
                                                 EntityReference reservationItem,
                                                 EntityReference contractItem,
                                                 decimal itemAmount)
        {
            var entity = this.buildDocumentInvoiceAddress(invoiceAddressData, reservationItem, contractItem, itemAmount);
            this.OrgService.Update(entity);
        }

        private Entity buildDocumentInvoiceAddress(InvoiceAddressData invoiceAddressData,
                                                  EntityReference reservationItem,
                                                  EntityReference contractItem,
                                                  decimal itemAmount)
        {
            Entity e = new Entity("rnt_documentinvoiceaddress");
            e["rnt_invoicetypecode"] = new OptionSetValue(invoiceAddressData.invoiceType);
            e["rnt_countryid"] = new EntityReference("rnt_country", invoiceAddressData.addressCountryId);
            //setting reservation item id as uniqueidenfier for also invoice addressid
            var id = Guid.Empty;

            if (reservationItem != null)
            {
                e["rnt_reservationitemid"] = reservationItem;
                id = reservationItem.Id;
            }
            if (contractItem != null)
            {
                e["rnt_contractitemid"] = contractItem;
                id = contractItem.Id;
            }

            if (invoiceAddressData.addressCityId.HasValue && invoiceAddressData.addressCityId.Value != Guid.Empty)
                e["rnt_cityid"] = new EntityReference("rnt_city", invoiceAddressData.addressCityId.Value);

            if (invoiceAddressData.addressDistrictId.HasValue && invoiceAddressData.addressDistrictId.Value != Guid.Empty)
                e["rnt_districtid"] = new EntityReference("rnt_district", invoiceAddressData.addressDistrictId.Value);


            if (invoiceAddressData.taxOfficeId.HasValue && invoiceAddressData.taxOfficeId.Value != Guid.Empty)
                e["rnt_taxofficeid"] = new EntityReference("rnt_taxoffice", invoiceAddressData.taxOfficeId.Value);
            else
                e["rnt_taxofficeid"] = null;

            //individual
            //todo enums
            if(invoiceAddressData.invoiceType == 10)
            {
                e["rnt_firstname"] = invoiceAddressData.firstName;
                e["rnt_lastname"] = invoiceAddressData.lastName;
                e["rnt_govermentid"] = invoiceAddressData.governmentId;
            }
            else
            {
                e["rnt_firstname"] = null;
                e["rnt_lastname"] = null;
                e["rnt_govermentid"] = null;
            }
            //corporate
            //todo enums
            if (invoiceAddressData.invoiceType == 20)
            {
                e["rnt_companyname"] = invoiceAddressData.companyName;
                e["rnt_taxnumber"] = invoiceAddressData.taxNumber;
            }
            else
            {
                e["rnt_companyname"] = null;
                e["rnt_taxnumber"] = null;
            }
   
            e["rnt_addressdetail"] = invoiceAddressData.addressDetail;
            e["rnt_amount"] = new Money(itemAmount);
            e["rnt_invoiceaddressid"] = Convert.ToString(invoiceAddressData.invoiceAddressId.Value);
            e.Id = id;

            return e;
        }
    }
}
