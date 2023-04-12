namespace RntCar.ClassLibrary
{
    public class RetrieveVirtualPosIdParameters : AuthInfo
    {
        public string cardBin { get; set; }
        public int? provider { get; set; }
    }
}
