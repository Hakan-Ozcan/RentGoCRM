using System.Collections.Generic;

namespace RntCar.ClassLibrary._Mobile
{
    public class LoginResponse_Mobile : ResponseBase
    {
        public MarketingPermission_Mobile marketingPermission { get; set; }
        public IndividualCustomerData_Mobile customerInformation { get; set; }
        public List<IndividualAddressData_Mobile> individualAddressInformation { get; set; }
        public List<InvoiceAddressData_Mobile> invoiceAddressInformation { get; set; }
        public List<CreditCardData_Mobile> customerCreditCards { get; set; }
        public List<CustomerAccountData_Mobile> customerAccounts { get; set; }
    }
}
