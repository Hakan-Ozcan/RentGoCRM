using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.ClassLibrary
{
    public class GetBirdUserInfoResponse : ResponseBase
    {
        public long birdId { get; set; }
        public string customerId { get; set; }
        public decimal coinBalance { get; set; }
        public decimal availableCoinBalance { get; set; }
        public string provisionTokens { get; set; }
        public decimal minBillAmount { get; set; }
        public decimal walletBalance { get; set; }
        public JoinedCampaigns joinedCampaigns { get; set; }
        public UsableCoinInfo usableCoinInfo { get; set; }
    }
    public class UsableCoinInfo
    {
        public BalanceInfo balanceInfo { get; set; }

    }

    public class BalanceInfo
    {
        public decimal amount { get; set; }
        public string coinVariant { get; set; }
        public long campaignId { get; set; }
    }

    public class JoinedCampaigns
    {
        public string code { get; set; }
        public string type { get; set; }
        public double multiplier { get; set; }
        public decimal limit { get; set; }

    }
}
