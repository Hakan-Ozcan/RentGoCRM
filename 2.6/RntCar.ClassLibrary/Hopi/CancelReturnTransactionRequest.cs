using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CancelReturnTransactionRequest
    {
        public string licenceKey { get; set; }
        public string merchantCode { get; set; }
        public string storeCode { get; set; }
        public string returnTrxId { get; set; }
    }
}
