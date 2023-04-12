namespace RntCar.ClassLibrary._Web
{
    public class SendSmsVericationIndividualCustomerCreationRequest_Web : RequestBase
    {
        public string mobilePhone { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string verificationCode { get; set; }
        public string nationalityId { get; set; }
        public string email { get; set; }
    }
}
