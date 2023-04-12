using MongoDB.Bson;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Model
{
    public class PositionalBonusCalculationDataMongoDB : PositionalBonusCalculationData
    {
        public ObjectId _id { get; set; }
    }
}
