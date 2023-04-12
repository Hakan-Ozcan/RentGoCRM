using System;

namespace RntCar.ClassLibrary._Broker
{
    public class AvailabilityData_Broker
    {
        /// <summary>
        /// Grup kod adı
        /// </summary>
        public string groupCodeName { get; set; }
        /// <summary>
        /// Grup koda ait benzersiz id değeri
        /// </summary>
        public Guid groupCodeId { get; set; }
        /// <summary>
        /// Doluluk oranı
        /// </summary>
        public decimal ratio { get; set; }
        /// <summary>
        /// İlgili grup kodun fiyatı
        /// </summary>
        public decimal payAmount { get; set; }
        public decimal dailyAmount { get; set; }
        public int kmLimit { get; set; }
        
    }
}
