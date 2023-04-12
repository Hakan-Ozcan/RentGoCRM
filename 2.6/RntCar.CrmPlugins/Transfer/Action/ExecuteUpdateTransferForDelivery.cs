using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Tablet;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Transfer.Action
{
    public class ExecuteUpdateTransferForDelivery : IPlugin
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

                #region create equipment transaction history
                EquipmentTransactionBL equipmentTransactionBL = new EquipmentTransactionBL(initializer.Service, initializer.TracingService);
                initializer.TraceMe("createEquipmentTransactionHistoryforDelivery start");
                var equipmentTransactionId = equipmentTransactionBL.createEquipmentTransactionHistoryforDelivery(new CreateEquipmentTransactionHistoryDeliveryParameters
                {
                    transferId = deserializedParameter.transferId,
                    deliveryFuelValue = deserializedParameter.equipmentInformation.currentFuelValue,
                    fuelValue = deserializedParameter.equipmentInformation.firstFuelValue,
                    deliveryKmValue = deserializedParameter.equipmentInformation.currentKmValue,
                    kmValue = deserializedParameter.equipmentInformation.firstKmValue,
                    equipmentId = deserializedParameter.equipmentInformation.equipmentId
                });
                initializer.TraceMe("createEquipmentTransactionHistoryforDelivery end");
                #endregion create equipment transaction history

                #region create equipment inventory history
                var inventoryList = deserializedParameter.equipmentInformation.equipmentInventoryData.
                 ConvertAll(x => new CreateEquipmentInventoryHistoryParameter
                 {
                     isExists = x.isExist.Value,
                     logicalName = x.logicalName
                 }).ToList();

                EquipmentInventoryBL equipmentInventoryBL = new EquipmentInventoryBL(initializer.Service, initializer.TracingService);
                var equipmentInventoryId = equipmentInventoryBL.createEquipmentInventoryHistoryforDelivery(deserializedParameter.equipmentInformation.equipmentId,
                                                                                                           null,
                                                                                                           deserializedParameter.transferId,
                                                                                                           inventoryList);
                #endregion create equipment inventory history                                                                                 

                #region update Equipment
                initializer.TraceMe("update equipment start");
                EquipmentBL equipmentBL = new EquipmentBL(initializer.Service);
                equipmentBL.updateEquipmentforDelivery(deserializedParameter.equipmentInformation.equipmentId,
                                                       equipmentInventoryId,
                                                       equipmentTransactionId,
                                                       deserializedParameter.equipmentInformation.currentKmValue,
                                                       deserializedParameter.equipmentInformation.currentFuelValue);

                TransferRepository transferRepository = new TransferRepository(initializer.Service);
                var transfer = transferRepository.getTransferById(deserializedParameter.transferId);

                var transferStatus = (int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.Transferred;
                initializer.TraceMe("transfer status " + transfer.GetAttributeValue<OptionSetValue>("statuscode").Value);
                var transferType = transfer.GetAttributeValue<OptionSetValue>("rnt_transfertype").Value;
                initializer.TraceMe("transfer type " + transferType);
                if (transferType == (int)rnt_TransferType.IkinciEl)
                {
                    //second hand waiting confirmation
                    equipmentBL.updateEquipmentStatus(deserializedParameter.equipmentInformation.equipmentId, 100000008);
                    transferStatus = (int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.Completed;
                }
                else
                {
                    initializer.TraceMe("Update Equipment For Transfer");
                    equipmentBL.updateEquipmentStatus(deserializedParameter.equipmentInformation.equipmentId, (int)rnt_equipment_StatusCode.InTransfer);
                }

                initializer.TraceMe("transferStatus: "+ transferStatus);
                equipmentBL.updateEquipmentTransferType(deserializedParameter.equipmentInformation.equipmentId, transferType);
                // update equipment status with in transfer
                
                initializer.TraceMe("update equipment end");
                #endregion update Equipment

                #region create damages
                DamageBL damageBL = new DamageBL(initializer.Service, initializer.TracingService);
                damageBL.createDamages(new CreateDamageParameter
                {
                    transferId = deserializedParameter.transferId,
                    equipmentId = deserializedParameter.equipmentInformation.equipmentId,
                    damageData = deserializedParameter.damageList,
                    userInformation = deserializedParameter.userInformation
                });
                #endregion create damages

                #region update transfer 


                TransferBL transferBL = new TransferBL(initializer.Service, initializer.TracingService);
                transferBL.updateTransferForDelivery(deserializedParameter.transferId,
                                                     deserializedParameter.equipmentInformation.currentKmValue,
                                                     deserializedParameter.equipmentInformation.currentFuelValue,
                                                     transferStatus);
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
