using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;

namespace RntCar.MongoDBRecordUpdate
{
    public class MongoDBRecordUpdateHelper
    {
        private IOrganizationService Service;
        public MongoDBRecordUpdateHelper(IOrganizationService _service)
        {
            Service = _service;
        }
        private void updateEquipmentsByGroupCodeInformationId(Guid groupCodeInformationId)
        {
            ProductRepository productRepository = new ProductRepository(this.Service);
            //equipment not directly related with groupcodeinformation
            //get products by group code id
            var products = productRepository.getProductsByGroupCodeInformationId(groupCodeInformationId);
            foreach (var product in products)
            {
                this.updateEquipmentsByProductId(product.Id);
            }
        }
        private void updateEquipmentsByProductId(Guid productId)
        {
            EquipmentRepository equipmentRepository = new EquipmentRepository(this.Service);
            //get equipment by product id
            var equipments = equipmentRepository.getEquipmentsByProductId(productId);
            foreach (var equipment in equipments)
            {
                //update equipment mongo db trigger field for mongo db update
                this.updateEquipmentMongoDBTriggerField(equipment.Id);
            }
        }
        private void updateEquipmentMongoDBTriggerField(Guid id)
        {
            Entity entity = new Entity("rnt_equipment");
            entity["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(10);
            entity.Id = id;
            this.Service.Update(entity);
        }
        private void deactiveRecord(Guid id)
        {
            XrmHelper xrmHelper = new XrmHelper(this.Service);
            xrmHelper.setState("rnt_updatedrecordsmongodb", id, 1, 2);
        }

        public void updateEquipments()
        {
            MongoDBUpdateRecordsRepository mongoDBUpdateRecordsRepository = new MongoDBUpdateRecordsRepository(this.Service);
            var records = mongoDBUpdateRecordsRepository.getAllActiveRecords();

            foreach (var item in records)
            {
                if (item.Attributes.Contains("rnt_groupcodeinformationid"))
                {
                    this.updateEquipmentsByGroupCodeInformationId(item.GetAttributeValue<EntityReference>("rnt_groupcodeinformationid").Id);
                }
                else if(item.Attributes.Contains("rnt_productid"))
                {
                    this.updateEquipmentsByProductId(item.GetAttributeValue<EntityReference>("rnt_productid").Id);
                }
                //deactive record after update process
                this.deactiveRecord(item.Id);
            }
        }
    }
}
