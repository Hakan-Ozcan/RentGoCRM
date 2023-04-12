using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CreateRefundParameters
    {
        public Guid? reservationId { get; set; }
        public Guid? contractId { get; set; }
        public int paymentChannel { get; set; }
        public string baseurl { get; set; }
        public string apikey { get; set; }
        public string secretKey { get; set; }
        public string conversationId { get; set; }
        public string paymentTransactionId { get; set; }
        public decimal refundAmount { get; set; }
        public int langId { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public string vendorCode { get; set; }
        public int vendorId { get; set; }
        public bool isDepositRefund { get; set; }
    }
}
