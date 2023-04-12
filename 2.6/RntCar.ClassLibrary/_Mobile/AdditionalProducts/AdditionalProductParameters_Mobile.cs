using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class AdditionalProductParameters_Mobile : RequestBase
    {
        public QueryParameters queryParameters { get; set; }
        public Guid? individualCustomerId { get; set; }
        public Guid? groupCodeId { get; set; }
    }
}
