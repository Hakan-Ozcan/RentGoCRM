using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualCustomerAdditionalDriverResponse : ResponseBase
    {
        public Guid contactId { get; set; }
        public AdditionalProductData additionalProduct { get; set; }
    }
}
