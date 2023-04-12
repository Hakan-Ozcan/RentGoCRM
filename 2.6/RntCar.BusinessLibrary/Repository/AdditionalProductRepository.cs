using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class AdditionalProductRepository : RepositoryHandler
    {
        public AdditionalProductRepository(IOrganizationService Service) : base(Service)
        {
        }

        public AdditionalProductRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public AdditionalProductRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public AdditionalProductRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public AdditionalProductRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public List<AdditionalProductData> GetAdditionalProductsWithCalculatedPrice(int totalDuration, Guid branchId)
        {

            QueryExpression getAdditionalProducts = new QueryExpression("rnt_additionalproduct");
            getAdditionalProducts.ColumnSet = new ColumnSet(new String[] { "rnt_name",
                                                           "rnt_additionalproducttype",
                                                           "rnt_additionalproductcode",
                                                           "rnt_maximumpieces",
                                                           "rnt_showonweb",
                                                           "rnt_showonwebsite",
                                                           "rnt_webrank",
                                                           "rnt_productdescription",
                                                           "rnt_price",
                                                           "rnt_pricecalculationtypecode",
                                                           "rnt_monthlypackageprice",
                                                           "rnt_webiconurl",
                                                           "rnt_showoncontractupdate",
                                                           "rnt_showoncontractupdateformonthly"});
            getAdditionalProducts.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            getAdditionalProducts.Criteria.AddFilter(getDateFilterForAdditionalProduct());
            if(branchId != null && branchId != Guid.Empty)
              getAdditionalProducts.Criteria.AddFilter(getBranchFilterForAdditionalProduct(branchId));
            var result = this.retrieveMultiple(getAdditionalProducts);
            List<AdditionalProductData> additionalProducts = new List<AdditionalProductData>();
            foreach (var item in result.Entities)
            {
                var monthlyPrice = item.Attributes.Contains("rnt_monthlypackageprice") ?
                                       item.GetAttributeValue<Money>("rnt_monthlypackageprice").Value :
                                       decimal.Zero;
                var actualTotalAmount = CommonHelper.calculateAdditionalProductPrice(item.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value,
                                                                                    monthlyPrice,
                                                                                    item.Attributes.Contains("rnt_price") ?
                                                                                    item.GetAttributeValue<Money>("rnt_price").Value :
                                                                                    decimal.Zero,
                                                                                    totalDuration);
                additionalProducts.Add(new AdditionalProductMapper().buildAdditionalProductData(item, actualTotalAmount, monthlyPrice));
            }

            return additionalProducts;
        }

        public List<AdditionalProductData> GetServiceAdditionalProductsWithCalculatedPrice(int totalDuration, Guid branchId)
        {
            QueryExpression query = new QueryExpression("rnt_additionalproduct");
            query.ColumnSet = new ColumnSet(new String[] { "rnt_name",
                                                           "rnt_additionalproducttype",
                                                           "rnt_additionalproductcode",
                                                           "rnt_maximumpieces",
                                                           "rnt_showonweb",
                                                           "rnt_showonwebsite",
                                                           "rnt_webrank",
                                                           "rnt_productdescription",
                                                           "rnt_price",
                                                           "rnt_pricecalculationtypecode",
                                                           "rnt_monthlypackageprice",
                                                           "rnt_webiconurl",
                                                           "rnt_showoncontractupdate",
                                                           "rnt_showontabletforservice"});
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_showontabletforservice", ConditionOperator.Equal, true);
            query.Criteria.AddFilter(getDateFilterForAdditionalProduct());
            if (branchId != null && branchId != Guid.Empty)
                query.Criteria.AddFilter(getBranchFilterForAdditionalProduct(branchId));
            var result = this.retrieveMultiple(query);
            List<AdditionalProductData> additionalProducts = new List<AdditionalProductData>();
            foreach (var item in result.Entities)
            {
                var monthlyPrice = item.Attributes.Contains("rnt_monthlypackageprice") ?
                                       item.GetAttributeValue<Money>("rnt_monthlypackageprice").Value :
                                       decimal.Zero;
                var actualTotalAmount = CommonHelper.calculateAdditionalProductPrice(item.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value,
                                                                                    monthlyPrice,
                                                                                    item.Attributes.Contains("rnt_price") ?
                                                                                    item.GetAttributeValue<Money>("rnt_price").Value :
                                                                                    decimal.Zero,
                                                                                    totalDuration);
                additionalProducts.Add(new AdditionalProductMapper().buildAdditionalProductData(item, actualTotalAmount, monthlyPrice));
            }

            return additionalProducts;
        }

        public List<AdditionalProductData> GetAdditionalProductDataForUpdate(string reservationId, int totalDuration, decimal reservationPaidAmount)
        {
            List<AdditionalProductData> additionalProducts = new List<AdditionalProductData>();

            QueryExpression reservationItemQuery = new QueryExpression("rnt_reservationitem");
            reservationItemQuery.ColumnSet = new ColumnSet("rnt_additionalproductid", "rnt_baseprice", "rnt_monthlypackageprice", "rnt_reservationduration", "rnt_itemtypecode", "rnt_totalamount", "rnt_billingtypecode");
            reservationItemQuery.LinkEntities.Add(new LinkEntity("rnt_reservationitem", "rnt_additionalproduct", "rnt_additionalproductid", "rnt_additionalproductid", JoinOperator.Inner));
            reservationItemQuery.LinkEntities[0].Columns.AddColumns("rnt_additionalproductid",
                                                                    "rnt_name",
                                                                    "rnt_additionalproducttype",
                                                                    "rnt_additionalproductcode",
                                                                    "rnt_maximumpieces",
                                                                    "rnt_showonweb",
                                                                    "rnt_showonwebsite",
                                                                    "rnt_webrank",
                                                                    "rnt_productdescription",
                                                                    "rnt_price",
                                                                    "rnt_pricecalculationtypecode",
                                                                    "rnt_monthlypackageprice");
            reservationItemQuery.LinkEntities[0].EntityAlias = "additionalProducts";
            reservationItemQuery.Criteria = new FilterExpression(LogicalOperator.And);
            reservationItemQuery.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.New);
            reservationItemQuery.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            reservationItemQuery.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)GlobalEnums.ItemTypeCode.AdditionalProduct);
            var reservationItem = this.retrieveMultiple(reservationItemQuery);

            foreach (var item in reservationItem.Entities)
            {

                if (item.Attributes.Contains("additionalProducts.rnt_additionalproductid"))
                {
                    var index = additionalProducts.FindIndex(x => x.productId == ((Guid)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_additionalproductid").Value));
                    if (index > -1)
                    {
                        additionalProducts[index].value += 1; // calculate selected items count like bebek koltuğu * 2
                                                              // if reservation paid amount greater then zero, calculate paid amount for additional products
                        additionalProducts[index].paidAmount = reservationPaidAmount > decimal.Zero ? ((decimal)additionalProducts[index].actualTotalAmount * additionalProducts[index].value) : 0; // calculate paid amount with selected count value
                        additionalProducts[index].tobePaidAmount = (additionalProducts[index].actualTotalAmount * additionalProducts[index].value) - additionalProducts[index].paidAmount; //calculate to be paid amount with value
                    }
                    else
                    {
                        var actualPrice = item.Attributes.Contains("rnt_baseprice") ?
                                          item.GetAttributeValue<Money>("rnt_baseprice").Value : 0;

                        var monthlyPrice = item.Attributes.Contains("rnt_monthlypackageprice") ?
                                           item.GetAttributeValue<Money>("rnt_monthlypackageprice").Value :
                                           decimal.Zero;
                        var actualTotalAmount = CommonHelper.calculateAdditionalProductPrice(((OptionSetValue)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_pricecalculationtypecode").Value).Value,
                                                                                              monthlyPrice,
                                                                                              actualPrice,
                                                                                              totalDuration);

                        additionalProducts.Add(new AdditionalProductMapper().buildAdditionalProductDatafromRelation(item, actualPrice, actualTotalAmount, monthlyPrice));
                    }
                }
            }
            return additionalProducts;
        }

        public List<AdditionalProductData> GetAdditionalProductDataForQuickContract(string reservationId)
        {
            // this method for just getting item id and display data
            List<AdditionalProductData> additionalProducts = new List<AdditionalProductData>();

            QueryExpression reservationItemQuery = new QueryExpression("rnt_reservationitem");
            reservationItemQuery.ColumnSet = new ColumnSet("rnt_additionalproductid", "rnt_baseprice", "rnt_reservationduration", "rnt_itemtypecode", "rnt_totalamount");
            reservationItemQuery.Criteria = new FilterExpression(LogicalOperator.And);
            reservationItemQuery.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)ReservationItemEnums.StatusCode.New);
            reservationItemQuery.Criteria.AddCondition("rnt_reservationid", ConditionOperator.Equal, reservationId);
            //reservationItemQuery.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)GlobalEnums.ItemTypeCode.AdditionalProduct);
            var reservationItem = this.retrieveMultiple(reservationItemQuery);

            foreach (var item in reservationItem.Entities)
            {
                var actualTotalAmount = item.Attributes.Contains("rnt_totalamount") ?
                                        item.GetAttributeValue<Money>("rnt_totalamount").Value : 0;
                var typeCode = item.Attributes.Contains("rnt_itemtypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value : 0;

                // get first additional product by id and just update first one value for price grid
                // add other items to list for the initialize from request guid list
                // check list with additional product name because there is no other unique field
                var additionalProduct = additionalProducts.Where(x => x.productId == (item.GetAttributeValue<EntityReference>("rnt_additionalproductid")?.Id)).FirstOrDefault();
                if (additionalProduct != null)
                {
                    additionalProduct.value += 1;
                    additionalProduct.maxPieces = 2;
                    additionalProducts.Add(new AdditionalProductData
                    {
                        productId = item.Id,// set item id to product id for initialize from request guid list
                        maxPieces = 0, // the value must be zero to not appear on the price grid
                    });
                }
                else
                {
                    additionalProducts.Add(new AdditionalProductMapper().buildAdditionalProductDataforQuickContract(item, actualTotalAmount, typeCode));
                }
            }

            return additionalProducts;
        }
        public List<AdditionalProductData> GetAdditionalProductDataForContract(string contractId, DateTime dropoffDateTime)
        {
            List<AdditionalProductData> additionalProducts = new List<AdditionalProductData>();

            QueryExpression contractItemQuery = new QueryExpression("rnt_contractitem");
            contractItemQuery.ColumnSet = new ColumnSet("rnt_additionalproductid", "rnt_pickupdatetime", "rnt_monthlypackageprice", "rnt_baseprice", "rnt_contractduration", "rnt_baseprice", "rnt_itemtypecode", "rnt_totalamount", "rnt_billingtype");
            contractItemQuery.LinkEntities.Add(new LinkEntity("rnt_contractitem", "rnt_additionalproduct", "rnt_additionalproductid", "rnt_additionalproductid", JoinOperator.Inner));
            contractItemQuery.LinkEntities[0].Columns.AddColumns("rnt_additionalproductid",
                                                                 "rnt_name",
                                                                 "rnt_additionalproducttype",
                                                                 "rnt_additionalproductcode",
                                                                 "rnt_maximumpieces",
                                                                 "rnt_showonweb",
                                                                 "rnt_showonwebsite",
                                                                 "rnt_webrank",
                                                                 "rnt_productdescription",
                                                                 "rnt_price",
                                                                 "rnt_pricecalculationtypecode",
                                                                 "rnt_monthlypackageprice");
            contractItemQuery.LinkEntities[0].EntityAlias = "additionalProducts";
            contractItemQuery.Criteria = new FilterExpression(LogicalOperator.And);
            contractItemQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            contractItemQuery.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            contractItemQuery.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.AdditionalProduct);

            FilterExpression filterExpression = new FilterExpression(LogicalOperator.Or);
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Rental));
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.WaitingForDelivery));
            contractItemQuery.Criteria.AddFilter(filterExpression);

            var contractItem = this.retrieveMultiple(contractItemQuery);

            foreach (var item in contractItem.Entities)
            {

                if (item.Attributes.Contains("additionalProducts.rnt_additionalproductid"))
                {
                    var index = additionalProducts.FindIndex(x => x.productId == ((Guid)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_additionalproductid").Value));
                    if (index > -1)
                    {
                        additionalProducts[index].value += 1; // calculate selected items count like bebek koltuğu * 2
                        additionalProducts[index].paidAmount = (item.GetAttributeValue<Money>("rnt_totalamount").Value * (additionalProducts[index].value)); // calculate paid amount with selected count value
                        additionalProducts[index].tobePaidAmount = (additionalProducts[index].actualTotalAmount * (additionalProducts[index].value)) - additionalProducts[index].paidAmount; //calculate to be paid amount with value
                    }
                    else
                    {
                        ContractHelper contractHelper = new ContractHelper(this.Service);
                        var totalDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(item.GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset),
                                                                                                            dropoffDateTime);
                        var monthlyPrice = item.Attributes.Contains("rnt_monthlypackageprice") ?
                                      item.GetAttributeValue<Money>("rnt_monthlypackageprice").Value :
                                      decimal.Zero;
                        // for discount product
                        var additionalProductPrice = item.Attributes.Contains("rnt_baseprice") ? item.GetAttributeValue<Money>("rnt_baseprice").Value : decimal.Zero;

                        var actualTotalAmount = CommonHelper.calculateAdditionalProductPrice(((OptionSetValue)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_pricecalculationtypecode").Value).Value,
                                                                                            monthlyPrice,
                                                                                            additionalProductPrice,
                                                                                            totalDuration);
                        additionalProducts.Add(new AdditionalProductMapper().buildAdditionalProductDatafromRelation(item, additionalProductPrice, actualTotalAmount, monthlyPrice, true));
                    }
                }
            }
            return additionalProducts;
        }

        public AdditionalProductData getYoungDriverProductForAdditionalDriversWithCalculatedPrice(int totalDuration, string productCode)
        {
            QueryExpression query = new QueryExpression("rnt_additionalproduct");
            query.ColumnSet = new ColumnSet("rnt_additionalproductid",
                                            "rnt_name",
                                            "rnt_additionalproducttype",
                                            "rnt_additionalproductcode",
                                            "rnt_maximumpieces",
                                            "rnt_showonweb",
                                            "rnt_showonwebsite",
                                            "rnt_webrank",
                                            "rnt_productdescription",
                                            "rnt_price",
                                            "rnt_pricecalculationtypecode");
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_additionalproductcode", ConditionOperator.Equal, productCode);
            query.Criteria.AddFilter(getDateFilterForAdditionalProduct()); 
            var item = this.retrieveMultiple(query).Entities.FirstOrDefault();
            if (item != null)
            {
                var actualTotalAmount = item.GetAttributeValue<Money>("rnt_price").Value * totalDuration;
                var additionalProductData = new AdditionalProductMapper().buildAdditionalProductData(item, actualTotalAmount, decimal.Zero);
                additionalProductData.value = 1;
                additionalProductData.paidAmount = 0;
                additionalProductData.tobePaidAmount = 0;
                additionalProductData.showonWeb = true;
                additionalProductData.isMandatory = true;
                return additionalProductData;
            }
            return null;
        }

        public Entity getAdditionalProductByProductCode(string productCode, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_additionalproduct");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_additionalproductcode", ConditionOperator.Equal, productCode);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getAdditionalProductByProductCode(string productCode)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_additionalproduct");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_additionalproductcode", ConditionOperator.Equal, productCode);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getAdditionalProductsByProductCodes(string[] productCodes, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_additionalproduct");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_additionalproductcode", ConditionOperator.In, productCodes);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public Entity getAdditionalProductById(Guid productId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_additionalproduct");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, productId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public Entity getAdditionalProductById(Guid productId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_additionalproduct");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, productId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getAdditonalProductByParentAdditionalProductId(Guid parentProductId, string[] columns)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_additionalproduct");
            queryExpression.ColumnSet = new ColumnSet(columns);
            queryExpression.Criteria.AddCondition("rnt_parentadditionalproductid", ConditionOperator.Equal, parentProductId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public Entity getAdditonalProductByParentAdditionalProductId(Guid parentProductId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_additionalproduct");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_parentadditionalproductid", ConditionOperator.Equal, parentProductId);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
        public List<Entity> getActiveAdditionalProducts()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_additionalproduct");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        public List<Entity> getAdditionalProducts()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_additionalproduct");
            queryExpression.ColumnSet = new ColumnSet(true);

            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }
        private FilterExpression getDateFilterForAdditionalProduct()
        {

            var mainFilter = new FilterExpression(LogicalOperator.Or);
            var secondFilter = new FilterExpression(LogicalOperator.And);
            var currentDateTime = DateTime.Now.Date;

            mainFilter.AddCondition("rnt_isdatefilter", ConditionOperator.Equal, false);

            secondFilter.AddCondition("rnt_isdatefilter", ConditionOperator.Equal, true);
            secondFilter.AddCondition("rnt_filterfirstdate", ConditionOperator.OnOrBefore, currentDateTime);
            secondFilter.AddCondition("rnt_filterlastdate", ConditionOperator.OnOrAfter, currentDateTime);

            mainFilter.AddFilter(secondFilter);

            return mainFilter;
        }

        private FilterExpression getBranchFilterForAdditionalProduct(Guid branchId)
        { 
            MultiSelectMappingRepository multiSelectMappingRepository = new MultiSelectMappingRepository(this.Service);
            var optionValue = multiSelectMappingRepository.getOptionValueByGuid("rnt_branch",branchId);

            var mainFilter = new FilterExpression(LogicalOperator.Or);
            var secondFilter = new FilterExpression(LogicalOperator.And);

            mainFilter.AddCondition("rnt_isbranchfilter", ConditionOperator.Equal, false);

            secondFilter.AddCondition("rnt_isbranchfilter", ConditionOperator.Equal, true);
            secondFilter.AddCondition("rnt_filterbranches", ConditionOperator.In, optionValue);

            mainFilter.AddFilter(secondFilter);

            return mainFilter;
        }
    }
}
