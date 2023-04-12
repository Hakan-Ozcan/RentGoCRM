using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Helper
{
    public class CampaignHelper
    {
        public CampaignData buildCampaignData(CampaignDataMongoDB campaign)
        {
            return new CampaignData
            {
                #region mapping
                groupCodePrices = campaign.groupCodePrices,
                additionalProductCode = campaign.additionalProductCode,
                additionalProductDailyPrice = campaign.additionalProductDailyPrice,
                additionalProductDiscountRatio = campaign.additionalProductDiscountRatio,
                beginingDate = campaign.beginingDate,
                branchCode = campaign.branchCode,
                campaignId = campaign.campaignId,
                campaignType = campaign.campaignType,
                createdby = campaign.createdby,
                createdon = campaign.createdon,
                description = campaign.description,
                endDate = campaign.endDate,
                groupCode = campaign.groupCode,
                minReservationDay = campaign.minReservationDay,
                maxReservationDay = campaign.maxReservationDay,
                modifiedby = campaign.modifiedby,
                modifiedon = campaign.modifiedon,
                name = campaign.name,
                payLaterDailyPrice = campaign.payLaterDailyPrice,
                payLaterDiscountRatio = campaign.payLaterDiscountRatio,
                payNowDailyPrice = campaign.payNowDailyPrice,
                payNowDiscountRatio = campaign.payNowDiscountRatio,
                priceEffect = campaign.priceEffect,
                productType = campaign.productType,
                reservatinChannelCode = campaign.reservatinChannelCode,
                reservationTypeCode = campaign.reservationTypeCode,
                statecode = campaign.statecode,
                statuscode = campaign.statuscode
                #endregion
            };
        }
    }
}
