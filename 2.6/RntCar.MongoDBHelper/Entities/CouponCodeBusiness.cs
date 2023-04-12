using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Entities
{
    public class CouponCodeBusiness : MongoDBInstance
    {
        public CouponCodeBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public CouponCodeBusiness(object client, object database) : base(client, database)
        {
        }

        public MongoDBResponse CreateCouponCode(CouponCodeData couponCodeData)
        {
            var collection = this.getCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            var methodName = ErrorLogsHelper.GetCurrentMethod();

            var couponCode = new CouponCodeDataMongoDB();
            couponCode.Map(couponCodeData);
            couponCode._id = ObjectId.GenerateNewId();

            var response = collection.Insert(couponCode, couponCode.couponCodeId, methodName);
            response.Id = Convert.ToString(couponCode.couponCodeId);

            return response;
        }

        public bool UpdateCouponCode(CouponCodeData couponCodeData, string id)
        {
            var collection = this.getCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
            
            var couponCode = new CouponCodeDataMongoDB();
            couponCode.Map(couponCodeData);
            couponCode._id = ObjectId.Parse(id);

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = couponCode._id.ToString();

            var filter = Builders<CouponCodeDataMongoDB>.Filter.Eq(p => p._id, couponCode._id);
            var response = collection.Replace(couponCode, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);

            if (response != null)
            {
                if (!response.IsAcknowledged)
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

        public void bulkCreateCouponCodes(List<CouponCodeDataMongoDB> couponCodeDataMongoDBs)
        {
            try
            {
                var collection = this.getCollection<CouponCodeDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeCollectionName"));
                collection.InsertMany(couponCodeDataMongoDBs);
            }
            catch (Exception ex)
            {
                //todo will implement error mechanism in mongoDB
            }
        }
    }
}
