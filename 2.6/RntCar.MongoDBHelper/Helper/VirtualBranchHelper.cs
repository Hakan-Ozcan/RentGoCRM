using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Helper
{
    public class VirtualBranchHelper
    {
        public string mongoDBHostName { get; set; }
        public string mongoDbDatabaseName { get; set; }
        public VirtualBranchHelper(string _mongoDBHostName, string _mongoDbDatabaseName)
        {
            mongoDBHostName = _mongoDBHostName;
            mongoDbDatabaseName = _mongoDbDatabaseName;
        }
        
        public Guid? getBranchId(Guid virtualBranchId)
        {
            VirtualBranchRepository virtualBranchRepository = new VirtualBranchRepository(mongoDBHostName, mongoDbDatabaseName);
            return virtualBranchRepository.getBranchByVirtualBranchId(virtualBranchId).branch;
        }

        public List<VirtualBranchDataMongoDB> getActiveVirtualBranches()
        {
            VirtualBranchRepository virtualBranchRepository = new VirtualBranchRepository(mongoDBHostName, mongoDbDatabaseName);
            return virtualBranchRepository.getActiveVirtualBranches();
        }

        public List<VirtualBranchDataMongoDB> getActiveBranchesByBrokerCodeandByChannelCode(string brokerCode, int channelCode)
        {
            VirtualBranchRepository virtualBranchRepository = new VirtualBranchRepository(mongoDBHostName, mongoDbDatabaseName);
            return virtualBranchRepository.getActiveBranchesByBrokerCodeandByChannelCode(brokerCode, channelCode);
        }

        public List<VirtualBranchDataMongoDB> getActiveBranchesByChannelCode(int channelCode)
        {
            VirtualBranchRepository virtualBranchRepository = new VirtualBranchRepository(mongoDBHostName, mongoDbDatabaseName);
            return virtualBranchRepository.getVirtualBranchByChannelCode(channelCode);
        }

        public VirtualBranchData mapVirtualBranch(string branchId)
        {
            VirtualBranchRepository virtualBranchRepository = new VirtualBranchRepository(mongoDBHostName, mongoDbDatabaseName);
            return virtualBranchRepository.getVirtualBranchByBranchId(branchId);
        }

        public QueryParameters buildVirtualBrachParameters(QueryParameters parameters)
        {
            return new QueryParameters
            {
                dropoffBranchId = getBranchId(parameters.dropoffBranchId).HasValue ? getBranchId(parameters.dropoffBranchId).Value : Guid.Empty,
                dropoffDateTime = parameters.dropoffDateTime,
                pickupBranchId = getBranchId(parameters.pickupBranchId).HasValue ? getBranchId(parameters.pickupBranchId).Value : Guid.Empty,
                pickupDateTime = parameters.pickupDateTime
            };
        }

        public RntCar.ClassLibrary._Web.QueryParameters buildVirtualBrachParameters(RntCar.ClassLibrary._Web.QueryParameters parameters)
        {
            return new RntCar.ClassLibrary._Web.QueryParameters
            {
                dropoffBranchId = getBranchId(parameters.dropoffBranchId).HasValue ? getBranchId(parameters.dropoffBranchId).Value : Guid.Empty,
                dropoffDateTime = parameters.dropoffDateTime,
                pickupBranchId = getBranchId(parameters.pickupBranchId).HasValue ? getBranchId(parameters.pickupBranchId).Value : Guid.Empty,
                pickupDateTime = parameters.pickupDateTime
            };
        }

        public RntCar.ClassLibrary._Mobile.QueryParameters buildVirtualBrachParameters(RntCar.ClassLibrary._Mobile.QueryParameters parameters)
        {
            return new RntCar.ClassLibrary._Mobile.QueryParameters
            {
                dropoffBranchId = getBranchId(parameters.dropoffBranchId).HasValue ? getBranchId(parameters.dropoffBranchId).Value : Guid.Empty,
                dropoffDateTime = parameters.dropoffDateTime,
                pickupBranchId = getBranchId(parameters.pickupBranchId).HasValue ? getBranchId(parameters.pickupBranchId).Value : Guid.Empty,
                pickupDateTime = parameters.pickupDateTime
            };
        }
    }
}
