using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class EquipmentAvailability
    {
        public Guid GroupCodeId { get; set; }
        public string GroupCode { get; set; }
        public string CurrentBranchId { get; set; }
        public string CurrentBranch { get; set; }
        public bool IsFranchise { get; set; }
        public int StatusCode { get; set; }
        public int TransferType { get; set; }
    }
}
