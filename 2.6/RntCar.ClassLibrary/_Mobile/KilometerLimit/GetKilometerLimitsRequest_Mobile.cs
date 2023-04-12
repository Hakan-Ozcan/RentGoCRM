using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetKilometerLimitsRequest_Mobile : RequestBase
    {
        public DateTime pickupDateTime { get; set; }
        public DateTime dropoffDateTime { get; set; }
    }
}
