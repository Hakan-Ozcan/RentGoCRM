using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RntCar.ClassLibrary.TransactionCurrency
{
    public class Currency
    {
        [XmlAttribute("CurrencyCode")]
        public string CurrencyCode { get; set; }
        [XmlElement("Unit")]
        public int Unit { get; set; }

        [XmlElement("Isim")]
        public string Isim { get; set; }

        [XmlElement("CurrencyName")]
        public string CurrencyName { get; set; }

        [XmlElement("ForexBuying")]
        public string ForexBuying { get; set; }

        [XmlElement("ForexSelling")]
        public string ForexSelling { get; set; }

        [XmlElement("BanknoteBuying")]
        public string BanknoteBuying { get; set; }

        [XmlElement("BanknoteSelling")]
        public string BanknoteSelling { get; set; }

        [XmlElement("CrossRateUSD")]
        public string CrossRateUSD { get; set; }

        [XmlElement("CrossRateOther")]
        public string CrossRateOther { get; set; }
    }
}
