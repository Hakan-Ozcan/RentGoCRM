using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class AdditionalProductParameters_Broker : RequestBase
    {
        /// <summary>
        /// Şube ve tarih parametreleri
        /// </summary>
        public QueryParameters queryParameters { get; set; }
        /// <summary>
        /// Grup koda ait benzersiz id değeri
        /// </summary>
        public Guid? groupCodeId { get; set; }
        public string brokerCode { get; set; }
    }
}
