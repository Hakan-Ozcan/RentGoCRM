using Microsoft.AspNet.OData;
using RntCar.ClassLibrary.odata;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RntCar.MongoDBWebAPI.Controllers.VirtualEntity
{
    public class DailyPricesController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get(string reservationItemId)
        {
            List<DailyPriceDataMongoDB> dailyPriceDataMongoDBs = new List<DailyPriceDataMongoDB>();
            DailyPricesRepository dailyPricesRepository = new DailyPricesRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                    StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            if (string.IsNullOrEmpty(reservationItemId))
            {
                dailyPriceDataMongoDBs = dailyPricesRepository.getDailyPrices();
            }
            else
            {
                dailyPriceDataMongoDBs = dailyPricesRepository.getDailyPricesByReservationItemId_str(new Guid(reservationItemId));
            }


            var dailyPrices = new List<DailyPrice>();
            foreach (var item in dailyPriceDataMongoDBs)
            {
                DailyPrice p = new DailyPrice();
                p = p.Map(item);
                p.dailyPricesId = new Guid(item.ID);
                dailyPrices.Add(p);
            }
            return Ok(dailyPrices);
        }
    }
}
