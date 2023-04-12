using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class KilometerLimitParameter
    {
        public List<Guid> gorupCodeList { get; set; }
        public Guid? reservationId { get; set; }
        public Guid? contractId { get; set; }
        public DateTime pickupDate { get; set; }
        public DateTime dropoffDate { get; set; }
    }
}
