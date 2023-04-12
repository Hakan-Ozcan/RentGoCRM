using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Linq;

namespace RntCar.SDK.Common
{
    public class CommonHelper
    {
        private CrmServiceClient _crmServiceClient;
        private IOrganizationService _service;

        private int day { get; set; }

        public IOrganizationService IOrganizationService
        {
            get { return _service; }
        }
        public CrmServiceClient CrmServiceClient
        {
            get { return _crmServiceClient; }
        }
        public static bool checkGovermentIdValidity(string governmentId) // goverment id valid control
        {
            Int64 ATCNO, BTCNO, TCKN;
            long C1, C2, C3, C4, C5, C6, C7, C8, C9, Q1, Q2;

            TCKN = Int64.Parse(governmentId);

            ATCNO = TCKN / 100;
            BTCNO = TCKN / 100;

            C1 = ATCNO % 10; ATCNO = ATCNO / 10;
            C2 = ATCNO % 10; ATCNO = ATCNO / 10;
            C3 = ATCNO % 10; ATCNO = ATCNO / 10;
            C4 = ATCNO % 10; ATCNO = ATCNO / 10;
            C5 = ATCNO % 10; ATCNO = ATCNO / 10;
            C6 = ATCNO % 10; ATCNO = ATCNO / 10;
            C7 = ATCNO % 10; ATCNO = ATCNO / 10;
            C8 = ATCNO % 10; ATCNO = ATCNO / 10;
            C9 = ATCNO % 10; ATCNO = ATCNO / 10;
            Q1 = ((10 - ((((C1 + C3 + C5 + C7 + C9) * 3) + (C2 + C4 + C6 + C8)) % 10)) % 10);
            Q2 = ((10 - (((((C2 + C4 + C6 + C8) + Q1) * 3) + (C1 + C3 + C5 + C7 + C9)) % 10)) % 10);

            return ((BTCNO * 100) + (Q1 * 10) + Q2 == TCKN);
        }

        public static string removeWhitespacesForTCKNValidation(string input)
        {
            var normalString = string.Empty;
            var splitted = input.Split(' ');
            for (int i = 0; i < splitted.Length; i++)
            {
                if (!string.IsNullOrEmpty(splitted[i]))
                {
                    if (i == splitted.Length - 1)
                    {
                        normalString += splitted[i].removeEmptyCharacters();
                    }
                    else
                    {
                        normalString += splitted[i].removeEmptyCharacters() + " ";
                    }
                }
            }
            normalString = normalString.TrimEnd().TrimStart();
            return normalString;
        }
        public static string buildTwelveCharacters(string number)
        {
            if (number.Length < 11)
            {
                for (int i = number.Length; i < 11; i++)
                {
                    number = "2" + number;
                }
            }
            else
            {
                number = number.Substring(0, 11);
            }

            return number;

        }
        public static string buildCharacters(string number, int lenght)
        {
            if (number.Length < lenght)
            {
                for (int i = number.Length; i < lenght; i++)
                {
                    number = "2" + number;
                }
            }
            else
            {
                number = number.Substring(0, lenght);
            }

            return number;
        }
        public static bool isDateorBranchChanged(DateTime pickupDateTime, DateTime dropoffDateTime,
                                        Guid pickupBranchId, Guid dropoffBranchId,
                                        DateTime pickupDateTimeParameter, DateTime dropoffDateTimeParameter,
                                        Guid pickupBranchIdParameter, Guid dropoffBranchIdParameter)
        {
            return (pickupDateTime != pickupDateTimeParameter ||
                                         dropoffDateTime != dropoffDateTimeParameter ||
                                         pickupBranchId != pickupBranchIdParameter ||
                                         dropoffBranchId != dropoffBranchIdParameter);
        }

