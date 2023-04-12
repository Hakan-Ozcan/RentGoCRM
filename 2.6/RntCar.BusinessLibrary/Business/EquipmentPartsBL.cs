using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Tablet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class EquipmentPartsBL : BusinessHandler
    {
        public EquipmentPartsBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public EquipmentPartsBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public EquipmentPartsBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public List<EquipmentPartData> getEquipmentParts()
        {
            EquipmentPartRepository equipmentPartRepository = new EquipmentPartRepository(this.OrgService, this.CrmServiceClient);
            var equipmentPartsData = equipmentPartRepository.getAllEquipmentParts();
            List<EquipmentPartData> equipmentParts = new List<EquipmentPartData>();
            foreach (var item in equipmentPartsData)
            {
                equipmentParts.Add(new EquipmentPartData
                {
                    equipmentPartId = item.Id,
                    equipmentSubPartName = item.GetAttributeValue<string>("rnt_name"),
                    equipmentMainPartId = item.GetAttributeValue<string>("rnt_mainpartid"),
                    equipmentSubPartId = item.GetAttributeValue<string>("rnt_subpartid")
                });
            }
            return equipmentParts;
        }
    }
}
