using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class EquipmentData
    {
        public Guid brandId { get; set; }
        public string brandName { get; set; }
        public string hgsNumber { get; set; }
        public Guid modelId { get; set; }
        public Guid groupCodeId { get; set; }
        public string groupCodeName { get; set; }
        public string modelName { get; set; }
        public string plateNumber { get; set; }
        public int equipmentColor { get; set; }
        public int seatNumber { get; set; }
        public int doorNumber { get; set; }
        public int luggageNumber { get; set; }
        public int fuelValue { get; set; }
        public int kmValue { get; set; }
        public Guid equipmentId { get; set; }
        public int statusReason { get; set; }
    }
}
