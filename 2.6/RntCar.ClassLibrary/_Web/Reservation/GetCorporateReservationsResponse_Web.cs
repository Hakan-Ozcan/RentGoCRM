using RntCar.ClassLibrary._Web.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class GetCorporateReservationsResponse_Web : ResponseBase
    {
        public List<ReservationData_Web> reservations { get; set; }
    }
}
