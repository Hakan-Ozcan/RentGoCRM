using System;

namespace RntCar.ClassLibrary.Campaign.Pegasus
{
    public class CheckPegasusMembershipRequest
    {
        public Guid customerId { get; set; }
        public string authBaseUrl { get; set; }
        public string bolbolurl { get; set; }
        public string PegasusAuthValues { get; set; }
    }
}
