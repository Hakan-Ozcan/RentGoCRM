using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model.MonthlyGroupCodePriceList;
using RntCar.SDK.Common;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class MonthlyGroupCodePriceListRepository : MongoDBInstance
    {
        private string collectionName { get; set; }
        public MonthlyGroupCodePriceListRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBMonthlyGroupCodePriceCollectionName");
        }

        public MonthlyGroupCodePriceListRepository(object client, object database) : base(client, database)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBMonthlyGroupCodePriceCollectionName");
        }
        public MonthlyGroupCodePriceListDataMongoDB getMonthlyGroupCodePriceListByGivenPriceList(string priceListId,string groupCodeId ,int month)
        {
            return this._database.GetCollection<MonthlyGroupCodePriceListDataMongoDB>(collectionName)
                        .AsQueryable()
                        .Where(p => 
                        p.stateCode == (int)GlobalEnums.StateCode.Active &&
                        p.monthlyPriceListId == priceListId &&
                               p.groupCodeId == groupCodeId &&
                               p.month == month)
                        .FirstOrDefault();
        }
    }
}
