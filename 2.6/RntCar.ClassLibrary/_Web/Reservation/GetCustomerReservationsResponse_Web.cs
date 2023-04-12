using RntCar.ClassLibrary._Web.Reservation;
using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class GetCustomerReservationsResponse_Web : ResponseBase
    {
        public List<ReservationData_Web> reservations { get; set; }
    }
}
