using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetEquipmentsResponse_Mobile : ResponseBase
    {
        public List<EquipmentData_Mobile> equipments { get; set; }
    }
}
