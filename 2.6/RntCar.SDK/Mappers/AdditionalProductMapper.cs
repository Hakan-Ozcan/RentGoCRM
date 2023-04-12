using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Tablet;
using RntCar.ClassLibrary._Web;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.SDK.Mappers
{
    public class AdditionalProductMapper
    {
        public List<ReservationAdditionalProductParameters> buildReservationAdditionalProducts(List<AdditionalProductData_Web> reservationAdditionalProducts, int totalDuration)
        {
            return reservationAdditionalProducts.ConvertAll(p => new ReservationAdditionalProductParameters
            {
                actualAmount = p.actualAmount,
                maxPieces = p.maxPieces,
                productCode = p.productCode,
                productId = p.productId,
                productName = p.productName,
                monthlyPackagePrice = p.monthlyPackagePrice,
                actualTotalAmount = p.actualAmount,
                productType = p.productType,
                priceCalculationType = p.priceCalculationType,
                totalDuration = totalDuration,
                value = p.value
            });
        }

        public List<ReservationAdditionalProductParameters> buildReservationAdditionalProducts(List<AdditionalProductData_Mobile> reservationAdditionalProducts, int totalDuration)
        {
            return reservationAdditionalProducts.ConvertAll(p => new ReservationAdditionalProductParameters
            {
                actualAmount = p.actualAmount,
                maxPieces = p.maxPieces,
                productCode = p.productCode,
                productId = p.productId,
                productName = p.productName,
                monthlyPackagePrice = p.monthlyPackagePrice,
                actualTotalAmount = p.actualAmount,
                productType = p.productType,
                priceCalculationType = p.priceCalculationType,
                totalDuration = totalDuration,
                value = p.value
            });
        }
        public List<ReservationAdditionalProductParameters> buildReservationAdditionalProducts(List<AdditionalProductData> reservationAdditionalProducts, List<ReservationAdditionalProduct_Broker> additionalProductParameters, int totalDuration, ReservationPriceParameter_Broker priceParameters)
        {
            List<ReservationAdditionalProductParameters> list = new List<ReservationAdditionalProductParameters>();

            reservationAdditionalProducts.ForEach(p =>
            {
                var value = additionalProductParameters.Where(i => i.productId == p.productId).Select(i => i.value).FirstOrDefault();
                var billingType = additionalProductParameters.Where(i => i.productId == p.productId).Select(i => i.billingType).FirstOrDefault();

                list.Add(new ReservationAdditionalProductParameters
                {
                    productId = p.productId,
                    actualAmount = p.actualAmount,
                    maxPieces = p.maxPieces,
                    monthlyPackagePrice = p.monthlyPackagePrice,
                    priceCalculationType = p.priceCalculationType,
                    productCode = p.productCode,
                    productName = p.productName,
                    productType = p.productType,
                    totalDuration = totalDuration,
                    billingType = CommonHelper.decideBillingType((int)rnt_reservationitem_rnt_itemtypecode.AdditionalProduct, Convert.ToString(priceParameters.paymentMethodCode), billingType).Value,
                    value = value
                });
            });

            return list;
        }
        public List<AdditionalProductData_Web> buildAdditionalProductsforWeb(List<AdditionalProductData> AdditionalProducts)
        {
            var web_AdditionalProducts = AdditionalProducts.Where(p => p.showonWebsite == true && p.productType != (int)rnt_additionalproduct_rnt_additionalproducttype.Fine).ToList();
            List<AdditionalProductData_Web> webAdditonalProducts = new List<AdditionalProductData_Web>();
            web_AdditionalProducts.ForEach(p => webAdditonalProducts.Add(new AdditionalProductData_Web().Map(p)));

            //mandatory ek ürünlerin value ve tobePaidAmount alanlarını güncelle
            foreach (var item in webAdditonalProducts.Where(p => p.isMandatory.Equals(true)).ToList())
            {
                item.tobePaidAmount = item.actualAmount.Value;
                item.value = 1;
            }


            return webAdditonalProducts;
        }

        public AdditionalProductData_Web buildAdditionalProductforWeb(AdditionalProductData additionalProduct)
        {
            AdditionalProductData_Web webAdditonalProduct = new AdditionalProductData_Web();
            var data = new AdditionalProductData_Web().Map(additionalProduct);
            return data;

        }

        public List<AdditionalProductData_Mobile> buildAdditionalProductsforMobile(List<AdditionalProductData> AdditionalProducts)
        {
            var mobile_AdditionalProducts = AdditionalProducts.Where(p => p.showonWebsite == true && p.productType != (int)rnt_additionalproduct_rnt_additionalproducttype.Fine).ToList();
            List<AdditionalProductData_Mobile> mobileAdditonalProducts = new List<AdditionalProductData_Mobile>();
            mobile_AdditionalProducts.ForEach(p => mobileAdditonalProducts.Add(new AdditionalProductData_Mobile().Map(p)));

            //mandatory ek ürünlerin value ve tobePaidAmount alanlarını güncelle
            foreach (var item in mobileAdditonalProducts.Where(p => p.isMandatory.Equals(true)).ToList())
            {
                item.tobePaidAmount = item.actualAmount.Value;
                item.value = 1;
            }

            return mobileAdditonalProducts;
        }

        public AdditionalProductData_Mobile buildAdditionalProductforMobile(AdditionalProductData additionalProduct)
        {
            AdditionalProductData_Mobile moibleAdditonalProduct = new AdditionalProductData_Mobile();
            var data = new AdditionalProductData_Mobile().Map(additionalProduct);
            return data;

        }

        public List<AdditionalProductData_Broker> buildAdditionalProductsforBroker(List<AdditionalProductData> AdditionalProducts)
        {
            var broker_AdditionalProducts = AdditionalProducts.Where(p => p.showonWebsite == true && p.productType != (int)rnt_additionalproduct_rnt_additionalproducttype.Fine).ToList();
            List<AdditionalProductData_Broker> brokerAdditonalProducts = new List<AdditionalProductData_Broker>();
            broker_AdditionalProducts.ForEach(p => brokerAdditonalProducts.Add(new AdditionalProductData_Broker().Map(p)));

            //mandatory ek ürünlerin value ve tobePaidAmount alanlarını güncelle
            foreach (var item in brokerAdditonalProducts.Where(p => p.isMandatory.Equals(true)).ToList())
            {
                item.tobePaidAmount = item.actualAmount.Value;
                item.value = 1;
            }

            return brokerAdditonalProducts;
        }

        public RntCar.ClassLibrary.Reservation.ReservationDateandBranchParameters buildAdditionalProductDateandBranchNeccessaryParameters(AdditionalProductParameters_Web additionalProductParameters_Web)
        {
            return new ClassLibrary.Reservation.ReservationDateandBranchParameters
            {
                dropoffBranchId = additionalProductParameters_Web.queryParameters.dropoffBranchId,
                pickupBranchId = additionalProductParameters_Web.queryParameters.pickupBranchId,
                dropoffDate = additionalProductParameters_Web.queryParameters.dropoffDateTime,
                pickupDate = additionalProductParameters_Web.queryParameters.pickupDateTime
            };
        }

        public RntCar.ClassLibrary.Reservation.ReservationDateandBranchParameters buildAdditionalProductDateandBranchNeccessaryParameters(AdditionalProductParameters_Mobile additionalProductParameters_mobile)
        {
            return new ClassLibrary.Reservation.ReservationDateandBranchParameters
            {
                dropoffBranchId = additionalProductParameters_mobile.queryParameters.dropoffBranchId,
                pickupBranchId = additionalProductParameters_mobile.queryParameters.pickupBranchId,
                dropoffDate = additionalProductParameters_mobile.queryParameters.dropoffDateTime,
                pickupDate = additionalProductParameters_mobile.queryParameters.pickupDateTime
            };
        }

        public RntCar.ClassLibrary.Reservation.ReservationDateandBranchParameters buildAdditionalProductDateandBranchNeccessaryParameters(AdditionalProductParameters_Broker additionalProductParameters_Broker)
        {
            return new ClassLibrary.Reservation.ReservationDateandBranchParameters
            {
                dropoffBranchId = additionalProductParameters_Broker.queryParameters.dropoffBranchId,
                pickupBranchId = additionalProductParameters_Broker.queryParameters.pickupBranchId,
                dropoffDate = additionalProductParameters_Broker.queryParameters.dropoffDateTime,
                pickupDate = additionalProductParameters_Broker.queryParameters.pickupDateTime
            };
        }

        public IndividualCustomerDetailData buildAdditionalProductIndividualNeccessaryParameters(DateTime birthDate, DateTime drivingLicenseDate)
        {
            return new IndividualCustomerDetailData
            {
                birthDate = birthDate,
                drivingLicenseDate = drivingLicenseDate
            };
        }

        public GroupCodeInformationCRMData buildAdditionalProductGroupCodeInformationNeccessaryParameters(int youngDriverAge,
                                                                                                          int minimumAge,
                                                                                                          int youngDriverMinimumLicense,
                                                                                                          int minimumLicense)
        {
            return new GroupCodeInformationCRMData
            {
                rnt_youngdriverage = youngDriverAge,
                rnt_minimumage = minimumAge,
                rnt_youngdriverlicence = youngDriverMinimumLicense,
                rnt_minimumdriverlicence = minimumLicense
            };
        }
        public AdditionalProductData buildAdditionalProductDataWithFixedPrice(Entity additionalProduct)
        {
            return new AdditionalProductData
            {

                productId = additionalProduct.Id,
                productName = additionalProduct.Attributes.Contains("rnt_name") ? additionalProduct.GetAttributeValue<string>("rnt_name") : string.Empty,
                productType = additionalProduct.Attributes.Contains("rnt_additionalproducttype") ?
                                additionalProduct.GetAttributeValue<OptionSetValue>("rnt_additionalproducttype").Value :
                                0,
                productCode = additionalProduct.Attributes.Contains("rnt_additionalproductcode") ? additionalProduct.GetAttributeValue<string>("rnt_additionalproductcode") : string.Empty,
                maxPieces = additionalProduct.Attributes.Contains("rnt_maximumpieces") ? additionalProduct.GetAttributeValue<int>("rnt_maximumpieces") : 0,
                showonWeb = additionalProduct.Attributes.Contains("rnt_showonweb") ? additionalProduct.GetAttributeValue<bool>("rnt_showonweb") : false,
                showonWebsite = additionalProduct.Attributes.Contains("rnt_showonwebsite") ? additionalProduct.GetAttributeValue<bool>("rnt_showonwebsite") : false,
                webRank = additionalProduct.Attributes.Contains("rnt_webrank") ?
                            additionalProduct.GetAttributeValue<int>("rnt_webrank") : 0,
                productDescription = additionalProduct.Attributes.Contains("rnt_productdescription") ? additionalProduct.GetAttributeValue<string>("rnt_productdescription") : string.Empty,
                isChecked = true,
                value = 1,
                priceCalculationType = additionalProduct.Attributes.Contains("rnt_pricecalculationtypecode") ?
                                         additionalProduct.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value :
                                         0,
                actualAmount = additionalProduct.Attributes.Contains("rnt_price") ? additionalProduct.GetAttributeValue<Money>("rnt_price").Value : 0,
                actualTotalAmount = additionalProduct.Attributes.Contains("rnt_price") ? additionalProduct.GetAttributeValue<Money>("rnt_price").Value : 0,
                tobePaidAmount = additionalProduct.Attributes.Contains("rnt_price") ? additionalProduct.GetAttributeValue<Money>("rnt_price").Value : 0
            };

        }

        public ContractUpdateParameters buildUpdateAdditionalProductsForContractParameter(Entity contract,
                                                                                          UpdateContractforDeliveryParameters updateContractforDeliveryParameters,
                                                                                          bool isCarChanged,
                                                                                          bool isDateorBranchChanged,
                                                                                          int changedReason,
                                                                                          int totalDuration)
        {
            return this.buildUpdateContractParameter(contract,
                                                     updateContractforDeliveryParameters.contractInformation.contractId,
                                                     updateContractforDeliveryParameters.contractInformation.contactId,
                                                     isCarChanged,
                                                     isDateorBranchChanged,
                                                     updateContractforDeliveryParameters.contractInformation.PickupDateTimeStamp,
                                                     updateContractforDeliveryParameters.contractInformation.DropoffTimeStamp,
                                                     changedReason,
                                                     updateContractforDeliveryParameters.changedEquipmentData != null ?
                                                     updateContractforDeliveryParameters.changedEquipmentData.groupCodeId :
                                                     contract.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id,
                                                     updateContractforDeliveryParameters.additionalProducts,
                                                     totalDuration);
        }
        public ContractUpdateParameters buildUpdateAdditionalProductsForContractParameter(Entity contract,
                                                                                          UpdateContractforRentalParameters updateContractforRentalParameters,
                                                                                          bool isCarChanged,
                                                                                          bool isDateorBranchChanged,
                                                                                          int changedReason,
                                                                                          int totalDuration)
        {
            return this.buildUpdateContractParameter(contract,
                                                     updateContractforRentalParameters.contractInformation.contractId,
                                                     updateContractforRentalParameters.contractInformation.contactId,
                                                     isCarChanged,
                                                     isDateorBranchChanged,
                                                     updateContractforRentalParameters.contractInformation.PickupDateTimeStamp,
                                                     updateContractforRentalParameters.contractInformation.DropoffTimeStamp,
                                                     changedReason,
                                                     contract.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id,
                                                     updateContractforRentalParameters.additionalProducts,
                                                     totalDuration);
        }

        public ContractUpdateAdditionalProductParameters convertTabletAdditionalProductDataToContractAdditonalProductData(List<AdditonalProductDataTablet> additonalProductDataTablets)
        {
            return new ContractUpdateAdditionalProductParameters
            {
                selectedAdditionalProductsData = additonalProductDataTablets.ConvertAll(p => new ContractAdditionalProductParameters().Map(p))
            };
        }
        public AdditionalProductData buildAdditionalProductData(Entity item, decimal actualTotalAmount, decimal monthlyPrice)
        {
            return new AdditionalProductData
            {
                billingType = item.GetAttributeValue<OptionSetValue>("rnt_billingtype")?.Value,
                productId = item.Id,
                productName = item.GetAttributeValue<string>("rnt_name"),
                productType = item.GetAttributeValue<OptionSetValue>("rnt_additionalproducttype").Value,
                productCode = item.GetAttributeValue<string>("rnt_additionalproductcode"),
                maxPieces = item.Attributes.Contains("rnt_maximumpieces") == true ? item.GetAttributeValue<int>("rnt_maximumpieces") : 0,
                showonWeb = item.GetAttributeValue<bool>("rnt_showonweb"),
                showonWebsite = item.Attributes.Contains("rnt_showonwebsite") ? item.GetAttributeValue<bool>("rnt_showonwebsite") : false,
                webRank = item.Attributes.Contains("rnt_webrank") == true ? item.GetAttributeValue<int>("rnt_webrank") : 0,
                productDescription = item.Attributes.Contains("rnt_productdescription") == true ? item.GetAttributeValue<string>("rnt_productdescription") : null,
                actualAmount = item.Attributes.Contains("rnt_price") ?
                               item.GetAttributeValue<Money>("rnt_price").Value :
                               decimal.Zero,
                actualTotalAmount = actualTotalAmount,
                isChecked = false,
                value = 0,
                priceCalculationType = item.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value,
                isMandatory = false,
                monthlyPackagePrice = monthlyPrice,
                webIconURL = item.GetAttributeValue<string>("rnt_webiconurl"),
                showOnContractUpdate = item.Attributes.Contains("rnt_showoncontractupdate") ? item.GetAttributeValue<bool>("rnt_showoncontractupdate") : false,
                showOnContractUpdateForMonthly = item.Attributes.Contains("rnt_showoncontractupdateformonthly") ? item.GetAttributeValue<bool>("rnt_showoncontractupdateformonthly") : false

            };
        }
        public AdditionalProductData buildAdditionalProductDatafromRelation(Entity item, decimal actualAmount, decimal actualTotalAmount, decimal monthlyPrice, bool isContract = false)
        {
            string billingTypeAttribute = "rnt_billingtypecode";
            if (isContract)
                billingTypeAttribute = "rnt_billingtype";

            return new AdditionalProductData
            {
                monthlyPackagePrice = monthlyPrice,
                billingType = item.Attributes.Contains(billingTypeAttribute) ? item.GetAttributeValue<OptionSetValue>(billingTypeAttribute).Value : 0,
                productId = item.Attributes.Contains("additionalProducts.rnt_additionalproductid") ? ((Guid)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_additionalproductid").Value) : Guid.Empty,
                productName = item.Attributes.Contains("additionalProducts.rnt_name") ? ((string)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_name").Value) : string.Empty,
                productType = item.Attributes.Contains("additionalProducts.rnt_additionalproducttype") ? ((OptionSetValue)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_additionalproducttype").Value).Value : 0,
                productCode = item.Attributes.Contains("additionalProducts.rnt_additionalproductcode") ? ((string)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_additionalproductcode").Value) : string.Empty,
                maxPieces = item.Attributes.Contains("additionalProducts.rnt_maximumpieces") ? ((int)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_maximumpieces").Value) : 0,
                showonWeb = item.Attributes.Contains("additionalProducts.rnt_showonweb") ? ((bool)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_showonweb").Value) : false,
                showonWebsite = item.Attributes.Contains("additionalProducts.rnt_showonwebsite") ? ((bool)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_showonwebsite").Value) : false,
                webRank = item.Attributes.Contains("additionalProducts.rnt_webrank") ? ((int)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_webrank").Value) : 0,
                productDescription = item.Attributes.Contains("additionalProducts.rnt_productdescription") ? ((string)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_productdescription").Value) : string.Empty,
                actualAmount = actualAmount,
                actualTotalAmount = actualTotalAmount,
                isChecked = true,
                value = 1,
                paidAmount = item.Attributes.Contains("rnt_totalamount") ? item.GetAttributeValue<Money>("rnt_totalamount").Value : 0,
                tobePaidAmount = item.Attributes.Contains("rnt_totalamount") ? (actualTotalAmount - item.GetAttributeValue<Money>("rnt_totalamount").Value) : 0,
                priceCalculationType = !item.Attributes.Contains("additionalProducts.rnt_pricecalculationtypecode") ? 1 : ((OptionSetValue)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_pricecalculationtypecode").Value).Value,
            };
        }

        public AdditionalProductData buildAdditionalProductDataforQuickContract(Entity item, decimal actualTotalAmount, int typeCode)
        {
            return new AdditionalProductData
            {
                productId = item.Attributes.Contains("rnt_additionalproductid") ? item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id : item.Id, // set item id to product id for initialize from request guid list
                productName = item.Attributes.Contains("rnt_additionalproductid") ? item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Name : string.Empty,
                actualTotalAmount = actualTotalAmount,
                isChecked = typeCode == (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.AdditionalProduct ? true : false,
                value = 1,
                maxPieces = 1,
                priceCalculationType = !item.Attributes.Contains("rnt_pricecalculationtypecode") ? 1 : item.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value,

            };
        }

        public AdditonalProductDataTablet createTabletModel(AdditionalProductData additionalProductData)
        {
            return new AdditonalProductDataTablet
            {
                billingType = additionalProductData.billingType,
                actualAmount = additionalProductData.actualAmount,
                actualTotalAmount = additionalProductData.actualTotalAmount,
                tobePaidAmount = additionalProductData.tobePaidAmount,
                isChecked = additionalProductData.isChecked,
                isMandatory = additionalProductData.isMandatory,
                showonWeb = additionalProductData.showonWeb,
                maxPieces = additionalProductData.maxPieces,
                productCode = additionalProductData.productCode,
                productDescription = additionalProductData.productDescription,
                productId = additionalProductData.productId,
                productName = additionalProductData.productName,
                productType = additionalProductData.productType,
                value = additionalProductData.value,
                webRank = additionalProductData.webRank,
                priceCalculationType = additionalProductData.priceCalculationType,
                monthlyPackagePrice = additionalProductData.monthlyPackagePrice
            };
        }
        private ContractUpdateParameters buildUpdateContractParameter(Entity contract,
                                                                      Guid _contractId,
                                                                      Guid _contactId,
                                                                      bool _isCarChanged,
                                                                      bool _isDateorBranchChanged,
                                                                      long _pickupDateTimeStamp,
                                                                      long _dropoffDateTimeStamp,
                                                                      int _changedReason,
                                                                      Guid _groupCodeInformationId,
                                                                      List<AdditonalProductDataTablet> additionalProducts,
                                                                      int totalDuration)
        {

            return new ContractUpdateParameters
            {
                contractId = _contractId,
                contactId = _contactId,
                isCarChanged = _isCarChanged,
                isDateorBranchChanged = _isDateorBranchChanged,
                dateAndBranch = new ContractDateandBranchParameters
                {
                    dropoffBranchId = contract.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id,
                    pickupBranchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                    dropoffDate = _dropoffDateTimeStamp.converttoDateTime(),
                    pickupDate = _pickupDateTimeStamp.converttoDateTime()
                },
                changedReason = _changedReason,
                contractStatusCode = contract.GetAttributeValue<OptionSetValue>("statuscode").Value,
                contractItemStatusCode = ContractMapper.getContractItemStatusCodeByContractStatusCode(contract.GetAttributeValue<OptionSetValue>("statuscode").Value),
                currency = contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id,
                customerType = contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value,
                totalDuration = totalDuration,
                groupCodeInformationId = _groupCodeInformationId,
                additionalProduct = this.convertTabletAdditionalProductDataToContractAdditonalProductData(additionalProducts)

            };
        }
    }
}
