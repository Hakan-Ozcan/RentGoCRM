using MongoDB.Bson;
using RntCar.ClassLibrary.MongoDB.BusinessClosure;

namespace RntCar.MongoDBHelper
{
    public class BusinessClosureDataMongoDB : BusinessClosureData
    {
        public ObjectId _id { get; set; }
    }
}
