using System;

namespace RntCar.ClassLibrary
{
    public class CalculateReservationRuleResponse
    {
        public int findeks { get; set; }
        public int paymentMethodCode { get; set; }
        public decimal depositAmount { get; set; }
        public bool processCustomerPrices { get; set; }
    }
    public class CalculateReservationRuleParameters
    {
        public GroupCodeInformationDetailDataForDocument groupCodeInformationDetailDataForDocument { get; set; }
        public int? customerType { get; set; }
        public string pricingType { get; set; }
        public Guid? corporateId { get; set; }
        public Guid contactId { get; set; }
    }
}
