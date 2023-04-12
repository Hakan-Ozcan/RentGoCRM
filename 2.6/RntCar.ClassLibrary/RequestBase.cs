using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class RequestBase
    {
        /// <summary>
        /// Dil kodu. İngilizce: 1033, Türkçe: 1055
        /// </summary>
        public int langId { get; set; }
        public int channelCode { get; set; }
    }
}
