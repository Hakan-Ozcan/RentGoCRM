using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetTransfersByBranchResponse : ResponseBase
    {
        public List<GetTransfersByBranchData> deliveryTransferList { get; set; }
        public List<GetTransfersByBranchData> returnTransferList { get; set; }
    }
}
