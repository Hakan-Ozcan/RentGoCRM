using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class PriceHourEffectRepository : MongoDBInstance
    {
        public PriceHourEffectRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public PriceHourEffectRepository(object client, object database) : base(client, database)
        {
        }
        public PriceHourEffectDataMongoDB getPriceHourEffectByDuration(double totalDuration)
        {
            //index_1
            return this._database.GetCollection<PriceHourEffectDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceEffectCollectionName"))
                       .AsQueryable()
                       .Where(p => p.minimumMinute <= totalDuration &&
                                   p.maximumMinute >= totalDuration &&                                   
                                   p.statecode.Equals((int)GlobalEnums.StateCode.Active)).FirstOrDefault();
        }
        public PriceHourEffectDataMongoDB getZeroPriceEffect()
        {
            //index_1
            return this._database.GetCollection<PriceHourEffectDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceEffectCollectionName"))
                       .AsQueryable()
                       .Where(p => p.effectRate == 0 &&                                   
                                   p.statecode.Equals((int)GlobalEnums.StateCode.Active)).FirstOrDefault();
        }
        public PriceHourEffectDataMongoDB getHundredPriceEffect()
        {
            //index_1
            return this._database.GetCollection<PriceHourEffectDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceEffectCollectionName"))
                       .AsQueryable()
                       .Where(p => p.effectRate == 100 &&
                                   p.statecode.Equals((int)GlobalEnums.StateCode.Active)).FirstOrDefault();
        }

    }
}
