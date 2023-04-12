using MongoDB.Driver;
using RntCar.MongoDBHelper.Model.Notification;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Repository
{
    public class NotificationRepository : MongoDBInstance
    {
        private string collectionName { get; set; }
        public NotificationRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBNotificationCollectionName");
        }

        public NotificationRepository(object client, object database) : base(client, database)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBNotificationCollectionName");
        }

        public List<NotificationDataMongoDB> GetNotificationsByDeviceToken(string deviceToken)
        {
            //count condition
            return this._database.GetCollection<NotificationDataMongoDB>("Notifications").AsQueryable()
               .Where(q => q.dTkn.Equals(deviceToken) && q.isRead == true).ToList();
        }

        public List<NotificationDataMongoDB> GetPublicNotifications()
        {
            //1000 genel tipli bildirim
            return this._database.GetCollection<NotificationDataMongoDB>("Notifications").AsQueryable()
               .Where(q => q.nType == 1000 && q.isRead == true).ToList();
        }
    }
}
