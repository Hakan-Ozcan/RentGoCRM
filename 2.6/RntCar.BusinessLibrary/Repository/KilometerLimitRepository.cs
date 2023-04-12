using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class KilometerLimitRepository : RepositoryHandler
    {
        public KilometerLimitRepository(IOrganizationService Service) : base(Service)
        {
        }

        public KilometerLimitRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }
        public Entity getDailyKilometerLimitsByReservationDayandGroupCode(int reservationDuration, Guid groupCodeInformationId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_kilometerlimit");
            queryExpression.ColumnSet = new ColumnSet("rnt_kmlimit");
            queryExpression.Criteria.AddCondition("rnt_groupcodeinformationid", ConditionOperator.Equal, Convert.ToString(groupCodeInformationId));
            queryExpression.Criteria.AddCondition("rnt_durationcode", ConditionOperator.Equal, (int)GlobalEnums.DurationCode.Daily);
            queryExpression.Criteria.AddCondition("rnt_maximumday", ConditionOperator.Equal, reservationDuration);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }

        public Entity getMonthlyKilometerLimitsByGroupCode(Guid groupCodeInformationId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_kilometerlimit");
            queryExpression.ColumnSet = new ColumnSet("rnt_kmlimit");
            queryExpression.Criteria.AddCondition("rnt_groupcodeinformationid", ConditionOperator.Equal, Convert.ToString(groupCodeInformationId));
            queryExpression.Criteria.AddCondition("rnt_durationcode", ConditionOperator.Equal, (int)GlobalEnums.DurationCode.Monthly);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();

        }
    }
}
