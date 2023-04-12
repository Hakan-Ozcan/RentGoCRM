using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace RntCar.SDK.Common
{

    public static class StaticHelper
    {
        public static readonly int offset = 180;
        public static readonly int defaultVirtualPosId = 4;
        public static readonly decimal _one_ = 1;
        public static readonly decimal hundred = 100M;
        public static readonly decimal dayDurationInMinutes = 1440m;
        public static readonly int vendorId = 8;
        public static readonly int retryCount = 2;
        public static readonly int retrySleep = 1000;
        public const string endLineStar = "***********************************************************";
        public const string defaultEmail = "pay@rentgo.com";
        public const string mongoDBNotCreatedYet = "Record hasnt been created in mongodb yet";
        public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string tlSymbol = "₺";
        public const string Number = "123456789";
        public const string _350 = "350";
        public const string _others = "Diğerleri";
        public const string paymentErrorPrefix = "CustomPaymentError: ";
        public const string paymentNotFoundReplacement = "Şu anda ödeme işleminizi gerçekleştiremiyoruz.Kredi kartının online işlemlere açık olduğundan emin olunuz.";
        public const string cancellationDescriptionForMobile = "Reservation cancelled by customer from mobile!"; 
        public static Guid createInvoiceWithLogoforContractWorkflowId = new Guid("AC45A087-6A39-41C3-B794-0E60537A164C");
        public static Guid createInvoiceWithLogoWorkflowId = new Guid("1C8E21D1-DEA0-4C59-BF76-8D386EBEA9D6");
        public static Guid createCreditCardSlipWithLogoWorkflowId = new Guid("5528C1C3-90AF-4368-AA27-77CD6513B482");
        public static Guid CalculateDebitAmount_ContractWorkflowId = new Guid("D7A63642-AB33-4748-BEB6-5BADCB6677BB");
        public static Guid sendEquipmentMaintenanceMailWorkflowId = new Guid("D8CFDE4F-934D-4B12-B903-5F7D6F73E578");
        public static Guid sendEMailWhenInvoiceInformationsChangeWorkflowId = new Guid("AA3AD555-7505-4F0D-9118-4E2E146642DD");
        public static string ignoreCanceAdditionalProdcutIds = "SRV027;";
        public static Guid dummyCampaignId = new Guid("{eb56df70-82f1-4876-9285-de68ea0d9f28}");
        public static Guid turkeyCountryId = new Guid("c2e4665c-9aaf-e811-9410-000c293d97f8");

        private static readonly Random random = new Random();


        private static readonly object Locker = new object();
        private static readonly object LockerAutoNumber = new object();
        public static DateTime GetLastDayOfMonth(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, DateTime.DaysInMonth(dateTime.Year, dateTime.Month));
        }
        public static string completeZeroCharacterByGivenLength(decimal input, int length)
        {
            var str = input.ToString().Replace(",", ".");
            var remaining = length - str.Length;

            for (int i = 0; i < remaining; i++)
            {
                str = "0" + str;
            }
            return str;
        }
        public static string completeEmptyCharacterByGivenLength(string input, int length)
        {
            var remaining = length - input.Length;
            for (int i = 0; i < remaining; i++)
            {
                input = input + " ";
            }
            return input;
        }
        public static string upperCaseTurkish(this string input)
        {
            return input.ToUpper(new CultureInfo("tr-TR", false));
        }
        public static T Map<T, TU>(this T target, TU source)
        {
            // get property list of the target object.
            // this is a reflection extension which simply gets properties (CanWrite = true).
            var tprops = target.GetType().GetProperties();

            tprops.ToList().ForEach(prop =>
            {
                // check whether source object has the the property
                var sp = source.GetType().GetProperty(prop.Name);
                if (sp != null)
                {
                    // if yes, copy the value to the matching property
                    var value = sp.GetValue(source, null);
                    target.GetType().GetProperty(prop.Name).SetValue(target, value, null);
                }
            });

            return target;
        }
        public static byte[] convertGuidToMongoDBId(this Guid id)
        {
            return id.ToByteArray().Take(12).ToArray();
        }
        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i)).ToList();
            }
        }
        public static void ShallowCopy(this Object dst, object src)
        {
            var srcT = src.GetType();
            var dstT = dst.GetType();

            foreach (var f in srcT.GetFields())
            {
                var dstF = dstT.GetField(f.Name);
                if (dstF == null)
                    continue;
                dstF.SetValue(dst, f.GetValue(src));
            }

            foreach (var f in srcT.GetProperties())
            {
                var dstF = dstT.GetProperty(f.Name);
                if (dstF == null)
                    continue;
                dstF.SetValue(dst, f.GetValue(src, null), null);
            }
        }
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items,
                    Func<T, TKey> keyer)
        {
            var set = new HashSet<TKey>();
            var list = new List<T>();
            foreach (var item in items)
            {
                var key = keyer(item);
                if (set.Contains(key))
                    continue;
                list.Add(item);
                set.Add(key);
            }
            return list;
        }
        public static string GetConfiguration(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static void HandleExceptions<E>(string message) where E : Exception
        {
            throw Activator.CreateInstance(typeof(E), message) as E;
        }
        public static String Serialize(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(obj.GetType());
                ser.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }
        public static T deserializeJSON<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static string RandomDigits(int length)
        {
            String s = String.Empty;
            lock (Locker)
            {
                for (int i = 0; i < length; i++)
                    s = String.Concat(s, random.Next(10).ToString());
            }
            return s;
        }

        public static string GenerateString(int size)
        {
            char[] chars = new char[size];
            lock (Locker)
            {
                for (int i = 0; i < size; i++)
                {
                    chars[i] = Alphabet[random.Next(Alphabet.Length)];
                }
            }
            return new string(chars);
        }

        public static bool isBetween(this long input, long start, long end)
        {
            return (input >= start && input <= end);
        }
        public static bool isBetweenNotEqualEnd(this DateTime input, DateTime start, DateTime end)
        {
            return (input >= start && input < end);
        }
        public static bool isBetween(this DateTime input, DateTime start, DateTime end)
        {
            return (input >= start && input <= end);
        }

        public static int intToDate(this int input)
        {
            return input * 1440;
        }

        public static bool IsBetween<T>(this T item, T start, T end)
        {
            return Comparer<T>.Default.Compare(item, start) >= 0
                && Comparer<T>.Default.Compare(item, end) <= 0;
        }

        public static int converttoTimeStamp(this DateTime givenTime)
        {
            return (Int32)(givenTime.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
        public static DateTime converttoDateTime(this long timestamp)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return dateTime.AddSeconds(timestamp);
        }
        public static int converttoHours(this int minute)
        {
            return minute / 60;
        }
        // Tolga AYKURT - 27.02.2019
        /// <summary>
        /// İki tarih arasında kalan tüm tarihleri bir liste olarak verir.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate">Bitiş tarihi</param>
        /// <returns></returns>
        public static List<DateTime> GetBetweenDates(this DateTime startDate, DateTime endDate)
        {
            var calculationDates = new List<DateTime>();

            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
            {
                calculationDates.Add(date);
            }

            return calculationDates;
        }

        public static int CalculateAge(this DateTime birthDate)
        {
            //var formattedToday = (DateTime.UtcNow.Year * 100 + DateTime.UtcNow.Month) * 100 + DateTime.UtcNow.Day;
            //var formattedBirthDate = (birthDate.Year * 100 + birthDate.Month) * 100 + birthDate.Day;

            //return (formattedToday - formattedBirthDate) / 1000;
            return DateTime.UtcNow.Year - birthDate.Year;
        }
        public static string removeAlphaNumericCharactersFromString(this string str)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");

            return rgx.Replace(str, "");
        }
        public static string replaceTurkishCharacters(this string str)
        {
            return str
                   .Replace("ı", "i")
                   .Replace("ş", "s")
                   .Replace("ç", "c")
                   .Replace("ğ", "g")
                   .Replace("İ", "I")
                   .Replace("Ş", "S")
                   .Replace("Ç", "C")
                   .Replace("Ğ", "G")
                   .Replace("I", "i");
        }
        public static string removeEmptyCharacters(this string str)
        {
            return Regex.Replace(str, @"\s+", "");
        }
        public static string insertSpaceBetweenWords(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }

            return newText.ToString();
        }
        public static string prepareAdditionalProductCacheKey(string productCode, string cacheKey)
        {
            return cacheKey + "_" + productCode;
        }
        public static bool isMultiple(int x, int y)
        {
            return x % y == 0;
        }
    }
}
