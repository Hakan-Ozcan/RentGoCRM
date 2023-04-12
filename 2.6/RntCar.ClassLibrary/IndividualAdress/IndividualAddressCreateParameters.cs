using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualAddressCreateParameters
    {
        public Guid addressCountryId { get; set; }
        public Guid? addressCityId { get; set; }
        public Guid? addressDistrictId { get; set; }
        public string addressDetail { get; set; }
        public Guid individualCustomerId { get; set; }

    }
}
