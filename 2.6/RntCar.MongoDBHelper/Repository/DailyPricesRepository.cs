using MongoDB.Driver;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class DailyPricesRepository : MongoDBInstance
    {
        public DailyPricesRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public DailyPricesRepository(object client, object database) : base(client, database)
        {
        }

        public List<DailyPriceDataMongoDB> getDailyPrices()
        {
            var collection = this._database.GetCollection<DailyPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBDailyPrice"));
            var query = from e in collection.AsQueryable<DailyPriceDataMongoDB>()
                        select e;
            return query.ToList();
        }
        public List<DailyPriceDataMongoDB> getDailyPricesByTrackingNumber(string trackingNumber)
        {
            var collection = this._database.GetCollection<DailyPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBDailyPrice"));
            var query = from e in collection.AsQueryable<DailyPriceDataMongoDB>()
                        where e.trackingNumber.Equals(trackingNumber)
                        select e;
            return query.ToList();
        }
        public List<DailyPriceDataMongoDB> getDailyPricesByReservationItemId(Guid reservationItemId)
        {
            var collection = this._database.GetCollection<DailyPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBDailyPrice"));
            var query = (from e in collection.AsQueryable<DailyPriceDataMongoDB>()
                        where  e.reservationItemId.Equals(reservationItemId)
                        select e).ToList();
           
            return query.OrderBy(p => p.priceDateTimeStamp.Value).ToList();
        }
        public List<DailyPriceDataMongoDB> getDailyPricesByReservationItemId_str(Guid reservationItemId)
        {
            var collection = this._database.GetCollection<DailyPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBDailyPrice"));
            var query = (from e in collection.AsQueryable<DailyPriceDataMongoDB>()
                         where e.reservationItemId_str.Equals(reservationItemId.ToString())
                         select e).ToList();

            return query.OrderBy(p => p.priceDateTimeStamp.Value).ToList();
        }
        public DailyPriceDataMongoDB getFirstDayPrice(Guid reservationItemId)
        {
            var collection = this._database.GetCollection<DailyPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBDailyPrice"));
            var result = (from e in collection.AsQueryable<DailyPriceDataMongoDB>()
                          where e.reservationItemId.Equals(reservationItemId)
                          select e).ToList();
           return  result.OrderBy(p => p.priceDateTimeStamp.Value).FirstOrDefault();

        }
    }
}
