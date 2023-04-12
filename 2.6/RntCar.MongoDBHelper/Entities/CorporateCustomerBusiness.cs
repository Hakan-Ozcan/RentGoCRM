using MongoDB.Driver;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.SDK.Common;
using System;

namespace RntCar.MongoDBHelper.Entities
{
    public class CorporateCustomerBusiness : MongoDBInstance
    {
        public CorporateCustomerBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public MongoDBResponse CreateCorporateCustomer(CorporateCustomerData corporateCustomerData)
        {
            var collection = this.getCollection<CorporateCustomerData>(StaticHelper.GetConfiguration("MongoDBCorporateCustomerCollectionName"));
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var response = collection.Insert(corporateCustomerData, corporateCustomerData.corporateCustomerId, methodName);
            response.Id = Convert.ToString(corporateCustomerData.corporateCustomerId);

            return response;
        }

        public bool UpdateCorporateCustomer(CorporateCustomerData corporateCustomerData, string id)
        {
            var collection = this.getCollection<CorporateCustomerData>(StaticHelper.GetConfiguration("MongoDBCorporateCustomerCollectionName"));
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var filter = Builders<CorporateCustomerData>.Filter.Eq(p => p.corporateCustomerId, id);
            var response = collection.Replace(corporateCustomerData, filter, new UpdateOptions { IsUpsert = false }, id, methodName);

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
