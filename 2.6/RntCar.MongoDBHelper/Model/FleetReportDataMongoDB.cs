using MongoDB.Bson;
using RntCar.ClassLibrary.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Model
{
    public class FleetReportDataMongoDB : FleetReportData
    {
        public ObjectId _id { get; set; }
    }
}
