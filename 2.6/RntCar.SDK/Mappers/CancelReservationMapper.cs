using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using RntCar.SDK.Common;

namespace RntCar.SDK.Mappers
{
    public class CancelReservationMapper
    {
        public ReservationCancellationParameters buildCancelReservationParameters(CancelReservationParameters_Web cancelReservationParameters_Web)
        {
            return new ReservationCancellationParameters
            {
                cancellationReason = cancelReservationParameters_Web.cancellationReason,
                langId = cancelReservationParameters_Web.langId,
                pnrNumber = cancelReservationParameters_Web.pnrNumber,
                reservationId = cancelReservationParameters_Web.reservationId
            };
        }

        public ReservationCancellationParameters buildCancelReservationParameters(CancelReservationParameters_Mobile cancelReservationParameters_Mobile)
        {
            return new ReservationCancellationParameters
            {
                cancellationDescription = StaticHelper.cancellationDescriptionForMobile,
                cancellationReason = cancelReservationParameters_Mobile.cancellationReason,
                langId = cancelReservationParameters_Mobile.langId,
                pnrNumber = cancelReservationParameters_Mobile.pnrNumber,
                reservationId = cancelReservationParameters_Mobile.reservationId
            };
        }

        public ReservationCancellationParameters buildCancelReservationParameters(CancelReservationParameters_Broker cancelReservationParameters_Broker)
        {
            return new ReservationCancellationParameters
            {
                cancellationReason = cancelReservationParameters_Broker.cancellationReason,
                langId = cancelReservationParameters_Broker.langId,
                pnrNumber = cancelReservationParameters_Broker.pnrNumber,
                reservationId = cancelReservationParameters_Broker.reservationId
            };
        }
    }
}
