using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Models;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.PaymentPlan;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class LoyaltyJobBL : BusinessHandler
    {
        public EntityCollection CustomerInvoiceCollection;
        public CustomerOverview customerOverview;
        public CustomerSegmentConfiguration customerSegmentConfiguration;
        public LoyaltyJobBL(IOrganizationService orgService) : base(orgService)
        {
            CustomerInvoiceCollection = new EntityCollection();            
        }

        public LoyaltyJobBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public void CreateCustomerOverviewForActiveCustomers()
        {
            LoyaltyJobRepository loyaltyJobRepository = new LoyaltyJobRepository(this.OrgService);
            loyaltyJobRepository.CreateCustomerOverviewForActiveCustomers();           
        }

        public void CreateLoyaltyJobForActiveCustomers()
        {
            LoyaltyJobRepository loyaltyJobRepository = new LoyaltyJobRepository(this.OrgService);

            EntityCollection invoiceCollection = loyaltyJobRepository.RetrieveInvoicesLast12Months();
            loyaltyJobRepository.CreateLoyaltyJobFromInvoiceCollection(invoiceCollection);
        }

        public void RetrieveCustomerSegmentConfiguration()
        {
            LoyaltyJobRepository loyaltyJobRepository = new LoyaltyJobRepository(this.OrgService);

            EntityCollection confCollection = loyaltyJobRepository.RetrieveCustomerSegmentConfiguration();

            Entity premiumSegmentEntity = confCollection.Entities.Where(x => x.GetAttributeValue<Money>("rnt_segmentcode").Value == 2).FirstOrDefault();
            Entity platiniumSegmentEntity = confCollection.Entities.Where(x => x.GetAttributeValue<Money>("rnt_segmentcode").Value == 3).FirstOrDefault();

            customerSegmentConfiguration = new CustomerSegmentConfiguration();

            customerSegmentConfiguration.minPremiumRevenue = premiumSegmentEntity.GetAttributeValue<Money>("rnt_minrevenue").Value;
            customerSegmentConfiguration.maxPremiumRevenue = premiumSegmentEntity.GetAttributeValue<Money>("rnt_maxrevenue").Value;

            customerSegmentConfiguration.minPremiumFrequency = premiumSegmentEntity.GetAttributeValue<int>("rnt_minrentalfrequency");
            customerSegmentConfiguration.maxPremiumFrequency = premiumSegmentEntity.GetAttributeValue<int>("rnt_maxrentalfrequency");

            customerSegmentConfiguration.minPlatiniumRevenue = platiniumSegmentEntity.GetAttributeValue<Money>("rnt_minrevenue").Value;
            customerSegmentConfiguration.maxPlatiniumRevenue = platiniumSegmentEntity.GetAttributeValue<Money>("rnt_maxrevenue").Value;

            customerSegmentConfiguration.minPlatiniumFrequency = platiniumSegmentEntity.GetAttributeValue<int>("rnt_minrentalfrequency");
            customerSegmentConfiguration.maxPlatiniumFrequency = platiniumSegmentEntity.GetAttributeValue<int>("rnt_maxrentalfrequency");
        }

        public EntityCollection RetrieveActiveLoyaltyJobs()
        {
            LoyaltyJobRepository loyaltyJobRepository = new LoyaltyJobRepository(this.OrgService);
            return loyaltyJobRepository.RetrieveActiveLoyaltyJobs();
            
        }

        public void CreateCustomerOverview(Entity jobItem)
        {
            customerOverview = new CustomerOverview();
            customerOverview.CustomerId = jobItem.GetAttributeValue<EntityReference>("rnt_customerid").Id;

            LoyaltyJobRepository loyaltyJobRepository = new LoyaltyJobRepository(this.OrgService);

            // Eğer müşterinin customer overview kaydı yok ise önce oluştur
            // Sonra Contact üzerindeki Customer Overview Lookup alanı update et
            Entity contact = loyaltyJobRepository.RetrieveCustomerById(jobItem.GetAttributeValue<EntityReference>("rnt_customerid").Id);

            customerOverview.CustomerCurrentSegment = contact.Contains("rnt_customersegmentationcode") ? contact.GetAttributeValue<OptionSetValue>("rnt_customersegmentationcode").Value : 1;

            if (!contact.Contains("rnt_customeroverviewid"))
            {
                customerOverview.CustomerOverviewId = loyaltyJobRepository.CreateCustomerOverview(contact.Id);
                loyaltyJobRepository.UpdateContactCustomerOverviewLookup(customerOverview.CustomerOverviewId, customerOverview.CustomerId);
            }
            else
            {
                customerOverview.CustomerOverviewId = contact.GetAttributeValue<EntityReference>("rnt_customeroverviewid").Id;
            }
        }

        public void UpdateCustomerOverview(Entity jobItem)
        {
            CustomerInvoiceCollection = (EntityCollection)RetrieveInvoicesByCustomerId(customerOverview.CustomerId);

            if (CustomerInvoiceCollection.Entities.Count > 0)
            {
                
                customerOverview.CustomerFirstInvoice = GetFirstInvoice();
                customerOverview.CustomerLastInvoice = GetLastInvoice();

                //3,6,9,12 ay toplamları al
                foreach (var invoiceItem in CustomerInvoiceCollection.Entities)
                {
                    CalculateCustomerOverviewFields(invoiceItem);
                }

                //Customer Overview entitysini update et
                SetCustomerSegment();
                UpdateCustomerOverviewEntity();

                CompleteLoyaltyJob(jobItem);

                CreateNextExecutionLoyaltyJobs(jobItem);

            }
        }

        private void CreateNextExecutionLoyaltyJobs(Entity jobItem)
        {
            LoyaltyJobRepository loyaltyJobRepository = new LoyaltyJobRepository(this.OrgService);
            loyaltyJobRepository.CreateNextExecutionLoyaltyJobs(jobItem, customerOverview);
        }

        private void CompleteLoyaltyJob(Entity jobItem)
        {
            LoyaltyJobRepository loyaltyJobRepository = new LoyaltyJobRepository(this.OrgService);
            loyaltyJobRepository.CompleteLoyaltyJob(jobItem);           
        }

        private void UpdateCustomerOverviewEntity()
        {            
            LoyaltyJobRepository loyaltyJobRepository = new LoyaltyJobRepository(this.OrgService);
            loyaltyJobRepository.UpdateCustomerOverviewEntity(customerOverview);            
        }

        private void SetCustomerSegment()
        {
            //Customer Segmenti güncelle (Mevcut segmenti Elit veya Premium ise sadece)

            if ((customerOverview.CustomerCurrentSegment == 1) || (customerOverview.CustomerCurrentSegment == 2))
            {
                if (((customerSegmentConfiguration.minPremiumRevenue <= customerOverview.totalRevenue12) && (customerOverview.totalRevenue12 <= customerSegmentConfiguration.maxPremiumRevenue)) 
                    || ((customerSegmentConfiguration.minPremiumFrequency <= customerOverview.frequency12) && (customerOverview.frequency12 <= customerSegmentConfiguration.maxPremiumFrequency)))
                {
                    customerOverview.CustomerCurrentSegment = 2;
                }

                if (((customerSegmentConfiguration.minPlatiniumRevenue <= customerOverview.totalRevenue12) && (customerOverview.totalRevenue12 <= customerSegmentConfiguration.maxPlatiniumRevenue)) 
                    || ((customerSegmentConfiguration.minPlatiniumFrequency <= customerOverview.frequency12) && (customerOverview.frequency12 <= customerSegmentConfiguration.maxPlatiniumFrequency)))
                {
                    customerOverview.CustomerCurrentSegment = 3;
                }
            }
        }
        private void CalculateCustomerOverviewFields(Entity invoiceItem)
        {
            customerOverview.totalRevenue += invoiceItem.GetAttributeValue<Money>("rnt_totalamount").Value;
            customerOverview.frequency += 1;

            if ((DateTime.Now - invoiceItem.GetAttributeValue<DateTime>("createdon")).TotalDays <= 365)
            {
                customerOverview.totalRevenue12 += invoiceItem.GetAttributeValue<Money>("rnt_totalamount").Value;
                customerOverview.frequency12 += 1;

                if (customerOverview.frequency12 == 1)
                {
                    customerOverview.nextExecution12 = invoiceItem.GetAttributeValue<DateTime>("createdon").AddDays(365);
                }
            }

            if ((DateTime.Now - invoiceItem.GetAttributeValue<DateTime>("createdon")).TotalDays <= 270)
            {
                customerOverview.totalRevenue9 += invoiceItem.GetAttributeValue<Money>("rnt_totalamount").Value;
                customerOverview.frequency9 += 1;

                if (customerOverview.frequency9 == 1)
                {
                    customerOverview.nextExecution9 = invoiceItem.GetAttributeValue<DateTime>("createdon").AddDays(270);
                }
            }

            if ((DateTime.Now - invoiceItem.GetAttributeValue<DateTime>("createdon")).TotalDays <= 180)
            {
                customerOverview.totalRevenue6 += invoiceItem.GetAttributeValue<Money>("rnt_totalamount").Value;
                customerOverview.frequency6 += 1;

                if (customerOverview.frequency6 == 1)
                {
                    customerOverview.nextExecution6 = invoiceItem.GetAttributeValue<DateTime>("createdon").AddDays(180);
                }
            }

            if ((DateTime.Now - invoiceItem.GetAttributeValue<DateTime>("createdon")).TotalDays <= 90)
            {
                customerOverview.totalRevenue3 += invoiceItem.GetAttributeValue<Money>("rnt_totalamount").Value;
                customerOverview.frequency3 += 1;

                if (customerOverview.frequency3 == 1)
                {
                    customerOverview.nextExecution3 = invoiceItem.GetAttributeValue<DateTime>("createdon").AddDays(90);
                }
            }
        }

        private Entity GetLastInvoice()
        {            
            return CustomerInvoiceCollection.Entities.FirstOrDefault();
        }

        private Entity GetFirstInvoice()
        {
            return CustomerInvoiceCollection.Entities.LastOrDefault();
        }

        public EntityCollection RetrieveInvoicesByCustomerId(Guid customerId)
        {
            LoyaltyJobRepository loyaltyJobRepository = new LoyaltyJobRepository(this.OrgService);
            return (EntityCollection)loyaltyJobRepository.RetrieveInvoicesByCustomerId(customerId);
        }
    }
}
