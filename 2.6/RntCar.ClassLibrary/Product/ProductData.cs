using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    //todo will implement all members later

    public class ProductData
    {
        public Guid productId { get; set; }
        public string product { get; set; }
        public int fuelTypeCode { get; set; }
        public string fuelTypeName { get; set; }
        public decimal tankCapacity { get; set; }
        public string model { get; set; }
        public string brand { get; set; }
        public string groupCode { get; set; }
        public int gearBox { get; set; }
        public string gearBoxName { get; set; }
        public int maintenancePeriod { get; set; }
        public string maintenancePeriodName { get; set; }
    }
}
