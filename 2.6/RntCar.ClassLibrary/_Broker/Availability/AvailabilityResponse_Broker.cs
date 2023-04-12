using System.Collections.Generic;

namespace RntCar.ClassLibrary._Broker
{
    public class AvailabilityResponse_Broker : ResponseBase
    {
        /// <summary>
        /// Kullanılabilirlik verisi
        /// </summary>
        public List<AvailabilityData_Broker> availabilityData { get; set; }
        /// <summary>
        /// Fiyat id değeri
        /// </summary>
        public string trackingNumber { get; set; }
        public decimal totalDuration { get; set; }
        public decimal exchangeRate { get; set; }
        public decimal oneWayFeeAmount { get; set; } = decimal.Zero;
    }
}
