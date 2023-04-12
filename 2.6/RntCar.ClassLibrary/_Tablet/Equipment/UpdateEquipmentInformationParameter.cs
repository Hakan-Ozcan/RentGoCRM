using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class UpdateEquipmentInformationParameter : RequestBase
    {
        public ContractEquipmentInformation equipmentInformation { get; set; }
        public int statusCode { get; set; }
    }
}
