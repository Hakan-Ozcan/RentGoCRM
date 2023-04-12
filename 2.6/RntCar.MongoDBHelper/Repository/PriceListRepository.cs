using RntCar.MongoDBHelper.Model;
using System.Linq;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using RntCar.SDK.Common;
using RntCar.ClassLibrary;

namespace RntCar.MongoDBHelper.Repository
{
    public class PriceListRepository : MongoDBInstance
    {
        public PriceListRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public PriceListRepository(object client, object database) : base(client, database)
        {
        }
        public List<PriceListDataMongoDB> getActivePriceLists()
        {
            //get active price list
            return this._database.GetCollection<PriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceListCollectionName"))
                           .AsQueryable()
                           .ToList();
        }

        public PriceListDataMongoDB getActivePriceListByPriceType(int priceType,DateTime relatedDateTime)
        {
            //get active price list
            return this._database.GetCollection<PriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceListCollectionName"))
                           .AsQueryable()
                           .Where(p => p.BeginDate <= relatedDateTime &&
                                  p.EndDate >= relatedDateTime && 
                                  p.State.Equals((int)GlobalEnums.StateCode.Active) &&
                                  p.Status.Equals(1) &&
                                  p.PriceType.Equals(priceType)).FirstOrDefault();

        }

        public PriceListDataMongoDB getRelatedPriceListByPriceCode(int priceType, DateTime relatedDateTime, string priceCodeId)
        {
            //get active price list
            return this._database.GetCollection<PriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceListCollectionName"))
                           .AsQueryable()
                           .Where(p => p.BeginDate <= relatedDateTime &&
                                  p.EndDate >= relatedDateTime &&
                                  p.State.Equals((int)GlobalEnums.StateCode.Active) &&
                                  p.Status.Equals(1) &&
                                  p.PriceCodeId.Equals(priceCodeId) &&
                                  p.PriceType.Equals(priceType)).FirstOrDefault();

        }
    }
}
