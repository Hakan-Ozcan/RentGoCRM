using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.Enums;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class PriceFactorRepository : MongoDBInstance
    {
        public PriceFactorRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public PriceFactorRepository(object client, object database) : base(client, database)
        {

        }
        public PriceFactorDataMongoDB getPriceFactorByPriceDateandTypeandChannelType(long priceDateTimeStamp,
                                                                                     PriceFactorEnums.PriceFactorType priceFactorType,
                                                                                     string channelType,
                                                                                     string pickupBranchId,
                                                                                     string groupCodeInformation,
                                                                                     int customerType,
                                                                                     string accountGroup)
        {
            //index_1
            customerType = customerType * 10;
            var query = this._database.GetCollection<PriceFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceFactorCollectionName"))
                         .AsQueryable()
                         .Where(p => p.priceFactorType.Equals((int)priceFactorType) &&
                                     p.reservationChannel.Contains(channelType) &&
                                     p.branchs.Contains(pickupBranchId) &&
                                     p.type.Contains(customerType.ToString()) &&
                                     p.groupCodes.Contains(groupCodeInformation) &&
                                     p.statecode.Equals((int)GlobalEnums.StateCode.Active)).ToList();

            query = query.Where(p => (p.endDateTimeStamp.Value.converttoDateTime().Date >= priceDateTimeStamp.converttoDateTime().Date) ||
                                     (p.dates != null && p.dates.Any(item => item.endDate.converttoDateTime().Date >= priceDateTimeStamp.converttoDateTime().Date))).ToList();

            query = query.Where(p => p.beginDateTimeStamp.Value.converttoDateTime().Date <= priceDateTimeStamp.converttoDateTime().Date ||
                                     (p.dates != null && p.dates.Any(item => item.beginDate.converttoDateTime().Date <= priceDateTimeStamp.converttoDateTime().Date))).ToList();

            if (!string.IsNullOrEmpty(accountGroup) && customerType != 10)
            {
                var temp = query.ToList();
                query = query.Where(p => !string.IsNullOrEmpty(p.accountGroups) &&
                                        p.accountGroups.Contains(accountGroup)).ToList();

                if (query.Count == 0)
                {
                    query = temp.Where(p => string.IsNullOrEmpty(p.accountGroups) ||
                                        p.accountGroups == "[]").ToList();
                }
            }
            else if (string.IsNullOrEmpty(accountGroup) && customerType != 10)
            {
                query = query.Where(p => string.IsNullOrEmpty(p.accountGroups) ||
                                    p.accountGroups == "[]").ToList();
            }
            return query.FirstOrDefault();
        }
        public PriceFactorDataMongoDB getPriceFactorByPriceDateandTypeandWeekdays(long priceDateTimeStamp,
                                                                                  PriceFactorEnums.PriceFactorType priceFactorType,
                                                                                  string pickupBranchId,
                                                                                  string groupCodeInformation,
                                                                                  int customerType,
                                                                                  string accountGroup)
        {
            //index_2
            customerType = customerType * 10;
            var dayoftheDate = Convert.ToString((int)priceDateTimeStamp.converttoDateTime().DayOfWeek);
            var query = this._database.GetCollection<PriceFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceFactorCollectionName"))
                            .AsQueryable()
                            .Where(p => p.priceFactorType.Equals((int)priceFactorType) &&                                   
                                   p.weekDays.Contains(dayoftheDate) &&
                                   p.branchs.Contains(pickupBranchId) &&
                                   p.type.Contains(customerType.ToString()) &&
                                   p.groupCodes.Contains(groupCodeInformation) &&
                                   p.statecode.Equals((int)GlobalEnums.StateCode.Active)).ToList();

            query = query.Where(p => (p.endDateTimeStamp.Value.converttoDateTime().Date >= priceDateTimeStamp.converttoDateTime().Date) ||
                                    (p.dates != null && p.dates.Any(item => item.endDate.converttoDateTime().Date >= priceDateTimeStamp.converttoDateTime().Date))).ToList();

            query = query.Where(p => p.beginDateTimeStamp.Value.converttoDateTime().Date <= priceDateTimeStamp.converttoDateTime().Date ||
                                     (p.dates != null && p.dates.Any(item => item.beginDate.converttoDateTime().Date <= priceDateTimeStamp.converttoDateTime().Date))).ToList();

            if (!string.IsNullOrEmpty(accountGroup) && customerType != 10)
            {
                var temp = query.ToList();
                query = query.Where(p => !string.IsNullOrEmpty(p.accountGroups) &&
                                        p.accountGroups.Contains(accountGroup)).ToList();

                if (query.Count == 0)
                {
                    query = temp.Where(p => string.IsNullOrEmpty(p.accountGroups) ||
                                        p.accountGroups == "[]").ToList();
                }
            }
            else if (string.IsNullOrEmpty(accountGroup) && customerType != 10)
            {
                query = query.Where(p => string.IsNullOrEmpty(p.accountGroups) ||
                                    p.accountGroups == "[]").ToList();
            }
            return query.FirstOrDefault();
        }
        public PriceFactorDataMongoDB getPriceFactorByPriceDateandTypeandCustomer(long priceDateTimeStamp,
                                                                                  PriceFactorEnums.PriceFactorType priceFactorType,
                                                                                  int? segment,
                                                                                  string pickupBranchId,
                                                                                  string groupCodeInformation,
                                                                                  int customerType,
                                                                                  string accountGroup)
        {
            customerType = customerType * 10;
            var query = this._database.GetCollection<PriceFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceFactorCollectionName"))
                            .AsQueryable()
                            .Where(p => p.priceFactorType.Equals((int)priceFactorType) &&
                             p.segments.Contains(Convert.ToString(segment)) &&
                             p.branchs.Contains(pickupBranchId) &&
                             p.groupCodes.Contains(groupCodeInformation) &&
                             p.statecode.Equals((int)GlobalEnums.StateCode.Active)).ToList();

            query = query.Where(p => (p.endDateTimeStamp.Value.converttoDateTime().Date >= priceDateTimeStamp.converttoDateTime().Date) ||
                                     (p.dates != null && p.dates.Any(item => item.endDate.converttoDateTime().Date >= priceDateTimeStamp.converttoDateTime().Date))).ToList();

            query = query.Where(p => p.beginDateTimeStamp.Value.converttoDateTime().Date <= priceDateTimeStamp.converttoDateTime().Date ||
                                     (p.dates != null && p.dates.Any(item => item.beginDate.converttoDateTime().Date <= priceDateTimeStamp.converttoDateTime().Date))).ToList();

            if (!string.IsNullOrEmpty(accountGroup) && customerType != 10)
            {
                var temp = query.ToList();
                query = query.Where(p => !string.IsNullOrEmpty(p.accountGroups) &&
                                        p.accountGroups.Contains(accountGroup)).ToList();

                if (query.Count == 0)
                {
                    query = temp.Where(p => string.IsNullOrEmpty(p.accountGroups) ||
                                        p.accountGroups == "[]").ToList();
                }
            }
            else if (string.IsNullOrEmpty(accountGroup) && customerType != 10)
            {
                query = query.Where(p => string.IsNullOrEmpty(p.accountGroups) ||
                                    p.accountGroups == "[]").ToList();
            }
            return query.FirstOrDefault();
        }
        public PriceFactorDataMongoDB getPriceFactorByDate(long priceDateTimeStamp,
                                                           int priceFactorType,
                                                           string pickupBranchId,
                                                           string groupCodeInformation,
                                                           int customerType,
                                                           string accountGroup)
        {
            //index_3
            customerType = customerType * 10;
            var query = this._database.GetCollection<PriceFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceFactorCollectionName"))
                       .AsQueryable()
                       .Where(p => p.priceFactorType.Equals(priceFactorType) &&
                                   p.branchs.Contains(pickupBranchId) &&
                                   p.groupCodes.Contains(groupCodeInformation) &&
                                   p.type.Contains(customerType.ToString()) &&
                                   p.statecode.Equals((int)GlobalEnums.StateCode.Active)).ToList();

            query = query.Where(p => (p.endDateTimeStamp.Value.converttoDateTime().Date >= priceDateTimeStamp.converttoDateTime().Date) ||
                                     (p.dates != null && p.dates.Any(item => item.endDate.converttoDateTime().Date >= priceDateTimeStamp.converttoDateTime().Date))).ToList();

            query = query.Where(p => p.beginDateTimeStamp.Value.converttoDateTime().Date <= priceDateTimeStamp.converttoDateTime().Date ||
                                    (p.dates != null && p.dates.Any(item => item.beginDate.converttoDateTime().Date <= priceDateTimeStamp.converttoDateTime().Date))).ToList();
            //bireysel olmayan senaryolarda calısması için
            if (!string.IsNullOrEmpty(accountGroup) && customerType != 10)
            {
                var temp = query.ToList();
                query = query.Where(p => !string.IsNullOrEmpty(p.accountGroups) &&
                                        p.accountGroups.Contains(accountGroup)).ToList();

                if (query.Count == 0)
                {
                    query = temp.Where(p => string.IsNullOrEmpty(p.accountGroups) ||
                                        p.accountGroups == "[]").ToList();
                }
            }
            else if (string.IsNullOrEmpty(accountGroup) && customerType != 10)
            {
                query = query.Where(p => string.IsNullOrEmpty(p.accountGroups) ||
                                    p.accountGroups == "[]").ToList();
            }
            return query.FirstOrDefault();

        }
        public PriceFactorDataMongoDB getPriceFactorByFactor(PriceFactorEnums.PriceFactorType priceFactorType,
                                                            string pickupBranchId,
                                                            string groupCodeInformationId,
                                                            int customerType,
                                                            string accountGroup)
        {
            //index_4
            customerType = customerType * 10;
            var query = this._database.GetCollection<PriceFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceFactorCollectionName"))
                            .AsQueryable()
                            .Where(p => p.priceFactorType.Equals((int)priceFactorType) &&
                                   p.type.Contains(customerType.ToString()) &&
                                   p.statecode.Equals((int)GlobalEnums.StateCode.Active) &&
                                   p.branchs.Contains(pickupBranchId) &&
                                   p.groupCodes.Contains(groupCodeInformationId)).ToList();

            if (!string.IsNullOrEmpty(accountGroup) && customerType != 10)
            {
                var temp = query.ToList();
                query = query.Where(p => !string.IsNullOrEmpty(p.accountGroups) &&
                                        p.accountGroups.Contains(accountGroup)).ToList();
                if (query.Count == 0)
                {
                    query = temp.Where(p => string.IsNullOrEmpty(p.accountGroups) ||
                                        p.accountGroups == "[]").ToList();
                }
            }
            else if (string.IsNullOrEmpty(accountGroup) && customerType != 10)
            {
                query = query.Where(p => string.IsNullOrEmpty(p.accountGroups) ||
                                    p.accountGroups == "[]").ToList();
            }
            return query.FirstOrDefault();
        }
        public List<PriceFactorDataMongoDB> getPriceFactors()
        {
            //index_4
            return this._database.GetCollection<PriceFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceFactorCollectionName"))
                       .AsQueryable()
                       .ToList();
        }

    }
}
