using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Mappers
{
    public class PaymentMapper
    {
        public static rnt_PaymentChannelCode mapDocumentChanneltoPaymentChannel(int documentChannel)
        {
            var paymentChannel = rnt_PaymentChannelCode.BRANCH;
            switch (documentChannel)
            {
                case (int)rnt_ReservationChannel.Web:
                    {
                        paymentChannel = rnt_PaymentChannelCode.WEB;
                        break;
                    }

                case (int)rnt_ReservationChannel.CallCenter:
                    {
                        paymentChannel = rnt_PaymentChannelCode.CALLCENTER;
                        break;
                    }
                case (int)rnt_ReservationChannel.Branch:
                    {
                        paymentChannel = rnt_PaymentChannelCode.BRANCH;
                        break;
                    }
                case (int)rnt_ReservationChannel.Tablet:
                    {
                        paymentChannel = rnt_PaymentChannelCode.TABLET;
                        break;
                    }
                case (int)rnt_ReservationChannel.Mobile:
                    {
                        paymentChannel = rnt_PaymentChannelCode.MOBILE;
                        break;
                    }
            }
            return paymentChannel;
        }
    }
}
