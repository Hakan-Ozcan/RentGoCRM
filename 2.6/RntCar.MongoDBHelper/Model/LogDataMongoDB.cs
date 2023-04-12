using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Model
{
    public class LogDataMongoDB
    {
        public ObjectId _id { get; set; }
        public BsonTimestamp errorDateTimeStamp { get; set; }
        public string actionName { get; set; }
        public string exceptionDetail { get; set; }
        public string innerExceptionDetail { get; set; }
        public string stackTrace { get; set; }
        public string itemId { get; set; }
        
    }
}
