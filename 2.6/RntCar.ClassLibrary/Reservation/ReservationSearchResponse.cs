using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ReservationSearchResponse : ResponseBase
    {
        public List<ReservationSearchData> ReservationData { get; set; }
    }
}
