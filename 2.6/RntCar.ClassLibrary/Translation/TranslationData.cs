using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.Translation
{
    public class TranslationData
    {
        public Guid TranslationDataId { get; set; }
        public Guid RegardingObjectId { get; set; }
        public string FieldName { get; set; }
        public string TranslationText { get; set; }
        public string TranslationHTMLText { get; set; }
        public int LangCode { get; set; }

    }
}
