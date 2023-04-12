using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RntCar.SDK.Common
{
    public class XmlSerializerHelper
    {
        public T Deserialize<T>(string input, string xmlRootName) where T : class
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T), new XmlRootAttribute(xmlRootName));

            using (StringReader sr = new StringReader(Encoding.GetEncoding("Windows-1254").GetString(Encoding.UTF8.GetBytes(input))))
            {
                return (T)ser.Deserialize(sr);
            }
        }
        public T Deserialize<T>(byte[] xmlByte, string xmlRootName)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T), new XmlRootAttribute(xmlRootName));

            using (StringReader sr = new StringReader(Encoding.GetEncoding("Windows-1254").GetString(xmlByte)))
            {
                return (T)ser.Deserialize(sr);
            }
        }
        public string Serialize<T>(T ObjectToSerialize)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, ObjectToSerialize);
                return textWriter.ToString();
            }
        }
    }
}
