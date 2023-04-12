using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class InvoiceBL : BusinessHandler
    {
        public Entity documentBranchInfo { get; set; }
        public string currentAccountCode { get; set; }
        private List<InvoiceInformation> invoiceInformationList { get; set; }
        private string documentNumber { get; set; }
        private int documentStatus { get; set; }
        private decimal documentTotalAmount { get; set; }
        private Guid pickupBranchId { get; set; }
        public InvoiceBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public InvoiceBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public InvoiceBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public InvoiceBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public string generateDocument(string invoiceNumber)
        {
            LogoHelper logoHelper = new LogoHelper(this.OrgService);
            return Convert.ToBase64String(logoHelper.getPDFContent(invoiceNumber));
        }

        public void updateInvoiceStatus(Guid invoiceId, int statusCode)
        {
            Entity e = new Entity("rnt_invoice");
            e.Id = invoiceId;
            e["statuscode"] = new OptionSetValue(statusCode);
            this.OrgService.Update(e);
        }
        public Guid createInvoice(InvoiceAddressData invoiceAddressData,
                             Guid? reservationId,
                             Guid? contractId,
                             Guid? currency)
        {
            var entity = this.buildInvoice(invoiceAddressData, null, reservationId, contractId, currency);
            return this.createEntity(entity);
        }

        public void updateInvoice(InvoiceAddressData invoiceAddressData,
                                  Guid? invoiceId,
                                  Guid? reservationId,
                                  Guid? contractId,
                                  Guid? currency)
        {
            var entity = this.buildInvoice(invoiceAddressData, invoiceId, reservationId, contractId, currency);
            this.updateEntity(entity);
        }
        public void updateInvoiceWithServicesParameters(Guid invoiceId,
                                                        string invoiceNumber,
                                                        int statusCode,
                                                        string currentAccountCodeInput,
                                                        string currentAccountCodeOutput,
                                                        string salesInvoiceInput,
                                                        string salesInvoiceOutput,
                                                        string firm)
        {
            Entity e = new Entity("rnt_invoice");
            e["statuscode"] = new OptionSetValue(statusCode);
            e["rnt_logoinvoicenumber"] = invoiceNumber;
            e["rnt_currentaccountcodeinput"] = currentAccountCodeInput;
            e["rnt_currentaccountcodeoutput"] = currentAccountCodeOutput;
            e["rnt_salesinvoiceinput"] = salesInvoiceInput;
            e["rnt_salesinvoiceoutput"] = salesInvoiceOutput + "-" + firm;
            e.Id = invoiceId;
            this.updateEntity(e);
        }

        public void deactiveInvoice(Guid invoiceId, int statusCode)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.setState("rnt_invoice", invoiceId, (int)GlobalEnums.StateCode.Passive, statusCode);
        }

        private Entity buildInvoice(InvoiceAddressData invoiceAddressData,
                                    Guid? invoiceId,
                                    Guid? reservationId,
                                    Guid? contractId,
                                    Guid? currency)
        {
            var logoCurrentAccountCode = string.Empty;
            Entity e = new Entity("rnt_invoice");
            if (invoiceAddressData.individualCustomerId != Guid.Empty)
            {
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
                var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(invoiceAddressData.individualCustomerId, new string[] { "emailaddress1",
                                                                                                                                                              "mobilephone",
                                                                                                                                                              "rnt_logoaccountcode"});
                e["rnt_email"] = customer.Attributes.Contains("emailaddress1") ? customer.GetAttributeValue<string>("emailaddress1") : string.Empty;
                e["rnt_mobilephone"] = customer.Attributes.Contains("mobilephone") ? customer.GetAttributeValue<string>("mobilephone") : string.Empty;
                //logoCurrentAccountCode = customer.GetAttributeValue<string>("rnt_logoaccountcode");
            }
            e["rnt_invoicetypecode"] = new OptionSetValue(invoiceAddressData.invoiceType);

            if (invoiceAddressData.addressCountryId != Guid.Empty)
                e["rnt_countryid"] = new EntityReference("rnt_country", invoiceAddressData.addressCountryId);

            if (reservationId.HasValue)
            {
                e["rnt_reservationid"] = new EntityReference("rnt_reservation", reservationId.Value);
            }
            if (contractId.HasValue)
            {
                e["rnt_contractid"] = new EntityReference("rnt_contract", contractId.Value);
            }

            if (invoiceAddressData.addressCityId.HasValue && invoiceAddressData.addressCityId.Value != Guid.Empty)
                e["rnt_cityid"] = new EntityReference("rnt_city", invoiceAddressData.addressCityId.Value);
            else
                e["rnt_cityid"] = null;

            if (invoiceAddressData.addressDistrictId.HasValue && invoiceAddressData.addressDistrictId.Value != Guid.Empty)
                e["rnt_districtid"] = new EntityReference("rnt_district", invoiceAddressData.addressDistrictId.Value);
            else
                e["rnt_districtid"] = null;

            if (invoiceAddressData.taxOfficeId.HasValue && invoiceAddressData.taxOfficeId.Value != Guid.Empty)
                e["rnt_taxofficeid"] = new EntityReference("rnt_taxoffice", invoiceAddressData.taxOfficeId.Value);
            else
                e["rnt_taxofficeid"] = null;

            //individual
            //todo enums
            if (invoiceAddressData?.invoiceType == (int)rnt_invoice_rnt_invoicetypecode.Individual)
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
            if (invoiceAddressData?.invoiceType == (int)rnt_invoice_rnt_invoicetypecode.Corporate)
            {
                if (contractId.HasValue)
                {
                    ContractRepository contractRepository = new ContractRepository(this.OrgService);
                    var contract = contractRepository.getContractById(contractId.Value, new string[] { "rnt_corporateid" });
                    if (contract.Contains("rnt_corporateid"))
                    {
                        CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
                        var corp = corporateCustomerRepository.getCorporateCustomerById(contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id, new string[] { "emailaddress1", "telephone1" });

                        e["rnt_email"] = corp.Attributes.Contains("emailaddress1") ? corp.GetAttributeValue<string>("emailaddress1") : string.Empty;
                        e["rnt_mobilephone"] = corp.Attributes.Contains("telephone1") ? corp.GetAttributeValue<string>("telephone1") : string.Empty;
                    }
                }
                e["rnt_companyname"] = invoiceAddressData.companyName;
                e["rnt_taxnumber"] = invoiceAddressData.taxNumber;
            }
            else
            {
                e["rnt_companyname"] = null;
                e["rnt_taxnumber"] = null;
            }

            if (!string.IsNullOrEmpty(invoiceAddressData.addressDetail))
                e["rnt_addressdetail"] = invoiceAddressData.addressDetail;

            if (currency.HasValue)
                e["transactioncurrencyid"] = new EntityReference("transactioncurrency", currency.Value);

            if (invoiceId.HasValue)
                e.Id = invoiceId.Value;

            if (invoiceAddressData.invoiceAddressId.HasValue)
                e["rnt_customerinvoiceaddressid"] = Convert.ToString(invoiceAddressData.invoiceAddressId.Value);

            return e;
        }


        public List<CustomerInvoices> getCustomerInvoices(Guid individualCustomerId)
        {
            InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
            var invoices = invoiceRepository.getCustomerInvoices(individualCustomerId);
            invoices = invoices.Where(p => Convert.ToDateTime(p.GetAttributeValue<DateTime>("modifiedon")).Date <= DateTime.UtcNow.Date.AddDays(-1)).ToList();

            //sistemdeki acenta ve broker faturalarını çıkarmak lazım.
            CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
            var corps = corporateCustomerRepository.getAgencyAndBrokerCorporateCustomers(new string[] { "rnt_taxnumber" });

            var taxIds = corps.Select(p => p.GetAttributeValue<string>("rnt_taxnumber")).ToList();
            invoices = invoices.Where(p => !taxIds.Any(z => z == p.GetAttributeValue<string>("rnt_taxnumber"))).ToList();
            List<CustomerInvoices> customerInvoices = new List<CustomerInvoices>();

            foreach (var item in invoices)
            {
                CustomerInvoices c = new CustomerInvoices
                {
                    invoiceNumber = item.GetAttributeValue<string>("rnt_name"),
                    contractNumber = Convert.ToString(item.GetAttributeValue<AliasedValue>("contract.rnt_name").Value),
                    logoInvoiceNumber = item.GetAttributeValue<string>("rnt_logoinvoicenumber"),
                    totalAmount = item.GetAttributeValue<Money>("rnt_totalamount").Value,
                    invoiceType = item.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value,
                    invoiceDate = item.GetAttributeValue<DateTime>("modifiedon"),
                    pnrNumber = item.GetAttributeValue<AliasedValue>("contract.rnt_pnrnumber").Value.ToString(),
                    invoiceTypeName = item.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value == (int)rnt_invoice_rnt_invoicetypecode.Individual ? "Bireysel" : "Kurumsal"
                };
                customerInvoices.Add(c);
            }
            return customerInvoices;
        }
        public CancelInvoiceResponse cancelInvoiceByLogoInvoiceNumber(string logoInvoiceNumber)
        {
            InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
            var invoice = invoiceRepository.getInvoiceByLogoInvoiceNumber(logoInvoiceNumber);
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            if (invoice != null)
            {
                xrmHelper.setState("rnt_invoice", invoice.Id, (int)GlobalEnums.StateCode.Passive, 2); // 2 mean cancelled by logo todo enum
                return new CancelInvoiceResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }

            var message = xrmHelper.GetXmlTagContent(this.UserId, "RecordsNotFound", ErrorMessageXml);
            return new CancelInvoiceResponse
            {
                ResponseResult = ResponseResult.ReturnError(message),
            };
        }

        public GetInvoiceByReservationOrContractResponse getInvoiceByReservationOrContractAsInvoiceAddress(GetInvoiceByReservationOrContractParameter parameter)
        {
            InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
            Entity invoice = null;
            if (parameter.reservationId.HasValue)
                invoice = invoiceRepository.getInvoiceByReservationId(parameter.reservationId.Value);
            else if (parameter.contractId.HasValue)
                invoice = invoiceRepository.getFirstInvoiceByContractId(parameter.contractId.Value);

            if (invoice == null)
            {
                this.Trace("invoice is null");
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContent(this.UserId, "ActiveRecordsNotFound", ErrorMessageXml);
                return new GetInvoiceByReservationOrContractResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message),
                };
            }

            var customerFirstName = invoice.Attributes.Contains("rnt_firstname") ? invoice.GetAttributeValue<string>("rnt_firstname") : string.Empty;
            var customerLastName = invoice.Attributes.Contains("rnt_lastname") ? invoice.GetAttributeValue<string>("rnt_lastname") : string.Empty;
            InvoiceAddressData invoiceAddressData = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoice);

            return new GetInvoiceByReservationOrContractResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess(),
                invoiceAddress = invoiceAddressData
            };
        }

        private bool checkIsInvioceCreateByPickupBranchIdAndFillBranchInfo()
        {
            BranchRepository branchRepository = new BranchRepository(this.OrgService);
            this.documentBranchInfo = branchRepository.getBranchById(this.pickupBranchId);
            var branchType = this.documentBranchInfo.GetAttributeValue<OptionSetValue>("rnt_branchtype").Value;
            return branchType == (int)ClassLibrary._Enums_1033.rnt_BranchType.Office;
        }
        public void prepareInvoiceInformationListParameterForContract(Guid contractId, List<Entity> invoiceItems, List<Entity> contractItems, bool invoiceAfterContractClose)
        {
            var useContactItemInformation = true;
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var contract = contractRepository.getContractById(contractId, new string[] { "rnt_equipmentid", "rnt_automaticinvoiceenabled", "rnt_pnrnumber" });
            if (contract.Contains("rnt_automaticinvoiceenabled") &&
               !contract.GetAttributeValue<bool>("rnt_automaticinvoiceenabled"))
            {
                useContactItemInformation = false;
            }

            this.Trace("invoiceAfterContractClose : " + invoiceAfterContractClose);
            this.Trace("useContactItemInformation : " + useContactItemInformation);

            this.invoiceInformationList = new List<InvoiceInformation>();

            string contractPlate = string.Empty;
            if (contract.Contains("rnt_equipmentid"))
            {
                contractPlate += "-" + contract.GetAttributeValue<EntityReference>("rnt_equipmentid").Name;
            }
            var totalDays = 0;
            var invoiceDays = 0;
            var plateNumber = string.Empty;
            this.Trace("prepareInvoiceInformationListParameterForContract start " + contractPlate);

            //var hgsCode = new ConfigurationBL(this.OrgService).GetConfigurationByName("additionalProduct_HGS");
            //var hgsService = new ConfigurationBL(this.OrgService).GetConfigurationByName("additionalProduct_HGSService");
            //var traffic = new ConfigurationBL(this.OrgService).GetConfigurationByName("additionalProduct_trafficFineProductCode");
            //var trafficService = new ConfigurationBL(this.OrgService).GetConfigurationByName("additionalProduct_trafficFineProductServiceCode");
            foreach (var invoiceItem in invoiceItems)
            {
                if (invoiceItem.GetAttributeValue<EntityReference>("rnt_contractitemid") != null && invoiceItem.GetAttributeValue<EntityReference>("rnt_contractitemid").Id != null)
                {
                    var contractItem = contractItems.Where(p => p.Id == invoiceItem.GetAttributeValue<EntityReference>("rnt_contractitemid").Id).FirstOrDefault();
                    //this.Trace("contractItem" + JsonConvert.SerializeObject(contractItem)) ;
                    if (contractItem != null)
                    {
                        var retrievedContractItem = this.OrgService.Retrieve(contractItem.LogicalName, contractItem.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
                        this.Trace("totalDays" + totalDays);
                        if (totalDays == 0)
                        {
                            ContractHelper contractHelper = new ContractHelper(this.OrgService);
                            totalDays = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(contractItem.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                                                                        contractItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));


                            var lastInvoiceDay = contractItem.Contains("rnt_invoiceduration") ? contractItem.GetAttributeValue<int>("rnt_invoiceduration") : retrievedContractItem.GetAttributeValue<int>("rnt_invoiceduration");
                            if (totalDays > lastInvoiceDay)
                            {
                                invoiceDays = totalDays - lastInvoiceDay;
                            }
                            else
                            {
                                invoiceDays = totalDays;
                            }

                        }

                        var materialCode = string.Empty;
                        string description = string.Empty;
                        var contractItemType = contractItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;
                        if (contractItemType == (int)rnt_contractitem_rnt_itemtypecode.Equipment ||
                            contractItemType == (int)rnt_contractitem_rnt_itemtypecode.PriceDifference)
                        {
                            this.Trace("Equipment start");
                            plateNumber = contractItem.Attributes.Contains("rnt_equipment") ? contractItem.GetAttributeValue<EntityReference>("rnt_equipment").Name : string.Empty;
                            materialCode = contractItem.Attributes.Contains("rnt_equipment") ? contractItem.GetAttributeValue<EntityReference>("rnt_equipment").Name : string.Empty;
                            var customerName = contractItem.Attributes.Contains("rnt_customerid") ? contractItem.GetAttributeValue<EntityReference>("rnt_customerid").Name : string.Empty;
                            var pickupDate = contractItem.GetAttributeValue<DateTime>("rnt_pickupdatetime").AddHours(3).ToString("dd/MM/yyyy");
                            var droppoffDate = contractItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime").AddHours(3).ToString("dd/MM/yyyy");
                            var pickupBranchName = contractItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name;
                            var pnrNumber = contract.Attributes.Contains("rnt_pnrnumber") ? contract.GetAttributeValue<string>("rnt_pnrnumber") : string.Empty;
                            this.Trace("Equipment continues");

                            //var name = contractItem.GetAttributeValue<string>("rnt_name");
                            //var equipmentId = contractItem.GetAttributeValue<EntityReference>("rnt_equipment").Id;
                            //EquipmentRepository equipmentRepository = new EquipmentRepository(this.OrgService);
                            //var equipment = equipmentRepository.getEquipmentByIdByGivenColumns(equipmentId, new string[] { "rnt_product" });
                            if (useContactItemInformation)
                            {
                                description = materialCode + "-" + customerName + "-" + pickupDate + "-" + droppoffDate + "-" + pickupBranchName + "-" + pnrNumber;
                            }
                            else
                            {
                                description = plateNumber + "-" + invoiceItem.GetAttributeValue<string>("rnt_name");
                            }
                            this.Trace("Equipment end : " + description);

                        }
                        else
                        {
                            this.Trace("additional start");
                            //get additional product logo id 
                            var additionalProductId = contractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id;
                            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
                            var additionalProduct = additionalProductRepository.getAdditionalProductById(additionalProductId, new string[] { "rnt_logoid", "rnt_additionalproductcode" });
                            materialCode = additionalProduct.Attributes.Contains("rnt_logoid") ? additionalProduct.GetAttributeValue<string>("rnt_logoid") : string.Empty;
                            var additonalProductCode = additionalProduct.Attributes.Contains("rnt_additionalproductcode") ? additionalProduct.GetAttributeValue<string>("rnt_additionalproductcode") : string.Empty;

                            var name = "";
                            if (useContactItemInformation)
                            {
                                name = contractItem.GetAttributeValue<string>("rnt_name");
                            }
                            else
                            {
                                name = invoiceItem.GetAttributeValue<string>("rnt_name");
                            }
                            //plakayı her türlü ekle. araç değişikliğini düşünmek lazım , su an için çözüm olmayabilir.
                            //01.01.2022 PA
                            if (contract.Contains("rnt_equipmentid"))
                            {
                                name += "-" + contract.GetAttributeValue<EntityReference>("rnt_equipmentid").Name;
                            }

                            if (invoiceAfterContractClose)
                            {
                                if (contractItem.Contains("rnt_customerid"))
                                {
                                    name += "-" + contractItem.GetAttributeValue<EntityReference>("rnt_customerid").Name;
                                }
                                if (contractItem.Contains("rnt_pickupdatetime"))
                                {
                                    name += "-" + contractItem.GetAttributeValue<DateTime>("rnt_pickupdatetime").ToString("dd/MM/yyyy");
                                }
                                if (contractItem.Contains("rnt_dropoffdatetime"))
                                {
                                    name += "-" + contractItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime").ToString("dd/MM/yyyy");
                                }
                                if (contractItem.Contains("rnt_pickupbranchid"))
                                {
                                    name += "-" + contractItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name;
                                }
                                if (contract.Contains("rnt_pnrnumber"))
                                {
                                    name += "-" + contract.GetAttributeValue<string>("rnt_pnrnumber");
                                }
                            }
                            else
                            {

                                if (useContactItemInformation)
                                {
                                    name = contractItem.GetAttributeValue<string>("rnt_name");
                                }
                                else
                                {
                                    name = invoiceItem.GetAttributeValue<string>("rnt_name");
                                }
                            }


                            description = name;
                            this.Trace("additional end : " + description);

                        }
                        var tAmount = decimal.Zero;
                        if (useContactItemInformation)
                        {
                            tAmount = contractItem.GetAttributeValue<Money>("rnt_totalamount").Value;

                        }
                        else
                        {
                            tAmount = invoiceItem.GetAttributeValue<Money>("rnt_totalamount").Value;
                        }
                        this.Trace("tAmount" + tAmount);
                        this.Trace("description : " + description);
                        var netAmount = (tAmount * 100) / (100 + contractItem.GetAttributeValue<decimal>("rnt_taxratio"));
                        netAmount = decimal.Round(netAmount, 4);
                        var vatRate = Convert.ToInt32(contractItem.GetAttributeValue<decimal>("rnt_taxratio"));

                        var materialDesc = string.Empty;
                        if (useContactItemInformation)
                        {
                            materialDesc = contractPlate + " - " + (contractItem.Attributes.Contains("rnt_name") ? contractItem.GetAttributeValue<string>("rnt_name") : string.Empty);
                        }
                        else
                        {
                            materialDesc = contractPlate + " - " + (invoiceItem.Attributes.Contains("rnt_name") ? invoiceItem.GetAttributeValue<string>("rnt_name") : string.Empty);

                        }
                        this.Trace("materialDesc : " + description);

                        InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
                        var invoice = invoiceRepository.getInvoiceById(invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id);

                        ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                        var t = configurationBL.GetConfigurationByName("YOLCUTaxNumber");
                        //
                        if (invoice.GetAttributeValue<string>("rnt_taxnumber") == t)
                        {
                            description += " " + configurationBL.GetConfigurationByName("YOLCUdefinition");
                        }
                        var newdescription = contractPlate + " " + description;
                        this.Trace("materialDesc " + materialDesc);
                        this.Trace("newdescription " + newdescription);

                        //Araç değilse diğer kalemlerde gün değeri yazmayacaktır.
                        string unitCode = string.Empty;
                        int quantity = 0;
                        if (contractItemType == (int)rnt_contractitem_rnt_itemtypecode.Equipment ||
                            contractItemType == (int)rnt_contractitem_rnt_itemtypecode.PriceDifference)
                        {
                            unitCode = "GÜN";
                            quantity = invoiceDays;

                        }
                        else
                        {
                            unitCode = "ADET";
                            quantity = 1;
                        }

                        this.invoiceInformationList.Add(new InvoiceInformation
                        {
                            description = newdescription,
                            metarialDescription = materialDesc,
                            metarialCode = materialCode,
                            plateNumber = plateNumber,
                            unitPrice = Convert.ToDouble(netAmount),
                            type = 4,//default
                            vatRate = vatRate,
                            vatExceptCode = vatRate == 0 ? StaticHelper._350 : string.Empty,
                            vatExceptReason = vatRate == 0 ? StaticHelper._others : string.Empty,
                            unitCode = unitCode,
                            quantity = quantity
                        });

                        this.Trace("materialDesc");

                        Entity updateContratcItem = new Entity(contractItem.LogicalName, contractItem.Id);
                        updateContratcItem.Attributes["rnt_invoiceduration"] = totalDays;
                        this.OrgService.Update(updateContratcItem);
                    }
                }
                // this code for get contract item informations

            }
            this.Trace("prepareInvoiceInformationListParameterForContract end");
        }
        private void prepareInvoiceInformationListParameterForReservation(Entity cancellationFeeItem)
        {
            var invoiceList = new List<InvoiceInformation>();
            var additionalProductId = cancellationFeeItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id;

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var additionalProduct = additionalProductRepository.getAdditionalProductById(additionalProductId, new string[] { "rnt_logoid" });
            var logoId = additionalProduct.Attributes.Contains("rnt_logoid") ? additionalProduct.GetAttributeValue<string>("rnt_logoid") : string.Empty;

            this.invoiceInformationList = new List<InvoiceInformation>();
            var netAmount = (cancellationFeeItem.GetAttributeValue<Money>("rnt_totalamount").Value * 100) / (100 + Convert.ToInt32(cancellationFeeItem.GetAttributeValue<decimal>("rnt_taxratio")));
            this.invoiceInformationList.Add(new InvoiceInformation
            {
                description = string.Empty,
                metarialDescription = cancellationFeeItem.Attributes.Contains("rnt_name") ? cancellationFeeItem.GetAttributeValue<string>("rnt_name") : string.Empty,
                metarialCode = logoId,
                unitPrice = Convert.ToDouble(netAmount),
                type = 4,//default
                vatRate = Convert.ToInt32(cancellationFeeItem.GetAttributeValue<decimal>("rnt_taxratio")),
                unitCode = "GÜN",
                quantity = cancellationFeeItem.GetAttributeValue<int>("rnt_reservationduration")
            });
        }
        public void getDocumentInfo(Guid? contractId, Guid? reservationId)
        {
            if (contractId.HasValue)
            {
                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                var contract = contractRepository.getContractById(contractId.Value, new string[] { "rnt_pickupbranchid",
                                                                                                    "statuscode",
                                                                                                    "rnt_totalamount",
                                                                                                    "rnt_contractnumber",
                                                                                                    "rnt_reservationid",
                                                                                                    "rnt_pnrnumber"});
                this.documentNumber = contract.GetAttributeValue<string>("rnt_contractnumber") + " - " + contract.GetAttributeValue<string>("rnt_pnrnumber");
                this.Trace($"getDocumentInfo rnt_pnrnumber {contract.GetAttributeValue<string>("rnt_pnrnumber")}");
                this.documentStatus = contract.GetAttributeValue<OptionSetValue>("statuscode").Value;
                this.documentTotalAmount = contract.GetAttributeValue<Money>("rnt_totalamount").Value;
                this.pickupBranchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id;
                if (contract.Contains("rnt_reservationid"))
                {
                    ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                    var reservation = reservationRepository.getReservationById(contract.GetAttributeValue<EntityReference>("rnt_reservationid").Id,
                                                                                new string[] { "rnt_referencenumber" });
                    this.documentNumber = this.documentNumber + " - " + reservation.GetAttributeValue<string>("rnt_referencenumber");
                }

            }
            else
            {
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                var reservation = reservationRepository.getReservationById(reservationId.Value, new string[] { "rnt_pickupbranchid", "statuscode", "rnt_reservationnumber", "rnt_pnrnumber" });

                this.documentNumber = reservation.GetAttributeValue<string>("rnt_reservationnumber") + " - " + reservation.GetAttributeValue<string>("rnt_pnrnumber");
                this.documentStatus = reservation.GetAttributeValue<OptionSetValue>("statuscode").Value;
                this.pickupBranchId = reservation.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id;
            }

            this.Trace($"getDocumentInfo this.documentNumber {this.documentNumber}");
        }
        public void setInvoiceDatetoInvoiceItem(List<Entity> invoiceItems)
        {
            foreach (var item in invoiceItems)
            {
                Entity e = new Entity("rnt_invoiceitem");
                e["rnt_invoicedate"] = DateTime.Now.AddMinutes(StaticHelper.offset);
                e.Id = item.Id;
                this.OrgService.Update(e);
            }

        }
        public void updateInvoiceItemsStatusByInvoiceHeader(List<Entity> invoiceItems, int statusCode)
        {
            InvoiceItemBL invoiceItemBL = new InvoiceItemBL(this.OrgService);
            ContractItemBL contractItemBL = new ContractItemBL(this.OrgService);

            foreach (var item in invoiceItems)
            {
                var itemStatusCode = (int)ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.Draft;
                if (statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.DealerInvoicing || statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegratedWithLogo)
                {
                    Entity updateInvoiceItem = new Entity(item.LogicalName,item.Id);
                    var invoiceDate = DateTime.UtcNow;
                    if (item.Attributes.Contains("rnt_contractitemid"))
                    {
                        var contractItemId = item.GetAttributeValue<EntityReference>("rnt_contractitemid").Id;
                        contractItemBL.updateContractItemInvoiceDate(contractItemId, invoiceDate);
                        ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
                        var contractItem = contractItemRepository.getContractItemId(contractItemId);
                        if (contractItem.Contains("rnt_additionalproductid") && item.Contains("rnt_totalamount"))
                        {
                            AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(this.OrgService);
                            var product = additionalProductHelper.getFineAdditionalProduct();

                            if (product.Id == contractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id)
                            {
                                updateInvoiceItem["rnt_netamount"] = item.GetAttributeValue<Money>("rnt_totalamount").Value;
                            }
                            else if (item.Contains("rnt_totalamount"))
                            {
                                updateInvoiceItem["rnt_netamount"] = item.GetAttributeValue<Money>("rnt_totalamount").Value / 1.18M;
                            }

                        }
                        else if (item.Contains("rnt_totalamount"))
                        {
                            updateInvoiceItem["rnt_netamount"] = item.GetAttributeValue<Money>("rnt_totalamount").Value / 1.18M;
                        }
                    }
                    updateInvoiceItem["rnt_invoicedate"] = DateTime.Now.AddMinutes(StaticHelper.offset);
                    this.Trace("updating contract item invoice date : " + invoiceDate);
                    this.OrgService.Update(updateInvoiceItem);
                }
                if (statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegratedWithLogo)
                    itemStatusCode = (int)ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.IntegratedWithLogo;
                else if (statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegrationError)
                    itemStatusCode = (int)ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.IntegrationError;
                else if (statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.DealerInvoicing)
                    itemStatusCode = (int)ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.DealerInvoicing;

                invoiceItemBL.updateInvoiceItemStatus(item.Id, (int)GlobalEnums.StateCode.Active, itemStatusCode);
            }
        }
        public void updateInvoiceItemsStatusByInvoiceHeader(Guid invoiceId, int statusCode)
        {
            InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
            var invoiceItems = invoiceItemRepository.getInvoiceItemsByInvoiceIdAllStatus(invoiceId);
            this.Trace("invoiceItems count : " + invoiceItems.Count);
            this.Trace("invoice status " + statusCode);
            InvoiceItemBL invoiceItemBL = new InvoiceItemBL(this.OrgService);
            foreach (var item in invoiceItems)
            {
                try
                {
                    var itemStatusCode = (int)ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.Draft;
                    if (statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegratedWithLogo)
                        itemStatusCode = (int)ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.IntegratedWithLogo;
                    else if (statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegrationError)
                        itemStatusCode = (int)ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.IntegrationError;
                    else if (statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.Completed)
                        itemStatusCode = (int)ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.Completed;
                    else if (statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.InternalError)
                        itemStatusCode = (int)ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.InternalError;
                    else if (statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.DealerInvoicing)
                        itemStatusCode = (int)ClassLibrary._Enums_1033.rnt_invoiceitem_StatusCode.DealerInvoicing;

                    invoiceItemBL.updateInvoiceItemStatus(item.Id, (int)GlobalEnums.StateCode.Active, itemStatusCode);
                }
                catch (Exception ex)
                {

                    this.Trace("invoice item status change error : " + ex.Message);
                    continue;
                }
            }

            if (statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.IntegratedWithLogo || statusCode == (int)ClassLibrary._Enums_1033.rnt_invoice_StatusCode.DealerInvoicing)
            {
                setInvoiceDatetoInvoiceItem(invoiceItems);
            }
        }
        public CurrentAccountCodeParameter prepareCurrentAccountCodeParameter(Entity invoice)
        {
            var firstName = string.Empty;
            var lastName = string.Empty;
            var companyName = string.Empty;
            var accountCodeTitle = string.Empty;
            var corporateType = string.Empty;
            List<string> paymentMethods = new List<string>();

            var invoiceType = invoice.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value;
            if (invoiceType == (int)ClassLibrary._Enums_1033.rnt_invoice_rnt_invoicetypecode.Individual)
            {
                firstName = invoice.Attributes.Contains("rnt_firstname") ? invoice.GetAttributeValue<string>("rnt_firstname") : string.Empty;
                lastName = invoice.Attributes.Contains("rnt_lastname") ? invoice.GetAttributeValue<string>("rnt_lastname") : string.Empty;
                accountCodeTitle = firstName + " " + lastName;
            }
            else
            {
                companyName = invoice.Attributes.Contains("rnt_companyname") ? invoice.GetAttributeValue<string>("rnt_companyname") : string.Empty;
                accountCodeTitle = companyName;

                if (invoice.Attributes.Contains("rnt_taxnumber"))
                {
                    CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
                    var corp = corporateCustomerRepository.getCorporateCustomersByTaxNumber(invoice.GetAttributeValue<string>("rnt_taxnumber"));
                    if (corp != null)
                    {
                        if (corp.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value == (int)rnt_AccountTypeCode.Agency)
                        {
                            corporateType = "Acenta";
                        }
                        else if (corp.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value == (int)rnt_AccountTypeCode.Broker)
                        {
                            corporateType = "Broker";
                        }
                        else if (corp.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value == (int)rnt_AccountTypeCode.Corporate)
                        {
                            corporateType = "Kurumsal";
                        }
                        var collection = corp.GetAttributeValue<OptionSetValueCollection>("rnt_paymentmethodcode");

                        foreach (var paymentMet in collection)
                        {
                            if ((int)rnt_PaymentMethodCode.Current == paymentMet.Value)
                            {
                                paymentMethods.Add("CR");
                            }
                            else if ((int)rnt_PaymentMethodCode.CreditCard == paymentMet.Value)
                            {
                                paymentMethods.Add("KKS");
                            }
                            else if ((int)rnt_PaymentMethodCode.FullCredit == paymentMet.Value)
                            {
                                paymentMethods.Add("FCRD");
                            }
                            else if ((int)rnt_PaymentMethodCode.LimitedCredit == paymentMet.Value)
                            {
                                paymentMethods.Add("LCRD");
                            }
                            else if ((int)rnt_PaymentMethodCode.PayBroker == paymentMet.Value)
                            {
                                paymentMethods.Add("BR");
                            }
                        }

                    }

                }
            }
            var emailAddress = invoice.Attributes.Contains("rnt_email") ? invoice.GetAttributeValue<string>("rnt_email") : string.Empty;
            // get current account code from logo
            return new CurrentAccountCodeParameter
            {
                corporateType = corporateType,
                paymentMethods = paymentMethods,
                tckn = invoice.Attributes.Contains("rnt_govermentid") ? invoice.GetAttributeValue<string>("rnt_govermentid") : string.Empty,
                taxNo = invoice.Attributes.Contains("rnt_taxnumber") ? invoice.GetAttributeValue<string>("rnt_taxnumber") : string.Empty,
                customerFirstName = firstName,
                customerLastName = lastName,
                title = accountCodeTitle,
                address = invoice.GetAttributeValue<string>("rnt_addressdetail"),
                address2 = string.Empty,
                mobilePhone = invoice.Attributes.Contains("rnt_mobilephone") ? invoice.GetAttributeValue<string>("rnt_mobilephone") : string.Empty,
                email = emailAddress,
                city = invoice.Attributes.Contains("rnt_cityid") ? invoice.GetAttributeValue<EntityReference>("rnt_cityid").Name : string.Empty,
                town = invoice.Attributes.Contains("rnt_districtid") ? invoice.GetAttributeValue<EntityReference>("rnt_districtid").Name : string.Empty,
                country = invoice.GetAttributeValue<EntityReference>("rnt_countryid").Name,
                taxOffice = invoice.Attributes.Contains("rnt_taxofficeid") ? invoice.GetAttributeValue<EntityReference>("rnt_taxofficeid").Name : string.Empty,
                einvoiceEmail = emailAddress
            };
        }
        public SalesInvoiceParameter prepareSalesInvoiceParameter(Entity invoice)
        {
            return new SalesInvoiceParameter
            {
                currentAccountCode = this.currentAccountCode,
                documentInvoiceNo = invoice.Attributes.Contains("rnt_invoicenumber") ? invoice.GetAttributeValue<string>("rnt_invoicenumber") : string.Empty,
                documentNumber = this.documentNumber,
                invoiceDate = DateTime.Now.AddHours(3).ToString("dd.MM.yyyy"),
                warehouse = Convert.ToInt32(this.documentBranchInfo.GetAttributeValue<string>("rnt_logowarehouse")),
                notes = null,
                tckn = invoice.Attributes.Contains("rnt_govermentid") ? invoice.GetAttributeValue<string>("rnt_govermentid") : string.Empty,
                taxNo = invoice.Attributes.Contains("rnt_taxnumber") ? invoice.GetAttributeValue<string>("rnt_taxnumber") : string.Empty,
                division = Convert.ToInt32(documentBranchInfo.GetAttributeValue<string>("rnt_logodivision")),
                projectCode = documentBranchInfo.GetAttributeValue<string>("rnt_logoprojectcode"),
                invoiceInformationList = this.invoiceInformationList,
                currentAccountCodeShpm = documentBranchInfo.Attributes.Contains("rnt_logoaccountnumber") ?
                                                       documentBranchInfo.GetAttributeValue<string>("rnt_logoaccountnumber") : string.Empty
            };
        }
        public Entity getReservationCancelletionFeeItem(Guid reservationId)
        {
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var reservationItems = reservationItemRepository.getCompletedReservationItemsWithGivenColumns(reservationId, new string[] { "rnt_taxratio",
                                                                                                                                        "rnt_reservationduration",
                                                                                                                                        "rnt_equipmentid",
                                                                                                                                        "rnt_totalamount",
                                                                                                                                        "rnt_netamount",
                                                                                                                                        "rnt_name",
                                                                                                                                        "rnt_additionalproductid" });
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var productCode = configurationRepository.GetConfigurationByKey("additionalProduct_cancellationFeeCode");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var cancellationFeeAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(productCode);
            return reservationItems.Where(item => item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id == cancellationFeeAdditionalProduct.Id).FirstOrDefault();
        }
        public CreateInvoiceWithLogoResponse handleCreateInvoiceWithLogoProcessForContract(Guid? contractId, int langId, Guid? invoiceId)
        {
            // execute cancelled and completed contracts 
            this.Trace("contractRef : " + contractId);
            //fatura içinden tetiklenmiyorsa , sözleşme kapandığında buraya düşüyorsa , sistem otomati fatura kesmesin!
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var c = contractRepository.getContractById(contractId.Value, new string[] { "rnt_automaticinvoiceenabled" });
            if (c.Contains("rnt_automaticinvoiceenabled") &&
               !c.GetAttributeValue<bool>("rnt_automaticinvoiceenabled") &&
               !invoiceId.HasValue)
            {
                this.Trace("automatic invoice is false: ");
                return new CreateInvoiceWithLogoResponse();
            }


            this.getDocumentInfo(contractId, null);
            BranchRepository branchRepository = new BranchRepository(this.OrgService);
            this.documentBranchInfo = branchRepository.getBranchById(this.pickupBranchId);

            InvoiceCreationValidation invoiceCreationValidation = new InvoiceCreationValidation(this.OrgService);
            var validationResponse = invoiceCreationValidation.checkContractStatus(this.documentStatus, langId);

            if (!validationResponse.ResponseResult.Result)
                return validationResponse;

            // check contract branch 
            //if (this.checkIsInvioceCreateByPickupBranchIdAndFillBranchInfo())
            //{
            RntCar.BusinessLibrary.ContractItemRepository contractItemRepository = new RntCar.BusinessLibrary.ContractItemRepository(this.OrgService);
            this.Trace("ContractItemRepository obj created");

            var contractItems = contractItemRepository.getCompletedContractItemsByContractIdWithGivenColmuns(contractId.Value, new string[] { "rnt_taxratio",
                                                                                                                                                  "rnt_contractduration",
                                                                                                                                                  "rnt_equipment",
                                                                                                                                                  "rnt_totalamount",
                                                                                                                                                  "rnt_netamount",
                                                                                                                                                  "rnt_name",
                                                                                                                                                  "rnt_customerid",
                                                                                                                                                  "rnt_pickupdatetime",
                                                                                                                                                  "rnt_equipment",
                                                                                                                                                  "rnt_dropoffdatetime",
                                                                                                                                                  "rnt_pickupbranchid",
                                                                                                                                                  "rnt_billingtype",
                                                                                                                                                  "rnt_additionalproductid",
                                                                                                                                                  "rnt_itemtypecode"});
            this.Trace("items count before removed " + contractItems.Count);
            contractItems = contractItems.Where(p => p.GetAttributeValue<Money>("rnt_totalamount").Value > decimal.Zero).ToList();
            this.Trace("items count after removed " + contractItems.Count);
            //eğer faturanın içinden tetikleniyorsa
            if (invoiceId != null)
            {
                return logoOperations(contractItems, contractId, invoiceId, langId, true, true);
            }
            else
            {
                BranchHelper branchHelper = new BranchHelper(this.OrgService);
                var branchInfo = branchHelper.getBranchType(null, contractId.Value);
                var contract = contractRepository.getContractById(contractId.Value, new string[] { "rnt_paymentmethodcode", "rnt_contracttypecode" });
                var paymentMethodCode = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
                var contractType = contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value;
                this.Trace("contract rnt_paymentmethodcode : " + paymentMethodCode);
                this.Trace("branchInfo : " + JsonConvert.SerializeObject(branchInfo));

                if (paymentMethodCode == (int)rnt_PaymentMethodCode.LimitedCredit)
                {
                    var individualItems = contractItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_billingtype").Value == (int)rnt_BillingTypeCode.Individual).ToList();
                    var corpItems = contractItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_billingtype").Value == (int)rnt_BillingTypeCode.Corporate).ToList();

                    if (individualItems.Count > 0 && branchInfo.branchType == (int)rnt_BranchType.Office)
                    {
                        this.Trace("limited credit office scenario");
                        InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
                        var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemId(individualItems.FirstOrDefault().Id);

                        logoOperations(individualItems, contractId, invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id, langId, false, true);
                    }
                    if (individualItems.Count > 0 && branchInfo.branchType == (int)rnt_BranchType.Franchise)
                    {
                        this.Trace("limited credit Franchise scenario");
                        InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
                        var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemId(individualItems.FirstOrDefault().Id);
                        var items = invoiceItemRepository.getInvoiceItemsByInvoiceId(invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id);
                        this.updateInvoiceStatus(invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id, (int)rnt_invoice_StatusCode.DealerInvoicing);
                        this.updateInvoiceItemsStatusByInvoiceHeader(items, (int)rnt_invoice_StatusCode.DealerInvoicing);
                        this.setInvoiceDatetoInvoiceItem(items);

                    }
                    if (corpItems.Count > 0)
                    {
                        InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
                        var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemId(corpItems.FirstOrDefault().Id);

                        return logoOperations(corpItems, contractId, invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id, langId, false, true);
                    }
                    else
                    {
                        return new CreateInvoiceWithLogoResponse();
                    }
                }

                else if (paymentMethodCode == (int)rnt_PaymentMethodCode.PayBroker)
                {
                    var corporateItems = contractItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_billingtype").Value == (int)rnt_BillingTypeCode.Corporate).ToList();
                    var individualItems = contractItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_billingtype").Value == (int)rnt_BillingTypeCode.Individual).ToList();

                    if (corporateItems.Count > 0)
                    {
                        InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
                        var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemId(corporateItems.FirstOrDefault().Id);
                        logoOperations(corporateItems, contractId, invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id, langId, false, true);
                    }

                    if (individualItems.Count > 0 && branchInfo.branchType == (int)rnt_BranchType.Office)
                    {
                        this.Trace("PayBroker office scenario");
                        var sum = individualItems.Sum(p => p.GetAttributeValue<Money>("rnt_totalamount").Value);
                        this.Trace("sum : " + sum);
                        if (sum > 0)
                        {
                            InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
                            var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemId(individualItems.FirstOrDefault().Id);
                            return logoOperations(individualItems, contractId, invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id, langId, false, true);
                        }

                        return new CreateInvoiceWithLogoResponse();
                    }
                    if (individualItems.Count > 0 && branchInfo.branchType == (int)rnt_BranchType.Franchise)
                    {
                        this.Trace("PayBroker Franchise scenario");
                        InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
                        var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemId(individualItems.FirstOrDefault().Id);
                        var items = invoiceItemRepository.getInvoiceItemsByInvoiceId(invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id);
                        this.updateInvoiceStatus(invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id, (int)rnt_invoice_StatusCode.DealerInvoicing);
                        this.updateInvoiceItemsStatusByInvoiceHeader(items, (int)rnt_invoice_StatusCode.DealerInvoicing);
                        this.setInvoiceDatetoInvoiceItem(items);
                    }

                    else
                        return new CreateInvoiceWithLogoResponse();
                }
                else
                {
                    if (branchInfo.branchType == (int)rnt_BranchType.Office)
                    {
                        this.Trace("office scenario");
                        return logoOperations(contractItems,
                                              contractId,
                                              invoiceId,
                                              langId,
                                              false,
                                              paymentMethodCode == (int)rnt_PaymentMethodCode.Current ||
                                              paymentMethodCode == (int)rnt_PaymentMethodCode.FullCredit
                                              ? true : false);
                    }
                    else if (branchInfo.branchType == (int)rnt_BranchType.Franchise)
                    {
                        if (contractType == (int)rnt_ReservationTypeCode.Kurumsal ||
                            paymentMethodCode == (int)rnt_PaymentMethodCode.FullCredit)
                        {
                            this.Trace("franchise scenario kurumsal veya fullcredit");

                            return logoOperations(contractItems,
                                                  contractId,
                                                  invoiceId,
                                                  langId,
                                                  false,
                                                  paymentMethodCode == (int)rnt_PaymentMethodCode.Current ||
                                                  paymentMethodCode == (int)rnt_PaymentMethodCode.FullCredit
                                                  ? true : false);
                        }
                        else
                        {
                            this.Trace("franchise scenario other");
                            if (!invoiceId.HasValue)
                            {
                                InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
                                invoiceId = invoiceRepository.getFirstActiveInvoiceByContractId(contractId.Value).FirstOrDefault()?.Id;
                            }
                            InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
                            var items = invoiceItemRepository.getInvoiceItemsByInvoiceId(invoiceId.Value);
                            this.updateInvoiceStatus(invoiceId.Value, (int)rnt_invoice_StatusCode.DealerInvoicing);
                            this.updateInvoiceItemsStatusByInvoiceHeader(items, (int)rnt_invoice_StatusCode.DealerInvoicing);
                            this.setInvoiceDatetoInvoiceItem(items);
                        }

                    }
                }
            }

            //}
            //else
            //{
            //    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            //    var message = xrmHelper.GetXmlTagContentByGivenLangId("BranchTypeError", langId, this.invoiceXmlPath);
            //    return new CreateInvoiceWithLogoResponse
            //    {
            //        ResponseResult = ResponseResult.ReturnError(message)
            //    };
            //    //invoice can only be created for office-type branches
            //}
            return new CreateInvoiceWithLogoResponse();
        }
        public CreateInvoiceWithLogoResponse handleCreateInvoiceWithLogoProcessForReservation(Guid? reservationId, int langId)
        {
            // execute only cancelled reservations
            this.Trace("reservationref : " + reservationId.Value);
            this.getDocumentInfo(null, reservationId);

            BranchRepository branchRepository = new BranchRepository(this.OrgService);
            this.documentBranchInfo = branchRepository.getBranchById(this.pickupBranchId);

            InvoiceBL invoiceBL = new InvoiceBL(this.OrgService);
            InvoiceCreationValidation invoiceCreationValidation = new InvoiceCreationValidation(this.OrgService);
            var validationResponse = invoiceCreationValidation.checkReservationStatus(this.documentStatus, langId);
            if (!validationResponse.ResponseResult.Result)
                return validationResponse;

            //if (this.checkIsInvioceCreateByPickupBranchIdAndFillBranchInfo())
            //{
            this.Trace("branch validation ok");
            // cancelled reservation means there is only one completed item (cancellation fee)

            //this.Trace("cancellation fee item id : " + cancellationFeeItem.Id);
            this.Trace("getting cancellation fee item");
            var cancellationFeeItem = this.getReservationCancelletionFeeItem(reservationId.Value);

            validationResponse = invoiceCreationValidation.checkReservationHasCancellationFeeItem(cancellationFeeItem, langId);
            if (!validationResponse.ResponseResult.Result)
                return validationResponse;

            // get reservatino invoice 
            InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
            var invoice = invoiceRepository.getInvoiceByReservationId(reservationId.Value);

            validationResponse = invoiceCreationValidation.checkReservationHasActiveOrFaultyInvoice(invoice, langId);
            if (!validationResponse.ResponseResult.Result)
                return validationResponse;

            validationResponse = invoiceCreationValidation.checkTaxNumberFormat(invoice, langId);
            if (!validationResponse.ResponseResult.Result)
                return validationResponse;

            LogoHelper logoHelper = new LogoHelper(this.OrgService, this.TracingService);
            this.Trace("getting login info");
            logoHelper.connect();
            var currentAccountCodeParameter = this.prepareCurrentAccountCodeParameter(invoice);
            BranchHelper branchHelper = new BranchHelper(this.OrgService);
            var branchInfo = branchHelper.getBranchType(reservationId.Value, null);
            this.Trace("branchInfo : " + JsonConvert.SerializeObject(branchInfo));

            if (branchInfo.branchType == (int)rnt_BranchType.Office)
            {
                this.Trace("prepare getting current account code parameter");
                this.currentAccountCode = logoHelper.currentAccountCode(currentAccountCodeParameter);
            }
            else
            {
                invoiceBL.updateInvoiceStatus(invoice.Id, (int)rnt_invoice_StatusCode.DealerInvoicing);
                this.updateInvoiceItemsStatusByInvoiceHeader(new List<Entity> { cancellationFeeItem }, (int)rnt_invoice_StatusCode.DealerInvoicing);

                setInvoiceDatetoInvoiceItem(new List<Entity>() { cancellationFeeItem });
                return new CreateInvoiceWithLogoResponse();
            }

            var salesInvoiceParameter = new SalesInvoiceParameter();
            var salesInvoceResponse = new SalesInvoiceResponse
            {
                ResponseResult = ResponseResult.ReturnError("Current account code is null"),
            };

            var statusCode = (int)rnt_invoice_StatusCode.IntegrationError;
            if (!string.IsNullOrEmpty(this.currentAccountCode))
            {

                this.prepareInvoiceInformationListParameterForReservation(cancellationFeeItem);
                this.Trace("created invoiceItem list for logo");

                salesInvoiceParameter = this.prepareSalesInvoiceParameter(invoice);
                this.Trace("params : " + JsonConvert.SerializeObject(salesInvoiceParameter));
                this.Trace("preapre sales invoice parameter");

                salesInvoceResponse = logoHelper.salesInvoice(salesInvoiceParameter);
                this.Trace("response : " + JsonConvert.SerializeObject(salesInvoceResponse));
                this.Trace("response ExceptionDetail: " + salesInvoceResponse.ResponseResult.ExceptionDetail);
                statusCode = salesInvoceResponse.ResponseResult.Result ? (int)rnt_invoice_StatusCode.IntegratedWithLogo : (int)rnt_invoice_StatusCode.IntegrationError;
                if (statusCode == (int)rnt_invoice_StatusCode.IntegratedWithLogo)
                {
                    setInvoiceDatetoInvoiceItem(new List<Entity>() { cancellationFeeItem });
                }
            }
            // update invoice by all service parameters and sales inovice response

            invoiceBL.updateInvoiceWithServicesParameters(invoice.Id,
                                                          salesInvoceResponse.salesInvoices?.invoiceNumber,
                                                          statusCode, //mean integrated with logo  todo enum
                                                          JsonConvert.SerializeObject(currentAccountCodeParameter),
                                                          JsonConvert.SerializeObject(this.currentAccountCode),
                                                          JsonConvert.SerializeObject(salesInvoiceParameter),
                                                          JsonConvert.SerializeObject(salesInvoceResponse),
                                                          logoHelper.getCurrentFirm());

            InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);
            var invItem = invoiceItemRepository.getInvoiceItemsByReservationItemId(cancellationFeeItem.Id);
            this.Trace("updateInvoiceItemsStatusByInvoiceHeader start");
            this.updateInvoiceItemsStatusByInvoiceHeader(new List<Entity> { invItem }, statusCode);
            this.Trace("updateInvoiceItemsStatusByInvoiceHeader end");

            if (!salesInvoceResponse.ResponseResult.Result)
            {
                return new CreateInvoiceWithLogoResponse
                {
                    ResponseResult = ResponseResult.ReturnError(salesInvoceResponse.ResponseResult.ExceptionDetail),
                };
            }

            return new CreateInvoiceWithLogoResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
            //}
            //else
            //{
            //    this.Trace("branch validation is not ok");
            //    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            //    var message = xrmHelper.GetXmlTagContentByGivenLangId("BranchTypeError", langId, this.invoiceXmlPath);
            //    return new CreateInvoiceWithLogoResponse
            //    {
            //        ResponseResult = ResponseResult.ReturnError(message)
            //    };
            //    //invoice can only be created for office-type branches
            //}
        }

        public CreateInvoiceWithLogoResponse logoOperations(List<Entity> contractItems, Guid? contractId, Guid? invoiceId, int langId, bool triggerAfterContractClose, bool passValidation = false, bool useLocalIP = false)
        {
            InvoiceCreationValidation invoiceCreationValidation = new InvoiceCreationValidation(this.OrgService);
            this.Trace("getting contract items : " + contractItems.Count());
            // check contract status code 
            // if contact cancelled, check cancelled item fee amount
            var validationResponse = invoiceCreationValidation.checkContractStatusForCancellation(contractItems, this.documentStatus, langId);
            if (!validationResponse.ResponseResult.Result)
                return validationResponse;

            InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(this.OrgService);

            InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);

            var invoice = new Entity();
            if (!invoiceId.HasValue)
                invoice = invoiceRepository.getFirstAvailableInvoiceByContractId(contractId.Value).FirstOrDefault();
            else
            {
                invoice = invoiceRepository.getInvoiceById(invoiceId.Value);
            }

            // check contract header and invoce headers total amounts
            var invoicesTotalAmount = invoice.GetAttributeValue<Money>("rnt_totalamount").Value;
            if (!invoiceId.HasValue && !passValidation)
            {
                // contract items total amount must be eq to invoce headers total amount 
                validationResponse = invoiceCreationValidation.checkDocumentTotalAmount(this.documentTotalAmount, invoicesTotalAmount, langId);
                if (!validationResponse.ResponseResult.Result)
                    return validationResponse;
            }
            LogoHelper logoHelper = new LogoHelper(this.OrgService, this.TracingService, useLocalIP);
            this.Trace("getting login info");

            logoHelper.connect();

            var currentAccountCodeParameter = this.prepareCurrentAccountCodeParameter(invoice);
            this.Trace("prepare getting current account code parameter");
            this.currentAccountCode = logoHelper.currentAccountCode(currentAccountCodeParameter);
            this.Trace("getting current account code : " + this.currentAccountCode);

            // get invoice items by invoce for salesinvoce
            var invoiceItems = invoiceItemRepository.getInvoiceItemsByInvoiceId(invoice.Id);
            this.Trace("invoice items length : " + invoiceItems.Count);


            //CH:01.05.2022 Closed
            //this.updateInvoiceStatus(invoiceId.Value, (int)rnt_invoice_StatusCode.SendingToLogo);

            var salesInvoiceParameter = new SalesInvoiceParameter();
            var salesInvoceResponse = new SalesInvoiceResponse
            {
                ResponseResult = ResponseResult.ReturnError("Current account code is null"),
            };
            var statusCode = (int)rnt_invoice_StatusCode.IntegrationError;


            if (!string.IsNullOrEmpty(this.currentAccountCode))
            {
                this.Trace("CurrentAcccountCode");
                // prepare invoice information for salesinvoce
                this.prepareInvoiceInformationListParameterForContract(contractId.Value, invoiceItems, contractItems, triggerAfterContractClose);

                if (this.invoiceInformationList.Count == 0)
                {
                    this.updateInvoiceItemsStatusByInvoiceHeader(invoiceItems, (int)rnt_invoice_StatusCode.InternalError);
                    this.Trace("invoiceInformationList  count == 0");
                    return new CreateInvoiceWithLogoResponse
                    {
                        ResponseResult = ResponseResult.ReturnError("invoiceInformationList  count == 0")
                    };
                }

                var quantity = invoiceInformationList.FirstOrDefault()?.quantity;
                if (quantity.HasValue)
                {
                    var sum = invoiceInformationList.Sum(p => p.unitPrice);
                    this.Trace("logo invoice unit price total amount : " + sum);
                    this.Trace("total amount send to logo : " + sum * quantity.Value);

                }

                this.Trace("created invoiceItem list for logo");

                salesInvoiceParameter = this.prepareSalesInvoiceParameter(invoice);
                this.Trace("params : " + JsonConvert.SerializeObject(salesInvoiceParameter));
                this.Trace("preapre sales invoice parameter");
                salesInvoceResponse = logoHelper.salesInvoice(salesInvoiceParameter);
                this.Trace("response : " + salesInvoceResponse.ResponseResult.Result);
                this.Trace("response : " + salesInvoceResponse.ResponseResult.ExceptionDetail);
                this.Trace("salesInvoceResponse: " + JsonConvert.SerializeObject(salesInvoceResponse));
                statusCode = salesInvoceResponse.ResponseResult.Result ? (int)rnt_invoice_StatusCode.IntegratedWithLogo :
                                                                         (int)rnt_invoice_StatusCode.IntegrationError;
                if (statusCode == (int)rnt_invoice_StatusCode.IntegratedWithLogo)
                {
                    setInvoiceDatetoInvoiceItem(invoiceItems);
                }
            }
            this.Trace("updateInvoiceWithServicesParameters");
            // update invoce record with all service parameters and sales invoice response
            this.updateInvoiceWithServicesParameters(invoice.Id,
                                                     salesInvoceResponse.salesInvoices?.invoiceNumber,
                                                     statusCode,
                                                     JsonConvert.SerializeObject(currentAccountCodeParameter),
                                                     JsonConvert.SerializeObject(currentAccountCode),
                                                     JsonConvert.SerializeObject(salesInvoiceParameter),
                                                     JsonConvert.SerializeObject(salesInvoceResponse),
                                                     logoHelper.getCurrentFirm());

            this.Trace("updateInvoiceItemsStatusByInvoiceHeader start");
            this.updateInvoiceItemsStatusByInvoiceHeader(invoiceItems, statusCode);
            this.Trace("updateInvoiceItemsStatusByInvoiceHeader end");


            if (!salesInvoceResponse.ResponseResult.Result)
            {
                return new CreateInvoiceWithLogoResponse
                {
                    ResponseResult = ResponseResult.ReturnError(salesInvoceResponse.ResponseResult.ExceptionDetail)
                };
            }
            return new CreateInvoiceWithLogoResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
    }
}
