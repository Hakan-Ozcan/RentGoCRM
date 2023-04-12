using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class CorporateCustomerBL : BusinessHandler
    {
        public CorporateCustomerBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }
        public CorporateCustomerBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public CorporateCustomerBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public MongoDBResponse createCorporateCustomerInMongoDB(Entity corporateCustomer)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateCorporateCustomerInMongoDB", Method.POST);
            var corporateCustomerData = this.builCorporateCustomerData(corporateCustomer);
            restSharpHelper.AddJsonParameter<ClassLibrary.MongoDB.CorporateCustomerData>(corporateCustomerData);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        public MongoDBResponse updateCorporateCustomerInMongoDB(Entity corporateCustomer)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateCorporateCustomerInMongoDB", Method.POST);

            var corporateCustomerData = this.builCorporateCustomerData(corporateCustomer);
            restSharpHelper.AddJsonParameter<ClassLibrary.MongoDB.CorporateCustomerData>(corporateCustomerData);
            restSharpHelper.AddQueryParameter("id", Convert.ToString(corporateCustomer.Id));

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }

        public List<ClassLibrary.CorporateCustomerData> searchCorporateCustomer(string criteria)
        {
            CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
            var result = corporateCustomerRepository.getCorporateCustomersByFetchXML(criteria);


            List<ClassLibrary.CorporateCustomerData> data = new List<ClassLibrary.CorporateCustomerData>();

            foreach (var item in result.Entities)
            {
                data.Add(new ClassLibrary.CorporateCustomerData
                {
                    companyName = item.GetAttributeValue<String>("name"),
                    taxNumber = item.GetAttributeValue<String>("rnt_taxnumber"),
                    taxOffice = item.Attributes.Contains("rnt_taxoffice") ?
                                item.GetAttributeValue<EntityReference>("rnt_taxoffice").Name :
                                null,
                    taxOfficeId = item.Attributes.Contains("rnt_taxoffice") ?
                                   Convert.ToString(item.GetAttributeValue<EntityReference>("rnt_taxoffice").Id) :
                                   null,
                    telephone = item.GetAttributeValue<String>("telephone1"),
                    corporateCustomerId = item.Id
                });
            }

            return data;
        }

        public InvoiceAddressData getCorporateCustomerAddress(Guid corporateCustomerId)
        {
            CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService, this.CrmServiceClient);
            var corpAddress = corporateCustomerRepository.getCorporateCustomerById(corporateCustomerId, new string[] {
                                                                                                                   "rnt_countryid",
                                                                                                                   "rnt_cityid",
                                                                                                                   "rnt_districtid",
                                                                                                                   "rnt_adressdetail",
                                                                                                                   "rnt_taxnumber",
                                                                                                                   "rnt_taxoffice",
                                                                                                                   "emailaddress1",
                                                                                                                   "name"});

            var r = new InvoiceAddressData
            {
                addressCityId = corpAddress.GetAttributeValue<EntityReference>("rnt_cityid").Id,
                addressCityName = corpAddress.GetAttributeValue<EntityReference>("rnt_cityid").Name,
                addressCountryId = corpAddress.GetAttributeValue<EntityReference>("rnt_countryid").Id,
                addressCountryName = corpAddress.GetAttributeValue<EntityReference>("rnt_countryid").Name,
                addressDistrictId = corpAddress.GetAttributeValue<EntityReference>("rnt_districtid").Id,
                addressDistrictName = corpAddress.GetAttributeValue<EntityReference>("rnt_districtid").Name,
                invoiceType = (int)rnt_invoiceaddress_rnt_invoicetypecode.Corporate,
                taxNumber = corpAddress.GetAttributeValue<string>("rnt_taxnumber"),
                taxOfficeId = corpAddress.GetAttributeValue<EntityReference>("rnt_taxoffice").Id,
                email = corpAddress.GetAttributeValue<string>("emailaddress1"),
                invoiceAddressId = corporateCustomerId,
                addressDetail = corpAddress.GetAttributeValue<string>("rnt_adressdetail"),
                companyName = corpAddress.GetAttributeValue<string>("name")
            };

            this.Trace("r : " + JsonConvert.SerializeObject(r));
            return r;
        }

        private ClassLibrary.MongoDB.CorporateCustomerData builCorporateCustomerData(Entity corporateCustomer)
        {
            var corporateCustomerData = new ClassLibrary.MongoDB.CorporateCustomerData
            {
                name = corporateCustomer.Attributes.Contains("name") ? corporateCustomer.GetAttributeValue<string>("name") : string.Empty,
                corporateCustomerId = Convert.ToString(corporateCustomer.Id),
                accountTypeCode = corporateCustomer.Attributes.Contains("rnt_accounttypecode") ? corporateCustomer.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value : 0,
                brokerCode = corporateCustomer.Attributes.Contains("rnt_brokercode") ? corporateCustomer.GetAttributeValue<string>("rnt_brokercode") : string.Empty,
                priceCodeId = corporateCustomer.Contains("rnt_pricecodeid") ?
                             corporateCustomer.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id :
                             Guid.Empty,
                monthlyPriceCodeId = corporateCustomer.Contains("rnt_monthlypricecodeid") ?
                                     corporateCustomer.GetAttributeValue<EntityReference>("rnt_monthlypricecodeid").Id :
                                     Guid.Empty,
                priceFactorGroupCode = corporateCustomer.Attributes.Contains("rnt_pricefactorgroupcode") ? corporateCustomer.GetAttributeValue<OptionSetValue>("rnt_pricefactorgroupcode").Value : 0,
                processIndividualPrices = corporateCustomer.Attributes.Contains("rnt_processindividualprices") ? corporateCustomer.GetAttributeValue<bool>("rnt_processindividualprices") : false,
                statuscode = corporateCustomer.GetAttributeValue<OptionSetValue>("statuscode").Value,
                statecode = corporateCustomer.GetAttributeValue<OptionSetValue>("statecode").Value,
                creditlimit = corporateCustomer.Attributes.Contains("creditlimit") ? corporateCustomer.GetAttributeValue<Money>("creditlimit").Value : 0,
            };

            return corporateCustomerData;
        }
    }
}
