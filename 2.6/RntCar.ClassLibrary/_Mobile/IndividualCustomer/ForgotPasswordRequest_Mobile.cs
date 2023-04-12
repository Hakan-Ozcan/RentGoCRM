using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class ForgotPasswordRequest_Mobile : RequestBase
    {
        public string emailAddress { get; set; }
        public string newPassword { get; set; }
        public string newPasswordAgain { get; set; }
    }
}
