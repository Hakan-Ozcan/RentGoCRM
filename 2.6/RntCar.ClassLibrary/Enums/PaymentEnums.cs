using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class PaymentEnums
    {
        public enum CreditCardType
        {
            VISA = 20,
            MASTER_CARD = 10,
            AMERICAN_EXPRESS = 30,
            TROY = 40,
            UNKNOWN = 40,
        }
        public enum CreditCardOrganizationType
        {
            CREDIT_CARD = 1,
            DEBIT_CARD = 2,
            PREPAID_CARD = 4

        }
        public enum CreditCardStatus
        {
            success = 10,
            failure = 20
        }
        public enum IyzicoResponse
        {
            success = 1,
            failure = 2
        }

        public enum CreditCardFamily
        {
            Axess = 1,
            Bonus = 2,
            World = 2,
            Maximum = 4,
            Paraf = 5,
            CardFinans = 6,
            Unknown = 7,
            Miles_Smiles = 8
        }
        public enum PaymentChannelCode
        {
            BRANCH = 10,
            MOBILE = 20,
            MOBILE_NOTUSE = 30,
            TABLET = 40,
            WEB = 50
        }
        public enum PaymentTransactionType
        {
            SALE = 10,
            REFUND = 20,
            DEPOSIT = 30
        }
        public enum PaymentTransactionResult
        {
            Draft = 3,
            Success = 1,
            Error = 2,
            Success_WithoutPaymentTransactions = 4,
            WaitingFor3D = 5,
        }
        public enum PaymentType
        {
            PayNow = 10,
            PayLater = 20
        };
        public enum PaymentMethodType
        {
            CURRENT = 10,
            CREDITCARD = 20,
            LIMITEDCREDIT = 30,
            FULLCREDIT = 40,
            PAYBROKER = 50,
            PAYOFFICE = 60
        }
        public enum RefundStatus
        {
            NoRefund = 1,
            Partially_Refund = 2,
            Totally_Refund = 3
        };
    }
}
