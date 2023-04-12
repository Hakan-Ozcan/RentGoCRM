using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerUpdateParameters
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string governmentId { get; set; }
        public SelectModel dialCode { get; set; }
        public string passportNumber { get; set; }
        public DateTime passportIssueDate { get; set; }
        public string drivingLicenseNumber { get; set; }
        public SelectModel drivingLicenseClass { get; set; }
        public DateTime drivingLicenseDate { get; set; }
        public string drivingLicensePlace { get; set; }
        public SelectModel drivingLicenseCountry { get; set; }
        public SelectModel gender { get; set; }
        public string mobilePhone { get; set; }
        public SelectModel nationality { get; set; }
        public DateTime birthDate { get; set; }
        public int? distributionChannelCode { get; set; }
        public SelectModel addressCountry { get; set; }
        public SelectModel addressCity { get; set; }
        public SelectModel addressDistrict { get; set; }
        public string addressDetail { get; set; }
        public string password { get; set; }
        public bool isTurkishCitizen { get; set; }
        public Guid? individualAddressId { get; set; }
        public Guid? individualCustomerId { get; set; }
        public int findeksPoint { get; set; }
        public bool isCallCenter { get; set; }
        public bool? markettingPermissions { get; set; }
    }
}
