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
    public class CouponCodeDefinitionBusiness : MongoDBInstance
    {
        public CouponCodeDefinitionBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public CouponCodeDefinitionBusiness(object client, object database) : base(client, database)
        {
        }

        public MongoDBResponse CreateCouponCodeDefinition(CouponCodeDefinitionData couponCodeDefinitionData)
        {
            var collection = this.getCollection<CouponCodeDefinitionDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeDefinitionCollectionName"));

            var couponCodeDefinition = new CouponCodeDefinitionDataMongoDB();
            couponCodeDefinition.ShallowCopy(couponCodeDefinitionData);
            couponCodeDefinition._id = ObjectId.GenerateNewId();

            couponCodeDefinition.startDateTimeStamp = new BsonTimestamp(couponCodeDefinition.startDate.converttoTimeStamp());
            couponCodeDefinition.endDateTimeStamp = new BsonTimestamp(couponCodeDefinition.endDate.converttoTimeStamp());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var response = collection.Insert(couponCodeDefinition, couponCodeDefinition.couponCodeDefinitionId, methodName);
            response.Id = couponCodeDefinition._id.ToString();

            return response;
        }

        public bool UpdateCouponCodeDefinition(CouponCodeDefinitionData couponCodeDefinitionData, string id)
        {
            var collection = this.getCollection<CouponCodeDefinitionDataMongoDB>(StaticHelper.GetConfiguration("MongoDBCouponCodeDefinitionCollectionName"));

            var couponCodeDefinition = new CouponCodeDefinitionDataMongoDB();
            couponCodeDefinition.ShallowCopy(couponCodeDefinitionData);
            couponCodeDefinition._id = ObjectId.Parse(id);

            couponCodeDefinition.startDateTimeStamp = new BsonTimestamp(couponCodeDefinition.startDate.converttoTimeStamp());
            couponCodeDefinition.endDateTimeStamp = new BsonTimestamp(couponCodeDefinition.endDate.converttoTimeStamp());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = couponCodeDefinition._id.ToString();

            var filter = Builders<CouponCodeDefinitionDataMongoDB>.Filter.Eq(p => p._id, couponCodeDefinition._id);
            var response = collection.Replace(couponCodeDefinition, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);

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
    }
}
