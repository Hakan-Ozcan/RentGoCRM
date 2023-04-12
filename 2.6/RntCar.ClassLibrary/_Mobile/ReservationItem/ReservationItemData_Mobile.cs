using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class ReservationItemData_Mobile
    {
        public Guid reservationId { get; set; }
        public Guid itemId { get; set; }
        public string itemName { get; set; }
        public string productDescription { get; set; }
        public string productCode { get; set; }
        public string itemNo { get; set; }
        public int? itemType { get; set; }
        public int? value { get; set; }
        public decimal?  basePrice { get; set; }
        public decimal? netAmount { get; set; }
        public decimal? totalAmount { get; set; }
        public Guid? additionalProductId { get; set; }
    }
}
