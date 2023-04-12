using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Repository
{
    public class UserContractItemsRepository : MongoDBInstance
    {
        public UserContractItemsRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public UserContractItemsRepository(object client, object database) : base(client, database)
        {
        }

        public List<UserBasedBonusCalculationDataMongoDB> getUserContractItemsByDate(DateTime startDate, DateTime endDate)
        {
            return this._database.GetCollection<UserBasedBonusCalculationDataMongoDB>("UserContractItems")
                .AsQueryable()
                .Where(p => p.InvoiceDate >= startDate && p.InvoiceDate <= endDate).ToList();
        }
    }
}
