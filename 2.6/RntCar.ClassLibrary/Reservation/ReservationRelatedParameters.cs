using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.Reservation
{
    public class ReservationRelatedParameters
    {
        public int reservationChannel { get; set; }
        public int reservationTypeCode { get; set; }
        public Guid reservationId { get; set; }
        public int statusCode { get; set; }
        public string trackingNumber { get; set; }
    }
}
