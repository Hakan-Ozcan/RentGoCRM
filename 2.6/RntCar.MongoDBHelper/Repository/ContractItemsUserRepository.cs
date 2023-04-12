using MongoDB.Driver;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Repository
{
    public class ContractItemsUserRepository : MongoDBInstance
    {
        private string collectionName { get; set; }

        public ContractItemsUserRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBUserBasedBonusCalculatorCollectionName");
        }

        public ContractItemsUserRepository(object client, object database) : base(client, database)
        {
        }

        public List<UserBasedBonusCalculationDataMongoDB> getRevenueBonusCalculationsByDate(DateTime startDate, DateTime endDate)
        {
            return this._database.GetCollection<UserBasedBonusCalculationDataMongoDB>(collectionName)
               
                .AsQueryable()
                .Where(p => p.InvoiceDate > startDate && p.InvoiceDate < endDate).ToList();
        }
    }
}
