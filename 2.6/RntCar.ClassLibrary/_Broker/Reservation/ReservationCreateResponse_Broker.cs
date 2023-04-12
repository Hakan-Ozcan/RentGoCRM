using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Broker
{
    public class ReservationCreateResponse_Broker : ResponseBase
    {
        public Guid reservationId { get; set; }
        public List<Guid> reservationItemList { get; set; }
        public string pnrNumber { get; set; }
    }
}
