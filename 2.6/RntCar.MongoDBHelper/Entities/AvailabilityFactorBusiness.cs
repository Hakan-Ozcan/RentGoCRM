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

namespace RntCar.MongoDBHelper.Entities
{
    public class AvailabilityFactorBusiness : MongoDBInstance
    {
        private string collectionName { get; set; }

        public AvailabilityFactorBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
            collectionName = StaticHelper.GetConfiguration("MongoDBAvailabilityFactorCollectionName");
        }

        public MongoDBResponse CreateAvailabilityFactor(AvailabilityFactorData availabilityFactorData)
        {
            var collection = this.getCollection<AvailabilityFactorDataMongoDB>(collectionName);

            var availabilityFactor = new AvailabilityFactorDataMongoDB();
            availabilityFactor.ShallowCopy(availabilityFactorData);
            availabilityFactor._id = ObjectId.GenerateNewId();

            availabilityFactor.beginDateTimeStamp = new BsonTimestamp(availabilityFactorData.beginDate.converttoTimeStamp());
            availabilityFactor.endDateTimeStamp = new BsonTimestamp(availabilityFactorData.endDate.converttoTimeStamp());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = availabilityFactor._id.ToString();

            var response = collection.Insert(availabilityFactor, itemId, methodName);
            response.Id = Convert.ToString(availabilityFactor._id);

            return response;
        }

        public bool UpdateAvailabilityFactor(AvailabilityFactorData availabilityFactorData, string id)
        {
            var collection = this.getCollection<AvailabilityFactorDataMongoDB>(collectionName);

            var availabilityFactor = new AvailabilityFactorDataMongoDB();
            availabilityFactor.ShallowCopy(availabilityFactorData);
            availabilityFactor._id = ObjectId.Parse(id);

            availabilityFactor.beginDateTimeStamp = new BsonTimestamp(availabilityFactorData.beginDate.converttoTimeStamp());
            availabilityFactor.endDateTimeStamp = new BsonTimestamp(availabilityFactorData.endDate.converttoTimeStamp());

            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var itemId = availabilityFactor._id.ToString();

            var filter = Builders<AvailabilityFactorDataMongoDB>.Filter.Eq(p => p._id, availabilityFactor._id);
            var response = collection.Replace(availabilityFactor, filter, new UpdateOptions { IsUpsert = false }, itemId, methodName);
            //var response = collection.ReplaceOne(p => p._id == availabilityFactor._id, availabilityFactor, new UpdateOptions { IsUpsert = false });
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

       

        public List<Guid> applyAvailabilityClosure(DateTime pickupDateTime,
                                                   DateTime dropoffDateTime,
                                                   string pickupBranchId,
                                                   int channel,
                                                   int? customerType,
                                                   string accountFactorType)
        {
            List<Guid> closedGroupCodes = new List<Guid>();

            pickupBranchId = pickupBranchId.ToLower().Replace("{", "").Replace("}", "");

            AvailabilityFactorRepository availabilityFactorRepository = new AvailabilityFactorRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                         StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var availabilityFactors = availabilityFactorRepository.getActiveAvailabilityClosureAvailabilityFactorByGivenChannel(pickupDateTime, dropoffDateTime, channel, customerType, accountFactorType);

            foreach (var availabilityFactor in availabilityFactors)
            {
                if (availabilityFactor != null)
                {
                    var branchIsInList = availabilityFactor.branchValues.Contains(pickupBranchId);

                    if (branchIsInList)
                    {
                        var groupCodes = availabilityFactor.groupCodeValues != null ? availabilityFactor.groupCodeValues.Split(',') : new string[] { };

                        closedGroupCodes.AddRange(groupCodes.ToList().ConvertAll(p => new Guid(p.removeAlphaNumericCharactersFromString())).ToList());
                    }
                }
            }

            return closedGroupCodes;
        }

        private string getErrorMessageforEarlistTime(double totalMinutes)
        {
            int hours = (int)(totalMinutes/60);
            int minutes = (int)(totalMinutes % 60);

            if(minutes == 0)
            {
                return string.Format("Seçtiğiniz şubemizden en az {0} saat sonrasına rezervasyon yapabilirsiniz.", hours);
            }

            return string.Format("Seçtiğiniz şubemizden en az {0} saat {1} dakika sonrasına rezervasyon yapabilirsiniz.", hours, minutes);
        }

        public AvailabilityResponse checkAvailability(AvailabilityParameters availabilityParameters, int langId, int channel,int? customerType , string accountGroup)
        {
            BusinessClosureRepository businessClosureRepository = new BusinessClosureRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                               StaticHelper.GetConfiguration("MongoDBDatabaseName"));


            var res = businessClosureRepository.getBusinessClosureByDate(availabilityParameters.pickupBranchId.ToString(),
                                                                         availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset),
                                                                         availabilityParameters.dropoffDateTime.AddMinutes(StaticHelper.offset));

