using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile.MarketingPermission
{
    public class MarketingPermissionParameters_Mobile:RequestBase
    {
        public bool? emailPermission { get; set; }
        public bool? smsPermission { get; set; }
        public bool? notificationPermission { get; set; }
        public int? operationType { get; set; }
        public Guid? contactId { get; set; }
    }
}
