using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class ContractRetrieveResponse : ResponseBase
    {
        public List<AdditionalProductData> additionalProducts { get; set; }
        public GroupCodeInformationDetailData groupCodeInformation { get; set; }
        public List<AdditionalDriverData> additionalDrivers { get; set; }
        public int contractKilometerLimit { get; set; }
        public int paymentMethod { get; set; }
        public int contractType { get; set; }
        public Guid contactId { get; set; }
        public Guid priceGroupCodeId { get; set; }
        public bool isMonthly { get; set; }
        public int howManyMonths { get; set; }
        public bool canContinueMonthly { get; set; }
    }
}
