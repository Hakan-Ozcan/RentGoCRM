using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class StartReturnTransactionResponse : ResponseBase
    {
        public decimal residual { get; set; }
        public ulong returnTrxId { get; set; }
    }
}
