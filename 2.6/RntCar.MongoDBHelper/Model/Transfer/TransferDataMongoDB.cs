using MongoDB.Bson;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Model
{
    public class TransferDataMongoDB : TransferData
    {
        public ObjectId _id { get; set; }
    }
}
