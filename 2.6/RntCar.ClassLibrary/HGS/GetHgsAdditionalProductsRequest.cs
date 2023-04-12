using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class GetHgsAdditionalProductsRequest : RequestBase
    {
        public decimal totalAmount { get; set; }
        public Guid contractId { get; set; }
    }
}
