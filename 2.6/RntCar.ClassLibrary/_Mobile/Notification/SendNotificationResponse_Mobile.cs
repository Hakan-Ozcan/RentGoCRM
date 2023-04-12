using System.Collections.Generic;

namespace RntCar.ClassLibrary._Mobile
{
    public class SendNotificationResponse_Mobile:ResponseBase
    {
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
    }
}
