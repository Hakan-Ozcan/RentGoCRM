using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class CalculateCancelFineAmountParameters_Web : RequestBase
    {
        public Guid reservationId { get; set; }
        public string pnrNumber { get; set; }
    }
}
