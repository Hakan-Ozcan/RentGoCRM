using RntCar.ClassLibrary.odata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreateContractDailyPricesFromScratchParameters
    {
        public List<DailyPrice> dailyPriceList { get; set; }
        public string contractItemId { get; set; }
        public string groupCodeId { get; set; }
    }
}
