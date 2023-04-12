using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class CreateDamageParameter
    {
        public Guid? contractId { get; set; }
        public Guid? transferId { get; set; }
        public Guid equipmentId { get; set; }
        public List<DamageData> damageData { get; set; }
    
        public UserInformation userInformation { get; set; }
    }
}
