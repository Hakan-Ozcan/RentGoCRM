using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.odata
{
    public class DailyPrice
    {
        [Key]
        public Guid dailyPricesId { get; set; }
        public string mongodbId { get; set; }
        public string trackingNumber { get; set; }
        public DateTime priceDate { get; set; }
        public long _priceDateTimeStamp { get; set; }
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
        public Guid reservationItemId { get; set; }
        public Guid contractItemId { get; set; }
        public decimal priceAfterAvailabilityFactor { get; set; }
        public decimal priceAfterCustomerFactor { get; set; }
        public decimal priceAfterChannelFactor { get; set; }
        public decimal priceAfterSpecialDaysFactor { get; set; }
        public decimal priceAfterWeekDaysFactor { get; set; }

        public Guid? campaignId { get; set; }
        public decimal priceAfterBranchFactor { get; set; }
        public decimal priceAfterBranch2Factor { get; set; }
        public decimal priceAfterEqualityFactor { get; set; }
        public decimal payNowAmount { get; set; }
        public decimal payLaterAmount { get; set; }
        public decimal priceAfterPayMethod { get; set; }
        public decimal priceAfterPayMethodPayLater { get; set; }
        public decimal priceAfterPayMethodPayNow { get; set; }

    }
}
