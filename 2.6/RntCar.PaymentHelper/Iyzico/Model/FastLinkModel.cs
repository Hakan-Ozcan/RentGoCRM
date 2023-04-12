using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.PaymentHelper.iyzico.Model
{
    public class FastLinkRequest
    {
        public string ConversationId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Base64EncodedImage { get; set; }
        public string Price { get; set; }
        public string Currency { get; set; }
    }

    public class FastLinkResponse
    {
        public string status { get; set; }
        public int statusCode { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string locale { get; set; }
        public string systemTime { get; set; }
        public string url { get; set; }
    }
}
