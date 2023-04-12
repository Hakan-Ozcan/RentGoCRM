namespace RntCar.ClassLibrary.Campaign.Pegasus
{
    public class PegasusMembershipRequest
    {
        public MemberDataConfirmationType memberDataConfirmationType { get; set; }
    }
    public class MemberContactNumberType
    {
        public string phoneNumber { get; set; }
    }

    public class MemberDataConfirmationType
    {
        public MemberContactNumberType memberContactNumberType { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
    }
}
