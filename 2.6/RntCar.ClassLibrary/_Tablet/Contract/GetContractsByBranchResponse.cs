using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetContractsByBranchResponse : ResponseBase
    {
        public List<DashboardContractData> deliveryContractList { get; set; }
        public List<DashboardRentalContractData> rentalContractList { get; set; }

    }
}
