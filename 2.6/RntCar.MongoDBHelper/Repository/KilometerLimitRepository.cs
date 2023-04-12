
using MongoDB.Driver;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.MongoDBHelper.Repository
{
    public class KilometerLimitRepository : MongoDBInstance
    {
        public KilometerLimitRepository(string serverName, string dataBaseName) : base(serverName, dataBaseName)
        {
        }

        public KilometerLimitRepository(object client, object database) : base(client, database)
        {
        }
        public KilometerLimitDataMongoDB getMonthlyKilometerLimitsByGroupCode(Guid groupCodeInformationId)
        {
            return this._database.GetCollection<KilometerLimitDataMongoDB>("KilometerLimits")
                       .AsQueryable()
                       .Where(p => p.groupCodeInformationId.Equals(groupCodeInformationId) && 
                              p.durationCode == (int)rnt_kilometerlimit_rnt_durationcode.Monthly &&
                              p.statecode == (int)GlobalEnums.StateCode.Active).FirstOrDefault();

        }

        public KilometerLimitDataMongoDB getDailyKilometerLimitsByReservationDayandGroupCode(int reservationDuration, Guid groupCodeInformationId)
        {
            return this._database.GetCollection<KilometerLimitDataMongoDB>("KilometerLimits")
                      .AsQueryable()
                      .Where(p => p.groupCodeInformationId.Equals(groupCodeInformationId) &&
                             p.durationCode == (int)rnt_kilometerlimit_rnt_durationcode.Daily &&
                             p.maximumDay == reservationDuration &&
                             p.statecode == (int)GlobalEnums.StateCode.Active).FirstOrDefault();           
           
        }

    }
}
