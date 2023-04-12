using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ReservationRelatedSystemParameters
    {
        public int reservationCancellationDuration { get; set; }
        public int reservationCancellationFineDuration { get; set; }
        public int reservationNoShowDuration { get; set; }
    }
}
