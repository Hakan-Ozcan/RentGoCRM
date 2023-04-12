using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class CreateTransferParameter
    {
        public ContractEquipmentInformation equipmentInformation { get; set; }
        public Branch pickupBranch { get; set; }
        public Branch dropoffBranch { get; set; }
        public int estimatedPickupTimestamp { get; set; }
        public int estimatedDropoffTimestamp { get; set; }
        public string serviceName { get; set; }
        public int transferType { get; set; }
        public string description { get; set; }
        public UserInformation userInformation { get; set; }
    }
}
