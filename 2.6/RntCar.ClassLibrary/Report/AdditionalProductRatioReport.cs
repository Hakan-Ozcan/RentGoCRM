using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class AdditionalProductRatioReport
    {
        public decimal additionalProductBonus { get; set; }
        public decimal additionalServiceBonus { get; set; }
        public decimal totalBonus { get { return additionalProductBonus + additionalServiceBonus; } set { } }
    }
}
