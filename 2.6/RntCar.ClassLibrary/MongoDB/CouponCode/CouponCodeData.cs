using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class CouponCodeData
    {
        public string mongoId { get; set; }

        public string couponCodeId { get; set; }

        public string couponCodeDefinitionId { get; set; }

        public string reservationId { get; set; }

        public string contractId { get; set; }

        public string contactId { get; set; }

        public string accountId { get; set; }

        public string couponCode { get; set; }

        public int statusCode { get; set; }
    }
}
