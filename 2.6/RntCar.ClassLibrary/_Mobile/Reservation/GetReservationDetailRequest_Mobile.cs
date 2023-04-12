using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetReservationDetailRequest_Mobile:RequestBase
    {
        public string reservationId { get; set; }
        public string PNRNumber { get; set; }
    }
}
