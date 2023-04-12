using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class GetHGSTransactionListParameter
    {
        public string productId { get; set; }
        public string plateNo { get; set; }
        public long startDateTimeStamp { get; set; }
        public long finishDateTimeStamp { get; set; }
    }
}
