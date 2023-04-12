namespace RntCar.ClassLibrary._Tablet
{
    public class DashboardContractData
    {
        public string contractId { get; set; }
        public string contractNumber { get; set; }
        public string pnrNumber { get; set; }
        public long pickupTimestamp { get; set; }
        public long dropoffTimestamp { get; set; }
        public int statusCode { get; set; }
        public int paymentMethodCode { get; set; }
        public string statusName { get; set; }    
        public DashboardCustomer customer { get; set; }
        public DashboardGroupCodeInformation groupCodeInformation { get; set; }
      
    }
}
