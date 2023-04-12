
using Microsoft.Xrm.Sdk;
using System;

namespace RntCar.ClassLibrary
{
    public class CustomerOverview
    {
        public Entity CustomerFirstInvoice { get; set; }
        public Entity CustomerLastInvoice { get; set; }
        public decimal totalRevenue3 { get; set; }
        public decimal totalRevenue6 { get; set; }
        public decimal totalRevenue9 { get; set; }
        public decimal totalRevenue12 { get; set; }
        public decimal totalRevenue { get; set; }
        public Guid CustomerId { get; set; }
        public int frequency3 { get; set; }
        public int frequency6 { get; set; }
        public int frequency9 { get; set; }
        public int frequency12 { get; set; }
        public int frequency { get; set; }
        public DateTime nextExecution3 { get; set; }
        public DateTime nextExecution6 { get; set; }
        public DateTime nextExecution9 { get; set; }
        public DateTime nextExecution12 { get; set; }
        public Guid CustomerOverviewId { get;  set; }
        public int CustomerCurrentSegment { get; set; }
    }
}
