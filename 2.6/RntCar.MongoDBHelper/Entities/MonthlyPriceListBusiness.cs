using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.MonthlyPriceList;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Model.MonthlyPriceList;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.MongoDBHelper.Entities
{
    public class MonthlyPriceListBusiness : MongoDBInstance
    {
        public MonthlyPriceListBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public MonthlyPriceListBusiness(object client, object database) : base(client, database)
        {
           
        }
        public MongoDBResponse createMonthlyPrice(MonthlyPriceListData monthlyPriceListData)
        {
            var collection = this.getCollection<MonthlyPriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBMonthlyPriceCollectionName"));

            var monthlyPriceListDataMongoDB = new MonthlyPriceListDataMongoDB();

            monthlyPriceListDataMongoDB.Map(monthlyPriceListData);
            monthlyPriceListDataMongoDB._id = new ObjectId(new Guid(monthlyPriceListData.monthlyPriceListId).ToByteArray().Take(12).ToArray());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = monthlyPriceListDataMongoDB._id.ToString();

            var response = collection.Insert(monthlyPriceListDataMongoDB, itemId, methodName);
            response.Id = Convert.ToString(monthlyPriceListDataMongoDB._id);

            return response;
        }
        public bool updateMonthlyPrice(MonthlyPriceListData monthlyPriceListData, string id)
        {
            var collection = this.getCollection<MonthlyPriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBMonthlyPriceCollectionName"));

            var monthlyPriceListDataMongoDB = new MonthlyPriceListDataMongoDB();
            monthlyPriceListDataMongoDB.Map(monthlyPriceListData);
            monthlyPriceListDataMongoDB._id = new ObjectId(new Guid(id).ToByteArray().Take(12).ToArray());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = monthlyPriceListDataMongoDB._id.ToString();

            var filter = Builders<MonthlyPriceListDataMongoDB>.Filter.Eq(p => p._id, monthlyPriceListDataMongoDB._id);
            var response = collection.Replace(monthlyPriceListDataMongoDB, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);

            if (response != null)
            {
                if (response.IsAcknowledged)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }
    }
}
