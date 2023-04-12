using RntCar.MongoDBHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Entities
{
    public class TransactionHistoryBusiness : MongoDBInstance
    {
        public TransactionHistoryBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }
        public List<TransactionHistoryDataMongoDB> getTransactionHistories()
        {
            List<TransactionHistoryDataMongoDB> list = new List<TransactionHistoryDataMongoDB>();
            list.Add(new TransactionHistoryDataMongoDB
            {
                id = "1",
                name = "Test"
            });
            list.Add(new TransactionHistoryDataMongoDB
            {
                id = "2",
                name = "Test"
            });

            return list;
        }
    }
}
