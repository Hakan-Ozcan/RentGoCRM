using System;

namespace RntCar.ClassLibrary._Web
{
    public class AvailabilityParameters_Web : RequestBase
    {
        public QueryParameters queryParameters { get; set; }
        public Guid? individualCustomerId { get; set; }
        public Guid? reservationId { get; set; }
        public int? segmentCode { get; set; }
        public Guid priceCodeId { get; set; }
        public Guid? corporateCustomerId { get; set; }
        public Guid? campaignId { get; set; }
        public bool? isMonthly { get; set; }
        public int monthValue { get; set; }
    }
}
