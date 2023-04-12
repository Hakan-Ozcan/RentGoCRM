using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerCreateInvoiceAddressParameter
    {
        public string addressDetail { get; set; }
        public string individualCustomerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string governmentId { get; set; }
        public string companyName { get; set; }
        public string taxNumber { get; set; }
        public string taxOfficeId { get; set; }
        public int? invoiceType { get; set; }
        public string addressCountryId { get; set; }
        public string addressCityId { get; set; }
        public string addressDistrictId { get; set; }
        public string invoiceAddressId { get; set; }
        public string invoiceName { get; set; }
    }
}
