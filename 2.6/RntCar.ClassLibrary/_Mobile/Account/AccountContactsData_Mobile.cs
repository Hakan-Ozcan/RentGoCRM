using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile.Account
{
    public class AccountContactsData_Mobile
    {
        public string relation { get; set; }
        public Guid individualCustomerId { get; set; }
        public string citizenship { get; set; }
        public string governmentId { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public DateTime birthdate { get; set; }
        public string emailAddress { get; set; }
        public string phoneNumber { get; set; }
        public string drivingLicenseClassName { get; set; }
        public DateTime drivingLicenseDate { get; set; }
        public string drivingLicenseNumber { get; set; }
        public string drivingLicensePlace { get; set; }
        public string city { get; set; }
        public string addressDetail { get; set; }
        public List<IndividualAddressData_Mobile> individualAddressDatas { get; set; }
    }
}
