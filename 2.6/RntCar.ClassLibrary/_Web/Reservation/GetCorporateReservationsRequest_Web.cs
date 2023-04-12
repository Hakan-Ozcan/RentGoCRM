using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class GetCorporateReservationsRequest_Web : RequestBase
    {
        public Guid corporateCustomerId { get; set; }
    }
}