            if (res.Count > 0)
            {
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError("Ofis seçilen tarihler arasında çalışmamaktadır.. Detaylı bilgi için 444 40 61 numaralı telefondan rezervasyon merkezimiz ile iletişime geçmenizi rica ederiz.")
                };
            }

            if (availabilityParameters.earlistPickupTime.HasValue && availabilityParameters.pickupDateTime < DateTime.UtcNow.AddMinutes(availabilityParameters.earlistPickupTime.Value))
            {
                TimeSpan ts = TimeSpan.FromMinutes(availabilityParameters.earlistPickupTime.Value);

                string error = getErrorMessageforEarlistTime(ts.TotalMinutes);
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError(error)
                };
            }
            OneWayFeeRepository oneWayFeeRepository = new OneWayFeeRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                             StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            var oneWay = oneWayFeeRepository.getOneWayFee(Convert.ToString(availabilityParameters.pickupBranchId),
                                                          Convert.ToString(availabilityParameters.dropOffBranchId));

            if (oneWay != null)
            {
                if (oneWay.isEnabled &&
                    availabilityParameters.pickupDateTime.IsBetween(oneWay.beginDate, oneWay.endDate) &&
                    availabilityParameters.dropoffDateTime.IsBetween(oneWay.beginDate, oneWay.endDate))
                {
                    return new AvailabilityResponse
                    {
                        ResponseResult = ResponseResult.ReturnError("Seçilen ofisler arasında araç iadesi yapılmamaktadır. Detaylı bilgi için 444 40 61 numaralı telefondan rezervasyon merkezimiz ile iletişime geçmenizi rica ederiz.")
                    };
                }
            }

            AvailabilityFactorRepository availabilityFactorRepository = new AvailabilityFactorRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                         StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var availabilityFactor = availabilityFactorRepository.getMinimumReservationDay(availabilityParameters.pickupDateTime,
                                                                                            availabilityParameters.dropoffDateTime,
                                                                                            Convert.ToString(availabilityParameters.pickupBranchId),
                                                                                            channel,
                                                                                            customerType,
                                                                                            accountGroup);
            if(availabilityFactor == null)
            {
                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
                
            }
            
            var minDuration = availabilityFactor.minimumReservationDuration;
            DurationHelper durationHelper = new DurationHelper(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                               StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var resDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(availabilityParameters.pickupDateTime.AddMinutes(StaticHelper.offset),
                                                                              availabilityParameters.dropoffDateTime.AddMinutes(StaticHelper.offset));


            var resDurationInMinutes = resDuration * 60 * 24;
            var minDurationInMinutes = minDuration * 60 * 24;
            if (minDurationInMinutes > resDurationInMinutes)
            {

                var hours = decimal.Round((decimal)minDurationInMinutes / 1440, 2);


                return new AvailabilityResponse
                {
                    ResponseResult = ResponseResult.ReturnError(string.Format("En az {0} günlük rezervasyon yapabilirsiniz.", hours))
                };
            };           

            return new AvailabilityResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
    }
}
