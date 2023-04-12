using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Tablet;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Transfer.Action
{
    public class ExecuteUpdateTransferForReturn : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);

            try
            {
                string updateTransferForDeliveryParameter;
                initializer.PluginContext.GetContextParameter<string>("UpdateTranferParameter", out updateTransferForDeliveryParameter);
                initializer.TraceMe("parameters : " + updateTransferForDeliveryParameter);
                var deserializedParameter = JsonConvert.DeserializeObject<UpdateTransferParameter>(updateTransferForDeliveryParameter);

                #region update equipment transaction history
                EquipmentRepository equipmentRepository = new EquipmentRepository(initializer.Service);
                var currentEquipmentTransaction = equipmentRepository.getEquipmentByIdByGivenColumns(deserializedParameter.equipmentInformation.equipmentId,
                                                                                                     new string[] { "rnt_equipmenttransactionid" });
                EquipmentTransactionBL equipmentTransactionBL = new EquipmentTransactionBL(initializer.Service, initializer.TracingService);
                equipmentTransactionBL.updateEquipmentTransactionHistoryforRental(new CreateEquipmentTransactionHistoryRentalParameters
                {
                    transferId = deserializedParameter.transferId,
                    firstFuelValue = deserializedParameter.equipmentInformation.firstFuelValue,
                    firstKmValue = deserializedParameter.equipmentInformation.firstKmValue,
                    equipmentId = deserializedParameter.equipmentInformation.equipmentId,
                    rentalFuelValue = deserializedParameter.equipmentInformation.currentFuelValue,
                    rentalKmValue = deserializedParameter.equipmentInformation.currentKmValue,
                    equipmentTransactionId = currentEquipmentTransaction.GetAttributeValue<EntityReference>("rnt_equipmenttransactionid").Id
                });
                #endregion update equipment transaction history

                #region create equipment inventory history
                var inventoryList = deserializedParameter.equipmentInformation.equipmentInventoryData.
                 ConvertAll(x => new CreateEquipmentInventoryHistoryParameter
                 {
                     isExists = x.isExist.Value,
                     logicalName = x.logicalName
                 }).ToList();

                EquipmentInventoryBL equipmentInventoryBL = new EquipmentInventoryBL(initializer.Service, initializer.TracingService);
                var equipmentInventoryId = equipmentInventoryBL.createEquipmentInventoryHistoryforRental(deserializedParameter.equipmentInformation.equipmentId,
                                                                                                         null,
                                                                                                         deserializedParameter.transferId,
                                                                                                         inventoryList);
                #endregion create equipment inventory history                                                                                 

                #region update equipment 
                initializer.TraceMe("updateEquipmentforRental start");

                EquipmentBL equipmentBL = new EquipmentBL(initializer.Service, initializer.TracingService);
                equipmentBL.updateEquipmentforRental(deserializedParameter.equipmentInformation.equipmentId,
                                                     equipmentInventoryId,
                                                     currentEquipmentTransaction.GetAttributeValue<EntityReference>("rnt_equipmenttransactionid").Id,
                                                     deserializedParameter.userInformation.branchId,
                                                     deserializedParameter.equipmentInformation.currentKmValue,
                                                     deserializedParameter.equipmentInformation.currentFuelValue);

                var transferType = -1;// means clear optionset value
                initializer.TraceMe("start update trasnfer Type");
                equipmentBL.updateEquipmentTransferType(deserializedParameter.equipmentInformation.equipmentId, transferType);

                // update equipment status with in transfer
                //equipmentBL.updateEquipmentStatus(deserializedParameter.equipmentInformation.equipmentId, (int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.Available);
                initializer.TraceMe("updateEquipmentforRental end");
                #endregion update equipment

                #region create and update damages
                if (deserializedParameter.damageList != null && deserializedParameter.damageList.Count > 0)
                {
                    var damagesForCreate = deserializedParameter.damageList.Where(p => !p.isRepaired).ToList();
                    DamageBL damageBL = new DamageBL(initializer.Service, initializer.TracingService);
                    damageBL.createDamages(new CreateDamageParameter
                    {
                        transferId = deserializedParameter.transferId,
                        equipmentId = deserializedParameter.equipmentInformation.equipmentId,
                        damageData = damagesForCreate,
                        userInformation = deserializedParameter.userInformation
                    });
                    var damagesForUpdate = deserializedParameter.damageList.Where(p => p.isRepaired).ToList();
                    damageBL.updateDamagesStatusRepair(damagesForUpdate);
                }
                #endregion create and update damages

                #region update transfer
                TransferBL transferBL = new TransferBL(initializer.Service, initializer.TracingService);
                transferBL.updateTransferForReturn(deserializedParameter.transferId,
                                                   deserializedParameter.equipmentInformation.currentKmValue,
                                                   deserializedParameter.equipmentInformation.currentFuelValue);
                #endregion update transfer

                initializer.PluginContext.OutputParameters["UpdateTransferResponse"] = JsonConvert.SerializeObject(ClassLibrary._Tablet.ResponseResult.ReturnSuccess());
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
