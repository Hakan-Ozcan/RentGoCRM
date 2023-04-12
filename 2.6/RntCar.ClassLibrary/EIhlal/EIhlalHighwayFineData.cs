using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RntCar.ClassLibrary
{
    public class EIhlalHighwayFineData
    {
        [XmlElement("plaka")]
        public string plateNumber { get; set; }
        [XmlElement("ihlalno")]
        public string reportNo { get; set; }
        [XmlElement("aracsinif")]
        public string equipmentClassNo { get; set; }
        [XmlElement("giris_istasyon")]
        public string entryStation { get; set; }
        [XmlElement("cikis_istasyon")]
        public string exitStation { get; set; }
        [XmlElement("giris_zaman")]
        public string entryDate { get; set; }
        [XmlElement("cikis_zaman")]
        public string exitDate { get; set; }
        [XmlElement("cezasiz_son_odeme")]
        public string dueDate { get; set; }
        [XmlElement("normalgecisucreti")]
        public string defaultAmount { get; set; }
        [XmlElement("cezatutari")]
        public string fineAmount { get; set; }
        [XmlElement("kurum")]
        public string corporation { get; set; }
        [XmlElement("silindi")]
        public string deleted { get; set; }
    }
}
