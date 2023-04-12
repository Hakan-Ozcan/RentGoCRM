using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RntCar.ClassLibrary
{
    public class EIhlalTrafficFineData
    {
        [XmlElement("arac")]
        public string equipment { get; set; }
        [XmlElement("tutanak_seri")]
        public string reportInfo { get; set; }
        [XmlElement("tutanak_sira_no")]
        public string reportNo { get; set; }
        [XmlElement("ceza_maddesi")]
        public string fineClause { get; set; }
        [XmlElement("ceza_tutari")]
        public string fineAmount { get; set; }
        [XmlElement("kesildigi_yer")]
        public string finePlace { get; set; }
        [XmlElement("ceza_tarihi")]
        public string fineDate { get; set; }
        [XmlElement("ceza_saat")]
        public string fineTime { get; set; }
        [XmlElement("duzenleyen_birim")]
        public string organizingUnit { get; set; }
        [XmlElement("il_ilce")]
        public string addressInfo { get; set; }
        [XmlElement("teblig_tarihi")]
        public string communiqueDate { get; set; }
    }
}
