using RntCar.MongoDBHelper.Model;
using System.Linq;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using RntCar.SDK.Common;
using RntCar.ClassLibrary.MongoDB;
using MongoDB.Bson;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;

namespace RntCar.MongoDBHelper.Repository
{
    public class ReservationItemRepository : MongoDBInstance
    {
        public ReservationItemRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public ReservationItemRepository(object client, object database) : base(client, database)
        {
        }
        public List<ReservationItemDataMongoDB> getCorporateReservationsByCustomer(Guid corporateCustomerId, Guid customerId)
        {
            var cId = customerId.ToString().Replace("{", "").Replace("}", "");
            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")

                                    .AsQueryable()
                                    .Where(p => p.ChangeReason == null && p.corporateCustomerId == corporateCustomerId && p.CustomerId == cId)
                                    .Select(x => new ReservationItemDataMongoDB
                                    {
                                        ReservationId = x.ReservationId,
                                        PnrNumber = x.PnrNumber,
                                        PickupTime = x.PickupTime,
                                        DropoffTime = x.DropoffTime,
                                        PickupBranchId = x.PickupBranchId,
                                        DropoffBranchId = x.DropoffBranchId,
                                        StatusCode = x.StatusCode,
                                        PickupBranchName = x.PickupBranchName,
                                        DropoffBranchName = x.DropoffBranchName,
                                        corporateCustomerName = x.corporateCustomerName,
                                        corporateCustomerId = x.corporateCustomerId,
                                        CustomerId = x.CustomerId,
                                        CustomerName = x.CustomerName,
                                        ReservationType = x.ReservationType,
                                        GroupCodeInformationId = x.GroupCodeInformationId,
                                        GroupCodeInformationName = x.GroupCodeInformationName
                                    }).ToList();
        }
        public List<ReservationItemDataMongoDB> getCorporateReservations(Guid corporateCustomerId)
        {
            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")

                                    .AsQueryable()
                                    .Where(p => p.ChangeReason == null && p.corporateCustomerId == corporateCustomerId)
                                    .Select(x => new ReservationItemDataMongoDB
                                    {
                                        ReservationId = x.ReservationId,
                                        PnrNumber = x.PnrNumber,
                                        PickupTime = x.PickupTime,
                                        DropoffTime = x.DropoffTime,
                                        PickupBranchId = x.PickupBranchId,
                                        DropoffBranchId = x.DropoffBranchId,
                                        StatusCode = x.StatusCode,
                                        PickupBranchName = x.PickupBranchName,
                                        DropoffBranchName = x.DropoffBranchName,
                                        corporateCustomerName = x.corporateCustomerName,
                                        corporateCustomerId = x.corporateCustomerId,
                                        CustomerId = x.CustomerId,
                                        CustomerName = x.CustomerName,
                                        ReservationType = x.ReservationType,
                                        GroupCodeInformationId = x.GroupCodeInformationId,
                                        GroupCodeInformationName = x.GroupCodeInformationName
                                    }).ToList();
        }
        public List<ReservationItemDataMongoDB> getReservationsByCustomerId(string customerId)
        {
            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")

                                    .AsQueryable()
                                    .Where(p => p.ChangeReason == null && p.CustomerId == customerId)
                                    .Select(x => new ReservationItemDataMongoDB
                                    {
                                        ReservationId = x.ReservationId,
                                        PnrNumber = x.PnrNumber,
                                        PickupTime = x.PickupTime,
                                        DropoffTime = x.DropoffTime,
                                        PickupBranchId = x.PickupBranchId,
                                        DropoffBranchId = x.DropoffBranchId,
                                        StatusCode = x.StatusCode,
                                        PickupBranchName = x.PickupBranchName,
                                        DropoffBranchName = x.DropoffBranchName,
                                        CustomerId = x.CustomerId,
                                        CustomerName = x.CustomerName,
                                        ReservationType = x.ReservationType,
                                        GroupCodeInformationName = x.GroupCodeInformationName,
                                        GroupCodeInformationId = x.GroupCodeInformationId
                                    }).ToList();
        }
        public List<ReservationItemDataMongoDB> getReservationsByCorporateId(Guid corporateCustomerId)
        {
            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")

                                    .AsQueryable()
                                    .Where(p => p.ChangeReason == null && p.corporateCustomerId == corporateCustomerId)
                                    .Select(x => new ReservationItemDataMongoDB
                                    {
                                        dummyContactInformation = x.dummyContactInformation,
                                        ReservationId = x.ReservationId,
                                        PnrNumber = x.PnrNumber,
                                        PickupTime = x.PickupTime,
                                        DropoffTime = x.DropoffTime,
                                        PickupBranchId = x.PickupBranchId,
                                        DropoffBranchId = x.DropoffBranchId,
                                        StatusCode = x.StatusCode,
                                        PickupBranchName = x.PickupBranchName,
                                        DropoffBranchName = x.DropoffBranchName,
                                        CustomerId = x.CustomerId,
                                        CustomerName = x.CustomerName,
                                        ReservationType = x.PaymentMethod.Value,
                                        GroupCodeInformationName = x.GroupCodeInformationName,
                                        GroupCodeInformationId = x.GroupCodeInformationId,
                                        TotalAmount = x.TotalAmount ,
                                        ReservationTotalAmount = x.ReservationTotalAmount
                                    }).ToList();
        }
        public List<ReservationItemDataMongoDB> getAllReservationItems()
        {
            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
            .AsQueryable()     
            .ToList();
        }

