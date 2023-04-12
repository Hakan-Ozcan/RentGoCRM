using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class GetCustomerInvoicesResponse_Web : ResponseBase
    {
        public List<CustomerInvoices_Web> customerInvoices { get; set; }
    }
}
