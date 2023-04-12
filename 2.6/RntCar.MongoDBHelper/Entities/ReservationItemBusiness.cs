using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.MongoDBHelper.Entities
{
    public class ReservationItemBusiness : MongoDBInstance
    {
        public ReservationItemBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public MongoDBResponse CreateReservation(ReservationItemData reservationItemData)
        {
            var collection = this.getCollection<ReservationItemDataMongoDB>(StaticHelper.GetConfiguration("MongoDBReservationCollectionName"));

            var reservation = new ReservationItemDataMongoDB();
            //copy reservationItemData to ReservationItemDataMongoDB
            reservation = reservation.Map(reservationItemData);
            reservation._id = ObjectId.GenerateNewId();
            reservation.DropoffTimeStamp = new BsonTimestamp(reservationItemData.DropoffTime.converttoTimeStamp());
            reservation.PickupTimeStamp = new BsonTimestamp(reservationItemData.PickupTime.converttoTimeStamp());
            reservation.NoShowTimeStamp = new BsonTimestamp(reservationItemData.NoShowTime.Value.converttoTimeStamp());
            reservation.CancellationTimeStamp = new BsonTimestamp(reservationItemData.CancellationTime.Value.converttoTimeStamp());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = reservation._id.ToString();

            MongoDBInstance mongoDBInstanceCamp = new MongoDBInstance(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                      StaticHelper.GetConfiguration("MongoDBDatabaseName"));


            var collection2 = mongoDBInstanceCamp.getCollection<CampaignAvailabilityMongoDB>("AvailabilityCampaings");

            var data = collection2.AsQueryable().Where(p => p.trackingNumber == reservationItemData.trackingNumber &&
                                                            p.groupCodedId == reservationItemData.pricingGroupCodeId).FirstOrDefault();

            if (data != null)
            {
                reservation.campaignId = new Guid(data.campaignId);
            }


            var response = collection.Insert(reservation, itemId, methodName);
            response.Id = Convert.ToString(reservation._id);

            if (data != null)
            {
                response.campaignId = data.campaignId;
            }
            if (reservationItemData.ItemTypeCode == 1)
            {
                DailyPricesBusiness dailyPricesBusiness = new DailyPricesBusiness(this._client, this._database);
                dailyPricesBusiness.createDailyPrices(reservation);
            }
            return response;
        }

        public MongoDBResponse UpdateReservation(ReservationItemData reservationItemData, string id)
        {
            try
            {
                //todo try catch mechasim
                var collection = this.getCollection<ReservationItemDataMongoDB>(StaticHelper.GetConfiguration("MongoDBReservationCollectionName"));
                var reservation = new ReservationItemDataMongoDB();
                //copy reservationItemData to ReservationItemDataMongoDB
                reservation = reservation.Map(reservationItemData);
                reservation.DropoffTimeStamp = new BsonTimestamp(reservationItemData.DropoffTime.converttoTimeStamp());
                reservation.PickupTimeStamp = new BsonTimestamp(reservationItemData.PickupTime.converttoTimeStamp());
                reservation.NoShowTimeStamp = new BsonTimestamp(reservationItemData.NoShowTime.Value.converttoTimeStamp());
                reservation.CancellationTimeStamp = new BsonTimestamp(reservationItemData.CancellationTime.Value.converttoTimeStamp());

                reservation.trackingNumber = reservationItemData.trackingNumber;
                reservation._id = ObjectId.Parse(id);

                var methodName = ErrorLogsHelper.GetCurrentMethod();
                var itemId = reservation._id.ToString();

                var filter = Builders<ReservationItemDataMongoDB>.Filter.Eq(p => p._id, reservation._id);
                var result = collection.Replace(reservation, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
                //var result = collection.ReplaceOne(p => p._id == reservation._id, reservation, new UpdateOptions { IsUpsert = false });
                if (result != null && reservationItemData.StateCode == (int)GlobalEnums.StateCode.Active)
                {
                    if (!result.IsAcknowledged)
                    {
                        return MongoDBResponse.ReturnError(string.Format("Update reservation error; id : {0} ", id));
                    }

                    if (result.IsAcknowledged && reservation.ItemTypeCode == (int)GlobalEnums.ItemTypeCode.Equipment)
                    {

                        DailyPricesBusiness dailyPricesBusiness = new DailyPricesBusiness(this._client, this._database);
                        //before delete all daily prices by reservationId
                        //because always need latest daiky prices for reservation update
                        var r = dailyPricesBusiness.deleteDailyPricesByReservationItemId(new Guid(reservationItemData.ReservationItemId));
                        //todo will implement a lofic if deletion in mongodb fails
                        if (r)
                        {
                            dailyPricesBusiness.createDailyPrices(reservationItemData);
                        }
                        return MongoDBResponse.ReturnSuccess();
                    }
                    else if (result.IsAcknowledged && reservation.ItemTypeCode != (int)GlobalEnums.ItemTypeCode.Equipment)
                    {
                        return MongoDBResponse.ReturnSuccess();
                    }
                    else
                    {
                        return MongoDBResponse.ReturnError(string.Format("Update reservation error; id : {0} ", id));
                    }

                }
                else
                {
                    return MongoDBResponse.ReturnError("Update reservation error , result is null : " + id);
                }
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(string.Format("Update reservation error id : {0} , exception detail : {1}", id, ex.Message));
            }

        }
        public MongoDBResponse updateDeposit(decimal depositAmount, string id)
        {
            try
            {
                //todo try catch mechasim
                var collection = this.getCollection<ReservationItemDataMongoDB>(StaticHelper.GetConfiguration("MongoDBReservationCollectionName"));


                ReservationItemRepository reservationItemRepository = new ReservationItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName")); ;
                var r = reservationItemRepository.getReservationItemById(id);
                r.DepositAmount = depositAmount;

                var methodName = ErrorLogsHelper.GetCurrentMethod();
                var itemId = r._id.ToString();

                var filter = Builders<ReservationItemDataMongoDB>.Filter.Eq(p => p._id, r._id);
                var result = collection.Replace(r, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
                return MongoDBResponse.ReturnSuccess();
            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(string.Format("Update reservation error id : {0} , exception detail : {1}", id, ex.Message));
            }

        }
        public void searchReservation(ReservationItemSearchParameters reservationItemSearchParameters)
        {

        }

        public ReservationFineAmountResponse getFirstDayPrice(Guid reservationItemId)
        {
            DailyPricesRepository dailyPricesRepository = new DailyPricesRepository(this._client, this._database);
            var price = dailyPricesRepository.getFirstDayPrice(reservationItemId);

            if (price == null)
            {
                return new ReservationFineAmountResponse
                {
                    //todo mongodb response text mechanism
                    ResponseResult = ResponseResult.ReturnError("first day price couldnt found")
                };
            }
            //check reservation type 
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this._client, this._database);
            var reservationItem = reservationItemRepository.getReservationItemByReservationItemId(Convert.ToString(reservationItemId));

            var firstDayAmount = decimal.Zero;
            if (reservationItem.PaymentChoice.Value == (int)PaymentEnums.PaymentType.PayNow)
            {
                firstDayAmount = price.payNowAmount;
            }
            else
            {
                firstDayAmount = price.payLaterAmount;
            }

            return new ReservationFineAmountResponse
            {
                firstDayAmount = firstDayAmount,
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public List<ClassLibrary._Tablet.DashboardReservationData> getDashboardNewReservationsByBranchId(string branchId)
        {
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this._client, this._database);
            var collection = reservationItemRepository.getNewReservationsByBranchId(branchId);

            List<ClassLibrary._Tablet.DashboardReservationData> data = new List<ClassLibrary._Tablet.DashboardReservationData>();
            foreach (var item in collection)
            {
                try
                {
                    data.Add(new ClassLibrary._Tablet.DashboardReservationData
                    {
                        reservationId = item.ReservationId,
                        reservationNumber = item.ReservationNumber,
                        pnrNumber = item.PnrNumber,
                        dropoffTimestamp = item.DropoffTimeStamp.Value,
                        pickupTimestamp = item.PickupTimeStamp.Value,
                        groupCodeInformation = new ClassLibrary._Tablet.DashboardGroupCodeInformation
                        {
                            groupCodeId = new Guid(item.GroupCodeInformationId),
                            groupCodeName = item.GroupCodeInformationName,
                        },
                        customer = new ClassLibrary._Tablet.DashboardCustomer
                        {
                            customerId = new Guid(item.CustomerId),
                            fullName = item.CustomerName,
                        },
                        pickupBranch = new ClassLibrary._Tablet.Branch
                        {
                            branchId = Guid.Parse(item.PickupBranchId),
                            branchName = item.PickupBranchName
                        },
                        dropoffBranch = new ClassLibrary._Tablet.Branch
                        {
                            branchId = Guid.Parse(item.DropoffBranchId),
                            branchName = item.DropoffBranchName
                        },
                        depositAmount = item.DepositAmount,
                        totalAmount = item.ReservationTotalAmount
                    });
                }
                catch (Exception)
                {
                    continue;
                }

            }
            return data;
        }
    }


}
