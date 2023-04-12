using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.Reservation
{
    public class ReservationCreationParameters
    {
        public ReservationCustomerParameters SelectedCustomer { get; set; }
        public ReservationDateandBranchParameters SelectedDateAndBranch { get; set; }
        public ReservationEquipmentParameters SelectedEquipment { get; set; }
        public ReservationPriceParameters PriceParameters { get; set; }
        public List<ReservationAdditionalProductParameters> SelectedAdditionalProducts { get; set; }
        public int ReservationChannel { get; set; }
        public int ReservationTypeCode { get; set; }
        public string Currency { get; set; } 
        public int TotalDuration { get; set; }
        public int offset { get; set; }
        public int LangId { get; set; }
        public string TrackingNumber { get; set; }


    }
}
