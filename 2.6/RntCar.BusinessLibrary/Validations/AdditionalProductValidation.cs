using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Validations
{
    public class AdditionalProductValidation : ValidationHandler
    {
        public AdditionalProductValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public AdditionalProductValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public AdditionalProductResponse checkFindeks(Guid groupCodeId, Guid individualCustomerId, string pricingType, Guid? reservationId, string corporateCustomerId, string changeType, int langId)
        {

            if (!string.IsNullOrEmpty(corporateCustomerId) && corporateCustomerId != "00000000-0000-0000-0000-000000000000")
            {
                this.Trace("corporate checking");
                var parsedPricingType = 0;
                if (int.TryParse(pricingType, out parsedPricingType))
                {
                    if (parsedPricingType == (int)rnt_PaymentMethodCode.Current ||
                       parsedPricingType == (int)rnt_PaymentMethodCode.FullCredit)
                    {
                        this.Trace("current corp doesnt need validation");
                        return new AdditionalProductResponse
                        {
                            ResponseResult = ResponseResult.ReturnSuccess()
                        };
                    }
                }
                CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
                var corp = corporateCustomerRepository.getCorporateCustomerById(new Guid(corporateCustomerId), new string[] { "rnt_checkfindeks" });

                if (corp.Contains("rnt_checkfindeks") && !corp.GetAttributeValue<bool>("rnt_checkfindeks"))
                {
                    this.Trace("corp parameter doesnt check findeks");
                    return new AdditionalProductResponse
                    {
                        ResponseResult = ResponseResult.ReturnSuccess()
                    };
                }

            }
            var groupFindeks = 0;
            if (!reservationId.HasValue)
            {
                GroupCodeInformationRepository groupCodeListRepository = new GroupCodeInformationRepository(this.OrgService);
                var groupCode = groupCodeListRepository.getGroupCodeInformationById(groupCodeId, new string[] { "rnt_findeks" });
                groupFindeks = groupCode.GetAttributeValue<int>("rnt_findeks");
            }
            else
            {
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                var res = reservationRepository.getReservationById(reservationId.Value, new string[] { "rnt_groupcodeid", "rnt_findeks" });
                //eğer grup kodu değişmiyorsa rez üstünden al
                if (res.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id == groupCodeId ||
                   (!string.IsNullOrEmpty(changeType) && (Convert.ToInt32(changeType) == (int)rnt_ChangeType.Downgrade || Convert.ToInt32(changeType) == (int)rnt_ChangeType.Upgrade)))
                {
                    groupFindeks = res.GetAttributeValue<int>("rnt_findeks");
                }
                else
                {
                    GroupCodeInformationRepository groupCodeListRepository = new GroupCodeInformationRepository(this.OrgService);
                    var groupCode = groupCodeListRepository.getGroupCodeInformationById(groupCodeId, new string[] { "rnt_findeks" });
                    groupFindeks = groupCode.GetAttributeValue<int>("rnt_findeks");
                }
            }
            if (individualCustomerId != Guid.Empty)
            {
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
                var contact = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(individualCustomerId, new string[] { "rnt_findekspoint" });


                var customerFindeks = contact.GetAttributeValue<int>("rnt_findekspoint");

                this.Trace("groupCodeInformationCRMData.rnt_findeks : " + groupFindeks);
                this.Trace("individualCustomerDetailData.findeksPoint : " + customerFindeks);
                if (groupFindeks > contact.GetAttributeValue<int>("rnt_findekspoint"))
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("FindeksValidation", langId, this.additionalProductXmlPath);
                    return new AdditionalProductResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(string.Format(message, customerFindeks, groupFindeks))
                    };
                }
            }
            return new AdditionalProductResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public bool checkAdditionalProductsMaxPieces(List<AdditionalProductData> data)
        {
            var products = data.Where(p => p.maxPieces > 1).ToList();
            foreach (var item in products)
            {
                AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
                var product = additionalProductRepository.getAdditionalProductById(item.productId, new string[] { "rnt_maximumpieces" });
                var maxPieces = product.GetAttributeValue<int>("rnt_maximumpieces");
                if (item.value > maxPieces)
                    return false;
            }
            return true;
        }
        public bool checkAdditionalProductsLogoId(List<AdditionalProductData> data)
        {
            foreach (var item in data)
            {
                AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
                var product = additionalProductRepository.getAdditionalProductById(item.productId, new string[] { "rnt_logoid" });
                var logoId = product.Attributes.Contains("rnt_logoid") ? product.GetAttributeValue<string>("rnt_logoid") : string.Empty;
                if (string.IsNullOrEmpty(logoId))
                    return false;
            }
            return true;
        }
        public bool checkAdditionalProductsRules(List<AdditionalProductData> data)
        {
            foreach (var item in data)
            {
                AdditionalProductRuleRepository additionalProductRuleRepository = new AdditionalProductRuleRepository(this.OrgService);
                var productRule = additionalProductRuleRepository.getAdditonalProductRuleByAdditionalProductId(item.productId);
                if (productRule != null)
                {
                    var parentProductId = productRule.GetAttributeValue<EntityReference>("rnt_parentproduct").Id;
                    if (data.Exists(p => p.productId == parentProductId))
                        return false;
                }
            }
            return true;
        }
        public bool checkOneWayFee(Guid pickUpBranchId, Guid dropoffBranchId)
        {
            if (pickUpBranchId != dropoffBranchId)
                return false;

            return true;
        }

        public bool compareAdditionalProductForMandatoryProducts(List<AdditionalProductData> currentAdditionalProducts,
                                                                 List<AdditionalProductData> calculatedAdditionalProducts)
        {
            var returnValue = false;
            foreach (var item in calculatedAdditionalProducts)
            {
                if (item.isMandatory)
                {
                    var product = currentAdditionalProducts.Where(p => p.productId == item.productId).FirstOrDefault();
                    if (product == null)
                    {

                        AdditionalProductRuleRepository additionalProductRuleRepository = new AdditionalProductRuleRepository(this.OrgService);
                        foreach (var prdct in currentAdditionalProducts)
                        {
                            var productRule = additionalProductRuleRepository.getAdditonalProductRuleByAdditionalProductandParentProductId(item.productId, prdct.productId);
                            if (productRule != null)
                            {
                                var parentProductId = productRule.GetAttributeValue<EntityReference>("rnt_parentproduct").Id;
                                var productId = productRule.GetAttributeValue<EntityReference>("rnt_product").Id;
                                if (item.productId == parentProductId || item.productId == productId)
                                    returnValue = true;
                                else
                                    returnValue = false;
                            }
                            else
                            {
                                productRule = additionalProductRuleRepository.getAdditonalProductRuleByAdditionalProductandParentProductId(prdct.productId, item.productId);
                                if (productRule != null)
                                {
                                    var parentProductId = productRule.GetAttributeValue<EntityReference>("rnt_parentproduct").Id;
                                    var productId = productRule.GetAttributeValue<EntityReference>("rnt_product").Id;
                                    if (item.productId == parentProductId || item.productId == productId)
                                        returnValue = true;
                                    else
                                        returnValue = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        returnValue = true;
                    }
                }
            }
            return returnValue;
        }
    }
}
