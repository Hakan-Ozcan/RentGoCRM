using MongoDB.Bson;

namespace RntCar.MongoDBHelper.Model
{
    public class CorporateCustomerDataMongoDB : RntCar.ClassLibrary.MongoDB.CorporateCustomerData
    {
        public ObjectId _id { get; set; }
    }
}
