using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class AvailabilityData
    {
        public string groupCodeName { get; set; }
        public Guid groupCodeId { get; set; }
        public string displayText { get; set; }
        public string transmissionName { get; set; }
        public string fuelTypeName { get; set; }
        public string groupCodeImage { get; set; }
        public decimal totalPrice { get; set; }
        public decimal amountToBePaid { get; set; }
        public bool isUpgrade { get; set; }
        public bool isDowngrade { get; set; }
        public List<Guid> upgradeGroupCodes { get; set; }
        public bool isUpsell { get; set; }
        public bool isDownsell { get; set; }
        public int changeType { get; set; }
        public string trackingNumber { get; set; }
    }
}
