using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetCorporateReservationsRequest_Mobile : RequestBase
    {
        public Guid corporateCustomerId { get; set; }
    }
}
