using System;

namespace RntCar.ClassLibrary._Mobile
{
    public class CreateCorporateRelationRequest_Mobile : RequestBase
    {
        public string governmentId { get; set; }
        public Guid accountId { get; set; }
        public int relationType { get; set; }
        public bool? breakRelation { get; set; }
    }
}
