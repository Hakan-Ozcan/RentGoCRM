using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary._Mobile
{
    public class GroupCodeInformation_Mobile
    {
        public string groupCodeName { get; set; }
        public Guid groupCodeId { get; set; }
        public string groupCodeDescription { get; set; }
        public string transmissionName { get; set; }
        public string fuelTypeName { get; set; }
        public int? findeksPoint { get; set; }
        public bool isDoubleCard { get; set; }
        public int minimumAge { get; set; }
        public int minimumDriverLicense { get; set; }
        public int youngDriverAge { get; set; }
        public int youngDriverMinimumLicense { get; set; }
        public decimal? depositAmount { get; set; }
        public int transmission { get; set; }
        public int fuelType { get; set; }
        public int segment { get; set; }
        public string segmentName { get; set; }
        public string showRoomBrandName { get; set; }
        public string showRoomModelName { get; set; }
        public string SIPPCode { get; set; }
    }
}
