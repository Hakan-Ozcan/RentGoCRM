using System.Collections.Generic;

namespace RntCar.ClassLibrary._Web
{
    public class ReservationCreateParameters_Web : RequestBase
    {
        public ReservationCustomerParameters_Web reservationCustomerParameters { get; set; }
        public QueryParameters reservationQueryParameters { get; set; }
        public GroupCodeInformation_Web reservationEquimentParameters { get; set; }
        public ReservationPriceParameter_Web reservationPriceParameters { get; set; }
        public List<AdditionalProductData_Web> reservationAdditionalProducts { get; set; }
        public string CouponCode { get; set; }
    }
}
