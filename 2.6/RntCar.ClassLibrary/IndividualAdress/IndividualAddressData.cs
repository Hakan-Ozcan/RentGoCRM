using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualAddressData
    {
        public bool isDefaultAddress { get; set; }
        public Guid individualAddressId { get; set; }
        public string name { get; set; }
        public string countryName { get; set; }
        public Guid countryId { get; set; }
        public string cityName { get; set; }
        public Guid cityId { get; set; }
        public string districtName { get; set; }
        public Guid districtId { get; set; }
        public string addressDetail { get; set; }
    }
}