        public List<ReservationItemDataMongoDB> getAvailableReservations(DateTime queryPickupDate,
                                                                         Guid queryPickupBranchId)
        {
            var pickupTimeStamp = new BsonTimestamp(queryPickupDate.converttoTimeStamp());
            var collection = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                            .AsQueryable()
                            .Where(p => p.DropoffTimeStamp <= pickupTimeStamp &&
                                   p.DropoffBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                   p.PickupBranchId != Convert.ToString(queryPickupBranchId) &&
                                   p.ItemTypeCode.Equals(1) &&
                                   p.StatusCode.Equals((int)ReservationItemEnums.StatusCode.New)).ToList();

            return collection;

        }
        public List<ReservationItemDataMongoDB> getExcludedReservations(DateTime queryPickupDate,
                                                                        DateTime queryDropOffTime,
                                                                        Guid queryPickupBranchId)
        {
            return getExcludeReservations(queryPickupDate,
                                          queryDropOffTime,
                                          queryPickupBranchId);




        }
        public ReservationItemDataMongoDB getReservationByPnrNumber(string pnrNumber)
        {
            var reservationItem = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                                       .AsQueryable()
                                       .Where(p => p.PnrNumber.Equals(pnrNumber)).FirstOrDefault();

            return reservationItem;
        }
        public ReservationItemDataMongoDB getReservationItemById(string id)
        {
            var reservationItem = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                                       .AsQueryable()
                                       .Where(p => p._id.Equals(ObjectId.Parse(id))).FirstOrDefault();

            return reservationItem;
        }
        public ReservationItemDataMongoDB getReservationItemByReservationItemId(string reservationItemId)
        {
            var reservationItem = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                                       .AsQueryable()
                                       .Where(p => p.ReservationItemId.Equals(reservationItemId)).FirstOrDefault();

            return reservationItem;
        }

        public ReservationItemDataMongoDB getReservationItemByReservationId(string reservationId)
        {
            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                                  .AsQueryable()
                                  .Where(p => p.ReservationId.Equals(reservationId) &&
                                         p.ItemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.Equipment) &&
                                         p.StatusCode.Equals((int)ReservationItemEnums.StatusCode.New)).FirstOrDefault();

        }

        public List<ReservationItemDataMongoDB> getNewReservationsByBranchId(string branchId)
        {
            var today = DateTime.UtcNow;
            var tomorrow = today.AddDays(1);

            var query = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                       .AsQueryable()
                       .Where(p => p.PickupBranchId.Equals(branchId) &&
                              p.StatusCode.Equals((int)ReservationItemEnums.StatusCode.New) &&
                              p.ItemTypeCode.Equals((int)ReservationItemEnums.ItemTypeCode.Equipment))
                       .ToList();
            query = query.Where(p => p.PickupTimeStamp.Value.converttoDateTime().Day == today.Day ||
                                     p.PickupTimeStamp.Value.converttoDateTime().Day == tomorrow.Day).ToList();

            query = query.Where(p => p.PickupTimeStamp.Value.converttoDateTime().Month == today.Month ||
                                    p.PickupTimeStamp.Value.converttoDateTime().Month == tomorrow.Month).ToList();

            query = query.Where(p => p.PickupTimeStamp.Value.converttoDateTime().Year == today.Year ||
                                    p.PickupTimeStamp.Value.converttoDateTime().Year == tomorrow.Year).ToList();
            return query;

        }

