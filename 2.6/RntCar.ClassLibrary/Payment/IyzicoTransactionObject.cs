using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class IyzicoTransactionObject
    {
        public string ConversationId { get; set; }
        public string IyzicoCommission { get; set; }
        public string IyzicoConversionAmount { get; set; }
        public string IyzicoFee { get; set; }
        public string PaidPrice { get; set; }
        public string Price { get; set; }
        public string TransactionCurrency { get; set; }

        public int Installment { get; set; }

        public long TransactionId { get; set; }
        public string TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public int? TransactionStatus { get; set; }

        public long PaymentId { get; set; }
        public long PaymentTransactionId { get; set; }
    }
}
