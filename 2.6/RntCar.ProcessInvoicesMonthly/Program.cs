using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.IntegrationHelper;
using RntCar.Logger;
using RntCar.MongoDBHelper.Entities;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RntCar.ProcessInvoicesMonthly
{
    class Program
    {
        public static LoggerHelper loggerHelper;
        public static CrmServiceHelper crmServiceHelper;

        public static XrmHelper xrmHelper;
        public static SystemParameterRepository systemParameterRepository;

        public static ContractHelper contractHelper;
        public static ContractRepository contractRepository;
        public static ContractItemRepository contractItemRepository;
        public static ContractInvoiceDateRepository contractInvoiceDateRepository;

        public static AdditionalProductRepository additionalProductRepository;

        public static PaymentPlanRepository paymentPlanRepository;

        public static InvoiceItemRepository invoiceItemRepository;
        public static InvoiceRepository invoiceRepository;
        public static InvoiceHelper invoiceHelper;
        public static InvoiceBL invoiceBL;

        public static HGSHelper hgsHelper;
        public static BranchRepository branchRepository;

        public static string mongoDBHostName;
        public static string mongoDBDatabaseName;
        public static int ratio;

        static void Main(string[] args)
        {
            loggerHelper = new LoggerHelper();
            crmServiceHelper = new CrmServiceHelper();
            xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
            systemParameterRepository = new SystemParameterRepository(crmServiceHelper.IOrganizationService);

            contractHelper = new ContractHelper(crmServiceHelper.IOrganizationService);
            contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
            contractItemRepository = new ContractItemRepository(crmServiceHelper.IOrganizationService);
            contractInvoiceDateRepository = new ContractInvoiceDateRepository(crmServiceHelper.IOrganizationService);

            additionalProductRepository = new AdditionalProductRepository(crmServiceHelper.IOrganizationService);

            paymentPlanRepository = new PaymentPlanRepository(crmServiceHelper.IOrganizationService);

            invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
            invoiceItemRepository = new InvoiceItemRepository(crmServiceHelper.IOrganizationService);
            invoiceHelper = new InvoiceHelper(crmServiceHelper.IOrganizationService);
            invoiceBL = new InvoiceBL(crmServiceHelper.IOrganizationService);


            hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService);
            branchRepository = new BranchRepository(crmServiceHelper.IOrganizationService);

            mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
            mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");
            ratio = systemParameterRepository.getTaxRatio();

            string processMode = StaticHelper.GetConfiguration("processMode");
            if (processMode == "SINGLEMOD")
            {
                SingleRecordProcess();
            }
            else
            {
                PeriodicProcess();
            }

        }

        private static void PeriodicProcess()
        {
            var invoices = contractInvoiceDateRepository.getContratcInvoicesLessThanGivenDay(0);

            ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
            var currencyId = configurationBL.GetConfigurationByName("currency_TRY");

            foreach (var item in invoices)
            {
                Console.WriteLine(invoices.IndexOf(item));
                var duration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(item.GetAttributeValue<DateTime>("rnt_pickupdatetime"), item.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));
                var products = additionalProductRepository.GetAdditionalProductsWithCalculatedPrice(duration, Guid.Empty);
                try
                {
                    var contractId = item.GetAttributeValue<EntityReference>("rnt_contractid").Id;
                    Entity e = new Entity("rnt_contract");
                    e.Id = contractId;
                    e["rnt_automaticinvoiceenabled"] = false;
                    crmServiceHelper.IOrganizationService.Update(e);

                    var template = item.GetAttributeValue<string>("rnt_invoicetemplate");
                    var deserializedTemplate = JsonConvert.DeserializeObject<List<InvoiceItemTemplate>>(template);

                    var invoiceEntity = invoiceRepository.getFirstActiveInvoiceByContractId(contractId).FirstOrDefault();
                    if (invoiceEntity == null)
                    {
                        invoiceEntity = invoiceRepository.getFirstInvoiceByContractId(contractId);
                    }

                    InvoiceHelper invoiceHelper = new InvoiceHelper(crmServiceHelper.IOrganizationService);

                    InvoiceBL invoiceBL = new InvoiceBL(crmServiceHelper.IOrganizationService);

                    var contract = contractRepository.getContractById(contractId, new string[] { "transactioncurrencyid", "rnt_pickupbranchid", "rnt_pickupdatetime", "rnt_dropoffdatetime", "rnt_customerid", "rnt_pnrnumber", "statuscode" });
                    loggerHelper.traceInfo(contract.GetAttributeValue<string>("rnt_pnrnumber"));
                    if (contract.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contract_StatusCode.Completed)
                    {
                        #region Completed
                        xrmHelper.setState("rnt_contractinvoicedate", item.Id, 1, 2);
                        #endregion
                    }
                    else
                    {
                        #region not completed
                        var documentBranchInfo = branchRepository.getBranchById(contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id);

                        var inv = invoiceBL.createInvoice(InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoiceEntity), null, (Guid?)contractId, contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);
                        Entity updateInvoice = new Entity("rnt_invoice");
                        updateInvoice.Id = inv;
                        updateInvoice["rnt_defaultinvoice"] = false;
                        crmServiceHelper.IOrganizationService.Update(updateInvoice);

                        List<Entity> contractItems = new List<Entity>();

                        var existingItems = contractItemRepository.getActiveContractItems(contractId);
                        var isEquipmentchanged = existingItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment).ToList().Count > 1 ? true : false;


                        existingItems = existingItems.Where(p => p.GetAttributeValue<DateTime>(p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment &&
                                                                                               p.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.Completed ?
                                                                                               "rnt_dropoffdatetime" :
                                                                                               "rnt_pickupdatetime").Date.isBetween(item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date, item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date)).ToList();
                        existingItems = existingItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct ||
                                                                 p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment).ToList();


                        foreach (var temp in deserializedTemplate)
                        {
                            if (existingItems.Where(p => p.Id == temp.contractItemId).FirstOrDefault() == null)
                            {
                                existingItems.Add(contractItemRepository.getContractItemId(temp.contractItemId));
                            }
                        }

                        //remove equipments not related
                        if (isEquipmentchanged)
                        {
                            var excludeItems = existingItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment &&
                                                                        !p.GetAttributeValue<DateTime>(p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment &&
                                                                                                       p.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.Completed ?
                                                                                                       "rnt_dropoffdatetime" :
                                                                                                       "rnt_pickupdatetime").Date.isBetween(item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date, item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date)).ToList();
                            existingItems = existingItems.Except(excludeItems).ToList();

                            foreach (var exclude in excludeItems)
                            {
                                deserializedTemplate = deserializedTemplate.Where(p => p.contractItemId != exclude.Id).ToList();
                            }


                        }
                        if (existingItems.Count == 0)
                        {
                            existingItems = contractItemRepository.getRentalContractItemsByContractId(contractId);
                            existingItems = existingItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct ||
                                                                p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment).ToList();

                            existingItems = existingItems.Where(p => p.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date.isBetween(item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date, item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date)).ToList();
                            if (existingItems.Count == 0)
                            {
                                existingItems = contractItemRepository.getActiveContractItems(contractId);

                                existingItems = existingItems.Where(p => item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date.isBetween(p.GetAttributeValue<DateTime>("rnt_pickupdatetime"), p.GetAttributeValue<DateTime>("rnt_dropoffdatetime"))).ToList();
                                existingItems = existingItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct ||
                                                              p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment).ToList();
                            }
                        }
                        foreach (var existingItem in existingItems)
                        {
                            var i = deserializedTemplate.Where(p => p.contractItemId == existingItem.Id).FirstOrDefault();
                            if (i == null)
                            {
                                decimal amount = decimal.Zero;
                                if (existingItem.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.Rental &&
                                    existingItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment &&
                                    isEquipmentchanged)
                                {
                                    ContractDailyPrices contractDailyPrices = new ContractDailyPrices(mongoDBHostName, mongoDBDatabaseName);
                                    var dailyprices = contractDailyPrices.getDailyPricesByContractItemId(existingItem.Id);

                                    var _items = dailyprices.Where(p => p.priceDate.Date >= existingItem.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date && p.priceDate <= item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date).ToList();
                                    amount = _items.Sum(p => p.totalAmount);
                                    if (_items.Count > 30)
                                    {
                                        _items = dailyprices.Where(p => p.priceDate.Date >= item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date && p.priceDate <= item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date).ToList();
                                        amount = _items.Sum(p => p.totalAmount);
                                    }
                                }
                                else if (existingItem.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.Rental &&
                                   existingItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct)
                                {
                                    var a = products.Where(p => p.productId == existingItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id).FirstOrDefault();

                                    amount = a.actualTotalAmount;
                                }
                                else if (existingItem.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.Completed &&
                                       existingItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment &&
                                       isEquipmentchanged)
                                {
                                    amount = existingItem.GetAttributeValue<Money>("rnt_totalamount").Value;

                                    ContractDailyPrices contractDailyPrices = new ContractDailyPrices(mongoDBHostName, mongoDBDatabaseName);
                                    var dailyprices = contractDailyPrices.getDailyPricesByContractItemId(existingItem.Id);

                                    var _items = dailyprices.Where(p => p.priceDate.Date >= existingItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date && p.priceDate <= item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date).ToList();
                                    if (_items.Count() == 0)
                                    {
                                        _items = dailyprices.Where(p => p.priceDate.Date <= existingItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date && p.priceDate >= item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date).ToList();
                                    }
                                    if (_items.Count > 30)
                                    {
                                        _items = dailyprices.Where(p => p.priceDate.Date >= item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date && p.priceDate <= item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date).ToList();
                                    }
                                    if (_items.Count > 0)
                                        amount = _items.Sum(p => p.totalAmount);

                                }
                                else
                                {
                                    amount = existingItem.GetAttributeValue<Money>("rnt_totalamount").Value;
                                }
                                var existing = invoiceItemRepository.getIntegratedInvoiceItemsByContractItemId(existingItem.Id);
                                //daha önce hiç faturası kesilmediyse
                                var b = decimal.Zero;

                                b = existingItem.GetAttributeValue<Money>("rnt_totalamount").Value - existing.Sum(p => p.GetAttributeValue<Money>("rnt_totalamount").Value) > amount ||
                                    existingItem.GetAttributeValue<Money>("rnt_totalamount").Value - existing.Sum(p => p.GetAttributeValue<Money>("rnt_totalamount").Value) == decimal.Zero ? amount :
                                    existingItem.GetAttributeValue<Money>("rnt_totalamount").Value - existing.Sum(p => p.GetAttributeValue<Money>("rnt_totalamount").Value);


                                deserializedTemplate.Add(new InvoiceItemTemplate
                                {
                                    contractItemId = existingItem.Id,
                                    amount = b,
                                    itemType = existingItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value,
                                    additionalProductId = existingItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value != (int)rnt_contractitem_rnt_itemtypecode.Equipment ?
                                                          existingItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id :
                                                          Guid.Empty,
                                    equipmentId = existingItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment ?
                                                  existingItem.GetAttributeValue<EntityReference>("rnt_equipment").Id :
                                                  Guid.Empty
                                });

                            }
                            else
                            {
                                if (existingItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment)
                                {
                                    if (existingItem.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.Completed &&
                                        existingItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment &&
                                        isEquipmentchanged)
                                    {
                                        i.amount = existingItem.GetAttributeValue<Money>("rnt_totalamount").Value;

                                        ContractDailyPrices contractDailyPrices = new ContractDailyPrices(mongoDBHostName, mongoDBDatabaseName);
                                        var dailyprices = contractDailyPrices.getDailyPricesByContractItemId(existingItem.Id);

                                        var _items = dailyprices.Where(p => p.priceDate.Date >= existingItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date && p.priceDate <= item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date).ToList();
                                        if (_items.Count() == 0)
                                        {
                                            _items = dailyprices.Where(p => p.priceDate.Date <= existingItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date && p.priceDate >= item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date).ToList();
                                        }
                                        if (_items.Count > 30)
                                        {
                                            _items = dailyprices.Where(p => p.priceDate.Date >= existingItem.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date && p.priceDate <= item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date).ToList();
                                        }
                                        if (_items.Count > 0)
                                            i.amount = _items.Sum(p => p.totalAmount);

                                    }
                                    //araç değişikliği olan kalem
                                    else if (existingItem.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.Rental &&
                                       existingItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment &&
                                       isEquipmentchanged)
                                    {
                                        ContractDailyPrices contractDailyPrices = new ContractDailyPrices(mongoDBHostName, mongoDBDatabaseName);
                                        var dailyprices = contractDailyPrices.getDailyPricesByContractItemId(existingItem.Id);

                                        var _items = dailyprices.Where(p => p.priceDate.Date >= existingItem.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date && p.priceDate <= item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date).ToList();
                                        i.amount = _items.Sum(p => p.totalAmount);

                                    }
                                }
                                else if (existingItem.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.Rental &&
                                   existingItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct)
                                {
                                    var product = additionalProductRepository.getAdditionalProductById(existingItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id);
                                    if (product.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value == (int)rnt_PriceCalculationTypeCode.DependedonDuration)
                                    {

                                        if (existingItem.Contains("rnt_monthlypackageprice"))
                                        {
                                            i.amount = existingItem.GetAttributeValue<Money>("rnt_monthlypackageprice").Value;
                                        }
                                        else
                                        {
                                            var a = products.Where(p => p.productId == existingItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id).FirstOrDefault();

                                            i.amount = a.actualTotalAmount;
                                        }

                                    }
                                    else
                                    {
                                        var existing = invoiceItemRepository.getIntegratedInvoiceItemsByContractItemId(existingItem.Id);
                                        if (existing.Count > 0)
                                        {
                                            i.amount = decimal.Zero;
                                        }
                                    }
                                }


                            }
                        }
                        var checkEquipment = deserializedTemplate.Where(x => x.itemType == 1).ToList();
                        if (checkEquipment.Count == 0)
                        {
                            InvoiceItemTemplate equipmentTemplate = createEquipmentTemplate(contract, item.GetAttributeValue<DateTime>("rnt_pickupdatetime"), item.GetAttributeValue<DateTime>("rnt_dropoffdatetime"), item.GetAttributeValue<DateTime>("rnt_pickupdatetime").Month);
                            deserializedTemplate.Add(equipmentTemplate);
                        }
                        deserializedTemplate = deserializedTemplate.Where(p => p.amount > 0).ToList();

                        Entity updateEntity = new Entity(item.LogicalName);
                        updateEntity.Id = item.Id;
                        updateEntity["rnt_amount"] = new Money(deserializedTemplate.Sum(p => p.amount));
                        updateEntity["rnt_invoicetemplate"] = JsonConvert.SerializeObject(deserializedTemplate);
                        crmServiceHelper.IOrganizationService.Update(updateEntity);

                        foreach (var invItems in deserializedTemplate)
                        {
                            var name = item.GetAttributeValue<DateTime>("rnt_pickupdatetime") + " - " + item.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                            var productRef = new EntityReference();
                            var materialCode = string.Empty;
                            if (invItems.equipmentId != Guid.Empty)
                            {
                                EquipmentRepository equipmentRepository = new EquipmentRepository(crmServiceHelper.IOrganizationService);
                                var eq = equipmentRepository.getEquipmentById(invItems.equipmentId);
                                productRef = new EntityReference
                                {
                                    Id = eq.Id,
                                    Name = eq.GetAttributeValue<string>("rnt_name"),
                                    LogicalName = "rnt_equipment"
                                };
                                materialCode += eq.GetAttributeValue<string>("rnt_name");
                            }
                            if (invItems.additionalProductId != Guid.Empty)
                            {
                                var product = additionalProductRepository.getAdditionalProductById(invItems.additionalProductId);
                                productRef = new EntityReference
                                {
                                    Id = invItems.additionalProductId,
                                    Name = product.GetAttributeValue<string>("rnt_name"),
                                    LogicalName = "rnt_additionalproduct"
                                };
                                materialCode += product.GetAttributeValue<string>("rnt_name");

                            }
                            Entity createInvoiceItem = xrmHelper.initializeFromRequest("rnt_contractitem", invItems.contractItemId, "rnt_invoiceitem");
                            createInvoiceItem["rnt_totalamount"] = new Money(invItems.amount);
                            createInvoiceItem["rnt_invoiceid"] = new EntityReference("rnt_invoice", inv);
                            createInvoiceItem["rnt_name"] = name;
                            createInvoiceItem["rnt_netamount"] = invItems.amount / 1.18M;
                            createInvoiceItem["rnt_name"] = materialCode + "-"
                                                + contract.GetAttributeValue<EntityReference>("rnt_customerid").Name + "-"
                                                + item.GetAttributeValue<DateTime>("rnt_pickupdatetime").ToString("dd/MM/yyyy") + "-" +
                                                  item.GetAttributeValue<DateTime>("rnt_dropoffdatetime").ToString("dd/MM/yyyy") + "-" +
                                                  contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name + "-" +
                                                  contract.GetAttributeValue<string>("rnt_pnrnumber");

                            crmServiceHelper.IOrganizationService.Create(createInvoiceItem);

                            Entity entity = new Entity("rnt_contractitem");
                            entity["rnt_totalamount"] = new Money(invItems.amount);
                            entity["rnt_pickupdatetime"] = item.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                            entity["rnt_pickupbranchid"] = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid");
                            entity["rnt_dropoffdatetime"] = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                            entity["rnt_taxratio"] = Convert.ToDecimal(ratio);
                            entity["rnt_itemtypecode"] = new OptionSetValue(invItems.itemType);


                            if (invItems.equipmentId != Guid.Empty)
                            {
                                entity["rnt_equipment"] = productRef;
                            }
                            if (invItems.additionalProductId != Guid.Empty)
                            {
                                entity["rnt_additionalproductid"] = productRef;

                            }

                            entity.Id = invItems.contractItemId;
                            contractItems.Add(entity);

                        }
                        loggerHelper.traceInfo("contract items found " + contractItems.Count);
                        loggerHelper.traceInfo("invoice id " + inv);

                        if (contractItems.Count > 0)
                        {
                            invoiceBL.documentBranchInfo = documentBranchInfo;
                            invoiceBL.getDocumentInfo(contractId, null);
                            var response = invoiceBL.logoOperations(contractItems, contractId, inv, 1055, false, true, true);
                            if (response.ResponseResult.Result)
                            {
                                Entity contractInvoiceEntity = new Entity("rnt_contractinvoicedate");
                                contractInvoiceEntity["rnt_ischarged"] = true;
                                contractInvoiceEntity.Id = item.Id;
                                crmServiceHelper.IOrganizationService.Update(contractInvoiceEntity);
                            }
                        }
                        #endregion
                        #region HGS operations

                        var items = contractItemRepository.getActiveContractEquipments(contractId);
                        // existingItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment).ToList();
                        foreach (var contractItem in items)
                        {
                            hgsHelper.processHGSBatch(contractId, loggerHelper);
                        }
                        //hgsler işlendikten sonra bu kalemleri faturalayalım
                        var rentalItems = contractItemRepository.getRentalFineItems(contractId);
                        Dictionary<Guid, List<Entity>> dictionarycontractItems = new Dictionary<Guid, List<Entity>>();
                        foreach (var cItem in rentalItems)
                        {
                            var invoiceItem = invoiceItemRepository.getDraftInvoiceItemsByContractItemId(cItem.Id);
                            if (invoiceItem != null)
                            {
                                var iId = invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id;

                                List<Entity> checkEntity = new List<Entity>();
                                dictionarycontractItems.TryGetValue(iId, out checkEntity);
                                if (checkEntity == null)
                                {
                                    checkEntity = new List<Entity>();
                                    checkEntity.Add(cItem);
                                    dictionarycontractItems.Add(iId, checkEntity);
                                }
                                else
                                {
                                    checkEntity.Add(cItem);
                                    dictionarycontractItems[iId] = checkEntity;
                                }
                            }
                            else
                            {
                                var invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contractId).FirstOrDefault();
                                Entity createInvoiceItem = xrmHelper.initializeFromRequest("rnt_contractitem", cItem.Id, "rnt_invoiceitem");
                                createInvoiceItem["rnt_invoiceid"] = new EntityReference(invoice.LogicalName, invoice.Id);
                                createInvoiceItem["rnt_name"] = cItem.GetAttributeValue<string>("rnt_name");
                                createInvoiceItem["rnt_netamount"] = cItem.GetAttributeValue<Money>("rnt_netamount").Value;
                                crmServiceHelper.IOrganizationService.Create(createInvoiceItem);
                                List<Entity> checkEntity = new List<Entity>();
                                dictionarycontractItems.TryGetValue(invoice.Id, out checkEntity);
                                if (checkEntity == null)
                                {
                                    checkEntity = new List<Entity>();
                                    checkEntity.Add(cItem);
                                    dictionarycontractItems.Add(invoice.Id, checkEntity);
                                }
                                else
                                {
                                    checkEntity.Add(cItem);
                                    dictionarycontractItems[invoice.Id] = checkEntity;
                                }
                            }
                        }
                        if (dictionarycontractItems.Count > 0)
                        {
                            foreach (var invoice in dictionarycontractItems)
                            {
                                invoiceBL.documentBranchInfo = documentBranchInfo;
                                invoiceBL.getDocumentInfo(contractId, null);
                                Thread.Sleep(2000);
                                loggerHelper.traceInfo("hgs invoice id " + invoice.Key);
                                var response = invoiceBL.logoOperations(invoice.Value, contractId, invoice.Key, 1055, false, true, true);
                                if (response.ResponseResult.Result)
                                {
                                    foreach (var conItem in invoice.Value)
                                    {
                                        Entity e1 = new Entity(conItem.LogicalName);
                                        e1["statuscode"] = new OptionSetValue((int)rnt_contractitem_StatusCode.Completed);
                                        e1.Id = conItem.Id;
                                        crmServiceHelper.IOrganizationService.Update(e1);
                                    }
                                }
                            }
                            var integratedInvoice = invoiceRepository.getLastIntegratedInvoice(contractId);
                            invoiceBL.createInvoice(InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(integratedInvoice), null, contractId, new Guid(currencyId));
                        }
                        loggerHelper.traceInfo(StaticHelper.endLineStar);

                        #endregion

                    }

                }
                catch (System.Exception ex)
                {
                    loggerHelper.traceInfo("hata : " + ex.Message);
                    continue;
                }

            }

            //senaryolar 
            //-->2 AYLIKTI 35 günde kapandı --> kalan 5 gün için
            //--> 2 aylıktı 62 günde kapandı , kalan 2 gün için

            var contracts = contractRepository.getMonthlyCompletedContractsByXlastDays(30);

            foreach (var item in contracts)
            {
                try
                {
                    var processInvoice = false;

                    var totalAmount = item.GetAttributeValue<Money>("rnt_generaltotalamount").Value;
                    var billedAmount = item.GetAttributeValue<Money>("rnt_billedamount").Value;
                    //faturalanacak tutar mevcut;
                    if (totalAmount > billedAmount)
                    {
                        loggerHelper.traceInfo(item.Id.ToString());
                        var invoice = invoiceRepository.getInvoicesByContractId(item.Id);
                        Entity _invoice = null;
                        if (invoice.Count > 0 &&
                           (invoice.FirstOrDefault().GetAttributeValue<OptionSetValue>("statuscode").Value != (int)rnt_invoice_StatusCode.IntegrationError &&
                            invoice.FirstOrDefault().GetAttributeValue<OptionSetValue>("statuscode").Value != (int)rnt_invoice_StatusCode.InternalError))
                        {

                            _invoice = invoice.FirstOrDefault();
                        }
                        if (_invoice == null)
                        {
                            InvoiceHelper invoiceHelper = new InvoiceHelper(crmServiceHelper.IOrganizationService);
                            var id = invoiceHelper.createInvoiceFromInvoice(item.Id, invoiceRepository.getLastIntegratedInvoice(item.Id).Id);

                            _invoice = invoiceRepository.getInvoiceById(id.Value);
                        }
                        var contracitems = contractItemRepository.getCompletedContractItemsByContractId(item.Id);

                        foreach (var contractItem in contracitems)
                        {
                            var amount = contractItem.GetAttributeValue<Money>("rnt_totalamount").Value;

                            var invoiceItems = invoiceItemRepository.getIntegratedInvoiceItemsByContractItemId(contractItem.Id);
                            amount = contractItem.GetAttributeValue<Money>("rnt_totalamount").Value - invoiceItems.Sum(p => p.GetAttributeValue<Money>("rnt_totalamount").Value);

                            if (amount > 0)
                            {
                                processInvoice = true;
                                //check for draft                      
                                var invoiceItem = invoiceItemRepository.getDraftInvoiceItemsByContractItemIdandInvoiceId(contractItem.Id, _invoice.Id);
                                if (invoiceItem == null)
                                {
                                    InvoiceItemBL invoiceItemBL = new InvoiceItemBL(crmServiceHelper.IOrganizationService);
                                    invoiceItemBL.createInvoiceItem(_invoice.Id,
                                                                    contractItem.Id,
                                                                    null,
                                                                    new Guid("036024dd-e8a5-e911-a847-000d3a2bd64e"),
                                                                    contractItem.GetAttributeValue<string>("rnt_name"),
                                                                    amount);
                                }

                            }
                        }

                        if (processInvoice)
                        {
                            ExecuteWorkflowRequest executeWorkflowRequest = new ExecuteWorkflowRequest
                            {
                                WorkflowId = new Guid("1C8E21D1-DEA0-4C59-BF76-8D386EBEA9D6"),
                                EntityId = _invoice.Id
                            };
                            crmServiceHelper.IOrganizationService.Execute(executeWorkflowRequest);
                        }

                    }
                }
                catch (Exception ex)
                {
                    loggerHelper.traceInfo("hata sözleşme: " + ex.Message);

                    continue;
                }
            }
        }

        private static void SingleRecordProcess()
        {
            do
            {
                UpdateContractInvoiceDateByContractNumber();
                Console.WriteLine("Devam Etmek İstiyor Musunuz? E/H");
                string answer = Console.ReadLine();
                if (answer.ToUpper() != "E")
                {
                    break;
                }
            } while (true);
        }

        private static void UpdateContractInvoiceDateByContractNumber()
        {
            Console.WriteLine("Sözleşme Numarasını Giriniz");
            string contractNumber = Console.ReadLine();
            Entity contract = contractRepository.getContractByContractNumber(contractNumber);
            if (contract == null || contract.Id == Guid.Empty)
            {
                Console.WriteLine($"{contractNumber} Sistemde Bulunamadı Numarayı Kontrol Ediniz");
                return;
            }
            var invoices = contractInvoiceDateRepository.getContratcInvoices(contract.Id);
            invoices = invoices.OrderBy(x => x.GetAttributeValue<DateTime>("rnt_pickupdatetime")).ToList();
            Console.WriteLine($"{invoices.Count} Kontrol Edilecek Kayıt Bulundu");
            foreach (var item in invoices)
            {
                var template = item.GetAttributeValue<string>("rnt_invoicetemplate");
                var pickupDateTime = item.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                var dropOffDateTime = item.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                var name = item.GetAttributeValue<string>("rnt_name");
                var monthValue = pickupDateTime.Month;

                var deserializedSourceTemplate = JsonConvert.DeserializeObject<List<InvoiceItemTemplate>>(template);

                if (deserializedSourceTemplate.Where(x => x.itemType == 1).ToList().Count > 0)
                {
                    Console.WriteLine($"{name} Kayıt Kontrol Edildi ve Devam Ediliyor");
                    continue;
                }

                Console.WriteLine($"{name} Kayıt İşleme Alındı");
                InvoiceItemTemplate equipmentTemplate = createEquipmentTemplate(contract, pickupDateTime, dropOffDateTime, monthValue);

                Console.WriteLine($"{equipmentTemplate.amount} Tutar Eksiktir İşlem Yapılacaktır");
                Entity invoiceTemplate = new Entity(item.LogicalName);
                invoiceTemplate["rnt_extensiontypecode"] = item.GetAttributeValue<OptionSetValue>("rnt_extensiontypecode");
                invoiceTemplate["rnt_dropoffdatetime"] = dropOffDateTime;
                invoiceTemplate["rnt_pickupdatetime"] = pickupDateTime;
                invoiceTemplate["rnt_invoicedate"] = item.GetAttributeValue<DateTime>("rnt_invoicedate");
                invoiceTemplate["rnt_contractid"] = item.GetAttributeValue<EntityReference>("rnt_contractid");
                invoiceTemplate["rnt_amount"] = new Money(equipmentTemplate.amount);
                invoiceTemplate["rnt_invoicetemplate"] = JsonConvert.SerializeObject(equipmentTemplate);
                invoiceTemplate["rnt_name"] = pickupDateTime.ToString("dd/MM/yyyy") + " " + dropOffDateTime.ToString("dd/MM/yyyy") + " Add Equipment"; ;
                Guid tempInvoiceDateId = crmServiceHelper.IOrganizationService.Create(invoiceTemplate);
                invoiceTemplate.Id = tempInvoiceDateId;

                Entity updateContract = new Entity("rnt_contract");
                updateContract.Id = contract.Id;
                updateContract["rnt_automaticinvoiceenabled"] = false;
                crmServiceHelper.IOrganizationService.Update(updateContract);

                var invoiceEntity = invoiceRepository.getFirstActiveInvoiceByContractId(contract.Id).FirstOrDefault();
                if (invoiceEntity == null)
                {
                    invoiceEntity = invoiceRepository.getFirstInvoiceByContractId(contract.Id);
                }

                loggerHelper.traceInfo(contract.GetAttributeValue<string>("rnt_pnrnumber"));
                if (contract.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contract_StatusCode.Completed)
                {
                    #region Completed
                    xrmHelper.setState("rnt_contractinvoicedate", tempInvoiceDateId, 1, 2);
                    #endregion
                }
                else
                {
                    #region not completed
                    var documentBranchInfo = branchRepository.getBranchById(contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id);

                    var inv = invoiceBL.createInvoice(InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoiceEntity), null, contract.Id, contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);
                    Entity updateInvoice = new Entity("rnt_invoice");
                    updateInvoice.Id = inv;
                    updateInvoice["rnt_defaultinvoice"] = false;
                    crmServiceHelper.IOrganizationService.Update(updateInvoice);

                    List<Entity> contractItems = new List<Entity>();


                    var invoiceItemName = pickupDateTime + " - " + dropOffDateTime;
                    var productRef = new EntityReference();
                    var materialCode = string.Empty;
                    if (equipmentTemplate.equipmentId != Guid.Empty)
                    {
                        EquipmentRepository equipmentRepository = new EquipmentRepository(crmServiceHelper.IOrganizationService);
                        var eq = equipmentRepository.getEquipmentById(equipmentTemplate.equipmentId);
                        productRef = new EntityReference
                        {
                            Id = eq.Id,
                            Name = eq.GetAttributeValue<string>("rnt_name"),
                            LogicalName = "rnt_equipment"
                        };
                        materialCode += eq.GetAttributeValue<string>("rnt_name");
                    }
                    Entity createInvoiceItem = xrmHelper.initializeFromRequest("rnt_contractitem", equipmentTemplate.contractItemId, "rnt_invoiceitem");
                    createInvoiceItem["rnt_totalamount"] = new Money(equipmentTemplate.amount);
                    createInvoiceItem["rnt_invoiceid"] = new EntityReference("rnt_invoice", inv);
                    createInvoiceItem["rnt_name"] = invoiceItemName;
                    createInvoiceItem["rnt_netamount"] = equipmentTemplate.amount / 1.18M;
                    createInvoiceItem["rnt_name"] = materialCode + "-"
                                        + contract.GetAttributeValue<EntityReference>("rnt_customerid").Name + "-"
                                        + pickupDateTime.ToString("dd/MM/yyyy") + "-" +
                                          dropOffDateTime.ToString("dd/MM/yyyy") + "-" +
                                          contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name + "-" +
                                          contract.GetAttributeValue<string>("rnt_pnrnumber");

                    crmServiceHelper.IOrganizationService.Create(createInvoiceItem);

                    Entity entity = new Entity("rnt_contractitem");
                    entity["rnt_totalamount"] = new Money(equipmentTemplate.amount);
                    entity["rnt_pickupdatetime"] = pickupDateTime;
                    entity["rnt_pickupbranchid"] = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid");
                    entity["rnt_dropoffdatetime"] = dropOffDateTime;
                    entity["rnt_taxratio"] = Convert.ToDecimal(ratio);
                    entity["rnt_itemtypecode"] = new OptionSetValue(equipmentTemplate.itemType);


                    if (equipmentTemplate.equipmentId != Guid.Empty)
                    {
                        entity["rnt_equipment"] = productRef;
                    }

                    entity.Id = equipmentTemplate.contractItemId;
                    contractItems.Add(entity);

                    loggerHelper.traceInfo("contract items found " + contractItems.Count);
                    loggerHelper.traceInfo("invoice id " + inv);

                    if (contractItems.Count > 0)
                    {
                        invoiceBL.documentBranchInfo = documentBranchInfo;
                        invoiceBL.getDocumentInfo(contract.Id, null);
                        var response = invoiceBL.logoOperations(contractItems, contract.Id, inv, 1055, false, true, true);
                        if (response.ResponseResult.Result)
                        {
                            var deserializedTemplate = JsonConvert.DeserializeObject<List<InvoiceItemTemplate>>(template);
                            deserializedTemplate.Add(equipmentTemplate);

                            Entity contractInvoiceEntity = new Entity("rnt_contractinvoicedate");
                            contractInvoiceEntity.Id = item.Id;
                            contractInvoiceEntity["rnt_ischarged"] = true;
                            contractInvoiceEntity["rnt_amount"] = new Money(item.GetAttributeValue<Money>("rnt_amount").Value + equipmentTemplate.amount);
                            contractInvoiceEntity["rnt_invoicetemplate"] = JsonConvert.SerializeObject(deserializedTemplate);
                            crmServiceHelper.IOrganizationService.Update(contractInvoiceEntity);

                            crmServiceHelper.IOrganizationService.Delete(invoiceTemplate.LogicalName, invoiceTemplate.Id);

                            Console.WriteLine($"{name} Kayıt Tamamlanmıştır Kontroller Sağlanabilir.");
                            Console.WriteLine();
                        }
                    }
                    #endregion
                }

            }
        }

        private static InvoiceItemTemplate createEquipmentTemplate(Entity contract, DateTime pickupDateTime, DateTime dropOffDateTime, int monthValue)
        {
            Entity equipmentContractItem = new Entity();
            List<Entity> contractItemList = contractItemRepository.getActiveContractEquipments(contract.Id);
            foreach (var equipmentItem in contractItemList)
            {
                DateTime equipmentItemPickupDateTime = equipmentItem.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                DateTime equipmentItemDropOffDateTime = equipmentItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                if (equipmentItemPickupDateTime <= pickupDateTime && dropOffDateTime <= equipmentItemDropOffDateTime)
                {
                    equipmentContractItem = equipmentItem;
                    break;
                }
            }

            Entity paymentPlan = paymentPlanRepository.getPaymentPlans(contract.Id, monthValue);

            InvoiceItemTemplate equipmentTemplate = new InvoiceItemTemplate
            {
                additionalProductId = Guid.Empty,
                amount = paymentPlan.GetAttributeValue<Money>("rnt_amount").Value,
                contractItemId = equipmentContractItem.Id,
                equipmentId = equipmentContractItem.Contains("rnt_equipment") ? equipmentContractItem.GetAttributeValue<EntityReference>("rnt_equipment").Id : Guid.Empty,
                itemType = equipmentContractItem.Contains("rnt_itemtypecode") ? equipmentContractItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value : 0,
            };
            return equipmentTemplate;
        }

    }
}
