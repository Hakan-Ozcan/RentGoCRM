using System;

namespace RntCar.ClassLibrary
{
    public class DocumentObject
    {
        public string downloadURL { get; set; }
        public string ContractNumber { get; set; }
        public Guid contractId { get; set; }
        public string customerName { get; set; }
        public string pnrNumber { get; set; }
        public Guid pickupBranchId { get; set; }
    }
}
