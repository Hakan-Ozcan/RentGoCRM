using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RntCar.SDK.Common
{
    public class CrmObjectSerializer
    {
        /// <summary>
        /// The xml serialiser instance.
        /// </summary>
        private readonly DataContractSerializer dataContractSerialiser;

        /// <summary>
        /// Initializes a new instance of the <see cref="SerialiserService.Serialiser"/> class.
        /// </summary>
        /// <param name="typeToSerilaise">The type to serilaise.</param>
        public CrmObjectSerializer(Type typeToSerilaise)
        {
            this.dataContractSerialiser = new DataContractSerializer(typeToSerilaise);
        }

        /// <summary>
        /// Serialises the specified candidate.
        /// </summary>
        /// <param name="candidate">The candidate.</param>
        /// <returns>A serialised representaiton of the specified candidate.</returns>
        public byte[] Serialise(object candidate)
        {
            byte[] output;

            using (var ms = new MemoryStream())
            {

                this.dataContractSerialiser.WriteObject(ms, candidate);
                var numberOfBytes = ms.Length;

                output = new byte[numberOfBytes];

                // Note: Only copy the exact stream length to avoid capturing trailing null bytes.
                Array.Copy(ms.GetBuffer(), output, numberOfBytes);
            }

            return output;
        }

        /// <summary>
        /// Deserialises the specified serialised instance.
        /// </summary>
        /// <param name="serialisedInstance">The serialised instance.</param>
        /// <returns>A deserialised instance of the specified type.</returns>
        public object Deserialise(byte[] serialisedInstance)
        {
            object output;

            using (var ms = new MemoryStream(serialisedInstance))
            using (var reader = XmlDictionaryReader.CreateTextReader(ms, new XmlDictionaryReaderQuotas()))
            {
                output = this.dataContractSerialiser.ReadObject(reader);
            }

            return output;
        }
    }
}
