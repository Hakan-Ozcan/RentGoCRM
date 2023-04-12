using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.BusinessLibrary.Business
{
    public class HGSTransitListBL : BusinessHandler
    {
        public HGSTransitListBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public HGSTransitListBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public HGSTransitListBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public HGSTransitListBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public HGSTransitListBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public HGSTransitListBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public HGSTransitListBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public HGSTransitListBL(IOrganizationService orgService, ITracingService tracingService, CrmServiceClient crmServiceClient) : base(orgService, tracingService, crmServiceClient)
        {
        }

        public HGSTransitListBL(IOrganizationService orgService, Guid userId, string orgName) : base(orgService, userId, orgName)
        {
        }

        public Guid createHGSList(HGSTransitData data, Guid contractId, Guid contractItemId, Guid equipmentId, Guid hgsLabelId)
        {
            Entity hgsTransit = new Entity("rnt_hgstransitlist");
            hgsTransit["rnt_hgsnumber"] = data.hgsNumber;
            hgsTransit["rnt_platenumber"] = data.plateNumber;
            hgsTransit["rnt_amount"] = new Money(data.amount);
            hgsTransit["rnt_description"] = data.description;
            if (data._entryDateTime != DateTime.MinValue && data._entryDateTime.Year > 2000)
            {
                hgsTransit["rnt_entrydatetime"] = data._entryDateTime;
            }
            hgsTransit["rnt_exitdatetime"] = data._exitDateTime;
            hgsTransit["rnt_entrylocation"] = data.entryLocation;
            hgsTransit["rnt_exitlocation"] = data.exitLocation;
            hgsTransit["rnt_integrationchannel"] = new OptionSetValue(1);
            hgsTransit["statuscode"] = new OptionSetValue((int)HGSTransitListStatusCode.NotInvoiced);
            hgsTransit["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
            hgsTransit["rnt_contractitemid"] = new EntityReference("rnt_contractitem", contractItemId);
            hgsTransit["rnt_equipmentid"] = new EntityReference("rnt_equipment", equipmentId);
            hgsTransit["rnt_hgslabelid"] = new EntityReference("rnt_hgslabel", hgsLabelId);
            return this.OrgService.Create(hgsTransit);
        }

        public EntityReference GetContractForHGSDate(DateTime entryDate, DateTime exitDate, EntityReference equipmentRef)
        {
            EntityReference contractRef = new EntityReference();
            if (entryDate == DateTime.MinValue || entryDate.Year < 2000)
            {
                entryDate = exitDate;
            }


            QueryExpression getContractItemQuery = new QueryExpression("rnt_contractitem");
            getContractItemQuery.ColumnSet = new ColumnSet("rnt_contractid", "rnt_pickupdatetime", "rnt_dropoffdatetime");
            getContractItemQuery.Criteria.AddCondition("rnt_pickupdatetime", ConditionOperator.OnOrBefore, entryDate);
            getContractItemQuery.Criteria.AddCondition("rnt_dropoffdatetime", ConditionOperator.OnOrAfter, exitDate);
            getContractItemQuery.Criteria.AddCondition("rnt_equipment", ConditionOperator.Equal, equipmentRef.Id);
            getContractItemQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            EntityCollection contractItemList = this.OrgService.RetrieveMultiple(getContractItemQuery);


            if (contractItemList.Entities.Count > 0)
            {
                foreach (var contract in contractItemList.Entities)
                {
                    DateTime pickupDate = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                    DateTime dropoffDate = contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                    if (dropoffDate >= exitDate && pickupDate <= entryDate)
                    {
                        contractRef = contract.GetAttributeValue<EntityReference>("rnt_contractid");
                        break;
                    }
                }
            }
            return contractRef;
        }

        public decimal GetHGSTotalAmountWithContractId(Guid contractId)
        {
            decimal hgsTotalAmount = 0;
            EntityCollection hgsTransitList = new EntityCollection();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;

            string pagingCookie = null;
            while (true)
            {
                LinkEntity ftpLinkEntity = new LinkEntity();
                ftpLinkEntity.EntityAlias = "ftpconsesusAlias";
                ftpLinkEntity.LinkFromAttributeName = "rnt_ftpconsensusid";
                ftpLinkEntity.LinkFromEntityName = "rnt_hgstransitlist";
                ftpLinkEntity.LinkToAttributeName = "rnt_ftpconsensusid";
                ftpLinkEntity.LinkToEntityName = "rnt_ftpconsensus";
                ftpLinkEntity.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);

                QueryExpression getHgsTransitListQuery = new QueryExpression("rnt_hgstransitlist");
                getHgsTransitListQuery.ColumnSet = new ColumnSet(true);
                getHgsTransitListQuery.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
                getHgsTransitListQuery.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Null);
                getHgsTransitListQuery.Criteria.AddCondition("rnt_ftpconsensusid", ConditionOperator.NotNull);
                getHgsTransitListQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                getHgsTransitListQuery.LinkEntities.Add(ftpLinkEntity);
                getHgsTransitListQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                EntityCollection results = this.OrgService.RetrieveMultiple(getHgsTransitListQuery);
                hgsTransitList.Entities.AddRange(results.Entities);

                if (results.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = results.PagingCookie;
                }
                else
                {
                    break;
                }
            }
            foreach (var hgsTransit in hgsTransitList.Entities)
            {
                Money hgsAmount = hgsTransit.GetAttributeValue<Money>("rnt_amount");
                if (hgsAmount != null && hgsAmount.Value != 0)
                {
                    hgsTotalAmount += hgsAmount.Value;
                }

            }
            return hgsTotalAmount;
        }


        public void createHGSList(HGSTransitData data, Guid ftpConsensusId, int indexOf, Guid contractId, Guid equipmentId, Guid hgsLabelid)
        {
            int statusCode = (int)HGSTransitListStatusCode.NotInvoiced;
            Entity hgsTransit = new Entity("rnt_hgstransitlist");
            hgsTransit.Attributes["rnt_hgsnumber"] = data.hgsNumber;
            hgsTransit.Attributes["rnt_platenumber"] = data.plateNumber;
            if (data._entryDateTime != DateTime.MinValue)
            {
                hgsTransit.Attributes["rnt_entrydatetime"] = data._entryDateTime;
            }
            hgsTransit.Attributes["rnt_entrylocation"] = data.entryLocation;
            hgsTransit.Attributes["rnt_exitdatetime"] = data._exitDateTime;
            hgsTransit.Attributes["rnt_exitlocation"] = data.exitLocation;
            hgsTransit.Attributes["rnt_amount"] = new Money(data.amount);
            hgsTransit.Attributes["rnt_ftprownumber"] = indexOf;
            hgsTransit.Attributes["rnt_integrationchannel"] = new OptionSetValue(2);
            hgsTransit.Attributes["rnt_ftpconsensusid"] = new EntityReference("rnt_ftpconsensus", ftpConsensusId);
            if (contractId != Guid.Empty)
            {
                hgsTransit.Attributes["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
            }
            else
            {
                statusCode = (int)HGSTransitListStatusCode.ContractNotFound;
            }
            if (equipmentId != Guid.Empty)
            {
                hgsTransit.Attributes["rnt_equipmentid"] = new EntityReference("rnt_equipment", equipmentId);
            }
            else
            {
                statusCode = (int)HGSTransitListStatusCode.PlateNotFound;
            }
            if (hgsLabelid != Guid.Empty)
            {
                hgsTransit.Attributes["rnt_hgslabelid"] = new EntityReference("rnt_hgslabel", hgsLabelid);
            }
            else
            {
                statusCode = (int)HGSTransitListStatusCode.HGSNotFound;
            }
            hgsTransit.Attributes["statuscode"] = new OptionSetValue(statusCode);
            this.OrgService.Create(hgsTransit);
        }

        public void updateHGSList(Guid hgsTransitId, Guid ftpConsensusId, int indexOf)
        {
            Entity hgsTransit = new Entity("rnt_hgstransitlist");
            hgsTransit.Id = hgsTransitId;
            hgsTransit.Attributes["rnt_ftprownumber"] = indexOf;
            hgsTransit.Attributes["rnt_ftpconsensusid"] = new EntityReference("rnt_ftpconsensus", ftpConsensusId);
            this.OrgService.Update(hgsTransit);
        }

        public void updateHGSListWithData(HGSTransitData data, Guid hgsTransitId, Guid ftpConsensusId, int indexOf, Guid contractId, Guid equipmentId, Guid hgsLabelid, int statusCrmCode)
        {
            int statusCode = (int)HGSTransitListStatusCode.NotInvoiced;
            Entity hgsTransit = new Entity("rnt_hgstransitlist");
            hgsTransit.Id = hgsTransitId;
            hgsTransit.Attributes["rnt_hgsnumber"] = data.hgsNumber;
            hgsTransit.Attributes["rnt_platenumber"] = data.plateNumber;
            if (data._entryDateTime != DateTime.MinValue)
            {
                hgsTransit.Attributes["rnt_entrydatetime"] = data._entryDateTime;
            }
            hgsTransit.Attributes["rnt_entrylocation"] = data.entryLocation;
            hgsTransit.Attributes["rnt_exitdatetime"] = data._exitDateTime;
            hgsTransit.Attributes["rnt_exitlocation"] = data.exitLocation;
            hgsTransit.Attributes["rnt_amount"] = new Money(data.amount);
            hgsTransit.Attributes["rnt_ftprownumber"] = indexOf;
            //hgsTransit.Attributes["rnt_integrationchannel"] = new OptionSetValue(2);
            hgsTransit.Attributes["rnt_ftpconsensusid"] = new EntityReference("rnt_ftpconsensus", ftpConsensusId);
            if (contractId != Guid.Empty)
            {
                hgsTransit.Attributes["rnt_contractid"] = new EntityReference("rnt_contract", contractId);
            }
            else
            {
                statusCode = (int)HGSTransitListStatusCode.ContractNotFound;
            }
            if (equipmentId != Guid.Empty)
            {
                hgsTransit.Attributes["rnt_equipmentid"] = new EntityReference("rnt_equipment", equipmentId);
            }
            else
            {
                statusCode = (int)HGSTransitListStatusCode.PlateNotFound;
            }
            if (hgsLabelid != Guid.Empty)
            {
                hgsTransit.Attributes["rnt_hgslabelid"] = new EntityReference("rnt_hgslabel", hgsLabelid);
            }
            else
            {
                statusCode = (int)HGSTransitListStatusCode.HGSNotFound;
            }
            if (statusCrmCode != (int)HGSTransitListStatusCode.Invoiced)
            {
                hgsTransit.Attributes["statuscode"] = new OptionSetValue(statusCode);
            }
            this.OrgService.Update(hgsTransit);
        }

        public void updateHGSListWithData(Guid hgsTransitId, Guid ftpConsensusId, int indexOf)
        {
            Entity hgsTransit = new Entity("rnt_hgstransitlist");
            hgsTransit.Id = hgsTransitId;
            hgsTransit.Attributes["rnt_ftprownumber"] = indexOf;
            hgsTransit.Attributes["rnt_ftpconsensusid"] = new EntityReference("rnt_ftpconsensus", ftpConsensusId);
            this.OrgService.Update(hgsTransit);
        }

        public void updateHGSListWithData(Guid hgsTransitId, Guid contractItemId)
        {
            Entity hgsTransit = new Entity("rnt_hgstransitlist");
            hgsTransit.Id = hgsTransitId;
            if (contractItemId != Guid.Empty)
            {
                hgsTransit.Attributes["rnt_contractid"] = new EntityReference("rnt_contractitemid", contractItemId);
            }
            hgsTransit.Attributes["statuscode"] = new OptionSetValue((int)HGSTransitListStatusCode.NotInvoiced);
            this.OrgService.Update(hgsTransit);
        }


        public EntityCollection getHGSTransitListByEquipmentId(DateTime exitDate, Guid equipmentId, decimal amount, string ftpConsensusId = null)
        {
            QueryExpression hgsTransitListQuery = new QueryExpression("rnt_hgstransitlist");
            hgsTransitListQuery.ColumnSet = new ColumnSet(true);
            hgsTransitListQuery.Criteria.AddCondition("rnt_equipmentid", ConditionOperator.Equal, equipmentId);
            hgsTransitListQuery.Criteria.AddCondition("rnt_amount", ConditionOperator.Equal, amount);
            hgsTransitListQuery.Criteria.AddCondition("rnt_exitdatetime", ConditionOperator.Equal, exitDate);

            if (!string.IsNullOrWhiteSpace(ftpConsensusId))
            {
                FilterExpression ftpConsensusFilter = new FilterExpression(LogicalOperator.Or);
                ftpConsensusFilter.AddCondition("rnt_ftpconsensusid", ConditionOperator.Equal, new Guid(ftpConsensusId));
                ftpConsensusFilter.AddCondition("rnt_ftpconsensusid", ConditionOperator.Null);
                hgsTransitListQuery.Criteria.AddFilter(ftpConsensusFilter);
            }

            EntityCollection hgsTransitList = this.OrgService.RetrieveMultiple(hgsTransitListQuery);
            return hgsTransitList;
        }

        public EntityCollection getHGSTransitListByPlateNumber(DateTime exitDate, string plateNumber, decimal amount, string ftpConsensusId = null)
        {
            QueryExpression hgsTransitListQuery = new QueryExpression("rnt_hgstransitlist");
            hgsTransitListQuery.ColumnSet = new ColumnSet(true);
            hgsTransitListQuery.Criteria.AddCondition("rnt_platenumber", ConditionOperator.Equal, plateNumber);
            hgsTransitListQuery.Criteria.AddCondition("rnt_amount", ConditionOperator.Equal, amount);
            hgsTransitListQuery.Criteria.AddCondition("rnt_exitdatetime", ConditionOperator.Equal, exitDate);

            if (!string.IsNullOrWhiteSpace(ftpConsensusId))
            {
                FilterExpression ftpConsensusFilter = new FilterExpression(LogicalOperator.Or);
                ftpConsensusFilter.AddCondition("rnt_ftpconsensusid", ConditionOperator.Equal, new Guid(ftpConsensusId));
                ftpConsensusFilter.AddCondition("rnt_ftpconsensusid", ConditionOperator.Null);
                hgsTransitListQuery.Criteria.AddFilter(ftpConsensusFilter);
            }

            EntityCollection hgsTransitList = this.OrgService.RetrieveMultiple(hgsTransitListQuery);
            return hgsTransitList;
        }
    }
}
