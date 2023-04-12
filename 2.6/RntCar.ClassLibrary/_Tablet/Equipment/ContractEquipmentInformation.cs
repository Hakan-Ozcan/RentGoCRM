using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class ContractEquipmentInformation
    {
        public Guid equipmentId { get; set; }
        public Guid groupCodeInformationId { get; set; }
        public string plateNumber { get; set; }
        public int currentKmValue { get; set; }
        public int firstKmValue { get; set; }
        public int currentFuelValue { get; set; }
        public int firstFuelValue { get; set; }
        public bool isEquipmentChanged { get; set; }
        public List<EquipmentInventoryData> equipmentInventoryData { get; set; }
    }
}
