using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ComparePlateNumberResponse : ResponseBase 
    {
        public List<EquipmentItem> equipmentItemsMongo { get; set; }
        public List<EquipmentItem> equipmentItemsCrm { get; set; }
        public List<EquipmentItem> equipmentItemsCompared { get; set; }
    }

    public class EquipmentItem
    {
        public string plateNumber { get; set; }
        public int statusCode { get; set; }
        public string statusName { get; set; }
        public Guid currentBranchId { get; set; }
    }
}
