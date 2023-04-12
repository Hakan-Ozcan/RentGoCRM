using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ReservationCancellationResponse : ResponseBase
    {
        public decimal fineAmount { get; set; }
        public decimal totalAmount { get; set; }
        public bool isCorporateReservation { get; set; }
        public decimal refundAmount { get; set; }
        public decimal discountAmount { get; set; }
        public int reservationPaymetType { get; set; }
        public bool willChargeFromUser { get; set; }
        public bool isCampaignCancelable { get; set; }
    }
}
