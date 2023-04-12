using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class AvailabilityPriceListData
    {
        public string AvailabilityPriceListId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public string PriceListId { get; set; }
        public string PriceListName { get; set; }
        public int MinimumAvailability { get; set; }
        public int MaximumAvailability { get; set; }
        public decimal PriceChangeRate { get; set; }
        public int StateCode { get; set; }
        public int StatusCode { get; set; }
        public Guid groupCodeId { get; set; }
        public string groupCodeName { get; set; }
    }
}
