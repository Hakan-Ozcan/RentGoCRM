using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web.Tegsoft
{
    public class CardInfo
    {
        public string GovernmentId { get; set; }
        public string CardNumber { get; set; }
        public string FullName { get; set; }
        public int ExpireDateMonth { get; set; }
        public int ExpireDateYear { get; set; }
        public int Cvv { get; set; }
    }
}
