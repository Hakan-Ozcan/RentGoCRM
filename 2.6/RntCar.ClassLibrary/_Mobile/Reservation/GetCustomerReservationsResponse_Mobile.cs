using RntCar.ClassLibrary._Mobile.Reservation;
using System.Collections.Generic;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetCustomerReservationsResponse_Mobile : ResponseBase
    {
        public List<ReservationData_Mobile> reservations { get; set; }
    }
}
