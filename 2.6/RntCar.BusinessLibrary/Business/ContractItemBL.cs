using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Tablet;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RntCar.BusinessLibrary.Business
{
    public class ContractItemBL : BusinessHandler
    {
        public ContractItemBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public ContractItemBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
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
        public Entity buildContractItemWithInitializeFromRequest(Guid contractItemId, int channelCode)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            this.Trace("initialize request start : " + DateTime.Now);
            Entity contractItem = xrmHelper.initializeFromRequest("rnt_contractitem", contractItemId, "rnt_contractitem");
            contractItem["rnt_channelcode"] = new OptionSetValue(channelCode);
            this.Trace("initialize request start : " + DateTime.Now);
            return contractItem;
        }
        public void updateContractItemChangeTypeandChangeReason(Guid contractItemId, int changeType)
        {
            Entity entity = new Entity("rnt_contractitem");
            entity["rnt_changetype"] = new OptionSetValue(changeType);
            if (changeType == (int)ClassLibrary._Enums_1033.rnt_ChangeType.Upsell || changeType == (int)ClassLibrary._Enums_1033.rnt_ChangeType.Downsell)
            {
                //prevent not to disturb change history
                entity["rnt_changereason"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_ChangeReasonCode.CustomerDemand);
            }
            else
            {
                entity["rnt_changereason"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_ChangeReasonCode.RentGo);
            }
            entity.Id = contractItemId;
            this.updateEntity(entity);
        }
        public Guid updateContractandContractItemWithNewGroupCodeRelatedFields(Entity entity,
                                                                               UpdateContractandContractItemWithNewGroupCodeParamaters updateContractandContractItemWithNewGroupCodeParamaters,
                                                                               DateTime utcNow,
                                                                               bool equipmentChangedBefore)
        {
            ContractHelper contractHelper = new ContractHelper(this.OrgService);

            entity["statuscode"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Rental);
            entity["rnt_name"] = updateContractandContractItemWithNewGroupCodeParamaters.contractItemName;
            entity["rnt_groupcodeinformations"] = new EntityReference("rnt_groupcodeinformations", updateContractandContractItemWithNewGroupCodeParamaters.groupCodeInformationId);

            entity.Attributes.Remove("rnt_mongodbid");
            entity.Id = Guid.NewGuid();
            if (updateContractandContractItemWithNewGroupCodeParamaters.changeType == (int)rnt_ChangeType.Upsell ||
               updateContractandContractItemWithNewGroupCodeParamaters.changeType == (int)rnt_ChangeType.Downsell)
            {
                //prevent not to disturb change history
                entity["rnt_totalamount"] = new Money(updateContractandContractItemWithNewGroupCodeParamaters.totalPrice);
                entity["rnt_baseprice"] = new Money(updateContractandContractItemWithNewGroupCodeParamaters.totalPrice);
                entity["rnt_mongodbtrackingnumber"] = updateContractandContractItemWithNewGroupCodeParamaters.trackingNumber;
                //if changetype upsell or downsell remove campaign from entity
                entity["rnt_campaignid"] = null;
            }


            var minutes = CommonHelper.calculateTotalDurationInMinutes(updateContractandContractItemWithNewGroupCodeParamaters.pickupTimeStamp, updateContractandContractItemWithNewGroupCodeParamaters.dropoffTimeStamp);
            //var totalDays = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(updateContractandContractItemWithNewGroupCodeParamaters.pickupTimeStamp.converttoDateTime(),
            //                                                                                updateContractandContractItemWithNewGroupCodeParamaters.dropoffTimeStamp.converttoDateTime());

            if (updateContractandContractItemWithNewGroupCodeParamaters.isManualProcess)
            {
                entity["rnt_pickupdatetime"] = updateContractandContractItemWithNewGroupCodeParamaters.manualPickupTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset);
                entity["rnt_dropoffdatetime"] = updateContractandContractItemWithNewGroupCodeParamaters.manualPickupTimeStamp.converttoDateTime().AddMinutes(minutes).AddMinutes(StaticHelper.offset);
            }
            else
            {
                entity["rnt_pickupdatetime"] = utcNow.AddMinutes(StaticHelper.offset);
                entity["rnt_dropoffdatetime"] = utcNow.AddMinutes(minutes).AddMinutes(StaticHelper.offset);
            }
            if (updateContractandContractItemWithNewGroupCodeParamaters.userInformation != null)
            {
                entity["rnt_externalusercreatedbyid"] = new EntityReference("rnt_serviceuser", updateContractandContractItemWithNewGroupCodeParamaters.userInformation.userId);
            }

            entity["rnt_equipment"] = new EntityReference("rnt_equipment", updateContractandContractItemWithNewGroupCodeParamaters.equipmentId);
            entity["statuscode"] = new OptionSetValue((int)ContractItemEnums.StatusCode.Rental);


            var contractItemId = contractHelper.createContractItemSafelyWithoutMongoDbIntegrationTrigger(entity);

            #region Contract Update 
            Entity entityContract = new Entity("rnt_contract");
            entityContract["rnt_groupcodeid"] = new EntityReference("rnt_groupcodeinformations", updateContractandContractItemWithNewGroupCodeParamaters.groupCodeInformationId);
            if (updateContractandContractItemWithNewGroupCodeParamaters.changeType == (int)rnt_ChangeType.Upsell ||
                updateContractandContractItemWithNewGroupCodeParamaters.changeType == (int)rnt_ChangeType.Downsell)
            {
                entityContract["rnt_pricinggroupcodeid"] = new EntityReference("rnt_groupcodeinformations", updateContractandContractItemWithNewGroupCodeParamaters.groupCodeInformationId);

                GroupCodeInformationBL groupCodeInformationBL = new GroupCodeInformationBL(this.OrgService);
                var groupCodeInformationForDocument = groupCodeInformationBL.getGroupCodeInformationDetailForDocument(updateContractandContractItemWithNewGroupCodeParamaters.groupCodeInformationId);

                entityContract["rnt_doublecreditcard"] = groupCodeInformationForDocument.doubleCreditCard;
                entityContract["new_findeks"] = groupCodeInformationForDocument.findeks;
                entityContract["new_minimumage"] = groupCodeInformationForDocument.minimumAge;
                entityContract["new_minimumdriverlicense"] = groupCodeInformationForDocument.minimumDriverLicense;
                entityContract["new_youngdriverage"] = groupCodeInformationForDocument.youngDriverAge;
                entityContract["new_youngdriverlicense"] = groupCodeInformationForDocument.youngDriverLicense;
                entityContract["rnt_overkilometerprice"] = new Money(groupCodeInformationForDocument.overKilometerPrice);
                entityContract["rnt_segmentcode"] = new OptionSetValue(groupCodeInformationForDocument.segmentCode);
                //var kilometer = calculateKmforContractDelivery(totalDays,
                //                                               updateContractandContractItemWithNewGroupCodeParamaters.groupCodeInformationId,
                //                                               updateContractandContractItemWithNewGroupCodeParamaters.additionalProducts);
                //entityContract["rnt_kilometerlimit"] = kilometer;
            }
            else if (updateContractandContractItemWithNewGroupCodeParamaters.changeType == (int)rnt_ChangeType.Downgrade ||
                     updateContractandContractItemWithNewGroupCodeParamaters.changeType == (int)rnt_ChangeType.Upgrade)
            {
                entityContract["rnt_pricinggroupcodeid"] = updateContractandContractItemWithNewGroupCodeParamaters.pricingGroupCode;

                //var kilometer = calculateKmforContractDelivery(totalDays,
                //                                              updateContractandContractItemWithNewGroupCodeParamaters.pricingGroupCode.Id,
                //                                              updateContractandContractItemWithNewGroupCodeParamaters.additionalProducts);
                //entityContract["rnt_kilometerlimit"] = kilometer;
            }
            if (!equipmentChangedBefore)
            {
                if (updateContractandContractItemWithNewGroupCodeParamaters.isManualProcess)
                {
                    entityContract["rnt_pickupdatetime"] = updateContractandContractItemWithNewGroupCodeParamaters.manualPickupTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset); ;
                    entityContract["rnt_dropoffdatetime"] = updateContractandContractItemWithNewGroupCodeParamaters.manualPickupTimeStamp.converttoDateTime().AddMinutes(minutes).AddMinutes(StaticHelper.offset);
                }
                else
                {
                    entityContract["rnt_pickupdatetime"] = utcNow.AddMinutes(StaticHelper.offset);
                    entityContract["rnt_dropoffdatetime"] = utcNow.AddMinutes(minutes).AddMinutes(StaticHelper.offset);
                }
            }

            entityContract["rnt_equipmentid"] = new EntityReference("rnt_equipment", updateContractandContractItemWithNewGroupCodeParamaters.equipmentId);
            entityContract["statuscode"] = new OptionSetValue((int)ContractEnums.StatusCode.Rental);
            entityContract.Id = updateContractandContractItemWithNewGroupCodeParamaters.contractId;
            this.updateEntity(entityContract);


            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var con = contractRepository.getContractById(updateContractandContractItemWithNewGroupCodeParamaters.contractId, new string[] { "rnt_pickupdatetime", "rnt_dropoffdatetime" });
            this.Trace("rnt_pickupdatetime : " + con.GetAttributeValue<DateTime>("rnt_pickupdatetime"));
            this.Trace("rnt_dropoffdatetime : " + con.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));
            var totalDays = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(con.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                                                            con.GetAttributeValue<DateTime>("rnt_dropoffdatetime"));

            Entity e = new Entity("rnt_contract");
            e.Id = updateContractandContractItemWithNewGroupCodeParamaters.contractId;
            if (updateContractandContractItemWithNewGroupCodeParamaters.changeType == (int)rnt_ChangeType.Upsell ||
                updateContractandContractItemWithNewGroupCodeParamaters.changeType == (int)rnt_ChangeType.Downsell)
            {
                var kilometer = calculateKmforContractDelivery(totalDays,
                                                               updateContractandContractItemWithNewGroupCodeParamaters.groupCodeInformationId,
                                                               updateContractandContractItemWithNewGroupCodeParamaters.additionalProducts);
                e["rnt_kilometerlimit"] = kilometer;
            }
            else if (updateContractandContractItemWithNewGroupCodeParamaters.changeType == (int)rnt_ChangeType.Downgrade ||
                    updateContractandContractItemWithNewGroupCodeParamaters.changeType == (int)rnt_ChangeType.Upgrade)
            {
                var kilometer = calculateKmforContractDelivery(totalDays,
                                                              updateContractandContractItemWithNewGroupCodeParamaters.pricingGroupCode.Id,
                                                              updateContractandContractItemWithNewGroupCodeParamaters.additionalProducts);
                e["rnt_kilometerlimit"] = kilometer;
            }
            this.OrgService.Update(e);
            return contractItemId;
            #endregion
        }
        public List<ContractItemResponse> createContractItemsWithInitializeFromRequest(List<Guid> reservationItemList,
                                                                                      Guid contractId,
                                                                                      Guid invoiceId,
                                                                                      int channelCode,
                                                                                      Guid? externalUserId)
        {
            List<ContractItemResponse> contractItemCreateResponses = new List<ContractItemResponse>();
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            foreach (var item in reservationItemList)
            {
                Entity contractItem = xrmHelper.initializeFromRequest("rnt_reservationitem", item, "rnt_contractitem");
                if (contractItem != null)
                {
                    contractItem.Attributes["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
                    contractItem["statuscode"] = new OptionSetValue((int)ContractItemEnums.StatusCode.WaitingforDelivery);
                    contractItem["rnt_channelcode"] = new OptionSetValue(channelCode);
                    //this.Trace("externalUserId : " + externalUserId);

                    if (channelCode == (int)RntCar.ClassLibrary._Enums_1033.rnt_ReservationChannel.Tablet &&
                       externalUserId != null)
                    {
                        contractItem["rnt_externalusercreatedbyid"] = new EntityReference("rnt_serviceuser", externalUserId.Value);
                    }
                    //this.Trace("contract item creation start for : " + item);

                    ContractHelper contractHelper = new ContractHelper(this.OrgService);
                    var contractItemId = contractHelper.createContractItemSafelyWithoutMongoDbIntegrationTrigger(contractItem);


                    var totalAmount = contractItem.GetAttributeValue<Money>("rnt_totalamount").Value;
                    var itemName = contractItem.GetAttributeValue<string>("rnt_name");
                    //InvoiceItemBL invoiceItemBL = new InvoiceItemBL(this.OrgService, this.TracingService);
                    //invoiceItemBL.createInvoiceItem(invoiceId, contractItemId, null, contractItem.GetAttributeValue<EntityReference>("transactioncurrencyid").Id, itemName, totalAmount);
                    //this.Trace("contract item creation end for : " + item);

                    ContractItemResponse contractItemCreateResponse = new ContractItemResponse
                    {
                        contractItemId = contractItemId,
                        itemTypeCode = contractItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value
                    };
                    contractItemCreateResponses.Add(contractItemCreateResponse);
                }
            }
            return contractItemCreateResponses;
        }

        public Guid createAdditionalProductForCancelContractFromContractItemWithInitializeFromRequest(Guid contractItemId, Guid invoiceId, decimal price)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            Entity newAdditionalProduct = xrmHelper.initializeFromRequest("rnt_contractitem", contractItemId, "rnt_contractitem");

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var productCode = configurationBL.GetConfigurationByName("additionalProduct_cancellationFeeCode");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(productCode, new string[] { "rnt_name" });

            newAdditionalProduct.Attributes["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", additionalProduct.Id);
            newAdditionalProduct.Attributes["rnt_name"] = additionalProduct.GetAttributeValue<string>("rnt_name");
            newAdditionalProduct.Attributes["rnt_itemtypecode"] = new OptionSetValue(3);
            newAdditionalProduct.Attributes["rnt_baseprice"] = new Money(price);
            newAdditionalProduct.Attributes["rnt_totalamount"] = new Money(price);
            newAdditionalProduct.Attributes["statuscode"] = new OptionSetValue((int)ContractItemEnums.StatusCode.Completed);
            var itemId = this.OrgService.Create(newAdditionalProduct);
            //InvoiceItemBL invoiceItemBL = new InvoiceItemBL(this.OrgService);
            //invoiceItemBL.createInvoiceItem(invoiceId, itemId, null, newAdditionalProduct.GetAttributeValue<EntityReference>("transactioncurrencyid").Id, additionalProduct.GetAttributeValue<string>("rnt_name"), price);

            return itemId;
        }

        public Guid createOtherTypeAdditionalProductDataforRental(AdditionalProductData additonalProductData,
                                                                  ContractItemRequiredData contractItemRequiredData,
                                                                  Guid invoiceId,
                                                                  Guid? externalUserId)
        {

            Entity contractItem = new Entity("rnt_contractitem");
            contractItem["rnt_contractid"] = new EntityReference("rnt_contract", contractItemRequiredData.contractId);
            contractItem["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", additonalProductData.productId);
            contractItem["rnt_baseprice"] = new Money((decimal)additonalProductData.actualAmount);
            contractItem["statuscode"] = new OptionSetValue(contractItemRequiredData.statuscode);
            contractItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", contractItemRequiredData.pickupBranchId);
            contractItem["rnt_pickupdatetime"] = contractItemRequiredData.pickupDateTime.Value;
            contractItem["rnt_dropoffdatetime"] = contractItemRequiredData.dropoffDateTime.Value;
            contractItem["rnt_offset"] = StaticHelper.offset;
            contractItem["rnt_dropoffbranch"] = new EntityReference("rnt_branch", contractItemRequiredData.dropoffBranchId);
            contractItem["rnt_itemtypecode"] = new OptionSetValue(contractItemRequiredData.itemTypeCode);
            contractItem["rnt_name"] = additonalProductData.productName;
            contractItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", contractItemRequiredData.transactionCurrencyId);
            contractItem["rnt_customerid"] = new EntityReference("contact", contractItemRequiredData.contactId);
            contractItem["rnt_groupcodeinformations"] = new EntityReference("rnt_groupcodeinformations", contractItemRequiredData.groupCodeInformationId);
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var ratio = systemParameterBL.getSystemTaxRatio();
            contractItem["rnt_taxratio"] = Convert.ToDecimal(ratio);
            contractItem["rnt_totalamount"] = new Money(additonalProductData.actualTotalAmount);
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var c = contractRepository.getContractById(contractItemRequiredData.contractId, new string[] { "rnt_paymentmethodcode" });
            contractItem["rnt_billingtype"] = CommonHelper.decideBillingType(contractItemRequiredData.itemTypeCode,
                                                                             c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value.ToString());
            contractItem["rnt_channelcode"] = new OptionSetValue((int)rnt_ReservationChannel.Tablet);

            contractItem["rnt_externalusercreatedbyid"] = new EntityReference("rnt_serviceuser", externalUserId.Value);
            var contractItemId = this.OrgService.Create(contractItem);

            //InvoiceItemBL invoiceItemBL = new InvoiceItemBL(this.OrgService, this.TracingService);
            //invoiceItemBL.createInvoiceItem(invoiceId, contractItemId, null, contractItemRequiredData.transactionCurrencyId, additonalProductData.productName, additonalProductData.actualTotalAmount);
            return contractItemId;
        }

        public List<Guid> createContractItemForAdditionalProducts(ContractDateandBranchParameters contractDateandBranchParameter,
                                                                 ContractAdditionalProductParameters contractItemAdditionalProductParameter,
                                                                 Guid contactId,
                                                                 Guid groupCodeInformationId,
                                                                 Guid contractId,
                                                                 Guid currencyId,
                                                                 Guid invoiceId,
                                                                 int totalDuration,
                                                                 int contractItemStatusCode,
                                                                 string trackingNumber,
                                                                 int channelCode,
                                                                 Guid? userId)
        {
            List<Guid> createdItems = new List<Guid>();
            if (contractItemAdditionalProductParameter.maxPieces == 1)
            {
                this.Trace("max pieces == 1");
                // create one additional item if there is one item
                createdItems.Add(this.createContractItemForAdditionalProduct(contractDateandBranchParameter,
                                                            contractItemAdditionalProductParameter,
                                                            contactId,
                                                            groupCodeInformationId,
                                                            contractId,
                                                            currencyId,
                                                            invoiceId,
                                                            totalDuration,
                                                            contractItemStatusCode,
                                                            trackingNumber,
                                                            channelCode,
                                                            userId, Guid.Empty));
            }
            else if (contractItemAdditionalProductParameter.maxPieces > 1)
            {
                this.Trace("max pieces > 1");

                // creating additional products for each item
                createdItems.AddRange(this.createContractItemForAdditionalProductEachItem(contractDateandBranchParameter,
                                                                                          contractItemAdditionalProductParameter,
                                                                                          contactId,
                                                                                          groupCodeInformationId,
                                                                                          contractId,
                                                                                          currencyId,
                                                                                          invoiceId,
                                                                                          totalDuration,
                                                                                          contractItemStatusCode,
                                                                                          trackingNumber,
                                                                                          channelCode,
                                                                                          userId));
            }
            return createdItems;
        }

        public Guid createContractItemForAdditionalProduct(ContractDateandBranchParameters contractDateandBranchParameter,
                                                           ContractAdditionalProductParameters contractItemAdditionalProductParameter,
                                                           Guid contactId,
                                                           Guid groupCodeInformationId,
                                                           Guid contractId,
                                                           Guid currencyId,
                                                           Guid invoiceId,
                                                           int totalDuration,
                                                           int contractItemStatusCode,
                                                           string trackingNumber,
                                                           int channelCode,
                                                           Guid? userId,
                                                           Guid? accountId,
                                                       int itemTypeCode = 2,
                                                       bool isCalculateAdditionalProduct = true)
        {
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var c = contractRepository.getContractById(contractId, new string[] { "rnt_paymentmethodcode" });

            Entity contractItem = new Entity("rnt_contractitem");
            contractItem["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
            contractItem["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", contractItemAdditionalProductParameter.productId);
            contractItem["rnt_contractduration"] = totalDuration;
            contractItem["rnt_baseprice"] = new Money((decimal)contractItemAdditionalProductParameter.actualAmount);
            contractItem["rnt_monthlypackageprice"] = new Money((decimal)contractItemAdditionalProductParameter.monthlyPackagePrice);
            contractItem["statuscode"] = new OptionSetValue(contractItemStatusCode);
            contractItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", contractDateandBranchParameter.pickupBranchId);
            contractItem["rnt_pickupdatetime"] = contractDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
            contractItem["rnt_dropoffdatetime"] = contractDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
            contractItem["rnt_offset"] = StaticHelper.offset;
            contractItem["rnt_dropoffbranch"] = new EntityReference("rnt_branch", contractDateandBranchParameter.dropoffBranchId);
            contractItem["rnt_groupcodeinformations"] = new EntityReference("rnt_groupcodeinformations", groupCodeInformationId);
            contractItem["rnt_mongodbtrackingnumber"] = trackingNumber;
            contractItem["rnt_itemtypecode"] = new OptionSetValue(itemTypeCode);
            contractItem["rnt_name"] = contractItemAdditionalProductParameter.productName;
            contractItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", currencyId);
            contractItem["rnt_customerid"] = new EntityReference("contact", contactId);
            if (accountId.HasValue && accountId.Value != Guid.Empty)
            {
                contractItem["rnt_corporateid"] = new EntityReference("account", accountId.Value);
            }
            contractItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
            contractItem["rnt_channelcode"] = new OptionSetValue(channelCode);
            contractItem["rnt_billingtype"] = CommonHelper.decideBillingType(itemTypeCode,
                                                                             c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value.ToString(), contractItemAdditionalProductParameter.billingType);
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var ratio = systemParameterBL.getSystemTaxRatio();
            contractItem["rnt_taxratio"] = Convert.ToDecimal(ratio);

            //decimal totalAmount = this.calculateTotalAmountAdditionalProducts(contractItemAdditionalProductParameter.priceCalculationType, (decimal)contractItemAdditionalProductParameter.actualAmount, totalDuration);
            if (isCalculateAdditionalProduct)
            {
                decimal totalAmount = CommonHelper.calculateAdditionalProductPrice(contractItemAdditionalProductParameter.priceCalculationType,
                                                                       contractItemAdditionalProductParameter.monthlyPackagePrice,
                                                                       contractItemAdditionalProductParameter.actualAmount.Value,
                                                                       totalDuration);
                contractItem["rnt_totalamount"] = new Money(totalAmount);

            }
            else
            {
                contractItem["rnt_totalamount"] = new Money((decimal)contractItemAdditionalProductParameter.actualAmount);
            }
            if (channelCode == (int)RntCar.ClassLibrary._Enums_1033.rnt_ReservationChannel.Tablet &&
                userId != null)
            {
                contractItem["rnt_externalusercreatedbyid"] = new EntityReference("rnt_serviceuser", userId.Value);
            }
            return this.OrgService.Create(contractItem);

            //InvoiceItemBL invoiceItemBL = new InvoiceItemBL(this.OrgService, this.TracingService);
            //invoiceItemBL.createInvoiceItem(invoiceId, contractItemId, null, currencyId, contractItemAdditionalProductParameter.productName, totalAmount);
        }


        public Guid CreateContractItem(Guid contractId, decimal amount, Guid additionalProductId, int? channelCode, int itemTypeCode,
                                                           Guid? userId, Guid? corporateId, bool isCalculateAdditionalProduct = true)
        {
            if (!channelCode.HasValue)
            {
                this.Trace("!channelCode.HasValue");

                channelCode = (int)rnt_ReservationChannel.Branch;
            }
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var contract = contractRepository.getContractById(contractId);
            var contactId = contract.GetAttributeValue<EntityReference>("rnt_customerid").Id;
            var currencyId = contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id;
            var contractStatusCode = contract.GetAttributeValue<OptionSetValue>("statuscode").Value;
            var groupCodeInformationId = contract.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id;

            this.Trace("before mapping contractStatusCode : " + contractStatusCode);
            this.Trace("after mapping contractStatusCode : " + ContractMapper.getContractItemStatusCodeByContractStatusCode(contractStatusCode));
            var contractItemStatusCode = ContractMapper.getContractItemStatusCodeByContractStatusCode(contractStatusCode);
            ContractDateandBranchParameters contractDateandBranchParameter = new ContractDateandBranchParameters
            {
                pickupBranchId = ((EntityReference)contract.Attributes["rnt_pickupbranchid"]).Id,
                pickupDate = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                dropoffDate = contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                dropoffBranchId = contract.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id
            };
            this.Trace("additional product retrieve start");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var additionalProduct = additionalProductRepository.getAdditionalProductById(additionalProductId, new string[] { "rnt_name" ,
                                                                                                                                             "rnt_pricecalculationtypecode" });
            this.Trace("additional product retrieve end");

            ContractAdditionalProductParameters contractItemAdditionalProductParameter = new ContractAdditionalProductParameters
            {
                productName = additionalProduct.GetAttributeValue<string>("rnt_name"),
                productId = additionalProductId,
                actualAmount = amount,
                priceCalculationType = additionalProduct.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value
            };
            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            var totalDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(contractDateandBranchParameter.pickupDate,
                                                                                                contractDateandBranchParameter.dropoffDate);

            //InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
            //var invoice = invoiceRepository.getFirstInvoiceByContractId(contractId);

            this.Trace("start createContractItemForManualPayment");
            var item = createContractItemForAdditionalProduct(contractDateandBranchParameter,
                                                                          contractItemAdditionalProductParameter,
                                                                          contactId,
                                                                          groupCodeInformationId,
                                                                          contractId,
                                                                          currencyId,
                                                                          Guid.Empty,
                                                                          totalDuration,
                                                                          contractItemStatusCode,
                                                                          string.Empty,
                                                                          channelCode.Value, userId, corporateId, itemTypeCode, isCalculateAdditionalProduct);
            this.Trace("end createContractItemForManualPayment");
            return item;

        }

        public Guid createContractItemForEqiupmentFromContractItemWithInitializeFromRequest(Guid equipmentId,
                                                                                            decimal price,
                                                                                            string trackingNumber,
                                                                                            DateTime utcNow,
                                                                                            DateTime dropoffDateTime,
                                                                                            Entity branch)
        {
            PriceCalculationSummariesBL priceCalculationSummariesBL = new PriceCalculationSummariesBL(this.OrgService);
            Entity contractEquipment = this.OrgService.Retrieve("rnt_contractitem", equipmentId,new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_groupcodeinformations", "rnt_contractduration", "rnt_totalamount"));
            EntityReference groupCodeInformationId = contractEquipment.GetAttributeValue<EntityReference>("rnt_groupcodeinformations");
            int totalDuration = contractEquipment.GetAttributeValue<int>("rnt_contractduration");
            Money totalAmount = contractEquipment.GetAttributeValue<Money>("rnt_totalamount");
           

            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            Entity newEquipment = xrmHelper.initializeFromRequest("rnt_contractitem", equipmentId, "rnt_contractitem");
            newEquipment.Attributes["statuscode"] = new OptionSetValue((int)ContractItemEnums.StatusCode.WaitingforDelivery);
            newEquipment.Attributes["rnt_baseprice"] = new Money(price);
            newEquipment.Attributes["rnt_totalamount"] = new Money(price);
            newEquipment.Attributes["rnt_pickupdatetime"] = utcNow.AddMinutes(StaticHelper.offset);
            newEquipment.Attributes["rnt_mongodbtrackingnumber"] = trackingNumber;
            newEquipment.Attributes["rnt_dropoffdatetime"] = dropoffDateTime.AddMinutes(StaticHelper.offset);

            if (branch != null)
            {
                newEquipment.Attributes["rnt_pickupbranchid"] = new EntityReference("rnt_branch", branch.Id);
            }

            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            var contractItemId = contractHelper.createContractItemSafelyWithoutMongoDbIntegrationTrigger(newEquipment);

            return contractItemId;
        }

        public void updateContractItemAdditionalProductAmountandDateBranchFields(Guid contractItemAdditionalProductId,
                                                                                 ContractDateandBranchParameters contractDateandBranchParameter,
                                                                                 ContractAdditionalProductParameters contractItemAdditionalProductParameter,
                                                                                 Guid contactId,
                                                                                 Guid groupCodeInformationId,
                                                                                 Guid contractId,
                                                                                 Guid currencyId,
                                                                                 int totalDuration,
                                                                                 int contractStatusCode,
                                                                                 bool isDateorBranchChanged,
                                                                                 string trackingNumber)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var resItem = contractItemRepository.getContractItemIdByGivenColumns(contractItemAdditionalProductId, new string[] { "rnt_baseprice",
                                                                                                                                 "rnt_monthlypackageprice",
                                                                                                                                 "rnt_additionalproductid" ,
                                                                                                                                 "rnt_dropoffbranch",
                                                                                                                                 "rnt_pickupbranchid"});
            var basePrice = resItem.GetAttributeValue<Money>("rnt_baseprice");
            this.Trace("contractitemBL : updateContractItemAdditionalProductAmountandDateBranchFields : " + isDateorBranchChanged);

            this.Trace("updateContractItemAdditionalProductAmountandDateBranchFields name: " + contractItemAdditionalProductParameter.productName);

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var maturityCode = configurationBL.GetConfigurationByName("additionalProduct_MaturityDifference");
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var add = additionalProductRepository.getAdditionalProductByProductCode(maturityCode);
            //this.Trace("maturityCode " + maturityCode);
            //this.Trace("add.Id " + add.Id);
            //this.Trace("resItem.addId " + resItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id);

            contractItemAdditionalProductParameter.actualAmount = basePrice.Value;

            if (isDateorBranchChanged || (resItem.Contains("rnt_additionalproductid") && add.Id == resItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id))
            {
                Entity contractItem = new Entity("rnt_contractitem");
                contractItem["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
                contractItem["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", contractItemAdditionalProductParameter.productId);
                contractItem["rnt_contractduration"] = totalDuration;
                contractItem["statuscode"] = new OptionSetValue(contractStatusCode);
                contractItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", contractDateandBranchParameter.pickupBranchId);
                contractItem["rnt_pickupdatetime"] = contractDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
                contractItem["rnt_dropoffdatetime"] = contractDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
                contractItem["rnt_dropoffbranch"] = new EntityReference("rnt_branch", contractDateandBranchParameter.dropoffBranchId);
                contractItem["rnt_groupcodeinformations"] = new EntityReference("rnt_groupcodeinformations", groupCodeInformationId);

                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                var c = contractRepository.getContractById(contractId, new string[] { "rnt_paymentmethodcode" });

                contractItem["rnt_billingtype"] = CommonHelper.decideBillingType((int)GlobalEnums.ItemTypeCode.AdditionalProduct, c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value.ToString(), contractItemAdditionalProductParameter.billingType);

                //contractItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
                SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
                var ratio = systemParameterBL.getSystemTaxRatio();
                contractItem["rnt_taxratio"] = Convert.ToDecimal(ratio);
                if (resItem.Contains("rnt_additionalproductid") && add.Id == resItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id)
                {
                    this.Trace("this is maturity");
                    contractItem["rnt_totalamount"] = new Money(contractItemAdditionalProductParameter.actualTotalAmount.Value);
                    contractItem["rnt_baseprice"] = new Money(contractItemAdditionalProductParameter.actualTotalAmount.Value);
                }
                else
                {
                    decimal totalAmount = CommonHelper.calculateAdditionalProductPrice(contractItemAdditionalProductParameter.priceCalculationType,
                                                                                    resItem.GetAttributeValue<Money>("rnt_monthlypackageprice").Value,
                                                                                    contractItemAdditionalProductParameter.actualAmount.Value,
                                                                                    totalDuration);
                    contractItem["rnt_baseprice"] = new Money((decimal)contractItemAdditionalProductParameter.actualAmount);
                    contractItem["rnt_totalamount"] = new Money(totalAmount);
                }


                contractItem.Id = contractItemAdditionalProductId;

                //this.Trace("contractItem Muco icin: " + JsonConvert.SerializeObject(contractItem));

                this.OrgService.Update(contractItem);
            }
            else
            {
                Entity contractItem = new Entity("rnt_contractitem");
                contractItem.Id = contractItemAdditionalProductId;
                contractItem["rnt_billingtype"] = new OptionSetValue((int)contractItemAdditionalProductParameter.billingType.Value);
                this.OrgService.Update(contractItem);
            }
        }

        public MongoDBResponse createContractItemInMongoDB(Entity contractItem)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateContractItemInMongoDB", RestSharp.Method.POST);

            this.Trace("mongodb contract build params start");
            var contractItemData = this.buildMongoDBContractItemData(contractItem);
            this.Trace("mongodb contract build params end");

            restSharpHelper.AddJsonParameter<ContractItemData>(contractItemData);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }

        public MongoDBResponse updateContractItemInMongoDB(Entity contractItem)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            //todo implement a good logic
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateContractItemInMongoDB", RestSharp.Method.POST);

            var contractItemData = this.buildMongoDBContractItemData(contractItem);

            restSharpHelper.AddJsonParameter<ContractItemData>(contractItemData);
            restSharpHelper.AddQueryParameter("id", contractItem.GetAttributeValue<string>("rnt_mongodbid"));
            this.Trace("id : " + contractItem.GetAttributeValue<string>("rnt_mongodbid"));
            var response = restSharpHelper.Execute<MongoDBResponse>();
            this.Trace("mongodb response : " + JsonConvert.SerializeObject(response));
            return response;
        }



        public void updateContractItemsDeliveryRelatedFields(UpdateContractforDeliveryParameters updateContractforDeliveryParametersSerialized,
                                                             List<Entity> contractItems,
                                                             List<Guid> createdAdditionalProducts,
                                                             DateTime utcNow,
                                                             bool equipmentChangedBefore,
                                                             bool changeStatus = true,
                                                             double minutes = double.MaxValue)
        {
            if (minutes == double.MaxValue)
            {
                minutes = CommonHelper.calculateTotalDurationInMinutes(updateContractforDeliveryParametersSerialized.contractInformation.PickupDateTimeStamp,
                                                                     updateContractforDeliveryParametersSerialized.contractInformation.DropoffTimeStamp);
            }

            foreach (var item in contractItems)
            {
                Entity e = new Entity("rnt_contractitem");
                e.Id = item.Id;
                var check = createdAdditionalProducts.Where(p => p == item.Id).FirstOrDefault() == Guid.Empty ? false : true;
                if (check || !equipmentChangedBefore)
                {
                    if (updateContractforDeliveryParametersSerialized.contractInformation.isManuelProcess)
                    {
                        e["rnt_pickupdatetime"] = updateContractforDeliveryParametersSerialized.contractInformation.manuelPickupDateTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset);
                        e["rnt_dropoffdatetime"] = updateContractforDeliveryParametersSerialized.contractInformation.manuelPickupDateTimeStamp.converttoDateTime().AddMinutes(minutes).AddMinutes(StaticHelper.offset);
                    }
                    else
                    {
                        e["rnt_pickupdatetime"] = utcNow.AddMinutes(StaticHelper.offset);
                        e["rnt_dropoffdatetime"] = utcNow.AddMinutes(minutes).AddMinutes(StaticHelper.offset);
                    }

                }

                if (item.Attributes.Contains("rnt_itemtypecode") &&
                    item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)GlobalEnums.ItemTypeCode.Equipment)
                {
                    e["rnt_equipment"] = new EntityReference("rnt_equipment", updateContractforDeliveryParametersSerialized.equipmentInformation.equipmentId);
                }
                if (changeStatus)
                    e["statuscode"] = new OptionSetValue((int)ContractItemEnums.StatusCode.Rental);
                this.Trace("updateContractforDeliveryParametersSerialized.changedEquipmentData?.groupCodeId  " + updateContractforDeliveryParametersSerialized.changedEquipmentData?.groupCodeId);
                if (updateContractforDeliveryParametersSerialized.changedEquipmentData != null &&
                    updateContractforDeliveryParametersSerialized.changedEquipmentData.groupCodeId != Guid.Empty)
                {
                    e["rnt_groupcodeinformations"] = new EntityReference("rnt_groupcodeinformations", (Guid)updateContractforDeliveryParametersSerialized.changedEquipmentData?.groupCodeId);
                }
                else
                {
                    e["rnt_groupcodeinformations"] = new EntityReference("rnt_groupcodeinformations", updateContractforDeliveryParametersSerialized.equipmentInformation.groupCodeInformationId);
                }
                ContractHelper contractHelper = new ContractHelper(this.OrgService);
                contractHelper.updateContractItemSafelyWithoutMongoDbIntegrationTrigger(e);
            }
        }

        public Guid updateContractItemsRentalRelatedFields(Guid contractId,
                                                           Guid? dropoffBranchId,
                                                           long dropoffTimeStamp,
                                                           bool ismanualProcess,
                                                           string trackingNumber,
                                                           decimal totalAmount)
        {
            Guid contractEquipmentId = Guid.Empty;

            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var items = contractItemRepository.getRentalContractItemsByContractIdByGivenColumns(contractId, new string[] { "rnt_itemtypecode" });

            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var c = contractRepository.getContractById(contractId, new string[] { "rnt_paymentmethodcode" });
            this.Trace("updateContractItemsRentalRelatedFields item found : " + items.Count);
            foreach (var item in items)
            {
                Entity e = new Entity("rnt_contractitem");
                e.Id = item.Id;

                if (dropoffBranchId.HasValue)
                    e["rnt_dropoffbranch"] = new EntityReference("rnt_branch", dropoffBranchId.Value);

                e["rnt_mongodbtrackingnumber"] = trackingNumber;
                e["statuscode"] = new OptionSetValue((int)rnt_contractitem_StatusCode.Completed);
                if (item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Equipment)
                {
                    e["rnt_dropoffdatetime"] = dropoffTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset);
                    e["rnt_totalamount"] = new Money(totalAmount);
                    SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
                    var ratio = systemParameterBL.getSystemTaxRatio();
                    e["rnt_taxratio"] = Convert.ToDecimal(ratio);
                    contractEquipmentId = item.Id;
                }
                else
                {
                    e["rnt_dropoffdatetime"] = dropoffTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset);
                }
                ContractHelper contractHelper = new ContractHelper(this.OrgService);
                contractHelper.updateContractItemSafelyWithoutMongoDbIntegrationTrigger(e);
            }
            return contractEquipmentId;
        }
        public void updateRentalContractItemsAdditionalProductstoWaitingForDelivery(Guid? dropoffBranchId,
                                                                                    string trackingNumber,
                                                                                    decimal totalAmount,
                                                                                    List<Entity> items)
        {
            foreach (var item in items)
            {
                Entity e = new Entity("rnt_contractitem");
                e.Id = item.Id;
                if (dropoffBranchId.HasValue)
                    e["rnt_dropoffbranch"] = new EntityReference("rnt_branch", dropoffBranchId.Value);

                e["rnt_mongodbtrackingnumber"] = trackingNumber;
                e["statuscode"] = new OptionSetValue((int)ContractItemEnums.StatusCode.WaitingforDelivery);

                ContractHelper contractHelper = new ContractHelper(this.OrgService);
                contractHelper.updateContractItemSafelyWithoutMongoDbIntegrationTrigger(e);
            }
        }
        public Guid updateContractItemRentalRelatedFields(Guid contractId,
                                                          Guid? dropoffBranchId,
                                                          long dropoffTimeStamp,
                                                          bool ismanualProcess,
                                                          string trackingNumber,
                                                          decimal totalAmount)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var item = contractItemRepository.getRentalEquipmentContractItemByGivenColumns(contractId, new string[] { });

            Entity e = new Entity("rnt_contractitem");
            e.Id = item.Id;
            if (dropoffBranchId.HasValue)
                e["rnt_dropoffbranch"] = new EntityReference("rnt_branch", dropoffBranchId.Value);

            e["rnt_mongodbtrackingnumber"] = trackingNumber;
            e["statuscode"] = new OptionSetValue((int)ContractItemEnums.StatusCode.Completed);
            e["rnt_dropoffdatetime"] = dropoffTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset);
            e["rnt_totalamount"] = new Money(totalAmount);
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var ratio = systemParameterBL.getSystemTaxRatio();
            e["rnt_taxratio"] = Convert.ToDecimal(ratio);

            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            contractHelper.updateContractItemSafelyWithoutMongoDbIntegrationTrigger(e);

            return item.Id;
        }


        public void updateContractItemStatus(Guid contractItemId, int changeReason)
        {

            Entity entity = new Entity("rnt_contractitem");
            entity.Id = contractItemId;
            entity.Attributes["statuscode"] = new OptionSetValue(changeReason);
            this.OrgService.Update(entity);

        }

        public void deactiveContractItemItemById(Guid contractItemId, int statusCode)
        {
            XrmHelper h = new XrmHelper(this.OrgService);
            h.setState("rnt_contractitem", contractItemId, (int)GlobalEnums.StateCode.Passive, statusCode);
        }
        public void updateContractItemStatusandDateTime(Guid contractItemId,
                                                       ContractDateandBranchParameters parameters,
                                                       int statusCode)
        {
            Entity entity = new Entity("rnt_contractitem");
            entity.Id = contractItemId;
            entity.Attributes["statuscode"] = new OptionSetValue(statusCode);
            entity.Attributes["rnt_pickupdatetime"] = parameters.pickupDate;
            entity.Attributes["rnt_dropoffdatetime"] = parameters.dropoffDate;
            this.OrgService.Update(entity);

        }
        public void updateContractItemStatusandChangeReasonandDateTime(Guid contractItemId, ContractDateandBranchParameters parameters, int status, int changeReason)
        {

            Entity entity = new Entity("rnt_contractitem");
            entity.Id = contractItemId;
            entity.Attributes["statuscode"] = new OptionSetValue(status);
            entity.Attributes["rnt_changereason"] = new OptionSetValue(changeReason);
            entity.Attributes["rnt_pickupdatetime"] = parameters.pickupDate;
            entity.Attributes["rnt_dropoffdatetime"] = parameters.dropoffDate;
            this.OrgService.Update(entity);

        }
        public void updateContractItemChangeReasonandDateTime(Guid contractItemId,
                                                              ContractDateandBranchParameters parameters,
                                                              int changeReason)
        {

            Entity entity = new Entity("rnt_contractitem");
            entity.Id = contractItemId;
            entity.Attributes["rnt_changereason"] = new OptionSetValue(changeReason);
            entity.Attributes["rnt_pickupdatetime"] = parameters.pickupDate;
            entity.Attributes["rnt_dropoffdatetime"] = parameters.dropoffDate;
            this.OrgService.Update(entity);
        }

        public decimal getContractEquipmentBasePrice(string contractId)
        {
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var entity = contractItemRepository.getContractEquipmentByGivenColumns(Guid.Parse(contractId), new string[] { "rnt_baseprice" });
            if (entity != null)
                return entity.Attributes.Contains("rnt_baseprice") ? entity.GetAttributeValue<Money>("rnt_baseprice").Value : decimal.Zero;

            return decimal.Zero;
        }
        public void updateContractItemforEquipmentChangeOnContractUpdate(Guid contractItemId,
                                                                         decimal price,
                                                                         ContractDateandBranchParameters parameters,
                                                                         string trackingNumber,
                                                                         DateTime utcNow,
                                                                         int statusCode)
        {
            Entity contractItem = new Entity("rnt_contractitem");
            //contractItem["rnt_pickupdatetime"] = parameters.pickupDate.AddMinutes(StaticHelper.offset);
            contractItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", parameters.pickupBranchId);
            contractItem["rnt_dropoffdatetime"] = utcNow.AddMinutes(StaticHelper.offset);
            contractItem.Attributes["statuscode"] = new OptionSetValue(statusCode);
            contractItem["rnt_dropoffbranch"] = new EntityReference("rnt_branch", parameters.dropoffBranchId);
            //contractItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
            contractItem["rnt_mongodbtrackingnumber"] = trackingNumber;
            contractItem["rnt_totalamount"] = new Money(price);
            contractItem["rnt_baseprice"] = new Money(price);
            contractItem.Id = contractItemId;

            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            contractHelper.updateContractItemSafelyWithoutMongoDbIntegrationTrigger(contractItem);
        }
        public void updateContractEquipmentDateandBranch(Guid contractId,
                                                         Guid contractItemId,
                                                         decimal price,
                                                         ContractDateandBranchParameters parameters,
                                                         string trackingNumber,
                                                         DateTime utcNow,
                                                         int statusCode,
                                                         bool isCarChanged = false)
        {
            Entity contractItem = new Entity("rnt_contractitem");
            if (statusCode == (int)rnt_contract_StatusCode.WaitingForDelivery)
            {
                contractItem["rnt_pickupdatetime"] = parameters.pickupDate.AddMinutes(StaticHelper.offset);
            }

            contractItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", parameters.pickupBranchId);
            if (isCarChanged)
            {
                contractItem["rnt_dropoffdatetime"] = utcNow.AddMinutes(StaticHelper.offset);
            }
            else
            {
                contractItem["rnt_dropoffdatetime"] = parameters.dropoffDate.AddMinutes(StaticHelper.offset);
            }
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var completedAmount = contractItemRepository.getSumCompletedEquipmentContractItems(contractId);
            contractItem["rnt_dropoffbranch"] = new EntityReference("rnt_branch", parameters.dropoffBranchId);
            contractItem["rnt_mongodbtrackingnumber"] = trackingNumber;
            contractItem["rnt_totalamount"] = new Money(price - completedAmount);
            contractItem["rnt_baseprice"] = new Money(price - completedAmount);
            contractItem.Id = contractItemId;

            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            contractHelper.updateContractItemSafelyWithoutMongoDbIntegrationTrigger(contractItem);
        }

        public int calculateKmforContractDelivery(int totalDays,
                                                  Guid groupCodeInformationId,
                                                  List<AdditonalProductDataTablet> additionalProductDataTablets)
        {
            KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(this.OrgService, this.TracingService);
            var kilometer = kilometerLimitBL.getKilometerLimitForGivenDurationandGroupCode(totalDays, groupCodeInformationId);

            this.Trace("begin additional");
            var additionalIds = additionalProductDataTablets.Select(x => x.productId).ToList();
            this.Trace("additionalIds: " + JsonConvert.SerializeObject(additionalIds));

            AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(this.OrgService);
            var kilometerLimitEffect = additionalProductHelper.getAdditionalKilometerByAdditionalIds(additionalIds);
            this.Trace("kilometerLimitEffect: " + kilometerLimitEffect);
            if (kilometerLimitEffect.HasValue)
            {
                kilometer += kilometerLimitEffect.Value;
            }

            return kilometer;
        }
        private decimal calculateTotalAmountAdditionalProducts(int priceCalculationType, decimal actualAmount, int totalDuration)
        {
            var totalAmount = decimal.Zero;
            if (priceCalculationType == 1)
                totalAmount = (decimal)actualAmount * totalDuration;

            else if (priceCalculationType == 2)
            {
                totalAmount = (decimal)actualAmount;
            }

            return totalAmount;
        }
        public ContractItemData buildMongoDBContractItemData(Entity contractItem)
        {
            this.Trace("contract id start");
            var contractRef = contractItem.GetAttributeValue<EntityReference>("rnt_contractid");
            var contract = this.OrgService.Retrieve(contractRef.LogicalName, contractRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_contractnumber",
                                                                                                                                   "rnt_pnrnumber",
                                                                                                                                   "rnt_contracttypecode",
                                                                                                                                   "rnt_contractchannelcode",
                                                                                                                                   "rnt_paymentchoicecode",
                                                                                                                                   "rnt_depositamount",
                                                                                                                                   "rnt_overkilometerprice",
                                                                                                                                   "rnt_kilometerlimit",
                                                                                                                                   "rnt_corporateid",
                                                                                                                                   "rnt_pickupdatetime",
                                                                                                                                   "rnt_processindividualprices",
                                                                                                                                   "rnt_pricinggroupcodeid",
                                                                                                                                   "rnt_paymentmethodcode",
                                                                                                                                   "rnt_reservationid",
                                                                                                                                   "rnt_ismonthly",
                                                                                                                                 "rnt_howmanymonths",
                                                                                                                                   "rnt_isclosedamountzero"));
            this.Trace("contract id ok" + contractRef.Id);

            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var res = reservationRepository.getReservationById(contract.GetAttributeValue<EntityReference>("rnt_reservationid").Id, new string[] { "transactioncurrencyid" });

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var turkishCurrency = configurationBL.GetConfigurationByName("currency_TRY");

            var changeCurrency = false;
            var exchangeRate = decimal.Zero;
            if (new Guid(turkishCurrency) != res.GetAttributeValue<EntityReference>("transactioncurrencyid").Id)
            {
                CurrencyRepository currencyRepository = new CurrencyRepository(this.OrgService);
                var c = currencyRepository.getCurrencyById(res.GetAttributeValue<EntityReference>("transactioncurrencyid").Id, new string[] { "exchangerate" });
                changeCurrency = true;
                exchangeRate = c.GetAttributeValue<decimal>("exchangerate");
            }
            PaymentPlanRepository paymentPlanRepository = new PaymentPlanRepository(this.OrgService);
            var plans = paymentPlanRepository.getPaymentPlansByContractId(contract.Id);

            var _plans = new PaymentPlanMapper().buildPaymentPlans(plans,
                                                                   contract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id,
                                                                   contract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Name);
            ContractItemData contractItemData = new ContractItemData
            {
                isMonthly = contract.GetAttributeValue<bool>("rnt_ismonthly"),
                monthValue = contract.Attributes.Contains("rnt_howmanymonths") ?
                             contract.GetAttributeValue<OptionSetValue>("rnt_howmanymonths").Value :
                              0,
                paymentPlans = _plans,
                exchangeRate = exchangeRate,
                changeCurrency = changeCurrency,
                processIndividualPrices = contract.Contains("rnt_processindividualprices") ? contract.GetAttributeValue<bool>("rnt_processindividualprices") : false,
                billingType = contractItem.GetAttributeValue<OptionSetValue>("rnt_billingtype").Value,
                paymentMethod = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value,
                pickupDateTime_Header = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                pickupDateTimeStamp_Header = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime").converttoTimeStamp(),
                contractId = contractRef.Id.ToString(),
                contractItemId = contractItem.Id.ToString(),
                itemNo = contractItem.Attributes.Contains("rnt_itemno") ? contractItem.GetAttributeValue<string>("rnt_itemno") : string.Empty,
                statuscode = contractItem.GetAttributeValue<OptionSetValue>("statuscode").Value,
                statecode = contractItem.GetAttributeValue<OptionSetValue>("statecode").Value,
                equipmentName = contractItem.Attributes.Contains("rnt_equipment") ? contractItem.GetAttributeValue<EntityReference>("rnt_equipment").Name : string.Empty,
                equipmentId = contractItem.Attributes.Contains("rnt_equipment") ? contractItem.GetAttributeValue<EntityReference>("rnt_equipment").Id.ToString() : string.Empty,
                additionalProductName = contractItem.Attributes.Contains("rnt_additionalproductid") ? contractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Name : string.Empty,
                additionalProductId = contractItem.Attributes.Contains("rnt_additionalproductid") ? contractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id.ToString() : string.Empty,
                changeReason = contractItem.Attributes.Contains("rnt_changereason") ? contractItem.GetAttributeValue<OptionSetValue>("rnt_changereason").Value : 0,
                groupCodeInformationsName = contractItem.Attributes.Contains("rnt_groupcodeinformations") ? contractItem.GetAttributeValue<EntityReference>("rnt_groupcodeinformations").Name : string.Empty,
                groupCodeInformationsId = contractItem.Attributes.Contains("rnt_groupcodeinformations") ? contractItem.GetAttributeValue<EntityReference>("rnt_groupcodeinformations").Id.ToString() : string.Empty,
                pricingGroupCodeName = contract.Attributes.Contains("rnt_pricinggroupcodeid") ?
                                       contract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Name :
                                       string.Empty,
                pricingGroupCodeId = contract.Attributes.Contains("rnt_pricinggroupcodeid") ?
                                     contract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id.ToString() :
                                     string.Empty,
                transactioncurrencyid = contractItem.Attributes.Contains("transactioncurrencyid") ? contractItem.GetAttributeValue<EntityReference>("transactioncurrencyid").Id.ToString() : string.Empty,
                netAmount = contractItem.Attributes.Contains("rnt_netamount") ? contractItem.GetAttributeValue<Money>("rnt_netamount").Value : decimal.Zero,
                taxRatio = contractItem.Attributes.Contains("rnt_taxratio") ? contractItem.GetAttributeValue<decimal>("rnt_taxratio") : 0,
                taxAmount = contractItem.Attributes.Contains("rnt_taxamount") ? contractItem.GetAttributeValue<Money>("rnt_taxamount").Value : 0,
                totalAmount = contractItem.Attributes.Contains("rnt_totalamount") ? contractItem.GetAttributeValue<Money>("rnt_totalamount").Value : 0,
                pickupDateTime = contractItem.Attributes.Contains("rnt_pickupdatetime") ?
                                 (DateTime?)contractItem.GetAttributeValue<DateTime?>("rnt_pickupdatetime").Value.AddMinutes(-StaticHelper.offset) :
                                 null,
                dropoffDateTime = contractItem.Attributes.Contains("rnt_dropoffdatetime") ?
                                  (DateTime?)contractItem.GetAttributeValue<DateTime?>("rnt_dropoffdatetime").Value.AddMinutes(-StaticHelper.offset) :
                                  null,
                pickupBranchName = contractItem.Attributes.Contains("rnt_pickupbranchid") ? contractItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name : string.Empty,
                pickupBranchId = contractItem.Attributes.Contains("rnt_pickupbranchid") ? contractItem.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id.ToString() : string.Empty,
                dropoffBranchName = contractItem.Attributes.Contains("rnt_dropoffbranch") ? contractItem.GetAttributeValue<EntityReference>("rnt_dropoffbranch").Name : string.Empty,
                offset = StaticHelper.offset,
                dropoffBranchId = contractItem.Attributes.Contains("rnt_dropoffbranch") ? contractItem.GetAttributeValue<EntityReference>("rnt_dropoffbranch").Id.ToString() : string.Empty,
                ownerId = contractItem.Attributes.Contains("ownerid") ?
                                        Convert.ToString(contractItem.GetAttributeValue<EntityReference>("ownerid").Id) :
                                         null,
                ownerName = contractItem.Attributes.Contains("ownerid") ?
                                         contractItem.GetAttributeValue<EntityReference>("ownerid").Name :
                                         null,
                customerId = contractItem.Attributes.Contains("rnt_customerid") ?
                                        Convert.ToString(contractItem.GetAttributeValue<EntityReference>("rnt_customerid").Id) :
                                         null,
                customerName = contractItem.Attributes.Contains("rnt_customerid") ?
                                         contractItem.GetAttributeValue<EntityReference>("rnt_customerid").Name :
                                         null,
                trackingNumber = contractItem.Attributes.Contains("rnt_mongodbtrackingnumber") ? contractItem.GetAttributeValue<string>("rnt_mongodbtrackingnumber") : string.Empty,
                itemTypeCode = contractItem.Attributes.Contains("rnt_itemtypecode") ? contractItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value : 0,
                campaignId = contractItem.Attributes.Contains("rnt_campaignid") ?
                                        (Guid?)contractItem.GetAttributeValue<EntityReference>("rnt_campaignid").Id :
                                         null,
                campaignName = contractItem.Attributes.Contains("rnt_campaignid") ?
                                        contractItem.GetAttributeValue<EntityReference>("rnt_campaignid").Name :
                                         null,
                contractNumber = contract.Attributes.Contains("rnt_contractnumber") ? contract.GetAttributeValue<string>("rnt_contractnumber") : string.Empty,
                pnrNumber = contract.Attributes.Contains("rnt_pnrnumber") ? contract.GetAttributeValue<string>("rnt_pnrnumber") : string.Empty,
                contractType = contract.Attributes.Contains("rnt_contracttypecode") ? contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value : 0,
                paymentChoice = contract.Attributes.Contains("rnt_paymentchoicecode") ? contract.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value : 0,
                contractChannel = contract.Attributes.Contains("rnt_contractchannelcode") ? contract.GetAttributeValue<OptionSetValue>("rnt_contractchannelcode").Value : 0,
                depositAmount = contract.Attributes.Contains("rnt_depositamount") ? contract.GetAttributeValue<Money>("rnt_depositamount").Value : decimal.Zero,
                overKilometerPrice = contract.GetAttributeValue<Money>("rnt_overkilometerprice").Value,
                kilometerLimit = contract.GetAttributeValue<int>("rnt_kilometerlimit"),
                corporateCustomerId = contract.Attributes.Contains("rnt_corporateid") ?
                                      Convert.ToString(contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id) :
                                      string.Empty,
                isClosedAmountZero = contract.GetAttributeValue<bool>("rnt_isclosedamountzero")
            };

            this.Trace(JsonConvert.SerializeObject(contractItemData));
            return contractItemData;
        }
        private List<Guid> createContractItemForAdditionalProductEachItem(ContractDateandBranchParameters contractDateandBranchParameter,
                                             ContractAdditionalProductParameters contractItemAdditionalProductParameter,
                                             Guid contactId,
                                             Guid groupCodeInformationId,
                                             Guid contractId,
                                             Guid currencyId,
                                             Guid invoiceId,
                                             int totalDuration,
                                             int contractItemStatusCode,
                                             string trackingNumber,
                                             int channelCode,
                                             Guid? userId)
        {
            List<Guid> createdItems = new List<Guid>();
            for (int i = 0; i < contractItemAdditionalProductParameter.value; i++)
            {
               
                Entity contractItem = new Entity("rnt_contractitem");
                contractItem["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
                contractItem["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", contractItemAdditionalProductParameter.productId);
                contractItem["rnt_contractduration"] = totalDuration;
                contractItem["rnt_baseprice"] = new Money((decimal)contractItemAdditionalProductParameter.actualAmount);
                contractItem["rnt_monthlypackageprice"] = new Money((decimal)contractItemAdditionalProductParameter.monthlyPackagePrice);
                contractItem["statuscode"] = new OptionSetValue(contractItemStatusCode);
                contractItem["rnt_pickupbranchid"] = new EntityReference("rnt_branch", contractDateandBranchParameter.pickupBranchId);
                contractItem["rnt_pickupdatetime"] = contractDateandBranchParameter.pickupDate.AddMinutes(StaticHelper.offset);
                contractItem["rnt_dropoffdatetime"] = contractDateandBranchParameter.dropoffDate.AddMinutes(StaticHelper.offset);
                contractItem["rnt_dropoffbranch"] = new EntityReference("rnt_branch", contractDateandBranchParameter.dropoffBranchId);
                contractItem["rnt_groupcodeinformations"] = new EntityReference("rnt_groupcodeinformations", groupCodeInformationId);
                contractItem["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
                SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
                var ratio = systemParameterBL.getSystemTaxRatio();
                contractItem["rnt_taxratio"] = Convert.ToDecimal(ratio);
                //decimal totalAmount = this.calculateTotalAmountAdditionalProducts(contractItemAdditionalProductParameter.priceCalculationType, (decimal)contractItemAdditionalProductParameter.actualAmount, totalDuration);
                decimal totalAmount = CommonHelper.calculateAdditionalProductPrice(contractItemAdditionalProductParameter.priceCalculationType,
                                                       contractItemAdditionalProductParameter.monthlyPackagePrice,
                                                       contractItemAdditionalProductParameter.actualAmount.Value,
                                                       totalDuration);
                contractItem["rnt_totalamount"] = new Money(totalAmount);
                contractItem["rnt_itemtypecode"] = new OptionSetValue((int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct);
                contractItem["rnt_name"] = contractItemAdditionalProductParameter.productName;
                contractItem["transactioncurrencyid"] = new EntityReference("transactioncurrency", currencyId);
                contractItem["rnt_customerid"] = new EntityReference("contact", contactId);
                if (channelCode == (int)RntCar.ClassLibrary._Enums_1033.rnt_ReservationChannel.Tablet &&
                    userId != null)
                {
                    contractItem["rnt_externalusercreatedbyid"] = new EntityReference("rnt_serviceuser", userId.Value);
                }
                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                var c = contractRepository.getContractById(contractId, new string[] { "rnt_paymentmethodcode" });
                contractItem["rnt_billingtype"] = CommonHelper.decideBillingType((int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct,
                                                                                 c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value.ToString(), contractItemAdditionalProductParameter.billingType);
                createdItems.Add(this.OrgService.Create(contractItem));
            }
            return createdItems;
        }

        public CalculateAmountResult calculateContractItemAmounts(List<Entity> contractItems)
        {
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);

            decimal totalAmount = 0;
            decimal revenueAmount = 0;

            contractItems.ForEach(item =>
            {
                var amount = item.GetAttributeValue<Money>("rnt_totalamount").Value;
                var type = item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;

                if (type == (int)GlobalEnums.ItemTypeCode.Equipment)
                {
                    revenueAmount += amount;
                }
                else if (type == (int)GlobalEnums.ItemTypeCode.AdditionalProduct || type == (int)GlobalEnums.ItemTypeCode.Fine)
                {
                    var additionalProduct = additionalProductRepository.getAdditionalProductById(item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id);
                    var revenue = additionalProduct.GetAttributeValue<bool>("rnt_revenue");

                    if (revenue)
                    {
                        revenueAmount += amount;
                    }
                }

                totalAmount += amount;

            });

            return new CalculateAmountResult
            {
                RevenueAmount = revenueAmount,
                TotalAmount = totalAmount
            };
        }

        public void updateContractItemInvoiceDate(Guid contractItemId, DateTime invoiceDate)
        {
            Entity entity = new Entity("rnt_contractitem");
            entity.Id = contractItemId;
            entity.Attributes["rnt_invoicedate"] = invoiceDate.AddMinutes(StaticHelper.offset);
            this.OrgService.Update(entity);
        }

        public void updateContractItemTotalDuration(Guid contractItemId, int itemTypeCode, DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            var contractHelper = new RntCar.BusinessLibrary.Helpers.ContractHelper(this.OrgService);
            decimal totalDuration = 0;

            totalDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(pickupDateTime, dropoffDateTime);

            this.Trace("pickupDateTime : " + pickupDateTime);
            this.Trace("dropoffDateTime : " + dropoffDateTime);
            if (itemTypeCode == (int)ContractItemEnums.ItemTypeCode.Equipment)
            {
                totalDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect_Decimal(pickupDateTime, dropoffDateTime);
            }
            this.Trace("totalDuration : " + totalDuration);

            Entity e = new Entity("rnt_contractitem");
            e.Id = contractItemId;
            e["rnt_contractduration"] = totalDuration;

            this.OrgService.Update(e);

        }

        public List<Entity> getContractEquipmentPriceWithPickupDate(DateTime startDate, DateTime endDate)
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression query = new QueryExpression("rnt_contractitem");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };

                FilterExpression statusFilter = new FilterExpression(LogicalOperator.Or);
                statusFilter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.WaitingForDelivery);
                statusFilter.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Rental);

                query.ColumnSet = new ColumnSet("rnt_totalamount", "rnt_pickupbranchid");
                query.Criteria.AddCondition("rnt_pickupdatetime", ConditionOperator.OnOrAfter, startDate);
                query.Criteria.AddCondition("rnt_pickupdatetime", ConditionOperator.OnOrBefore, endDate);
                query.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Equipment);
                query.Criteria.AddFilter(statusFilter);
                var reservationItems = this.OrgService.RetrieveMultiple(query);
                result.AddRange(reservationItems.Entities.ToList());
                if (reservationItems.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = reservationItems.PagingCookie;
                }
                else
                {
                    break;
                }
            }

            return result;
        }


        public EntityCollection getContractItemsByAdditionalProduct(Guid additionalProductId, Guid contractId)
        {
            EntityCollection result = new EntityCollection();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression query = new QueryExpression("rnt_contractitem");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };

                query.ColumnSet = new ColumnSet("rnt_totalamount", "rnt_pickupbranchid");
                query.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProductId);
                query.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                query.AddOrder("rnt_totalamount", OrderType.Descending);
                var reservationItems = this.OrgService.RetrieveMultiple(query);
                result.Entities.AddRange(reservationItems.Entities);
                if (reservationItems.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = reservationItems.PagingCookie;
                }
                else
                {
                    break;
                }
            }

            return result;
        }
    }
}
