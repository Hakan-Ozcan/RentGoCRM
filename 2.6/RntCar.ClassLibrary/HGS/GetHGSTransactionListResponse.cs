using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class GetHGSTransactionListResponse : ResponseBase
    {
        public List<HGSTransactionData> transactions { get; set; }
        public bool showErrorMessage { get; set; } = false;
    }
}
