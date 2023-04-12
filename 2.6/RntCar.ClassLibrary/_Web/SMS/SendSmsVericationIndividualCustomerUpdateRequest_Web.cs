using System;

namespace RntCar.ClassLibrary._Web
{
    public class SendSmsVericationIndividualCustomerUpdateRequest_Web : RequestBase
    {
        public Guid individualCustomerId { get; set; }
        public string mobilePhone { get; set; }
        public string verificationCode { get; set; }
    }
}
