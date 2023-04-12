using Microsoft.Xrm.Sdk;
using MongoDB.Driver;
using RntCar.MongoDBHelper;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.UpdateEquipmentsStatus
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            var mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
            var mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");

            MongoDBHelper.Repository.EquipmentRepository equipmentRepositoryMongo = new MongoDBHelper.Repository.EquipmentRepository(mongoDBHostName, mongoDBDatabaseName);
            BusinessLibrary.Repository.EquipmentRepository equipmentRepositoryCrm = new BusinessLibrary.Repository.EquipmentRepository(crmServiceHelper.IOrganizationService);


            var collection = equipmentRepositoryMongo.getCollection<EquipmentDataMongoDB>(StaticHelper.GetConfiguration("MongoDBEquipmentsCollectionName"));
            var allEquipmentsMongo = equipmentRepositoryMongo.getAllEquipments();
            var allEquipmentsCrm = equipmentRepositoryCrm.getAllEquipments(new string[] { "rnt_equipmentid", "statecode", "statuscode" });
            foreach (var equipmentCrm in allEquipmentsCrm)
            {
                try
                {
                    var equipmentId = equipmentCrm.Id;
                    var statecode = equipmentCrm.GetAttributeValue<OptionSetValue>("statecode").Value;
                    var statuscode = equipmentCrm.GetAttributeValue<OptionSetValue>("statuscode").Value;

                    var document = allEquipmentsMongo.Where(p => p.EquipmentId == Convert.ToString(equipmentId)).FirstOrDefault();
                    document.StateCode = statecode;
                    document.StatusCode = statuscode;

                    var filter = Builders<EquipmentDataMongoDB>.Filter.Eq(s => s.EquipmentId, Convert.ToString(equipmentId));
                    var result = collection.Replace(document, filter, new UpdateOptions { IsUpsert = false }, document.EquipmentId);
                }
                catch
                {
                    continue;
                }
            }            
        }
    }
}
