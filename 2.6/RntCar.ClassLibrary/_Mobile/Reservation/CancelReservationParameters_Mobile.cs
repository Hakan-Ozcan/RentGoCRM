using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class CancelReservationParameters_Mobile : RequestBase
    {
        public Guid reservationId { get; set; }
        public int cancellationReason { get; set; }
        public string pnrNumber { get; set; }
        public int langId { get; set; }
    }
}
