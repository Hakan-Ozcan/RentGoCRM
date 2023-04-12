using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Model.Notification
{
    public class NotificationDataMongoDB
    {
        public ObjectId _id{ get; set; }
        public int nType { get; set; }
        public bool isRead { get; set; }
        public string nCnt { get; set; }
        public string rObjId { get; set; }
        public string rObjName { get; set; }
        public string dTkn { get; set; }
    }
}
