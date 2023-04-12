using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetContractGroupCodeReportResponse : ResponseBase
    {
        public List<BranchCount> waitingContractsByGroupCode { get; set; }
    }
}
