using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Model
{
    public class AvailabilityQueryAnalyzer
    {
        public double elapsedMiliSeconds { get; set; }
        public string trackingNumber { get; set; }
        public ObjectId _id { get; set; }
    }
}
