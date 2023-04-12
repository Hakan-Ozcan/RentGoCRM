using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Models;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Repository
{
    public class LoyaltyJobRepository : RepositoryHandler
    {
        public LoyaltyJobRepository(IOrganizationService Service) : base(Service)
        {
        }

        public LoyaltyJobRepository(IOrganizationService Service, Guid UserId) : base(Service, UserId)
        {
        }

        public LoyaltyJobRepository(IOrganizationService Service, Guid UserId, string OrganizationName) : base(Service, UserId, OrganizationName)
        {
        }

        public LoyaltyJobRepository(IOrganizationService Service, CrmServiceClient _crmServiceClient) : base(Service, _crmServiceClient)
        {
        }

        public Entity getReservationById(Guid Id)
        {
            try
            {
                return this.Service.Retrieve("rnt_reservation", Id, new ColumnSet(true));
            }
            catch
            {
                return null;
            }
        }

        internal EntityCollection RetrieveInvoicesLast12Months()
        {
            QueryExpression invoiceQueryJob = new QueryExpression("rnt_contract");
            invoiceQueryJob.ColumnSet = new ColumnSet(new string[] { "rnt_customerid" });
            invoiceQueryJob.Distinct = true;

            invoiceQueryJob.LinkEntities.Add(new LinkEntity
            {
                LinkToEntityName = "rnt_invoice",
                LinkFromAttributeName = "rnt_contractid",
                LinkToAttributeName = "rnt_contractid",
                Columns = new ColumnSet(false),
                EntityAlias = "intersect",
                LinkCriteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                                    {
                                        new ConditionExpression
                                        {
                                            AttributeName = "createdon",
                                            Operator = ConditionOperator.GreaterThan,
                                            Values = { new DateTime(2019,1,1) }
                                        }
                                    }
                }
            });

            EntityCollection invoiceCollection = this.Service.RetrieveMultiple(invoiceQueryJob);

            return invoiceCollection;
        }

        internal EntityCollection RetrieveInvoicesByCustomerId(Guid customerId)
        {
            QueryExpression invoiceQuery = new QueryExpression("rnt_invoice");
            invoiceQuery.ColumnSet = new ColumnSet(true);

            invoiceQuery.LinkEntities.Add(new LinkEntity
            {
                LinkToEntityName = "rnt_contract",
                LinkFromAttributeName = "rnt_contractid",
                LinkToAttributeName = "rnt_contractid",
                Columns = new ColumnSet(new string[] { "rnt_customerid" }),
                EntityAlias = "intersect",
                LinkCriteria = new FilterExpression
                {
                    FilterOperator = LogicalOperator.And,
                    Conditions =
                                    {
                                        new ConditionExpression
                                        {
                                            AttributeName = "rnt_customerid",
                                            Operator = ConditionOperator.Equal,
                                            Values = { customerId }
                                        }
                                    }
                }

            });

            invoiceQuery.Criteria.AddCondition(new ConditionExpression("createdon", ConditionOperator.LastXDays, 365));
            invoiceQuery.Orders.Add(new OrderExpression("createdon", OrderType.Ascending));

            return this.Service.RetrieveMultiple(invoiceQuery);
        }

        internal void UpdateCustomerOverviewEntity(CustomerOverview customerOverviewObj)
        {
            Entity customerOverview = new Entity("rnt_customeroverview");
            customerOverview.Id = customerOverviewObj.CustomerOverviewId;

            customerOverview["rnt_3monthsmonetary"] = new Money(customerOverviewObj.totalRevenue3);
            customerOverview["rnt_6monthsmonetary"] = new Money(customerOverviewObj.totalRevenue6);
            customerOverview["rnt_9monthsmonetary"] = new Money(customerOverviewObj.totalRevenue9);
            customerOverview["rnt_12monthsmonetary"] = new Money(customerOverviewObj.totalRevenue12);

            customerOverview["rnt_3monthsfrequency"] = customerOverviewObj.frequency3;
            customerOverview["rnt_6monthsfrequency"] = customerOverviewObj.frequency6;
            customerOverview["rnt_9monthsfrequency"] = customerOverviewObj.frequency9;
            customerOverview["rnt_12monthsfrequency"] = customerOverviewObj.frequency12;

            customerOverview["rnt_firstrentdate"] = customerOverviewObj.CustomerFirstInvoice.GetAttributeValue<DateTime>("createdon");
            customerOverview["rnt_lastrentdate"] = customerOverviewObj.CustomerLastInvoice.GetAttributeValue<DateTime>("createdon");

            customerOverview["rnt_customersegmentcode"] = new OptionSetValue(customerOverviewObj.CustomerCurrentSegment);

            this.Service.Update(customerOverview);
        }

        internal void UpdateContactCustomerOverviewLookup(Guid customerOverviewId, Guid customerId)
        {
            Entity cust = new Entity("contact");
            cust.Id = customerId;
            cust["rnt_customeroverviewid"] = new EntityReference("rnt_customeroverview", customerOverviewId);
            this.Service.Update(cust);
        }

        internal void CreateNextExecutionLoyaltyJobs(Entity jobItem, CustomerOverview customerOverviewObj)
        {
            if (customerOverviewObj.frequency12 > 0)
            {
                Entity nextExecution = new Entity("rnt_customerloyaltyjob");
                nextExecution["rnt_executiondate"] = customerOverviewObj.nextExecution12;
                nextExecution["rnt_customerid"] = new EntityReference("contact", customerOverviewObj.CustomerId);
                nextExecution["rnt_name"] = jobItem.GetAttributeValue<string>("rnt_name");
                nextExecution["statuscode"] = new OptionSetValue(1);
                nextExecution["rnt_jobtypecode"] = new OptionSetValue(2);
                this.Service.Create(nextExecution);
            }

            if (customerOverviewObj.frequency9 > 0)
            {
                Entity nextExecution = new Entity("rnt_customerloyaltyjob");
                nextExecution["rnt_executiondate"] = customerOverviewObj.nextExecution9;
                nextExecution["rnt_customerid"] = new EntityReference("contact", customerOverviewObj.CustomerId);
                nextExecution["rnt_name"] = jobItem.GetAttributeValue<string>("rnt_name");
                nextExecution["statuscode"] = new OptionSetValue(1);
                nextExecution["rnt_jobtypecode"] = new OptionSetValue(2);
                this.Service.Create(nextExecution);
            }

            if (customerOverviewObj.frequency6 > 0)
            {
                Entity nextExecution = new Entity("rnt_customerloyaltyjob");
                nextExecution["rnt_executiondate"] = customerOverviewObj.nextExecution6;
                nextExecution["rnt_customerid"] = new EntityReference("contact", customerOverviewObj.CustomerId);
                nextExecution["rnt_name"] = jobItem.GetAttributeValue<string>("rnt_name");
                nextExecution["statuscode"] = new OptionSetValue(1);
                nextExecution["rnt_jobtypecode"] = new OptionSetValue(2);
                this.Service.Create(nextExecution);
            }

            if (customerOverviewObj.frequency3 > 0)
            {
                Entity nextExecution = new Entity("rnt_customerloyaltyjob");
                nextExecution["rnt_executiondate"] = customerOverviewObj.nextExecution3;
                nextExecution["rnt_customerid"] = new EntityReference("contact", customerOverviewObj.CustomerId);
                nextExecution["rnt_name"] = jobItem.GetAttributeValue<string>("rnt_name");
                nextExecution["statuscode"] = new OptionSetValue(1);
                nextExecution["rnt_jobtypecode"] = new OptionSetValue(2);
                this.Service.Create(nextExecution);
            }
        }

        internal void CompleteLoyaltyJob(Entity jobItem)
        {
            Entity job = new Entity("rnt_customerloyaltyjob");
            job.Id = jobItem.Id;
            job["statuscode"] = new OptionSetValue(100000000);
            this.Service.Update(job);
        }

        internal Guid CreateCustomerOverview(Guid customerId)
        {
            Entity createCustomerOverview = new Entity("rnt_customeroverview");
            createCustomerOverview["rnt_customerid"] = new EntityReference("contact", customerId);
            createCustomerOverview["rnt_customersegmentcode"] = new OptionSetValue(1);
            return this.Service.Create(createCustomerOverview);
        }

        internal Entity RetrieveCustomerById(Guid customerId)
        {
            return this.Service.Retrieve("contact", customerId, new ColumnSet(true));
        }

        internal EntityCollection RetrieveActiveLoyaltyJobs()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_customerloyaltyjob");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, 1);
            queryExpression.Criteria.AddCondition("rnt_executiondate", ConditionOperator.LessEqual, DateTime.Now);

            return this.Service.RetrieveMultiple(queryExpression);
        }

        internal EntityCollection RetrieveCustomerSegmentConfiguration()
        {
            QueryExpression querySegmentConfExpression = new QueryExpression("rnt_customersegmentconfiguration");
            querySegmentConfExpression.ColumnSet = new ColumnSet(true);
            var confCollection = this.Service.RetrieveMultiple(querySegmentConfExpression);
            return confCollection;            
        }

        internal void CreateLoyaltyJobFromInvoiceCollection(EntityCollection invoiceCollection)
        {
            foreach (var item in invoiceCollection.Entities)
            {
                Guid custId = item.GetAttributeValue<EntityReference>("rnt_customerid").Id;
                Entity job = new Entity("rnt_customerloyaltyjob");
                job["rnt_jobtypecode"] = new OptionSetValue(1);
                job["statuscode"] = new OptionSetValue(1);
                job["rnt_customerid"] = new EntityReference("contact", item.GetAttributeValue<EntityReference>("rnt_customerid").Id);
                job["rnt_executiondate"] = DateTime.Now;
                job["rnt_name"] = item.GetAttributeValue<EntityReference>("rnt_customerid").Name + " Sadakat Hesaplama";
                this.Service.Create(job);

            }
        }

        internal void CreateCustomerOverviewForActiveCustomers()
        {
            QueryExpression contactQuery = new QueryExpression("contact");
            contactQuery.ColumnSet = new ColumnSet(true);
            contactQuery.Criteria.AddCondition("rnt_customeroverviewid", ConditionOperator.Null);
            var contactCol = this.Service.RetrieveMultiple(contactQuery);

            foreach (var item in contactCol.Entities)
            {
                Entity custOverview = new Entity("rnt_customeroverview");
                custOverview["rnt_customerid"] = new EntityReference("contact", item.Id);
                custOverview["rnt_customersegmentcode"] = new OptionSetValue(1);
                var cId = this.Service.Create(custOverview);

                Entity contact = new Entity("contact");
                contact.Id = item.Id;
                contact["rnt_customeroverviewid"] = new EntityReference("rnt_customeroverview", cId);
                this.Service.Update(contact);
            }
        }       

    }
}
