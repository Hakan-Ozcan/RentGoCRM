namespace RntCar.ClassLibrary._Tablet
{
    public class CalculateAvailabilityParameters : RequestBase
    {
        public ContractInformation contractInformation { get; set; }
        public Branch dropoffBranch { get; set; }
    }
}
