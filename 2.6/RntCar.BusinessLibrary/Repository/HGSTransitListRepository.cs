using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Collections.Generic;
using System.Linq;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.BusinessLibrary.Repository
{
    public class HGSTransitListRepository : RepositoryHandler
    {

        public HGSTransitListRepository(IOrganizationService Service) : base(Service)
        {
        }

        public HGSTransitListRepository(CrmServiceClient _crmServiceClient) : base(_crmServiceClient)
        {
        }

        public HGSTransitListRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public HGSTransitListRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public HGSTransitListRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public EntityCollection getHGSTransitListByContractId(Guid contractId)
        {
            QueryExpression hgsTransitListQuery = new QueryExpression("rnt_hgstransitlist");
            hgsTransitListQuery.ColumnSet = new ColumnSet(true);
            hgsTransitListQuery.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, contractId);
            hgsTransitListQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            hgsTransitListQuery.ColumnSet = new ColumnSet(true);
            return this.retrieveMultiple(hgsTransitListQuery);
        }
        public List<Entity> getHGSTransitListByContractItemId(Guid contractItemId)
        {
            QueryExpression queryExpression = new QueryExpression("rnt_hgstransitlist");
            queryExpression.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.Equal, contractItemId);
            queryExpression.ColumnSet = new ColumnSet(true);
            return this.retrieveMultiple(queryExpression).Entities.ToList();
        }

        public EntityCollection getHGSTransitListNotPaymentWithFtpConsensus()
        {
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
                getHgsTransitListQuery.Criteria.AddCondition("rnt_contractid", ConditionOperator.NotNull);
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
                EntityCollection results = this.retrieveMultiple(getHgsTransitListQuery);
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
            return hgsTransitList;
        }

        public EntityCollection getHGSTransitListNotInvoicedWithFtpConsensus()
        {
            EntityCollection hgsTransitList = new EntityCollection();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;

            string pagingCookie = null;
            while (true)
            {
                LinkEntity contractLink = new LinkEntity();
                contractLink.EntityAlias = "contractAlias";
                contractLink.LinkFromAttributeName = "rnt_contractid";
                contractLink.LinkFromEntityName = "rnt_hgstransitlist";
                contractLink.LinkToAttributeName = "rnt_contractid";
                contractLink.LinkToEntityName = "rnt_contract";
                contractLink.LinkCriteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Completed);

                LinkEntity ftpLinkEntity = new LinkEntity();
                ftpLinkEntity.EntityAlias = "ftpconsesusAlias";
                ftpLinkEntity.LinkFromAttributeName = "rnt_ftpconsensusid";
                ftpLinkEntity.LinkFromEntityName = "rnt_hgstransitlist";
                ftpLinkEntity.LinkToAttributeName = "rnt_ftpconsensusid";
                ftpLinkEntity.LinkToEntityName = "rnt_ftpconsensus";
                ftpLinkEntity.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);

                QueryExpression getHgsTransitListQuery = new QueryExpression("rnt_hgstransitlist");
                getHgsTransitListQuery.ColumnSet = new ColumnSet(true);
                getHgsTransitListQuery.Criteria.AddCondition("rnt_contractid", ConditionOperator.NotNull);
                getHgsTransitListQuery.Criteria.AddCondition("rnt_contractitemid", ConditionOperator.NotNull);
                getHgsTransitListQuery.Criteria.AddCondition("rnt_invoiceitemid", ConditionOperator.Null);
                getHgsTransitListQuery.Criteria.AddCondition("rnt_ftpconsensusid", ConditionOperator.NotNull);
                getHgsTransitListQuery.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                getHgsTransitListQuery.LinkEntities.Add(ftpLinkEntity);
                getHgsTransitListQuery.LinkEntities.Add(contractLink);
                getHgsTransitListQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                EntityCollection results = this.retrieveMultiple(getHgsTransitListQuery);
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
            return hgsTransitList;
        }


        public EntityCollection getHGSTransitListNotInvoicedWithContractId(Guid contractId)
        {
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
                EntityCollection results = this.retrieveMultiple(getHgsTransitListQuery);
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
            return hgsTransitList;
        }
    }
}
