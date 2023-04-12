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

namespace RntCar.CrmPlugins.Equipment.Actions
{
    public class ExecuteUpdateEquipmentInformation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string updateEquipmentInformationParameter;
                initializer.PluginContext.GetContextParameter<string>("UpdateEquipmentInformationParameter", out updateEquipmentInformationParameter);
                var deserializedParameter = JsonConvert.DeserializeObject<UpdateEquipmentInformationParameter>(updateEquipmentInformationParameter);

                #region update equipment transaction history
                EquipmentRepository equipmentRepository = new EquipmentRepository(initializer.Service);
                var currentEquipmentTransaction = equipmentRepository.getEquipmentByIdByGivenColumns(deserializedParameter.equipmentInformation.equipmentId,
                                                                                                     new string[] { "rnt_equipmenttransactionid" });
                EquipmentTransactionBL equipmentTransactionBL = new EquipmentTransactionBL(initializer.Service, initializer.TracingService);
                equipmentTransactionBL.updateEquipmentTransactionHistoryforRental(new CreateEquipmentTransactionHistoryRentalParameters
                {
                    firstFuelValue = deserializedParameter.equipmentInformation.firstFuelValue,
                    firstKmValue = deserializedParameter.equipmentInformation.firstKmValue,
                    equipmentId = deserializedParameter.equipmentInformation.equipmentId,
                    rentalFuelValue = deserializedParameter.equipmentInformation.currentFuelValue,
                    rentalKmValue = deserializedParameter.equipmentInformation.currentKmValue,
                    equipmentTransactionId = currentEquipmentTransaction.GetAttributeValue<EntityReference>("rnt_equipmenttransactionid").Id
                });
                #endregion update equipment transaction history

                #region create equipment inventory history
                //EquipmentInventoryBL equipmentInventoryBL = new EquipmentInventoryBL(initializer.Service, initializer.TracingService);
                //equipmentInventoryBL.updateEquipmentInventoryHistories(deserializedParameter.equipmentInformation.equipmentInventoryData);
                #endregion create equipment inventory history                                                                                 

                #region update equipment 
                initializer.TraceMe("updateEquipmentforRental start");

                EquipmentBL equipmentBL = new EquipmentBL(initializer.Service);
                equipmentBL.updateEquipmentInformation(deserializedParameter.equipmentInformation.equipmentId,
                                                       null,
                                                       null,
                                                       deserializedParameter.equipmentInformation.currentKmValue,
                                                       deserializedParameter.equipmentInformation.currentFuelValue,
                                                       deserializedParameter.statusCode);
                #endregion update equipment
            }
            catch (Exception)
            {
            }
            throw new NotImplementedException();
        }
    }
}
