using RntCar.ClassLibrary.MongoDB;

namespace RntCar.ClassLibrary._Web
{
    public class CouponCodeOperationsResponse_Web : ResponseBase
    {
        public int definitionType { get; set; }
        public string couponCodeName { get; set; }
        public decimal definitionPayLaterDiscountValue { get; set; }
        public decimal definitionPayNowDiscountValue { get; set; }
        public string groupCodes { get; set; }
        public CouponCodeData  couponCodeData { get; set; }
    }
}
