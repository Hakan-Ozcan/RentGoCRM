using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;

namespace RntCar.BusinessLibrary.Business
{
    public class InvoiceAddressBL : BusinessHandler
    {
        public InvoiceAddressBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }

        public InvoiceAddressBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public InvoiceAddressBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public InvoiceAddressBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public Guid createInvoiceAddress(InvoiceAddressCreateParameters invoiceAddressCreateParameters)
        {
            Entity invoiceAddress = new Entity("rnt_invoiceaddress");
            invoiceAddress["rnt_contactid"] = new EntityReference("contact", invoiceAddressCreateParameters.individualCustomerId);
            invoiceAddress["rnt_addressdetail"] = invoiceAddressCreateParameters.addressDetail;
            invoiceAddress["rnt_name"] = invoiceAddressCreateParameters.invoiceName;

            if (invoiceAddressCreateParameters.invoiceType == (int)rnt_invoiceaddress_rnt_invoicetypecode.Individual)
            {
                invoiceAddress["rnt_firstname"] = invoiceAddressCreateParameters.firstName;
                invoiceAddress["rnt_lastname"] = invoiceAddressCreateParameters.lastName;
                invoiceAddress["rnt_government"] = CommonHelper.buildCharacters(invoiceAddressCreateParameters.governmentId, 11);
            }
            else if (invoiceAddressCreateParameters.invoiceType == (int)rnt_invoiceaddress_rnt_invoicetypecode.Corporate)
            {
                invoiceAddress["rnt_companyname"] = invoiceAddressCreateParameters.companyName;
                invoiceAddress["rnt_taxnumber"] = CommonHelper.buildCharacters(invoiceAddressCreateParameters.taxNumber, 10);
                if (invoiceAddressCreateParameters.taxOfficeId != null && invoiceAddressCreateParameters.taxOfficeId.HasValue)
                    invoiceAddress["rnt_taxofficeid"] = new EntityReference("rnt_taxoffice", invoiceAddressCreateParameters.taxOfficeId.Value);
            }            
            invoiceAddress["rnt_countryid"] = new EntityReference("rnt_country", invoiceAddressCreateParameters.addressCountryId);
            invoiceAddress["rnt_invoicetypecode"] = new OptionSetValue(invoiceAddressCreateParameters.invoiceType);
        
            if (invoiceAddressCreateParameters.addressCityId.HasValue)
                invoiceAddress["rnt_cityid"] = new EntityReference("rnt_city", invoiceAddressCreateParameters.addressCityId.Value);
            if (invoiceAddressCreateParameters.addressDistrictId != null && invoiceAddressCreateParameters.addressDistrictId.HasValue)
                invoiceAddress["rnt_districtid"] = new EntityReference("rnt_district", invoiceAddressCreateParameters.addressDistrictId.Value);

            return this.OrgService.Create(invoiceAddress);
        }

