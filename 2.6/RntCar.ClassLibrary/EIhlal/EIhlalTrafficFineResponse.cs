using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RntCar.ClassLibrary
{
    public class EIhlalTrafficFineResponse
    {
        [XmlElement("arac_list")]
        public List<EIhlalTrafficFineData> trafficFineData { get; set; }
        [XmlElement("sistem")]
        public bool result { get; set; }
    }
}
