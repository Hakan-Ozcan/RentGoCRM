using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualAddresUpdateParameters
    {
        public Guid addressCountryId { get; set; }
        public Guid? addressCityId { get; set; }
        public Guid? addressDistrictId { get; set; }
        public string addressDetail { get; set; }
        public Guid individualAddressId { get; set; }
    }
}
