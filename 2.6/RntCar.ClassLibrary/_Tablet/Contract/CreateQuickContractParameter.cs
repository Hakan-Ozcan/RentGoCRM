using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class CreateQuickContractParameter : RequestBase
    {
        public ContractCustomerParameters customerInformation { get; set; }
        public UserInformation userInformation { get; set; }
        public ContractPriceParameters priceInformation { get; set; }
        public string reservationId { get; set; }
        public string reservationPNR { get; set; }
        public Guid currency { get; set; }
    }
}
