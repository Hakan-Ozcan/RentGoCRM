using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class IndividualCustomerData_Mobile
    {
        public Guid individualCustomerId { get; set; }
        public string customerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public DateTime birthDate { get; set; }
        public string governmentId { get; set; }
        public string emailAddress { get; set; }
        public string mobilePhone { get; set; }
        public string licensePlace { get; set; }
        public DateTime licenseDate { get; set; }
        public string licenseNumber { get; set; }
        public int? licenseClass { get; set; }
        public int? genderCode { get; set; }
        public string licenseClassName { get; set; }
        public Guid? citizenShipId { get; set; }
        public string citizenShipName{ get; set; }
        public int? segmentCode { get; set; }
        public string segmentCodeName { get; set; }
        public DateTime? expireRequestDate { get; set; }
        public DateTime? expireDate { get; set; }
        public int statusCode { get; set; }
    }
}
