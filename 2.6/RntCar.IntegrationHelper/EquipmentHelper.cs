using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.IntegrationHelper
{
    public class EquipmentHelper
    {
        public class EquipmentItemDataComparer : IEqualityComparer<EquipmentItem>
        {
            public bool Equals(EquipmentItem x, EquipmentItem y)
            {
                return (string.Equals(x.plateNumber, y.plateNumber));
            }

            public int GetHashCode(EquipmentItem obj)
            {
                return obj.plateNumber.GetHashCode();
            }
        }
    }
}
