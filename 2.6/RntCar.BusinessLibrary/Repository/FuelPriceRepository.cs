using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class FuelPriceRepository : RepositoryHandler
    {
        public FuelPriceRepository(IOrganizationService Service) : base(Service)
        {
        }

        public FuelPriceRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public FuelPriceRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public FuelPriceRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public FuelPriceRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fuelType"></param>
        /// <param name="offset">BeginDate and EndDate are timezoneindependent.
        /// offset will be added to Datetime.utcnow</param>
        /// <returns></returns>
        public Entity getFuelPriceByFuelTypeAllColumns(int fuelType)
        {

            QueryExpression queryExpression = new QueryExpression("rnt_fuelprice");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            queryExpression.Criteria.AddCondition("rnt_fueltypecode", ConditionOperator.Equal, fuelType);
            queryExpression.Criteria.AddCondition(new ConditionExpression("rnt_begindate", ConditionOperator.OnOrBefore, DateTime.UtcNow.AddMinutes(StaticHelper.offset)));
            queryExpression.Criteria.AddCondition(new ConditionExpression("rnt_enddate", ConditionOperator.OnOrAfter, DateTime.UtcNow.AddMinutes(StaticHelper.offset)));           

            return this.retrieveMultiple(queryExpression).Entities.FirstOrDefault();
        }
    }
}
