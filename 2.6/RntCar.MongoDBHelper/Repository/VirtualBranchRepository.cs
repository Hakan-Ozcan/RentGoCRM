using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class VirtualBranchRepository : MongoDBInstance
    {
        public VirtualBranchRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public VirtualBranchRepository(object client, object database) : base(client, database)
        {
        }

        public List<VirtualBranchDataMongoDB> getActiveVirtualBranches()
        {
            return this._database.GetCollection<VirtualBranchDataMongoDB>(StaticHelper.GetConfiguration("MongoDBVirtualBranchCollectionName"))
                           .AsQueryable()
                           .Where(p => p.statecode == (int)GlobalEnums.StateCode.Active).ToList();
        }
        public List<VirtualBranchDataMongoDB> getActiveBranchesByBrokerCodeandByChannelCode(string brokerCode, int channelCode)
        {
            return this._database.GetCollection<VirtualBranchDataMongoDB>(StaticHelper.GetConfiguration("MongoDBVirtualBranchCollectionName"))
                           .AsQueryable()
                           .Where(p => p.statecode == (int)GlobalEnums.StateCode.Active && 
                           p.brokerCode == brokerCode &&
                           p.channelCode == channelCode.ToString()).ToList();
        }
        public VirtualBranchDataMongoDB getBranchByVirtualBranchId(Guid virtualBranchId)
        {
            return this._database.GetCollection<VirtualBranchDataMongoDB>(StaticHelper.GetConfiguration("MongoDBVirtualBranchCollectionName"))
                           .AsQueryable()
                           .Where(p => p.virtualBranchId == Convert.ToString(virtualBranchId)).FirstOrDefault();
        }
        public VirtualBranchDataMongoDB getVirtualBranchByBranchId(string branchId)
        {
            return this._database.GetCollection<VirtualBranchDataMongoDB>(StaticHelper.GetConfiguration("MongoDBVirtualBranchCollectionName"))
                           .AsQueryable()
                           .Where(p => p.branch == new Guid(branchId)).FirstOrDefault();
        }

        public List<VirtualBranchDataMongoDB> getVirtualBranchByChannelCode(int channelCode)
        {
            return this._database.GetCollection<VirtualBranchDataMongoDB>(StaticHelper.GetConfiguration("MongoDBVirtualBranchCollectionName"))
                           .AsQueryable()
                           .Where(p => p.channelCode==channelCode.ToString()).ToList();
        }
    }
}
