using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class PriceCalculationBL : BusinessHandler
    {
        public PriceCalculationBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public PriceCalculationBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public PriceCalculationResponse executeCreatePriceCalculation(string priceListParameter, string groupCodesData, string groupCodeListPricesData, string availabilityPriceListData)
        {
            PriceFactorValidation priceListBL = new PriceFactorValidation(this.OrgService, this.TracingService);

            var priceList = JsonConvert.DeserializeObject<PriceListRelationData>(priceListParameter);

            try
            {
                var _id = priceListBL.createPriceListFromPriceCalculation(priceList);
                this.Trace("pricelist created with id : " + _id);
                var groupCodes = JsonConvert.DeserializeObject<List<GroupCodeRelationData>>(groupCodesData);

                var groupCodesListPrice = JsonConvert.DeserializeObject<List<GroupCodeListPriceRelationData>>(groupCodeListPricesData);

                GroupCodeListPriceBL groupCodeListPriceBL = new GroupCodeListPriceBL(this.OrgService, this.TracingService);

                groupCodeListPriceBL.createGroupCodeListPriceFromPriceCalculation(groupCodes, groupCodesListPrice, _id);
                this.Trace("groupCode Lsit prices are created : " + groupCodesListPrice.Count);
                // if price type eq individual create availability price list
                if (priceList.rnt_pricetype == 1)
                {
                    var availabilityPriceList = JsonConvert.DeserializeObject<List<AvailabilityPriceListRelationData>>(availabilityPriceListData);

                    AvailabilityPriceListBL availabilityPriceListBL = new AvailabilityPriceListBL(this.OrgService, this.TracingService);
                    availabilityPriceListBL.createAvailabilityPriceListFromPriceCalculation(availabilityPriceList, _id);
                    this.Trace("individual availability prices created : " + availabilityPriceList.Count);

                  
                }

                return new PriceCalculationResponse
                {
                    priceListId = _id,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public bool checkActivePriceListByDates(PriceListRelationData priceListData)
        {
            if (priceListData.rnt_pricetype == 1)
            {
                PriceListRepository priceListRepository = new PriceListRepository(this.OrgService);
                var response = priceListRepository.getPriceListDataByDate(priceListData.rnt_begindate, priceListData.rnt_enddate);
                if (response.Entities.Count > 0)
                    return false;
            }

            return true;
        }
    }
}
