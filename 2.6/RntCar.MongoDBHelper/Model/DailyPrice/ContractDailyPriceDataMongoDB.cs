﻿using MongoDB.Bson;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RntCar.MongoDBHelper.Model
{
    public class ContractDailyPriceDataMongoDB
    {
        public ObjectId _id { get; set; }
        public string trackingNumber { get; set; }
        [NotMapped]
        public BsonTimestamp priceDateTimeStamp { get; set; }
        public DateTime priceDate { get; set; }
        public string userId { get; set; }
        public string userEntityLogicalName { get; set; }
        public decimal totalAmount { get; set; }
        public string selectedPriceListId { get; set; }
        public string selectedGroupCodePriceListId { get; set; }
        public decimal selectedGroupCodeAmount { get; set; }
        public string selectedAvailabilityPriceListId { get; set; }
        public decimal selectedAvailabilityPriceRate { get; set; }
        public string relatedGroupCodeId { get; set; }
        public string relatedGroupCodeName { get; set; }
        public decimal availabilityRate { get; set; }
        [Key]
        public string ID { get; set; }
        public Guid contractItemId { get; set; }
        public string contractItemId_str { get; set; }
        public decimal priceAfterAvailabilityFactor { get; set; }
        public decimal priceAfterCustomerFactor { get; set; }
        public decimal priceAfterChannelFactor { get; set; }
        public decimal priceAfterSpecialDaysFactor { get; set; }
        public decimal priceAfterWeekDaysFactor { get; set; }
        public decimal priceAfterBranchFactor { get; set; }

        public decimal priceAfterBranch2Factor { get; set; }
        public decimal priceAfterEqualityFactor { get; set; }
        public decimal priceAfterPayMethodPayLater { get; set; }
        public decimal priceAfterPayMethodPayNow { get; set; }
        public bool isTickDay { get; set; } = false;
        public Guid campaignId { get; set; }
        public decimal payNowAmount { get; set; }
        public decimal payLaterAmount { get; set; }
        public decimal payNowWithoutTickDayAmount { get; set; }
        public decimal payLaterWithoutTickDayAmount { get; set; }

    }
}
