using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreditCardSlipParameter
    {
        public DateTime paymentCreatedon { get; set; }
        public int division { get; set; }
        public string pnrNumber { get; set; } // eq note 1
        public string documentNumber { get; set; } // eq note 2
        public string currentAccountCode { get; set; }
        public string bankCode { get; set; }
        public string projectCode { get; set; }
        public string documentPaymentResultId { get; set; }
        public string description { get; set; }
        public decimal credit { get; set; }
        public string approveNumber { get; set; }
        public bool paymentType { get; set; } // true : refund - false : sale
        public int installment { get; set; } // true : refund - false : sale
    }
}
