using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetContractsByBranchParameters : RequestBase
    {
        public Guid branchId { get; set; }
    }
}
