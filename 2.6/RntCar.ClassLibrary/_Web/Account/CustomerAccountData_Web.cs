using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class CustomerAccountData_Web
    {
        public string relation { get; set; }
        public string accountName { get; set; }
        public Guid accountId { get; set; }
        public string accountType { get; set; }
        public string addressDetail { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string district { get; set; }
        public List<string> paymentMethodCode { get; set; }
        public string priceCode { get; set; }
        public Guid priceCodeId { get; set; }
        public string taxNumber { get; set; }
        public string taxOffice { get; set; }
        public string phoneNumber { get; set; }
        public string emailAddress { get; set; }
    }
}
