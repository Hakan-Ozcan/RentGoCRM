using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class EquipmentData_Web
    {
        public string GroupCodeInformationName { get; set; }
        public string GroupCodeInformationId { get; set; }
        public List<Equipment_Web> Equipments { get; set; }
    }
}