        private List<ReservationItemDataMongoDB> getExcludeReservations(DateTime queryPickupDate,
                                                                        DateTime queryDropOffTime,
                                                                        Guid queryPickupBranchId)
        {
            var allReservations = new List<ReservationItemDataMongoDB>();
            Int32 queryPickupTimeStamp = queryPickupDate.converttoTimeStamp();
            Int32 queryDropoffTimeStamp = queryDropOffTime.converttoTimeStamp();

            var queryPickupBsonTimeStamp = new BsonTimestamp(queryPickupTimeStamp);
            var queryDropoffBsonTimeStamp = new BsonTimestamp(queryDropoffTimeStamp);

            //status code --> 1 new
            //sample -->
            //Query1 --> index for taht query can be found named --> ExcluededReservation_Query2
            //query is 10 october to 20 october
            //reservation 18 october to 22 october
            var collectionpickOff = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                            .AsQueryable()
                            .Where(p => queryPickupBsonTimeStamp <= p.PickupTimeStamp &&
                                   p.PickupTimeStamp <= queryDropoffBsonTimeStamp &&
                                   p.DropoffTimeStamp > queryDropoffBsonTimeStamp &&
                                   p.PickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                   p.ItemTypeCode.Equals(1) &&
                                   p.StatusCode.Equals((int)ReservationItemEnums.StatusCode.New)).ToList();
            allReservations.AddRange(collectionpickOff);
            //sample -->
            //Query2 --> index for taht query can be found named --> ExcluededReservation_Query2
            //query is 10 october to 20 october
            //reservation 8 october to 12 october
            var collectionDropOff = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                           .AsQueryable()
                           .Where(p => queryPickupBsonTimeStamp < p.DropoffTimeStamp &&
                                  p.DropoffTimeStamp <= queryDropoffBsonTimeStamp &&
                                  p.PickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                  p.ItemTypeCode.Equals(1) &&
                                  p.StatusCode.Equals((int)ReservationItemEnums.StatusCode.New)).ToList();

            allReservations.AddRange(collectionDropOff);

            //sample -->
            //query is 10 october to 20 october
            //reservation 8 october to 22 october
            var collectionBoth = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                           .AsQueryable()
                           .Where(p => queryPickupBsonTimeStamp > p.PickupTimeStamp &&
                                  queryDropoffBsonTimeStamp < p.DropoffTimeStamp &&
                                  p.PickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                  p.ItemTypeCode.Equals(1) &&
                                  p.StatusCode.Equals((int)ReservationItemEnums.StatusCode.New)).ToList();

            allReservations.AddRange(collectionBoth);
            //sample -->
            //query is 10 october to 20 october
            //reservation 12 october to 16 october
            var collectionDropOff1 = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                           .AsQueryable()
                           .Where(p => queryPickupBsonTimeStamp <= p.PickupTimeStamp &&
                                      p.PickupTimeStamp <= queryDropoffBsonTimeStamp &&
                                      queryPickupBsonTimeStamp <= p.DropoffTimeStamp &&
                                      p.DropoffTimeStamp <= queryDropoffBsonTimeStamp &&
                                      p.PickupBranchId.Equals(Convert.ToString(queryPickupBranchId)) &&
                                      p.ItemTypeCode.Equals(1) &&
                                      p.StatusCode.Equals((int)ReservationItemEnums.StatusCode.New)).ToList();
            allReservations.AddRange(collectionDropOff1);
            //res 9.12.2018 21:30 --> 10.12.2018 21:30 
            //query 9.12.2018 21:45 --> 10.12.2018 21:45 
            //var collectionDropOff2 = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
            //             .AsQueryable()
            //             .Where(p => queryPickupBsonTimeStamp >= p.PickupTimeStamp &&
            //                         queryPickupBsonTimeStamp <= p.DropoffTimeStamp &&
            //                         queryDropoffBsonTimeStamp >= p.DropoffTimeStamp &&
            //                         p.PickupBranchId.Equals(Convert.ToString(queryDropoffBranchId)) &&
            //                         p.ItemTypeCode.Equals(1) &&
            //                         p.StateCode.Equals(0)).ToList();

            //allReservations.AddRange(collectionBoth);

            return allReservations;
        }


