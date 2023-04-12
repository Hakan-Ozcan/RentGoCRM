using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class EquipmentAvailabilityInvoiceItemResponse
    {
        public List<RevenueItem> revenueItems { get; set; }
    }

    public class RevenueItem
    {
        public Guid branchId { get; set; }
        public decimal? revenue { get; set; }
    }
}
