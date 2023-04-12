using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ReservationCancellationParameters
    {
        public Guid reservationId { get; set; }
        public int cancellationReason { get; set; }
        public string cancellationDescription { get; set; }
        public string pnrNumber { get; set; }
        public int langId { get; set; }
    }
}
