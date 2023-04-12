using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class ContractInformation : ContractManualDateInformation
    {        
        public int segment { get; set; }
        public Guid priceCodeId { get; set; }
    }
}
