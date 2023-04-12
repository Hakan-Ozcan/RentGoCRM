using System;

namespace RntCar.ClassLibrary.Report
{
    public class DailyReportData
    {
        public Guid CurrentBranchId { get; set; }
        public bool isFranchise { get; set; }
        public string CurrentBranch { get; set; }
        public Guid GroupCodeInformationId { get; set; }

        public double Total { get; set; }
        public double TotalPreviousYear { get; set; }
        public double growth { get { if (TotalPreviousYear == 0) { return 100; } else { return Math.Round(((Total - TotalPreviousYear) / TotalPreviousYear) * 100); } } set { } }

        public double RentalCount { get; set; }

        public double Availability { get { if (Total == 0) { return 0; } else { return Math.Round((RentalCount / Total) * 100); } } set { } }
        public decimal? Revenue { get; set; }
        public decimal? RevenuePrevious { get; set; }
        public double growthRevenue { get { if (RevenuePrevious == 0 || RevenuePrevious == null) { return 100; } else { return Math.Round(Convert.ToDouble(((Revenue - RevenuePrevious) / RevenuePrevious) * 100)); } } set { } }

        public decimal? Revenue_USD { get; set; }
        public decimal? RevenuePrevious_USD { get; set; }
        public double growthRevenue_USD { get { if (RevenuePrevious_USD == 0 || RevenuePrevious_USD == null) { return 100; } else { return Math.Round(Convert.ToDouble(((Revenue_USD - RevenuePrevious_USD) / RevenuePrevious_USD) * 100)); } } set { } }

        public double revenuepercar { get { if (Total == 0) { return 0; } else { return Math.Round((Convert.ToDouble(Revenue) / Total)); } } set { } }
        public double revenuepercarprevious { get { if (TotalPreviousYear == 0) { return 0; } else { return Math.Round((Convert.ToDouble(RevenuePrevious) / TotalPreviousYear)); } } set { } }
        public double growthRevenuepercar { get { if (revenuepercarprevious == 0) { return 100; } else { return Math.Round(Convert.ToDouble(((revenuepercar - revenuepercarprevious) / revenuepercarprevious) * 100)); } } set { } }

        public double revenuepercar_USD { get { if (Total == 0) { return 0; } else { return Math.Round((Convert.ToDouble(Revenue_USD) / Total)); } } set { } }
        public double revenuepercarprevious_USD { get { if (TotalPreviousYear == 0) { return 0; } else { return Math.Round((Convert.ToDouble(RevenuePrevious_USD) / TotalPreviousYear)); } } set { } }
        public double growthRevenuepercar_USD { get { if (revenuepercarprevious == 0) { return 100; } else { return Math.Round(Convert.ToDouble(((revenuepercar_USD - revenuepercarprevious_USD) / revenuepercarprevious_USD) * 100)); } } set { } }

        public double CompletedContractCount { get; set; }
        public double CompletedContractCounttprevious { get; set; }
        public double growthContractCount { get { if (CompletedContractCounttprevious == 0) { return 100; } else { return Math.Round(((CompletedContractCount - CompletedContractCounttprevious) / CompletedContractCounttprevious) * 100); } } set { } }

        public decimal totalday { get; set; }
        public decimal totaldayprevious { get; set; }
        public double growthDayCount { get { if (totaldayprevious == 0) { return 100; } else { return Math.Round(Convert.ToDouble(((totalday - totaldayprevious) / totaldayprevious) * 100)); } } set { } }

        public double RevenuePerDay { get { if (totalday == 0) { return 0; } else { return Math.Round((Convert.ToDouble(Revenue / totalday))); } } set { } }
        public double RevenuePerDayPrevious { get { if (totaldayprevious == 0) { return 0; } else { return Math.Round((Convert.ToDouble(RevenuePrevious / totaldayprevious))); } } set { } }
        public double growthrevenueperday { get { if (RevenuePerDayPrevious == 0) { return 100; } else { return Math.Round(((RevenuePerDay - RevenuePerDayPrevious) / RevenuePerDayPrevious) * 100); } } set { } }

        public double RevenuePerDay_USD { get { if (totalday == 0) { return 0; } else { return Math.Round((Convert.ToDouble(Revenue_USD / totalday))); } } set { } }
        public double RevenuePerDayPrevious_USD { get { if (totaldayprevious == 0) { return 0; } else { return Math.Round((Convert.ToDouble(RevenuePrevious_USD / totaldayprevious))); } } set { } }
        public double growthrevenueperday_USD { get { if (RevenuePerDayPrevious == 0) { return 100; } else { return Math.Round(((RevenuePerDay_USD - RevenuePerDayPrevious_USD) / RevenuePerDayPrevious_USD) * 100); } } set { } }

        public double lor { get { if (CompletedContractCount == 0) { return 0; } else { return Math.Round((Convert.ToDouble(totalday) / CompletedContractCount)); } } set { } }
        public double lorprevious { get { if (totaldayprevious == 0) { return 0; } else { return Math.Round((Convert.ToDouble(totaldayprevious) / CompletedContractCounttprevious)); } } set { } }
        public double growthlor { get { if (lorprevious == 0) { return 100; } else { return Math.Round(((lor - lorprevious) / lorprevious) * 100); } } set { } }
    }
}
