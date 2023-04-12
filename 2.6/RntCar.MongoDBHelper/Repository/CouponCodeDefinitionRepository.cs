using MongoDB.Driver;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Repository
{
    public class CouponCodeDefinitionRepository : MongoDBInstance
    {
        public CouponCodeDefinitionRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public CouponCodeDefinitionRepository(object client, object database) : base(client, database)
        {
        }

        public List<CouponCodeDefinitionDataMongoDB> getCouponCodeDefinitions()
        {
            var collection = this._database.GetCollection<CouponCodeDefinitionDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeDefinitionCollectionName"));
            var query = from e in collection.AsQueryable<CouponCodeDefinitionDataMongoDB>()
                        select e;
            return query.ToList();
        }
        public CouponCodeDefinitionDataMongoDB getCouponCodeDefinitionById(string id)
        {
            var collection = this._database.GetCollection<CouponCodeDefinitionDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeDefinitionCollectionName"));
            var query = from e in collection.AsQueryable<CouponCodeDefinitionDataMongoDB>()
                        where e.couponCodeDefinitionId == id.ToString()
                        select e;
            return query.FirstOrDefault();
        }
    }
}
