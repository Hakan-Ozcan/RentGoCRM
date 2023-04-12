using MongoDB.Bson;
using RntCar.ClassLibrary.MongoDB;

namespace RntCar.MongoDBHelper.Model
{
    public class CrmConfigurationDataMongoDB : CrmConfigurationData
    {
        public ObjectId _id { get; set; }
    }
}
