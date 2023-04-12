using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractData
    {
        public DateTime PickupDatetime { get; set; }
        public DateTime DropoffDatetime { get; set; }
        public Guid PickupBranchId { get; set; }
        public Guid DropoffBranchId { get; set; }
        public Guid GroupCodeId { get; set; }
        public int StatusCode { get; set; }
    }
}
