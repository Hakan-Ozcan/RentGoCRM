using Microsoft.AspNet.OData;
using RntCar.ClassLibrary._Enums_1033;
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
    public class ContractDailyPricesController : ODataController
    {
        [EnableQuery]
        public IHttpActionResult Get(string contractItemId)
        {
            List<ContractDailyPriceDataMongoDB> dailyPriceDataMongoDBs = new List<ContractDailyPriceDataMongoDB>();
            ContractDailyPricesRepository dailyPricesRepository = new ContractDailyPricesRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                    StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            if (string.IsNullOrEmpty(contractItemId))
            {
                dailyPriceDataMongoDBs = dailyPricesRepository.getContractDailyPrices();
            }
            else
            {
                dailyPriceDataMongoDBs = dailyPricesRepository.getContractDailyPricesByReservationItemId_str(new Guid(contractItemId));
            }

            ContractItemRepository contractItemRepository = new ContractItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                       StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            var paymentMethod = 0;
            if (!string.IsNullOrEmpty(contractItemId))
            {
                var contract = contractItemRepository.getContractItemById(contractItemId);
                paymentMethod = contract.paymentChoice;
            }
            var dailyPrices = new List<DailyPrice>();
            foreach (var item in dailyPriceDataMongoDBs)
            {
                DailyPrice p = new DailyPrice();
                p = p.Map(item);
                p.priceAfterPayMethod = paymentMethod == (int)rnt_contract_rnt_paymentchoicecode.PayNow ? item.payNowWithoutTickDayAmount : item.payLaterWithoutTickDayAmount;
                p.dailyPricesId = string.IsNullOrEmpty(item.ID) ? Guid.NewGuid() : new Guid(item.ID);
                dailyPrices.Add(p);
            }
            return Ok(dailyPrices);
        }
    }
}