        public void getReservationItemByGivenQuery(ReservationItemSearchParameters reservationItemSearchParameters)
        {
            var collection = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems");

            List<ReservationItemDataMongoDB> response = new List<ReservationItemDataMongoDB>();

            var builder = Builders<ReservationItemDataMongoDB>.Filter;
            //default filter 
            var filter = Builders<ReservationItemDataMongoDB>.Filter.Ne(x => x.ReservationId, null);
            if (reservationItemSearchParameters.dropoffDateTime.HasValue)
            {
                var value = reservationItemSearchParameters.dropoffDateTime.Value;
                var end = value.AddHours(-value.Hour).AddHours(23)
                               .AddMinutes(-value.Minute).AddMinutes(59)
                               .AddSeconds(-value.Second);
                var start = value.AddMinutes(-value.Minute)
                                 .AddHours(-value.Hour)
                                 .AddSeconds(-value.Second);

                var f = builder.Lte(p => p.DropoffTime, end);
                var f1 = builder.Gte(p => p.DropoffTime, start);

                filter = filter & f & f1;

                //var result = collection.Find(f).ToList();
            }
            if (reservationItemSearchParameters.pickupDateTime.HasValue)
            {
                var value = reservationItemSearchParameters.pickupDateTime.Value;
                var end = value.AddHours(-value.Hour).AddHours(23)
                               .AddMinutes(-value.Minute).AddMinutes(59)
                               .AddSeconds(-value.Second);
                var start = value.AddMinutes(-value.Minute)
                                 .AddHours(-value.Hour)
                                 .AddSeconds(-value.Second);

                var f = builder.Lte(p => p.PickupTime, end);
                var f1 = builder.Gte(p => p.PickupTime, start);

                filter = filter & f & f1;
            }
            if (!string.IsNullOrEmpty(reservationItemSearchParameters.customerId))
            {
                var f = builder.Eq(p => p.CustomerId, reservationItemSearchParameters.customerId);

                filter = filter & f;
            }
            if (!string.IsNullOrEmpty(reservationItemSearchParameters.dropOffBranchId))
            {
                var f = builder.Eq(p => p.PickupBranchId, reservationItemSearchParameters.pickupBranchId);
                filter = filter & f;
            }
            if (!string.IsNullOrEmpty(reservationItemSearchParameters.reservationNumber))
            {
                var f = builder.ElemMatch(p => p.ReservationNumber, reservationItemSearchParameters.reservationNumber);
                var f1 = builder.ElemMatch(p => p.ReservationNumber, reservationItemSearchParameters.reservationNumber);
                filter = filter & (f | f1);
            }
            var result = collection.Find(filter).ToList();

        }

        public List<ReservationItemDataMongoDB> getWillNoShowReservationItems(DateTime currentDate)
        {
            var allReservations = new List<ReservationItemDataMongoDB>();

            var dateFilter = new BsonTimestamp(currentDate.converttoTimeStamp());

            var collectionNoShow = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                            .AsQueryable()
                            .Where(p => p.NoShowTimeStamp <= dateFilter &&
                                   p.ItemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.Equipment) &&
                                   p.StatusCode.Equals((int)rnt_reservationitem_StatusCode.New)).ToList();
            allReservations.AddRange(collectionNoShow);

