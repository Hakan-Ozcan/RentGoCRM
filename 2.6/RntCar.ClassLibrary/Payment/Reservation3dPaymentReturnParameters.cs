using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.Payment
{
    public class Reservation3dPaymentReturnParameters
    {
        public Guid? reservationId { get; set; }
        public Guid? contractId { get; set; }
        public Guid? paymentId { get; set; }
        public string providerPaymentId { get; set; }
        public string conversationId { get; set; }
        public string conversationData { get; set; }
        public int success { get; set; }
        public string detail { get; set; }
    }
}
