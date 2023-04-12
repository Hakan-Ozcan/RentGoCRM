using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class InvoiceAddressData
    {
        public string addressDetail { get; set; }
        public Guid individualCustomerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string governmentId { get; set; }
        public string companyName { get; set; }
        public string taxNumber { get; set; }
        public Guid? taxOfficeId { get; set; }
        public int invoiceType { get; set; }
        public Guid addressCountryId { get; set; }
        public string addressCountryName { get; set; }
        public Guid? addressCityId { get; set; }
        public string addressCityName { get; set; }
        public Guid? addressDistrictId { get; set; }
        public string addressDistrictName{ get; set; }
        public Guid? invoiceAddressId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string mobilePhone { get; set; }
        public bool defaultInvoice { get; set; }
    }
}
