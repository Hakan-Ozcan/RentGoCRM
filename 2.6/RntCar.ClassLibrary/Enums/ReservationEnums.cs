using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ReservationEnums
    {
        public enum ReservationPaymentType
        {
            INDIVIDUAL = 5,
            CURRENT = 10,
            CORPORATE = 15,
            CREDITCARD = 20,
            LIMITEDCREDIT = 30,
            FULLCREDIT = 40,
            PAYBROKER = 50,
            PAYOFFICE = 60
        }

        public enum CancellationReason
        {
            ByRentgo  = 100000007,
            ByCustomer = 100000006
        }
        public enum StatusCode
        {
            New = 1,
            NoShow = 100000000,
            Rental  = 100000001,
            WaitingforDelivery  = 100000002,
            Completed = 100000003,
            Waitingfor3D = 100000008
        }
        public enum ReservationPaymentChoise
        {
            PayNow = 10,
            PayLater = 20
        }
    }
}
