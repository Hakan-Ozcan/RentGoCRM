using System;

namespace RntCar.ClassLibrary._Web
{
    public class IndividualAddressData_Web
    {
        public bool isDefaultAddress { get; set; }
        public Guid individualAddressId { get; set; }
        public string name { get; set; }
        public string countryName { get; set; }
        public Guid? countryId { get; set; }
        public string cityName { get; set; }
        public Guid? cityId { get; set; }
        public string districtName { get; set; }
        public Guid? districtId { get; set; }
        public string addressDetail { get; set; }
    }
}
