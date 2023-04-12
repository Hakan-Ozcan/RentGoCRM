using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class GetMasterDataRequest_Broker : RequestBase
    {
        /// <summary>
        /// Broker için ayrılan benzersiz kod. Entegrasyon sırasında size ait kodu talep ediniz
        /// </summary>
        public string brokerCode { get; set; }
    }
}
