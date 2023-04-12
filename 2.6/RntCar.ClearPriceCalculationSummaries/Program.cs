using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Repository;
using Microsoft.Xrm.Sdk.Query;
using RntCar.MongoDBHelper.Entities;

namespace RntCar.ClearPriceCalculationSummaries
{
    class Program
    {
        static string mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
        static string mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");

        static void Main(string[] args)
        {
            var _date = DateTime.Now;
            _date = new DateTime(_date.Year, _date.Month, _date.Day).AddDays(1).AddTicks(-1);

            var startDate = _date;
            var endDate = _date;
            var startDateForNotActive = _date;


            int beforeXdaysForDeactiveEndDate = Convert.ToInt32(StaticHelper.GetConfiguration("beforeXdaysForDeactiveEndDate"));
            int beforeXdaysForCompletedEndDate = Convert.ToInt32(StaticHelper.GetConfiguration("beforeXdaysForCompletedEndDate"));
            int beforeXhoursForPriceCalculationSummaries = Convert.ToInt32(StaticHelper.GetConfiguration("beforeXhoursForPriceCalculationSummaries"));
            int beforeXdaysForAllStartDate = Convert.ToInt32(StaticHelper.GetConfiguration("beforeXdaysForStartDateNotActive"));
            int beforeXdaysForStartDateActive = Convert.ToInt32(StaticHelper.GetConfiguration("beforeXdaysForStartDateActive"));

            beforeXdaysForDeactiveEndDate = -1 * beforeXdaysForDeactiveEndDate;
            beforeXdaysForCompletedEndDate = -1 * beforeXdaysForCompletedEndDate;
            beforeXhoursForPriceCalculationSummaries = -1 * beforeXhoursForPriceCalculationSummaries;

            beforeXdaysForStartDateActive = -1 * beforeXdaysForStartDateActive;
            startDate = startDate.AddDays(beforeXdaysForStartDateActive);

            beforeXdaysForAllStartDate = -1 * beforeXdaysForAllStartDate;
            startDateForNotActive = startDateForNotActive.AddDays(beforeXdaysForAllStartDate);


            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            ReservationBL reservationBL = new ReservationBL(crmServiceHelper.IOrganizationService);

            BusinessLibrary.ContractItemRepository contractItemRepository = new BusinessLibrary.ContractItemRepository(crmServiceHelper.IOrganizationService);
            BusinessLibrary.Repository.ReservationItemRepository reservationItemRepository = new BusinessLibrary.Repository.ReservationItemRepository(crmServiceHelper.IOrganizationService);
            PriceCalculationSummariesRepository priceCalculationSummariesRepository = new PriceCalculationSummariesRepository(mongoDBHostName, mongoDBDatabaseName);
            PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(mongoDBHostName, mongoDBDatabaseName);

            #region Active Record For True
            var allActiveTrackingNumbers = contractItemRepository.getActiveContractItemBetweenGivenDates(startDate, endDate, new string[] { "rnt_mongodbtrackingnumber" }).Entities.Select(x => x.GetAttributeValue<string>("rnt_mongodbtrackingnumber")).Distinct().ToList();
            var activeReservationTrackingNumbers = reservationItemRepository.getActiveReservationItemBetweenGivenDates(startDate, endDate, new string[] { "rnt_mongodbtrackingnumber" }).Entities.Select(x => x.GetAttributeValue<string>("rnt_mongodbtrackingnumber")).Distinct().ToList();

            allActiveTrackingNumbers.AddRange(activeReservationTrackingNumbers);

            foreach (var item in allActiveTrackingNumbers)
            {
                Console.WriteLine(allActiveTrackingNumbers.IndexOf(item));
                var crmPriceCalculationSummariesTrackingNumbers = priceCalculationSummariesRepository.getPriceCalculationSummariesWithIsCrmCreatedByTrackingNumber(item, false);
                if (crmPriceCalculationSummariesTrackingNumbers.Count > 0)
                {
                    crmPriceCalculationSummariesTrackingNumbers.ForEach(t => t.isCrmCreated = true);
                    priceCalculationSummariesBusiness.bulkUpdatePriceCalculationSummaries(crmPriceCalculationSummariesTrackingNumbers);
                }
            }
            #endregion

            #region Completed Record For False
            var allCompletedTrackingNumbers = contractItemRepository.getCompletedContractItemBetweenGivenDates(startDateForNotActive.AddDays(beforeXdaysForCompletedEndDate), _date.AddDays(beforeXdaysForCompletedEndDate), new string[] { "rnt_mongodbtrackingnumber" }).Entities.Select(x => x.GetAttributeValue<string>("rnt_mongodbtrackingnumber")).Distinct().ToList();
            var completedReservationTrackingNumbers = reservationItemRepository.getCompletedReservationItemBetweenGivenDates(startDateForNotActive.AddDays(beforeXdaysForCompletedEndDate), _date.AddDays(beforeXdaysForCompletedEndDate), new string[] { "rnt_mongodbtrackingnumber" }).Entities.Select(x => x.GetAttributeValue<string>("rnt_mongodbtrackingnumber")).Distinct().ToList();

            allCompletedTrackingNumbers.AddRange(completedReservationTrackingNumbers);

            foreach (var item in allCompletedTrackingNumbers)
            {
                Console.WriteLine(allCompletedTrackingNumbers.IndexOf(item));
                var crmPriceCalculationSummariesTrackingNumbers = priceCalculationSummariesRepository.getPriceCalculationSummariesWithIsCrmCreatedByTrackingNumber(item, true);
                if (crmPriceCalculationSummariesTrackingNumbers.Count > 0)
                {
                    crmPriceCalculationSummariesTrackingNumbers.ForEach(t => t.isCrmCreated = false);
                    priceCalculationSummariesBusiness.bulkUpdatePriceCalculationSummaries(crmPriceCalculationSummariesTrackingNumbers);
                }
            }
            #endregion

            #region Deactive Record For False
            var allDeactiveTrackingNumbers = contractItemRepository.getDeactiveContractItemBetweenGivenDates(startDateForNotActive.AddDays(beforeXdaysForDeactiveEndDate), _date.AddDays(beforeXdaysForDeactiveEndDate), new string[] { "rnt_mongodbtrackingnumber" }).Entities.Select(x => x.GetAttributeValue<string>("rnt_mongodbtrackingnumber")).Distinct().ToList();
            var deactiveReservationTrackingNumbers = reservationItemRepository.getDeactiveReservationItemBetweenGivenDates(startDateForNotActive.AddDays(beforeXdaysForDeactiveEndDate), _date.AddDays(beforeXdaysForDeactiveEndDate), new string[] { "rnt_mongodbtrackingnumber" }).Entities.Select(x => x.GetAttributeValue<string>("rnt_mongodbtrackingnumber")).Distinct().ToList();

            allDeactiveTrackingNumbers.AddRange(deactiveReservationTrackingNumbers);

            foreach (var item in allDeactiveTrackingNumbers)
            {
                Console.WriteLine(allDeactiveTrackingNumbers.IndexOf(item));
                var crmPriceCalculationSummariesTrackingNumbers = priceCalculationSummariesRepository.getPriceCalculationSummariesWithIsCrmCreatedByTrackingNumber(item, true);
                if (crmPriceCalculationSummariesTrackingNumbers.Count > 0)
                {
                    crmPriceCalculationSummariesTrackingNumbers.ForEach(t => t.isCrmCreated = false);
                    priceCalculationSummariesBusiness.bulkUpdatePriceCalculationSummaries(crmPriceCalculationSummariesTrackingNumbers);
                }
            }
            #endregion

            int packageCount = 0;
            while (true)
            {
                var priceCalculationSummariesTrackingNumbers = priceCalculationSummariesRepository.getPricesCalculationSummariesIsNotCreated(beforeXhoursForPriceCalculationSummaries);
                if (priceCalculationSummariesTrackingNumbers.Count == 0)
                {
                    break;
                }

                #region CRM Control Blok
                var existsTrackingNumbersOnCrm = priceCalculationSummariesTrackingNumbers.Where(t => allActiveTrackingNumbers.Select(c => c).Contains(t.trackingNumber)).ToList();
                if (existsTrackingNumbersOnCrm.Count > 0)
                {
                    existsTrackingNumbersOnCrm.ForEach(t => t.isCrmCreated = true);
                    priceCalculationSummariesBusiness.bulkUpdatePriceCalculationSummaries(existsTrackingNumbersOnCrm);
                }

                foreach (var item in existsTrackingNumbersOnCrm)
                {
                    priceCalculationSummariesTrackingNumbers.Remove(item);
                }
                #endregion

                priceCalculationSummariesBusiness.bulkDeletePriceCalculationSummaries(priceCalculationSummariesTrackingNumbers);
                Console.WriteLine(packageCount++);
            }
        }
    }
}
