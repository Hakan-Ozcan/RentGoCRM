using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractEquipmentParameters
    {
        public Guid groupCodeInformationId { get; set; }
        public string groupCodeInformationName { get; set; }
        public int segment { get; set; }
        public decimal depositAmount { get; set; }
        public bool isEquipment { get; set; }
        public string itemName { get; set; }
    }
}
