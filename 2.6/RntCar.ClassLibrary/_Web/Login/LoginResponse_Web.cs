using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class LoginResponse_Web : ResponseBase
    {
        public MarketingPermission_Web marketingPermission { get; set; }
        public IndividualCustomerData_Web customerInformation { get; set; }
        public List<IndividualAddressData_Web> individualAddressInformation { get; set; }
        public List<InvoiceAddressData_Web> invoiceAddressInformation { get; set; }
        public List<CreditCardData_Web> customerCreditCards { get; set; }
        public List<CustomerAccountData_Web> customerAccounts { get; set; }
    }
}
