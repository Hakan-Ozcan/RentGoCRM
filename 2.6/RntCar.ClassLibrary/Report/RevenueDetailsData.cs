using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class RevenueDetailsData
    {
        public decimal TotalAmount { get; set; }
        public decimal RevenueAmount { get; set; }
        public string PickupBranch { get; set; }
        public Guid PickupBranchId { get; set; }
        public string BusinessRole { get; set; }
        public decimal BaseBonusRatio { get; set; }
        public decimal PositionalBonusRatio { get; set; }
        public decimal NetBonusAmount { get { return Decimal.Round((RevenueAmount * BaseBonusRatio * PositionalBonusRatio) / 10000, 2); } set { } }
    }
}
