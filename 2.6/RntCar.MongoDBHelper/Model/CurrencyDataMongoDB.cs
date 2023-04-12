using MongoDB.Bson;

namespace RntCar.MongoDBHelper.Model
{
    public class CurrencyDataMongoDB
    {
        public ObjectId _id { get; set; }
        public decimal exchangerate { get; set; }
    }
}
