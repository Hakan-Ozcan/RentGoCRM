using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractUpdateResponse : ResponseBase
    {
        public Guid contractId { get; set; }
        public string pnrNumber { get; set; }
    }
}
