using MongoDB.Bson;
using System;

namespace RntCar.MongoDBHelper.Model
{
    public class BrokerAvailabilityLog
    {
        public ObjectId _id { get; set; }
        public string content { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
        public decimal totalSeconds { get; set; }
        public string brokerCode { get; set; }
        public string agencyCustomerId { get; set; }
    }
}
