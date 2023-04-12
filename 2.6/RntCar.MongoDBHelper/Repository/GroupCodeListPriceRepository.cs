using RntCar.MongoDBHelper.Model;
using System.Linq;
using MongoDB.Driver;
using System.Collections.Generic;
using System;
using RntCar.SDK.Common;

namespace RntCar.MongoDBHelper.Repository
{
    public class GroupCodeListPriceRepository : MongoDBInstance
    {
        public GroupCodeListPriceRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public GroupCodeListPriceRepository(object client, object database) : base(client, database)
        {
        }


        public List<GroupCodeListPriceDataMongoDB> getGroupCodeListPricesByPriceList(string priceListId)
        {
            //todo add statecode == 0
            return this._database.GetCollection<GroupCodeListPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBGroupCodePriceListCollectionName"))
                           .AsQueryable()
                           .Where(p => p.PriceListId.Equals(priceListId) && p.State == 0).ToList();

        }
        public List<GroupCodeListPriceDataMongoDB> getGroupCodeListPricesByPriceListWithDuration(string priceListId, int duration)
        {
            //todo add statecode == 0
            var query = this._database.GetCollection<GroupCodeListPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBGroupCodePriceListCollectionName"))
                           .AsQueryable()
                           .Where(p => p.PriceListId.Equals(priceListId) && p.State == 0).ToList();


            return query.Where(p => p.MinimumDay.intToDate() <= duration && p.MaximumDay.intToDate() >= duration).ToList();

        }

        public GroupCodeListPriceDataMongoDB getGroupCodeListPricesByPriceListandGroupCodeWithDuration(string priceListId, Guid groupCodeInformationId, int duration)
        {
            //todo add statecode == 0
            var query = this._database.GetCollection<GroupCodeListPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBGroupCodePriceListCollectionName"))
                           .AsQueryable()
                           .Where(p => p.PriceListId.Equals(priceListId) && p.GroupCodeInformationId.Equals(groupCodeInformationId) && p.State == 0).ToList();


            var q = query.Where(p => p.MinimumDay.intToDate() <= duration && p.MaximumDay.intToDate() >= duration).ToList();

            if (q.Count == 0)
            {
                query = query.Where(p => p.MinimumDay.intToDate() >= duration).ToList();
                return query.OrderBy(item => item.MaximumDay).FirstOrDefault();
            }
            return q.FirstOrDefault();

        }
    }
}
