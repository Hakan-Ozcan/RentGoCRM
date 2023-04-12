using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class DashboardReservationData
    {
        public string reservationId { get; set; }
        public string reservationNumber { get; set; }
        public string pnrNumber { get; set; }
        public DashboardCustomer customer { get; set; }
        public DashboardGroupCodeInformation groupCodeInformation { get; set; }
        public Branch pickupBranch { get; set; }
        public Branch dropoffBranch { get; set; }
        public long pickupTimestamp { get; set; }
        public long dropoffTimestamp { get; set; }
        public decimal depositAmount { get; set; }
        public decimal totalAmount { get; set; }
    }
}
