using MongoDB.Bson;
using RntCar.ClassLibrary.MongoDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Model
{
    public class GroupCodeListPriceDataMongoDB : GroupCodeListPriceData
    {
        public ObjectId _id { get; set; }
    }
}
