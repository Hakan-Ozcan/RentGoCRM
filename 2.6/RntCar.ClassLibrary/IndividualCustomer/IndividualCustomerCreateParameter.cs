using System;

namespace RntCar.ClassLibrary
{

    public class IndividualCustomerCreateParameter
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string governmentId { get; set; }
        public SelectModel dialCode { get; set; }
        public string passportNumber { get; set; }        
        public string drivingLicenseNumber { get; set; }
        public SelectModel drivingLicenseClass { get; set; }
        public DateTime drivingLicenseDate { get; set; }
        public string drivingLicensePlace { get; set; }
        public SelectModel drivingLicenseCountry { get; set; }
        public SelectModel gender { get; set; }
        public string mobilePhone { get; set; }
        public SelectModel nationality { get; set; }
        public DateTime? birthDate { get; set; }
        public int distributionChannelCode { get; set; }
        public SelectModel addressCountry { get; set; }
        public SelectModel addressCity { get; set; }
        public SelectModel addressDistrict { get; set; }
        public string addressDetail { get; set; }
        public bool isAlsoInvoiceAddress { get; set; } = true;
        public bool isTurkishCitizen { get; set; }
        public int? findeksPoint { get; set; }
        public bool isCallCenter { get; set; }
        public bool? markettingPermissions { get; set; }
        public IndividualCustomerCreateInvoiceAddressParameter invoiceAddress { get; set; }

    }
}
