using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Web
{
    public class CreateCorporateRelationRequest_Web : RequestBase
    {
        public string governmentId { get; set; }
        public Guid accountId { get; set; }
        public int relationType { get; set; }
        public bool? breakRelation { get; set; }
    }
}
