using System;

namespace RntCar.ClassLibrary._Web.IndividualCustomer
{
    public class IndividualCustomerMarketingPermissionData_Web
    {
        public int? permissionChannel { get; set; }
        public bool allowSMS { get; set; }
        public bool allowEmail { get; set; }
        public bool allowNotification { get; set; }
        public bool isTurkishCitizen { get; set; }
        public int? segmentCode { get; set; }
        public string identityKey { get; set; }
        public string mobilePhone { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string customerNumber { get; set; }
        public string emailaddress { get; set; }
        public int? genderCode { get; set; }
        public int findeksPoint { get; set; }
        public DateTime? birthDate { get; set; }
        public DateTime? licenseDate { get; set; }
        public DateTime? permissionDate { get; set; }
        public DateTime createdon { get; set; }
        public string lastRentalOffice { get; set; }
    }
}
