﻿using Microsoft.Xrm.Sdk;
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
    public class AdditionalProductKilometerLimitActionRepository : RepositoryHandler
    {
        public AdditionalProductKilometerLimitActionRepository(IOrganizationService Service) : base(Service)
        {
        }

        public Entity getAdditionalProductKilometerLimitActionByAdditionalProductId(Guid additionalProductId)
        {
            //Bu metot, rnt_additionalproductkilometerlimitaction tablosundan rnt_additionalproductid sütununun verilen Guid değerine eşit olan verileri seçer. Bu seçim işlemini gerçekleştirmek için QueryExpression nesnesi kullanılır. Daha sonra, "rnt_kilometerlimiteffect" sütunu belirtilir ve sadece etkin kayıtlar seçilir. Seçim işlemi RetrieveMultiple metodu kullanılarak gerçekleştirilir ve ilk kayıt geri döndürülür.
            QueryExpression query = new QueryExpression("rnt_additionalproductkilometerlimitaction");
            query.ColumnSet = new ColumnSet("rnt_kilometerlimiteffect");
            query.Criteria.AddCondition("rnt_additionalproductid", ConditionOperator.Equal, additionalProductId);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.FirstOrDefault();
        }
        public List<Entity> getAdditionalProductKilometerLimitActions()
        {
            QueryExpression query = new QueryExpression("rnt_additionalproductkilometerlimitaction");
            query.ColumnSet = new ColumnSet(true);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(query).Entities.ToList();
        }
    }
}
