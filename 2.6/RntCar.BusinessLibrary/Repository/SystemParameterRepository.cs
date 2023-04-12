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
    public class SystemParameterRepository : RepositoryHandler
    {
        public SystemParameterRepository(IOrganizationService Service) : base(Service)
        {
        }

        public SystemParameterRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public SystemParameterRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public Entity getSystemParameters()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet(true);
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault();
        }

        public int getTaxRatio()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_taxratio");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault().GetAttributeValue<int>("rnt_taxratio");
        }

        public int getProvider()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_defaultprovider");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault().GetAttributeValue<OptionSetValue>("rnt_defaultprovider").Value;
        }

        public Entity getReservationRelatedSystemParameters()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_reservationcancellationduration", "rnt_reservationfineduration", "rnt_reservationnoshowduration");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault();
        }
        public Entity getCampaignPegasusEnabled()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_pegasuscampaingnenabled");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault();
        }
        public Entity getIsInstallmentEnabled()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_isinstallmentenabled");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault();
        }
        public Entity getContractRelatedSystemParameters()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_quickcontractminduration", "rnt_contractcancellationduration", "rnt_contractminduration", "rnt_checkuserbranchforcontractcreate");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault();
        }

        public int getReservationShiftDuration()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_reservationshiftduration");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault().GetAttributeValue<int>("rnt_reservationshiftduration");
        }

        public int getCustomerCreditCardLimit()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_customercreditcardlimit");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault().GetAttributeValue<int>("rnt_customercreditcardlimit");
        }

        public int getCustomerExpireDay()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_customerexpireday");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault().GetAttributeValue<int>("rnt_customerexpireday");
        }

        public Entity getMaintenanceRelatedSystemParameters()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_maintenanceinformkm", "rnt_maintenancelimitkm", "rnt_maintenanceinformday");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault();
        }

        public int getMonthlyKilometerLimit()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_kilometerlimitmaximumdayfordaily");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault().GetAttributeValue<int>("rnt_kilometerlimitmaximumdayfordaily");
        }

        public int getKilometerLimitTolerance()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_kilometerlimittolerance");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault().GetAttributeValue<int>("rnt_kilometerlimittolerance");
        }


        public EntityReference getAdminId()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_systemuserid");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault().GetAttributeValue<EntityReference>("rnt_systemuserid");
        }

        public OptionSetValueCollection getExists3dForChannel()
        {
            QueryExpression exp = new QueryExpression("rnt_systemparameter");
            exp.ColumnSet = new ColumnSet("rnt_exists3dforchannel");
            exp.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            return this.retrieveMultiple(exp).Entities.FirstOrDefault().GetAttributeValue<OptionSetValueCollection>("rnt_exists3dforchannel");
        }
    }
}
