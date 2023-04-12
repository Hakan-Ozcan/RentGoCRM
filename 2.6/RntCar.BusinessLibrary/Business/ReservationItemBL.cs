using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class ReservationItemBL : BusinessHandler
    {



        public ReservationItemBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public ReservationItemBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public ReservationItemBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public void removeTaxAmount(Entity entity)
        {
            if (entity.Attributes.Contains("rnt_additionalproductid"))
            {
                AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
                var additionalProduct = additionalProductRepository.getAdditionalProductById(entity.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id, new string[] { "rnt_additionalproductcode" });

                ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
                var trafficFineCode = configurationRepository.GetConfigurationByKey("additionalProduct_trafficFineProductCode");

                if (additionalProduct.GetAttributeValue<string>("rnt_additionalproductcode") == trafficFineCode)
                {
                    entity["rnt_taxratio"] = decimal.Zero;
                }
            }
        }
        public void UpdateMongoDBUpdateRelatedFields(Entity entity, MongoDBResponse mongoDBResponse)

        {
            if (mongoDBResponse.Result)
            {
                this.UpdateMongoDBUpdateRelatedFields(entity);
            }
            //update by error
            else
            {
                this.UpdateMongoDBUpdateRelatedFieldsWithError(entity, mongoDBResponse.ExceptionDetail);
            }
        }

        public Guid createReservationItemForEquipment(ReservationCustomerParameters reservationCustomerParameter,
                                                      ReservationDateandBranchParameters reservationDateandBranchParameter,
                                                      ReservationEquipmentParameters reservationEquipmentParameter,
                                                      ReservationPriceParameters reservationPriceParameters,
                                                      Guid reservationId,
                                                      Guid currencyId,
                                                      int totalDuration,
                                                      string trackingNumber,
                                                      string dummyContactId,
                                                      EntityReference campaign,
                                                      int changeType,
                                                      int channelCode,
                                                      int itemType = (int)rnt_reservationitem_rnt_itemtypecode.Equipment)
        {
            Guid campaignId = new Guid();
            if (reservationPriceParameters.campaignId.HasValue)
            {
                campaignId = reservationPriceParameters.campaignId.Value;
            }
            PriceCalculationSummariesBL priceCalculationSummariesBL = new PriceCalculationSummariesBL(this.OrgService);
            


            Entity reservationItem = new Entity("rnt_reservationitem");
            reservationItem["rnt_reservationid"] = new EntityReference("rnt_reservation", reservationId);
            reservationItem["statuscode"] = new OptionSetValue(1);
            reservationItem["rnt_reservationduration"] = totalDuration;
            reservationItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.pickupBranchId);
            reservationItem["rnt_pickupdatetime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
            reservationItem["rnt_dropoffdatetime"] = reservationDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
            reservationItem["rnt_offset"] = StaticHelper.offset;
            reservationItem["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.dropoffBranchId);
            reservationItem["rnt_groupcodeinformationsid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
            reservationItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
            reservationItem["rnt_baseprice"] = new Money(reservationPriceParameters.price);
            reservationItem["rnt_totalamount"] = new Money(reservationPriceParameters.price);
            reservationItem["rnt_itemtypecode"] = new OptionSetValue(itemType);
            reservationItem["rnt_name"] = reservationEquipmentParameter.itemName;
            reservationItem["rnt_mongodbtrackingnumber"] = trackingNumber;
            reservationItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", currencyId);
            reservationItem["rnt_channelcode"] = new OptionSetValue(channelCode);
            reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var ratio = systemParameterBL.getSystemTaxRatio();
            reservationItem["rnt_taxratio"] = Convert.ToDecimal(ratio);
            if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Individual)
            {
                reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);

            }
            else if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Corporate &&
                    reservationCustomerParameter.corporateCustomerId.HasValue)
            {
                reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
                reservationItem["rnt_corporateid"] = new EntityReference("account", reservationCustomerParameter.corporateCustomerId.Value);

            }
            else if ((reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Broker ||
                     reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Agency) &&
                     reservationCustomerParameter.corporateCustomerId.HasValue)
            {
                reservationItem["rnt_corporateid"] = new EntityReference("account", reservationCustomerParameter.corporateCustomerId.Value);
                reservationItem["rnt_customerid"] = new EntityReference("contact", new Guid(dummyContactId));

            }
            this.Trace("reservationPriceParameters.campaignId" + reservationPriceParameters.campaignId);
            //set campaign if exists
            if (reservationPriceParameters.campaignId.HasValue)
            {
                reservationItem["rnt_campaignid"] = new EntityReference("rnt_campaign", reservationPriceParameters.campaignId.Value);
            }
            //updgrade yada downgrade'se kampanya koşulsuz yeni kaleme taşınır
            if (changeType == (int)ClassLibrary._Enums_1033.rnt_ChangeType.Upgrade ||
                changeType == (int)ClassLibrary._Enums_1033.rnt_ChangeType.Downgrade)
            {
                reservationItem["rnt_campaignid"] = campaign;

            }
            reservationItem["rnt_billingtypecode"] = CommonHelper.decideBillingType((int)rnt_reservationitem_rnt_itemtypecode.Equipment, reservationPriceParameters.pricingType, reservationEquipmentParameter.billingType);
            var id = this.OrgService.Create(reservationItem);

            return id;

        }

        public List<Guid> createReservationItemForAdditionalProducts(ReservationCustomerParameters reservationCustomerParameter,
                                                                     ReservationDateandBranchParameters reservationDateandBranchParameter,
                                                                     ReservationEquipmentParameters reservationEquipmentParameter,
                                                                     ReservationPriceParameters reservationPriceParameters,
                                                                     List<ReservationAdditionalProductParameters> reservationItemAdditionalProductParameter,
                                                                     int itemTypeCode,
                                                                     Guid reservationId,
                                                                     Guid currencyId,
                                                                     int totalDuration,
                                                                     string trackingNumber,
                                                                     string dummyContactId,
                                                                     int channelCode)

        {
            List<Guid> additionalProductGuidList = new List<Guid>();

            foreach (var item in reservationItemAdditionalProductParameter)
            {
                if (item.maxPieces <= 1)
                {
                    Entity reservationItem = new Entity("rnt_reservationitem");
                    reservationItem["rnt_reservationid"] = new EntityReference("rnt_reservation", reservationId);
                    reservationItem["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", item.productId);
                    reservationItem["rnt_reservationduration"] = totalDuration;
                    reservationItem["rnt_baseprice"] = new Money((decimal)item.actualAmount);
                    reservationItem["rnt_monthlypackageprice"] = new Money((decimal)item.monthlyPackagePrice);
                    reservationItem["statuscode"] = new OptionSetValue(1);
                    reservationItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.pickupBranchId);
                    reservationItem["rnt_pickupdatetime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
                    reservationItem["rnt_dropoffdatetime"] = reservationDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
                    reservationItem["rnt_offset"] = StaticHelper.offset;
                    reservationItem["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.dropoffBranchId);
                    reservationItem["rnt_groupcodeinformationsid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
                    reservationItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
                    reservationItem["rnt_mongodbtrackingnumber"] = trackingNumber;
                    reservationItem["rnt_itemtypecode"] = new OptionSetValue(itemTypeCode);
                    reservationItem["rnt_name"] = item.productName;
                    reservationItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", currencyId);
                    if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Individual)
                    {
                        reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);

                    }
                    else if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Corporate &&
                            reservationCustomerParameter.corporateCustomerId.HasValue)
                    {
                        reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
                        reservationItem["rnt_corporateid"] = new EntityReference("account", reservationCustomerParameter.corporateCustomerId.Value);

                    }
                    else if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Broker &&
                             reservationCustomerParameter.corporateCustomerId.HasValue)
                    {
                        reservationItem["rnt_corporateid"] = new EntityReference("account", reservationCustomerParameter.corporateCustomerId.Value);
                        reservationItem["rnt_customerid"] = new EntityReference("contact", new Guid(dummyContactId));

                    }
                    reservationItem["rnt_channelcode"] = new OptionSetValue(channelCode);

                    SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
                    var ratio = systemParameterBL.getSystemTaxRatio();
                    reservationItem["rnt_taxratio"] = Convert.ToDecimal(ratio);
                    decimal totalAmount = CommonHelper.calculateAdditionalProductPrice(item.priceCalculationType,
                                                                                       item.monthlyPackagePrice,
                                                                                       item.actualAmount.Value,
                                                                                       totalDuration);

                    reservationItem["rnt_totalamount"] = new Money(totalAmount);
                    this.Trace("billing type : " + CommonHelper.decideBillingType((int)rnt_reservationitem_rnt_itemtypecode.AdditionalProduct, reservationPriceParameters.pricingType, item.billingType));
                    reservationItem["rnt_billingtypecode"] = CommonHelper.decideBillingType((int)rnt_reservationitem_rnt_itemtypecode.AdditionalProduct, reservationPriceParameters.pricingType, item.billingType);
                    var id = this.OrgService.Create(reservationItem);
                    additionalProductGuidList.Add(id);

                }
                else
                {
                    for (int i = 0; i < item.value; i++)
                    {
                        Entity reservationItem = new Entity("rnt_reservationitem");
                        reservationItem["rnt_reservationid"] = new EntityReference("rnt_reservation", reservationId);
                        reservationItem["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", item.productId);
                        reservationItem["rnt_reservationduration"] = totalDuration;
                        reservationItem["rnt_baseprice"] = new Money((decimal)item.actualAmount);
                        reservationItem["rnt_monthlypackageprice"] = new Money((decimal)item.monthlyPackagePrice);
                        reservationItem["statuscode"] = new OptionSetValue(1);
                        reservationItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.pickupBranchId);
                        reservationItem["rnt_pickupdatetime"] = reservationDateandBranchParameter.pickupDate;
                        reservationItem["rnt_dropoffdatetime"] = reservationDateandBranchParameter.dropoffDate;
                        reservationItem["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.dropoffBranchId);
                        reservationItem["rnt_groupcodeinformationsid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
                        reservationItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
                        SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
                        var ratio = systemParameterBL.getSystemTaxRatio();
                        reservationItem["rnt_taxratio"] = Convert.ToDecimal(ratio);
                        decimal totalAmount = CommonHelper.calculateAdditionalProductPrice(item.priceCalculationType,
                                                                                           item.monthlyPackagePrice,
                                                                                           (decimal)item.actualAmount,
                                                                                            totalDuration);

                        reservationItem["rnt_totalamount"] = new Money(totalAmount);
                        reservationItem["rnt_itemtypecode"] = new OptionSetValue(itemTypeCode);
                        reservationItem["rnt_name"] = item.productName;
                        reservationItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", currencyId);
                        if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Individual)
                        {
                            reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);

                        }
                        else if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Corporate &&
                                reservationCustomerParameter.corporateCustomerId.HasValue)
                        {
                            reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
                            reservationItem["rnt_corporateid"] = new EntityReference("account", reservationCustomerParameter.corporateCustomerId.Value);

                        }
                        else if (reservationCustomerParameter?.customerType == (int)GlobalEnums.CustomerType.Broker &&
                                 reservationCustomerParameter.corporateCustomerId.HasValue)
                        {
                            reservationItem["rnt_corporateid"] = new EntityReference("account", reservationCustomerParameter.corporateCustomerId.Value);
                            reservationItem["rnt_customerid"] = new EntityReference("contact", new Guid(dummyContactId));

                        }
                        reservationItem["rnt_channelcode"] = new OptionSetValue(channelCode);
                        this.Trace("billing type : " + CommonHelper.decideBillingType((int)rnt_reservationitem_rnt_itemtypecode.AdditionalProduct, reservationPriceParameters.pricingType));
                        reservationItem["rnt_billingtypecode"] = CommonHelper.decideBillingType((int)rnt_reservationitem_rnt_itemtypecode.AdditionalProduct, reservationPriceParameters.pricingType, item.billingType);
                        var id = this.OrgService.Create(reservationItem);
                        additionalProductGuidList.Add(id);

                    }
                }
            }

            return additionalProductGuidList;
        }

        public void createReservationItemForManualPayment(ReservationCustomerParameters reservationCustomerParameter,
                                                              ReservationDateandBranchParameters reservationDateandBranchParameter,
                                                              ReservationEquipmentParameters reservationEquipmentParameter,
                                                              ReservationAdditionalProductParameters reservationAdditionalProductParameters,
                                                              Guid reservationId,
                                                              Guid currencyId,
                                                              int totalDuration,
                                                              int paymentType,
                                                              int itemTypeCode)
        {
            Entity reservationItem = new Entity("rnt_reservationitem");

            reservationItem["rnt_baseprice"] = new Money((decimal)reservationAdditionalProductParameters.actualAmount);
            reservationItem["rnt_reservationid"] = new EntityReference("rnt_reservation", reservationId);
            reservationItem["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", reservationAdditionalProductParameters.productId);
            reservationItem["rnt_reservationduration"] = totalDuration;
            reservationItem["statuscode"] = new OptionSetValue(1);
            reservationItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.pickupBranchId);
            reservationItem["rnt_pickupdatetime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
            reservationItem["rnt_dropoffdatetime"] = reservationDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
            reservationItem["rnt_offset"] = StaticHelper.offset;
            reservationItem["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.dropoffBranchId);
            reservationItem["rnt_groupcodeinformationsid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
            reservationItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var ratio = systemParameterBL.getSystemTaxRatio();
            reservationItem["rnt_taxratio"] = Convert.ToDecimal(ratio);

            //decimal totalAmount = CommonHelper.calculateAdditionalProductPrice(reservationAdditionalProductParameters.priceCalculationType,
            //                                                                   reservationAdditionalProductParameters.monthlyPackagePrice,
            //                                                                   reservationAdditionalProductParameters.actualAmount.Value,
            //                                                                   totalDuration);

            reservationItem["rnt_totalamount"] = new Money((decimal)reservationAdditionalProductParameters.actualAmount);
            reservationItem["rnt_itemtypecode"] = new OptionSetValue(itemTypeCode);
            reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
            reservationItem["rnt_name"] = reservationAdditionalProductParameters.productName;
            reservationItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", currencyId);
            reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);

            var id = this.OrgService.Create(reservationItem);
        }

        public Guid createReservationItemForAdditionalProduct(ReservationCustomerParameters reservationCustomerParameter,
                                                              ReservationDateandBranchParameters reservationDateandBranchParameter,
                                                              ReservationEquipmentParameters reservationEquipmentParameter,
                                                              ReservationAdditionalProductParameters reservationAdditionalProductParameters,
                                                              Guid reservationId,
                                                              Guid currencyId,
                                                              int totalDuration,
                                                              int paymentType,
                                                              int itemTypeCode,
                                                              int channelCode,
                                                              string pricingType)

        {

            Entity reservationItem = new Entity("rnt_reservationitem");

            reservationItem["rnt_baseprice"] = new Money((decimal)reservationAdditionalProductParameters.actualAmount);
            reservationItem["rnt_monthlypackageprice"] = new Money((decimal)reservationAdditionalProductParameters.monthlyPackagePrice);
            reservationItem["rnt_reservationid"] = new EntityReference("rnt_reservation", reservationId);
            reservationItem["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", reservationAdditionalProductParameters.productId);
            reservationItem["rnt_reservationduration"] = totalDuration;
            reservationItem["statuscode"] = new OptionSetValue(1);
            reservationItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.pickupBranchId);
            reservationItem["rnt_pickupdatetime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
            reservationItem["rnt_dropoffdatetime"] = reservationDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
            reservationItem["rnt_offset"] = StaticHelper.offset;
            reservationItem["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.dropoffBranchId);
            reservationItem["rnt_groupcodeinformationsid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
            reservationItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var ratio = systemParameterBL.getSystemTaxRatio();
            reservationItem["rnt_taxratio"] = Convert.ToDecimal(ratio);

            decimal totalAmount = CommonHelper.calculateAdditionalProductPrice(reservationAdditionalProductParameters.priceCalculationType,
                                                                               reservationAdditionalProductParameters.monthlyPackagePrice,
                                                                               reservationAdditionalProductParameters.actualAmount.Value,
                                                                               totalDuration);

            reservationItem["rnt_totalamount"] = new Money(totalAmount);
            reservationItem["rnt_itemtypecode"] = new OptionSetValue(itemTypeCode);
            reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
            reservationItem["rnt_name"] = reservationAdditionalProductParameters.productName;
            reservationItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", currencyId);
            reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
            reservationItem["rnt_channelcode"] = new OptionSetValue(channelCode);
            reservationItem["rnt_billingtypecode"] = CommonHelper.decideBillingType((int)rnt_reservationitem_rnt_itemtypecode.AdditionalProduct, pricingType, reservationAdditionalProductParameters.billingType);
            return this.OrgService.Create(reservationItem);
        }
        public Guid createAdditionalProductForCancelReservationFromReservationItemWithInitializeFromRequest(Guid reservationItemId, Guid? invoiceId, decimal price)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            Entity newAdditionalProduct = xrmHelper.initializeFromRequest("rnt_reservationitem", reservationItemId, "rnt_reservationitem");

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var productCode = configurationBL.GetConfigurationByName("additionalProduct_cancellationFeeCode");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(productCode, new string[] { "rnt_name" });

            newAdditionalProduct["rnt_billingtypecode"] = new OptionSetValue((int)rnt_BillingTypeCode.Individual);
            newAdditionalProduct.Attributes["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", additionalProduct.Id);
            newAdditionalProduct.Attributes["rnt_name"] = additionalProduct.GetAttributeValue<string>("rnt_name");
            newAdditionalProduct.Attributes["rnt_itemtypecode"] = new OptionSetValue(3);
            newAdditionalProduct.Attributes["rnt_baseprice"] = new Money(price);
            newAdditionalProduct.Attributes["rnt_totalamount"] = new Money(price);
            newAdditionalProduct.Attributes["statuscode"] = new OptionSetValue((int)ReservationItemEnums.StatusCode.Completed);
            var itemId = this.OrgService.Create(newAdditionalProduct);
            if (invoiceId.HasValue)
            {
                InvoiceItemBL invoiceItemBL = new InvoiceItemBL(this.OrgService);
                invoiceItemBL.createInvoiceItem(invoiceId.Value, null, itemId, newAdditionalProduct.GetAttributeValue<EntityReference>("transactioncurrencyid").Id, additionalProduct.GetAttributeValue<string>("rnt_name"), price);
            }
            return itemId;
        }
        public UpdateReservationItemAdditionalProductAmountandDateBranchFieldsResponse UpdateReservationItemAdditionalProductAmountandDateBranchFields(Guid reservationItemAdditionalProductId,
                                                                                       ReservationCustomerParameters reservationCustomerParameter,
                                                                                       ReservationDateandBranchParameters reservationDateandBranchParameter,
                                                                                       ReservationAdditionalProductParameters reservationAdditionalProductParameter,
                                                                                       Guid reservationId,
                                                                                       ReservationEquipmentParameters reservationEquipmentParameter,
                                                                                       int totalDuration,
                                                                                       int paymentType,
                                                                                       bool isDateorBranchChanged,
                                                                                       bool isContract)

        {
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var c = reservationRepository.getReservationById(reservationId, new string[] { "rnt_paymentmethodcode" });

            var item = reservationAdditionalProductParameter;

            //need to get old prices = 

            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var resItem = reservationItemRepository.getReservationItemByIdByGivenColumns(reservationItemAdditionalProductId, new string[] { "rnt_baseprice","rnt_monthlypackageprice", "rnt_additionalproductid" });

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var maturityCode = configurationBL.GetConfigurationByName("additionalProduct_MaturityDifference");
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var add = additionalProductRepository.getAdditionalProductByProductCode(maturityCode);
            this.Trace("maturityCode " + maturityCode);
            this.Trace("add.Id " + add.Id);
            this.Trace("resItem.addId " + resItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id);
            var bsePrice = resItem.GetAttributeValue<Money>("rnt_baseprice");
            //this.Trace("bsePrice :" + bsePrice);
            item.actualAmount = bsePrice.Value;

            var totalAmount = CommonHelper.calculateAdditionalProductPrice(item.priceCalculationType, resItem.GetAttributeValue<Money>("rnt_monthlypackageprice").Value, (decimal)item.actualAmount, totalDuration);
            if (isDateorBranchChanged || (resItem.Contains("rnt_additionalproductid") && add.Id == resItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id))
            {
                this.Trace("item.actualAmount : " + item.actualAmount);
                this.Trace("totalAmount : " + totalAmount);

                Entity reservationItem = new Entity("rnt_reservationitem");
                reservationItem["rnt_reservationduration"] = totalDuration;
                //reservationItem["statuscode"] = new OptionSetValue(1);
                reservationItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.pickupBranchId);
                reservationItem["rnt_pickupdatetime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
                reservationItem["rnt_dropoffdatetime"] = reservationDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
                reservationItem["rnt_offset"] = StaticHelper.offset;
                reservationItem["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.dropoffBranchId);
                reservationItem["rnt_groupcodeinformationsid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
                if (!isContract)
                {
                    reservationItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
                }
                SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
                var ratio = systemParameterBL.getSystemTaxRatio();
                reservationItem["rnt_taxratio"] = Convert.ToDecimal(ratio);
                if (resItem.Contains("rnt_additionalproductid") && add.Id == resItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id)
                {
                    this.Trace("this is maturity");
                    reservationItem["rnt_totalamount"] = new Money(item.actualTotalAmount.Value);
                    reservationItem["rnt_baseprice"] = new Money(item.actualTotalAmount.Value);
                }
                else
                {
                    this.Trace("this is not maturity");
                    reservationItem["rnt_totalamount"] = new Money(totalAmount);
                    reservationItem["rnt_baseprice"] = new Money((decimal)item.actualAmount);
                }

                reservationItem["rnt_billingtypecode"] = CommonHelper.decideBillingType((int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct,
                                                                             c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value.ToString(), item.billingType);

                reservationItem.Id = reservationItemAdditionalProductId;
                this.OrgService.Update(reservationItem);
            }
            else
            {
                Entity reservationItem = new Entity("rnt_reservationitem");
                reservationItem["rnt_groupcodeinformationsid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
                if (!isContract)
                {
                    reservationItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
                }

                reservationItem["rnt_billingtypecode"] = CommonHelper.decideBillingType((int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct,
                                                                             c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value.ToString(), item.billingType);

                reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
                reservationItem.Id = reservationItemAdditionalProductId;
                this.OrgService.Update(reservationItem);
            }

            return new UpdateReservationItemAdditionalProductAmountandDateBranchFieldsResponse
            {
                totalAmount = totalAmount
            };

        }
        public void UpdateReservationItemEquipment(Guid reservationItemId,
                                                   ReservationCustomerParameters reservationCustomerParameter,
                                                   ReservationDateandBranchParameters reservationDateandBranchParameter,
                                                   ReservationEquipmentParameters reservationEquipmentParameter,
                                                   ReservationPriceParameters reservationPriceParameters,
                                                   int totalDuration,
                                                   string trackingNumber,
                                                   bool isDateorBranchChanged,
                                                   bool isContract)

        {
            PriceCalculationSummariesBL priceCalculationSummariesBL = new PriceCalculationSummariesBL(this.OrgService);
           
            this.Trace("UpdateReservationItemEquipment : reservationPriceParameters :" + JsonConvert.SerializeObject(reservationPriceParameters));
            Entity reservationItem = new Entity("rnt_reservationitem");
            //reservationItem["statuscode"] = new OptionSetValue(1);          
            reservationItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.pickupBranchId);
            if (isDateorBranchChanged)
            {
                reservationItem["rnt_reservationduration"] = totalDuration;
                reservationItem["rnt_pickupdatetime"] = reservationDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
                reservationItem["rnt_dropoffdatetime"] = reservationDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
                reservationItem["rnt_offset"] = StaticHelper.offset;
            }
            reservationItem["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", reservationDateandBranchParameter.dropoffBranchId);
            reservationItem["rnt_groupcodeinformationsid"] = new EntityReference("rnt_groupcodeinformations", reservationEquipmentParameter.groupCodeInformationId);
            if (!isContract)
            {
                reservationItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
            }
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var ratio = systemParameterBL.getSystemTaxRatio();
            reservationItem["rnt_taxratio"] = Convert.ToDecimal(ratio);
            reservationItem["rnt_baseprice"] = new Money(reservationPriceParameters.price);
            reservationItem["rnt_totalamount"] = new Money(reservationPriceParameters.price);
            reservationItem["rnt_itemtypecode"] = new OptionSetValue(1);
            reservationItem["rnt_name"] = reservationEquipmentParameter.itemName;
            reservationItem["rnt_mongodbtrackingnumber"] = trackingNumber;
            reservationItem["rnt_customerid"] = new EntityReference("contact", reservationCustomerParameter.contactId);
            if (reservationPriceParameters.campaignId.HasValue && reservationPriceParameters.campaignId.Value != Guid.Empty)
            {
                // this.Trace("has campaign id");
                reservationItem["rnt_campaignid"] = new EntityReference("rnt_campaign", reservationPriceParameters.campaignId.Value);
            }
            else
            {
                //this.Trace("doesnt has campaign id");
                reservationItem["rnt_campaignid"] = null;
            }
            reservationItem["rnt_billingtypecode"] = CommonHelper.decideBillingType((int)rnt_reservationitem_rnt_itemtypecode.Equipment, reservationPriceParameters.pricingType, reservationEquipmentParameter.billingType);
            reservationItem.Id = reservationItemId;
            this.OrgService.Update(reservationItem);

        }

        public MongoDBResponse callReservationItemActionInMongoDB(Entity entity, string messageName)
        {
            try
            {
                this.Trace("serialized entity :  " + messageName.ToLower());
                if ((entity.Attributes.Contains("rnt_mongodbintegrationtrigger") && messageName.ToLower() == "create") ||
                    messageName.ToLower() == "update")
                {

                    OrganizationRequest request = new OrganizationRequest("rnt_createreservationiteminmongodb");
                    request["MessageName"] = messageName;
                    request["ReservationItemEntity"] = entity;
                    var response = this.OrgService.Execute(request);

                    var result = Convert.ToString(response.Results["ExecutionResult"]);
                    var mongodbId = Convert.ToString(response.Results["ID"]);

                    if (!string.IsNullOrEmpty(result))
                    {
                        MongoDBResponse.ReturnError(result);
                    }
                    //means get id from mongodb
                    return MongoDBResponse.ReturnSuccessWithId(mongodbId);
                }
                //if triggered fields is not populated return success without any integration with mongodb
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }
        public MongoDBResponse CreateReservationItemInMongoDB(Entity entity)
        {

            //Entity reservationItem = new ReservationItemRepository(this.OrgService).GetReservationItemById(entityReference.Id);
            Entity reservationItem = entity;
            //todo null check

            //first get the webapi url from crm config
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            //request initilization
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "CreateReservationInMongoDB", Method.POST);
            //prepare parameters
            var reservationItemData = this.buildMongoDBReservationData(reservationItem);

            this.Trace("serialized before webservice call" + JsonConvert.SerializeObject(reservationItemData));
            //add parameter to body 
            helper.AddJsonParameter<ReservationItemData>(reservationItemData);
            // execute the request
            var responseReservation = helper.Execute<MongoDBResponse>();
            //check response
            if (responseReservation == null)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContent(UserId, "IntegrationError", mongoDbXmlPath);
                return MongoDBResponse.ReturnError(message);
            }

            if (!responseReservation.Result)
            {
                return MongoDBResponse.ReturnError(responseReservation.ExceptionDetail);
            }
            return responseReservation;

        }
        public MongoDBResponse UpdateReservationItemInMongoDB(Entity entity)
        {
            this.Trace("UpdateReservationItemInMongoDB start");
            Entity reservationItem = entity;
            //todo null check            
            //first get the webapi url from crm config
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            //request initilization
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "UpdateReservationInMongoDB", Method.POST);
            //prepare parameters
            var reservationItemData = this.buildMongoDBReservationData(reservationItem);
            //add parameter to body 
            helper.AddJsonParameter<ReservationItemData>(reservationItemData);
            helper.AddQueryParameter("id", reservationItem.GetAttributeValue<string>("rnt_mongodbid"));
            this.Trace("rnt_mongodbid : " + reservationItem.GetAttributeValue<string>("rnt_mongodbid"));
            if (string.IsNullOrEmpty(reservationItem.GetAttributeValue<string>("rnt_mongodbid")))
            {
                this.Trace("rnt_mongodbid is null");
                return new MongoDBResponse
                {
                    Result = false,
                    ExceptionDetail = StaticHelper.mongoDBNotCreatedYet
                };
            }
            var responseReservation = helper.Execute<MongoDBResponse>();
            if (responseReservation == null)
            {
                this.Trace("responseReservation == null MongoDBResponse");

                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContent(UserId, "IntegrationError", mongoDbXmlPath);
                return MongoDBResponse.ReturnError(message);
            }
            //check response
            if (!responseReservation.Result)
            {
                this.Trace("responseReservation.ExceptionDetail" + responseReservation.ExceptionDetail);
                return MongoDBResponse.ReturnError(responseReservation.ExceptionDetail);
            }

            return responseReservation;
        }
        public MongoDBResponse UpdateReservationDeposit(Entity reservationItem)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            //request initilization
            RestSharpHelper helper = new RestSharpHelper(responseUrl, "UpdateReservationDeposit", Method.POST);

            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var res = reservationRepository.getReservationById(reservationItem.GetAttributeValue<EntityReference>("rnt_reservationid").Id, new string[] { "rnt_depositamount"});

            helper.AddQueryParameter("depositAmount", res.GetAttributeValue<Money>("rnt_depositamount").Value.ToString());
            helper.AddQueryParameter("id", reservationItem.GetAttributeValue<string>("rnt_mongodbid"));
            
            if (string.IsNullOrEmpty(reservationItem.GetAttributeValue<string>("rnt_mongodbid")))
            {
                this.Trace("rnt_mongodbid is null");
                return new MongoDBResponse
                {
                    Result = false,
                    ExceptionDetail = StaticHelper.mongoDBNotCreatedYet
                };
            }
            var responseReservation = helper.Execute<MongoDBResponse>();
            if (responseReservation == null)
            {
                this.Trace("responseReservation == null MongoDBResponse");

                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContent(UserId, "IntegrationError", mongoDbXmlPath);
                return MongoDBResponse.ReturnError(message);
            }
            //check response
            if (!responseReservation.Result)
            {
                this.Trace("responseReservation.ExceptionDetail" + responseReservation.ExceptionDetail);
                return MongoDBResponse.ReturnError(responseReservation.ExceptionDetail);
            }

            return responseReservation;
        }
        public Entity getReservationItemEquipment(Guid reservationId)
        {
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var item = reservationItemRepository.getActiveReservationItemEquipment(reservationId);
            return item;
        }

        public ReservationItemResponse getReservationItems(string reservationId)
        {
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var items = reservationItemRepository.getActiveReservationItemsByReservationId(new Guid(reservationId));

            var response = new ReservationItemResponse();

            //now lets create the return object
            //get the reservation item type == 1 means not additional products
            var reservationItem = items.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == 1).FirstOrDefault();

            var customerType = reservationItem.GetAttributeValue<EntityReference>("rnt_customerid").LogicalName;
            //fill the response object
            response.reservationItemDetail = new ReservationItemDetail
            {
                SelectedDateAndBranch = new SelectedDateAndBranchInformation
                {
                    dropoffBranchId = reservationItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                    pickupBranchId = reservationItem.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id,
                    dropoffDate = reservationItem.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                    pickupDate = reservationItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                },
                SelectedCustomer = new SelectedIndividualCustomerInformation
                {
                    customerType = customerType,
                    contactId = customerType == "contact" ? (Guid?)reservationItem.GetAttributeValue<EntityReference>("rnt_customerid").Id : null
                }
            };
            //get the reservation item type == 2 additional products
            var additonalProducts = items.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == 2).FirstOrDefault();

            return response;
        }
        public List<Guid> getActiveReservationItemsGuidsByReservationId(Guid reservationId)
        {
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            var reservationItems = reservationItemRepository.getActiveReservationItemsByReservationId(reservationId);
            List<Guid> guids = new List<Guid>();
            foreach (var item in reservationItems)
            {
                guids.Add(item.Id);
            }
            return guids;
        }
        public void deactiveReservationItemById(Guid rnt_reservationItemId, int statusCode)
        {
            XrmHelper h = new XrmHelper(this.OrgService);
            h.setState("rnt_reservationitem", rnt_reservationItemId, (int)GlobalEnums.StateCode.Passive, statusCode);
        }

        public void completeReservationItem(Guid rnt_reservationItemId)
        {
            XrmHelper h = new XrmHelper(this.OrgService);
            h.setState("rnt_reservationitem", rnt_reservationItemId, (int)GlobalEnums.StateCode.Active, (int)ReservationItemEnums.StatusCode.Completed);
        }
        public void rentReservationItem(Guid rnt_reservationItemId)
        {
            XrmHelper h = new XrmHelper(this.OrgService);
            h.setState("rnt_reservationitem", rnt_reservationItemId, (int)GlobalEnums.StateCode.Active, (int)ReservationItemEnums.StatusCode.Rental);
        }

        public void updateReservationItemChangeReason(Guid reservationItemId, int changeReason)
        {
            Entity entity = new Entity("rnt_reservationitem");
            entity.Id = reservationItemId;
            entity.Attributes["rnt_changereason"] = new OptionSetValue(changeReason);
            this.OrgService.Update(entity);

        }
        public void updateChangedEquipmentChangeTypeandChangeReason(Guid reservationItemId, int changeType)
        {
            Entity entity = new Entity("rnt_reservationitem");
            entity.Id = reservationItemId;
            //change type 
            if (changeType == (int)ClassLibrary._Enums_1033.rnt_ChangeType.Upsell ||
                changeType == (int)ClassLibrary._Enums_1033.rnt_ChangeType.Downsell)
            {
                entity["rnt_changereason"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_ChangeReasonCode.CustomerDemand);
            }
            else if (changeType == (int)ClassLibrary._Enums_1033.rnt_ChangeType.Upgrade ||
                     changeType == (int)ClassLibrary._Enums_1033.rnt_ChangeType.Downgrade)
            {
                entity["rnt_changereason"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_ChangeReasonCode.RentGo);
            }
            else if (changeType == 10)
            {
                entity["rnt_changereason"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_ChangeReasonCode.CustomerDemand);
            }

            if (changeType != 10)
            {
                entity["rnt_changetype"] = new OptionSetValue(changeType);
            }

            this.OrgService.Update(entity);

        }
        public void updateReservationEquipmentChangeTypeById(Guid reservationItemId, int changeType)
        {
            if (changeType != (int)GlobalEnums.GroupCodeChangeType.NotChanged)
            {
                Entity entity = new Entity("rnt_reservationitem");
                entity.Id = reservationItemId;
                entity.Attributes["rnt_changetype"] = new OptionSetValue(changeType);
                this.OrgService.Update(entity);
            }
        }

        public void createInvoiceAddress(int paymentType,
                                        ReservationCustomerParameters reservationCustomerParameter,
                                        Guid reservationId,
                                        decimal totalPrice)
        {
            if (paymentType == (int)PaymentEnums.PaymentType.PayNow)
            {
                DocumentInvoiceAddressBL documentInvoiceAddress = new DocumentInvoiceAddressBL(this.OrgService, this.TracingService);
                documentInvoiceAddress.createDocumentInvoiceAddress(reservationCustomerParameter.invoiceAddress,
                                                                    new EntityReference("rnt_reservationitem", reservationId),
                                                                    null,
                                                                    totalPrice);
            }

        }

        public ReservationItemData buildMongoDBReservationData(Entity reservationItem)
        {
            var reservationRef = reservationItem.GetAttributeValue<EntityReference>("rnt_reservationid");
            var reservation = this.OrgService.Retrieve(reservationRef.LogicalName,
                                                       reservationRef.Id,
                                                       new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_reservationnumber",
                                                                                             "rnt_pnrnumber",
                                                                                             "rnt_corporateid",
                                                                                             "rnt_reservationtypecode",
                                                                                             "rnt_reservationchannelcode",
                                                                                             "rnt_paymentchoicecode",
                                                                                             "rnt_depositamount",
                                                                                             "rnt_overkilometerprice",
                                                                                             "rnt_pricinggroupcodeid",
                                                                                             "transactioncurrencyid",
                                                                                             "rnt_cancellationtime",
                                                                                             "rnt_noshowtime",
                                                                                             "rnt_totalamount",
                                                                                             "rnt_processindividualprices",
                                                                                             "rnt_paymentmethodcode",
                                                                                             "rnt_dummycontactinformation",
                                                                                             "rnt_ismonthly",
                                                                                             "rnt_generaltotalamount",
                                                                                             "rnt_howmanymonths"));

            CurrencyRepository currencyRepository = new CurrencyRepository(this.OrgService);
            var currency = currencyRepository.getCurrencyById(reservation.GetAttributeValue<EntityReference>("transactioncurrencyid").Id, new string[] { "currencysymbol", "exchangerate" });

            PaymentPlanRepository paymentPlanRepository = new PaymentPlanRepository(this.OrgService);
            var plans = paymentPlanRepository.getPaymentPlansByReservationId(reservation.Id);

            var _plans = new PaymentPlanMapper().buildPaymentPlans(plans,
                                                                   reservationItem.GetAttributeValue<EntityReference>("rnt_groupcodeinformationsid").Id,
                                                                   reservationItem.GetAttributeValue<EntityReference>("rnt_groupcodeinformationsid").Name);

            ReservationItemData reservationItemData = new ReservationItemData
            {
                dummyContactInformation = reservation.GetAttributeValue<string>("rnt_dummycontactinformation"),
                isMonthly = reservation.GetAttributeValue<bool>("rnt_ismonthly"),
                monthValue = reservation.Attributes.Contains("rnt_howmanymonths") ?
                             reservation.GetAttributeValue<OptionSetValue>("rnt_howmanymonths").Value :
                              0,
                paymentPlans = _plans,
                transactionCurrencyCode = currency.GetAttributeValue<string>("currencysymbol"),
                transactionCurrencyName = reservation.Contains("transactioncurrencyid") ? reservation.GetAttributeValue<EntityReference>("transactioncurrencyid").Name : null,
                corporateCustomerName = reservation.Contains("rnt_corporateid") ? reservation.GetAttributeValue<EntityReference>("rnt_corporateid").Name : null,
                corporateCustomerId = reservation.Contains("rnt_corporateid") ? (Guid?)reservation.GetAttributeValue<EntityReference>("rnt_corporateid").Id : null,
                processIndividualPrices = reservation.Contains("rnt_processindividualprices") ? reservation.GetAttributeValue<bool>("rnt_processindividualprices") : false,
                billingType = reservationItem.GetAttributeValue<OptionSetValue>("rnt_billingtypecode").Value,
                ReservationItemId = Convert.ToString(reservationItem.Id),
                ChangeReason = reservationItem.Attributes.Contains("rnt_changereason") ?
                              (int?)reservationItem.GetAttributeValue<OptionSetValue>("rnt_changereason").Value :
                              null,
                GroupCodeInformationId = reservationItem.Attributes.Contains("rnt_groupcodeinformationsid") ?
                                        Convert.ToString(reservationItem.GetAttributeValue<EntityReference>("rnt_groupcodeinformationsid").Id) :
                                         null,
                GroupCodeInformationName = reservationItem.Attributes.Contains("rnt_groupcodeinformationsid") ?
                                         reservationItem.GetAttributeValue<EntityReference>("rnt_groupcodeinformationsid").Name :
                                         null,
                pricingGroupCodeId = reservation.Attributes.Contains("rnt_pricinggroupcodeid") ?
                                        Convert.ToString(reservation.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id) :
                                         null,
                pricingGroupCodeName = reservation.Attributes.Contains("rnt_pricinggroupcodeid") ?
                                         reservation.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Name :
                                         null,
                CreatedOn = reservationItem.GetAttributeValue<DateTime>("createdon").ToUniversalTime(),
                CreatedBy = Convert.ToString(reservationItem.GetAttributeValue<EntityReference>("createdby").Id),
                ModifiedOn = reservationItem.GetAttributeValue<DateTime>("modifiedon").ToUniversalTime(),
                ModifiedBy = Convert.ToString(reservationItem.GetAttributeValue<EntityReference>("modifiedby").Id),
                StatusCode = reservationItem.GetAttributeValue<OptionSetValue>("statuscode").Value,
                StateCode = reservationItem.GetAttributeValue<OptionSetValue>("statecode").Value,
                CurrencyId = reservationItem.Attributes.Contains("transactioncurrencyid") ?
                                        Convert.ToString(reservationItem.GetAttributeValue<EntityReference>("transactioncurrencyid").Id) :
                                         null,
                DropoffBranchId = reservationItem.Attributes.Contains("rnt_dropoffbranchid") ?
                                        Convert.ToString(reservationItem.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id) :
                                         null,
                DropoffBranchName = reservationItem.Attributes.Contains("rnt_dropoffbranchid") ?
                                         reservationItem.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Name :
                                         null,
                PickupBranchId = reservationItem.Attributes.Contains("rnt_pickupbranchid") ?
                                        Convert.ToString(reservationItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id) :
                                         null,
                PickupBranchName = reservationItem.Attributes.Contains("rnt_pickupbranchid") ?
                                         reservationItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name :
                                         null,
                DropoffTime = reservationItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime").AddMinutes(-StaticHelper.offset),
                PickupTime = reservationItem.GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset),
                CancellationTime = reservation.Attributes.Contains("rnt_cancellationtime") ?
                                   (DateTime?)reservation.GetAttributeValue<DateTime>("rnt_cancellationtime").AddMinutes(-StaticHelper.offset) :
                                    null,
                NoShowTime = reservation.Attributes.Contains("rnt_noshowtime") ?
                             (DateTime?)reservation.GetAttributeValue<DateTime>("rnt_noshowtime").AddMinutes(-StaticHelper.offset) :
                             null,
                EquipmentId = reservationItem.Attributes.Contains("rnt_equipmentid") ?
                                        Convert.ToString(reservationItem.GetAttributeValue<EntityReference>("rnt_equipmentid").Id) :
                                         null,
                EquipmentName = reservationItem.Attributes.Contains("rnt_equipmentid") ?
                                         reservationItem.GetAttributeValue<EntityReference>("rnt_equipmentid").Name :
                                         null,
                OwnernId = reservationItem.Attributes.Contains("ownerid") ?
                                        Convert.ToString(reservationItem.GetAttributeValue<EntityReference>("ownerid").Id) :
                                         null,
                OwnerName = reservationItem.Attributes.Contains("ownerid") ?
                                         reservationItem.GetAttributeValue<EntityReference>("ownerid").Name :
                                         null,
                CustomerId = reservationItem.Attributes.Contains("rnt_customerid") ?
                                        Convert.ToString(reservationItem.GetAttributeValue<EntityReference>("rnt_customerid").Id) :
                                         null,
                CustomerName = reservationItem.Attributes.Contains("rnt_customerid") ?
                                         reservationItem.GetAttributeValue<EntityReference>("rnt_customerid").Name :
                                         null,
                ItemTypeCode = reservationItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value,
                NetAmount = reservationItem.GetAttributeValue<Money>("rnt_netamount").Value,
                TaxRatio = reservationItem.GetAttributeValue<decimal>("rnt_taxratio"),
                TotalAmount = reservationItem.GetAttributeValue<Money>("rnt_totalamount").Value,
                ReservationId = reservationItem.Attributes.Contains("rnt_reservationid") ?
                                        Convert.ToString(reservationItem.GetAttributeValue<EntityReference>("rnt_reservationid").Id) :
                                         null,
                campaignId = reservationItem.Attributes.Contains("rnt_campaignid") ?
                                        (Guid?)reservationItem.GetAttributeValue<EntityReference>("rnt_campaignid").Id :
                                         null,
                campaignName = reservationItem.Attributes.Contains("rnt_campaignid") ?
                                        reservationItem.GetAttributeValue<EntityReference>("rnt_campaignid").Name :
                                         null,
                ReservationNumber = reservation.GetAttributeValue<string>("rnt_reservationnumber"),
                PnrNumber = reservation.GetAttributeValue<string>("rnt_pnrnumber"),
                ReservationType = reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value,
                PaymentChoice = reservation.Attributes.Contains("rnt_paymentchoicecode") ? reservation.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value : 0,
                ReservationChannel = reservation.GetAttributeValue<OptionSetValue>("rnt_reservationchannelcode").Value,
                DepositAmount = reservation.Attributes.Contains("rnt_depositamount") ? reservation.GetAttributeValue<Money>("rnt_depositamount").Value : 0,
                ReservationTotalAmount = reservation.Attributes.Contains("rnt_generaltotalamount") ? reservation.GetAttributeValue<Money>("rnt_generaltotalamount").Value : decimal.Zero,
                Offset = StaticHelper.offset,
                trackingNumber = reservationItem.GetAttributeValue<string>("rnt_mongodbtrackingnumber"),
                overKilometerPrice = reservation.GetAttributeValue<Money>("rnt_overkilometerprice").Value,
                PaymentMethod = reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value,
            };

            this.Trace("before sending data to mongodb reservation update : " + JsonConvert.SerializeObject(reservationItemData));
            return reservationItemData;
        }
        //private decimal calculateTotalAmountAdditionalProducts(int priceCalculationType, decimal actualAmount, int totalDuration)
        //{
        //    var totalAmount = decimal.Zero;
        //    if (priceCalculationType == 1)
        //        totalAmount = (decimal)actualAmount * totalDuration;

        //    else if (priceCalculationType == 2)
        //    {
        //        totalAmount = (decimal)actualAmount;
        //    }

        //    return totalAmount;
        //}

    }
}
