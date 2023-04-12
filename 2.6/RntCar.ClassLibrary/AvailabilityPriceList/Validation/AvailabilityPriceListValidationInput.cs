using System;

namespace RntCar.ClassLibrary
{
    // Tolga AYKURT - 11.03.2019
    public class AvailabilityPriceListValidationInput
    {
        // Tolga AYKURT - 11.03.2019
        public Guid PriceListId { get; set; }

        // Tolga AYKURT - 11.03.2019
        public int MaximumAvailability { get; set; }

        // Tolga AYKURT - 11.03.2019
        public int MinimumAvailability { get; set; }

        public Guid InitiatingUserId { get; set; }
        public Guid groupCodeId { get; set; }
    }
}
