using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class SendSmsVericationIndividualCustomerUpdateRequest_Mobile : RequestBase
    {
        public Guid individualCustomerId { get; set; }
        public string mobilePhone { get; set; }
        public string verificationCode { get; set; }
    }
}
