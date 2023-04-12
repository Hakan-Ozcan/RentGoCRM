using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerCreateResponse: ResponseBase
    {
        public string CustomerId { get; set; }
        public string IndividualAddressId { get; set; }
        public string InvoiceAddressId { get; set; }

    }
}
