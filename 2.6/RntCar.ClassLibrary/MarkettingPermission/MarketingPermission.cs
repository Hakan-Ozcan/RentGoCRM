using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MarkettingPermission
{
    public class MarketingPermission
    {
        public int? operationType { get; set; }
        public int? channelCode { get; set; }
        public bool? callPermission { get; set; }
        public bool? etkPermission { get; set; }
        public bool? kvkkPermission { get; set; }
        public bool? emailPermission { get; set; }
        public bool? smsPermission { get; set; }
        public bool? notificationPermission { get; set; }
        public Guid? marketingPermissionId { get; set; }
        public Guid? contactId { get; set; }
    }
}
