using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetTransfersByBranchData
    {
        public Guid transferId { get; set; }
        public string transferNumber { get; set; }
        public EquipmentData selectedEquipment { get; set; }
        public Branch pickupBranch { get; set; }
        public Branch dropoffBranch { get; set; }
        public long? pickupTimestamp { get; set; }
        public long? dropoffTimestamp { get; set; }
        public string serviceName { get; set; }
        public int transferType { get; set; }
        public int statusCode { get; set; }
    }
}
