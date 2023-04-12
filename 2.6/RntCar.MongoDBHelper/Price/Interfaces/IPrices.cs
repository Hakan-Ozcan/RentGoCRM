using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Price.Interfaces
{
    public interface IPrices
    {
        decimal basePrice { get; set; }
        decimal priceAfterAvailabilityFactor { get; set; }
        decimal priceAfterChannelFactor { get; set; }
        decimal priceAfterWeekDaysFactor { get; set; }
        decimal priceAfterSpecialDaysFactor { get; set; }
        decimal priceAfterBranchFactor { get; set; }
        decimal priceAfterBranch2Factor { get; set; }
        decimal priceAfterCustomerFactor { get; set; }
        decimal priceAfterEqualityFactor { get; set; }

        decimal totalPrice { get; set; }
        decimal payNowPrice { get; set; }
        decimal payLaterPrice { get; set; }
        Guid priceListId { get; set; }
        string priceListIdName { get; set; }
        DateTime priceDate { get; set; }
        long priceDateTimeStamp { get; set; }
    }
}
