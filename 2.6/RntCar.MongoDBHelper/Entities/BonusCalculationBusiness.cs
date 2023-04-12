using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.Logger;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;

namespace RntCar.MongoDBHelper.Entities
{
    public class BonusCalculationBusiness : MongoDBInstance
    {
        private IMongoCollection<PositionalBonusCalculationDataMongoDB> positionalCollection;
        private IMongoCollection<UserBasedBonusCalculationData> userBasedCollection;
        private readonly LoggerHelper logger;
        private bool isPositionalRecordsDeleted;
        private bool isUserRecordsDeleted;

        public BonusCalculationBusiness(string serverName, string dataBaseName, int day) : base(serverName, dataBaseName)
        {
            logger = new LoggerHelper();
            var positionalCollectionName = StaticHelper.GetConfiguration("MongoDBPositionalBonusCalculatorCollectionName");
            var userBasedCollectionName = StaticHelper.GetConfiguration("MongoDBUserBasedBonusCalculatorCollectionName");
            positionalCollection = this.getCollection<PositionalBonusCalculationDataMongoDB>(positionalCollectionName);
            userBasedCollection = this.getCollection<UserBasedBonusCalculationData>(userBasedCollectionName);

            //isRecordsDeleted = deleteLastXDayRecords(day);
        }

        //private bool deleteLastXDayRecords(int day)
        //{
        //    try
        //    {
        //        positionalCollection.DeleteMany(p => p.QueryDate >= DateTime.Today.AddDays(day * -1));
        //        userBasedCollection.DeleteMany(p => p.QueryDate >= DateTime.Today.AddDays(day * -1));
        //        logger.traceInfo($"Records deleted from {DateTime.Today.AddDays(day * -1).Date} to {DateTime.Today.Date}");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.traceError("Could not delete records: " + ex.Message);
        //        return false;
        //    }
        //}

        public void deleteLastXDayPositionalRecords(int day)
        {
            try
            {
                positionalCollection.DeleteMany(p => p.QueryDate >= DateTime.Today.AddDays(day * -1));
                logger.traceInfo($"Positional records deleted from {DateTime.Today.AddDays(day * -1).Date} to {DateTime.Today.Date}");
                isPositionalRecordsDeleted = true;
            }
            catch (Exception ex)
            {
                logger.traceError("Could not delete positional records: " + ex.Message);
            }
        }

        public void deleteLastXDayUserRecords(int day)
        {
            try
            {
                userBasedCollection.DeleteMany(p => p.QueryDate >= DateTime.Today.AddDays(day * -1));
                logger.traceInfo($"User records deleted from {DateTime.Today.AddDays(day * -1).Date} to {DateTime.Today.Date}");
                isUserRecordsDeleted = true;
            }
            catch (Exception ex)
            {
                logger.traceError("Could not delete user records: " + ex.Message);
            }
        }

        public void createPositionalBonusCalculation(List<PositionalBonusCalculationData> bonusCalculationDatas)
        {
            if (isPositionalRecordsDeleted)
            {
                logger.traceInfo("Inserting positional bonuses");
                var bonusCalculationItem = new PositionalBonusCalculationDataMongoDB();

                bonusCalculationDatas.ForEach(item =>
                {
                    logger.traceInfo("Positional bonus - contract no: " + item.ContractNumber);
                    logger.traceInfo("Positional bonus - contract item id: " + item.ContractItemId);

                    bonusCalculationItem = bonusCalculationItem.Map(item);
                    bonusCalculationItem._id = ObjectId.GenerateNewId();

                    var response = positionalCollection.Insert(bonusCalculationItem, Convert.ToString(bonusCalculationItem._id), ErrorLogsHelper.GetCurrentMethod());
                });
                logger.traceInfo("Positional bonuses inserted");
            }
        }

        public void createUserBasedBonusCalculation(List<UserBasedBonusCalculationData> bonusCalculationDatas)
        {
            if (isUserRecordsDeleted)
            {
                logger.traceInfo("Inserting user bonuses");
                var bonusCalculationItem = new UserBasedBonusCalculationDataMongoDB();

                bonusCalculationDatas.ForEach(item =>
                {
                    logger.traceInfo("User bonus - contract no: " + item.ContractNumber);
                    logger.traceInfo("User bonus - additional product id: " + item.AdditionalProductId);

                    bonusCalculationItem = bonusCalculationItem.Map(item);
                    bonusCalculationItem._id = ObjectId.GenerateNewId();

                    var response = userBasedCollection.Insert(bonusCalculationItem, Convert.ToString(bonusCalculationItem._id), ErrorLogsHelper.GetCurrentMethod());
                });
                logger.traceInfo("User bonuses inserted");
            }
        }
    }
}
