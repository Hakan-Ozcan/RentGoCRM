using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ReservationRefund
{
    class Program
    {
        static void Main(string[] args)
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            LoyaltyJobBL loyaltyJobBL = new LoyaltyJobBL(crmServiceHelper.IOrganizationService);

            /*
              * GOLIVE: Tüm müşterilere Customer Overview create etmek için aşağıdaki kod yazıldı
              *              
             */
            loyaltyJobBL.CreateCustomerOverviewForActiveCustomers();

            /*
             * GOLIVE: Aktif müşteriler için loyaltyjob oluşturmak için aşağıdaki kod yazıldı                        
            */

            loyaltyJobBL.CreateLoyaltyJobForActiveCustomers();

            //Get Segment Configurations           
            loyaltyJobBL.RetrieveCustomerSegmentConfiguration();
            
            //Get Loyalty Jobs. Durum = Active ve ExecutionDate küçüktür now
            EntityCollection loyaltyJobs = loyaltyJobBL.RetrieveActiveLoyaltyJobs();

            foreach (var jobItem in loyaltyJobs.Entities)
            {
                loyaltyJobBL.CreateCustomerOverview(jobItem);
                loyaltyJobBL.UpdateCustomerOverview(jobItem);                
            }
        }
    }
}
