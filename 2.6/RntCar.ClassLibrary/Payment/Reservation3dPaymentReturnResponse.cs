using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class Reservation3dPaymentReturnResponse
    {
        public string clientReferenceCode { get; set; }
        public string paymentId { get; set; }
        public string paymentTransactionId { get; set; }
        public bool status { get; set; }
        public decimal providerComission { get; set; } = 0;
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string errorGroup { get; set; } 
        public bool? creditCardSaveSafely { get; set; } 
    }
} 
