using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class BinResponse
    {
        public string externalId { get; set; }
        public string conversationId { get; set; }
        public string binNumber { get; set; }
        public string bankName { get; set; }
        public string cardType { get; set; }
        public string cardFamily { get; set; }
        public string cardAssociation { get; set; }
        public long? bankCode { get; set; }
        public string errorCode { get; set; }
        public string errorGroup { get; set; }
        public string errorMessage { get; set; }
        public string status { get; set; }
    }
}
