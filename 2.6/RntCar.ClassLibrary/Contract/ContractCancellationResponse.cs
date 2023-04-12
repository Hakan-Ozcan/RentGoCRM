using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractCancellationResponse : ResponseBase
    {
        public decimal fineAmount { get; set; }
        public decimal totalAmount { get; set; }
        public decimal refundAmount { get; set; }
        public bool  isCorporateContract { get; set; }
        public int contractPaymetType { get; set; }
        public bool willChargeFromUser { get; set; }
        public bool isCampaignCancelable { get; set; }
    }
}
