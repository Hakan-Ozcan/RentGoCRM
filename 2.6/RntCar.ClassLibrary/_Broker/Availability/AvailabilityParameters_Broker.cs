using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class AvailabilityParameters_Broker : RequestBase
    {
        /// <summary>
        /// Şube ve tarih parametreleri
        /// </summary>
        public QueryParameters queryParameters { get; set; }
        /// <summary>
        /// Broker için ayrılan benzersiz kod. Entegrasyon sırasında size ait kodu talep ediniz
        /// </summary>
        public string brokerCode { get; set; }
    }
}
