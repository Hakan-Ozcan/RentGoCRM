using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Price.Interfaces
{
    public interface IPriceCalculator
    {
        IPrices calculate(decimal price);
    }
}
