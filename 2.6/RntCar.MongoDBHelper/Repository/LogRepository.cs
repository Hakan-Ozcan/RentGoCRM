using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.MongoDBHelper.Model.Log;
using RntCar.SDK.Common;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class LogRepository : MongoDBInstance
    {
        public LogRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public LogRepository(object client, object database) : base(client, database)
        {
        }
        public List<LogDetailMongoDB> getLogs(string typeName, string keyword)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return this._database.GetCollection<LogDetailMongoDB>(StaticHelper.GetConfiguration("MongoDBLogDetail"))
                           .AsQueryable()
                           .Where(p => p.exceptionDetail.Contains(keyword) || p.messageBlock.Contains(keyword)).ToList();
            }
            return this._database.GetCollection<LogDetailMongoDB>(StaticHelper.GetConfiguration("MongoDBLogDetail"))
             .AsQueryable()
             .Where(p => p.messageName.Contains(typeName) &&
                    (p.exceptionDetail.Contains(keyword) || p.messageBlock.Contains(keyword))).ToList();
        }

        public List<LogData> getTabletLogs(string typeName, string keyword)
        {
         


            if (string.IsNullOrEmpty(typeName))
            {
                return this._database.GetCollection<LogData>("WebServiceLogs")
                           .AsQueryable()
                           .Where(p => p.message.Contains(keyword)).ToList();
            }

            return this._database.GetCollection<LogData>("WebServiceLogs")
             .AsQueryable()
             .Where(p => p.message.Contains(typeName) || p.message.Contains(keyword)).ToList();
        }


    }
    public class LogData
    {
        public ObjectId _id { get; set; }
        public string date { get; set; }
        public string type { get; set; }
        public string controller { get; set; }
        public string method { get; set; }
        public List<string> message { get; set; }
    }
}
