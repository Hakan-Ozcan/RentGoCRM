namespace RntCar.ClassLibrary._Mobile
{
    public class SendSmsVericationIndividualCustomerCreationRequest_Mobile : RequestBase
    {
        public string mobilePhone { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string verificationCode { get; set; }
    }
}
