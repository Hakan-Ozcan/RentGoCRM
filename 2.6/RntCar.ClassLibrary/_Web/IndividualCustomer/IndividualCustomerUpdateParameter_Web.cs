using System;

namespace RntCar.ClassLibrary._Web
{
    public class IndividualCustomerUpdateParameter_Web : RequestBase
    {
        public Guid individualCustomerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string emailAddress { get; set; }
        public string governmentId { get; set; }
        public string licenseNumber { get; set; }
        public int licenseClass { get; set; }
        public string licenseDate { get; set; }
        public string licensePlace { get; set; }
        public int genderCode { get; set; }
        public string birthDate { get; set; }
        public Guid citizenShipId { get; set; }
        public string mobilePhone { get; set; }
        public string dialCode { get; set; }
        public string password { get; set; }
        public bool allowMarketingPermissions { get; set; }
        public IndividualAddressData_Web individualAddressInformation { get; set; }
    }
}
