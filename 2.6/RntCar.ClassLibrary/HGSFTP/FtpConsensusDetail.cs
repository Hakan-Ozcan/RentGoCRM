using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class FtpConsensusDetail
    {
        public string rowDetail { get; set; }
        public string exceptionDetail { get; set; }
        public int fileRow { get; set; }
        public decimal amount { get; set; }
        public int indexOf { get; set; }
        public string transactionId { get; set; }
        public bool isException { get; set; }
    }
}
