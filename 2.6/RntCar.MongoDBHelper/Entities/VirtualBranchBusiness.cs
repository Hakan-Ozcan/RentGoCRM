using MongoDB.Driver;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.SDK.Common;
using System;

namespace RntCar.MongoDBHelper.Entities
{
    public class VirtualBranchBusiness : MongoDBInstance
    {
        public VirtualBranchBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public MongoDBResponse CreateVirtualBranch(VirtualBranchData virtualBranchData)
        {
            var collection = this.getCollection<VirtualBranchData>(StaticHelper.GetConfiguration("MongoDBVirtualBranchCollectionName"));
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var response = collection.Insert(virtualBranchData, virtualBranchData.virtualBranchId, methodName);
            response.Id = Convert.ToString(virtualBranchData.virtualBranchId);

            return response;
        }

        public bool UpdateVirtualBranch(VirtualBranchData virtualBranchData, string id)
        {
            var collection = this.getCollection<VirtualBranchData>(StaticHelper.GetConfiguration("MongoDBVirtualBranchCollectionName"));
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var filter = Builders<VirtualBranchData>.Filter.Eq(p => p.virtualBranchId, id);
            var response = collection.Replace(virtualBranchData, filter, new UpdateOptions { IsUpsert = false }, id, methodName);

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
