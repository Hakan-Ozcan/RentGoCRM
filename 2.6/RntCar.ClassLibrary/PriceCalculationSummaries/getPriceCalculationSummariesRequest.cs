namespace RntCar.ClassLibrary
{
    public class GetPriceCalculationSummariesRequest
    {
        public string groupCodeInformationId { get; set; }
        public string trackingNumber { get; set; }
        public string campaignId { get; set; }
        public string pickupBranchId { get; set; }
        public string documentId { get; set; }
    }
}
