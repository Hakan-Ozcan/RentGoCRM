using System;

namespace RntCar.ClassLibrary.MongoDB
{
    public class AvailabilityParameters
    {
        public Guid pickupBranchId { get; set; }
        public Guid dropOffBranchId { get; set; }
        public DateTime pickupDateTime { get; set; }        
        public DateTime dropoffDateTime { get; set; }
        public string reservationId { get; set; }
        public string contractId { get; set; }
        public int shiftDuration { get; set; }
        public int channel { get; set; }
        public int customerType { get; set; }
        public Guid priceCodeId { get; set; }
        public string corporateCustomerId { get; set; }
        public string individualCustomerId { get; set; }
        public Guid? campaignId { get; set; }
        public int? segment { get; set; }

        public int? corporateType { get; set; }
        public bool checkGroupClosure { get; set; } = true;
        /// <summary>
        /// 50 --> Rental
        /// </summary>
        public int? operationType { get; set; }
        public bool processIndividualPrices_broker { get; set; } = false;
        public decimal exchangeRate { get; set; }
        public string accountGroup { get; set; }
        public int? earlistPickupTime { get; set; }

        //aylık fiyatlama
        public bool isMonthly { get; set; } = false;
        public int monthValue { get; set; } = 0;
        public DateTime month_pickupdatetime { get; set; } 
        public DateTime month_dropoffdatetime { get; set; } 
    }
}
