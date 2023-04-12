using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class ReservationCreateParameters_Broker : RequestBase
    {
        /// <summary>
        /// Müşteri bilgileri
        /// </summary>
        public ReservationCustomerParameters_Broker reservationCustomerParameters { get; set; }
        /// <summary>
        /// Şube ve tarih parametreleri
        /// </summary>
        public QueryParameters reservationQueryParameters { get; set; }
        /// <summary>
        /// Grup kod bilgisi
        /// </summary>
        public ReservationEquimentParameters_Broker reservationEquimentParameters { get; set; }
        /// <summary>
        /// Fiyat bilgisi
        /// </summary>
        public ReservationPriceParameter_Broker reservationPriceParameters { get; set; }
        /// <summary>
        /// Rezervasyon ek ürün bilgisi
        /// </summary>
        public List<ReservationAdditionalProduct_Broker> reservationAdditionalProducts { get; set; }
    }
}
