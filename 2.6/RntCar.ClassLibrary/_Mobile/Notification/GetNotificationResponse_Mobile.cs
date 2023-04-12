using System.Collections.Generic;

namespace RntCar.ClassLibrary._Mobile
{
    public class GetNotificationResponse_Mobile:ResponseBase
    {
        public List<NotificationData> notifications { get; set; }
    }
}
