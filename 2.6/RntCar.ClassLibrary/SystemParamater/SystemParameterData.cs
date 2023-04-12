using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class SystemParameterData
    {
        public bool isMernisEnabled { get; set; }
        public bool isCustomerDuplicateCheckEnabled { get; set; }
        public bool isTaxnoValidationEnabled { get; set; }
        public bool isReservationandContractCheckEnabled { get; set; }
        public int maximumFindeksPoint { get; set; }
    }
}
