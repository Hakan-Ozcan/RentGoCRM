using RntCar.ClassLibrary._Mobile.AdditionalProductRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetMasterDataResponse_Mobile : ResponseBase
    {
        public List<GroupCodeInformation_Mobile> groupCodeInformation { get; set; }
        public List<GroupCodeImage_Mobile> groupCodeImages { get; set; }
        public List<CountryData> countries { get; set; }
        public List<CityData> cities { get; set; }
        public List<Branch_Mobile> branchs { get; set; }
        public List<WorkingHour_Mobile> workingHours { get; set; }
        public List<TaxOfficeData> taxOffices { get; set; }
        public List<DistrictData> districts { get; set; }
        public List<ShowRoomProduct_Mobile> showRoomProducts { get; set; }
        public List<AdditionalProductRule_Mobile> additionalProductRules { get; set; }

    }
}
