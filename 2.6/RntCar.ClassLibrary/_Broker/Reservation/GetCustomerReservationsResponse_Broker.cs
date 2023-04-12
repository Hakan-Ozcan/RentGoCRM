using System.Collections.Generic;

namespace RntCar.ClassLibrary._Broker
{
    public class GetCustomerReservationsResponse_Broker : ResponseBase
    {
        public List<ReservationData_Broker> reservations { get; set; }
    }
}
