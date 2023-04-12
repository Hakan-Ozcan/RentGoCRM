using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class NotifyCheckoutRequest
    {
        public string merchantCode { get; set; }
        public string storeCode { get; set; }
        public long birdId { get; set; }
        public DateTime dateTime { get; set; }
        public string transactionId { get; set; }
        public AmountDetails[] paymentDetails { get; set; }
        public AmountDetails[] campaignFreePaymentDetails { get; set; }
        public TransactionInfo[] transactionInfos { get; set; }
        public AmountDetails[] subtotalDetails { get; set; }
        public UsedCampaignDetails[] usedCampaignDetails { get; set; }
    }

    public class BenefitDetails 
    {
        public decimal coins { get; set; }
    }
    public class AmountDetails
    {
        public decimal percent { get; set; }
        public decimal amount { get; set; }
    }
    public class UsedCoinDetails
    {

    }
    public class UsedCampaignDetails
    {
        public string campaignCode { get; set; }
        public AmountDetails[] amountDetails { get; set; }
        public BenefitDetails benefit { get; set; }

    }
    public class TransactionInfo
    {
        public string barcode { get; set; }
        public int quantity { get; set; }
        public decimal amount { get; set; }
        public string[] campaign { get; set; }
    }
}
