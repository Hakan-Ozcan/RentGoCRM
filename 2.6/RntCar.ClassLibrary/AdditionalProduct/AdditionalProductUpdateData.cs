using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class AdditionalProductUpdateData
    {
        public decimal? currentBasePrice { get; set; }
        public int actualReservationDuration { get; set; }
        public int currentReservationDuration { get; set; }
        public decimal? currentTotalPrice { get; set; }
        public decimal? actualBasePrice { get; set; }
        public decimal? actualTotalPrice { get; set; }
        public decimal? amountToBePaid { get; set; }
    }
}
