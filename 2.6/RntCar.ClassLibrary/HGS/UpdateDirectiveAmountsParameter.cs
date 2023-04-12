using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class UpdateDirectiveAmountsParameter
    {
        public string productId { get; set; }
        public string plateNo { get; set; }
        public string accountNumber { get; set; }
        public string creditCardNumber { get; set; }
        public decimal loadingLowerLimit { get; set; }
        public decimal loadingAmount { get; set; }
    }
}
