using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ReservationCreateResponse : ResponseBase
    {
        public Guid reservationId { get; set; }
        public List<Guid> reservationItemList { get; set; }
        public string pnrNumber { get; set; }
        public string dummyContactId { get; set; }
        public bool use3DSecure { get; set; }
        public string htmlContent { get; set; }
    }
}
