using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class UpdateTransferParameter
    {
        public Guid transferId { get; set; }
        public UserInformation userInformation { get; set; }
        public ContractEquipmentInformation equipmentInformation { get; set; }
        public List<DamageData> damageList { get; set; }
    }
}
