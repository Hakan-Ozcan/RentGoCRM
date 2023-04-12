using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class UpdateContractEquipmentPrices
    {
        public string oldTrackingNumber { get; set; }
        public string newTrackingNumber { get; set; }
        public decimal oldEquipmentPrice { get; set; }
        public decimal newEquipmentPrice { get; set; }
    }
}
