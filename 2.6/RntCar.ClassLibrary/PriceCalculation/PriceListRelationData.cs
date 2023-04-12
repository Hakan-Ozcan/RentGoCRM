using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class PriceListRelationData
    {
        public string rnt_name { get; set; }
        public int rnt_pricetype { get; set; }
        public DateTime rnt_begindate { get; set; }
        public DateTime rnt_enddate { get; set; }
        public Guid rnt_pricecodeid { get; set; }
    }
}
