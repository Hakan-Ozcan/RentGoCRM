using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class ProductRepository : RepositoryHandler
    {
        public ProductRepository(IOrganizationService Service) : base(Service)
        {
        }

        public ProductRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public ProductRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public ProductRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public ProductRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getProductById(Guid id)
        {
            return this.retrieveById("rnt_product", id);
        }
        public Entity getProductByIdWithGivenColumns(Guid id,string[] columns)
        {
            return this.retrieveById("rnt_product", id, columns);
        }

        public List<Entity> getProductsByGroupCodeInformationId(Guid groupCodeInformationId)
        {
            QueryExpression query = new QueryExpression("rnt_product");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_groupcodeid", ConditionOperator.Equal, groupCodeInformationId);
            return this.retrieveMultiple(query).Entities.ToList();
        }

        public ProductData getProductByEquipmentId(Guid equipmentId)
        {
            var equipmentQuery = new QueryExpression("rnt_product");
            equipmentQuery.LinkEntities.Add(new LinkEntity( "rnt_product", "rnt_equipment", "rnt_productid", "rnt_product", JoinOperator.Inner));
            equipmentQuery.ColumnSet = new ColumnSet("rnt_fueltypecode", "rnt_name",
                                                               "rnt_productid", "rnt_fueltankcapacity",
                                                               "rnt_brandid", "rnt_gearbox", "rnt_groupcodeid",
                                                               "rnt_maintenanceperiod", "rnt_modelid");
        
            equipmentQuery.LinkEntities[0].LinkCriteria.AddCondition("rnt_equipmentid", ConditionOperator.Equal, equipmentId);
            var equipment = this.retrieveMultiple(equipmentQuery).Entities.FirstOrDefault();
            return new ProductData
            {
                fuelTypeCode = equipment.GetAttributeValue<OptionSetValue>("rnt_fueltypecode") != null ? equipment.GetAttributeValue<OptionSetValue>("rnt_fueltypecode").Value : 0,
                fuelTypeName = equipment.GetAttributeValue<OptionSetValue>("rnt_fueltypecode") != null ? equipment.FormattedValues["rnt_fueltypecode"] : string.Empty,
                productId = equipment.GetAttributeValue<Guid>("rnt_productid"),
                product = equipment.GetAttributeValue<string>("rnt_name"),
                tankCapacity = equipment.GetAttributeValue<decimal>("rnt_fueltankcapacity"),
                brand = equipment.GetAttributeValue<EntityReference>("rnt_brandid") != null ? equipment.GetAttributeValue<EntityReference>("rnt_brandid").Name : string.Empty,
                gearBox = equipment.GetAttributeValue<OptionSetValue>("rnt_gearbox") != null ? equipment.GetAttributeValue<OptionSetValue>("rnt_gearbox").Value : 0,
                gearBoxName = equipment.GetAttributeValue<OptionSetValue>("rnt_gearbox") != null ? equipment.FormattedValues["rnt_gearbox"] : string.Empty,
                groupCode = equipment.GetAttributeValue<EntityReference>("rnt_groupcodeid") != null ? equipment.GetAttributeValue<EntityReference>("rnt_groupcodeid").Name : string.Empty,
                maintenancePeriod = equipment.GetAttributeValue<OptionSetValue>("rnt_maintenanceperiod") != null ? equipment.GetAttributeValue<OptionSetValue>("rnt_maintenanceperiod").Value : 0,
                maintenancePeriodName = equipment.GetAttributeValue<OptionSetValue>("rnt_maintenanceperiod") != null ? equipment.FormattedValues["rnt_maintenanceperiod"] : string.Empty,
                model= equipment.GetAttributeValue<EntityReference>("rnt_modelid") != null ? equipment.GetAttributeValue<EntityReference>("rnt_modelid").Name : string.Empty,
            };            
        }

        public Entity getProductByEquipmentId(Guid equipmentId, string[] columns)
        {
            var equipmentQuery = new QueryExpression("rnt_equipment");
            equipmentQuery.ColumnSet = new ColumnSet("rnt_product");
            equipmentQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            equipmentQuery.Criteria.AddCondition("rnt_equipmentid", ConditionOperator.Equal, equipmentId);

            var equipment = this.retrieveMultiple(equipmentQuery).Entities.FirstOrDefault();
            var productId = equipment.GetAttributeValue<EntityReference>("rnt_product").Id;

            var productQuery = new QueryExpression("rnt_product");
            productQuery.ColumnSet = new ColumnSet(columns);
            productQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            productQuery.Criteria.AddCondition("rnt_productid", ConditionOperator.Equal, productId);

            return this.retrieveMultiple(productQuery).Entities.FirstOrDefault();
        }

        public List<Entity> getAllProducts()
        {
            QueryExpression query = new QueryExpression("rnt_product");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.ToList();
        }
    }
}
