using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class AdditionalProductUpdateResponse : ResponseBase
    {
        public List<AdditionalProductUpdateData> additionalProducts { get; set; }
        public decimal? totalAmountToBePaid { get; set; }
    }
}
