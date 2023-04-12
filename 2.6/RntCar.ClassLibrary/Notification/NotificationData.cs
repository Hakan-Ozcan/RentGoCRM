using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class NotificationData
    {
        public int notificationType { get; set; }
        public bool isRead { get; set; }
        public string notificationContent { get; set; }
        public string regardingObjectId { get; set; }
        public string regardingObjectName { get; set; }
        public string deviceToken { get; set; }
    }
}
