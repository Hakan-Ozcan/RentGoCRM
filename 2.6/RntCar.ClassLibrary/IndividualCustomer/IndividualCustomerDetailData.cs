using System;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerDetailData
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string governmentId { get; set; }
        public string dialCode { get; set; }
        public string passportNumber { get; set; }
        public DateTime passportIssueDate { get; set; }
        public string drivingLicenseNumber { get; set; }
        public int? drivingLicenseClass { get; set; }
        public DateTime drivingLicenseDate { get; set; }
        public string drivingLicensePlace { get; set; }
        public Guid? drivingLicenseCountry { get; set; }
        public int? gender { get; set; }
        public string mobilePhone { get; set; }
        public Guid? nationalityId { get; set; }
        public string nationalityIdName { get; set; }
        public DateTime birthDate { get; set; }
        public int findeksPoint { get; set; }
        public int distributionChannelCode { get; set; }
        public string addressCountryId { get; set; }
        public string addressCountry{ get; set; }
        public string addressCityId { get; set; }
        public string addressCity{ get; set; }
        public string addressDistrictId { get; set; }
        public string addressDistrict{ get; set; }
        public string addressDetail { get; set; }
        public string corporateCustomerId { get; set; }
        public string pricingType { get; set; }
        public bool isAlsoInvoiceAddress { get; set; }
        public bool isTurkishCitizen { get; set; }
        public bool isAdditionalDriver { get; set; } = false;
        public Guid? individualAddressId { get; set; }
        public Guid individualCustomerId { get; set; }
        public int customerSegment { get; set; }

    }
}
