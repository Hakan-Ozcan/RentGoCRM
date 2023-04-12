using MongoDB.Bson;
using RntCar.ClassLibrary.MongoDB;

namespace RntCar.MongoDBHelper.Model
{
    public class CampaignDataMongoDB : CampaignData
    {
        public ObjectId _id { get; set; }
        public BsonTimestamp BeginingDateTimeStamp { get; set; }
        public BsonTimestamp EndDateTimeStamp { get; set; }
    }
}
