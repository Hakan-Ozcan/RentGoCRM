using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Repository
{
    public class AvailabilityFactorRepository : MongoDBInstance
    {
        public AvailabilityFactorRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public AvailabilityFactorRepository(object client, object database) : base(client, database)
        {
        }
        public AvailabilityFactorDataMongoDB getMinimumReservationDay(DateTime pickupDateTime,
                                                                      DateTime dropoffDateTime,
                                                                      string pickupBranchId,
                                                                      int channel,
                                                                      int? customerType,
                                                                      string accountFactorType)
        {
            var pickupTimeStamp = new BsonTimestamp(pickupDateTime.converttoTimeStamp());
            var dropoffTimeStamp = new BsonTimestamp(dropoffDateTime.converttoTimeStamp());

            //AvailabilityFactorDataMongoDB
            var collection = this._database.GetCollection<AvailabilityFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityFactorCollectionName"));

            var list = (from e in collection.AsQueryable<AvailabilityFactorDataMongoDB>()
                        where e.availabilityFactorType.Equals((int)rnt_availabilityfactors_rnt_availabilityfactortypecode.MinimumReservationDay) &&
                        e.statecode.Equals(0) &&
                        ((e.endDateTimeStamp > dropoffTimeStamp && e.beginDateTimeStamp < dropoffTimeStamp) ||
                        (e.endDateTimeStamp > pickupTimeStamp && e.beginDateTimeStamp < pickupTimeStamp)) &&
                        e.channelValues.Contains(channel.ToString()) &&
                        e.branchValues.Contains(pickupBranchId)
                        select e).ToList();
            list = list.OrderByDescending(p => p.beginDateTimeStamp.Value.converttoDateTime()).ToList();

            //globalenum ve crm'deki option set farklılıgından dolayı brokersa acentaya , acentaysa brokera döndür!!!
            //crmde broker 40 , acenta 30
            if (customerType != null)
            {
                if (customerType == (int)GlobalEnums.CustomerType.Broker)
                {
                    customerType = (int)GlobalEnums.CustomerType.Agency * 10;
                }
                else if (customerType == (int)GlobalEnums.CustomerType.Agency)
                {
                    customerType = (int)GlobalEnums.CustomerType.Broker * 10;
                }
                else
                {
                    customerType = customerType * 10;
                }
            }

            if (customerType != null)
            {
                list = list.Where(p => p.type.Any(z => z.Contains(customerType.ToString()))).ToList();
            }
            if (accountFactorType != null)
            {
                var temp = list.ToList();

                list = list.Where(p => p.accountGroups.Any(z => z.Contains(accountFactorType))).ToList();
                //eğer alt kırılıma ait birşey bulamazsa , alt kırılım olmayan faktörlerden etkilensin.
                if (list.Count == 0)
                {
                    list = temp.Where(p => p.accountGroups.Count == 0).ToList();
                }
            }

            return list.FirstOrDefault();

        }
       

        public List<AvailabilityFactorData> getActiveIncreaseCapacityAvailabilitiesFactorByGivenChannel(DateTime pickupDateTime,
                                                                                                        DateTime dropoffDateTime,
                                                                                                        string pickupBranchId,
                                                                                                        int channel,
                                                                                                        int? customerType,
                                                                                                        string accountFactorType)
        {
            //globalenum ve crm'deki option set farklılıgından dolayı brokersa acentaya , acentaysa brokera döndür!!!
            //crmde broker 40 , acenta 30
            if(customerType != null)
            {
                if (customerType == (int)GlobalEnums.CustomerType.Broker)
                {
                    customerType = (int)GlobalEnums.CustomerType.Agency * 10;
                }
                else if (customerType == (int)GlobalEnums.CustomerType.Agency)
                {
                    customerType = (int)GlobalEnums.CustomerType.Broker * 10;
                }
                else
                {
                    customerType = customerType * 10;
                }
            }
            
            var pickupTimeStamp = new BsonTimestamp(pickupDateTime.AddMinutes(StaticHelper.offset).converttoTimeStamp());
            var dropoffTimeStamp = new BsonTimestamp(dropoffDateTime.AddMinutes(StaticHelper.offset).converttoTimeStamp());

            //AvailabilityFactorDataMongoDB
            var collection = this._database.GetCollection<AvailabilityFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityFactorCollectionName"));

            var list = (from e in collection.AsQueryable<AvailabilityFactorDataMongoDB>()
                        where e.availabilityFactorType.Equals((int)GlobalEnums.AvailabilityFactorType.IncreaseCapacity) &&
                        e.statecode.Equals(0) &&
                        e.branchValues.Contains(pickupBranchId) &&
                        e.endDateTimeStamp > dropoffTimeStamp && e.beginDateTimeStamp < dropoffTimeStamp &&
                        e.endDateTimeStamp > pickupTimeStamp && e.beginDateTimeStamp < pickupTimeStamp &&
                        e.channelValues.Contains(channel.ToString())
                        select e).ToList();
            var l = list.OrderByDescending(p => p.beginDateTimeStamp.Value.converttoDateTime()).ToList();

            if(customerType != null)
            {
                l = l.Where(p => p.type.Any(z => z.Contains(customerType.ToString()))).ToList();
            }
            if(accountFactorType != null)
            {
                var temp = l.ToList();
               
                l = l.Where(p => p.accountGroups.Any(z => z.Contains(accountFactorType))).ToList();
                //eğer alt kırılıma ait birşey bulamazsa , alt kırılım olmayan faktörlerden etkilensin.
                if (l.Count == 0)
                {
                    l = temp.Where(p => p.accountGroups.Count == 0).ToList();
                }
            }
            return l.ConvertAll(p => (AvailabilityFactorData)p).ToList();

        }

        public List<AvailabilityFactorDataMongoDB> getActiveAvailabilityClosureAvailabilityFactorByGivenChannel(DateTime pickupDateTime,
                                                                                                                DateTime dropoffDateTime,
                                                                                                                int channel,
                                                                                                                int? customerType,
                                                                                                                string accountFactorType)
        {
            //globalenum ve crm'deki option set farklılıgından dolayı brokersa acentaya , acentaysa brokera döndür!!!
            //crmde broker 40 , acenta 30
            if (customerType != null)
            {
                if (customerType == (int)GlobalEnums.CustomerType.Broker)
                {
                    customerType = (int)GlobalEnums.CustomerType.Agency * 10;
                }
                else if (customerType == (int)GlobalEnums.CustomerType.Agency)
                {
                    customerType = (int)GlobalEnums.CustomerType.Broker * 10;
                }
                else
                {
                    customerType = customerType * 10;
                }
            }


            var pickupTimeStamp = new BsonTimestamp(pickupDateTime.AddMinutes(StaticHelper.offset).converttoTimeStamp());
            var dropoffTimeStamp = new BsonTimestamp(dropoffDateTime.AddMinutes(StaticHelper.offset).converttoTimeStamp());

            //AvailabilityFactorDataMongoDB
            var collection = this._database.GetCollection<AvailabilityFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityFactorCollectionName"));

            var query =  (from e in collection.AsQueryable<AvailabilityFactorDataMongoDB>()
                    where e.availabilityFactorType.Equals((int)GlobalEnums.AvailabilityFactorType.AvailabilityClosure) &&
                    e.statecode.Equals(0) &&
                    ((e.endDateTimeStamp > dropoffTimeStamp && e.beginDateTimeStamp < dropoffTimeStamp) ||
                    (e.endDateTimeStamp > pickupTimeStamp && e.beginDateTimeStamp < pickupTimeStamp)) &&
                    e.channelValues.Contains(channel.ToString())
                    select e).ToList();

            if (customerType != null)
            {
                query = query.Where(p => p.type.Any(z => z.Contains(customerType.ToString()))).ToList();
            }
            if (accountFactorType != null)
            {
                var temp = query.ToList();

                query = query.Where(p => p.accountGroups.Any(z => z.Contains(accountFactorType))).ToList();
                //eğer alt kırılıma ait birşey bulamazsa , alt kırılım olmayan faktörlerden etkilensin.
                if (query.Count == 0)
                {
                    query = temp.Where(p => p.accountGroups.Count == 0).ToList();
                }
            }

            return query.ToList();

        }

        public List<AvailabilityFactorDataMongoDB> getActiveAvailabilityFactors()
        {           
            var collection = this._database.GetCollection<AvailabilityFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityFactorCollectionName"));

            return (from e in collection.AsQueryable<AvailabilityFactorDataMongoDB>()
                    where e.statecode.Equals(0)
                    select e).ToList();

        }
    }
}
