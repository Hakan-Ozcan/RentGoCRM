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
    public class ExecuteUpdateContractforRental : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            ContractHelper contractHelper = new ContractHelper(initializer.Service, initializer.TracingService);
            string updateContractforRentalParameters;
            initializer.PluginContext.GetContextParameter<string>("updateContractforRentalParameters", out updateContractforRentalParameters);
            var updateContractforRentalParametersSerialized = JsonConvert.DeserializeObject<UpdateContractforRentalParameters>(updateContractforRentalParameters);
            if (updateContractforRentalParametersSerialized.contractInformation.isManuelProcess)
            {
                updateContractforRentalParametersSerialized.contractInformation.DropoffTimeStamp = updateContractforRentalParametersSerialized.contractInformation.manuelDropoffTimeStamp;
            }
            else
            {
                updateContractforRentalParametersSerialized.contractInformation.DropoffTimeStamp = DateTime.UtcNow.converttoTimeStamp();
            }

            try
            {

                ContractItemRepository contractItemRepository = new ContractItemRepository(initializer.Service);
                var contractItemsCheck = contractItemRepository.getContractEquipmentsByGivenColumns(updateContractforRentalParametersSerialized.contractInformation.contractId, new string[] { "statuscode" });


                if (contractItemsCheck.Where(p => p.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.Rental).ToList().Count > 0 &&
                contractItemsCheck.Where(p => p.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)rnt_contractitem_StatusCode.WaitingForDelivery).ToList().Count > 0 &&
                !updateContractforRentalParametersSerialized.equipmentInformation.isEquipmentChanged)
                {
                    throw new Exception("Parametrelerde problem var. Lütfen merkez ile görüşün");
                }
                //initializer.TraceMe("params : " + updateContractforRentalParameters);
                // initializer.TraceMe("dropoff " + updateContractforRentalParametersSerialized.contractInformation.DropoffTimeStamp.converttoDateTime());

                #region Get Contract Detail
                ContractRepository contractRepository = new ContractRepository(initializer.Service);
                var contract = contractRepository.getContractById(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                  new string[] { "rnt_pickupbranchid",
                                                                                  "rnt_dropoffbranchid" ,
                                                                                  "rnt_reservationid" ,
                                                                                  "rnt_totalamount",
                                                                                  "rnt_pickupdatetime",
                                                                                  "transactioncurrencyid",
                                                                                  "rnt_contracttypecode",
                                                                                  "rnt_groupcodeid",
                                                                                  "rnt_customerid",
                                                                                  "rnt_pnrnumber",
                                                                                  "statuscode",
                                                                                  "rnt_ismonthly",
                                                                                  "rnt_contracttypecode",
                                                                                  "rnt_paymentmethodcode",
                                                                                  "rnt_corporateid"
                                                                               });
                #endregion

                #region createEquipmentInventoryHistoryfor Rental
                initializer.TraceMe("createEquipmentInventoryHistoryforrental start");
                var inventoryList = updateContractforRentalParametersSerialized.equipmentInformation.equipmentInventoryData.
                                  ConvertAll(x => new CreateEquipmentInventoryHistoryParameter
                                  {
                                      isExists = x.isExist.Value,
                                      logicalName = x.logicalName
                                  }).ToList();
                EquipmentInventoryBL equipmentInventoryBL = new EquipmentInventoryBL(initializer.Service);
                var equipmentInventoryId = equipmentInventoryBL.createEquipmentInventoryHistoryforRental(updateContractforRentalParametersSerialized.equipmentInformation.equipmentId,
                                                                                                         updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                                         null,
                                                                                                         inventoryList);
                initializer.TraceMe("createEquipmentInventoryHistoryforrental end");
                #endregion

                #region Equipment Transaction Operations

                //initializer.TraceMe("updateEquipmentTransactionHistoryforRental start");

                EquipmentRepository equipmentRepository = new EquipmentRepository(initializer.Service);
                var currentEquipmentTransaction = equipmentRepository.getEquipmentByIdByGivenColumns(updateContractforRentalParametersSerialized.equipmentInformation.equipmentId,
                                                                                                     new string[] { "rnt_equipmenttransactionid" });

                EquipmentTransactionBL equipmentTransactionBL = new EquipmentTransactionBL(initializer.Service);
                equipmentTransactionBL.updateEquipmentTransactionHistoryforRental(new CreateEquipmentTransactionHistoryRentalParameters
                {
                    contractId = updateContractforRentalParametersSerialized.contractInformation.contractId,
                    firstFuelValue = updateContractforRentalParametersSerialized.equipmentInformation.firstFuelValue,
                    firstKmValue = updateContractforRentalParametersSerialized.equipmentInformation.firstKmValue,
                    equipmentId = updateContractforRentalParametersSerialized.equipmentInformation.equipmentId,
                    rentalFuelValue = updateContractforRentalParametersSerialized.equipmentInformation.currentFuelValue,
                    rentalKmValue = updateContractforRentalParametersSerialized.equipmentInformation.currentKmValue,
                    equipmentTransactionId = currentEquipmentTransaction.GetAttributeValue<EntityReference>("rnt_equipmenttransactionid").Id
                });
                //initializer.TraceMe("updateEquipmentTransactionHistoryforRental end");
                #endregion

                #region Get Configuration one way code
                initializer.TraceMe("get additional product code from configuration start");
                ConfigurationRepository configurationRepository = new ConfigurationRepository(initializer.Service);
                var oneWayFeeCode = configurationRepository.GetConfigurationByKey("OneWayFeeCode");
                initializer.TraceMe("get additional product code from configuration end");
                #endregion

                #region GetAdditional Products
                initializer.TraceMe("get additional product by code start");
                AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(initializer.Service);
                var product = additionalProductRepository.getAdditionalProductByProductCode(oneWayFeeCode);
                initializer.TraceMe("get additional product by code end");
                #endregion

                #region Get Contract One Way Fee and Operations

                var contractOneWayFee = contractItemRepository.getActiveContractItemByAdditionalProductIdandContractId(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                                                 product.Id);
                ContractItemBL contractItemBL = new ContractItemBL(initializer.Service);
                ContractBL contractBL = new ContractBL(initializer.Service, initializer.TracingService);

                var _items = contractItemRepository.getRentalContractItemAdditionalProductsByContractIdByGivenColumns(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                                                      new string[] { "rnt_pickupdatetime", "rnt_itemtypecode", "rnt_additionalproductid" });

                if (contractOneWayFee != null)
                {
                    _items = _items.Where(p => p.Id != contractOneWayFee.Id).ToList();
                    var oneWayFeeProduct = updateContractforRentalParametersSerialized.otherAdditionalProducts
                                           .Where(p => p.productCode == oneWayFeeCode)
                                           .FirstOrDefault();

                    //initializer.TraceMe("contract.GetAttributeValue<EntityReference>('rnt_dropoffbranchid').Id)" + contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name);
                    //initializer.TraceMe("updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId" + updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchName);
                    if (oneWayFeeProduct != null &&
                        contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id == updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId)
                    {
                        contractItemBL.deactiveContractItemItemById(contractOneWayFee.Id, (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Inactive);
                        updateContractforRentalParametersSerialized.otherAdditionalProducts.Remove(oneWayFeeProduct);
                        initializer.TraceMe("one way is removed from list");
                    }
                    //var olanı deaktif edelim yeni oneway calculate remaining amountta hesaplanıyor zaten
                    else if (oneWayFeeProduct != null &&
                        contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id != updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId)
                    {
                        initializer.TraceMe("we need to deactivate the one way code");
                        // first disable contract one way fee
                        contractItemBL.deactiveContractItemItemById(contractOneWayFee.Id, (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Inactive);
                        // if value eq zero, disable contract one way fee and remove from create list
                    }
                }
                #endregion

                #region get contract invoice
                InvoiceRepository invoiceRepository = new InvoiceRepository(initializer.Service);
                var invoice = invoiceRepository.getFirstInvoiceByContractId(updateContractforRentalParametersSerialized.contractInformation.contractId);
                if (invoice == null || invoice.Id == Guid.Empty)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>("Sözleşmeye bağlı işlem yapılabilecek (Taslak) fatura mevcut değildir.");
                }
                #endregion

                #region HGS Calculate Operations 

                AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(initializer.Service);

                decimal hgsAmount = 0;
                var hgsCode = configurationRepository.GetConfigurationByKey("additionalProduct_HGS");
                var hgsAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(hgsCode);
                var hgsAdditionalProductContractItemList = contractItemBL.getContractItemsByAdditionalProduct(hgsAdditionalProduct.Id, contract.Id);
                foreach (var hgsAdditionalProductContractItem in hgsAdditionalProductContractItemList.Entities)
                {
                    Money totalAmount = hgsAdditionalProductContractItem.GetAttributeValue<Money>("rnt_totalamount");
                    hgsAmount = hgsAmount + totalAmount.Value;
                }


                var serviceContractItem = additionalProductHelper.getAdditionalProductService_Contract(hgsAdditionalProduct.Id, contract.Id);
                if (serviceContractItem.subProduct != null && serviceContractItem.serviceItem == null && hgsAmount != 0)
                {
                    EntityReference corporateRef = contract.GetAttributeValue<EntityReference>("rnt_corporateid");
                    var corporateId = corporateRef != null ? corporateRef.Id : Guid.Empty;
                    var subAmount = additionalProductHelper.calculateFineProductServicePrice(Guid.Empty, hgsAdditionalProduct.Id, hgsAmount, serviceContractItem.subProduct.GetAttributeValue<Money>("rnt_price").Value);
                    Guid contractSubItemId = contractItemBL.CreateContractItem(contract.Id,
                                                                         subAmount,
                                                                         serviceContractItem.subProduct.Id,
                                                                         (int)rnt_ReservationChannel.Tablet, (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Fine, updateContractforRentalParametersSerialized.userInformation.userId, corporateId, false);
                }
                #endregion

                #region Traffic Fines Closed 26.09.2022 Tabletten Trafik Cezası Kesilmemektedir.
                //if (updateContractforRentalParametersSerialized.fineList != null &&
                //    updateContractforRentalParametersSerialized.fineList.Count > 0)
                //{
                //    TrafficBL trafficBL = new TrafficBL(initializer.Service);
                //    foreach (var item in updateContractforRentalParametersSerialized.fineList)
                //    {
                //        try
                //        {
                //            var equipmentItem = contractItemRepository.getRentalEquipmentContractItemByGivenColumns(updateContractforRentalParametersSerialized.contractInformation.contractId, new string[] { "rnt_equipment" });
                //            var equipmentRef = equipmentItem.GetAttributeValue<EntityReference>("rnt_equipment");

                //            trafficBL.createFineProduct(item,
                //                                        updateContractforRentalParametersSerialized.contractInformation.contractId,
                //                                        equipmentItem.Id,
                //                                        updateContractforRentalParametersSerialized.equipmentInformation.equipmentId);
                //        }
                //        catch (Exception ex)
                //        {
                //            initializer.TraceMe("fineList create error " + ex.Message);
                //            continue;
                //        }
                //    }
                //}
                #endregion

                #region updateAdditionalProductsForContract 
                initializer.TraceMe("additional product operation start");

                ContractItemRepository contractItemRepositoryCrm = new ContractItemRepository(initializer.Service);
                var manualDiscount = contractItemRepositoryCrm.getDiscountContractItem(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                       new string[] { "rnt_totalamount" });

                if (manualDiscount != null)
                {
                    _items = _items.Where(p => p.Id != manualDiscount.Id).ToList();
                }
                if (_items.Count > 0 &&
                    updateContractforRentalParametersSerialized.additionalProducts.Count == 0 &&
                   !updateContractforRentalParametersSerialized.equipmentInformation.isEquipmentChanged)
                {
                    initializer.TraceMe("param 12: " + JsonConvert.SerializeObject(updateContractforRentalParametersSerialized));
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>("Ek ürün hesaplamasında sorun var. Lütfen işleme en baştan başlayınız.Tekrar hata almanız durumunda Sistem Yöneticisi ile görüşün");
                }
                if (updateContractforRentalParametersSerialized.additionalProducts != null &&
                    updateContractforRentalParametersSerialized.additionalProducts.Count > 0)
                {
                    //initializer.TraceMe("ContractItemRepository start");
                    var items = contractItemRepository.getRentalContractItemsByContractIdByGivenColumns(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                                        new string[] { "rnt_pickupdatetime", "rnt_itemtypecode", "rnt_additionalproductid" });
                    //initializer.TraceMe("ContractItemRepository end");

                    var duration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(updateContractforRentalParametersSerialized.contractInformation.PickupDateTimeStamp.converttoDateTime(),
                                                                                                   updateContractforRentalParametersSerialized.contractInformation.DropoffTimeStamp.converttoDateTime());
                    //additional product operations                
                    var contractUpdateParameter = new AdditionalProductMapper()
                                                 .buildUpdateAdditionalProductsForContractParameter(contract,
                                                  updateContractforRentalParametersSerialized,
                                                  false,
                                                  true,
                                                  (int)ContractItemEnums.ChangeReason.CustomerDemand,
                                                  duration);
                    //initializer.TraceMe("contractUpdateParameter" + contractUpdateParameter);

                    contractUpdateParameter.contractItemStatusCode = (int)ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Completed;
                    AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(initializer.Service);
                    additionalProductsBL.updateAdditionalProductsForContract(
                    contractUpdateParameter,
                    items,
                    invoice.Id,
                    updateContractforRentalParametersSerialized.channelCode,
                    null);
                    initializer.TraceMe("additional product operation end");
                }
                #endregion

                #region Create Other additional Products
                initializer.TraceMe("creating other additional products create start");

                //var additionalProduct_OtherCode = configurationRepository.GetConfigurationByKey("additionalProduct_otherkey");
                //var other_product = additionalProductRepository.getAdditionalProductByProductCode(additionalProduct_OtherCode);

                foreach (var item in updateContractforRentalParametersSerialized.otherAdditionalProducts)
                {
                    var additionalProductData = new AdditionalProductData();
                    //copy reservationItemData to ReservationItemDataMongoDB
                    additionalProductData = additionalProductData.Map(item);

                    var itemTypeCode = (int)rnt_contractitem_rnt_itemtypecode.Fine;

                    if (item.productType != (int)rnt_additionalproduct_rnt_additionalproducttype.Fine)
                    {
                        itemTypeCode = (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct;
                    }
                    initializer.TraceMe("itemTypeCode : " + itemTypeCode);
                    initializer.TraceMe("item tobePaidAmount : " + additionalProductData.tobePaidAmount);

                    Guid temp = contractItemBL.createOtherTypeAdditionalProductDataforRental(additionalProductData,
                    new ContractItemRequiredData
                    {
                        contactId = updateContractforRentalParametersSerialized.contractInformation.contactId,//new Guid("7F824836-97F6-E811-A94F-000D3A454330"),
                        contractId = updateContractforRentalParametersSerialized.contractInformation.contractId,
                        statuscode = contract.GetAttributeValue<bool>("rnt_ismonthly") ? (int)rnt_contractitem_StatusCode.Rental : (int)rnt_contractitem_StatusCode.Completed,
                        transactionCurrencyId = contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id,
                        dropoffBranchId = updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId.Value,
                        pickupBranchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                        //because crm stores without utc
                        pickupDateTime = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                        groupCodeInformationId = contract.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id,
                        dropoffDateTime = updateContractforRentalParametersSerialized.contractInformation.DropoffTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset),
                        itemTypeCode = itemTypeCode
                    },
                    invoice.Id,
                    updateContractforRentalParametersSerialized.userInformation.userId);
                    initializer.TraceMe("other additional product created");

                }
                initializer.TraceMe("creating other additional products create end");
                #endregion


                Guid contractEquipmentId = Guid.Empty;
                #region Check Equipment change and Update Contract , ContractItems accordingly
                //eğer araç değişikliği varsa ek ürünleri ve contract headerı update etmemesi için
                //bir contractta hem waiting for dlivery hemde rental statüsünde araç varsa bu araç değişikliği demektir
                if (updateContractforRentalParametersSerialized.equipmentInformation.isEquipmentChanged)
                {
                    initializer.TraceMe("updateContractforRentalParametersSerialized.equipmentInformation.isEquipmentChanged");

                    contractEquipmentId = contractItemBL.updateContractItemRentalRelatedFields(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                               updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId,
                                                                                               updateContractforRentalParametersSerialized.contractInformation.DropoffTimeStamp,
                                                                                               updateContractforRentalParametersSerialized.contractInformation.isManuelProcess,
                                                                                               updateContractforRentalParametersSerialized.trackingNumber,
                                                                                               updateContractforRentalParametersSerialized.totalAmount);

                    var items = contractItemRepository.getRentalContractItemAdditionalProductsByContractIdByGivenColumns(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                                             new string[] { });

                    contractItemBL.updateRentalContractItemsAdditionalProductstoWaitingForDelivery(updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId,
                                                                                                   updateContractforRentalParametersSerialized.trackingNumber,
                                                                                                   updateContractforRentalParametersSerialized.totalAmount,
                                                                                                   items);
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    xrmHelper.setState("rnt_contract",
                                        updateContractforRentalParametersSerialized.contractInformation.contractId,
                                        (int)GlobalEnums.StateCode.Active,
                                        (int)rnt_contract_StatusCode.WaitingForDelivery);
                }
                else
                {
                    contractBL.updateContractRentalRelatedFields(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                 updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId,
                                                                 updateContractforRentalParametersSerialized.contractInformation.manuelDropoffTimeStamp,
                                                                 updateContractforRentalParametersSerialized.contractInformation.isManuelProcess,
                                                                 updateContractforRentalParametersSerialized.contractDeptStatus);


                    contractEquipmentId = contractItemBL.updateContractItemsRentalRelatedFields(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                                updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId,
                                                                                                updateContractforRentalParametersSerialized.contractInformation.DropoffTimeStamp,
                                                                                                updateContractforRentalParametersSerialized.contractInformation.isManuelProcess,
                                                                                                updateContractforRentalParametersSerialized.trackingNumber,
                                                                                                updateContractforRentalParametersSerialized.totalAmount);


                }
                #endregion

                #region Update Contract Header Rental User
                contractBL.updateContractRentalUser(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                    updateContractforRentalParametersSerialized.userInformation.userId);
                #endregion

                //var equipments = contractItemRepository.getActiveContractEquipments(updateContractforRentalParametersSerialized.contractInformation.contractId, new string[] { });

                #region Clear Campaign if neccessary
                initializer.TraceMe("updateContractforRentalParametersSerialized.canUserStillHasCampaignBenefit : " + updateContractforRentalParametersSerialized.canUserStillHasCampaignBenefit);
                //initializer.TraceMe("equipments.Count : " + equipments.Count);
                if (!updateContractforRentalParametersSerialized.canUserStillHasCampaignBenefit)
                {
                    initializer.TraceMe("clearing campaign start");
                    Entity campaignContact = new Entity("rnt_contract");
                    campaignContact.Id = updateContractforRentalParametersSerialized.contractInformation.contractId;
                    campaignContact["rnt_campaignid"] = null;
                    initializer.Service.Update(campaignContact);

                    Entity campaignContractItem = new Entity("rnt_contractitem");
                    campaignContractItem.Id = contractEquipmentId;
                    campaignContractItem["rnt_campaignid"] = null;
                    initializer.Service.Update(campaignContractItem);
                    initializer.TraceMe("clearing campaign end");

                }

                #endregion

                #region Update Equipment
                initializer.TraceMe("updateEquipmentforRental start");
                EquipmentBL equipmentBL = new EquipmentBL(initializer.Service);
                equipmentBL.updateEquipmentforRental(updateContractforRentalParametersSerialized.equipmentInformation.equipmentId,
                                                     equipmentInventoryId,
                                                     currentEquipmentTransaction.GetAttributeValue<EntityReference>("rnt_equipmenttransactionid").Id,
                                                     updateContractforRentalParametersSerialized.userInformation.branchId,
                                                     updateContractforRentalParametersSerialized.equipmentInformation.currentKmValue,
                                                     updateContractforRentalParametersSerialized.equipmentInformation.currentFuelValue);
                initializer.TraceMe("updateEquipmentforRental end");
                #endregion

                #region Create Damages
                var damageParameters = new CreateDamageParameter
                {
                    contractId = updateContractforRentalParametersSerialized.contractInformation.contractId,
                    equipmentId = updateContractforRentalParametersSerialized.equipmentInformation.equipmentId,
                    damageData = updateContractforRentalParametersSerialized.damageData,
                    userInformation = updateContractforRentalParametersSerialized.userInformation
                };
                DamageBL damageBL = new DamageBL(initializer.Service);
                var response = damageBL.createDamages(damageParameters);
                if (!response.responseResult.result)
                {
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.responseResult.exceptionDetail);
                }
                #endregion

                var corpContract = contractHelper.checkMakePayment_CorporateContracts(contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value,
                                                                                      contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value);

                initializer.TraceMe("corpContract :" + corpContract);
                var paymentResponse = new ContractCreateResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
                };
                if (!corpContract)
                {
                    #region Get Contract Amounts
                    var latestContract = contractRepository.getContractById(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                        new string[] { "rnt_totalamount" });


                    contractHelper.updateItemsforRental(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                               contractEquipmentId,
                                                               updateContractforRentalParametersSerialized.equipmentInformation.isEquipmentChanged);
                    //araç değişikliği varsa fiyatı eksi fiyatı düş
                    //araç değişikliği yoksa eksi fiyatı düşme
                    var contractPaymentAmount = contractHelper.calculatePaymentAmount(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                      null,
                                                                                      null,
                                                                                      updateContractforRentalParametersSerialized.equipmentInformation.isEquipmentChanged);

                    initializer.TraceMe("contractPaymentAmount " + JsonConvert.SerializeObject(contractPaymentAmount));
                    #endregion

                    #region make payment
                    var vPosResponse = new VirtualPosResponse { ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess(), virtualPosId = 0 };

                    var creditCard = updateContractforRentalParametersSerialized.paymentInformation.creditCardData.FirstOrDefault();

                    //initializer.TraceMe("updateContractforRentalParametersSerialized.paymentInformation.creditCardData" + updateContractforRentalParametersSerialized.paymentInformation.creditCardData);

                    // if contract has no dept status try payment
                    // if has dept status update contract header with dept status
                    if (updateContractforRentalParametersSerialized.contractDeptStatus.Value == (int)ClassLibrary._Enums_1033.rnt_contract_rnt_deptstatus.CompletedWithoutDept)
                    {
                        if (contractPaymentAmount.contractAmountDifference > StaticHelper._one_ &&
                            updateContractforRentalParametersSerialized.paymentInformation.creditCardData.Count == 0)
                        {
                            initializer.TraceMe("checking amount");
                            throw new Exception(string.Format("Ödenecek tutar mevcut : {0}.Lütfen kart seçimi yapın.", contractPaymentAmount.contractAmountDifference));
                        }
                        if (contractPaymentAmount.contractAmountDifference > StaticHelper._one_ ||
                            contractPaymentAmount.contractAmountDifference < (-1 * StaticHelper._one_))
                        {
                            initializer.TraceMe("start payment operations");
                            paymentResponse = contractBL.makeContractPayment(contractPaymentAmount.contractAmountDifference,
                                                                                 decimal.Zero,
                                                                                 creditCard,
                                                                                 null,
                                                                                 null,
                                                                                 contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id,
                                                                                 contract.GetAttributeValue<EntityReference>("rnt_customerid").Id,
                                                                                 latestContract.Id,
                                                                                 Guid.Empty,
                                                                                 contract.GetAttributeValue<string>("rnt_pnrnumber"),
                                                                                 new PaymentStatus
                                                                                 {
                                                                                     isDepositPaid = true,
                                                                                     isReservationPaid = contractPaymentAmount.contractAmountDifference == decimal.Zero ? true : false,
                                                                                 },
                                                                                 updateContractforRentalParametersSerialized.langId,
                                                                                 vPosResponse.virtualPosId,
                                                                                 1,
                                                                                 rnt_PaymentChannelCode.TABLET,
                                                                                 updateContractforRentalParametersSerialized.paymentInformation.use3DSecure,
                                                                                 updateContractforRentalParametersSerialized.paymentInformation.callBackUrl);
                        }
                        if (!paymentResponse.ResponseResult.Result)
                        {
                            var str = "CustomPaymentError: ";
                            initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(str + paymentResponse.ResponseResult.ExceptionDetail);
                        }

                    }
                    #endregion
                }
                #region sending equipment type item to mongodb

                initializer.TraceMe("mongodb started");
                try
                {

                    initializer.TraceMe("mongodb Rental update start");
                    initializer.RetryMethod<MongoDBResponse>(() => contractHelper.updateContractItemInMongoDB(new ContractItemResponse
                    {
                        contractItemId = contractEquipmentId,
                        status = (int)rnt_contractitem_StatusCode.Completed
                    }), StaticHelper.retryCount, StaticHelper.retrySleep);
                    initializer.TraceMe("mongodb rental Rental end");

                }
                catch (Exception ex)
                {
                    //will think a logic
                    initializer.TraceMe("mongodb integration error : " + ex.Message);
                }
                initializer.TraceMe("mongodb end");

                #endregion
                updateContractforRentalParametersSerialized.equipmentInformation.equipmentInventoryData = null;
                updateContractforRentalParametersSerialized.transits = null;
                updateContractforRentalParametersSerialized.paymentInformation = null;
                initializer.TraceMe("param : " + JsonConvert.SerializeObject(updateContractforRentalParametersSerialized));
                initializer.PluginContext.OutputParameters["UpdateContractforRentalResponse"] = JsonConvert.SerializeObject(paymentResponse.ResponseResult);

            }
            catch (Exception ex)
            {
                initializer.TraceMe("param : " + JsonConvert.SerializeObject(updateContractforRentalParametersSerialized));
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
