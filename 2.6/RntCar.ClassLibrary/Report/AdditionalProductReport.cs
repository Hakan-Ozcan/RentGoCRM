using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class AdditionalProductReport
    {
        public List<AdditionalProductReportData> additionalProductReportDatas { get; set; }
        public decimal totalAdditionalProductAmount { get; set; }
        public decimal totalAdditionalServiceAmount { get; set; }
        public decimal totalRevenue { get; set; }
        public decimal ratio { get; set; }
    }
}
