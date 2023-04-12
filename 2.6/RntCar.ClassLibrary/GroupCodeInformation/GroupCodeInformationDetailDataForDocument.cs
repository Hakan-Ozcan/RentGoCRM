using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class GroupCodeInformationDetailDataForDocument
    {
        public bool doubleCreditCard { get; set; }
        public int findeks { get; set; }
        public int minimumAge { get; set; }
        public int minimumDriverLicense { get; set; }
        public int youngDriverAge { get; set; }
        public int youngDriverLicense { get; set; }
        public decimal overKilometerPrice { get; set; }
        public decimal depositAmount { get; set; }
        public int segmentCode { get; set; }
        public List<Guid> upgradeGroupCodes { get; set; }
}
}
