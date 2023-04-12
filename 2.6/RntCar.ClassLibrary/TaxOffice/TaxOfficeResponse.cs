using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class TaxOfficeResponse: ResponseBase
    {
        public List<TaxOfficeData> taxOfficeData { get; set; }
    }
}
