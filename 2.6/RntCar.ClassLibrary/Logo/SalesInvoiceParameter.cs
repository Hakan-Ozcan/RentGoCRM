using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class SalesInvoiceParameter
    {
        public string currentAccountCode { get; set; }
        public string documentInvoiceNo { get; set; }
        public string documentNumber { get; set; }
        public string invoiceDate { get; set; }
        public int warehouse { get; set; }
        public List<string> notes { get; set; }// max 4 
        public string tckn { get; set; }
        public string taxNo { get; set; }
        public int division { get; set; }
        public string projectCode { get; set; }
        public List<InvoiceInformation> invoiceInformationList { get; set; }
        public string currentAccountCodeShpm { get; set; }
    }
}
