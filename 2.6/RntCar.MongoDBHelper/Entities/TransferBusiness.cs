using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Entities
{
    public class TransferBusiness : MongoDBInstance
    {
        public TransferBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }
        public MongoDBResponse CreateTransfer(TransferData transferData)
        {
            var collection = this.getCollection<TransferDataMongoDB>(StaticHelper.GetConfiguration("MongoDBTransferCollectionName"));

            var transfer = new TransferDataMongoDB();
            transfer = transfer.Map(transferData);
            transfer._id = ObjectId.GenerateNewId();
            ;
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = transfer._id.ToString();

            var response = collection.Insert(transfer, itemId, methodName);
            response.Id = Convert.ToString(transfer._id);

            return response;
        }
        public bool UpdateTransfer(TransferData transferData, string id)
        {
            var collection = this.getCollection<TransferDataMongoDB>(StaticHelper.GetConfiguration("MongoDBTransferCollectionName"));

            var transfer = new TransferDataMongoDB();
            transfer = transfer.Map(transferData);
            transfer._id = ObjectId.Parse(id);

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = transfer._id.ToString();

            var filter = Builders<TransferDataMongoDB>.Filter.Eq(p => p._id, transfer._id);
            var response = collection.Replace(transfer, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == transfer._id, transfer, new UpdateOptions { IsUpsert = false });

            if (response != null)
            {
                if (!response.IsAcknowledged)
                {
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }
    }
}
