using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class BranchRepository : RepositoryHandler
    {
        public BranchRepository(IOrganizationService Service) : base(Service)
        {
        }

        public BranchRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public BranchRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public BranchRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public List<BranchData> getActiveBranchs()
        {
            QueryExpression expression = new QueryExpression("rnt_branch");
            expression.ColumnSet = new ColumnSet(new string[] {"rnt_telephone",
                                                               "rnt_name",
                                                               "rnt_cityid",
                                                               "rnt_regionmanagerid",
                                                               "rnt_address",
                                                               "rnt_emailaddress",
                                                               "rnt_longitude",
                                                               "rnt_latitude" ,
                                                               "rnt_seokeyword",
                                                               "rnt_soedescription",
                                                               "rnt_seotitle",
                                                               "rnt_branchzone",
                                                               "rnt_earlistpickuptime",
                                                               "rnt_branchtype"});


            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.buildBranchs(this.retrieveMultiple(expression).Entities);
        }

        public List<BranchData> getActiveBranchsByBranchIds(object[] branchIds)
        {
            QueryExpression expression = new QueryExpression("rnt_branch");
            expression.ColumnSet = new ColumnSet(new string[] {"rnt_telephone",
                                                               "rnt_name",
                                                               "rnt_cityid",
                                                               "rnt_address",
                                                               "rnt_emailaddress",
                                                               "rnt_longitude",
                                                               "rnt_latitude" ,
                                                               "rnt_seokeyword",
                                                               "rnt_soedescription",
                                                               "rnt_seotitle",
                                                               "rnt_earlistpickuptime",
                                                               "rnt_branchzone",
                                                               "rnt_branchtype",
                                                                "rnt_postalcode"});

            expression.Criteria.AddCondition("rnt_branchid", ConditionOperator.In, branchIds);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.buildBranchs(this.retrieveMultiple(expression).Entities);
        }

        public List<BranchData> buildBranchs(DataCollection<Entity> entities)
        {
            List<BranchData> data = new List<BranchData>();
            foreach (var item in entities)
            {
                BranchData b = new BranchData
                {
                    regionManagerId = item.Contains("rnt_regionmanagerid") ? (Guid?)item.GetAttributeValue<EntityReference>("rnt_regionmanagerid").Id : null,
                    regionManagerName = item.Contains("rnt_regionmanagerid") ? item.GetAttributeValue<EntityReference>("rnt_regionmanagerid").Name : null,
                    BranchId = Convert.ToString(item.Id),
                    BranchName = item.GetAttributeValue<string>("rnt_name"),
                    CityId = item.Attributes.Contains("rnt_cityid") ?
                                Convert.ToString(item.GetAttributeValue<EntityReference>("rnt_cityid").Id) : string.Empty,
                    CityName = item.Attributes.Contains("rnt_cityid") ?
                                item.GetAttributeValue<EntityReference>("rnt_cityid").Name : string.Empty,
                    addressDetail = item.GetAttributeValue<string>("rnt_address"),
                    emailaddress = item.GetAttributeValue<string>("rnt_emailaddress"),
                    latitude = item.Attributes.Contains("rnt_latitude") ? Convert.ToDouble(item["rnt_latitude"], new CultureInfo("en-US")) : Convert.ToDouble(0),
                    longitude = item.Attributes.Contains("rnt_longitude") ? Convert.ToDouble(item["rnt_longitude"], new CultureInfo("en-US")) : Convert.ToDouble(0),
                    telephone = item.GetAttributeValue<string>("rnt_telephone"),
                    seoDescription = item.GetAttributeValue<string>("rnt_soedescription"),
                    postalCode = item.GetAttributeValue<string>("rnt_postalcode"),
                    seoKeyword = item.GetAttributeValue<string>("rnt_seokeyword"),
                    seoTitle = item.GetAttributeValue<string>("rnt_seotitle"),
                    earlistPickupTime = item.Attributes.Contains("rnt_earlistpickuptime") ? item.GetAttributeValue<int>("rnt_earlistpickuptime") : 0,
                    branchZone = item.Attributes.Contains("rnt_branchzone") ? item.GetAttributeValue<OptionSetValue>("rnt_branchzone").Value : 0,
                    branchType = item.Attributes.Contains("rnt_branchtype") ? item.GetAttributeValue<OptionSetValue>("rnt_branchtype").Value : 0,
                };
                data.Add(b);
            }

            return data;
        }
        public List<Entity> getBranchByBusinessUnitId(Guid businessUnitId)
        {
            QueryExpression expression = new QueryExpression("rnt_branch");
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            expression.Criteria.AddCondition("rnt_businessunitid", ConditionOperator.Equal, businessUnitId);
            return this.retrieveMultiple(expression).Entities.ToList();
        }
        public Entity getBranchById(Guid branchId)
        {
            return this.retrieveById("rnt_branch", branchId, true);
        }
        public Entity getBranchById(Guid branchId,string[] columns)
        {
            return this.retrieveById("rnt_branch", branchId, columns);
        }
        public int? getEarlistPickupTimeByBranchId(Guid branchId)
        {
            QueryExpression expression = new QueryExpression("rnt_branch");
            expression.ColumnSet = new ColumnSet(new string[] { "rnt_earlistpickuptime" });

            expression.Criteria.AddCondition("rnt_branchid", ConditionOperator.In, branchId);
            expression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            var branch = this.retrieveMultiple(expression).Entities.FirstOrDefault();

            return branch.GetAttributeValue<int>("rnt_earlistpickuptime");
        }
    }

}


