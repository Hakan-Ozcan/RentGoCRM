using RntCar.ClassLibrary._Mobile.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetCorporateReservationsResponse_Mobile : ResponseBase
    {
        public List<ReservationData_Mobile> reservations { get; set; }
    }
}
