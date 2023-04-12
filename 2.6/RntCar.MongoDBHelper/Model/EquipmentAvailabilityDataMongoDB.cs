using MongoDB.Bson;
using RntCar.ClassLibrary;

namespace RntCar.MongoDBHelper.Model
{
    public class EquipmentAvailabilityDataMongoDB : EquipmentAvailabilityData
    {
        public ObjectId _id { get; set; }
    }
}
