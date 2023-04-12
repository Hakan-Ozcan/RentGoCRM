using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ReservationReport
    {
        public int count { get; set; }
        public List<ReservationDetailCount> reservationDetailCount { get; set; }
        public DateTime currentDate { get; set; }
    }
    public class ReservationDetailCount
    {
        public int status { get; set; }
        public int count { get; set; }
        public List<ReservationDetail> reservationDetails { get; set; }
    }
    public class ReservationDetail
    {
        public string reservationId { get; set; }
        public string reservationNumber { get; set; }
        public string reservationPNR { get; set; }
        public string customerName { get; set; }
        public string groupCodeInformationName { get; set; }
        public DateTime pickupDateTime { get; set; }
        public DateTime dropoffDateTime { get; set; }
    }
}
