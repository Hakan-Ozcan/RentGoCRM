using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.Enums
{
    public class InvoiceEnums
    {
        public enum ActiveStatus
        {
            Draft = 1,
            IntegratedWithLogo = 100000002,
            IntegrationError = 100000003
        }
        public enum DeactiveStatus
        {
            Cancelled = 100000004,
            CancelledByLogo = 2
        }
    }
}
