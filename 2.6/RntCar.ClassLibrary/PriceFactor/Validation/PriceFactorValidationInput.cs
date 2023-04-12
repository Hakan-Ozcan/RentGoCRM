using System;
using System.Collections.Generic;

namespace RntCar.ClassLibrary.PriceFactor.Validation
{
    // Tolga AYKURT - 07.03.2019
    public class PriceFactorValidationInput
    {
        // Tolga AYKURT - 07.03.2019
        public DateTime BeginDate { get; set; }

        // Tolga AYKURT - 07.03.2019
        public DateTime EndDate { get; set; }

        // Tolga AYKURT - 07.03.2019
        public int PriceFactorType { get; set; }

        public List<string> type { get; set; }

        // Tolga AYKURT - 07.03.2019
        public List<ValidationAtrribute> AttributeToValidate { get; set; }

        // Tolga AYKURT - 07.03.2019
        public Guid PluginInitializerUserId { get; set; }

        // Tolga AYKURT - 14.03.2019
        public class ValidationAtrribute
        {
            public enum AttributeTypeEnum
            {
                OptionSet,
                MultiSelectOptionSet
            }

            public AttributeTypeEnum AttributeType { get; set; }

            public List<string> AttributeValues { get; set; }

            public string AttributeName { get; set; }
        }
    }
}
