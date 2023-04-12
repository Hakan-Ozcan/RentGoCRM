using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class DamagePriceData
    {
        public Guid damagePriceId { get; set; }
        public string damagePriceName { get; set; }
        public decimal? damageAmount { get; set; }

        public DamageTypeData damageType { get; set; }
        public DamageSizeData damageSize { get; set; }
        public EquipmentPartData equipmentPart { get; set; }
    }
}
