using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class InvoiceAddressData_Broker
    {
        public int invoiceTypeCode { get; set; }
        public Guid invoiceAddressId { get; set; }
        public string name { get; set; }
        public string countryName { get; set; }
        public Guid? countryId { get; set; }
        public string cityName { get; set; }
        public Guid? cityId { get; set; }
        public string districtName { get; set; }
        public Guid? districtId { get; set; }
        public string addressDetail { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string companyName { get; set; }
        public string governmentId { get; set; }
        public string taxNumber { get; set; }
        public Guid? taxOfficeId { get; set; }
        public string taxOfficeName { get; set; }
    }
}
