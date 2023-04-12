using MongoDB.Bson;
using MongoDB.Driver;
using RntCar.ClassLibrary.MongoDB;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Entities
{
    public class KilometerLimitBusiness : MongoDBInstance
    {
        public KilometerLimitBusiness(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public MongoDBResponse CreateKilometerLimit(KilometerLimitData kilometerLimitData)
        {
            var collection = this.getCollection<KilometerLimitData>(StaticHelper.GetConfiguration("MongoDBKilometerLimitCollectionName"));
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var response = collection.Insert(kilometerLimitData, kilometerLimitData.kilometerLimitId, methodName);
            response.Id = Convert.ToString(kilometerLimitData.kilometerLimitId);

            return response;
        }

        public bool UpdateKilometerLimit(KilometerLimitData kilometerLimitData, string id)
        {
            var collection = this.getCollection<KilometerLimitData>(StaticHelper.GetConfiguration("MongoDBKilometerLimitCollectionName"));
            var methodName = ErrorLogsHelper.GetCurrentMethod();
            var filter = Builders<KilometerLimitData>.Filter.Eq(p => p.kilometerLimitId, id);
            var response = collection.Replace(kilometerLimitData, filter, new UpdateOptions { IsUpsert = false }, id, methodName);

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

        public int getKilometerLimitForGivenDurationandGroupCode(int duration, Guid groupCodeInformationId)
        {
            var systemKilometerLimit = Convert.ToInt32(StaticHelper.GetConfiguration("kmlimit"));
            var kilometerLimit = 0;

            //means it is monthly
            while (true)
            {
                if (duration >= systemKilometerLimit)
                {
                    KilometerLimitRepository kilometerLimitRepository = new KilometerLimitRepository(this._client, this._database);
                    var kmLimit = kilometerLimitRepository.getMonthlyKilometerLimitsByGroupCode(groupCodeInformationId);
                    if (kmLimit != null)
                    {
                        kilometerLimit += kmLimit.kilometerLimit;
                    }

                }
                //means daily
                else
                {

                    KilometerLimitRepository kilometerLimitRepository = new KilometerLimitRepository(this._client, this._database);
                    var kmLimit = kilometerLimitRepository.getDailyKilometerLimitsByReservationDayandGroupCode(duration, groupCodeInformationId);
                    if (kmLimit != null)
                    {
                        kilometerLimit += kmLimit.kilometerLimit; ;
                    }
                }
                var quotient = Convert.ToInt32(duration / 30);
                if (quotient == 0)
                    break;
                quotient -= 1;
                duration -= 30;
            }

            return kilometerLimit;
        }
    }
}
