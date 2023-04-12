using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class GetMasterDataResponse_Web : ResponseBase
    {
        public List<GroupCodeInformation_Web> groupCodeInformation { get; set; }
        public List<CountryData> countries { get; set; }
        public List<CityData> cities { get; set; }
        public List<Branch_Web> branchs { get; set; }
        public List<WorkingHour_Web> workingHours { get; set; }
        public List<TaxOfficeData> taxOffices { get; set; }
        public List<DistrictData> districts { get; set; }
        public List<ShowRoomProduct_Web> showRoomProducts { get; set; }
        public List<AdditionalProductRule_Web> additionalProductRules { get; set; }

    }
}
