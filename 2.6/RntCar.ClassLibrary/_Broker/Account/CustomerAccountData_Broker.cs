using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary._Broker
{
    public class CustomerAccountData_Broker
    {
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
        public string brokerCode { get; set; }
        public double balance { get; set; }
    }
}
