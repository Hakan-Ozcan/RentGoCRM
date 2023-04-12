using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IndividualAddressResponse : ResponseBase
    {
        public List<IndividualAddressData> addressDatas { get; set; }
    }
}
