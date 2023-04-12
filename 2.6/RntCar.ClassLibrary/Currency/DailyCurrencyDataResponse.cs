using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RntCar.ClassLibrary
{
    [XmlRoot("Tarih_Date")]
    public class DailyCurrencyDataResponse
    {
        [XmlElement("Currency")]
        public List<Step> Steps { get; set; }


        //public class Curreny
        //{
        //    public string Unit { get; set; }
        //    public string Isim { get; set; }
        //    public string CurrencyName { get; set; }
        //    public string ForexBuying { get; set; }
        //    public string ForexSelling { get; set; }
        //    public string BanknoteBuying { get; set; }
        //    public string BanknoteSelling { get; set; }
        //    public string CrossRateUSD { get; set; }
        //    public string CrossRateOther { get; set; }
        //}
    }

    public class Step
    {
        [XmlElement("Unit")]
        public string Unit { get; set; }
        [XmlElement("Isim")]
        public string Isim { get; set; }
    }

}
