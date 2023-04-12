using System;

namespace RntCar.ClassLibrary._Web
{
    public class GetKilometerLimitsRequest_Web : RequestBase
    {
        public DateTime pickupDateTime { get; set; }
        public DateTime dropoffDateTime { get; set; }
    }
}
