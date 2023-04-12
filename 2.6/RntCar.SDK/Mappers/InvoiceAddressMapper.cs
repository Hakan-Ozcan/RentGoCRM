using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using System;
using System.Collections.Generic;

namespace RntCar.SDK.Mappers
{
    public class InvoiceAddressMapper
    {
        public List<InvoiceAddressData_Web> createWebInvoiceAddressList(List<Entity> invoiceAddresses)
        {
            var convertedData = invoiceAddresses.ConvertAll(item => new InvoiceAddressData_Web
            {
                addressDetail = item.GetAttributeValue<string>("rnt_addressdetail"),
                cityId = item.Attributes.Contains("rnt_cityid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_cityid").Id : null,
                cityName = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Name : null,
                countryId = item.Attributes.Contains("rnt_countryid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_countryid").Id : null,
                countryName = item.Attributes.Contains("rnt_countryid") ? item.GetAttributeValue<EntityReference>("rnt_countryid").Name : null,
                districtId = item.Attributes.Contains("rnt_districtid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_districtid").Id : null,
                districtName = item.Attributes.Contains("rnt_districtid") ? item.GetAttributeValue<EntityReference>("rnt_districtid").Name : null,
                companyName = item.GetAttributeValue<string>("rnt_companyname"),
                firstName = item.GetAttributeValue<string>("rnt_firstname"),
                lastName = item.GetAttributeValue<string>("rnt_lastname"),
                governmentId = item.GetAttributeValue<string>("rnt_government"),
                invoiceTypeCode = item.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value,
                invoiceAddressId = item.Id,
                taxNumber = item.GetAttributeValue<string>("rnt_taxnumber"),
                taxOfficeId = item.Attributes.Contains("rnt_taxofficeid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_taxofficeid").Id : null,
                taxOfficeName = item.Attributes.Contains("rnt_taxofficeid") ? item.GetAttributeValue<EntityReference>("rnt_taxofficeid").Name : null,
                name = item.GetAttributeValue<string>("rnt_name")
            });
            return convertedData;
        }

        public List<InvoiceAddressData_Mobile> createMobileInvoiceAddressList(List<Entity> invoiceAddresses)
        {
            var convertData = invoiceAddresses.ConvertAll(item => new InvoiceAddressData_Mobile
            {
                addressDetail = item.GetAttributeValue<string>("rnt_addressdetail"),
                cityId = item.Attributes.Contains("rnt_cityid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_cityid").Id : null,
                cityName = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Name : null,
                countryId = item.Attributes.Contains("rnt_countryid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_countryid").Id : null,
                countryName = item.Attributes.Contains("rnt_countryid") ? item.GetAttributeValue<EntityReference>("rnt_countryid").Name : null,
                districtId = item.Attributes.Contains("rnt_districtid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_districtid").Id : null,
                districtName = item.Attributes.Contains("rnt_districtid") ? item.GetAttributeValue<EntityReference>("rnt_districtid").Name : null,
                companyName = item.GetAttributeValue<string>("rnt_companyname"),
                firstName = item.GetAttributeValue<string>("rnt_firstname"),
                lastName = item.GetAttributeValue<string>("rnt_lastname"),
                governmentId = item.GetAttributeValue<string>("rnt_government"),
                invoiceTypeCode = item.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value,
                invoiceAddressId = item.Id,
                taxNumber = item.GetAttributeValue<string>("rnt_taxnumber"),
                taxOfficeId = item.Attributes.Contains("rnt_taxofficeid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_taxofficeid").Id : null,
                taxOfficeName = item.Attributes.Contains("rnt_taxofficeid") ? item.GetAttributeValue<EntityReference>("rnt_taxofficeid").Name : null,
                name = item.GetAttributeValue<string>("rnt_name")
            });

            return convertData;
        }

        public InvoiceAddressData  buildInvoiceAddressData(InvoiceAddressData_Web invoiceAddressData_Web, Guid contactId)
        {
            return new InvoiceAddressData
            {
                addressCityId = invoiceAddressData_Web.cityId,
                addressCityName = invoiceAddressData_Web.cityName,
                addressCountryId = invoiceAddressData_Web.countryId.Value,               
                addressCountryName = invoiceAddressData_Web.countryName,
                addressDistrictId = invoiceAddressData_Web.districtId,
                addressDistrictName = invoiceAddressData_Web.districtName,
                firstName = invoiceAddressData_Web.firstName,
                lastName = invoiceAddressData_Web.lastName,
                governmentId = invoiceAddressData_Web.governmentId,
                invoiceType = invoiceAddressData_Web.invoiceTypeCode,
                taxNumber = invoiceAddressData_Web.taxNumber,
                taxOfficeId= invoiceAddressData_Web.taxOfficeId,
                invoiceAddressId = invoiceAddressData_Web.invoiceAddressId,                
                companyName = invoiceAddressData_Web.companyName,
                addressDetail = invoiceAddressData_Web.addressDetail,
                individualCustomerId = contactId,
            };
        }

        public InvoiceAddressData buildInvoiceAddressData(InvoiceAddressData_Mobile invoiceAddressData_mobile, Guid contactId)
        {
            return new InvoiceAddressData
            {
                addressCityId = invoiceAddressData_mobile.cityId,
                addressCityName = invoiceAddressData_mobile.cityName,
                addressCountryId = invoiceAddressData_mobile.countryId.Value,
                addressCountryName = invoiceAddressData_mobile.countryName,
                addressDistrictId = invoiceAddressData_mobile.districtId,
                addressDistrictName = invoiceAddressData_mobile.districtName,
                firstName = invoiceAddressData_mobile.firstName,
                lastName = invoiceAddressData_mobile.lastName,
                governmentId = invoiceAddressData_mobile.governmentId,
                invoiceType = invoiceAddressData_mobile.invoiceTypeCode,
                taxNumber = invoiceAddressData_mobile.taxNumber,
                taxOfficeId = invoiceAddressData_mobile.taxOfficeId,
                invoiceAddressId = invoiceAddressData_mobile.invoiceAddressId,
                companyName = invoiceAddressData_mobile.companyName,
                addressDetail = invoiceAddressData_mobile.addressDetail,
                individualCustomerId = contactId,
            };
        }

        public InvoiceAddressData buildInvoiceAddressData(InvoiceAddressData_Broker invoiceAddressData_Broker, Guid contactId)
        { 
            return new InvoiceAddressData
            {
                addressCityId = invoiceAddressData_Broker.cityId,
                addressCityName = invoiceAddressData_Broker.cityName,
                addressCountryId = invoiceAddressData_Broker.countryId.Value,
                addressCountryName = invoiceAddressData_Broker.countryName,
                addressDistrictId = invoiceAddressData_Broker.districtId,
                addressDistrictName = invoiceAddressData_Broker.districtName,
                firstName = invoiceAddressData_Broker.firstName,
                lastName = invoiceAddressData_Broker.lastName,
                governmentId = invoiceAddressData_Broker.governmentId,
                invoiceType = invoiceAddressData_Broker.invoiceTypeCode,
                taxNumber = invoiceAddressData_Broker.taxNumber,
                taxOfficeId = invoiceAddressData_Broker.taxOfficeId,
                invoiceAddressId = invoiceAddressData_Broker.invoiceAddressId,
                companyName = invoiceAddressData_Broker.companyName,
                addressDetail = invoiceAddressData_Broker.addressDetail,
                individualCustomerId = contactId,
            };
        }
    }
}
