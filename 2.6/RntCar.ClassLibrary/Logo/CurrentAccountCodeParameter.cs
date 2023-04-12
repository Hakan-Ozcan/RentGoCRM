using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CurrentAccountCodeParameter
    {
        public string tckn { get; set; }
        public string taxNo { get; set; }
        public string customerFirstName { get; set; }
        public string customerLastName { get; set; }
        public string title { get; set; }
        public string address { get; set; }
        public string address2 { get; set; }
        public string mobilePhone { get; set; }
        public string email { get; set; }
        public string city { get; set; }
        public string town { get; set; }
        public string country { get; set; }
        public string taxOffice { get; set; }
        public string einvoiceEmail { get; set; }
        public string corporateType { get; set; }
        public List<string> paymentMethods { get; set; }
    }
}
