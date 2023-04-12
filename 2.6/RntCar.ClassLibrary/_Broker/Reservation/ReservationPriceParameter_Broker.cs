using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class ReservationPriceParameter_Broker
    {
        public int paymentType { get { return 20;  } set { } }
        /// <summary>
        /// Rezervasyon oluşturulacak grup koduna ait calculateReservation'dan dönen numara. Bu değer fiyatın id değerini temsil eder.
        /// </summary>
        public string trackingNumber { get; set; }
        /// <summary>
        /// Rezervasyonun ön ödemeli veya ofis ödemeli olduğu bilgisi
        /// </summary>
        public int paymentMethodCode { get; set; }
    }
}
