using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary.MongoDB
{
    public class CouponCodeDefinitionData
    {
        public string couponCodeDefinitionId { get; set; }

        public string branchCodes { get; set; }

        public string channelCodes { get; set; }

        public string groupCodeInformations { get; set; }

        public bool isUnique { get; set; }

        public string name { get; set; }

        public DateTime startDate { get; set; }

        public DateTime endDate { get; set; }
        
        public DateTime validityStartDate { get; set; }

        public DateTime validityEndDate { get; set; }
        
        public int numberOfCoupons { get; set; }

        public decimal paynowdiscountvalue { get; set; }

        public decimal paylaterdiscountvalue { get; set; }

        public int stateCode { get; set; }

        public int statusCode { get; set; }

        public DateTime createdon { get; set; }

        public DateTime modifiedon { get; set; }

        public bool enterManuelCouponCode { get; set; }

        public string couponCode { get; set; }
    }
}
