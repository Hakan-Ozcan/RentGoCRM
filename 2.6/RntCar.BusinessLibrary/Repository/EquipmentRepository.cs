using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class EquipmentRepository : RepositoryHandler
    {
        public EquipmentRepository(IOrganizationService Service) : base(Service)
        {
        }

        public EquipmentRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public EquipmentRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public EquipmentRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public EquipmentRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        public Entity getEquipmentByHGSNumber(string hgsNumber)
        {
            QueryExpression query = new QueryExpression("rnt_equipment");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_hgsnumber", ConditionOperator.Equal, hgsNumber);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public string getEquipmentHGSNumber(Guid equipmentId)
        {
            var entity = this.retrieveById("rnt_equipment", equipmentId, new string[] { "rnt_hgsnumber" });
            return entity.GetAttributeValue<string>("rnt_hgsnumber");
        }
        public Entity getEquipmentByIdByGivenColumns(Guid equipmentId, string[] columns)
        {
            return this.retrieveById("rnt_equipment", equipmentId, columns);
        }
        public Entity getEquipmentById(Guid equipmentId)
        {
            return this.retrieveById("rnt_equipment", equipmentId,true);
        }
        public Entity getEquipmentLastKmandFuelInformation(Guid equipmentId)
        {
            return this.retrieveById("rnt_equipment", equipmentId, new string[] { "rnt_fuelcode", "rnt_currentkm" });
        }

        public List<Entity> getEquipmentsByProductId(Guid productId)
        {
            QueryExpression query = new QueryExpression("rnt_equipment");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_product", ConditionOperator.Equal, productId);
            return this.retrieveMultiple(query).Entities.ToList();
        }

        public List<Entity> getAllActiveEquipments(string[] columns)
        {
            QueryExpression query = new QueryExpression("rnt_equipment");
            query.ColumnSet = new ColumnSet(columns);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getAllActiveEquipments()
        {
            QueryExpression query = new QueryExpression("rnt_equipment");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(query).Entities.ToList();
        }
        public List<Entity> getAllActiveEquipmentsWithProducts()
        {
            LinkEntity contractLinkEntity = new LinkEntity();
            contractLinkEntity.JoinOperator = JoinOperator.LeftOuter;
            contractLinkEntity.EntityAlias = "contractLink";
            contractLinkEntity.LinkFromEntityName = "rnt_equipment";
            contractLinkEntity.LinkFromAttributeName = "rnt_equipmentid";
            contractLinkEntity.LinkToEntityName = "rnt_contract";
            contractLinkEntity.LinkToAttributeName = "rnt_equipmentid";
            contractLinkEntity.Columns = new ColumnSet("rnt_pnrnumber");
            contractLinkEntity.LinkCriteria.AddCondition("statuscode",ConditionOperator.Equal, 100000000);

            LinkEntity transferLinkEntity = new LinkEntity();
            transferLinkEntity.JoinOperator = JoinOperator.LeftOuter;
            transferLinkEntity.EntityAlias = "transferLink";
            transferLinkEntity.LinkFromEntityName = "rnt_equipment";
            transferLinkEntity.LinkFromAttributeName = "rnt_equipmentid";
            transferLinkEntity.LinkToEntityName = "rnt_transfer";
            transferLinkEntity.LinkToAttributeName = "rnt_equipmentid";
            transferLinkEntity.Columns = new ColumnSet("rnt_transfernumber");
            transferLinkEntity.LinkCriteria.AddCondition("statuscode", ConditionOperator.Equal, 100000001);


            var equipmentQuery = new QueryExpression("rnt_equipment");

            equipmentQuery.ColumnSet = new ColumnSet(true);
            equipmentQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            equipmentQuery.LinkEntities.Add(new LinkEntity("rnt_equipment", "rnt_product", "rnt_product", "rnt_productid", JoinOperator.Inner));
            equipmentQuery.LinkEntities[0].Columns.AddColumns("rnt_fueltypecode", "rnt_name",
                                                               "rnt_productid", "rnt_fueltankcapacity",
                                                               "rnt_brandid", "rnt_gearbox", "rnt_groupcodeid",
                                                               "rnt_maintenanceperiod", "rnt_modelid");
            equipmentQuery.LinkEntities[0].EntityAlias = "products";

           
            return this.retrieveMultiple(equipmentQuery).Entities.ToList();


          

        }

        public List<Entity> getAllEquipments(string[] columns)
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression query = new QueryExpression("rnt_equipment");
                query.ColumnSet = new ColumnSet(columns);
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var reservationItems = this.retrieveMultiple(query);
                result.AddRange(reservationItems.Entities.ToList());
                if (reservationItems.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = reservationItems.PagingCookie;
                }
                else
                {
                    break;
                }
            }


            return result;


        }
        public List<Entity> getAllEquipments()
        {
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {
                QueryExpression query = new QueryExpression("rnt_equipment");
                query.ColumnSet = new ColumnSet(true);
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                var reservationItems = this.retrieveMultiple(query);
                result.AddRange(reservationItems.Entities.ToList());
                if (reservationItems.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = reservationItems.PagingCookie;
                }
                else
                {
                    break;
                }
            }


            return result;


        }
        public Guid getEquipmentGroupCodeIdByEquipmentId(Guid equipmentId)
        {
            QueryExpression equipmentQuery = new QueryExpression("rnt_equipment");
            equipmentQuery.Criteria.AddCondition("rnt_equipmentid", ConditionOperator.Equal, equipmentId);
            equipmentQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            equipmentQuery.ColumnSet = new ColumnSet(new string[] { "rnt_product"} );

            var equipment = this.retrieveMultiple(equipmentQuery).Entities.FirstOrDefault();
            var productId = equipment.GetAttributeValue<EntityReference>("rnt_product").Id;

            QueryExpression productQuery = new QueryExpression("rnt_product");
            productQuery.Criteria.AddCondition("rnt_productid", ConditionOperator.Equal, productId);
            productQuery.ColumnSet = new ColumnSet(new string[] { "rnt_groupcodeid" } );

            var product = this.retrieveMultiple(productQuery).Entities.FirstOrDefault();
            var groupCodeId = product.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id;

            return groupCodeId;
        }

        public EntityCollection getEquipmentsByStatusCode(string branchId, List<int> statusCodes)
        {
            QueryExpression query = new QueryExpression("rnt_equipment");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_currentbranchid", ConditionOperator.Equal, branchId);
            query.Criteria.AddCondition("statuscode", ConditionOperator.In, statusCodes.ConvertAll(p => Convert.ToString(p)).ToArray());

            return retrieveMultiple(query);
        }
    }
}
