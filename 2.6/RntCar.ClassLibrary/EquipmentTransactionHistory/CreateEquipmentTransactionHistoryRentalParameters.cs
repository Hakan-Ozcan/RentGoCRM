using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreateEquipmentTransactionHistoryRentalParameters
    {
        public int rentalKmValue { get; set; }
        public int firstKmValue { get; set; }
        public Guid? transferId { get; set; }
        public Guid? contractId { get; set; }
        public Guid? equipmentId { get; set; }
        public int rentalFuelValue { get; set; }
        public int firstFuelValue { get; set; }
        public Guid? equipmentTransactionId { get; set; }
    }
}
