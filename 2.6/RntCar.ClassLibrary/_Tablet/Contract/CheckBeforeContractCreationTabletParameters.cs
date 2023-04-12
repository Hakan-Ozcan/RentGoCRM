using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Tablet
{
    public class CheckBeforeContractCreationTabletParameters
    {
        public Guid reservationId { get; set; }
        public bool isQuickContract { get; set; }
        public long pickupDateTimeStamp { get; set; }
        public Guid contactId { get; set; }
        public int langId { get; set; }
    }
}
