using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class AvailabilityFactorsRepository : RepositoryHandler
    {
        public AvailabilityFactorsRepository(IOrganizationService Service) : base(Service)
        {
        }

        public AvailabilityFactorsRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {

        }
        public List<Entity> getActiveAvailabilityFactors()
        {
            QueryExpression query = new QueryExpression("rnt_availabilityfactors");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            return this.retrieveMultiple(query).Entities.ToList();


        }
        public Entity getMinimumDayAvailabilityByChannel(DateTime beginDate,DateTime endDate,int channel)
        {
            QueryExpression query = new QueryExpression("rnt_availabilityfactors");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

            query.Criteria.AddCondition("rnt_availabilityfactortypecode", ConditionOperator.Equal, (int)GlobalEnums.AvailabilityFactorType.MinimumReservationDay);
            query.Criteria.AddCondition("rnt_begindate", ConditionOperator.LessThan, beginDate);
            query.Criteria.AddCondition("rnt_enddate", ConditionOperator.GreaterThan, beginDate);
            query.Criteria.AddCondition("rnt_begindate", ConditionOperator.LessThan, endDate);
            query.Criteria.AddCondition("rnt_enddate", ConditionOperator.GreaterThan, endDate);
            query.Criteria.AddCondition("rnt_channelcode", ConditionOperator.ContainValues, channel);

            return this.Service.RetrieveMultiple(query).Entities.FirstOrDefault();


        }

    }
}
