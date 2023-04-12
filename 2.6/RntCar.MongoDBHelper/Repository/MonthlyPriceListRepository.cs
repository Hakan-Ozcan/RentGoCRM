using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model.MonthlyPriceList;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class MonthlyPriceListRepository : MongoDBInstance
    {
        private string collectionName { get; set; }
        public MonthlyPriceListRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBMonthlyPriceCollectionName");
        }

        public MonthlyPriceListRepository(object client, object database) : base(client, database)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBMonthlyPriceCollectionName");
        }

        public MonthlyPriceListDataMongoDB getIndividualMonthlyPriceList()
        {
            return this._database.GetCollection<MonthlyPriceListDataMongoDB>(collectionName)
                        .AsQueryable()
                        .Where(p => p.priceType == 1 && p.stateCode == (int)GlobalEnums.StateCode.Active)
                        .FirstOrDefault();
        }
        public MonthlyPriceListDataMongoDB getCorporateMonthlyPriceList(Guid priceCodeId)
        {
            return this._database.GetCollection<MonthlyPriceListDataMongoDB>(collectionName)
                        .AsQueryable()
                        .Where(p => p.priceType == 2
                         && p.priceCodeId == Convert.ToString(priceCodeId)
                         && p.stateCode == (int)GlobalEnums.StateCode.Active)

                        .FirstOrDefault();
        }
        public MonthlyPriceListDataMongoDB getIndividualMonthlyPriceListById(string monthlyPriceListId)
        {
            return this._database.GetCollection<MonthlyPriceListDataMongoDB>(collectionName)
                        .AsQueryable()
                        .Where(p => p.monthlyPriceListId == monthlyPriceListId)
                        .FirstOrDefault();
        }
    }
}
