using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RntCar.ClassLibrary
{
    public class EIhlalHighwayResponse
    {
        [XmlElement("arac_list")]
        public List<EIhlalHighwayFineData> highwayFineData { get; set; }
        [XmlElement("sistem")]
        public bool result { get; set; }
    }
}
