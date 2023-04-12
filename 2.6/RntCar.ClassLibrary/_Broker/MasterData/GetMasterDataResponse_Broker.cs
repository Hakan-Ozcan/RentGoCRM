using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class GetMasterDataResponse_Broker : ResponseBase
    {
        /// <summary>
        /// Grup kodu bilgisi
        /// </summary>
        public List<GroupCodeInformation_Broker> groupCodeInformation { get; set; }
        /// <summary>
        /// Ülke bilgisi
        /// </summary>
        public List<CountryData> countries { get; set; }
        /// <summary>
        /// Şehir bilgisi
        /// </summary>
        public List<CityData> cities { get; set; }
        /// <summary>
        /// Şube bilgisi
        /// </summary>
        public List<Branch_Broker> branchs { get; set; }
        /// <summary>
        /// Şubeye ait çalışma aralığı bilgisi
        /// </summary>
        public List<WorkingHour_Broker> workingHours { get; set; }
        /// <summary>
        /// Vergi dairesi bilgisi
        /// </summary>
        public List<TaxOfficeData> taxOffices { get; set; }
        /// <summary>
        /// İlçe veya bölge bilgisi
        /// </summary>
        public List<DistrictData> districts { get; set; }
    }
}
