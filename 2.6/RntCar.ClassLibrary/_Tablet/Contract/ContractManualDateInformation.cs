using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class ContractManualDateInformation
    {
        public Guid contractId { get; set; }
        public Guid contactId { get; set; }
        public Branch dropoffBranch { get; set; }
        public long manuelPickupDateTimeStamp { get; set; }
        public long manuelDropoffTimeStamp { get; set; }
        public long PickupDateTimeStamp { get; set; }
        public long DropoffTimeStamp { get; set; }
        public bool isManuelProcess { get; set; }
    }
}
