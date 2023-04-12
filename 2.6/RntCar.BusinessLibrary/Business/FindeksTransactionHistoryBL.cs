using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Business
{
    // Tolga AYKURT - 21.03.2019
    public class FindeksTransactionHistoryBL : BusinessHandler
    {
        #region CONSTRUCTORS
        public FindeksTransactionHistoryBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public FindeksTransactionHistoryBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public FindeksTransactionHistoryBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public FindeksTransactionHistoryBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }
        #endregion

        #region METHODS
        // Tolga AYKURT - 21.03.2019
        public void Create(Entity findeksTransactionHistory)
        {
            try
            {
                this.OrgService.Create(findeksTransactionHistory);
            }
            catch(Exception ex)
            {
                this.Trace(ex.Message);
            }
        }

        // Tolga AYKURT - 21.03.2019
        public void Create(Entity targetEntity, Entity preImageEntity)
        {
            try
            {
                var findeksTransactionHistoryEntity = CreateFindeksTransactionEntity(targetEntity, preImageEntity);
                this.OrgService.Create(findeksTransactionHistoryEntity);
            }
            catch (Exception ex)
            {
                this.Trace(ex.Message);
            }
        }
        #endregion

        #region HELPER METHODS
        // Tolga AYKURT - 21.03.2019
        private Entity CreateFindeksTransactionEntity(Entity targetEntity, Entity preImageEntity)
        {
            var findeksTransactionHistoryEntity = new Entity("rnt_findekstransactionhistory");

            if (preImageEntity.Attributes.Contains("rnt_findekspoint") == true &&
                targetEntity.Attributes.Contains("rnt_findekspoint") == true)
            {
                findeksTransactionHistoryEntity.Attributes["rnt_newfindeksvalue"] = targetEntity.GetAttributeValue<int>("rnt_findekspoint");
                findeksTransactionHistoryEntity.Attributes["rnt_oldfindeksvalue"] = preImageEntity.GetAttributeValue<int>("rnt_findekspoint");
                findeksTransactionHistoryEntity.Attributes["createdby"] = preImageEntity.GetAttributeValue<EntityReference>("createdby");
                findeksTransactionHistoryEntity.Attributes["createdon"] = preImageEntity.GetAttributeValue<DateTime>("createdon");
            }
            else if (targetEntity.Attributes.Contains("rnt_findekspoint") == true &&
                    preImageEntity.Attributes.Contains("rnt_findekspoint") == false)
            {
                findeksTransactionHistoryEntity.Attributes["rnt_newfindeksvalue"] = targetEntity.GetAttributeValue<int>("rnt_findekspoint");
                findeksTransactionHistoryEntity.Attributes["rnt_oldfindeksvalue"] = 0;
                findeksTransactionHistoryEntity.Attributes["createdby"] = targetEntity.GetAttributeValue<EntityReference>("createdby");
                findeksTransactionHistoryEntity.Attributes["createdon"] = targetEntity.GetAttributeValue<DateTime>("createdon");
            }

            findeksTransactionHistoryEntity.Attributes["rnt_contactid"] = new EntityReference("contact", targetEntity.Id);
            findeksTransactionHistoryEntity.Attributes["rnt_findekscreationtype"] = targetEntity.Attributes.Contains("rnt_findekscreationtype") == true ? targetEntity.GetAttributeValue<OptionSetValue>("rnt_findekscreationtype") : new OptionSetValue(2); // 1: Integration, 2: Manual

            return findeksTransactionHistoryEntity;
        }
        #endregion
    }
}
