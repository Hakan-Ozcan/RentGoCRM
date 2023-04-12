using System;

namespace RntCar.ClassLibrary.MongoDB
{
    public class CorporateCustomerData
    {
        public string corporateCustomerId { get; set; }
        public string name { get; set; }
        public int accountTypeCode { get; set; }
        public string brokerCode { get; set; }
        public Guid priceCodeId { get; set; }
        public Guid monthlyPriceCodeId { get; set; }
        public int? priceFactorGroupCode { get; set; } = null;
        public bool? processIndividualPrices { get; set; }
        public int statecode { get; set; }
        public int statuscode { get; set; }
        public decimal creditlimit { get; set; }
    }
}
