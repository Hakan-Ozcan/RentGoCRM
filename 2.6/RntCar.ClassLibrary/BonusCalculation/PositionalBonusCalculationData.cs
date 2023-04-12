using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class PositionalBonusCalculationData
    {
        public DateTime QueryDate { get; set; }
        public string ContractItemId { get; set; }
        public string ContractItem { get; set; }
        public decimal Amount { get; set; }
        public bool IsRevenue { get; set; }
        public string ContractNumber { get; set; }
        public string PnrNumber { get; set; }
        public string BonusCalculationId { get; set; }
        public string PickupBranch { get; set; }
        public Guid PickupBranchId { get; set; }
        public string DropoffBranch { get; set; }
        public Guid DropoffBranchId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime DropoffDate { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string BusinessRole { get; set; }
        public decimal BaseBonusRatio { get; set; }
        public decimal PositionalBonusRatio { get; set; }
        //public decimal NetBonusAmount { get { return (RevenueAmount * BaseBonusRatio * PositionalBonusRatio) / 10000; } set { } }
    }
}
