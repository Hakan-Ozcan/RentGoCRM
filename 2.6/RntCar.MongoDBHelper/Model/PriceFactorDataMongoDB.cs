using MongoDB.Bson;
using RntCar.ClassLibrary.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Model
{
    public class PriceFactorDataMongoDB : PriceFactorData
    {
        public ObjectId _id { get; set; }
        public BsonTimestamp beginDateTimeStamp { get; set; }
        public BsonTimestamp endDateTimeStamp { get; set; }
    }
}
