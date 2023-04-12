using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class PaymentModel
    {
        public string regardingLogicalName { get; set; }
        public Guid regardingObjectId { get; set; }
        public List<BasketItemDetail> basketItemDetails { get; set; }
        public decimal paidAmount { get; set; }
        public decimal price { get; set; }
        public int installment { get; set; }
        public int paymentChannel { get; set; }
        public BuyerModel buyerModel { get; set; }
        public AddressModel shippingAddress { get; set; }
        public AddressModel billingAddress { get; set; }
        public string cardHolderName { get; set; }
        public string cardNumber { get; set; }
        public int expireMonth { get; set; }
        public int expireYear { get; set; }
        public string cvc { get; set; }
        public string cardUserKey { get; set; }
        public string cardToken { get; set; }
        public int registerCard { get; set; }
        public int langId { get; set; }
    }
}
