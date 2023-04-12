using System;

namespace RntCar.ClassLibrary._Web.Contract
{
    public class GetLastRentalInformationResponse : ResponseBase
    {
        public DateTime? pickupDateTime { get; set; }
        public DateTime? dropffDateTime { get; set; }
        public Guid? contractId { get; set; }
    }
}
