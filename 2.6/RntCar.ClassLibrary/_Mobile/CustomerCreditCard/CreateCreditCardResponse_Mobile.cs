using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class CreateCreditCardResponse_Mobile:ResponseBase
    {
        public string externalId { get; set; }
        public string conversationId { get; set; }
        public string cardUserKey { get; set; }
        public string cardToken { get; set; }
        public string binNumber { get; set; }
        public string bankName { get; set; }
        public string cardType { get; set; }
        public string cardFamily { get; set; }
        public string cardAssociation { get; set; }
        public long? bankCode { get; set; }
        public int isHidden { get; set; }

        public string errorCode { get; set; }
        public string errorGroup { get; set; }
        public string errorMessage { get; set; }
        public string emailAddress { get; set; }
        public Guid creditCardId { get; set; }
    }
}
