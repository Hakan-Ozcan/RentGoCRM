using MongoDB.Bson;
using RntCar.ClassLibrary.MongoDB;

namespace RntCar.MongoDBHelper.Model
{
    public class KilometerLimitDataMongoDB : KilometerLimitData
    {
        public ObjectId _id { get; set; }
    }
}
