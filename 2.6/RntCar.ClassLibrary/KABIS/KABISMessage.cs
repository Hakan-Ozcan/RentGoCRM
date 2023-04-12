using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class KABISMessage
    {
        public KABISProcessType KabisPeocessType { get; set; }
        public string contractNumber { get; set; }
        public string plateNumber { get; set; }
        public string office { get; set; }
        public string code { get; set; }
        public string message { get; set; }
        public string officeMail { get; set; }
    }

    public enum KABISProcessType
    {
        Rental = 1,
        Complete = 2,
        Transfer = 3
    }
}
