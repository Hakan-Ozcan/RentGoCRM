using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class ReservationAdditionalProduct_Broker
    {
        /// <summary>
        /// Ek ürüne ait benzersiz id değeri
        /// </summary>
        public Guid productId { get; set; }
        public int value { get; set; }
        /// <summary>
        /// Ödeme tipi [ CreditCard: 20, LimitedCredit: 30, FullCredit: 40 ]
        /// </summary>
        public int? billingType { get; set; }
    }
}
