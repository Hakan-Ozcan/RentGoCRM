using MongoDB.Bson;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Helper
{
    public class ErrorLogsHelper
    {
        public void LoggingToMongoDB(Exception exception, string itemId ="", string methodName = "")
        {
            var mongoDbInstance = new MongoDBInstance(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                              StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var logDataMongoDB = new LogDataMongoDB();
            logDataMongoDB._id = ObjectId.GenerateNewId();
            logDataMongoDB.errorDateTimeStamp = new BsonTimestamp(DateTime.UtcNow.converttoTimeStamp());
            logDataMongoDB.exceptionDetail = exception.Message.ToString();
            logDataMongoDB.stackTrace = exception.StackTrace.ToString();
            if (methodName != null && methodName != string.Empty)
            {
                logDataMongoDB.actionName = methodName;
            }
            if (exception.InnerException != null)
            {
                logDataMongoDB.innerExceptionDetail = exception.InnerException.Message.ToString();
            }
            if (itemId != null && itemId != string.Empty)
            {
                logDataMongoDB.itemId = itemId;
            }    
            var logCollection = mongoDbInstance.getCollection<LogDataMongoDB>("ErrorLogs");
            var v = logCollection.InsertOneAsync(logDataMongoDB);
            v.Wait();
        }

        //public string GetCurrentMethod()
        //{
        //    var st = new StackTrace();
        //    var sf = st.GetFrame(1);
        //    return sf.GetMethod().Name;
        //}
        public static string GetCurrentMethod()
        {
            var st = new StackTrace();
            var sf = st.GetFrame(1);
            return sf.GetMethod().Name;
        }
    }
}
