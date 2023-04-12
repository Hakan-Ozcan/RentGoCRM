using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreateEquipmentTransactionHistoryDeliveryParameters
    {
        public int deliveryKmValue { get; set; }
        public int kmValue { get; set; }
        public Guid? transferId { get; set; }
        public Guid? contractId { get; set; }
        public Guid? equipmentId { get; set; }
        public int deliveryFuelValue { get; set; }
        public int fuelValue { get; set; }

    }
}
