using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Repository;
using RntCar.Logger;
using RntCar.MongoDBHelper.Entities;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RntCar.BonusCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            var mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
            var mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");
            var day = Convert.ToInt32(StaticHelper.GetConfiguration("LastXDays"));

            LoggerHelper logger = new LoggerHelper();
            logger.traceInfo("Bonus calculation started");

            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
            BonusCalculationBL bonusCalculationBL = new BonusCalculationBL(crmServiceHelper.IOrganizationService);
            BonusCalculationBusiness bonusCalculationBusiness = new BonusCalculationBusiness(mongoDBHostName, mongoDBDatabaseName, day);

            try
            {
                var contracts = contractRepository.getCompletedContractsByXlastDays(day, new string[] { "rnt_contractid", "rnt_contractnumber", "rnt_pnrnumber", "rnt_dropoffbranchid", "rnt_pickupbranchid", "rnt_pickupdatetime", "rnt_dropoffdatetime" });
                logger.traceInfo($"Number of completed contracts for last {day} days: " + contracts.Count);

                List<Task> tasks = new List<Task>();

                var userBasedBonusCalculationTask = new Task(() =>
                {
                    logger.traceInfo("User based bonus calculation started");
                    var userBasedBonusCalculation = bonusCalculationBL.calculateUserBasedNetBonusAmount(contracts);
                    logger.traceInfo("User based bonus calculation end");

                    if (userBasedBonusCalculation.IsSuccess)
                    {
                        logger.traceInfo("User bonus calculation have value");
                        bonusCalculationBusiness.deleteLastXDayUserRecords(day);
                        bonusCalculationBusiness.createUserBasedBonusCalculation(userBasedBonusCalculation.Result);
                    }
                    else
                    {
                        logger.traceError(userBasedBonusCalculation.Message);
                    }
                });
                tasks.Add(userBasedBonusCalculationTask);
                userBasedBonusCalculationTask.Start();

                var positionalBonusCalculationTask = new Task(() =>
                {
                    logger.traceInfo("Positional bonus calculation started");
                    var positionalBonusCalculation = bonusCalculationBL.calculatePositionalNetBonusAmount(contracts);
                    logger.traceInfo("Positional bonus calculation end");

                    if (positionalBonusCalculation.IsSuccess)
                    {
                        logger.traceInfo("Positional bonus calculation have value");
                        bonusCalculationBusiness.deleteLastXDayPositionalRecords(day);
                        bonusCalculationBusiness.createPositionalBonusCalculation(positionalBonusCalculation.Result);
                    }
                    else
                    {
                        logger.traceError(positionalBonusCalculation.Message);
                    }
                });
                tasks.Add(positionalBonusCalculationTask);
                positionalBonusCalculationTask.Start();

                Task.WaitAll(tasks.ToArray());

                logger.traceInfo("Bonus calculation end");
            }
            catch (Exception ex)
            {
                logger.traceError(ex.Message);
                throw;
            }
        }
    }
}
