using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class CheckBeforeContractCreationParameters
    {
        public Guid reservationId { get; set; }
        public bool isQuickContract { get; set; }
        public DateTime? pickupDateTime { get; set; }
        public Guid contactId { get; set; }
        public int langId { get; set; }
    }
}
