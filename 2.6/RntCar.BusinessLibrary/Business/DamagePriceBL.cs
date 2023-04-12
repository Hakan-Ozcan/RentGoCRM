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
    public class DamagePriceBL : BusinessHandler
    {
        public DamagePriceBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public DamagePriceBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public DamagePriceBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public List<DamagePriceData> getDamagePrices()
        {
            DamagePriceRepository damagePriceRepository = new DamagePriceRepository(this.OrgService, this.CrmServiceClient);
            var damagePriceData = damagePriceRepository.getAllDamagePrices();
            List<DamagePriceData> damagePrices = new List<DamagePriceData>();
            foreach (var item in damagePriceData.Entities)
            {
                EntityReference damageSizeRef = item.GetAttributeValue<EntityReference>("rnt_damagesizeid");
                EntityReference damageTypeRef = item.GetAttributeValue<EntityReference>("rnt_demagetypeid");
                EntityReference carPartRef = item.GetAttributeValue<EntityReference>("rnt_carpartid");

                damagePrices.Add(new DamagePriceData
                {
                    damagePriceId = item.Id,
                    damageAmount = item.GetAttributeValue<Money>("rnt_amount").Value,
                    damagePriceName = item.GetAttributeValue<string>("rnt_name"),
                    damageSize = new DamageSizeData() { damageSizeId = damageSizeRef.Id, damageSizeName = damageSizeRef.Name },
                    damageType = new DamageTypeData() { damageTypeId = damageTypeRef.Id, damageTypeName = damageTypeRef.Name },
                    equipmentPart = new EquipmentPartData() { equipmentPartId = carPartRef.Id, equipmentSubPartName = carPartRef.Name }
                });
            }
            return damagePrices;
        }

        public DamagePriceData getDamagePriceWithParameters(EntityReference damageSizeRef, EntityReference damageTypeRef, EntityReference carPartRef)
        {
            DamagePriceData damagePrice = new DamagePriceData();
            DamagePriceRepository damagePriceRepository = new DamagePriceRepository(this.OrgService, this.CrmServiceClient);
            var damagePriceData = damagePriceRepository.getDamagePriceWithParameters(damageSizeRef.Id, damageTypeRef.Id, carPartRef.Id);
            if (damagePriceData != null && damagePriceData.Id != Guid.Empty)
            {
                damagePrice = new DamagePriceData
                {
                    damagePriceId = damagePriceData.Id,
                    damageAmount = damagePriceData.GetAttributeValue<Money>("rnt_amount").Value,
                    damagePriceName = damagePriceData.GetAttributeValue<string>("rnt_name"),
                    damageSize = new DamageSizeData() { damageSizeId = damageSizeRef.Id, damageSizeName = damageSizeRef.Name },
                    damageType = new DamageTypeData() { damageTypeId = damageTypeRef.Id, damageTypeName = damageTypeRef.Name },
                    equipmentPart = new EquipmentPartData() { equipmentPartId = carPartRef.Id, equipmentSubPartName = carPartRef.Name }
                };
            }

            return damagePrice;
        }
    }
}
