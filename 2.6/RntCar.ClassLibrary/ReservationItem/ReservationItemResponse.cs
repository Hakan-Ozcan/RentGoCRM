using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ReservationItemResponse : ResponseBase
    {
        public ReservationItemDetail reservationItemDetail { get; set; }
    }
}
