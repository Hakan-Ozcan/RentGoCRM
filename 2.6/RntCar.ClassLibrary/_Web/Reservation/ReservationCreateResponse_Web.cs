using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class ReservationCreateResponse_Web : ResponseBase
    {
        public Guid reservationId { get; set; }
        public List<Guid> reservationItemList { get; set; }
        public string pnrNumber { get; set; }
        public bool use3DSecure { get; set; }
        public string htmlContent { get; set; }
    }
}
