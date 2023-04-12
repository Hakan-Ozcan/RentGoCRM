using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
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

namespace RntCar.CrmPlugins.Contract.Action
{
    public class ExecuteUpdateContractforDelivery : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            List<ContractItemResponse> contractItemResponses = new List<ContractItemResponse>();
            string updateContractforDeliveryParameters;
            initializer.PluginContext.GetContextParameter<string>("updateContractforDeliveryParameters", out updateContractforDeliveryParameters);
            try
            {
                #region parameter serialization 
                
                //initializer.TraceMe("updateContractforDeliveryParameters" + updateContractforDeliveryParameters);

                var updateContractforDeliveryParametersSerialized = JsonConvert.DeserializeObject<UpdateContractforDeliveryParameters>(updateContractforDeliveryParameters);
                #endregion

                var utcNow = updateContractforDeliveryParametersSerialized.dateNowTimeStamp.converttoDateTime();

                #region Get Contract Detail
                ContractRepository contractRepository = new ContractRepository(initializer.Service);
                var contract = contractRepository.getContractById(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                  new string[] { "rnt_pickupbranchid",
                                                                                 "rnt_dropoffbranchid" ,
                                                                                 "rnt_reservationid" ,
                                                                                 "statuscode",
                                                                                 "transactioncurrencyid",
                                                                                 "rnt_groupcodeid",
                                                                                 "rnt_contracttypecode",
                                                                                 "rnt_totalamount",
                                                                                 "transactioncurrencyid",
                                                                                 "rnt_customerid",
                                                                                 "rnt_pnrnumber",
                                                                                 "rnt_pricinggroupcodeid",
                                                                                 "rnt_paymentmethodcode",
                                                                                 "rnt_generaltotalamount",
                                                                                 "rnt_ismonthly"
                                                                               });
                #endregion

                #region Create EquipmentTransactionHistory
                EquipmentTransactionBL equipmentTransactionBL = new EquipmentTransactionBL(initializer.Service, initializer.TracingService);
                initializer.TraceMe("createEquipmentTransactionHistoryforDelivery start");
                var equipmentTransactionId = equipmentTransactionBL.createEquipmentTransactionHistoryforDelivery(new CreateEquipmentTransactionHistoryDeliveryParameters
                {
                    contractId = updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                    deliveryFuelValue = updateContractforDeliveryParametersSerialized.equipmentInformation.currentFuelValue,
                    fuelValue = updateContractforDeliveryParametersSerialized.equipmentInformation.firstFuelValue,
                    deliveryKmValue = updateContractforDeliveryParametersSerialized.equipmentInformation.currentKmValue,
                    kmValue = updateContractforDeliveryParametersSerialized.equipmentInformation.firstKmValue,
                    equipmentId = updateContractforDeliveryParametersSerialized.equipmentInformation.equipmentId
                });
                initializer.TraceMe("createEquipmentTransactionHistoryforDelivery end");
                #endregion

                #region Get Contract Item with simple columns
                ContractItemRepository contractItemRepository = new ContractItemRepository(initializer.Service);
                var allitems = contractItemRepository.getContractItemsByContractId(updateContractforDeliveryParametersSerialized.contractInformation.contractId.ToString(),
                                                                                new string[] { "rnt_itemtypecode", "statuscode", "rnt_billingtype" });

                var equipmentChangedBeforeCount = allitems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value.Equals((int)rnt_contractitem_rnt_itemtypecode.Equipment) &&
                                                        (p.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.Completed ||
                                                         p.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.WaitingForDelivery)).ToList();

                bool equipmentChangedBefore = equipmentChangedBeforeCount.Count > 1 ? true : false;

                var items = contractItemRepository.getWaitingforDeliveryContractItemsByContractIdByGivenColumns(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                                                  new string[] { "rnt_billingtype", "rnt_pickupdatetime", "rnt_itemtypecode", "rnt_additionalproductid" });

