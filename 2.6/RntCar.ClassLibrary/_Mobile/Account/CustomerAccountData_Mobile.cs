using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class CustomerAccountData_Mobile
    {
        public string relation { get; set; }
        public string accountName { get; set; }
        public Guid accountId { get; set; }
        public string accountType { get; set; }
        public string addressDetail { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string district { get; set; }
        public List<int> paymentMethodCode { get; set; }
        public string priceCode { get; set; }
        public Guid priceCodeId { get; set; }
        public string taxNumber { get; set; }
        public string taxOffice { get; set; }
        public string phoneNumber { get; set; }
        public string emailAddress { get; set; }
    }
}
