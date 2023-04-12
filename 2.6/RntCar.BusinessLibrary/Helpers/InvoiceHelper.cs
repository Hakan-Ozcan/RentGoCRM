using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Helpers
{

    public class InvoiceHelper : HelperHandler
    {
        public InvoiceHelper(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }
        public InvoiceHelper(IOrganizationService organizationService) : base(organizationService)
        {
        }

        public InvoiceHelper(CrmServiceClient crmServiceClient, IOrganizationService organizationService) : base(crmServiceClient, organizationService)
        {
        }

        public InvoiceHelper(IOrganizationService organizationService, ITracingService tracingService) : base(organizationService, tracingService)
        {
        }
        public static InvoiceAddressData buildInvoiceAddressDataFromAccountEntity(Entity invoice)
        {
            return new InvoiceAddressData
            {
                addressCityId = invoice.Contains("rnt_cityid") ? (Guid?)invoice.GetAttributeValue<EntityReference>("rnt_cityid").Id : null,
                addressCityName = invoice.Contains("rnt_cityid") ? invoice.GetAttributeValue<EntityReference>("rnt_cityid").Name : null,
                addressDistrictId = invoice.Contains("rnt_districtid") ? (Guid?)invoice.GetAttributeValue<EntityReference>("rnt_districtid").Id : null,
                addressDistrictName = invoice.Contains("rnt_districtid") ? invoice.GetAttributeValue<EntityReference>("rnt_districtid").Name : null,
                addressCountryId = invoice.GetAttributeValue<EntityReference>("rnt_countryid").Id,
                addressCountryName = invoice.GetAttributeValue<EntityReference>("rnt_countryid").Name,
                addressDetail = invoice.GetAttributeValue<string>("rnt_adressdetail"),
                email = invoice.GetAttributeValue<string>("emailaddress1"),
                companyName = invoice.GetAttributeValue<string>("name"),
                mobilePhone = invoice.GetAttributeValue<string>("telephone1"),
                invoiceType = (int)rnt_invoice_rnt_invoicetypecode.Corporate,
                taxNumber = invoice.GetAttributeValue<string>("rnt_taxnumber"),
                taxOfficeId = invoice.Contains("rnt_taxoffice") ? (Guid?)invoice.GetAttributeValue<EntityReference>("rnt_taxoffice").Id : null,

            };
        }
        public static InvoiceAddressData buildInvoiceAddressDataFromInvoiceAddressEntity(Entity invoice)
        {
            return new InvoiceAddressData
            {
                invoiceAddressId = invoice.Id,
                addressCityId = invoice.Contains("rnt_cityid") ? (Guid?)invoice.GetAttributeValue<EntityReference>("rnt_cityid").Id : null,
                addressCityName = invoice.Contains("rnt_cityid") ? invoice.GetAttributeValue<EntityReference>("rnt_cityid").Name : null,
                addressDistrictId = invoice.Contains("rnt_districtid") ? (Guid?)invoice.GetAttributeValue<EntityReference>("rnt_districtid").Id : null,
                addressDistrictName = invoice.Contains("rnt_districtid") ? invoice.GetAttributeValue<EntityReference>("rnt_districtid").Name : null,
                addressCountryId = invoice.GetAttributeValue<EntityReference>("rnt_countryid").Id,
                addressCountryName = invoice.GetAttributeValue<EntityReference>("rnt_countryid").Name,
                addressDetail = invoice.GetAttributeValue<string>("rnt_addressdetail"),
                email = invoice.GetAttributeValue<string>("rnt_email"),
                companyName = invoice.GetAttributeValue<string>("rnt_companyname"),
                firstName = invoice.GetAttributeValue<string>("rnt_firstname"),
                lastName = invoice.GetAttributeValue<string>("rnt_lastname"),
                governmentId = invoice.GetAttributeValue<string>("rnt_government"),
                mobilePhone = invoice.GetAttributeValue<string>("rnt_mobilephone"),
                individualCustomerId = invoice.GetAttributeValue<EntityReference>("rnt_contactid").Id,
                invoiceType = invoice.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value,
                taxNumber = invoice.GetAttributeValue<string>("rnt_taxnumber"),
                taxOfficeId = invoice.Contains("rnt_taxofficeid") ? (Guid?)invoice.GetAttributeValue<EntityReference>("rnt_taxofficeid").Id : null,

            };
        }
        public static InvoiceAddressData buildInvoiceAddressDataFromInvoiceEntity(Entity invoice)
        {
            return new InvoiceAddressData
            {
                invoiceAddressId = invoice.Attributes.Contains("rnt_customerinvoiceaddressid") ? Guid.Parse(invoice.GetAttributeValue<string>("rnt_customerinvoiceaddressid")) : Guid.Empty,
                addressCityId = invoice.Contains("rnt_cityid") ? (Guid?)invoice.GetAttributeValue<EntityReference>("rnt_cityid").Id : null,
                addressCityName = invoice.Contains("rnt_cityid") ? invoice.GetAttributeValue<EntityReference>("rnt_cityid").Name : null,
                addressDistrictId = invoice.Contains("rnt_districtid") ? (Guid?)invoice.GetAttributeValue<EntityReference>("rnt_districtid").Id : null,
                addressDistrictName = invoice.Contains("rnt_districtid") ? invoice.GetAttributeValue<EntityReference>("rnt_districtid").Name : null,
                addressCountryId = invoice.GetAttributeValue<EntityReference>("rnt_countryid").Id,
                addressCountryName = invoice.GetAttributeValue<EntityReference>("rnt_countryid").Name,
                addressDetail = invoice.GetAttributeValue<string>("rnt_addressdetail"),
                email = invoice.GetAttributeValue<string>("rnt_email"),
                companyName = invoice.GetAttributeValue<string>("rnt_companyname"),
                firstName = invoice.GetAttributeValue<string>("rnt_firstname"),
                lastName = invoice.GetAttributeValue<string>("rnt_lastname"),
                governmentId = invoice.GetAttributeValue<string>("rnt_govermentid"),
                mobilePhone = invoice.GetAttributeValue<string>("rnt_mobilephone"),
                invoiceType = invoice.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value,
                taxNumber = invoice.GetAttributeValue<string>("rnt_taxnumber"),
                taxOfficeId = invoice.Contains("rnt_taxofficeid") ? (Guid?)invoice.GetAttributeValue<EntityReference>("rnt_taxofficeid").Id : null,

            };
        }
        public Guid? createInvoiceFromInvoiceAddress(Guid contractId, Guid? invoiceAddressId)
        {
            ContractRepository contractRepository = new ContractRepository(this.IOrganizationService);
            var contractEntity = contractRepository.getContractById(contractId, new string[] { "statuscode", "rnt_pickupbranchid" });
            var contractStatus = contractEntity.GetAttributeValue<OptionSetValue>("statuscode").Value;

            if (contractStatus == (int)RntCar.ClassLibrary._Enums_1033.rnt_contract_StatusCode.Completed)
            {
                InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(this.IOrganizationService);
                var invoiceAddress = invoiceAddressRepository.getInvoiceAddressById(invoiceAddressId.Value);

                ConfigurationRepository configurationRepository = new ConfigurationRepository(this.IOrganizationService);
                var currency = configurationRepository.GetConfigurationByKey("currency_TRY");
                InvoiceBL invoiceBL = new InvoiceBL(this.IOrganizationService, this.ITracingService);
                return invoiceBL.createInvoice(buildInvoiceAddressDataFromInvoiceAddressEntity(invoiceAddress), null, (Guid?)contractId, new Guid(currency));
            }
            return null;
        }


        public Guid? createInvoiceFromInvoiceAddress_Reservation(Guid reservationId, Guid? invoiceAddressId)
        {
            ReservationRepository reservationRepository = new ReservationRepository(this.IOrganizationService);

            InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(this.IOrganizationService);
            var invoiceAddress = invoiceAddressRepository.getInvoiceAddressById(invoiceAddressId.Value);

            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.IOrganizationService);
            var currency = configurationRepository.GetConfigurationByKey("currency_TRY");
            InvoiceBL invoiceBL = new InvoiceBL(this.IOrganizationService, this.ITracingService);
            return invoiceBL.createInvoice(buildInvoiceAddressDataFromInvoiceAddressEntity(invoiceAddress), reservationId, null, new Guid(currency));

        }
        public Guid? createInvoiceFromInvoice(Guid contractId, Guid? invoiceId)
        {
            ContractRepository contractRepository = new ContractRepository(this.IOrganizationService);
            var contractEntity = contractRepository.getContractById(contractId, new string[] { "statuscode", "rnt_pickupbranchid" });
            var contractStatus = contractEntity.GetAttributeValue<OptionSetValue>("statuscode").Value;

            if (contractStatus == (int)RntCar.ClassLibrary._Enums_1033.rnt_contract_StatusCode.Completed)
            {
                ConfigurationRepository configurationRepository = new ConfigurationRepository(this.IOrganizationService);
                var currency = configurationRepository.GetConfigurationByKey("currency_TRY");

                InvoiceRepository invoiceRepository = new InvoiceRepository(this.IOrganizationService);
                var _invoice = invoiceRepository.getInvoiceById(invoiceId.Value);
                InvoiceBL invoiceBL = new InvoiceBL(this.IOrganizationService, this.ITracingService);
                return invoiceBL.createInvoice(buildInvoiceAddressDataFromInvoiceEntity(_invoice), null, (Guid?)contractId, new Guid(currency));
            }
            return null;
        }
        public void manualPaymentInvoiceOperations(Guid contractId, List<Guid> contractItems, Guid? invoiceId)
        {
            List<Entity> contractItemsLogo = new List<Entity>();
            ContractRepository contractRepository = new ContractRepository(this.IOrganizationService);
            var contractEntity = contractRepository.getContractById(contractId, new string[] { "statuscode", "rnt_pickupbranchid" });

            var contractStatus = contractEntity.GetAttributeValue<OptionSetValue>("statuscode").Value;
            if (contractStatus == (int)RntCar.ClassLibrary._Enums_1033.rnt_contract_StatusCode.Completed)
            {
                Entity updateInvoice = new Entity("rnt_invoice");
                updateInvoice["rnt_defaultinvoice"] = false;
                updateInvoice.Id = invoiceId.Value;
                this.IOrganizationService.Update(updateInvoice);

                InvoiceRepository invoiceRepository = new InvoiceRepository(this.IOrganizationService);
                var newInvoice = invoiceRepository.getInvoiceById(invoiceId.Value);

                foreach (var item in contractItems)
                {
                    ContractItemRepository contractItemRepository = new ContractItemRepository(this.IOrganizationService);
                    var contractItem = contractItemRepository.getContractItemId(item);
                    contractItemsLogo.Add(contractItem);

                    this.Trace("default invoice setting end");

                    var createInvoiceItem = this.createInvoiceItemfromContractItem(item, invoiceId.Value);
                    createInvoiceItem["rnt_netamount"] = contractItem.GetAttributeValue<Money>("rnt_netamount").Value;
                    this.IOrganizationService.Create(createInvoiceItem);
                    this.Trace("create invoice item end");
                }

                BranchHelper branchHelper = new BranchHelper(this.IOrganizationService);
                var branchInfo = branchHelper.getBranchType(null, contractId);
                if (branchInfo.branchType == (int)rnt_BranchType.Office)
                {
                    //workflow trigger onn rnt_invoiceid_manualpayment
                    this.Trace("sending to logo");
                    this.Trace("contractItems : " + JsonConvert.SerializeObject(contractItems));
                    this.Trace("rnt_invoiceid_manualpayment : " + newInvoice.Id.ToString());
                    Entity e = new Entity("rnt_contract");
                    e.Id = contractId;
                    e["rnt_contractitems_manualpayment"] = JsonConvert.SerializeObject(contractItems);
                    e["rnt_invoiceid_manualpayment"] = newInvoice.Id.ToString();
                    this.IOrganizationService.Update(e);

                    //this.sendInvoicetoLogo(newInvoice, contractItemsLogo, contractId, contractEntity.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id);
                }
                else if (branchInfo.branchType == (int)rnt_BranchType.Franchise)
                {
                    var c = contractRepository.getContractById(contractId, new string[] { "rnt_paymentmethodcode", "rnt_contracttypecode" });
                    var paymentMethodCode = c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
                    var contractType = c.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value;
                    if (contractType == (int)rnt_ReservationTypeCode.Kurumsal ||
                        paymentMethodCode == (int)rnt_PaymentMethodCode.FullCredit)
                    {
                        this.Trace("sending to logo");
                        //workflow trigger onn rnt_invoiceid_manualpayment

                        Entity e = new Entity("rnt_contract");
                        e.Id = contractId;
                        e["rnt_contractitems_manualpayment"] = JsonConvert.SerializeObject(contractItems);
                        e["rnt_invoiceid_manualpayment"] = newInvoice.Id.ToString();
                        this.IOrganizationService.Update(e);
                        //this.sendInvoicetoLogo(newInvoice, contractItemsLogo, contractId, contractEntity.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id);
                    }
                    else
                    {
                        InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.IOrganizationService);
                        var items = invoiceItemRepository.getInvoiceItemsByInvoiceId(invoiceId.Value);
                        InvoiceBL invoiceBL = new InvoiceBL(this.IOrganizationService);
                        invoiceBL.updateInvoiceStatus(invoiceId.Value, (int)rnt_invoice_StatusCode.DealerInvoicing);
                        invoiceBL.updateInvoiceItemsStatusByInvoiceHeader(items, (int)rnt_invoice_StatusCode.DealerInvoicing);
                        invoiceBL.setInvoiceDatetoInvoiceItem(items);
                    }

                }
            }
            else
            {
                InvoiceRepository invoiceRepository = new InvoiceRepository(this.IOrganizationService);
                var invoice = invoiceRepository.getFirstInvoiceByContractId(contractId);
                if (invoice == null)
                {
                    invoice = invoiceRepository.getInvoiceById(invoiceId.Value);
                }
                foreach (var item in contractItems)
                {
                    var createInvoiceItem = this.createInvoiceItemfromContractItem(item, invoice.Id);
                    this.IOrganizationService.Create(createInvoiceItem);
                }

            }


        }

        public Invoice_Corporate invoiceOperationsCorporate(Guid contractId, Guid corporateId)
        {
            Guid invoiceId = Guid.Empty;
            var corpInvoiceAdress = new InvoiceAddressData();
            ContractRepository contractRepository = new ContractRepository(this.IOrganizationService);
            var contractEntity = contractRepository.getContractById(contractId, new string[] { "statuscode", "rnt_corporateid" });
            if (contractEntity.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)RntCar.ClassLibrary._Enums_1033.rnt_contract_StatusCode.Completed)
            {
                CorporateCustomerBL corporateCustomerBL = new CorporateCustomerBL(this.IOrganizationService, this.ITracingService);
                corpInvoiceAdress = corporateCustomerBL.getCorporateCustomerAddress(corporateId);

                ConfigurationRepository configurationRepository = new ConfigurationRepository(this.IOrganizationService);
                var currency = configurationRepository.GetConfigurationByKey("currency_TRY");

                InvoiceBL invoiceBL = new InvoiceBL(this.IOrganizationService);
                invoiceId = invoiceBL.createInvoice(corpInvoiceAdress, null, contractId, new Guid(currency));
            }
            else
            {
                InvoiceRepository invoiceRepository = new InvoiceRepository(this.IOrganizationService);
                var invoice = invoiceRepository.getFirstInvoiceByContractId(contractId);
                if (invoice == null)
                {
                    this.Trace("invoice is null");
                    CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.IOrganizationService);
                    var acc = corporateCustomerRepository.getCorporateCustomerById(corporateId);
                    var data = buildInvoiceAddressDataFromAccountEntity(acc);
                    InvoiceBL invoiceBL = new InvoiceBL(this.IOrganizationService);
                    ConfigurationBL configurationBL = new ConfigurationBL(this.IOrganizationService);
                    var currency = configurationBL.GetConfigurationByName("currency_TRY");
                    var createdInvoice = invoiceBL.createInvoice(data, null, contractId, new Guid(currency));

                    invoice = invoiceRepository.getInvoiceById(createdInvoice);
                }
                corpInvoiceAdress = buildInvoiceAddressDataFromInvoiceEntity(invoice);

                invoiceId = invoice.Id;
            }

            return new Invoice_Corporate
            {
                invoiceId = invoiceId,
                invoiceAddressData = corpInvoiceAdress
            };
        }

        public void sendInvoicetoLogo(Entity invoice, List<Entity> contractItems, Guid contractId, Guid pickupBranchId)
        {
            InvoiceBL invoiceBL = new InvoiceBL(this.IOrganizationService);
            invoiceBL.getDocumentInfo(contractId, null);
            var currentAccountCodeParameter = invoiceBL.prepareCurrentAccountCodeParameter(invoice);
            this.Trace("prepare getting current account code parameter");

            LogoHelper logoHelper = new LogoHelper(this.IOrganizationService, this.ITracingService);
            logoHelper.connect();

            invoiceBL.currentAccountCode = logoHelper.currentAccountCode(currentAccountCodeParameter);

            this.Trace("getting current account code : " + invoiceBL.currentAccountCode);

            InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.IOrganizationService);
            // get invoice items by invoce for salesinvoce
            var invoiceItems = invoiceItemRepository.getInvoiceItemsByInvoiceId(invoice.Id);
            this.Trace("invoice items length : " + invoiceItems.Count);

            var salesInvoiceParameter = new SalesInvoiceParameter();
            var salesInvoceResponse = new SalesInvoiceResponse
            {
                ResponseResult = ResponseResult.ReturnError("Current account code is null"),
            };
            var statusCode = (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegrationError;

            if (!string.IsNullOrEmpty(invoiceBL.currentAccountCode))
            {
                // prepare invoice information for salesinvoce
                invoiceBL.prepareInvoiceInformationListParameterForContract(contractId, invoiceItems, contractItems, true);
                this.Trace("created invoiceItem list for logo");

                BranchRepository branchRepository = new BranchRepository(this.IOrganizationService);
                invoiceBL.documentBranchInfo = branchRepository.getBranchById(pickupBranchId);

                salesInvoiceParameter = invoiceBL.prepareSalesInvoiceParameter(invoice);
                this.Trace("params : " + JsonConvert.SerializeObject(salesInvoiceParameter));
                this.Trace("preapre sales invoice parameter");
                salesInvoceResponse = logoHelper.salesInvoice(salesInvoiceParameter);
                this.Trace("response : " + salesInvoceResponse.ResponseResult.Result);
                this.Trace("response : " + salesInvoceResponse.ResponseResult.ExceptionDetail);
                statusCode = salesInvoceResponse.ResponseResult.Result ? (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegratedWithLogo : (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegrationError;


                if (statusCode == (int)rnt_invoice_StatusCode.IntegratedWithLogo)
                {
                    invoiceBL.setInvoiceDatetoInvoiceItem(invoiceItems);
                }
            }
            else
            {
                this.Trace("Current account code is null");
            }
            this.Trace("statusCode : " + statusCode);
            // update invoce record with all service parameters and sales invoice response
            invoiceBL.updateInvoiceWithServicesParameters(invoice.Id,
                                                          salesInvoceResponse.salesInvoices?.invoiceNumber,
                                                          statusCode,
                                                          JsonConvert.SerializeObject(currentAccountCodeParameter),
                                                          JsonConvert.SerializeObject(invoiceBL.currentAccountCode),
                                                          JsonConvert.SerializeObject(salesInvoiceParameter),
                                                          JsonConvert.SerializeObject(salesInvoceResponse),
                                                          logoHelper.getCurrentFirm());


            invoiceBL.updateInvoiceItemsStatusByInvoiceHeader(invoiceItems, statusCode);


        }

        public Entity createInvoiceFromCorporateCustomer(Guid corporateId, Guid contractId)
        {
            CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.IOrganizationService);
            var acc = corporateCustomerRepository.getCorporateCustomerById(corporateId);
            var data = buildInvoiceAddressDataFromAccountEntity(acc);
            InvoiceBL invoiceBL = new InvoiceBL(this.IOrganizationService);
            ConfigurationBL configurationBL = new ConfigurationBL(this.IOrganizationService);
            var currency = configurationBL.GetConfigurationByName("currency_TRY");
            var createdInvoice = invoiceBL.createInvoice(data, null, contractId, new Guid(currency));
            InvoiceRepository invoiceRepository = new InvoiceRepository(this.IOrganizationService);
            return invoiceRepository.getInvoiceById(createdInvoice);
        }
        public Entity createInvoiceFromIndividualCustomer(Guid customerId, Guid contractId)
        {
            InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(this.IOrganizationService);
            var customerAddress = invoiceAddressRepository.getFirstInvoiceAddressByCustomerIdByGivenColumns(customerId).FirstOrDefault();
            var data = buildInvoiceAddressDataFromInvoiceAddressEntity(customerAddress);
            InvoiceBL invoiceBL = new InvoiceBL(this.IOrganizationService);
            ConfigurationBL configurationBL = new ConfigurationBL(this.IOrganizationService);
            var currency = configurationBL.GetConfigurationByName("currency_TRY");
            var createdInvoice = invoiceBL.createInvoice(data, null, contractId, new Guid(currency));
            InvoiceRepository invoiceRepository = new InvoiceRepository(this.IOrganizationService);
            return invoiceRepository.getInvoiceById(createdInvoice);
        }
        private Entity createInvoiceItemfromContractItem(Guid contractItemId, Guid invoiceId)
        {
            XrmHelper xrmHelper = new XrmHelper(this.IOrganizationService);
            Entity createInvoiceItem = xrmHelper.initializeFromRequest("rnt_contractitem", contractItemId, "rnt_invoiceitem");

            ContractItemRepository contractItemRepository = new ContractItemRepository(this.IOrganizationService);
            var contractItem = contractItemRepository.getContractItemIdByGivenColumns(contractItemId, new string[] { "rnt_name" });
            createInvoiceItem["rnt_name"] = contractItem.GetAttributeValue<string>("rnt_name");
            createInvoiceItem["rnt_invoiceid"] = new EntityReference("rnt_invoice", invoiceId);
            return createInvoiceItem;
        }
    }
}
