using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    // Tolga AYKURT - 27.02.2019
    public class CampaignRepository : MongoDBInstance
    {
        #region CONSTRUCTORS
        public CampaignRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public CampaignRepository(object client, object database) : base(client, database)
        {
        }
        #endregion

        #region METHODS
        public List<CampaignDataMongoDB> getAllCampaings()
        {
            var list = this._database.GetCollection<CampaignDataMongoDB>("Campaigns").AsQueryable().ToList();

            return list;
        }
        public List<CampaignDataMongoDB> getActiveCampaigns(DateTime relatedDay, int channel)
        {
            var list = this._database.GetCollection<CampaignDataMongoDB>("Campaigns")
              .AsQueryable()
              .Where(c =>
                      c.campaignType != 4 &&
                      c.reservatinChannelCode.Contains(channel.ToString()) &&
                      c.statuscode.Equals((int)ClassLibrary._Enums_1033.rnt_campaign_StatusCode.Active)).ToList();

            list = list.Where(p => p.BeginingDateTimeStamp.Value.converttoDateTime().AddMinutes(StaticHelper.offset).Date <= relatedDay.Date).ToList();
            list = list.Where(p => p.EndDateTimeStamp.Value.converttoDateTime().AddMinutes(StaticHelper.offset).Date >= relatedDay.Date).ToList();
            return list;
        }

        // Tolga AYKURT - 27.02.2019
        public List<CampaignDataMongoDB> GetCampaigns(BsonTimestamp relatedDayTimeStamp,
                                                     int reservationDayCount,
                                                     string reservationChannelCode,
                                                     string branchId,
                                                     int customerType,
                                                     string groupCodeInformationId)
        {
            customerType = customerType * 10;
            var list = this._database.GetCollection<CampaignDataMongoDB>("Campaigns")
                .AsQueryable()
                .Where(c =>

                        c.reservatinChannelCode.Contains(reservationChannelCode) &&
                        c.branchCode.Contains(branchId) &&
                        c.groupCode.Contains(groupCodeInformationId) &&
                        c.statuscode.Equals((int)ClassLibrary._Enums_1033.rnt_campaign_StatusCode.Active) &&
                        c.minReservationDay <= reservationDayCount &&
                        c.reservationTypeCode.Contains(Convert.ToString(customerType)) &&
                        c.maxReservationDay >= reservationDayCount).ToList();

            list = list.Where(p => p.BeginingDateTimeStamp.Value.converttoDateTime().Date <= relatedDayTimeStamp.Value.converttoDateTime().Date).ToList();
            list = list.Where(p => p.EndDateTimeStamp.Value.converttoDateTime().Date >= relatedDayTimeStamp.Value.converttoDateTime().Date).ToList();
            return list;
        }
        public List<CampaignDataMongoDB> GetCampaign(BsonTimestamp relatedDayTimeStamp,
                                               int reservationDayCount,
                                               string reservationChannelCode,
                                               string branchId,
                                               int customerType,
                                               string groupCodeInformationId,
                                               string campaignId)
        {
            customerType = customerType * 10;
            var list = this._database.GetCollection<CampaignDataMongoDB>("Campaigns")
                .AsQueryable()
                .Where(c =>

                        c.reservatinChannelCode.Contains(reservationChannelCode) &&
                        c.branchCode.Contains(branchId) &&
                        c.groupCode.Contains(groupCodeInformationId) &&
                        c.statuscode.Equals((int)ClassLibrary._Enums_1033.rnt_campaign_StatusCode.Active) &&
                        c.minReservationDay <= reservationDayCount &&
                        c.campaignType != 4 &&
                        c.reservationTypeCode.Contains(Convert.ToString(customerType)) &&
                        c.maxReservationDay >= reservationDayCount &&
                        c.campaignId.Equals(campaignId)).ToList();

            list = list.Where(p => p.BeginingDateTimeStamp.Value.converttoDateTime().Date <= relatedDayTimeStamp.Value.converttoDateTime().Date).ToList();
            list = list.Where(p => p.EndDateTimeStamp.Value.converttoDateTime().Date >= relatedDayTimeStamp.Value.converttoDateTime().Date).ToList();
            return list.ToList();
        }

        public CampaignDataMongoDB GetCampaign_MainPrice(BsonTimestamp relatedDayTimeStamp,
                                                         int reservationDayCount,
                                                         string reservationChannelCode,
                                                         string branchId,
                                                         int customerType)
        {
            if (customerType == 3)
            {
                customerType = 4;
            }
            else if(customerType == 4)
            {
                customerType = 3;
            }
            customerType = customerType * 10;
            var list = this._database.GetCollection<CampaignDataMongoDB>("Campaigns")
                .AsQueryable()
                .Where(c =>

                        c.reservatinChannelCode.Contains(reservationChannelCode) &&
                        c.branchCode.Contains(branchId) &&
                        c.statuscode.Equals((int)ClassLibrary._Enums_1033.rnt_campaign_StatusCode.Active) &&
                        c.minReservationDay <= reservationDayCount &&
                        c.reservationTypeCode.Contains(Convert.ToString(customerType)) &&
                        c.maxReservationDay >= reservationDayCount &&
                        c.campaignType == 4).ToList();

            list = list.Where(p => p.BeginingDateTimeStamp.Value.converttoDateTime().Date <= relatedDayTimeStamp.Value.converttoDateTime().Date).ToList();
            list = list.Where(p => p.EndDateTimeStamp.Value.converttoDateTime().Date >= relatedDayTimeStamp.Value.converttoDateTime().Date).ToList();
            return list.FirstOrDefault();
        }

        public CampaignDataMongoDB GetCampaign_MainPrice(BsonTimestamp relatedDayTimeStamp,
                                                         int reservationDayCount,
                                                         string reservationChannelCode,
                                                         string branchId)
        {

            var list = this._database.GetCollection<CampaignDataMongoDB>("Campaigns")
                .AsQueryable()
                .Where(c =>

                        c.reservatinChannelCode.Contains(reservationChannelCode) &&
                        c.branchCode.Contains(branchId) &&
                        c.statuscode.Equals((int)ClassLibrary._Enums_1033.rnt_campaign_StatusCode.Active) &&
                        c.minReservationDay <= reservationDayCount &&
                        c.maxReservationDay >= reservationDayCount &&
                        c.campaignType == 4).ToList();

            list = list.Where(p => p.BeginingDateTimeStamp.Value.converttoDateTime().Date <= relatedDayTimeStamp.Value.converttoDateTime().Date).ToList();
            list = list.Where(p => p.EndDateTimeStamp.Value.converttoDateTime().Date >= relatedDayTimeStamp.Value.converttoDateTime().Date).ToList();
            return list.FirstOrDefault();
        }

        public CampaignDataMongoDB getCampaingById(string campaignId)
        {

            var list = this._database.GetCollection<CampaignDataMongoDB>("Campaigns")
                .AsQueryable()
                .Where(c =>
                        c.campaignId == campaignId).FirstOrDefault();

            return list;
        }

        public CampaignDataMongoDB getCampaignByParameters(string campaignId,
                                                           BsonTimestamp relatedDayTimeStamp,
                                                           int reservationDayCount,
                                                           string reservationChannelCode,
                                                           string branchId,
                                                           int customerType,
                                                           string groupCodeInformationId,
                                                           bool checkMaxDate)
        {
            customerType = customerType * 10;
            var list = this._database.GetCollection<CampaignDataMongoDB>("Campaigns")
                        .AsQueryable()
                        .Where(c =>
                                //c.reservatinChannelCode.Contains(reservationChannelCode) &&
                                //c.branchCode.Contains(branchId) &&
                                //c.groupCode.Contains(groupCodeInformationId) &&
                                //c.reservationTypeCode.Contains(Convert.ToString(customerType)) &&

                                c.campaignId == campaignId &&
                                c.minReservationDay <= reservationDayCount).ToList();
            list = list.Where(p => p.reservatinChannelCode.Contains(reservationChannelCode)).ToList();
            list = list.Where(p => p.branchCode.Contains(branchId)).ToList();
            list = list.Where(p => p.groupCode.Contains(groupCodeInformationId)).ToList();
            list = list.Where(p => p.reservationTypeCode.Contains(Convert.ToString(customerType))).ToList();
            list = list.Where(p => p.BeginingDateTimeStamp.Value.converttoDateTime().Date <= relatedDayTimeStamp.Value.converttoDateTime().Date).ToList();
            list = list.Where(p => p.EndDateTimeStamp.Value.converttoDateTime().Date >= relatedDayTimeStamp.Value.converttoDateTime().Date).ToList();
            if (checkMaxDate)
            {
                list = list.Where(p => p.maxReservationDay >= reservationDayCount).ToList();
            }

            return list.FirstOrDefault();
        }
        #endregion
    }
}
