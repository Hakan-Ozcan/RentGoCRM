using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractCustomerParameters
    {
        public Guid contactId { get; set; }
        public int customerType { get; set; }
        public Guid? corporateCustomerId { get; set; }
        public InvoiceAddressData invoiceAddress { get; set; }
    }
}
