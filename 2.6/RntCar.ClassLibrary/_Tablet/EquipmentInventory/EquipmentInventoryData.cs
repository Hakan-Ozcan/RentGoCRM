using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class EquipmentInventoryData
    {
        public bool? isExist { get; set; }
        public string inventoryName { get; set; }
        public string logicalName { get; set; }
        public Guid? equipmentInventoryId { get; set; }
        public decimal price { get; set; }
    }
}
