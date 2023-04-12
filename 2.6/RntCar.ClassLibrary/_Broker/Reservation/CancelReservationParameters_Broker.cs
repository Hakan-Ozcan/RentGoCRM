using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class CancelReservationParameters_Broker : RequestBase
    {
        public Guid reservationId { get; set; }
        public int cancellationReason { get { return 100000006; } set { } }
        public string pnrNumber { get; set; }
        public int langId { get; set; }
    }
}
