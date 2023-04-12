using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class AdditionalProductReportData
    {
        public string branchId { get; set; }
        public string branchName { get; set; }
        public decimal revenue { get; set; }
        public decimal additionalProductAmount { get; set; }
        public string additionalProductId { get; set; }
        public decimal progressRatio { get; set; }
        public decimal baseBonusAmount { get; set; }
        public decimal progressBonusAmount { get; set; }
        public int userType { get; set; }
        public string userName { get; set; }
        public List<AdditionalProductRoleRate> additionalProductRoleRates { get; set; }
        public List<AdditionalProductUserRate> additionalProductUserRates { get; set; }

    }
}