            return allReservations;
        }

        public List<ReservationItemDataMongoDB> getWillCancelReservationItems(DateTime currentDate)
        {
            var allReservations = new List<ReservationItemDataMongoDB>();

            var dateFilter = new BsonTimestamp(currentDate.converttoTimeStamp());

            var collectionNoShow = this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                            .AsQueryable()
                            .Where(p => p.CancellationTimeStamp <= dateFilter &&
                                   p.ItemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.Equipment) &&
                                   p.StatusCode.Equals((int)rnt_reservationitem_StatusCode.NoShow)).ToList();
            allReservations.AddRange(collectionNoShow);

            return allReservations;
        }

        public List<ReservationItemDataMongoDB> getAdditionalReservationItemsByReservationId(string reservationId)
        {
            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                                  .AsQueryable()
                                  .Where(p => p.ReservationId.Equals(reservationId) &&
                                              p.ItemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.AdditionalProduct) &&
                                              p.StatusCode.Equals((int)rnt_reservationitem_StatusCode.New)).ToList();
        }

        public List<ReservationItemDataMongoDB> getReservationsCreatedCurrentDateByBranchId(Guid branchId)
        {
            var currentDateStart = DateTime.Now.Date;
            var currentDateEnd = currentDateStart.Date.AddDays(1);
            var dateFilterStart = new BsonTimestamp(currentDateStart.converttoTimeStamp());
            var dateFilterEnd = new BsonTimestamp(currentDateEnd.converttoTimeStamp());

            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                       .AsQueryable()
                       .Where(item => item.ItemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.Equipment) &&
                                      (item.StatusCode.Equals((int)rnt_reservationitem_StatusCode.Completed) ||
                                      item.StatusCode.Equals((int)rnt_reservationitem_StatusCode.New)) &&
                                      item.PickupBranchId.Equals(branchId) &&
                                      item.PickupTimeStamp >= dateFilterStart && item.PickupTimeStamp < dateFilterEnd)
                       .ToList();
        }

        public List<ReservationItemDataMongoDB> getReservationsCreatedCurrentDate()
        {
            var currentDateStart = DateTime.Now.Date;
            var currentDateEnd = currentDateStart.Date.AddDays(1);
            var dateFilterStart = new BsonTimestamp(currentDateStart.converttoTimeStamp());
            var dateFilterEnd = new BsonTimestamp(currentDateEnd.converttoTimeStamp());

            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                       .AsQueryable()
                       .Where(item => item.ItemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.Equipment) &&
                                      (item.StatusCode.Equals((int)rnt_reservationitem_StatusCode.Completed) ||
                                      item.StatusCode.Equals((int)rnt_reservationitem_StatusCode.New)) &&
                                      item.PickupTimeStamp >= dateFilterStart && item.PickupTimeStamp < dateFilterEnd)
                       .ToList();
        }

        public List<ReservationItemDataMongoDB> getDailyAllReservationItemsByBranchId(Guid branchId)
        {
            var currentDateStart = DateTime.Now.AddMinutes(StaticHelper.offset).Date;
            var currentDateEnd = currentDateStart.Date.AddDays(1);
            var dateFilterStart = new BsonTimestamp(currentDateStart.converttoTimeStamp());
            var dateFilterEnd = new BsonTimestamp(currentDateEnd.converttoTimeStamp());

            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
                       .AsQueryable()
                       .Where(item => item.PickupBranchId.Equals(branchId) &&
                                       (item.StatusCode != (int)rnt_reservationitem_StatusCode.CustomerDemand &&
                                        item.StatusCode != (int)rnt_reservationitem_StatusCode.GroupCodeChanges &&
                                        item.StatusCode != (int)rnt_reservationitem_StatusCode.Rental &&
                                        item.StatusCode != (int)rnt_reservationitem_StatusCode.WaitingForDelivery &&
                                        item.StatusCode != (int)rnt_reservationitem_StatusCode.Completed) &&
                                        item.ItemTypeCode.Equals((int)GlobalEnums.ItemTypeCode.Equipment) &&
                                        item.PickupTimeStamp >= dateFilterStart && item.PickupTimeStamp < dateFilterEnd)
                       .ToList();
        }
        
        public  List<ReservationItemDataMongoDB> getAllReservationItems_ALL()
        {
            return this._database.GetCollection<ReservationItemDataMongoDB>("ReservationItems")
            .AsQueryable()
            .ToList();
        }
    }


}
