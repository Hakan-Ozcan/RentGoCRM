using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CorporateCustomerSearchResponse : ResponseBase
    {
        public List<CorporateCustomerData> CorporateCustomerData { get; set; }

    }
}
