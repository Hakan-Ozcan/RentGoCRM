using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class InvoiceInformation
    {
        public int type { get; set; }
        public string metarialCode { get; set; }
        public string metarialDescription { get; set; }
        public string unitCode { get; set; }
        public double unitPrice { get; set; }
        public int quantity { get; set; }
        public int vatRate { get; set; }
        public string description { get; set; }
        public string plateNumber { get; set; }
        public string vatExceptCode { get; set; }
        public string vatExceptReason { get; set; }

    }
}
