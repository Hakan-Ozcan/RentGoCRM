using System;
namespace RntCar.ClassLibrary
{
    public class IndividualCustomerContractRelationData
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string governmentId { get; set; }
        public string dialCode { get; set; }
        public string passportNumber { get; set; }
        public string drivingLicenseNumber { get; set; }
        public int? drivingLicenseClass { get; set; }
        public DateTime drivingLicenseDate { get; set; }
        public string drivingLicensePlace { get; set; }
        public string mobilePhone { get; set; }
        public Guid? nationalityId { get; set; }
        public DateTime birthDate { get; set; }
        public int findeksPoint { get; set; }        
        public bool isTurkishCitizen { get; set; }
        public Guid individualCustomerId { get; set; }
    }
}
