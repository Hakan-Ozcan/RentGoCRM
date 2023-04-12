using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class GroupCodeInformationDetailData
    {
        public decimal? deposit { get; set; }
        public int? findeks { get; set; }
        public string carModelImage { get; set; }
        public int? minimumAge { get; set; }
        public int? minimumDriverLicense { get; set; }
        public int? segment { get; set; }
        public string showRoomBrandName { get; set; }       
        public Guid? showRoomBrandId { get; set; }
        public string showRoomModelName { get; set; }
        public Guid? showRoomModelId{ get; set; }
        public int? youngDriverAge { get; set; }
        public int? youngDriverMinimumLicense { get; set; }
        public Guid groupCodeInformationId { get; set; }
        public string groupCodeInformationName { get; set; }
        public string gearboxcodeName { get; set; }
        public string fueltypecodeName { get; set; }
        public int gearboxcode{ get; set; }
        public int fueltypecode { get; set; }
        public string image { get; set; }
        public decimal overKilometerPrice { get; set; }
        public bool isDoubleCard { get; set; }
        public string groupCodeDescription { get; set; }
        public string webImageURL { get; set; }
        public string mobileImageURLs { get; set; }
        public int stateCode { get; set; }
        public string colorName { get; set; }
        public string engineVolume { get; set; }
        public string SIPPCode { get; set; }
    }
}
