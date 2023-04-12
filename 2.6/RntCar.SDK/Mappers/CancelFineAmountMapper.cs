using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Mappers
{
    public class CancelFineAmountMapper
    {
        public ReservationCancellationParameters buildCancelFineAmountParameters(CalculateCancelFineAmountParameters_Web calculateCancelFineAmountParameters_Web)
        {
            return new ReservationCancellationParameters
            {
                reservationId = calculateCancelFineAmountParameters_Web.reservationId,
                pnrNumber = calculateCancelFineAmountParameters_Web.pnrNumber,
                langId = calculateCancelFineAmountParameters_Web.langId
            };
        }

        public ReservationCancellationParameters buildCancelFineAmountParameters(CalculateCancelFineAmountParameters_Mobile calculateCancelFineAmountParameters_Mobile)
        {
            return new ReservationCancellationParameters
            {
                reservationId = calculateCancelFineAmountParameters_Mobile.reservationId,
                pnrNumber = calculateCancelFineAmountParameters_Mobile.pnrNumber,
                langId = calculateCancelFineAmountParameters_Mobile.langId
            };
        }
    }
}
