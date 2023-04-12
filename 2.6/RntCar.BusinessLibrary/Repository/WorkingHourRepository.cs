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
    public class WorkingHourRepository : RepositoryHandler
    {
        public WorkingHourRepository(IOrganizationService Service) : base(Service)
        {
        }

        public WorkingHourRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public WorkingHourRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public WorkingHourRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public WorkingHourData GetWorkingHourByBranchIdAndSelectedDay(Guid branchId, int dayCode)
        {
            QueryExpression query = new QueryExpression("rnt_workinghour");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_branchid", ConditionOperator.Equal, branchId);
            query.Criteria.AddCondition("rnt_daycode", ConditionOperator.Equal, dayCode);
            var result = this.retrieveMultiple(query).Entities.FirstOrDefault();

            if (result == null)
                return null;

            return new WorkingHourData
            {
                WorkingHourId = result.Id,
                BranchId = result.GetAttributeValue<EntityReference>("rnt_branchid").Id,
                BranchName = result.GetAttributeValue<EntityReference>("rnt_branchid").Name,
                DayCode = result.GetAttributeValue<OptionSetValue>("rnt_daycode").Value,
                BeginingTime = result.GetAttributeValue<OptionSetValue>("rnt_beginingtimecode").Value,
                EndTime = result.GetAttributeValue<OptionSetValue>("rnt_endtimecode").Value
            };
        }

        public List<Entity> GetWorkingHourByBranchId(Guid branchId)
        {
            QueryExpression query = new QueryExpression("rnt_workinghour");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition("rnt_branchid", ConditionOperator.Equal, branchId);
            var result = this.retrieveMultiple(query).Entities.ToList();

            if (result == null)
                return null;
            
            return result;
        }

        public List<WorkingHourData> getWorkingHours()
        {
            QueryExpression query = new QueryExpression("rnt_workinghour");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            var result = this.retrieveMultiple(query).Entities.ToList();

            List<WorkingHourData> workingHours = new List<WorkingHourData>();
            foreach (var item in result)
            {
                var _workingHour = new WorkingHourData
                {
                    WorkingHourId = item.Id,
                    BranchId = item.GetAttributeValue<EntityReference>("rnt_branchid").Id,
                    BranchName = item.GetAttributeValue<EntityReference>("rnt_branchid").Name,
                    DayCode = item.GetAttributeValue<OptionSetValue>("rnt_daycode").Value,
                    BeginingTime = item.GetAttributeValue<OptionSetValue>("rnt_beginingtimecode").Value == 1429 ? 1439 : item.GetAttributeValue<OptionSetValue>("rnt_beginingtimecode").Value,
                    EndTime = item.GetAttributeValue<OptionSetValue>("rnt_endtimecode").Value == 1429 ? 1439 : item.GetAttributeValue<OptionSetValue>("rnt_endtimecode").Value
                };
                workingHours.Add(_workingHour);
            }
            return workingHours;
        }
    }
}
