using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class StartCoinTransactionRequest
    {
        public string licenceKey { get; set; }
        public string merchantCode { get; set; }
        public string storeCode { get; set; }
        public long birdId { get; set; }
        public decimal amount { get; set; }
        public string cashDeskTag { get; set; }
        public string provisionToken { get; set; }
        public decimal billAmount { get; set; }


    }
}
