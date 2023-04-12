using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using RntCar.ClassLibrary.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Mappers
{
    public class PriceMapper
    {
        public ReservationPriceParameters buildReservationPriceParameter(ReservationCustomerParameters reservationCustomerParameters,
                                                                         ReservationPriceParameter_Web reservationPriceParameter_Web,
                                                                         List<CreditCardData> creditCardDatas,
                                                                         decimal totalAmount,
                                                                         Guid individualCustomerId,
                                                                         int virtualPosId)
        {


            return new ReservationPriceParameters
            {

                campaignId = reservationPriceParameter_Web.campaignId,
                creditCardData = creditCardDatas,
                installment = reservationPriceParameter_Web.installment,
                installmentTotalAmount = reservationPriceParameter_Web.installmentAmount,
                pricingType = reservationCustomerParameters.customerType == (int)GlobalEnums.CustomerType.Individual ?
                             individualCustomerId.ToString() :
                             reservationPriceParameter_Web.pricingType,
                price = totalAmount,
                discountAmount = 0,
                virtualPosId = virtualPosId,
                paymentType = reservationPriceParameter_Web.paymentChoice,
                isMonthly = reservationPriceParameter_Web.isMonthly.HasValue ? reservationPriceParameter_Web.isMonthly.Value : false,
                use3DSecure = reservationPriceParameter_Web.use3DSecure,
                callBackUrl = reservationPriceParameter_Web.callBackUrl
            };
        }

        public ReservationPriceParameters buildReservationPriceParameter(ReservationCustomerParameters reservationCustomerParameters,
                                                                         ReservationPriceParameter_Mobile reservationPriceParameter_Mobile,
                                                                         List<CreditCardData> creditCardDatas,
                                                                         decimal totalAmount,
                                                                         Guid individualCustomerId,
                                                                         int virtualPosId)
        {


            return new ReservationPriceParameters
            {

                campaignId = reservationPriceParameter_Mobile.campaignId,
                creditCardData = creditCardDatas,
                installment = reservationPriceParameter_Mobile.installment,
                installmentTotalAmount = reservationPriceParameter_Mobile.installmentAmount,
                pricingType = reservationCustomerParameters.customerType == (int)GlobalEnums.CustomerType.Individual ?
                             individualCustomerId.ToString() :
                             reservationPriceParameter_Mobile.pricingType,
                price = totalAmount,
                discountAmount = 0,
                virtualPosId = virtualPosId,
                paymentType = reservationPriceParameter_Mobile.paymentChoice,
                use3DSecure = reservationPriceParameter_Mobile.use3DSecure,
                callBackUrl = reservationPriceParameter_Mobile.callBackUrl
            };
        }
        public ReservationPriceParameters buildReservationPriceParameter(ReservationPriceParameter_Broker reservationPriceParameter_Broker,
                                                                         decimal totalAmount,
                                                                         Guid individualCustomerId)
        {
            return new ReservationPriceParameters
            {
                installment = 1,
                installmentTotalAmount = totalAmount,
                pricingType = Convert.ToString(reservationPriceParameter_Broker.paymentMethodCode),
                price = totalAmount,
                discountAmount = 0,
                paymentType = (int)rnt_reservation_rnt_paymentchoicecode.PayLater
            };
        }
    }
}
