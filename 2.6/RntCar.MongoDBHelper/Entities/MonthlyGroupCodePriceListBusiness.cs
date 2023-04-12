using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.MonthlyGroupCodePriceList;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model.MonthlyGroupCodePriceList;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.MongoDBHelper.Entities
{
    public class MonthlyGroupCodePriceListBusiness : MongoDBInstance
    {
        public MonthlyGroupCodePriceListBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public MonthlyGroupCodePriceListBusiness(object client, object database) : base(client, database)
        {
        }

        public MongoDBResponse createMonthlyGroupCodePrice(MonthlyGroupCodePriceListData monthlyGroupCodePriceListData)
        {
            var collection = this.getCollection<MonthlyGroupCodePriceListData>(StaticHelper.GetConfiguration("MongoDBMonthlyGroupCodePriceCollectionName"));

            var monthlyPriceListDataMongoDB = new MonthlyGroupCodePriceListDataMongoDB();

            monthlyPriceListDataMongoDB.Map(monthlyGroupCodePriceListData);
            monthlyPriceListDataMongoDB._id = new ObjectId(new Guid(monthlyGroupCodePriceListData.monthlyGroupCodeListId).ToByteArray().Take(12).ToArray());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = monthlyPriceListDataMongoDB._id.ToString();

            var response = collection.Insert(monthlyPriceListDataMongoDB, itemId, methodName);
            response.Id = Convert.ToString(monthlyPriceListDataMongoDB._id);

            return response;
        }
        public bool updateMonthlyGroupCodePrice(MonthlyGroupCodePriceListData monthlyGroupCodePriceListData, string id)
        {
            var collection = this.getCollection<MonthlyGroupCodePriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBMonthlyGroupCodePriceCollectionName"));

            var monthlyPriceListDataMongoDB = new MonthlyGroupCodePriceListDataMongoDB();
            monthlyPriceListDataMongoDB.Map(monthlyGroupCodePriceListData);
            monthlyPriceListDataMongoDB._id = new ObjectId(new Guid(id).ToByteArray().Take(12).ToArray());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = monthlyPriceListDataMongoDB._id.ToString();

            var filter = Builders<MonthlyGroupCodePriceListDataMongoDB>.Filter.Eq(p => p._id, monthlyPriceListDataMongoDB._id);
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
