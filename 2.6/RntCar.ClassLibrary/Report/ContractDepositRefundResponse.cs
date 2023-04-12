using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractDepositRefundResponse : ResponseBase
    {
        public List<ContractDetailForDepositRefund> contractDetail { get; set; }
    }
    public class ContractDetailForDepositRefund
    {
        public string contractId { get; set; }
        public DateTime dropoffDateTime { get; set; }
        public string dropoffDateTimeStr { get; set; }
        public string customerName { get; set; }
        public string contractPNR { get; set; }
        public decimal totalAmount { get; set; }
        public decimal netPayment { get; set; }
        public bool? depositBlockage { get; set; }
    }
}
