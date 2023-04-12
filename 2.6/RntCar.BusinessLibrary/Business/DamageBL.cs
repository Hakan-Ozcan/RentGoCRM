using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Tablet;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class DamageBL : BusinessHandler
    {
        public DamageBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public DamageBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public DamageBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public DamageBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public List<DamageData> getDamageDataByEquipmentId(Guid equipmentId)
        {
            DamageRepository damageRepository = new DamageRepository(this.OrgService, this.CrmServiceClient);
            var damages = damageRepository.getDamagesByEquipmentIdWithGivenColumns(equipmentId, new string[] { "rnt_carpartid",
                                                                                                               "rnt_damagesizeid",
                                                                                                               "rnt_damagetypeid",
                                                                                                               "rnt_branchid",
                                                                                                               "rnt_damagedate",
                                                                                                               "rnt_blobstoragepath"});
            List<DamageData> damageData = new List<DamageData>();
            foreach (var item in damages)
            {
                //get damages equipment part for mesh name
                EquipmentPartRepository equipmentPartRepository = new EquipmentPartRepository(this.OrgService, this.CrmServiceClient);
                var equipmentPart = equipmentPartRepository.getEquipmentPartById(item.GetAttributeValue<EntityReference>("rnt_carpartid").Id);
                //create damage data object
                damageData.Add(new DamageData
                {
                    damageId = item.Id,
                    damageInfo = new DamageInfoData
                    {
                        damageDate = item.GetAttributeValue<DateTime>("rnt_damagedate").converttoTimeStamp(),
                        damageBranch = new Branch
                        {
                            branchId = item.GetAttributeValue<EntityReference>("rnt_branchid").Id,
                            branchName = item.GetAttributeValue<EntityReference>("rnt_branchid").Name
                        }
                    },
                    damageSize = new DamageSizeData
                    {
                        damageSizeId = item.GetAttributeValue<EntityReference>("rnt_damagesizeid").Id,
                        damageSizeName = item.GetAttributeValue<EntityReference>("rnt_damagesizeid").Name
                    },
                    damageType = new DamageTypeData
                    {
                        damageTypeId = item.GetAttributeValue<EntityReference>("rnt_damagetypeid").Id,
                        damageTypeName = item.GetAttributeValue<EntityReference>("rnt_damagetypeid").Name
                    },
                    equipmentPart = new EquipmentPartData
                    {
                        equipmentPartId = equipmentPart.Id,
                        equipmentSubPartName = equipmentPart.Attributes.Contains("rnt_name") ? equipmentPart.GetAttributeValue<string>("rnt_name") : string.Empty,
                        equipmentSubPartId = equipmentPart.Attributes.Contains("rnt_subpartid") ? equipmentPart.GetAttributeValue<string>("rnt_subpartid") : string.Empty,
                        equipmentMainPartId = equipmentPart.Attributes.Contains("rnt_mainpartid") ? equipmentPart.GetAttributeValue<string>("rnt_mainpartid") : string.Empty
                    },
                    blobStoragePath = item.Attributes.Contains("rnt_blobstoragepath") ? item.GetAttributeValue<string>("rnt_blobstoragepath") : string.Empty
                });
            }

            return damageData;
        }

        public List<DamageData> getCalculatedDamagesAmounts(List<DamageData> damageDataList, Guid contractId)
        {
            foreach (var damage in damageDataList)
            {
                // if the damage data has a police report no need to calculate damage amaount the amount must be zero
                if (damage.damageDocument.damageDocumentType == (int)DamageDocumentEnums.DocumentType.PoliceReport)
                {
                    damage.damageAmount = decimal.Zero;
                }
                else
                {
                    DamagePriceRepository damagePriceRepository = new DamagePriceRepository(this.OrgService, this.CrmServiceClient);
                    var damagePriceEntity = damagePriceRepository.getDamagePriceWithParameters(damage.equipmentPart.equipmentPartId,
                                                                                               damage.damageSize.damageSizeId,
                                                                                               damage.damageType.damageTypeId);
                    if (damagePriceEntity != null)
                    {
                        var damagePrice = damagePriceEntity.GetAttributeValue<Money>("rnt_amount").Value;
                        // get assurance additional products by products codes
                        ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService, this.CrmServiceClient);
                        var productCodes = configurationBL.GetConfigurationByName("additionalProduct_insuranceProductCodes").Split(';');
                        AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService, this.CrmServiceClient);
                        var additionalProducts = additionalProductRepository.getAdditionalProductsByProductCodes(productCodes, new string[] { "rnt_insurancecoverageamount" });
                        string[] additonalProductIds = additionalProducts.Select(p => p.Id.ToString()).ToArray();

                        //check contract has assurance additional product by additional products ids
                        ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService, this.CrmServiceClient);
                        var insuranceContractItem = contractItemRepository.getContractItemAdditionalProductsByContractIdByAdditionalProductIds(contractId, additonalProductIds).FirstOrDefault();

                        if (insuranceContractItem != null)
                        {
                            // if contract have insurance with the statement check the damage amount 
                            if (damage.damageDocument.damageDocumentType == (int)DamageDocumentEnums.DocumentType.Statement)
                            {
                                //get selected insurance item in additionalProducts by additional product id
                                var insuranceItem = additionalProducts.Where(x => x.Id == insuranceContractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id).FirstOrDefault();
                                var insuranceAmount = insuranceItem.GetAttributeValue<Money>("rnt_insurancecoverageamount").Value;
                                // if damage amaount greater than insurance amount set damage amount with calculated prices
                                if (damagePrice > insuranceAmount)
                                {
                                    damage.damageAmount = damagePrice;
                                }
                                //if damage amaount smaller than insurance amount set damage amount decimal zero
                                else
                                {

                                    damage.damageAmount = decimal.Zero;
                                }
                            }
                            // if there is no statement set damage amount with calculated prices
                            else
                            {

                                damage.damageAmount = damagePrice;
                            }
                        }
                        // if there is no insurance set damage amount with calculated prices
                        else
                        {

                            damage.damageAmount = damagePrice;
                        }
                    }
                    // if damage price couldnt find set isPriceCalculated false for damage reflection cost additional product
                    else
                    {

                        damage.damageAmount = decimal.Zero;
                        damage.isPriceCalculated = false;
                    }
                }
            }
            return damageDataList;
        }

        public CreateDamageResponse createDamages(CreateDamageParameter damageParameter)
        {
            try
            {

                foreach (var damageItem in damageParameter.damageData)
                {
                    Entity damage = new Entity("rnt_damage");
                    damage.Attributes["statuscode"] = new OptionSetValue((int)DamageEnums.StatusCode.Open);
                    if (damageParameter.contractId.HasValue)
                        damage.Attributes["rnt_contractid"] = new EntityReference("rnt_contract", damageParameter.contractId.Value);
                    if (damageParameter.transferId.HasValue)
                        damage.Attributes["rnt_transfernumberid"] = new EntityReference("rnt_transfer", damageParameter.transferId.Value);

                    damage.Attributes["rnt_branchid"] = new EntityReference("rnt_branch", damageItem.damageInfo.damageBranch.branchId.Value);
                    damage.Attributes["rnt_equipmentid"] = new EntityReference("rnt_equipment", damageParameter.equipmentId);
                    damage.Attributes["rnt_carpartid"] = new EntityReference("rnt_carpart", damageItem.equipmentPart.equipmentPartId);
                    damage.Attributes["rnt_damagesizeid"] = new EntityReference("rnt_demagesize", damageItem.damageSize.damageSizeId);
                    damage.Attributes["rnt_damagetypeid"] = new EntityReference("rnt_damagetype", damageItem.damageType.damageTypeId);
                    damage.Attributes["rnt_damagedocumentid"] = new EntityReference("rnt_damagedocument", damageItem.damageDocument.damageDocumentId);
                    damage.Attributes["rnt_chargeout"] = false;
                    damage.Attributes["rnt_depositblockage"] = false;
                    damage.Attributes["rnt_blobstoragepath"] = damageItem.blobStoragePath;
                    damage.Attributes["rnt_totalamount"] = new Money(damageItem.damageAmount.Value);

                    if (damageItem.damageId.HasValue)
                        damage.Id = damageItem.damageId.Value;

                    damage.Attributes["rnt_damagedate"] = DateTime.UtcNow.AddMinutes(StaticHelper.offset);
                    if (damageParameter.userInformation != null && damageParameter.userInformation.userId != null)
                    {
                        damage["rnt_externaluserid"] = new EntityReference("rnt_serviceuser", damageParameter.userInformation.userId);
                    }
                    this.OrgService.Create(damage);
                }

                return new CreateDamageResponse
                {
                    responseResult = RntCar.ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new CreateDamageResponse
                {
                    responseResult = RntCar.ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        public void updateDamagesStatusRepair(List<DamageData> damageParameters)
        {
            foreach (var item in damageParameters)
            {
                this.setDamageStatus(item.damageId.Value, (int)GlobalEnums.StateCode.Active, (int)ClassLibrary._Enums_1033.rnt_damage_StatusCode.Repaired);
            }
        }

        private void setDamageStatus(Guid damageId, int stateCode, int statusCode)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.setState("rnt_damage", damageId, stateCode, statusCode);
        }
    }
}
