using System;

namespace RntCar.ClassLibrary._Web
{
    public class AdditionalProductParameters_Web : RequestBase
    {
        public QueryParameters queryParameters { get; set; }
        public Guid? individualCustomerId { get; set; }
        public Guid? groupCodeId { get; set; }
    }
}
