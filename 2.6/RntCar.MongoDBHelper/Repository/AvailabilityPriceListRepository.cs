using RntCar.MongoDBHelper.Model;
using System.Linq;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using RntCar.SDK.Common;
using RntCar.ClassLibrary;

namespace RntCar.MongoDBHelper.Repository
{
    public class AvailabilityPriceListRepository : MongoDBInstance
    {
        public AvailabilityPriceListRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public AvailabilityPriceListRepository(object client, object database) : base(client, database)
        {
        }
        public List<AvailabilityPriceListDataMongoDB> getAvailabilityPriceByPriceLists()
        {
            return this._database.GetCollection<AvailabilityPriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityPriceListListCollectionName"))
                           .AsQueryable()
                           .ToList();
        }
        public List<AvailabilityPriceListDataMongoDB> getAvailabilityPriceByPriceList(string priceListId)
        {
            return this._database.GetCollection<AvailabilityPriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityPriceListListCollectionName"))
                           .AsQueryable()
                           .Where(p => p.PriceListId.Equals(priceListId)).ToList();
        }
        public AvailabilityPriceListDataMongoDB getAvailabilityPriceByPriceListWithDuration(string priceListId, int availabilityRatio)
        {
            return this._database.GetCollection<AvailabilityPriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityPriceListListCollectionName"))
                           .AsQueryable()
                           .Where(p => p.PriceListId.Equals(priceListId) &&
                                    p.StateCode == (int)GlobalEnums.StateCode.Active &&
                                  p.MinimumAvailability <= availabilityRatio &&
                                  p.MaximumAvailability >= availabilityRatio).FirstOrDefault();
        }
        public AvailabilityPriceListDataMongoDB getAvailabilityPriceByPriceListWithDurationByGroupCode(string priceListId, int availabilityRatio, Guid groupCodeId)
        {
            return this._database.GetCollection<AvailabilityPriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityPriceListListCollectionName"))
                           .AsQueryable()
                           .Where(p => p.PriceListId.Equals(priceListId) &&
                                    p.StateCode == (int)GlobalEnums.StateCode.Active &&
                                  p.groupCodeId.Equals(groupCodeId) &&
                                  p.MinimumAvailability <= availabilityRatio &&
                                  p.MaximumAvailability >= availabilityRatio).FirstOrDefault();
        }
        public List<AvailabilityPriceListDataMongoDB> getAvailabilityPriceByPriceListWithDurationByGroupCode(string priceListId, Guid groupCodeId)
        {
            return this._database.GetCollection<AvailabilityPriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityPriceListListCollectionName"))
                           .AsQueryable()
                           .Where(p => p.PriceListId.Equals(priceListId) &&
                                    p.StateCode == (int)GlobalEnums.StateCode.Active &&
                                  p.groupCodeId.Equals(groupCodeId)).ToList();
        }
    }
}
