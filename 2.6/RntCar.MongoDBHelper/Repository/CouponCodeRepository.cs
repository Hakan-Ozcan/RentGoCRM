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
    public class CouponCodeRepository : MongoDBInstance
    {
        public CouponCodeRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public CouponCodeRepository(object client, object database) : base(client, database)
        {
        }

        public List<CouponCodeDataMongoDB> getCouponCodes()
        {
            var collection = this._database.GetCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            var query = from e in collection.AsQueryable<CouponCodeDataMongoDB>()
                        select e;
            return query.ToList();
        }
        public CouponCodeDataMongoDB getCouponCodeByDefinitionIdandCouponCode(string definitionId, string couponCode)
        {
            var collection = this._database.GetCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            var query = (from e in collection.AsQueryable<CouponCodeDataMongoDB>()
                         where e.couponCodeDefinitionId.Equals(definitionId) && e.couponCode.Equals(couponCode)
                         select e).ToList();
            return query.FirstOrDefault();
        }
        public List<CouponCodeDataMongoDB> getCouponCodeDetailsByCouponCode(string couponCode)
        {
            var collection = this._database.GetCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            var query = (from e in collection.AsQueryable<CouponCodeDataMongoDB>()
                         where e.couponCode.Equals(couponCode)
                         select e).ToList();
            return query.ToList();
        }
        public List<CouponCodeDataMongoDB> getNotBurnedCouponCodeListByCouponCode(string couponCode)
        {
            var collection = this._database.GetCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            var query = (from e in collection.AsQueryable<CouponCodeDataMongoDB>()
                         where e.couponCode.Equals(couponCode) && !e.statusCode.Equals(2)
                         select e).ToList();
            return query.ToList();
        }
        public List<CouponCodeDataMongoDB> getCouponCodesByReservationId(Guid reservationId)
        {
            var collection = this._database.GetCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            var query = (from e in collection.AsQueryable<CouponCodeDataMongoDB>()
                         where e.reservationId.Equals(reservationId)
                         select e).ToList();

            return query.ToList();
        }
        public List<CouponCodeDataMongoDB> getCouponCodesByContractId(Guid contractId)
        {
            var collection = this._database.GetCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            var query = (from e in collection.AsQueryable<CouponCodeDataMongoDB>()
                         where e.contractId.Equals(contractId)
                         select e).ToList();

            return query.ToList();
        }
        public List<CouponCodeDataMongoDB> getCouponCodesByContactId(Guid contactId)
        {
            var collection = this._database.GetCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            var query = (from e in collection.AsQueryable<CouponCodeDataMongoDB>()
                         where e.contactId.Equals(contactId)
                         select e).ToList();

            return query.ToList();
        }
        public List<CouponCodeDataMongoDB> getCouponCodesByAccountId(Guid accountId)
        {
            var collection = this._database.GetCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            var query = (from e in collection.AsQueryable<CouponCodeDataMongoDB>()
                         where e.accountId.Equals(accountId)
                         select e).ToList();

            return query.ToList();
        }
        public List<CouponCodeDataMongoDB> getCouponCodesByDefinitionId(Guid couponCodeDefinition)
        {
            var collection = this._database.GetCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            var query = (from e in collection.AsQueryable<CouponCodeDataMongoDB>()
                         where e.couponCodeDefinitionId.Equals(couponCodeDefinition)
                         select e).ToList();

            return query.ToList();
        }
    }
}
