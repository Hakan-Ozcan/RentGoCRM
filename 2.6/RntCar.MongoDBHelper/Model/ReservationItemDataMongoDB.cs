using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RntCar.ClassLibrary.MongoDB;

namespace RntCar.MongoDBHelper.Model
{
    //purpose of this class to add _id field and prevent dependency with classlibrary and mongodb core dlls
    public class ReservationItemDataMongoDB : ReservationItemData
    {
        public ObjectId _id { get; set; }
        public BsonTimestamp PickupTimeStamp { get; set; }
        public BsonTimestamp DropoffTimeStamp { get; set; }
        public BsonTimestamp CancellationTimeStamp { get; set; }
        public BsonTimestamp NoShowTimeStamp { get; set; }
        public decimal exchangeRate { get; set; }
    }
}
