using System;

namespace RntCar.ClassLibrary._Web
{
    public class MarketingPermission_Web
    {
        public bool etkPermission { get; set; }
        public bool kvkkPermission { get; set; }
        public bool emailPermission { get; set; }
        public bool smsPermission { get; set; }
        public bool notificationPermission { get; set; }
        public Guid marketingPermissionId { get; set; }
    }
}
