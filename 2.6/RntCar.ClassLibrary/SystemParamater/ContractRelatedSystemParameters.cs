using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ContractRelatedSystemParameters
    {
        public int contractCancellationFineDuration { get; set; }
        public int contractMinimumDuration { get; set; }
        public string quickContractMinimumDuration { get; set; }
        public bool checkUserBranch { get; set; }
    }
}
