using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper
{
    public class MongoDBInstance
    {
        public IMongoClient _client { get; set; }
        public IMongoDatabase _database { get; set; }

        public MongoDBInstance(String serverName, String dataBaseName)
        {
            _client = new MongoClient(serverName);//"mongodb://localhost:27017"
            _database = _client.GetDatabase(dataBaseName);
        }
        public MongoDBInstance(object client, object database)
        {
            _client = (IMongoClient)client;
            _database = (IMongoDatabase)database;
        }
        public IMongoCollection<T> getCollection<T>(String collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }

        public DateTime ChangeDateTimeForGivenTimeZone(DateTime time, TimeZoneInfo zone)
        {

            return TimeZoneInfo.ConvertTime(time, zone);
        }

    }
    public static class MongoDBInstanceStatics
    {
        public static MongoDBResponse Insert<T>(this IMongoCollection<T> collection, T _item, string itemId = "", string methodName = "")
        {
            Exception exception = null;
            try
            {
                collection.InsertOne(_item);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                exception = ex;
                return MongoDBResponse.ReturnError(ex.ToString());
                //throw new Exception(ex.Message);
            }
            finally
            {
                if (exception != null)
                {
                    ErrorLogsHelper errorLoggingHelper = new ErrorLogsHelper();
                    errorLoggingHelper.LoggingToMongoDB(exception, itemId, methodName);
                }
            }
        }
        public static async Task<MongoDBResponse> InsertAsync<T>(this IMongoCollection<T> collection, T _item)
        {
            try
            {
                await collection.InsertOneAsync(_item);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }

        public static ReplaceOneResult Replace<T>(this IMongoCollection<T> collection, 
                                                  T _item, 
                                                  FilterDefinition<T> filter, 
                                                  UpdateOptions updateOptions, 
                                                  string itemId = "", 
                                                  string methodName = "")
        {
            Exception exception = null;
            ReplaceOneResult result = null;
            try
            {
                return collection.ReplaceOne(filter, _item, updateOptions);
            }
            catch (Exception ex)
            {
                exception = ex;
                return result;
                //throw new Exception(ex.Message);
            }
            finally
            {
                if (exception != null)
                {
                    ErrorLogsHelper errorLoggingHelper = new ErrorLogsHelper();
                    errorLoggingHelper.LoggingToMongoDB(exception, itemId, methodName);
                }
            }
        }
    }
}

