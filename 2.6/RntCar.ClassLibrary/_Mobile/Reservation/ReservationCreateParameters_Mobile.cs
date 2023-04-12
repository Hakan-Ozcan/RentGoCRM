using System.Collections.Generic;

namespace RntCar.ClassLibrary._Mobile
{
    public class ReservationCreateParameters_Mobile : RequestBase
    {
        public ReservationCustomerParameters_Mobile reservationCustomerParameters { get; set; }
        public QueryParameters reservationQueryParameters { get; set; }
        public GroupCodeInformation_Mobile reservationEquimentParameters { get; set; }
        public ReservationPriceParameter_Mobile reservationPriceParameters { get; set; }
        public List<AdditionalProductData_Mobile> reservationAdditionalProducts { get; set; }
        public string CouponCode { get; set; }
    }
}
