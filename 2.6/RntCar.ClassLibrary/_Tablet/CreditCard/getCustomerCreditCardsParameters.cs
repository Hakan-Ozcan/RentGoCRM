using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class getCustomerCreditCardsParameters : RequestBase
    {
        public Guid customerId { get; set; }
        public Guid? reservationId { get; set; }
        public Guid? contractId { get; set; }
    }
}
