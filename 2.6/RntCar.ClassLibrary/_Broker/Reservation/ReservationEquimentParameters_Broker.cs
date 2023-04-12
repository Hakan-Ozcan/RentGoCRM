using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class ReservationEquimentParameters_Broker
    {
        /// <summary>
        /// Grup koda ait benzersiz id değeri
        /// </summary>
        public Guid groupCodeId { get; set; }
        /// <summary>
        /// Ödeme tipi [ CreditCard: 20, LimitedCredit: 30, FullCredit: 40 ]
        /// </summary>
        public int? billingType { get; set; }
    }
}
