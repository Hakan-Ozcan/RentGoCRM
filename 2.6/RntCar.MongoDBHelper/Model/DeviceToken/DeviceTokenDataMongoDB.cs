using MongoDB.Bson;
using RntCar.ClassLibrary;
using System;

namespace RntCar.MongoDBHelper.Model
{
    public class DeviceTokenDataMongoDB:DeviceTokenData
    {
        public ObjectId _id { get; set; }

    }
}
