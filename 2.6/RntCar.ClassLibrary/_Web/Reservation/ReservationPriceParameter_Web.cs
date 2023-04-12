using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class ReservationPriceParameter_Web
    {
        public int paymentChoice { get; set; }
        public string trackingNumber { get; set; }
        public string pricingType { get; set; }
        public Guid? campaignId { get; set; }
        public int installment { get; set; } = 1;
        public decimal installmentAmount { get; set; }
        public CreditCardData_Web customerCreditCard { get; set; }
        public bool? isMonthly { get; set; }
        public bool use3DSecure { get; set; }
        public string callBackUrl { get; set; }
    }
}
