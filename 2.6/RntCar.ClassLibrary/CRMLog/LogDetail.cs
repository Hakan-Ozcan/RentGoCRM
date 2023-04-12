using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class LogDetail
    {
        public string organizationId { get; set; }
        public string requestId { get; set; }
        public int operationType { get; set; }
        public string primaryEntity { get; set; }
        public string correlationId { get; set; }
        public string createdBy { get; set; }
        public bool isSystemCreated { get; set; }
        public string messageBlock { get; set; }
        public string messageName { get; set; }
        public DateTime performanceStartTime { get; set; }
        public int depth { get; set; }
        public string pluginTraceLogId { get; set; }
        public int mode { get; set; }
        public string typeName { get; set; }
        public string pluginStepId { get; set; }
        public DateTime createdOn { get; set; }

        public int duration { get; set; }

        public string exceptionDetail { get; set; }
    }
}