        public static bool isDateChanged(DateTime pickupDateTime, DateTime dropoffDateTime,
                                DateTime pickupDateTimeParameter, DateTime dropoffDateTimeParameter)
        {
            return (pickupDateTime != pickupDateTimeParameter || dropoffDateTime != dropoffDateTimeParameter);
        }
        public static bool checkBiggerThanDay(DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            return (dropoffDateTime - pickupDateTime).TotalMinutes >= 1440 ? true : false;
        }
        public static double calculateTotalDurationInMinutes(long pickupDateTimeStamp, long dropoffDateTimeStamp)
        {
            return (dropoffDateTimeStamp.converttoDateTime() - pickupDateTimeStamp.converttoDateTime()).TotalMinutes;
        }
        public static double calculateTotalDurationInMinutes(DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            return (dropoffDateTime - pickupDateTime).TotalMinutes;
        }
        public static double calculateTotalDurationInDays(long pickupDateTimeStamp, long dropoffDateTimeStamp)
        {
            return (dropoffDateTimeStamp.converttoDateTime() - pickupDateTimeStamp.converttoDateTime()).TotalDays;
        }
        public static double calculateTotalDurationInDays(DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            return (dropoffDateTime - pickupDateTime).TotalDays;
        }
        /// <summary>
        /// returns one if day difference is less than one
        /// </summary>
        /// <param name="pickupDateTime"></param>
        /// <param name="dropoffDateTime"></param>
        /// <returns></returns>
        public static int calculateTotalDurationInDaysCheckIfzero(DateTime pickupDateTime, DateTime dropoffDateTime)
        {
            return Convert.ToInt32((dropoffDateTime - pickupDateTime).TotalDays) == 0 ? 1 : Convert.ToInt32((dropoffDateTime - pickupDateTime).TotalDays);
        }
        /// <summary>
        /// returns one if day difference is less than one
        /// </summary>
        /// <param name="pickupDateTime"></param>
        /// <param name="dropoffDateTime"></param>
        /// <returns></returns>
        public static int calculateTotalDurationInDaysCheckIfzero(long pickupDateTimeStamp, long dropoffDateTimeStamp)
        {
            return Convert.ToInt32((dropoffDateTimeStamp.converttoDateTime() - pickupDateTimeStamp.converttoDateTime()).TotalDays) == 0 ?
                   1 :
                   Convert.ToInt32((dropoffDateTimeStamp.converttoDateTime() - pickupDateTimeStamp.converttoDateTime()).TotalDays);
        }
        public static DateTime addOffsetToGivenDateTimeInMinutes(DateTime time)
        {
            return time.AddMinutes(StaticHelper.offset);
        }
        public static string generateRandomStringByGivenNumber(int length)
        {
            var str = string.Empty;
            for (int i = 0; i < length - 1; i++)
            {
                str += StaticHelper.GenerateString(1);
            }
            return str;
        }

        public static decimal calculateAdditionalProductPrice(int priceCalculationType,
                                                              decimal monthlyPrice,
                                                              decimal dailyPrice,
                                                              int totalDuration)
        {
            var willMultiplyWithTotalDuration = true;
            if (priceCalculationType == 2)
            {
                willMultiplyWithTotalDuration = false;
            }

            decimal actualTotalAmount = decimal.Zero;
            if (willMultiplyWithTotalDuration)
            {
                var quotient = Convert.ToInt32(totalDuration / 30);
                var remainder = totalDuration % 30;
                actualTotalAmount = (quotient * monthlyPrice) + (remainder * dailyPrice > monthlyPrice ?
                                                                 monthlyPrice :
                                                                 remainder * dailyPrice);
            }
            else
            {
                actualTotalAmount = dailyPrice;
            }
            return actualTotalAmount;
        }

        public static string formatCreditCardNumber(string creditCardNumber)
        {
            var cardWithoutEmptyStrings = creditCardNumber.removeEmptyCharacters();
            var formatted = cardWithoutEmptyStrings.Substring(0, 6);
            formatted += "******";
            formatted += cardWithoutEmptyStrings.Substring(cardWithoutEmptyStrings.Length - 4, 4);
            return formatted;
        }

        public static OptionSetValue decideBillingType(int itemType, string pricingType, int? billingType = null)
        {
            int paymentMethod = int.MinValue;
            //means individual
            if (!int.TryParse(pricingType, out paymentMethod))
            {
                return new OptionSetValue((int)rnt_BillingTypeCode.Individual);
            }

            if (paymentMethod == (int)rnt_PaymentMethodCode.Current ||
                paymentMethod == (int)rnt_PaymentMethodCode.FullCredit)
            {
                return new OptionSetValue((int)rnt_BillingTypeCode.Corporate);
            }

            if (paymentMethod == (int)rnt_PaymentMethodCode.LimitedCredit)
            {
                if (billingType == (int)rnt_BillingTypeCode.Corporate)
                {
                    return new OptionSetValue((int)rnt_BillingTypeCode.Corporate);
                }

                return new OptionSetValue((int)rnt_BillingTypeCode.Individual);
            }

            if (itemType != (int)rnt_reservationitem_rnt_itemtypecode.Equipment)
            {
                return new OptionSetValue((int)rnt_BillingTypeCode.Individual);
            }

            if (paymentMethod == (int)rnt_PaymentMethodCode.PayBroker)
            {
                paymentMethod = (int)rnt_BillingTypeCode.Corporate;
            }
            else
            {
                paymentMethod = (int)rnt_BillingTypeCode.Individual;
            }
            return new OptionSetValue(paymentMethod);
        }

        public static string couponCodeGenerator(int length)
        {
            var alphaNumSeed = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            var coupon = alphaNumSeed.ToArray().OrderBy(o => Guid.NewGuid()).Take(length);

            return new string(coupon.ToArray());
        }
    }
}
