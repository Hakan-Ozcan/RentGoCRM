using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CountryData
    {
        /// <summary>
        /// Ülke adı
        /// </summary>
        public string countryName { get; set; }
        /// <summary>
        /// Ülke kodu
        /// </summary>
        public string countryCode { get; set; }
        /// <summary>
        /// Ülkeye ait benzersiz ID değeri
        /// </summary>
        public string countryId { get; set; }
        /// <summary>
        /// Ülkeye ait telefon kodu
        /// </summary>
        public string countryDialCode { get; set; }

    }
}
