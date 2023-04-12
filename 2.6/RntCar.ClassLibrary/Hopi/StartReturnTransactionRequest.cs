using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class StartReturnTransactionRequest
    {
        public string licenceKey { get; set; }
        public string merchantCode { get; set; }
        public string storeCode { get; set; }
        public string transactionId { get; set; }
        public decimal campaignFreeAmount { get; set; }
        public ReturnCampaignDetails returnCampaignDetails { get; set; }
        public TransactionInfos transactionInfos { get; set; }
    }

    public class ReturnCampaignDetails
    {
        public string campaignCode { get; set; }
        public decimal returnPayment { get; set; }
        public decimal requestedCoinReturnAmount { get; set; }

    }
    public class TransactionInfos
    {
        public string barcode { get; set; }
        public decimal quantity { get; set; }
        public decimal amount { get; set; }
        public string campaign { get; set; }

    }
}
