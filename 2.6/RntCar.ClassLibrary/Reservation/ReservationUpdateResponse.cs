using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ReservationUpdateResponse : ResponseBase
    {
        public Guid reservationId { get; set; }
        public string pnrNumber { get; set; }
        public decimal corporateAmount { get; set; }
    }
}
