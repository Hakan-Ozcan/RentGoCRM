using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class AvailabilityParameters_Mobile : RequestBase
    {
        public QueryParameters queryParameters { get; set; }
        public Guid? individualCustomerId { get; set; }
        public Guid? reservationId { get; set; }
        public int? segmentCode { get; set; }
        public Guid priceCodeId { get; set; }
        public Guid? corporateCustomerId { get; set; }
        public Guid? campaignId { get; set; }
    }
}
