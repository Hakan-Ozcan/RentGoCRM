using MongoDB.Bson;
using RntCar.ClassLibrary.Report;

namespace RntCar.MongoDBHelper.Model
{
    public class EquipmentAvailabilityOldDataMongoDB : EquipmentAvailabilityOldData
    {
        public ObjectId _id { get; set; }
    }
}
