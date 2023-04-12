using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary._Mobile
{
    public class CouponCodeOperationsRequest_Mobile : RequestBase
    {
        public string couponCode { get; set; }

        public string reservationId { get; set; }

        public string contractId { get; set; }

        public string accountId { get; set; }

        public string contactId { get; set; }

        public int? statusCode { get; set; }

        public Guid? groupCodeInformationId { get; set; }

        public Guid pickupBranchId { get; set; }

        public int reservationChannelCode { get; set; }

        public int reservationTypeCode { get; set; }
    }
}
