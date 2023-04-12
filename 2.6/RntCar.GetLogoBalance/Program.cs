using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Repository;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.GetLogoBalance
{
    class Program
    {
        static void Main(string[] args)
        {
            LoggerHelper logger = new LoggerHelper();
            logger.traceInfo("Logo credit limit process started");

            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            IOrganizationService orgService = crmServiceHelper.IOrganizationService;
            CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(orgService);

            logger.traceInfo("Class end");
            var corpList = corporateCustomerRepository.getCorporateCustomers(new string[] { "rnt_taxnumber", "rnt_logobalance" });
            logger.traceInfo("Get Corporate Customer List");
            foreach (var corp in corpList)
            {
                var logoCRMBalanceValue = corp.GetAttributeValue<Money>("rnt_logobalance") != null ? corp.GetAttributeValue<Money>("rnt_logobalance").Value : 0;
                var taxNumber = corp.GetAttributeValue<string>("rnt_taxnumber");

                LogoHelper logoHelper = new LogoHelper(orgService);
                ReservationRepository reservationRepository = new ReservationRepository(orgService);

                var logoBalance = logoHelper.getAccountBalance(taxNumber);
                logger.traceInfo($"Corpatare Customer Info {taxNumber} Logo Balance: {logoBalance}");

                string[] columns = new string[] { "rnt_generaltotalamount" };
                decimal generalTotalAmountValue = 0;
                List<Entity> reservationList = reservationRepository.getNewReservationsByCorporateId(columns, corp.Id);
                foreach (var reservation in reservationList)
                {
                    Money generalTotalAmount = reservation.GetAttributeValue<Money>("rnt_generaltotalamount");
                    generalTotalAmountValue += generalTotalAmount.Value;
                }
                decimal currentBalance = Convert.ToDecimal(logoBalance) + generalTotalAmountValue;

                if (logoCRMBalanceValue != currentBalance)
                {
                    try
                    {
                        Entity updateCorp = new Entity(corp.LogicalName, corp.Id);
                        updateCorp["rnt_logobalance"] = new Money(currentBalance);
                        orgService.Update(updateCorp);

                        logger.traceInfo($"Update Rentuna Corpatare Customer Info {taxNumber} Logo Balance: {logoBalance} Current Balance: {currentBalance} ");
                    }
                    catch (Exception ex)
                    {
                        logger.traceInfo($"Exception Corpatare Customer Info {taxNumber} Logo Balance: {logoBalance} Current Balance: {currentBalance}");
                        logger.traceInfo($"ex.Message {ex.Message}");
                    }
                }
            }

        }
    }
}
