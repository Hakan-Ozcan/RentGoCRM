using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class RevenueReportData
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; }
        public decimal GoalRevenue { get; set; }
        public decimal ReachedRevenue { get; set; }
        public decimal RestRevenue { get { return GoalRevenue - ReachedRevenue; } set { } }

        public List<RevenueDetailsData> RevenueDetailsDatas { get; set; }
    }
}
