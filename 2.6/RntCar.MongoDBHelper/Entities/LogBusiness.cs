using MongoDB.Bson;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Model.Log;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Entities
{
    public class LogBusiness : MongoDBInstance
    {
        public LogBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public LogBusiness(object client, object database) : base(client, database)
        {
        }
        public MongoDBResponse createLog(LogDetail logDetail)
        {

            var collection = this.getCollection<LogDetailMongoDB>(StaticHelper.GetConfiguration("MongoDBLogDetail"));

            var logDetailMongoDB = new LogDetailMongoDB();

            logDetailMongoDB.Map(logDetail);
            logDetailMongoDB._id = ObjectId.GenerateNewId();   

            var response = collection.Insert(logDetailMongoDB,"","");
            response.Id = Convert.ToString(logDetailMongoDB._id);

            return response;
        }
        public void createLog(List<LogDetail> logDetail)
        {
            
            var collection = this.getCollection<LogDetailMongoDB>(StaticHelper.GetConfiguration("MongoDBLogDetail"));

            var logDetailMongoDB = new List<LogDetailMongoDB>();
            foreach (var item in logDetail)
            {
                LogDetailMongoDB l = new LogDetailMongoDB();
                l.Map(item);
                l._id = ObjectId.GenerateNewId();
                logDetailMongoDB.Add(l);
            }
            collection.InsertMany(logDetailMongoDB);

        }
    }
}
