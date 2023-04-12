using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractSearchParameters
    {
        public string contractNumber { get; set; }
        public Guid? pickupBranchId { get; set; }
        public DateTime? pickupDate { get; set; }
        public Guid? dropoffBranchId { get; set; }
        public DateTime? dropoffDate { get; set; }
        public string customerId { get; set; }
        public string plateNumber { get; set; }

    }
}
