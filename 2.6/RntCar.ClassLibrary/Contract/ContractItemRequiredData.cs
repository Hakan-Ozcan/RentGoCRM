using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractItemRequiredData
    {
        public Guid contractId { get; set; }
        public int statuscode { get; set; }
        public DateTime? pickupDateTime { get; set; }
        public DateTime? dropoffDateTime { get; set; }
        public Guid pickupBranchId { get; set; }
        public Guid dropoffBranchId { get; set; }
        public Guid groupCodeInformationId { get; set; }
        public Guid transactionCurrencyId { get; set; }
        public Guid contactId { get; set; }
        public int itemTypeCode { get; set; }
    }
}