        public void updateInvoiceAddress(InvoiceAddressCreateParameters invoiceAddressCreateParameters)
        {
            Entity invoiceAddress = new Entity("rnt_invoiceaddress");
            invoiceAddress["rnt_contactid"] = new EntityReference("contact", invoiceAddressCreateParameters.individualCustomerId);
            invoiceAddress["rnt_addressdetail"] = invoiceAddressCreateParameters.addressDetail;
            if (invoiceAddressCreateParameters.invoiceType == (int)rnt_invoiceaddress_rnt_invoicetypecode.Individual)
            {
                var formattedGovernment = CommonHelper.buildCharacters(invoiceAddressCreateParameters.governmentId, 11);
                invoiceAddress["rnt_firstname"] = invoiceAddressCreateParameters.firstName;
                invoiceAddress["rnt_lastname"] = invoiceAddressCreateParameters.lastName;
                invoiceAddress["rnt_government"] = formattedGovernment;
                //null corporate related fields
                invoiceAddress["rnt_companyname"] = null;
                invoiceAddress["rnt_taxnumber"] = null;
                invoiceAddress["rnt_taxofficeid"] = null;
            }
            else if (invoiceAddressCreateParameters.invoiceType == (int)rnt_invoiceaddress_rnt_invoicetypecode.Corporate)
            {
                invoiceAddress["rnt_companyname"] = invoiceAddressCreateParameters.companyName;
                invoiceAddress["rnt_taxnumber"] = CommonHelper.buildCharacters(invoiceAddressCreateParameters.taxNumber, 10);
                if (invoiceAddressCreateParameters.taxOfficeId != null && invoiceAddressCreateParameters.taxOfficeId.HasValue)
                    invoiceAddress["rnt_taxofficeid"] = new EntityReference("rnt_taxoffice", invoiceAddressCreateParameters.taxOfficeId.Value);

                //null individual related
                invoiceAddress["rnt_firstname"] = null;
                invoiceAddress["rnt_lastname"] = null;
                invoiceAddress["rnt_government"] = null;
            }
            invoiceAddress["rnt_name"] = invoiceAddressCreateParameters.invoiceName;
            invoiceAddress["rnt_countryid"] = new EntityReference("rnt_country", invoiceAddressCreateParameters.addressCountryId);
            invoiceAddress["rnt_invoicetypecode"] = new OptionSetValue(invoiceAddressCreateParameters.invoiceType);
            if (invoiceAddressCreateParameters.taxOfficeId != null && invoiceAddressCreateParameters.taxOfficeId.HasValue)
                invoiceAddress["rnt_taxofficeid"] = new EntityReference("rnt_taxoffice", invoiceAddressCreateParameters.taxOfficeId.Value);
            if (invoiceAddressCreateParameters.addressCityId.HasValue)
                invoiceAddress["rnt_cityid"] = new EntityReference("rnt_city", invoiceAddressCreateParameters.addressCityId.Value);
            if (invoiceAddressCreateParameters.addressDistrictId != null && invoiceAddressCreateParameters.addressDistrictId.HasValue)
                invoiceAddress["rnt_districtid"] = new EntityReference("rnt_district", invoiceAddressCreateParameters.addressDistrictId.Value);
            invoiceAddress.Id = invoiceAddressCreateParameters.invoiceAddressId.Value;

            this.OrgService.Update(invoiceAddress);
        }

