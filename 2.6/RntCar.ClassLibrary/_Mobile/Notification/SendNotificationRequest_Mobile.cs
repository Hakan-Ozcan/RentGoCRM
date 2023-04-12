using System;
using System.Collections.Generic;
namespace RntCar.ClassLibrary._Mobile
{
    public class SendNotificationRequest_Mobile:RequestBase
    {
        public List<string> deviceTokens { get; set; }
        public NotificationData notification { get; set; }
    }
}
