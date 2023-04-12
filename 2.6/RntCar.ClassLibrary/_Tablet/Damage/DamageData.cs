using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class DamageData
    {
        public EquipmentPartData equipmentPart { get; set; }
        public DamageTypeData damageType { get; set; }
        public DamageSizeData damageSize { get; set; }
        public DamageInfoData damageInfo { get; set; }
        public DamageDocumentData damageDocument { get; set; }
        public decimal? damageAmount { get; set; }
        public bool isPriceCalculated { get; set; } = true;
        public Guid? damageId { get; set; }
        public bool isRepaired { get; set; } = false;
        public string blobStoragePath { get; set; }
    }
}
