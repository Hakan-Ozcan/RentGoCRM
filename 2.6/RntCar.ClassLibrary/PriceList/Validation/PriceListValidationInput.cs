using System;

namespace RntCar.ClassLibrary.PriceList.Validation
{
    // Tolga AYKURT - 07.03.2019
    public class PriceListValidationInput
    {
        // Tolga AYKURT - 07.03.2019
        public DateTime BeginDate { get; set; }

        // Tolga AYKURT - 07.03.2019
        public DateTime EndDate { get; set; }

        // Tolga AYKURT - 07.03.2019
        public int PriceListTypeCode { get; set; }

        public Guid PriceCodeId { get; set; }

        // Tolga AYKURT - 07.03.2019
        public Guid PluginInitializerUserId { get; set; }
    }
}
