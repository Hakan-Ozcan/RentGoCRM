namespace RntCar.ClassLibrary
{
    public class RetrieveInstallmentParameters : AuthInfo
    {       
        public string cardBin { get; set; }
        public decimal amount { get; set; }
        public int? provider { get; set; }
    }
}
