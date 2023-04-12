using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class PriceHourEffectData
    {
        public string priceHourEffectId { get; set; }
        public int minimumMinute { get; set; }
        public int maximumMinute { get; set; }
        public int effectRate { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public int statuscode { get; set; }
        public int statecode { get; set; }
    }
}
