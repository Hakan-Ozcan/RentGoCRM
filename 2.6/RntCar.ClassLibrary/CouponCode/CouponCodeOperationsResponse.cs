using RntCar.ClassLibrary.MongoDB;

namespace RntCar.ClassLibrary
{
    public class CouponCodeOperationsResponse : ResponseBase
    {
        public string couponCode { get; set; }

        public int definitionType { get; set; }

        public decimal definitionPayLaterDiscountValue { get; set; }

        public decimal definitionPayNowDiscountValue { get; set; }

        public string definitionName { get; set; }

        public CouponCodeData couponCodeData { get; set; }
    }
}
