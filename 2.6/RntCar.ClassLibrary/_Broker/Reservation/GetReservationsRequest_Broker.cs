using System;

namespace RntCar.ClassLibrary._Broker
{
    public class GetReservationsRequest_Broker : RequestBase
    {
        public Guid accountId { get; set; }
    }
}
