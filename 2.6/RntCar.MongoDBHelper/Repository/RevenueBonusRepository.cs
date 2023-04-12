using MongoDB.Driver;
using RntCar.MongoDBHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Repository
{
    public class RevenueBonusRepository : MongoDBInstance
    {
        public RevenueBonusRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public List<PositionalBonusCalculationDataMongoDB> getRevenueBonusCalculationsByDate(DateTime startDate, DateTime endDate)
        {
            return this._database.GetCollection<PositionalBonusCalculationDataMongoDB>("ContractItems_Bonus")
                .AsQueryable()
                .Where(p => p.InvoiceDate >= startDate && p.InvoiceDate <= endDate).ToList();
        }
    }
}
