using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GroupCodeInformation
    {
        public string groupCodeName { get; set; }
        public Guid groupCodeId { get; set; }
        public Guid pricingGroupCodeId { get; set; }
        public string groupCodeDescription { get; set; }
        public string transmissionName { get; set; }
        public string fuelTypeName { get; set; }
        public string groupCodeImage { get; set; }
        public decimal totalPrice { get; set; }
        public bool isDoubleCard { get; set; }
        public decimal? depositAmount { get; set; }       
    }
}
