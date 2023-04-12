using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetContractDetailParameters : RequestBase
    {
        public Guid contractId { get; set; }

        public int statusCode { get; set; }
    }
}
