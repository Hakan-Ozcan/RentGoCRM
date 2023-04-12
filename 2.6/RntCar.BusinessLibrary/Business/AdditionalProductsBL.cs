using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class AdditionalProductsBL : BusinessHandler
    {
        public AdditionalProductsBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public AdditionalProductsBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public AdditionalProductsBL()
        {
        }
        public AdditionalProductsBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public AdditionalProductsBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public AdditionalProductData getAdditionalProductMaturityDifference(decimal amount, string maturityProductCode)
        {
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var maturityAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(maturityProductCode);

            var data = new AdditionalProductMapper().buildAdditionalProductDataWithFixedPrice(maturityAdditionalProduct);
            data.actualAmount = amount;
            data.actualTotalAmount = amount;
            data.tobePaidAmount = amount;

            return data;
        }

        /// <summary>
        /// Get Additional Products for master data
        /// </summary>
        /// <param name="groupCodeData"></param>
        /// <param name="individualCustomerData"></param>
        /// <param name="selectedDateAndBranchData"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public AdditionalProductResponse GetAdditionalProducts(GroupCodeInformationCRMData groupCodeData,
                                                               IndividualCustomerDetailData individualCustomerData,
                                                               IDateAndBranch selectedDateAndBranchData,
                                                               int totalDuration,
                                                               Guid? currencyId = null)
        {
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService, this.CrmServiceClient);
            var additionalProducts = additionalProductRepository.GetAdditionalProductsWithCalculatedPrice(totalDuration, selectedDateAndBranchData.pickupBranchId);
            var res = this.additionalProductMandatoryOperations(additionalProducts, groupCodeData, individualCustomerData, selectedDateAndBranchData, totalDuration);
            if (!res.ResponseResult.Result)
            {
                return new AdditionalProductResponse
                {
                    ResponseResult = res.ResponseResult
                };
            }
            res.AdditionalProducts.ForEach(p => p.currencySymbol = StaticHelper.tlSymbol);
            if (currencyId != null)
            {
                calculateMultiCurrency(currencyId, additionalProducts, totalDuration);
            }
            return res;
        }

        public AdditionalProductResponse GetServiceAdditionalProducts(GroupCodeInformationCRMData groupCodeData,
                                                               IndividualCustomerDetailData individualCustomerData,
                                                               IDateAndBranch selectedDateAndBranchData,
                                                               int totalDuration,
                                                               Guid? currencyId = null)
        {
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService, this.CrmServiceClient);
            var additionalProducts = additionalProductRepository.GetServiceAdditionalProductsWithCalculatedPrice(totalDuration, selectedDateAndBranchData.pickupBranchId);
            var res = this.additionalProductMandatoryOperations(additionalProducts, groupCodeData, individualCustomerData, selectedDateAndBranchData, totalDuration);
            if (!res.ResponseResult.Result)
            {
                return new AdditionalProductResponse
                {
                    ResponseResult = res.ResponseResult
                };
            }
            res.AdditionalProducts.ForEach(p => p.currencySymbol = StaticHelper.tlSymbol);
            if (currencyId != null)
            {
                calculateMultiCurrency(currencyId, additionalProducts, totalDuration);
            }
            return res;
        }


        public void calculateMultiCurrency(Guid? currencyId, List<AdditionalProductData> additionalProducts, int totalDuration)
        {

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var turkishCurrency = configurationBL.GetConfigurationByName("currency_TRY");
            if (new Guid(turkishCurrency) != currencyId.Value)
            {
                var oneWayFeeCode = configurationBL.GetConfigurationByName("OneWayFeeCode");

                CurrencyRepository currencyRepository = new CurrencyRepository(this.OrgService);
                var symbol = currencyRepository.getCurrencyCode(currencyId.Value);
                AdditionalProductPriceRepository additionalProductPriceRepository = new AdditionalProductPriceRepository(this.OrgService);
                var prices = additionalProductPriceRepository.getAdditionalProductPrices(currencyId.Value);
                foreach (var item in prices)
                {
                    var product = additionalProducts.Where(p => p.productId == item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id).FirstOrDefault();
                    if (product != null)
                    {
                        var actualAmount = item.Attributes.Contains("rnt_price") ? item.GetAttributeValue<Money>("rnt_price").Value : decimal.Zero;
                        var actualTotalAmount = CommonHelper.calculateAdditionalProductPrice(product.priceCalculationType,
                                                                                             item.GetAttributeValue<Money>("rnt_monthlytotalprice").Value,
                                                                                             actualAmount,
                                                                                             totalDuration);
                        product.actualAmount = actualAmount;
                        product.actualTotalAmount = actualTotalAmount;
                        product.monthlyPackagePrice = item.GetAttributeValue<Money>("rnt_monthlytotalprice").Value;
                        product.currencySymbol = symbol;
                    }
                }
                //tek yön işlemleri
                var onewayFeeProduct = additionalProducts.Where(p => p.productCode == oneWayFeeCode).FirstOrDefault();
                if (onewayFeeProduct != null)
                {
                    var c = currencyRepository.getCurrencyById(currencyId.Value, new string[] { "exchangerate", "currencysymbol" });
                    var e = c.GetAttributeValue<decimal>("exchangerate");
                    onewayFeeProduct.actualAmount = onewayFeeProduct.actualAmount * e;
                    onewayFeeProduct.actualTotalAmount = onewayFeeProduct.actualTotalAmount * e;
                    onewayFeeProduct.monthlyPackagePrice = onewayFeeProduct.monthlyPackagePrice * e;
                    onewayFeeProduct.currencySymbol = c.GetAttributeValue<string>("currencysymbol");
                }
            }
        }

        public AdditionalProductResponse additionalProductMandatoryOperations(List<AdditionalProductData> additionalProducts,
                                                                              GroupCodeInformationCRMData groupCodeData,
                                                                              IndividualCustomerDetailData individualCustomerData,
                                                                              IDateAndBranch selectedDateAndBranchData,
                                                                              int totalDuration)
        {
            var customerAgeInDays = (selectedDateAndBranchData.pickupDate - individualCustomerData.birthDate).TotalDays;
            var customerLicenseInDays = (selectedDateAndBranchData.pickupDate - individualCustomerData.drivingLicenseDate).TotalDays;

            //first check reservation is in one way
            var youngDriverLicenseInDays = groupCodeData.rnt_youngdriverlicence * 365;
            var youngDriverAgeInDays = groupCodeData.rnt_youngdriverage * 365;
            var driverAgeInDays = groupCodeData.rnt_minimumage * 365;
            var minimumdriverLicense = groupCodeData.rnt_minimumdriverlicence * 365;

            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService, this.CrmServiceClient);
            var oneWayFeeCode = configurationRepository.GetConfigurationByKey("OneWayFeeCode");
            var youngDriverCode = configurationRepository.GetConfigurationByKey("additionalProduct_youngDriverCode");
            var smhgCode = configurationRepository.GetConfigurationByKey("additionalProduct_SMHG");
            var manuelDiscountCode = configurationRepository.GetConfigurationByKey("additionalProducts_manuelDiscount");
            //var maturityCode = configurationRepository.GetConfigurationByKey("additionalProduct_MaturityDifference");
            //#region maturity
            //if (additionalProducts.Where(p => p.productCode == maturityCode).FirstOrDefault() != null)
            //{
            //    additionalProducts.Where(p => p.productCode == maturityCode).FirstOrDefault().isChecked = true;
            //    additionalProducts.Where(p => p.productCode == maturityCode).FirstOrDefault().isMandatory = true;
            //}
            //#endregion

            #region Check Oneway fee
            if (selectedDateAndBranchData.pickupBranchId != selectedDateAndBranchData.dropoffBranchId)
            {
                this.Trace("one way fee" + groupCodeData.rnt_name);
                var res = this.checkReservationHasOneWayFee(selectedDateAndBranchData.pickupBranchId, selectedDateAndBranchData.dropoffBranchId);

                var product = additionalProducts.Where(x => x.productCode == oneWayFeeCode).FirstOrDefault();
                product.showonWeb = true; //SRV006 - Tek Yön
                product.showonWebsite = true;
                product.isChecked = true;
                product.isMandatory = true;
                if (product.priceCalculationType == 2)
                {
                    product.actualAmount = res;
                    product.actualTotalAmount = res;
                }
                else if (product.priceCalculationType == 1)
                {
                    product.actualAmount = res;
                    product.actualTotalAmount = res * totalDuration;
                }
                this.Trace("one way fee result" + res);
            }


            #endregion

            this.Trace("additionalProductMandatoryOperations : groupCodeData.rnt_minimumage : " + groupCodeData.rnt_minimumage);
            this.Trace("additionalProductMandatoryOperations : groupCodeData.rnt_minimumage * 365 : " + groupCodeData.rnt_minimumage * 365);

            //young driver control
            if (customerAgeInDays < youngDriverAgeInDays)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContent(this.UserId, "AgeControl", additionalProductXmlPath);
                return new AdditionalProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(message),
                    AdditionalProducts = null
                };
            }
            else if (customerLicenseInDays < youngDriverLicenseInDays)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContent(this.UserId, "DrivingLicenceYearControl", additionalProductXmlPath);
                return new AdditionalProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(message),
                    AdditionalProducts = null
                };
            }
            else if (customerAgeInDays < driverAgeInDays)
            {
                this.Trace("customer is young driver");

                additionalProducts.First(x => x.productCode == youngDriverCode).showonWeb = true; //SRV005 - Genç sürücü
                additionalProducts.First(x => x.productCode == youngDriverCode).showonWebsite = true;
                additionalProducts.First(x => x.productCode == youngDriverCode).isChecked = true;
                additionalProducts.First(x => x.productCode == youngDriverCode).isMandatory = true;

                additionalProducts.First(x => x.productCode == smhgCode).showonWeb = true; //SRV001 - SMHG
                additionalProducts.First(x => x.productCode == smhgCode).showonWebsite = true; //SRV001 - SMHG
                additionalProducts.First(x => x.productCode == smhgCode).isChecked = true;
                additionalProducts.First(x => x.productCode == smhgCode).isMandatory = true;

                return new AdditionalProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess(),
                    AdditionalProducts = additionalProducts
                };

            }
            else if (customerAgeInDays >= driverAgeInDays)
            {
                this.Trace("customerAge >= groupCodeData.rnt_minimumage");

                if (customerLicenseInDays <= youngDriverLicenseInDays)
                {
                    additionalProducts.First(x => x.productCode == youngDriverCode).showonWeb = true; //SRV005 - Genç sürücü
                    additionalProducts.First(x => x.productCode == youngDriverCode).showonWebsite = true; //SRV005 - Genç sürücü
                    additionalProducts.First(x => x.productCode == youngDriverCode).isChecked = true;
                    additionalProducts.First(x => x.productCode == youngDriverCode).isMandatory = true;

                    additionalProducts.First(x => x.productCode == smhgCode).showonWeb = true; //SRV001 - SMHG
                    additionalProducts.First(x => x.productCode == smhgCode).showonWebsite = true; //SRV001 - SMHG
                    additionalProducts.First(x => x.productCode == smhgCode).isChecked = true;
                    additionalProducts.First(x => x.productCode == smhgCode).isMandatory = true;

                    return new AdditionalProductResponse
                    {
                        ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess(),
                        AdditionalProducts = additionalProducts
                    };
                }
                else if (customerLicenseInDays >= youngDriverLicenseInDays && customerLicenseInDays <= minimumdriverLicense)
                {
                    additionalProducts.First(x => x.productCode == youngDriverCode).showonWeb = true; //SRV005 - Genç sürücü
                    additionalProducts.First(x => x.productCode == youngDriverCode).showonWebsite = true; //SRV005 - Genç sürücü
                    additionalProducts.First(x => x.productCode == youngDriverCode).isChecked = true;
                    additionalProducts.First(x => x.productCode == youngDriverCode).isMandatory = true;

                    additionalProducts.First(x => x.productCode == smhgCode).showonWeb = true; //SRV001 - SMHG
                    additionalProducts.First(x => x.productCode == smhgCode).showonWebsite = true; //SRV001 - SMHG
                    additionalProducts.First(x => x.productCode == smhgCode).isChecked = true;
                    additionalProducts.First(x => x.productCode == smhgCode).isMandatory = true;

                    return new AdditionalProductResponse
                    {
                        ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess(),
                        AdditionalProducts = additionalProducts
                    };
                }
                else
                {
                    return new AdditionalProductResponse
                    {
                        ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess(),
                        AdditionalProducts = additionalProducts
                    };
                }
            }
            else
            {
                return new AdditionalProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess(),
                    AdditionalProducts = additionalProducts
                };
            }
        }


        /// <summary>
        /// check and get young driver additional product for additional drivers
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="contractId"></param>
        /// <param name="pickupDateTime"></param>
        /// <param name="drivingLicenseDate"></param>
        /// <param name="birthDate"></param>
        /// <param name="totalDuration"></param>
        /// <returns></returns>
        public AdditionalProductResponse getYoungDriverProductByValidationsForAdditionalDrivers(Guid? reservationId,
                                                                                                Guid? contractId,
                                                                                                DateTime pickupDateTime,
                                                                                                DateTime drivingLicenseDate,
                                                                                                DateTime birthDate,
                                                                                                int totalDuration)
        {
            var groupCodeInfo = new GroupCodeInformationCRMData();
            // get group code info from reservation or contrat
            if (reservationId.HasValue)
            {
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                var reservation = reservationRepository.getReservationById(reservationId.Value, new string[] { "rnt_minimumage", "rnt_youngdriverage", "rnt_youngdriverlicence" });
                groupCodeInfo.rnt_youngdriverlicence = reservation.GetAttributeValue<int>("rnt_youngdriverlicence");
                groupCodeInfo.rnt_youngdriverage = reservation.GetAttributeValue<int>("rnt_youngdriverage");
                groupCodeInfo.rnt_minimumage = reservation.GetAttributeValue<int>("rnt_minimumage");
            }
            else if (contractId.HasValue)
            {
                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                var contract = contractRepository.getContractById(contractId.Value, new string[] { "new_minimumage", "new_youngdriverage", "new_youngdriverlicense" });
                groupCodeInfo.rnt_youngdriverlicence = contract.GetAttributeValue<int>("new_youngdriverlicense");
                groupCodeInfo.rnt_youngdriverage = contract.GetAttributeValue<int>("new_youngdriverage");
                groupCodeInfo.rnt_minimumage = contract.GetAttributeValue<int>("new_minimumage");
            }

            var customerAgeInDays = (pickupDateTime - birthDate).TotalDays;
            var customerLicenseInDays = (pickupDateTime - drivingLicenseDate).TotalDays;

            var youngDriverLicenseInDays = groupCodeInfo.rnt_youngdriverlicence * 365;
            var youngDriverAgeInDays = groupCodeInfo.rnt_youngdriverage * 365;
            var driverAgeInDays = groupCodeInfo.rnt_minimumage * 365;

            List<AdditionalProductData> products = new List<AdditionalProductData>();

            if (customerAgeInDays < youngDriverAgeInDays)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContent(this.UserId, "AgeControl", additionalProductXmlPath);
                return new AdditionalProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(message),
                    AdditionalProducts = null
                };
            }
            else if (customerAgeInDays >= youngDriverAgeInDays && customerAgeInDays < driverAgeInDays)
            {
                this.Trace("customer is young driver");

                if (customerLicenseInDays < youngDriverLicenseInDays)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContent(this.UserId, "DrivingLicenceYearControl", additionalProductXmlPath);
                    return new AdditionalProductResponse
                    {
                        ResponseResult = ClassLibrary.ResponseResult.ReturnError(message),
                        AdditionalProducts = null
                    };
                }
                else
                {
                    ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                    var productCode = configurationBL.GetConfigurationByName("additionalProduct_youngDriverCode");
                    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
                    var youngDriverProduct = additionalProductRepository.getYoungDriverProductForAdditionalDriversWithCalculatedPrice(totalDuration, productCode);
                    products.Add(youngDriverProduct);
                    return new AdditionalProductResponse
                    {
                        ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess(),
                        AdditionalProducts = products
                    };
                }
            }
            else if (customerAgeInDays >= driverAgeInDays)
            {
                this.Trace("customerAge >= groupCodeData.rnt_minimumage");

                if (customerLicenseInDays <= youngDriverLicenseInDays)
                {
                    ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                    var productCode = configurationBL.GetConfigurationByName("additionalProduct_youngDriverCode");
                    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
                    var youngDriverProduct = additionalProductRepository.getYoungDriverProductForAdditionalDriversWithCalculatedPrice(totalDuration, productCode);
                    products.Add(youngDriverProduct);

                    return new AdditionalProductResponse
                    {
                        ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess(),
                        AdditionalProducts = products
                    };
                }
                else
                {
                    return new AdditionalProductResponse
                    {
                        ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess(),
                        AdditionalProducts = null
                    };
                }
            }
            else
            {
                return new AdditionalProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess(),
                    AdditionalProducts = null
                };
            }
        }

        /// <summary>
        /// This method is used for price grid for calculations for update
        /// </summary>
        /// <param name="groupCodeData"></param>
        /// <param name="individualCustomerData"></param>
        /// <param name="selectedDateAndBranchData"></param>
        /// <param name="reservationId"></param>
        /// <param name="totalDuration"></param>
        /// <param name="langId"></param>
        /// <returns></returns>
        public AdditionalProductResponse GetAdditionalProductForUpdate(GroupCodeInformationCRMData groupCodeData,
                                                                       IndividualCustomerDetailData individualCustomerData,
                                                                       ReservationDateandBranchParameters selectedDateAndBranchData,
                                                                       string reservationId,
                                                                       int langId)
        {
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            try
            {
                var reservation = reservationRepository.getReservationById(new Guid(reservationId), new string[] { "transactioncurrencyid", "rnt_paymentchoicecode", "rnt_paidamount", "rnt_dropoffbranchid" });
                ContractHelper contractHelper = new ContractHelper(this.OrgService);
                var totalDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(selectedDateAndBranchData.pickupDate, selectedDateAndBranchData.dropoffDate);
                this.Trace("totalDuration : " + totalDuration);
                var additionalProducts = this.GetAdditionalProducts(groupCodeData,
                                                                    individualCustomerData,
                                                                    selectedDateAndBranchData,
                                                                    totalDuration,
                                                                    reservation.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);// get all additional products with validations
                this.Trace("result" + additionalProducts.ResponseResult.Result);
                if (additionalProducts.ResponseResult.Result)
                {
                    ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                    var youngDriverCode = configurationBL.GetConfigurationByName("additionalProduct_youngDriverCode");
                    var smhgCode = configurationBL.GetConfigurationByName("additionalProduct_SMHG");
                    var oneWayFeeCode = configurationBL.GetConfigurationByName("OneWayFeeCode");
                    var manuelDiscountCode = configurationBL.GetConfigurationByName("additionalProducts_manuelDiscount");
                    var maturityCode = configurationBL.GetConfigurationByName("additionalProduct_MaturityDifference");
                    //todo check from payments record in reservation or contract not from payment choice field.
                    //it can be tricky that it can be pay later reservation but has payment.
                    var paymentChoise = reservation.Attributes.Contains("rnt_paymentchoicecode") ? reservation.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value : 0;
                    var reservationPaidAmount = reservation.Attributes.Contains("rnt_paidamount") ? reservation.GetAttributeValue<Money>("rnt_paidamount").Value : 0;

                    var relatedAdditionalProducts = additionalProductRepository.GetAdditionalProductDataForUpdate(reservationId, totalDuration, reservationPaidAmount);//get related additional products with reservation
                    this.Trace("relatedAdditionalProducts: " + JsonConvert.SerializeObject(relatedAdditionalProducts));
                    relatedAdditionalProducts.ForEach(x =>
                    {
                        var item = additionalProducts.AdditionalProducts.Where(p => p.productId == x.productId).FirstOrDefault();
                        if (item != null)
                        {
                            if (x.billingType != null)
                                item.billingType = x.billingType;

                            if ((item.productCode == youngDriverCode && item.showonWeb == false) ||
                                (item.productCode == oneWayFeeCode && item.showonWeb == false) ||
                                (item.productCode == smhgCode && item.showonWeb == false))
                            {// if item is mandotory like genç sürücü and it's not in to additional products list(showonWeb == false) then calculate amounts and set ischecked true
                                item.isChecked = x.isChecked;
                                item.tobePaidAmount = x.tobePaidAmount;
                                item.showonWeb = true;
                                item.paidAmount = x.paidAmount;
                                item.actualTotalAmount = x.actualTotalAmount;
                            }
                            else if (item.productCode == oneWayFeeCode && item.showonWeb == true) // if item code SRV006 - tek yön don't calculate tobepaidamount
                            {
                                var price = checkDocumentOneWayFeeHasChanged(selectedDateAndBranchData.dropoffBranchId,
                                                                              reservation.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id,
                                                                              item.actualTotalAmount,
                                                                              x.actualTotalAmount);
                                item.isChecked = x.isChecked;
                                item.paidAmount = x.paidAmount;
                                item.tobePaidAmount = price - x.paidAmount;
                                item.actualTotalAmount = price;
                            }

                            else if (item.productCode == manuelDiscountCode)
                            {
                                item.actualAmount = x.actualTotalAmount;
                                item.actualTotalAmount = x.actualTotalAmount;
                                item.isChecked = x.isChecked;
                                item.paidAmount = x.paidAmount;
                            }
                            else if (item.productCode == maturityCode)
                            {
                                item.actualAmount = x.actualTotalAmount;
                                item.actualTotalAmount = x.actualTotalAmount;
                                item.isChecked = true;
                                item.paidAmount = x.paidAmount;
                                item.value = 1;
                                item.showonWeb = true;
                                item.isMandatory = true;
                            }
                            else
                            {
                                item.value = x.value;
                                item.isChecked = x.isChecked;
                                item.paidAmount = x.paidAmount;
                                item.tobePaidAmount = x.tobePaidAmount;
                                item.actualTotalAmount = x.actualTotalAmount;
                            }
                        }
                    });

                    // get mandotory additional products like genç sürücü
                    checkMandatoryAdditionalProducts(additionalProducts.AdditionalProducts, relatedAdditionalProducts, youngDriverCode, oneWayFeeCode, smhgCode, manuelDiscountCode, maturityCode);
                    
                    return new AdditionalProductResponse
                    {
                        AdditionalProducts = additionalProducts.AdditionalProducts,
                        totaltobePaidAmount = additionalProducts.AdditionalProducts.Sum(x => x.tobePaidAmount),
                        ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
                    };
                }

                this.Trace("operation end");
                return additionalProducts;
            }
            catch (Exception ex)
            {
                return new AdditionalProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        /// <summary>
        /// This method for getting additional products without any validation
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="totalDuration"></param>
        /// <param name="langId"></param>
        /// <returns></returns>
        public AdditionalProductResponse GetAdditionalProductForUpdateContract(string contractId, IDateAndBranch selectedDateAndBranchData, int totalDuration, int langId)
        {
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService, this.CrmServiceClient);
            ContractRepository contractRepository = new ContractRepository(this.OrgService, this.CrmServiceClient);
            try
            {
                var additionalProducts = additionalProductRepository.GetAdditionalProductsWithCalculatedPrice(totalDuration, selectedDateAndBranchData.pickupBranchId);// get all additional products with validations
                ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService, this.CrmServiceClient);
                var youngDriverCode = configurationBL.GetConfigurationByName("additionalProduct_youngDriverCode");
                var smhgCode = configurationBL.GetConfigurationByName("additionalProduct_SMHG");
                var oneWayFeeCode = configurationBL.GetConfigurationByName("OneWayFeeCode");
                var manuelDiscountCode = configurationBL.GetConfigurationByName("additionalProducts_manuelDiscount");
                var maturityCode = configurationBL.GetConfigurationByName("additionalProduct_MaturityDifference");
                #region Check Oneway fee
                if (selectedDateAndBranchData.pickupBranchId != selectedDateAndBranchData.dropoffBranchId)
                {
                    //TracingService.Trace("one way fee" + groupCodeData.rnt_name);

                    var res = this.checkReservationHasOneWayFee(selectedDateAndBranchData.pickupBranchId, selectedDateAndBranchData.dropoffBranchId);

                    var product = additionalProducts.Where(x => x.productCode == oneWayFeeCode).FirstOrDefault();
                    product.showonWeb = true; //SRV006 - Tek Yön
                    product.isChecked = true;
                    product.isMandatory = true;
                    if (product.priceCalculationType == 2)
                    {
                        product.actualAmount = res;
                        product.actualTotalAmount = res;
                    }
                    else if (product.priceCalculationType == 1)
                    {
                        product.actualAmount = res;
                        product.actualTotalAmount = res * totalDuration;
                    }
                    this.Trace("one way fee result" + res);
                }


                #endregion

                var relatedAdditionalProducts = additionalProductRepository.GetAdditionalProductDataForContract(contractId, selectedDateAndBranchData.dropoffDate);//get related additional products with reservation
                var contract = contractRepository.getContractById(new Guid(contractId), new string[] { "rnt_paymentchoicecode" });
                relatedAdditionalProducts.ForEach(x =>
                {
                    this.Trace("foreach x: " + x.productName + " " + x.billingType);
                    var item = additionalProducts.Where(p => p.productId == x.productId).FirstOrDefault();
                    if (item != null)
                    {
                        if (x.billingType != null)
                            item.billingType = x.billingType;

                        if ((item.productCode == youngDriverCode && item.showonWeb == false) || (item.productCode == smhgCode && item.showonWeb == false) || (item.productCode == oneWayFeeCode && item.showonWeb == false))
                        {// if item is mandotory like genç sürücü and it's not in to additional products list(showonWeb == false) then calculate amounts and set ischecked true
                            item.value = x.value;
                            item.isChecked = x.isChecked;
                            item.tobePaidAmount = x.tobePaidAmount;
                            item.showonWeb = true;
                            item.paidAmount = x.paidAmount;
                            item.actualTotalAmount = x.actualTotalAmount;
                        }
                        else if (item.productCode == oneWayFeeCode && item.showonWeb == true) // if item code SRV006 - tek yön don't calculate tobepaidamount
                        {
                            item.value = x.value;
                            item.isChecked = x.isChecked;
                            item.paidAmount = x.paidAmount;
                            item.tobePaidAmount = x.tobePaidAmount;
                            item.actualTotalAmount = x.actualTotalAmount;
                        }
                        else if (item.productCode == manuelDiscountCode)
                        {
                            item.actualAmount = x.actualTotalAmount;
                            item.actualTotalAmount = x.actualTotalAmount;
                            item.isChecked = x.isChecked;
                            item.paidAmount = x.paidAmount;
                        }
                        else
                        {
                            item.value = x.value;
                            item.isChecked = x.isChecked;
                            item.paidAmount = x.paidAmount;
                            item.tobePaidAmount = x.tobePaidAmount;
                            item.actualTotalAmount = x.actualTotalAmount;
                        }
                    }
                });
                checkMandatoryAdditionalProducts(additionalProducts, relatedAdditionalProducts, youngDriverCode, oneWayFeeCode, smhgCode, manuelDiscountCode, maturityCode);
               
                return new AdditionalProductResponse
                {
                    AdditionalProducts = additionalProducts,
                    totaltobePaidAmount = additionalProducts.Sum(x => x.tobePaidAmount),
                    ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new AdditionalProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        /// <summary>
        /// get additional products for tablet
        /// </summary>
        /// <param name="groupCodeData"></param>
        /// <param name="individualCustomerData"></param>
        /// <param name="selectedDateAndBranchData"></param>
        /// <param name="contractId"></param>
        /// <param name="totalDuration"></param>
        /// <param name="langId"></param>
        /// <returns></returns>
        public AdditionalProductResponse GetAdditionalProductForUpdateContract_tablet(GroupCodeInformationCRMData groupCodeData,
                                                                                       IndividualCustomerDetailData individualCustomerData,
                                                                                       ContractDateandBranchParameters selectedDateAndBranchData,
                                                                                       string contractId,
                                                                                       int totalDuration,
                                                                                       int langId)
        {
            //this method is for tablet service
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService, this.CrmServiceClient);
            ContractRepository contractRepository = new ContractRepository(this.OrgService, this.CrmServiceClient);
            try
            {
                var additionalProducts = this.GetAdditionalProducts(groupCodeData, individualCustomerData, selectedDateAndBranchData, totalDuration);// get all additional products with validations
                if (!additionalProducts.ResponseResult.Result)
                {
                    return new AdditionalProductResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(additionalProducts.ResponseResult.ExceptionDetail)
                    };
                }
                var relatedAdditionalProducts = additionalProductRepository.GetAdditionalProductDataForContract(contractId, selectedDateAndBranchData.dropoffDate);//get related additional products with reservation
                ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService, this.CrmServiceClient);
                var additionalDriverCode = configurationRepository.GetConfigurationByKey("additionalProducts_additionalDriver");
                var manuelDiscountCode = configurationRepository.GetConfigurationByKey("additionalProducts_manuelDiscount");
                var youngDriverCode = configurationRepository.GetConfigurationByKey("additionalProduct_youngDriverCode");
                var smhgCode = configurationRepository.GetConfigurationByKey("additionalProduct_SMHG");
                var onewayFeeCode = configurationRepository.GetConfigurationByKey("OneWayFeeCode");
                var maturityCode = configurationRepository.GetConfigurationByKey("additionalProduct_MaturityDifference");

                var reservation = contractRepository.getContractById(new Guid(contractId), new string[] { "rnt_paymentchoicecode" });
                //todo check from payments record in reservation or contract not from payment choice field.
                //it can be tricky that it can be pay later reservation but has payment.
                var paymentChoise = reservation.Attributes.Contains("rnt_paymentchoicecode") ? reservation.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value : 0;

                relatedAdditionalProducts.ForEach(x =>
                {
                    var item = additionalProducts.AdditionalProducts.Where(p => p.productId == x.productId).FirstOrDefault();
                    if (item != null)
                    {
                        if (x.billingType != null)
                            item.billingType = x.billingType;

                        if (paymentChoise == 20) //if payment choise is pay later then set total amount to tobepaid amount
                        {
                            item.value = x.value;
                            item.isChecked = x.isChecked;
                            item.paidAmount = 0;
                            item.tobePaidAmount = (item.productCode == youngDriverCode || item.productCode == smhgCode || item.productCode == onewayFeeCode) ? item.actualTotalAmount : x.actualTotalAmount;
                        }
                        else if ((item.productCode == youngDriverCode && item.showonWeb == false) || (item.productCode == smhgCode && item.showonWeb == false) || (item.productCode == onewayFeeCode && item.showonWeb == false))
                        {// if item is mandotory like genç sürücü and it's not in to additional products list(showonWeb == false) then calculate amounts and set ischecked true
                            item.isChecked = x.isChecked;
                            item.tobePaidAmount = (item.paidAmount - x.paidAmount);
                            item.paidAmount = x.paidAmount;
                        }
                        else if (item.productCode == onewayFeeCode && item.showonWeb == true) // if item code SRV006 - tek yön don't calculate tobepaidamount
                        {
                            item.isChecked = x.isChecked;
                            item.paidAmount = x.paidAmount;
                        }
                        else
                        {
                            item.value = x.value;
                            item.isChecked = x.isChecked;
                            item.paidAmount = x.paidAmount;
                            item.tobePaidAmount = x.tobePaidAmount;
                        }
                        item.actualAmount = x.actualAmount;
                        item.actualTotalAmount = x.actualTotalAmount;

                    }
                });

                // get mandotory additional products like genç sürücü
                checkMandatoryAdditionalProducts(additionalProducts.AdditionalProducts, relatedAdditionalProducts, youngDriverCode, onewayFeeCode, smhgCode, manuelDiscountCode, maturityCode);
                
                return new AdditionalProductResponse
                {
                    AdditionalProducts = additionalProducts.AdditionalProducts,
                    totaltobePaidAmount = additionalProducts.AdditionalProducts.Sum(x => x.tobePaidAmount),
                    ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new AdditionalProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        public AdditionalProductResponse GetServiceAdditionalProductForUpdateContract_tablet(GroupCodeInformationCRMData groupCodeData,
                                                                                    IndividualCustomerDetailData individualCustomerData,
                                                                                    ContractDateandBranchParameters selectedDateAndBranchData,
                                                                                    string contractId,
                                                                                    int totalDuration,
                                                                                    int langId)
        {
            //this method is for tablet service
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService, this.CrmServiceClient);
            ContractRepository contractRepository = new ContractRepository(this.OrgService, this.CrmServiceClient);
            try
            {
                var additionalProducts = this.GetServiceAdditionalProducts(groupCodeData, individualCustomerData, selectedDateAndBranchData, totalDuration);// get all additional products with validations
                if (!additionalProducts.ResponseResult.Result)
                {
                    return new AdditionalProductResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(additionalProducts.ResponseResult.ExceptionDetail)
                    };
                }
                var relatedAdditionalProducts = additionalProductRepository.GetAdditionalProductDataForContract(contractId, selectedDateAndBranchData.dropoffDate);//get related additional products with reservation
                ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService, this.CrmServiceClient);
                var additionalDriverCode = configurationRepository.GetConfigurationByKey("additionalProducts_additionalDriver");
                var manuelDiscountCode = configurationRepository.GetConfigurationByKey("additionalProducts_manuelDiscount");
                var youngDriverCode = configurationRepository.GetConfigurationByKey("additionalProduct_youngDriverCode");
                var smhgCode = configurationRepository.GetConfigurationByKey("additionalProduct_SMHG");
                var onewayFeeCode = configurationRepository.GetConfigurationByKey("OneWayFeeCode");
                var maturityCode = configurationRepository.GetConfigurationByKey("additionalProduct_MaturityDifference");

                var reservation = contractRepository.getContractById(new Guid(contractId), new string[] { "rnt_paymentchoicecode" });
                //todo check from payments record in reservation or contract not from payment choice field.
                //it can be tricky that it can be pay later reservation but has payment.
                var paymentChoise = reservation.Attributes.Contains("rnt_paymentchoicecode") ? reservation.GetAttributeValue<OptionSetValue>("rnt_paymentchoicecode").Value : 0;

                relatedAdditionalProducts.ForEach(x =>
                {
                    var item = additionalProducts.AdditionalProducts.Where(p => p.productId == x.productId).FirstOrDefault();
                    if (item != null)
                    {
                        if (x.billingType != null)
                            item.billingType = x.billingType;

                        if (paymentChoise == 20) //if payment choise is pay later then set total amount to tobepaid amount
                        {
                            item.value = x.value;
                            item.isChecked = x.isChecked;
                            item.paidAmount = 0;
                            item.tobePaidAmount = (item.productCode == youngDriverCode || item.productCode == smhgCode || item.productCode == onewayFeeCode) ? item.actualTotalAmount : x.actualTotalAmount;
                        }
                        else if ((item.productCode == youngDriverCode && item.showonWeb == false) || (item.productCode == smhgCode && item.showonWeb == false) || (item.productCode == onewayFeeCode && item.showonWeb == false))
                        {// if item is mandotory like genç sürücü and it's not in to additional products list(showonWeb == false) then calculate amounts and set ischecked true
                            item.isChecked = x.isChecked;
                            item.tobePaidAmount = (item.paidAmount - x.paidAmount);
                            item.paidAmount = x.paidAmount;
                        }
                        else if (item.productCode == onewayFeeCode && item.showonWeb == true) // if item code SRV006 - tek yön don't calculate tobepaidamount
                        {
                            item.isChecked = x.isChecked;
                            item.paidAmount = x.paidAmount;
                        }
                        else
                        {
                            item.value = x.value;
                            item.isChecked = x.isChecked;
                            item.paidAmount = x.paidAmount;
                            item.tobePaidAmount = x.tobePaidAmount;
                        }
                        item.actualAmount = x.actualAmount;
                        item.actualTotalAmount = x.actualTotalAmount;

                    }
                });

                // get mandotory additional products like genç sürücü
                checkMandatoryAdditionalProducts(additionalProducts.AdditionalProducts, relatedAdditionalProducts, youngDriverCode, onewayFeeCode, smhgCode, manuelDiscountCode, maturityCode);
                
                return new AdditionalProductResponse
                {
                    AdditionalProducts = additionalProducts.AdditionalProducts,
                    totaltobePaidAmount = additionalProducts.AdditionalProducts.Sum(x => x.tobePaidAmount),
                    ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new AdditionalProductResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        /// <summary>
        /// get additonal products for quick contract
        /// </summary>
        /// <param name="reservationId"></param>
        /// <param name="totalDuration"></param>
        /// <param name="reservationPaidAmount"></param>
        /// <returns></returns>
        public AdditionalProductResponse getReservationSelectedAdditionalProducts(string reservationId, decimal reservationPaidAmount)
        {
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var reservation = reservationRepository.getReservationById(new Guid(reservationId), new string[] { "rnt_pickupdatetime", "rnt_dropoffdatetime" });

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var response = additionalProductRepository.GetAdditionalProductDataForQuickContract(reservationId);

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var discountProductCode = configurationBL.GetConfigurationByName("additionalProducts_manuelDiscount");
            var discountAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(discountProductCode, new string[] { "rnt_additionalproductid" });
            var discountProduct = response.Where(item => item.productId == discountAdditionalProduct.Id).FirstOrDefault();
            if (discountProduct != null)
            {
                discountProduct.maxPieces = 0;
                discountProduct.productCode = discountProductCode;
            }

            return new AdditionalProductResponse
            {
                AdditionalProducts = response,
                ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
            };
        }


        public List<Guid> updateAdditionalProductsForContract(ContractUpdateParameters parameters,
                                                              List<Entity> contractItems,
                                                              Guid invoiceId,
                                                              int channelCode,
                                                              Guid? userId,
                                                              bool isUpdateContract = false)
        {
            List<Guid> createdItems = new List<Guid>();
            this.Trace("parameters.contractStatusCode" + parameters.contractStatusCode);
            this.Trace("isUpdateContract" + isUpdateContract);

            ContractItemBL contractItemBL = new ContractItemBL(this.OrgService, this.TracingService);
            InvoiceItemBL invoiceItemBL = new InvoiceItemBL(this.OrgService);
            var existingAdditionalProducts = new List<Entity>();
            existingAdditionalProducts.AddRange(contractItems.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)ContractItemEnums.ItemTypeCode.AdditionalProduct).ToList());
            //this.Trace("crm data " + JsonConvert.SerializeObject(existingAdditionalProducts));

            // todo will be added additional product controls and process
            foreach (var item in parameters.additionalProduct.selectedAdditionalProductsData)
            {
                var producttobeCheck = existingAdditionalProducts.Where(p => p.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id.Equals(item.productId)).ToList();
                //if producttobeCheck length == 0 , means we dont have this product before, so we need to add.
                //tobe careful if it is maxpieces == 1 or higher
                //because maxpieces > 1 different contract items in crm
                if (producttobeCheck.Count == 0)
                {
                    var t = parameters.totalDuration;

                    var dateandBranchParamter = new ContractDateandBranchParameters
                    {
                        pickupDate = parameters.dateAndBranch.pickupDate,
                        pickupBranchId = parameters.dateAndBranch.pickupBranchId,
                        dropoffDate = parameters.dateAndBranch.dropoffDate,
                        dropoffBranchId = parameters.dateAndBranch.dropoffBranchId
                    };
                    if (parameters.contractStatusCode == (int)ContractEnums.StatusCode.Rental)
                    {
                        // if contract status is rental, set pickup date with now
                        dateandBranchParamter.pickupDate = DateTime.UtcNow.AddMinutes(isUpdateContract ? 0 : StaticHelper.offset);

                        ContractHelper contractHelper = new ContractHelper(this.OrgService);
                        t = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(dateandBranchParamter.pickupDate,
                                                                                            parameters.dateAndBranch.dropoffDate);

                        this.Trace("totalDuration_add product : " + parameters.totalDuration);
                    }

                    createdItems.AddRange(contractItemBL.createContractItemForAdditionalProducts(dateandBranchParamter,
                                                                                                 item,
                                                                                                 parameters.contactId,
                                                                                                 parameters.groupCodeInformationId,
                                                                                                 parameters.contractId,
                                                                                                 parameters.currency,
                                                                                                 invoiceId,
                                                                                                 t,
                                                                                                 parameters.contractItemStatusCode,
                                                                                                 parameters.trackingNumber,
                                                                                                 channelCode,
                                                                                                 userId));
                }
                //if producttobeCheck length == 1 , means we found only one item in crm , create or update operation will decide by maxpieces = 1 or > 1
                else if (producttobeCheck.Count == 1)
                {
                    //this.Trace("producttobeCheck.Count == 1 " + JsonConvert.SerializeObject(item));
                    this.Trace("producttobeCheck.Count == 1");
                    //check its max pieces == 1
                    if (item.maxPieces == 1)
                    {
                        var _pickup = producttobeCheck.FirstOrDefault().GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset);
                        ContractHelper contractHelper = new ContractHelper(this.OrgService);
                        var t = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(_pickup,
                                                                                                parameters.dateAndBranch.dropoffDate);

                        var dateandBranchParamter = new ContractDateandBranchParameters
                        {
                            pickupDate = _pickup,
                            pickupBranchId = parameters.dateAndBranch.pickupBranchId,
                            dropoffDate = parameters.dateAndBranch.dropoffDate,
                            dropoffBranchId = parameters.dateAndBranch.dropoffBranchId
                        };

                        if (item.value == 1 && item.billingType.HasValue)
                        {
                            Entity contractItem = new Entity("rnt_contractitem");
                            contractItem["rnt_billingtype"] = new OptionSetValue((int)item.billingType.Value);
                            contractItem.Id = producttobeCheck.FirstOrDefault().Id;
                            this.OrgService.Update(contractItem);
                        }

                        //just update the reservation item with the new date fields.                        
                        contractItemBL.updateContractItemAdditionalProductAmountandDateBranchFields(producttobeCheck.FirstOrDefault().Id,
                                                                                                    dateandBranchParamter,
                                                                                                    item,
                                                                                                    parameters.contactId,
                                                                                                    parameters.groupCodeInformationId,
                                                                                                    parameters.contractId,
                                                                                                    parameters.currency,
                                                                                                    t,
                                                                                                    parameters.contractItemStatusCode,
                                                                                                    parameters.isDateorBranchChanged,
                                                                                                    parameters.trackingNumber);
                    }
                    else if (item.maxPieces > 1)
                    {
                        this.Trace("item.maxPieces > 1");
                        var _pickup = producttobeCheck.FirstOrDefault().GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset);
                        ContractHelper contractHelper = new ContractHelper(this.OrgService);
                        var t = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(_pickup,
                                                                                                parameters.dateAndBranch.dropoffDate);
                        //check max pieces added more 
                        //this scenerio is user didnt add any more item just update the date fields is enough
                        if (item.value == 1)
                        {
                            var dateandBranchParamter = new ContractDateandBranchParameters
                            {
                                pickupDate = _pickup,
                                pickupBranchId = parameters.dateAndBranch.pickupBranchId,
                                dropoffDate = parameters.dateAndBranch.dropoffDate,
                                dropoffBranchId = parameters.dateAndBranch.dropoffBranchId
                            };
                            //this.Trace("will update item : " + JsonConvert.SerializeObject(item));
                            //just update the reservation item with the new date fields.
                            //todo check the dates are changed.if not avoid from unneccessary update
                            contractItemBL.updateContractItemAdditionalProductAmountandDateBranchFields(producttobeCheck.FirstOrDefault().Id,
                                                                                                        dateandBranchParamter,
                                                                                                        item,
                                                                                                        parameters.contactId,
                                                                                                        parameters.groupCodeInformationId,
                                                                                                        parameters.contractId,
                                                                                                        parameters.currency,
                                                                                                        t,
                                                                                                        parameters.contractItemStatusCode,
                                                                                                        parameters.isDateorBranchChanged,
                                                                                                        parameters.trackingNumber);
                        }
                        else if (item.value > 1)
                        {

                            var dateandBranchParamter = new ContractDateandBranchParameters
                            {
                                pickupDate = parameters.dateAndBranch.pickupDate,
                                pickupBranchId = parameters.dateAndBranch.pickupBranchId,
                                dropoffDate = parameters.dateAndBranch.dropoffDate,
                                dropoffBranchId = parameters.dateAndBranch.dropoffBranchId
                            };
                            if (parameters.contractStatusCode == (int)ContractEnums.StatusCode.Rental)
                            {
                                // if contract status is rental, set pickup date with now
                                dateandBranchParamter.pickupDate = DateTime.Now.AddMinutes(isUpdateContract ? 0 : StaticHelper.offset);

                                t = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(dateandBranchParamter.pickupDate,
                                                                                                    parameters.dateAndBranch.dropoffDate);
                                this.Trace("totalDuration : " + parameters.totalDuration);
                            }
                            // we need to create just by the difference( -1 means removing existing reservationitem with the same productId type)
                            item.value -= 1;
                            this.Trace("item.name: " + item.productName);
                            this.Trace("item.billingType: " + item.billingType.ToString());
                            createdItems.AddRange(contractItemBL.createContractItemForAdditionalProducts(dateandBranchParamter,
                                                                                                         item,
                                                                                                         parameters.contactId,
                                                                                                         parameters.groupCodeInformationId,
                                                                                                         parameters.contractId,
                                                                                                         parameters.currency,
                                                                                                         invoiceId,
                                                                                                         t,
                                                                                                         parameters.contractItemStatusCode,
                                                                                                         parameters.trackingNumber,
                                                                                                         channelCode,
                                                                                                         userId));
                        }
                    }
                }
                //if we found more than one more , it is definetely maxpieces > 1 typeof additional product
                //just need to check the counts
                else if (producttobeCheck.Count > 1)
                {
                    // this.Trace("producttobeCheck.Count >  1 " + JsonConvert.SerializeObject(item));

                    if (producttobeCheck.Count == item.value)
                    {
                        this.Trace("producttobeCheck.Count == item.Value");

                        //nothing added just update contract items with new date fields
                        foreach (var sameProduct in producttobeCheck)
                        {
                            var _pickup = sameProduct.GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset);
                            ContractHelper contractHelper = new ContractHelper(this.OrgService);
                            var t = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(_pickup,
                                                                                                    parameters.dateAndBranch.dropoffDate);
                            var dateandBranchParamter = new ContractDateandBranchParameters
                            {
                                pickupDate = _pickup,
                                pickupBranchId = parameters.dateAndBranch.pickupBranchId,
                                dropoffDate = parameters.dateAndBranch.dropoffDate,
                                dropoffBranchId = parameters.dateAndBranch.dropoffBranchId
                            };
                            #region Update existing contract item with the new date fields
                            //just update the contract item with the new date fields.
                            //todo check the dates are changed.if not avoid from unneccessary update
                            contractItemBL.updateContractItemAdditionalProductAmountandDateBranchFields(sameProduct.Id,
                                                                                                        dateandBranchParamter,
                                                                                                        item,
                                                                                                        parameters.contactId,
                                                                                                        parameters.groupCodeInformationId,
                                                                                                        parameters.contractId,
                                                                                                        parameters.currency,
                                                                                                        t,
                                                                                                        parameters.contractItemStatusCode,
                                                                                                        parameters.isDateorBranchChanged,
                                                                                                        parameters.trackingNumber);

                            #endregion
                        }
                    }
                    //some products are removed so need to complete them
                    else if (producttobeCheck.Count > item.value)
                    {
                        this.Trace("producttobeCheck.Count > item.Value");

                        var diff = producttobeCheck.Count - item.value;
                        this.Trace("diff" + diff);
                        this.Trace("parameters.contractStatusCode" + parameters.contractStatusCode);

                        #region Need to complete the 
                        for (int j = 0; j < producttobeCheck.Count; j++)
                        {
                            var dateandBranchParamter = new ContractDateandBranchParameters
                            {
                                pickupDate = parameters.dateAndBranch.pickupDate,
                                pickupBranchId = parameters.dateAndBranch.pickupBranchId,
                                dropoffDate = parameters.dateAndBranch.dropoffDate,
                                dropoffBranchId = parameters.dateAndBranch.dropoffBranchId
                            };
                            if (j < diff)
                            {
                                dateandBranchParamter.dropoffDate = DateTime.UtcNow.AddMinutes(StaticHelper.offset);
                                //car is not delivered yet so need to deactivate
                                if (parameters.contractStatusCode == (int)ContractEnums.StatusCode.WaitingforDelivery)
                                {
                                    dateandBranchParamter.pickupDate = DateTime.UtcNow.AddMinutes(StaticHelper.offset);
                                    contractItemBL.updateContractItemChangeReasonandDateTime(producttobeCheck[j].Id, dateandBranchParamter, (int)rnt_ChangeReasonCode.CustomerDemand);
                                    //todo contractitemstatuschange enum                                  

                                    contractItemBL.deactiveContractItemItemById(producttobeCheck[j].Id, (int)rnt_contractitem_StatusCode.Inactive);
                                    //todo contractitemstatuschange enum
                                    //invoiceItemBL.deactiveInvoiceItem(producttobeCheck[j].Id, 2);
                                }
                                else if (parameters.contractStatusCode == (int)ContractEnums.StatusCode.Rental)
                                {
                                    // set drop off date with datetime now for removed additional products
                                    contractItemBL.updateContractItemStatusandDateTime(producttobeCheck[j].Id, dateandBranchParamter, (int)rnt_contractitem_StatusCode.Completed);
                                    //invoiceItemBL.deactiveInvoiceItem(producttobeCheck[j].Id, 2);
                                }
                            }
                            else
                            {
                                // update other items datetime
                                if (parameters.isDateorBranchChanged)
                                {
                                    contractItemBL.updateContractItemStatusandDateTime(producttobeCheck[j].Id, dateandBranchParamter, parameters.contractItemStatusCode);
                                }
                            }

                        }
                        #endregion
                    }
                    else if (producttobeCheck.Count < item.value)
                    {
                        this.Trace("producttobeCheck.Count < item.Value");

                        var dateandBranchParamter = new ContractDateandBranchParameters
                        {
                            pickupDate = parameters.dateAndBranch.pickupDate,
                            pickupBranchId = parameters.dateAndBranch.pickupBranchId,
                            dropoffDate = parameters.dateAndBranch.dropoffDate,
                            dropoffBranchId = parameters.dateAndBranch.dropoffBranchId
                        };
                        if (parameters.contractStatusCode == (int)ContractEnums.StatusCode.Rental)
                        {
                            // if contract status is rental, set pickup date with now
                            dateandBranchParamter.pickupDate = DateTime.Now.AddMinutes(StaticHelper.offset);
                        }

                        var diff = item.value - producttobeCheck.Count;
                        for (int i = 0; i < diff; i++)
                        {
                            this.Trace("createContractItemForAdditionalProduct");

                            #region Create Additional Product
                            createdItems.Add(contractItemBL.createContractItemForAdditionalProduct(dateandBranchParamter,
                                                                                  item,
                                                                                  parameters.contactId,
                                                                                  parameters.groupCodeInformationId,
                                                                                  parameters.contractId,
                                                                                  parameters.currency,
                                                                                  invoiceId,
                                                                                  parameters.totalDuration,
                                                                                  parameters.contractItemStatusCode,
                                                                                  parameters.trackingNumber,
                                                                                  channelCode,
                                                                                  userId, Guid.Empty));
                            #endregion
                        }
                    }
                }
                existingAdditionalProducts.RemoveAll(p => p.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id.Equals(item.productId));
            }

            //if we still some values on the list lets complete them
            foreach (var existingItem in existingAdditionalProducts)
            {
                //this.Trace("existingAdditionalProducts" + JsonConvert.SerializeObject(existingItem));

                var dateandBranchParamter = new ContractDateandBranchParameters
                {
                    pickupDate = parameters.dateAndBranch.pickupDate,
                    pickupBranchId = parameters.dateAndBranch.pickupBranchId,
                    dropoffDate = parameters.dateAndBranch.dropoffDate,
                    dropoffBranchId = parameters.dateAndBranch.dropoffBranchId
                };
                dateandBranchParamter.dropoffDate = DateTime.Now.AddMinutes(StaticHelper.offset);
                if (parameters.contractStatusCode == (int)ContractEnums.StatusCode.WaitingforDelivery)
                {
                    this.Trace("deactivating");
                    //todo contractitemstatuschange enum
                    contractItemBL.updateContractItemChangeReasonandDateTime(existingItem.Id, dateandBranchParamter, (int)rnt_ChangeReasonCode.CustomerDemand);
                    contractItemBL.deactiveContractItemItemById(existingItem.Id, (int)rnt_contractitem_StatusCode.Inactive);
                    this.Trace("deactivating end");
                }
                else
                {
                    contractItemBL.updateContractItemStatusandChangeReasonandDateTime(existingItem.Id, dateandBranchParamter, (int)rnt_contractitem_StatusCode.Completed, (int)rnt_ChangeReasonCode.CustomerDemand);
                }
                //invoiceItemBL.deactiveInvoiceItem(existingItem.Id, 2);
            }

            return createdItems;
        }


        /// <summary>
        /// This method only retrieve master data of the additional products.
        /// Any prices will not be calculated int this method
        /// </summary>
        /// <param name="oneWayFeeCode"></param>
        public AdditionalProductData createDummyAdditionalProductDataByGivenProductCode(string productCode, decimal amount)
        {
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService, this.CrmServiceClient);
            var additionalProduct = additionalProductRepository.getAdditionalProductByProductCode(productCode);
            return buildAdditionalProductData(additionalProduct, amount);
        }

        public AdditionalProductData buildAdditionalProductData(Entity additionalProduct, decimal amount)
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
                actualAmount = amount,
                actualTotalAmount = amount
            };

        }


        private decimal checkReservationHasOneWayFee(Guid pickupBranchId, Guid dropoffBranchId)
        {
            OneWayFeeRepository oneWayFeeRepository = new OneWayFeeRepository(this.OrgService, this.CrmServiceClient);
            var res = oneWayFeeRepository.getOneWayFeeByPickupandDropoffBranch(pickupBranchId, dropoffBranchId);
            if (res != null && res.Attributes.Contains("rnt_price"))
                return res.GetAttributeValue<Money>("rnt_price").Value;

            return decimal.Zero;
        }

        private decimal checkDocumentOneWayFeeHasChanged(Guid queryDropoffBranchId, Guid documentDropoffBranchId, decimal oneWayFee, decimal documentOneWayFee)
        {
            if (queryDropoffBranchId == documentDropoffBranchId)
            {
                return documentOneWayFee;
            }
            return oneWayFee;
        }

        private void checkMandatoryAdditionalProducts(List<AdditionalProductData> additionalProducts,
                                                      List<AdditionalProductData> relatedAdditionalProducts,
                                                      string youngDriverCode,
                                                      string oneWayFeeCode,
                                                      string smhgCode,
                                                      string manuelDiscountCode,
                                                      string maturityCode)
        {
            additionalProducts.Where(p => (p.productCode == youngDriverCode && p.showonWeb == true) ||
                                          (p.productCode == oneWayFeeCode && p.showonWeb == true) ||
                                          (p.productCode == manuelDiscountCode) ||
                                          (p.productCode == maturityCode))
                                          .ToList()
                                          .ForEach(x =>
                                           {
                                               var index = relatedAdditionalProducts.FindIndex(p => p.productId == x.productId);// search mandotory products in related products
                                               if (index <= -1)// if item is not in related products set tobePaidAmount 
                                               {
                                                   x.tobePaidAmount = x.actualTotalAmount;
                                               }
                                               if (x.productCode == youngDriverCode)//
                                               {
                                                   var i = relatedAdditionalProducts.FindIndex(p => p.productCode == smhgCode);
                                                   if (i <= -1)
                                                   {
                                                       //SMHG eğer sözleşmeye eklenmediyse
                                                       //İlk olarak üst paketler çekilir ve sözleşmede olup olmadığı kontrol edilir.
                                                       AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
                                                       var smhgId = additionalProductRepository.getAdditionalProductByProductCode(smhgCode).Id;
                                                       AdditionalProductRuleRepository additionalProductRuleRepository = new AdditionalProductRuleRepository(this.OrgService);
                                                       AdditionalProductData parentProduct = new AdditionalProductData();
                                                       EntityCollection parentAdditionalProductRuleList = additionalProductRuleRepository.getAdditonalProductRuleListByAdditionalProductId(smhgId);
                                                       
                                                       foreach (var parentAdditionalProductRule in parentAdditionalProductRuleList.Entities)
                                                       {
                                                           EntityReference parentProductRef = parentAdditionalProductRule.GetAttributeValue<EntityReference>("rnt_parentproduct");
                                                           parentProduct = relatedAdditionalProducts.Find(p => p.productId == parentProductRef.Id);
                                                           break;
                                                       }

                                                       if (parentProduct != null && parentProduct.productId != Guid.Empty)
                                                       {
                                                           //SMHG üst paketleri varsa, SMGH değerleri sıfrlanır.
                                                           additionalProducts.Where(p => p.productCode == smhgCode).FirstOrDefault().tobePaidAmount = 0;
                                                           additionalProducts.Where(p => p.productCode == smhgCode).FirstOrDefault().isMandatory = false;
                                                           additionalProducts.Where(p => p.productCode == smhgCode).FirstOrDefault().isChecked = false;

                                                           additionalProducts.Where(p => p.productId == parentProduct.productId).FirstOrDefault().isMandatory = true;
                                                           additionalProducts.Where(p => p.productId == parentProduct.productId).FirstOrDefault().isChecked = true;
                                                       }
                                                       else
                                                       {
                                                           //SMHG üst paketleri yoksa, SMGH 'da sözleşmede olmadığı için eklenir.
                                                           additionalProducts.Where(p => p.productCode == smhgCode).FirstOrDefault().tobePaidAmount =
                                                                additionalProducts.Where(p => p.productCode == smhgCode).FirstOrDefault().actualTotalAmount;
                                                           additionalProducts.Where(p => p.productCode == smhgCode).FirstOrDefault().isMandatory = true;
                                                           additionalProducts.Where(p => p.productCode == smhgCode).FirstOrDefault().isChecked = true;
                                                       }

                                                   }
                                               }
                                               if (x.productCode == manuelDiscountCode)
                                               {
                                                   x.isMandatory = true;
                                               }
                                               if (x.productCode == maturityCode)
                                               {
                                                   x.isMandatory = true;
                                               }
                                           });
        }

        //public AdditionalProductData checkAndFixMandotoryYoungDriverFromAdditionalProductLists(List<AdditionalProductData> additionalProductData)
        //{
        //    this.Trace("checkAndFixMandotoryYoungDriver additionalProductData : " + JsonConvert.SerializeObject(additionalProductData));
        //    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
        //    AdditionalProductRuleRepository additionalProductRuleRepository = new AdditionalProductRuleRepository(this.OrgService);
        //    ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService, this.CrmServiceClient);

        //    var youngDriverCode = configurationRepository.GetConfigurationByKey("additionalProduct_youngDriverCode");
        //    var smhgCode = configurationRepository.GetConfigurationByKey("additionalProduct_SMHG");
        //    var smhgId = additionalProductRepository.getAdditionalProductByProductCode(smhgCode).Id;

        //    var existsYoungDriver = additionalProductData.Where(p => p.productCode == youngDriverCode && p.isMandatory && p.isChecked).FirstOrDefault();
        //    var existsSMHG = additionalProductData.Where(p => p.productCode == smhgCode && p.isMandatory && p.isChecked).FirstOrDefault();
        //    this.Trace($"young driver : {JsonConvert.SerializeObject(existsYoungDriver)} \n  SMHG: {JsonConvert.SerializeObject(existsSMHG)}");

        //    if (existsYoungDriver != null && existsSMHG != null)
        //    {
        //        foreach (var item in additionalProductData)
        //        {
        //            if (additionalProductRuleRepository.getAdditonalProductRuleByAdditionalProductandParentProductId(item.productId, smhgId) != null && item.isChecked)
        //            {
        //                additionalProductData.Where(p => p.productCode == smhgCode).FirstOrDefault().isChecked = false;
        //            }
        //        }
        //    }
        //    return new AdditionalProductData();
        //}
    }
}
