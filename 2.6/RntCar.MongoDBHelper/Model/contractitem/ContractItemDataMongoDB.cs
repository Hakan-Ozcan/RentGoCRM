using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using RntCar.ClassLibrary.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Model
{
    public class ContractItemDataMongoDB : ContractItemData
    {
        public ObjectId _id { get; set; }
        public BsonTimestamp PickupTimeStamp { get; set; }
        public BsonTimestamp DropoffTimeStamp { get; set; }

        [BsonIgnore]
        public string plateNumber{ get; set; }
    }
}
