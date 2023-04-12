using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class ExtraField
    {
        public string extraFieldType { get; set; }
        public string extraFieldKey { get; set; }
        public string extraFieldValue { get; set; }
    }

    public class ExtraFields
    {
        public List<ExtraField> extraFields { get; set; }
    }
}