        public InvoceAddressDeleteResponse deleteInvoiceAddress(Guid invoiceAddressId)
        {
            this.OrgService.Delete("rnt_invoiceaddress", invoiceAddressId);
            return new InvoceAddressDeleteResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public List<InvoiceAddressData> getCustomerInvoiceAddresses(string individualCustomerId)
        {
            InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(this.OrgService);
            string[] columns = new string[] { "rnt_countryid",
                                              "rnt_name",
                                              "rnt_cityid",
                                              "rnt_districtid",
                                              "rnt_addressdetail" ,
                                              "rnt_firstname" ,
                                              "rnt_lastname",
                                              "rnt_companyname",
                                              "rnt_government",
                                              "rnt_taxnumber",
                                              "rnt_taxofficeid",
                                              "rnt_invoicetypecode"};
            var response = invoiceAddressRepository.getInvoiceAddressByCustomerIdByGivenColumns(new Guid(individualCustomerId), columns);

            List<InvoiceAddressData> invoiceAddressDatas = new List<InvoiceAddressData>();
            foreach (var item in response)
            {
                InvoiceAddressData invoiceAddressData = new InvoiceAddressData
                {
                    invoiceAddressId = item.Id,
                    name = item.Attributes.Contains("rnt_name") ? item.GetAttributeValue<string>("rnt_name") : string.Empty,
                    addressCountryId = item.Attributes.Contains("rnt_countryid") ? item.GetAttributeValue<EntityReference>("rnt_countryid").Id : Guid.Empty,
                    addressCountryName = item.Attributes.Contains("rnt_countryid") ? item.GetAttributeValue<EntityReference>("rnt_countryid").Name : string.Empty,
                    addressCityId = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Id : Guid.Empty,
                    addressCityName = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Name : string.Empty,
                    addressDistrictId = item.Attributes.Contains("rnt_districtid") ? item.GetAttributeValue<EntityReference>("rnt_districtid").Id : Guid.Empty,
                    addressDistrictName = item.Attributes.Contains("rnt_districtid") ? item.GetAttributeValue<EntityReference>("rnt_districtid").Name : string.Empty,
                    addressDetail = item.Attributes.Contains("rnt_addressdetail") ? item.GetAttributeValue<string>("rnt_addressdetail") : string.Empty,
                    firstName = item.Attributes.Contains("rnt_firstname") ? item.GetAttributeValue<string>("rnt_firstname") : string.Empty,
                    lastName = item.Attributes.Contains("rnt_lastname") ? item.GetAttributeValue<string>("rnt_lastname") : string.Empty,
                    companyName = item.Attributes.Contains("rnt_companyname") ? item.GetAttributeValue<string>("rnt_companyname") : string.Empty,
                    governmentId = item.Attributes.Contains("rnt_government") ? item.GetAttributeValue<string>("rnt_government") : string.Empty,
                    taxNumber = item.Attributes.Contains("rnt_taxnumber") ? item.GetAttributeValue<string>("rnt_taxnumber") : string.Empty,
                    taxOfficeId = item.Attributes.Contains("rnt_taxofficeid") ? item.GetAttributeValue<EntityReference>("rnt_taxofficeid").Id : Guid.Empty,
                    invoiceType = item.Attributes.Contains("rnt_invoicetypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value : 0,
                };
                invoiceAddressDatas.Add(invoiceAddressData);
            }
            return invoiceAddressDatas;
        }

        public List<InvoiceAddressData> getInvoiceAddressByGovermentIdOrByTaxNumber(string key)
        {
            InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(this.OrgService);
            var response = invoiceAddressRepository.getInvoiceAddressByGovermentIdOrByTaxNumber(key);

            List<InvoiceAddressData> invoiceAddressDatas = new List<InvoiceAddressData>();
            foreach (var item in response)
            {
                InvoiceAddressData invoiceAddressData = new InvoiceAddressData
                {
                    invoiceAddressId = item.Id,
                    name = item.Attributes.Contains("rnt_name") ? item.GetAttributeValue<string>("rnt_name") : string.Empty,
                    addressCountryId = item.Attributes.Contains("rnt_countryid") ? item.GetAttributeValue<EntityReference>("rnt_countryid").Id : Guid.Empty,
                    addressCountryName = item.Attributes.Contains("rnt_countryid") ? item.GetAttributeValue<EntityReference>("rnt_countryid").Name : string.Empty,
                    addressCityId = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Id : Guid.Empty,
                    addressCityName = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Name : string.Empty,
                    addressDistrictId = item.Attributes.Contains("rnt_districtid") ? item.GetAttributeValue<EntityReference>("rnt_districtid").Id : Guid.Empty,
                    addressDistrictName = item.Attributes.Contains("rnt_districtid") ? item.GetAttributeValue<EntityReference>("rnt_districtid").Name : string.Empty,
                    addressDetail = item.Attributes.Contains("rnt_addressdetail") ? item.GetAttributeValue<string>("rnt_addressdetail") : string.Empty,
                    firstName = item.Attributes.Contains("rnt_firstname") ? item.GetAttributeValue<string>("rnt_firstname") : string.Empty,
                    lastName = item.Attributes.Contains("rnt_lastname") ? item.GetAttributeValue<string>("rnt_lastname") : string.Empty,
                    companyName = item.Attributes.Contains("rnt_companyname") ? item.GetAttributeValue<string>("rnt_companyname") : string.Empty,
                    governmentId = item.Attributes.Contains("rnt_government") ? item.GetAttributeValue<string>("rnt_government") : string.Empty,
                    taxNumber = item.Attributes.Contains("rnt_taxnumber") ? item.GetAttributeValue<string>("rnt_taxnumber") : string.Empty,
                    taxOfficeId = item.Attributes.Contains("rnt_taxofficeid") ? item.GetAttributeValue<EntityReference>("rnt_taxofficeid").Id : Guid.Empty,
                    invoiceType = item.Attributes.Contains("rnt_invoicetypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value : 0,
                };
                invoiceAddressDatas.Add(invoiceAddressData);
            }
            return invoiceAddressDatas;
        }
    }
}
