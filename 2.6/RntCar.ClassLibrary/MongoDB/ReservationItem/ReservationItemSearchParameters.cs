using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class ReservationItemSearchParameters
    {
        public string reservationNumber { get; set; }
        public string customerId { get; set; }
        public string pickupBranchId { get; set; }
        public string dropOffBranchId { get; set; }
        public DateTime? pickupDateTime { get; set; }
        public DateTime? dropoffDateTime { get; set; }
        public int? status { get; set; }
    }
}
