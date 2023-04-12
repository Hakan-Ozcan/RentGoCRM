using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class GetMasterDataResponse : ResponseBase
    {
        public List<EquipmentPartData> equipmentPartList { get; set; }
        public List<DamageSizeData> damageSizeList { get; set; }
        public List<DamageTypeData> damageTypeList { get; set; }
        public List<DamageDocumentData> damageDocumentList { get; set; }
        public List<Branch> branchList { get; set; }
        public List<GroupCodeInformation> groupCodeList { get; set; }
        public List<OptionSetModel> equipmentStatusOptionSetList { get; set; }
    }
}
