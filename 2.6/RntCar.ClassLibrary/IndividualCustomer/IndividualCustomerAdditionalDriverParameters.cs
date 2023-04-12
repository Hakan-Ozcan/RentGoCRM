using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerAdditionalDriverParameters
    {
        public Guid? reservationId { get; set; }
        public Guid? contractId { get; set; }
        public Guid? contactId { get; set; }
        public Guid documentContactId { get; set; }
        public Guid nationalityId { get; set; }
        public Guid? additionalDriverId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string governmentId { get; set; }
        public string passportNumber { get; set; }
        public string mobilePhone { get; set; }
        public string dialCode { get; set; }
        public DateTime birthDate { get; set; }
        public DateTime drivingLicenseDate { get; set; }
        public DateTime pickupDateTime { get; set; }
        public DateTime dropoffDateTime { get; set; }
        public bool isTurkishCitizen { get; set; }
    }
}
