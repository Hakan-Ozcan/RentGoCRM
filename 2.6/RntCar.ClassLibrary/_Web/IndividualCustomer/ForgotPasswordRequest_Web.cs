using System;

namespace RntCar.ClassLibrary._Web
{
    public class ForgotPasswordRequest_Web : RequestBase
    {
        public string emailAddress { get; set; }
        public string newPassword { get; set; }
        public string newPasswordAgain { get; set; }
    }
}
