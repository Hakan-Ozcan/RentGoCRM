using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class ReservationDetailData_Mobile
    {
        public decimal? totalAmount { get; set; }
        public decimal? depositAmount { get; set; }

        public List<ReservationItemData_Mobile> reservationItems { get; set; }
    }
}
