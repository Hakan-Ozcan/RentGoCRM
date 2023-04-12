using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class ReservationCustomerParameters_Broker
    {
        /// <summary>
        /// Broker için ayrılan benzersiz kod. Entegrasyon sırasında size ait kodu talep ediniz
        /// </summary>
        public string brokerCode { get; set; }
        /// <summary>
        /// Sürücü bilgileri
        /// </summary>
        public DummyContactData dummyContactData { get; set; }
    }
}
