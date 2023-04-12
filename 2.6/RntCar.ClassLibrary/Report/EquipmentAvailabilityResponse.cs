using System.Collections.Generic;

namespace RntCar.ClassLibrary
{
    public class EquipmentAvailabilityResponse : ResponseBase
    {
        public List<EquipmentAvailabilityData> EquipmentAvailabilityDatas { get; set; }
    }
}
