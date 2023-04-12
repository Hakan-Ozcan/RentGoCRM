using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ManualPaymentParameters
    {
        public int langId { get; set; }
        public string description { get; set; }
        public string entityId { get; set; }
        public string creditCardNumber { get; set; }
        public string nameOnCard { get; set; }
        public int? year { get; set; }
        public int? month { get; set; }
        public string cvc { get; set; }
        public Guid? creditCardId { get; set; }
        public Guid? reservationId { get; set; }
        public Guid? contractId { get; set; }
        public decimal? paymentAmount { get; set; }
        public decimal? amount { get; set; }      
        public decimal? amount2 { get; set; }
        public decimal? amount3 { get; set; }
        public Guid? additionalProductId { get; set; }
        public Guid? invoiceAddressId { get; set; }
        public int manualPaymentType { get; set; }
        public Guid? additionalProductId2 { get; set; }
        public Guid? additionalProductId3 { get; set; }
        public Guid? invoiceId { get; set; }
        public bool isDebt { get; set; }
        public int? channelCode { get; set; }
    }
}