                initializer.TraceMe("equipmentChangedBefore" + equipmentChangedBefore);
                if (updateContractforDeliveryParametersSerialized.changedEquipmentData != null &&
                  (updateContractforDeliveryParametersSerialized.changedEquipmentData.changeType == (int)rnt_ChangeType.Downsell || updateContractforDeliveryParametersSerialized.changedEquipmentData.changeType == (int)rnt_ChangeType.Upsell) &&
                   contract.GetAttributeValue<bool>("rnt_ismonthly") &&
                   equipmentChangedBefore)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>("Aylık sözleşmelerde araç değişimi esnasında upsell/downsell yapılamaz.Lütfen upgrade/downgrade ile işleminizi tamamlayınız.");
                }

                #endregion

              

                #region Check it is group Change and create equipment
                if (updateContractforDeliveryParametersSerialized.changedEquipmentData != null)
                {
                    if(updateContractforDeliveryParametersSerialized.changedEquipmentData.changeType == (int)rnt_ChangeType.Downsell &&
                       contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker)
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>("Broker/Acenta sözleşmelerinde downsell yapılamaz.");
                    }
                    var contractEquipment = items.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value ==
                                                            (int)ContractItemEnums.ItemTypeCode.Equipment).FirstOrDefault();

                    ContractItemBL contractItemBLChanged = new ContractItemBL(initializer.Service, initializer.TracingService);
                    contractItemBLChanged.updateContractItemChangeTypeandChangeReason(contractEquipment.Id, updateContractforDeliveryParametersSerialized.changedEquipmentData.changeType);
                    //todo contractitemstatuschange enum
                    initializer.TraceMe("deactivated item " + contractEquipment.Id);

                    contractItemResponses.Add(new ContractItemResponse
                    {
                        contractItemId = contractEquipment.Id,
                        status = (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Inactive
                    });
                    initializer.TraceMe("changedEquipmentData is not null and added Inactive item to list");

                    contractItemBLChanged.deactiveContractItemItemById(contractEquipment.Id, (int)rnt_contractitem_StatusCode.Inactive);

                    var entity = contractItemBLChanged.buildContractItemWithInitializeFromRequest(contractEquipment.Id, updateContractforDeliveryParametersSerialized.channelCode);
                    initializer.TraceMe("new item " + entity.Id);
                    var updateContractandContractItemWithNewGroupCodeParamaters = new UpdateContractandContractItemWithNewGroupCodeParamaters
                    {
                        trackingNumber = updateContractforDeliveryParametersSerialized.changedEquipmentData.trackingNumber,
                        changeType = updateContractforDeliveryParametersSerialized.changedEquipmentData.changeType,
                        contractId = updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                        contractItemName = updateContractforDeliveryParametersSerialized.changedEquipmentData.displayText,
                        groupCodeInformationId = updateContractforDeliveryParametersSerialized.changedEquipmentData.groupCodeId,
                        totalPrice = updateContractforDeliveryParametersSerialized.changedEquipmentData.totalPrice,
                        dropoffTimeStamp = updateContractforDeliveryParametersSerialized.contractInformation.DropoffTimeStamp,
                        pickupTimeStamp = updateContractforDeliveryParametersSerialized.contractInformation.PickupDateTimeStamp,
                        isManualProcess = updateContractforDeliveryParametersSerialized.contractInformation.isManuelProcess,
                        manualPickupTimeStamp = updateContractforDeliveryParametersSerialized.contractInformation.manuelPickupDateTimeStamp,
                        pricingGroupCode = contract.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid"),
                        equipmentId = updateContractforDeliveryParametersSerialized.equipmentInformation.equipmentId,
                        additionalProducts = updateContractforDeliveryParametersSerialized.additionalProducts,
                        userInformation = updateContractforDeliveryParametersSerialized.userInformation
                    };
                    var newContractItemId = contractItemBLChanged.
                                            updateContractandContractItemWithNewGroupCodeRelatedFields(entity,
                                                                                                       updateContractandContractItemWithNewGroupCodeParamaters,
                                                                                                       utcNow,
                                                                                                       equipmentChangedBefore);
                    //araç değişikliği yoksa 
                    //brokerlarda pickup günü güncelle
                    if (contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker && !equipmentChangedBefore)
                    {
                        var priceDiff = contractItemRepository.getPriceFactorDifference(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                                       new string[] { "rnt_pickupdatetime", "rnt_dropoffdatetime" });

                        if (priceDiff != null)
                        {
                            initializer.TraceMe("updating pricediff ");
                            Entity e = new Entity("rnt_contractitem");
                            e.Id = priceDiff.Id;
                            e["rnt_pickupdatetime"] = utcNow.AddMinutes(StaticHelper.offset);
                            e["rnt_dropoffdatetime"] = utcNow.AddMinutes(CommonHelper.calculateTotalDurationInMinutes(priceDiff.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                                                                                      priceDiff.GetAttributeValue<DateTime>("rnt_dropoffdatetime")))
                                                                                                                      .AddMinutes(StaticHelper.offset);
                            initializer.Service.Update(e);

                            contractItemResponses.Add(new ContractItemResponse
                            {
                                contractItemId = priceDiff.Id,
                                status = (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Completed,
                                itemTypeCode = (int)rnt_contractitem_rnt_itemtypecode.PriceDifference
                            });

                        }
                        else
                        {
                            initializer.TraceMe("not updating pricediff ");
                        }
                    }

                    contractItemResponses.Add(new ContractItemResponse
                    {
                        contractItemId = newContractItemId,
                        status = (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Rental
                    });
                    initializer.TraceMe("changedEquipmentData is not null and rental Inactive item to list");

                }

                #endregion

                #region get contract invoice
                InvoiceRepository invoiceRepository = new InvoiceRepository(initializer.Service);
                var invoice = invoiceRepository.getFirstInvoiceByContractId(updateContractforDeliveryParametersSerialized.contractInformation.contractId);
                if (invoice == null || invoice.Id == Guid.Empty)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>("Sözleşmeye bağlı işlem yapılabilecek (Taslak) fatura mevcut değildir.");
                }

                #endregion

                #region updateAdditionalProductsForContract 
                initializer.TraceMe("additional product operation start");
                //additional product operations      
                ContractHelper contractHelper = new ContractHelper(initializer.Service);
                var duration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(updateContractforDeliveryParametersSerialized.contractInformation.PickupDateTimeStamp.converttoDateTime(),
                                                                                               updateContractforDeliveryParametersSerialized.contractInformation.DropoffTimeStamp.converttoDateTime());
                var contractUpdateParameter = new AdditionalProductMapper()
                                            .buildUpdateAdditionalProductsForContractParameter(contract,
                                             updateContractforDeliveryParametersSerialized,
                                             false,
                                             false,
                                             (int)ContractItemEnums.ChangeReason.CustomerDemand,
                                             duration);
                initializer.TraceMe("contractUpdateParameter : " + JsonConvert.SerializeObject(contractUpdateParameter));
                var userId = updateContractforDeliveryParametersSerialized.userInformation?.userId;
                initializer.TraceMe("userId : " + userId);
                AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(initializer.Service);
                var createdItems = additionalProductsBL.updateAdditionalProductsForContract(
                contractUpdateParameter,
                items,
                invoice.Id,
                updateContractforDeliveryParametersSerialized.channelCode,
                userId);
                initializer.TraceMe("additional product operation end");
                #endregion

                #region updateContractDeliveryRelatedFields
                ContractBL contractBL = new ContractBL(initializer.Service);
                //prevent not duplicate records on change history
                if (updateContractforDeliveryParametersSerialized.changedEquipmentData == null)
                {
                    var r = contractRepository.getContractById(updateContractforDeliveryParametersSerialized.contractInformation.contractId, new string[] { "rnt_pricinggroupcodeid" });
                    initializer.TraceMe("updateContractDeliveryRelatedFields start");
                    contractBL.updateContractDeliveryRelatedFields(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                   updateContractforDeliveryParametersSerialized.contractInformation.PickupDateTimeStamp,
                                                                   updateContractforDeliveryParametersSerialized.contractInformation.DropoffTimeStamp,
                                                                   updateContractforDeliveryParametersSerialized.contractInformation.manuelPickupDateTimeStamp,
                                                                   updateContractforDeliveryParametersSerialized.contractInformation.isManuelProcess,
                                                                   utcNow,
                                                                   r.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id,
                                                                   updateContractforDeliveryParametersSerialized.additionalProducts,
                                                                   updateContractforDeliveryParametersSerialized.userInformation.userId,
                                                                   updateContractforDeliveryParametersSerialized.equipmentInformation.equipmentId,
                                                                   equipmentChangedBefore);
                    initializer.TraceMe("updateContractDeliveryRelatedFields end");
                }

                #endregion

                #region GetContractItems again with updated data
                initializer.TraceMe("ContractItemRepository start");
                //need to retrieve again
                //todo need a a logic implementation to get rid of multiple select
                items = contractItemRepository.getWaitingforDeliveryContractItemsByContractIdByGivenColumns(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                                               new string[] { "rnt_itemtypecode" });
                initializer.TraceMe("ContractItemRepository end items count : " + items.Count);
                initializer.TraceMe("ContractItemRepository end");

                #endregion

                #region updateContractItemsDeliveryRelatedFields

                ContractItemBL contractItemBL = new ContractItemBL(initializer.Service);
                initializer.TraceMe("updateContractItemsDeliveryRelatedFields start");

                contractItemBL.updateContractItemsDeliveryRelatedFields(updateContractforDeliveryParametersSerialized,
                                                                        items.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value ==
                                                                        (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.AdditionalProduct).ToList(),
                                                                        createdItems,
                                                                        utcNow,
                                                                        equipmentChangedBefore);
                //araç değişikliği varsa yukarıda bütün işlemler yapılıyor.
                //araç değişikliği olmadığı senaryoda equipment tipli item'ın tarih, plaka vs gibi bilgilerinin güncellenmesi için
                if (updateContractforDeliveryParametersSerialized.changedEquipmentData == null)
                {
                    var item = items.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value ==
                                                                  (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Equipment).ToList();

                    contractItemBL.updateContractItemsDeliveryRelatedFields(updateContractforDeliveryParametersSerialized,
                                                                            item,
                                                                            item.Select(p => p.Id).ToList(),
                                                                            utcNow,
                                                                            equipmentChangedBefore);
                    initializer.TraceMe("changedEquipmentData is null and added rental item to list");
                    contractItemResponses.Add(new ContractItemResponse
                    {
                        contractItemId = item.FirstOrDefault().Id,
                        status = (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Rental
                    });

                    if (contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker)
                    {
                        var priceDiff = contractItemRepository.getPriceFactorDifference(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                                        new string[] { "rnt_pickupdatetime", "rnt_dropoffdatetime" });

                        if (priceDiff != null)
                        {
                            Entity e = new Entity("rnt_contractitem");
                            e.Id = priceDiff.Id;
                            e["rnt_pickupdatetime"] = utcNow.AddMinutes(StaticHelper.offset);
                            e["rnt_dropoffdatetime"] = utcNow.AddMinutes(CommonHelper.calculateTotalDurationInMinutes(priceDiff.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                                                                                                                      priceDiff.GetAttributeValue<DateTime>("rnt_dropoffdatetime")))
                                                                                                                      .AddMinutes(StaticHelper.offset);

                            initializer.Service.Update(e);
                            contractItemResponses.Add(new ContractItemResponse
                            {
                                contractItemId = priceDiff.Id,
                                status = (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Completed,
                                itemTypeCode = (int)rnt_contractitem_rnt_itemtypecode.PriceDifference
                            });
                        }
                    }

                }

                //price calculation
                initializer.TraceMe("updateContractItemsDeliveryRelatedFields end");
                #endregion

                #region createEquipmentInventoryHistoryforDelivery
                var inventoryList = updateContractforDeliveryParametersSerialized.equipmentInformation.equipmentInventoryData.
                                 ConvertAll(x => new CreateEquipmentInventoryHistoryParameter
                                 {
                                     isExists = x.isExist.Value,
                                     logicalName = x.logicalName
                                 }).ToList();

                EquipmentInventoryBL equipmentInventoryBL = new EquipmentInventoryBL(initializer.Service);
                var equipmentInventoryId = equipmentInventoryBL.createEquipmentInventoryHistoryforDelivery(updateContractforDeliveryParametersSerialized.equipmentInformation.equipmentId,
                                                                                                           updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                                                           null,
                                                                                                           inventoryList);
                #endregion

                #region Update Equipment
                initializer.TraceMe("update equipment start");
                EquipmentBL equipmentBL = new EquipmentBL(initializer.Service);
                equipmentBL.updateEquipmentforDelivery(updateContractforDeliveryParametersSerialized.equipmentInformation.equipmentId,
                                                       equipmentInventoryId,
                                                       equipmentTransactionId,
                                                       updateContractforDeliveryParametersSerialized.equipmentInformation.currentKmValue,
                                                       updateContractforDeliveryParametersSerialized.equipmentInformation.currentFuelValue);
                initializer.TraceMe("update equipment end");
                #endregion

                #region Create Damages
                var damageParameters = new CreateDamageParameter
                {
                    contractId = updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                    equipmentId = updateContractforDeliveryParametersSerialized.equipmentInformation.equipmentId,
                    damageData = updateContractforDeliveryParametersSerialized.damageData,
                    userInformation = updateContractforDeliveryParametersSerialized.userInformation
                };
                DamageBL damageBL = new DamageBL(initializer.Service, initializer.TracingService);
                var response = damageBL.createDamages(damageParameters);
                if (!response.responseResult.result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.responseResult.exceptionDetail);
                }
                #endregion

                #region PaymentPlans if it monthly
                //eğer upsell'se ödeme planlarını yeniden olusturmak lazım.
                if (updateContractforDeliveryParametersSerialized.changedEquipmentData != null && contract.GetAttributeValue<bool>("rnt_ismonthly") && !equipmentChangedBefore)
                {

                    initializer.TraceMe("PaymentPlans if it monthly");

                    if ((updateContractforDeliveryParametersSerialized.changedEquipmentData.changeType == (int)rnt_ChangeType.Upsell ||
                        updateContractforDeliveryParametersSerialized.changedEquipmentData.changeType == (int)rnt_ChangeType.Downsell))
                    {
                        PaymentPlanBL paymentPlanBL = new PaymentPlanBL(initializer.Service);
                        paymentPlanBL.deletePaymentPlansByContractId(updateContractforDeliveryParametersSerialized.contractInformation.contractId);
                        paymentPlanBL.createPaymentPlans(updateContractforDeliveryParametersSerialized.paymentPlans, null, updateContractforDeliveryParametersSerialized.contractInformation.contractId);
                    } 
                }
                #endregion

                #region Create Invoice Date for monthly
                if (contract.GetAttributeValue<bool>("rnt_ismonthly") && !equipmentChangedBefore)
                {
                    initializer.TraceMe("Create Invoice Date for monthly");
                    var c = contractRepository.getContractById(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                 new string[] { "rnt_generaltotalamount" });
                    List<InvoiceItemTemplate> inoiceItemTemplates = new List<InvoiceItemTemplate>();
                    var con = contractItemRepository.getRentalEquipmentContractItemByGivenColumns(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                                        new string[] {"rnt_equipment",
                                                                                                      "rnt_dropoffdatetime",
                                                                                                      "rnt_pickupdatetime",
                                                                                                      "rnt_totalamount" });
                    inoiceItemTemplates.Add(new InvoiceItemTemplate
                    {
                        contractItemId = con.Id,
                        amount = con.GetAttributeValue<Money>("rnt_totalamount").Value,
                        equipmentId = con.GetAttributeValue<EntityReference>("rnt_equipment").Id,
                        itemType = (int)rnt_contractitem_rnt_itemtypecode.Equipment

                    });
                    initializer.TraceMe("before select additional product start");
                    var additionalProducts = contractItemRepository.getRentalContractItemAdditionalProductsByContractIdByGivenColumns(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                                                                                      new string[] { "rnt_itemtypecode", "rnt_additionalproductid", "rnt_totalamount" });


                    initializer.TraceMe("before select additional product end");

                    foreach (var item in additionalProducts)
                    {
                        inoiceItemTemplates.Add(new InvoiceItemTemplate
                        {
                            contractItemId = item.Id,
                            amount = item.GetAttributeValue<Money>("rnt_totalamount").Value,
                            additionalProductId = item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id,
                            itemType = (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct

                        });
                    }
                    contractBL.createContractInvoiceDate(new CreateContractInvoiceDateParameters
                    {
                        amount = c.GetAttributeValue<Money>("rnt_generaltotalamount").Value,
                        //1 means monthly
                        type = 1,
                        contractId = updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                        invoiceDate = con.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                        pickupDatime = con.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                        dropoffDateTime = con.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                        templates = JsonConvert.SerializeObject(inoiceItemTemplates)
                    });
                }
                #endregion


                #region Last Rental Office

                Entity contactUpdate = new Entity("contact");
                contactUpdate.Id = contract.GetAttributeValue<EntityReference>("rnt_customerid").Id;
                contactUpdate["rnt_lastrentalofficeid"] = new EntityReference("rnt_branch", contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id);
                initializer.Service.Update(contactUpdate);

                #endregion

                var corpContract = contractHelper.checkMakePayment_CorporateContracts(contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value,
                                                                                      contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value);

                initializer.TraceMe("corpContract :" + corpContract);
                if (!corpContract)
                {
                    #region Get Contract Amounts
                    var contractPaymentAmount = contractHelper.calculatePaymentAmount(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                                                      updateContractforDeliveryParametersSerialized.changedEquipmentData?.groupCodeId,
                                                                                      updateContractforDeliveryParametersSerialized.changedEquipmentData?.changeType,
                                                                                      true);
                    initializer.TraceMe("differenceAmount" + JsonConvert.SerializeObject(contractPaymentAmount));

                    if (contractPaymentAmount.contractAmountDifference > 1 && updateContractforDeliveryParametersSerialized.paymentInformation.creditCardData.Count == 0)
                    {
                        //todo xml
                        var m = string.Format("Ödenecek tutar mevcut : {0}.Lütfen kredi kartı seçimi yapın.", contractPaymentAmount.contractAmountDifference);
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(m);
                    }
                    #endregion

                    #region Deposit and Payment Card Selection
                    var paymentCard = updateContractforDeliveryParametersSerialized.paymentInformation.creditCardData.Where(x => x.cardType == (int)PaymentEnums.PaymentTransactionType.SALE &&
                                                                  (x.creditCardId != null || !string.IsNullOrEmpty(x.creditCardNumber))).FirstOrDefault();

                    initializer.TraceMe("paymentCard" + JsonConvert.SerializeObject(paymentCard));

                    var depositCard = updateContractforDeliveryParametersSerialized.paymentInformation.creditCardData.Where(x => x.cardType == (int)PaymentEnums.PaymentTransactionType.DEPOSIT &&
                                                            (x.creditCardId != null || !string.IsNullOrEmpty(x.creditCardNumber))).FirstOrDefault();
                    initializer.TraceMe("depositCard" + JsonConvert.SerializeObject(depositCard));
                    #endregion

                    #region Check Double Credit Card

                    var doubleCreditCard = contractRepository.getContractById(updateContractforDeliveryParametersSerialized.contractInformation.contractId, new string[] { "rnt_doublecreditcard" });
                    initializer.TraceMe("doubleCreditCard" + doubleCreditCard.GetAttributeValue<bool>("rnt_doublecreditcard"));
                    var creditCardResponse = contractHelper.checkDoubleCreditCard(contractPaymentAmount.contractAmountDifference,
                                                                                 contractPaymentAmount.contractDepositAmountDifference,
                                                                                 paymentCard,
                                                                                 depositCard,
                                                                                 doubleCreditCard.GetAttributeValue<bool>("rnt_doublecreditcard"),
                                                                                 contract.GetAttributeValue<EntityReference>("rnt_customerid").Id,
                                                                                 updateContractforDeliveryParametersSerialized.langId,
                                                                                 updateContractforDeliveryParametersSerialized.changedEquipmentData?.changeType);
                    initializer.TraceMe("creditCardResponse" + creditCardResponse);
                    if (!string.IsNullOrEmpty(creditCardResponse))
                    {
                        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(creditCardResponse);
                    }
                    #endregion

                    #region Update Contract header with the new deposit amount
                    //update Contract with the new deposit difference
                    if (contractPaymentAmount.contractDepositAmountDifference != decimal.Zero 
                        && (contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value != (int)rnt_PaymentMethodCode.Current
                        || contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value != (int)rnt_PaymentMethodCode.FullCredit
                        || contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value != (int)rnt_PaymentMethodCode.Corporate))
                    {
                        contractBL.updateContractDepositAmount(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                               contractPaymentAmount.totalDepositAmount);
                    }
                    #endregion

                    #region Make Payment     

                    initializer.TraceMe("contractPaymentAmount : " + JsonConvert.SerializeObject(contractPaymentAmount));
                    if (contractPaymentAmount.contractAmountDifference > StaticHelper._one_ ||
                        contractPaymentAmount.contractAmountDifference < (-1 * StaticHelper._one_) ||
                        contractPaymentAmount.contractDepositAmountDifference != decimal.Zero)
                    {
                        var vPosResponse = new VirtualPosResponse
                        {
                            ResponseResult = new ClassLibrary.ResponseResult
                            {
                                Result = true
                            },
                            virtualPosId = 0
                        };

                        initializer.TraceMe("payment start");
                        initializer.TraceMe("vPosResponse.virtualPosId" + vPosResponse.virtualPosId);


                        var contractPaymentResponse = contractBL.makeContractPayment(contractPaymentAmount.contractAmountDifference,
                                                                                     contractPaymentAmount.contractDepositAmountDifference,
                                                                                     paymentCard,
                                                                                     depositCard,
                                                                                     null,
                                                                                     contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id,
                                                                                     contract.GetAttributeValue<EntityReference>("rnt_customerid").Id,
                                                                                     contract.Id,
                                                                                     Guid.Empty,
                                                                                     contract.GetAttributeValue<string>("rnt_pnrnumber"),
                                                                                     new PaymentStatus
                                                                                     {
                                                                                         isDepositPaid = contractPaymentAmount.contractDepositAmountDifference == decimal.Zero ? true : false,
                                                                                         isReservationPaid = contractPaymentAmount.contractAmountDifference == decimal.Zero ? true : false,
                                                                                     },
                                                                                     updateContractforDeliveryParametersSerialized.langId,
                                                                                     vPosResponse.virtualPosId,
                                                                                     1,
                                                                                     rnt_PaymentChannelCode.TABLET,
                                                                                     updateContractforDeliveryParametersSerialized.paymentInformation.use3DSecure,
                                                                                     updateContractforDeliveryParametersSerialized.paymentInformation.callBackUrl);

                        if (!contractPaymentResponse.ResponseResult.Result)
                        {
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(contractPaymentResponse.ResponseResult.ExceptionDetail);
                        }

                        initializer.TraceMe("payment end");
                    }
                    #endregion

                }

                #region Update Contract Header Rental User
                initializer.TraceMe("updateContractforDeliveryParametersSerialized.userInformation.userId start");
                contractBL.updateContractDeliveryUser(updateContractforDeliveryParametersSerialized.contractInformation.contractId,
                                                      updateContractforDeliveryParametersSerialized.userInformation.userId);
                initializer.TraceMe("updateContractforDeliveryParametersSerialized.userInformation.userId end");

                #endregion

                #region sending equipment type item to mongodb

                initializer.TraceMe("mongodb started");
                try
                {
                    foreach (var item in contractItemResponses)
                    {
                        //deactivate old item
                        if (item.status == (int)rnt_contractitem_StatusCode.Inactive)
                        {
                            initializer.TraceMe("mongodb inactive start");
                            initializer.RetryMethod<MongoDBResponse>(() => contractHelper.updateContractItemInMongoDB(item), StaticHelper.retryCount, StaticHelper.retrySleep);
                            initializer.TraceMe("mongodb inactive end");
                        }
                        else if (item.status == (int)rnt_contractitem_StatusCode.Rental)
                        {
                            if (updateContractforDeliveryParametersSerialized.changedEquipmentData == null)
                            {
                                initializer.TraceMe("mongodb Rental update start");
                                initializer.RetryMethod<MongoDBResponse>(() => contractHelper.updateContractItemInMongoDB(item), StaticHelper.retryCount, StaticHelper.retrySleep);
                                initializer.TraceMe("mongodb rental Rental end");
                            }
                            else
                            {
                                initializer.TraceMe("mongodb Rental create start");
                                initializer.RetryMethod<MongoDBResponse>(() => contractHelper.createContractItemInMongoDB(item), StaticHelper.retryCount, StaticHelper.retrySleep);
                                initializer.TraceMe("mongodb Rental create end");

                            }

                        }
                        if (item.itemTypeCode == (int)rnt_contractitem_rnt_itemtypecode.PriceDifference)
                        {
                            initializer.TraceMe("mongodb Rental create start");
                            initializer.RetryMethod<MongoDBResponse>(() => contractHelper.createContractItemInMongoDB(item), StaticHelper.retryCount, StaticHelper.retrySleep);
                            initializer.TraceMe("mongodb Rental create end");
                        }

                    }


                }
                catch (Exception ex)
                {
                    //will think a logic
                    initializer.TraceMe("mongodb integration error : " + ex.Message);
                }
                initializer.TraceMe("mongodb end");

                #endregion
                initializer.TraceMe("updateContractforDeliveryParameters : " + updateContractforDeliveryParameters);


                initializer.PluginContext.OutputParameters["UpdateContractforDeliveryResponse"] = JsonConvert.SerializeObject(ClassLibrary._Tablet.ResponseResult.ReturnSuccess());
            }
            catch (Exception ex)
            {
                initializer.TraceMe("updateContractforDeliveryParameters : " + updateContractforDeliveryParameters);
                initializer.TraceMe("delivery error : " + ex.Message);

                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
