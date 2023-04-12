using Iyzipay.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary 
{
    public class Iyzico3DReturnParameter : AuthInfo
    {
        public string Locale { get; set; }
        public string ConversationId { get; set; }
        public string PaymentId { get; set; }
        public string ConversationData { get; set; }
         
    }
}
