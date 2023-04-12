using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Repository
{
    public class SMSContentRepository : RepositoryHandler
    {
        public SMSContentRepository(IOrganizationService Service) : base(Service)
        {
        }

        public SMSContentRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {

        }

        public Entity getSMSContentByCodeandLangId(int contentCode, int langId)
        {
            QueryExpression query = new QueryExpression("rnt_smscontent");
            query.ColumnSet = new ColumnSet("rnt_message");
            query.Criteria.AddCondition("rnt_smscontentcode", ConditionOperator.Equal, contentCode);
            query.Criteria.AddCondition("rnt_langid", ConditionOperator.Equal, langId);

            var smsEntity = this.Service.RetrieveMultiple(query).Entities.FirstOrDefault();
            return smsEntity;
        }
    }
}
