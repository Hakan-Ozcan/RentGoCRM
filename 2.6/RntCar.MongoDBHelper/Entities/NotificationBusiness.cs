using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model.Notification;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Entities
{
    public class NotificationBusiness : MongoDBInstance
    {
        public NotificationBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public NotificationBusiness(object client, object database) : base(client, database)
        {
        }

        public MongoDBResponse CreateNotification(NotificationData notificationData)
        {
            var collection = this.getCollection<NotificationDataMongoDB>("Notifications");

            var notification = new NotificationDataMongoDB()
            {
                _id = ObjectId.GenerateNewId(),
                isRead = notificationData.isRead,
                dTkn = notificationData.deviceToken,
                nCnt = notificationData.notificationContent,
                nType = notificationData.notificationType,
                rObjId = notificationData.regardingObjectId,
                rObjName = notificationData.regardingObjectName
            };

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = notification._id.ToString();

            var response = collection.Insert(notification, itemId, methodName);
            response.Id = itemId;

            return response;
        }

        public MongoDBResponse UpdateNotification(NotificationData notificationData, string id)
        {
            var collection = this.getCollection<NotificationDataMongoDB>("Notifications");

            var notification = new NotificationDataMongoDB()
            {
                _id = ObjectId.Parse(id),
                isRead = notificationData.isRead,
                dTkn = notificationData.deviceToken,
                nCnt = notificationData.notificationContent,
                nType = notificationData.notificationType,
                rObjId = notificationData.regardingObjectId,
                rObjName = notificationData.regardingObjectName
            };

            var methodName = ErrorLogsHelper.GetCurrentMethod();

            var filter = Builders<NotificationDataMongoDB>.Filter.Eq(p => p._id, notification._id);
            var response = collection.Replace(notification, filter, new UpdateOptions { IsUpsert = false }, id, methodName);

            if (response != null && !response.IsAcknowledged)
            {
                MongoDBResponse.ReturnError("Updating failed!");
            }

            return MongoDBResponse.ReturnSuccessWithId(id);
        }
    }
}
