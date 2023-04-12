using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
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
    public class CouponCodeController : ODataController
    {
        [EnableQuery]
        [ODataRoute("GetCouponCodes")]
        public IHttpActionResult getCouponCodes()
        {
            List<CouponCodeDataMongoDB> couponCodeDataMongoDBs = new List<CouponCodeDataMongoDB>();
            CouponCodeRepository couponCodeRepository = new CouponCodeRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                 StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            couponCodeDataMongoDBs = couponCodeRepository.getCouponCodes();

            var couponCodes = new List<CouponCode>();
            foreach (var item in couponCodeDataMongoDBs)
            {
                CouponCode p = new CouponCode();
                p = p.Map(item);
                p.couponCodeId = string.IsNullOrEmpty(item.ID) ? Guid.NewGuid() : new Guid(item.ID);
                couponCodes.Add(p);
            }
            return Ok(couponCodes);
        }
        [EnableQuery]
        [ODataRoute("GetCouponCodesByReservationId")]
        public IHttpActionResult getCouponCodesByReservationId(string reservationId)
        {
            List<CouponCodeDataMongoDB> couponCodeDataMongoDBs = new List<CouponCodeDataMongoDB>();
            CouponCodeRepository couponCodeRepository = new CouponCodeRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                 StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            if (string.IsNullOrEmpty(reservationId))
            {
                couponCodeDataMongoDBs = couponCodeRepository.getCouponCodes();
            }
            else
            {
                couponCodeDataMongoDBs = couponCodeRepository.getCouponCodesByReservationId(new Guid(reservationId));
            }


            var couponCodes = new List<CouponCode>();
            foreach (var item in couponCodeDataMongoDBs)
            {
                CouponCode p = new CouponCode();
                p = p.Map(item);
                p.couponCodeId = string.IsNullOrEmpty(item.ID) ? Guid.NewGuid() : new Guid(item.ID);
                couponCodes.Add(p);
            }
            return Ok(couponCodes);
        }
        [EnableQuery]
        [ODataRoute("GetCouponCodesByContractId")]
        public IHttpActionResult getCouponCodesByContractId(string contractId)
        {
            List<CouponCodeDataMongoDB> couponCodeDataMongoDBs = new List<CouponCodeDataMongoDB>();
            CouponCodeRepository couponCodeRepository = new CouponCodeRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                 StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            if (string.IsNullOrEmpty(contractId))
            {
                couponCodeDataMongoDBs = couponCodeRepository.getCouponCodes();
            }
            else
            {
                couponCodeDataMongoDBs = couponCodeRepository.getCouponCodesByContractId(new Guid(contractId));
            }


            var couponCodes = new List<CouponCode>();
            foreach (var item in couponCodeDataMongoDBs)
            {
                CouponCode p = new CouponCode();
                p = p.Map(item);
                p.couponCodeId = string.IsNullOrEmpty(item.ID) ? Guid.NewGuid() : new Guid(item.ID);
                couponCodes.Add(p);
            }
            return Ok(couponCodes);
        }
        [EnableQuery]
        [ODataRoute("GetCouponCodesByDefinitionId")]
        public IHttpActionResult getCouponCodesByDefinitionId(string couponCodeDefinitionId)
        {
            List<CouponCodeDataMongoDB> couponCodeDataMongoDBs = new List<CouponCodeDataMongoDB>();
            CouponCodeRepository couponCodeRepository = new CouponCodeRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                 StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            if (string.IsNullOrEmpty(couponCodeDefinitionId))
            {
                couponCodeDataMongoDBs = couponCodeRepository.getCouponCodes();
            }
            else
            {
                couponCodeDataMongoDBs = couponCodeRepository.getCouponCodesByDefinitionId(new Guid(couponCodeDefinitionId));
            }


            var couponCodes = new List<CouponCode>();
            foreach (var item in couponCodeDataMongoDBs)
            {
                CouponCode p = new CouponCode();
                p = p.Map(item);
                p.couponCodeId = string.IsNullOrEmpty(item.ID) ? Guid.NewGuid() : new Guid(item.ID);
                couponCodes.Add(p);
            }
            return Ok(couponCodes);
        }
        [EnableQuery]
        [ODataRoute("GetCouponCodesByContactId")]
        public IHttpActionResult getCouponCodesByContactId(string contactId)
        {
            List<CouponCodeDataMongoDB> couponCodeDataMongoDBs = new List<CouponCodeDataMongoDB>();
            CouponCodeRepository couponCodeRepository = new CouponCodeRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                 StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            if (string.IsNullOrEmpty(contactId))
            {
                couponCodeDataMongoDBs = couponCodeRepository.getCouponCodes();
            }
            else
            {
                couponCodeDataMongoDBs = couponCodeRepository.getCouponCodesByContactId(new Guid(contactId));
            }


            var couponCodes = new List<CouponCode>();
            foreach (var item in couponCodeDataMongoDBs)
            {
                CouponCode p = new CouponCode();
                p = p.Map(item);
                p.couponCodeId = string.IsNullOrEmpty(item.ID) ? Guid.NewGuid() : new Guid(item.ID);
                couponCodes.Add(p);
            }
            return Ok(couponCodes);
        }
        [EnableQuery]
        [ODataRoute("GetCouponCodesByAccountId")]
        public IHttpActionResult getCouponCodesByAccountId(string accountId)
        {
            List<CouponCodeDataMongoDB> couponCodeDataMongoDBs = new List<CouponCodeDataMongoDB>();
            CouponCodeRepository couponCodeRepository = new CouponCodeRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                 StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            if (string.IsNullOrEmpty(accountId))
            {
                couponCodeDataMongoDBs = couponCodeRepository.getCouponCodes();
            }
            else
            {
                couponCodeDataMongoDBs = couponCodeRepository.getCouponCodesByAccountId(new Guid(accountId));
            }


            var couponCodes = new List<CouponCode>();
            foreach (var item in couponCodeDataMongoDBs)
            {
                CouponCode p = new CouponCode();
                p = p.Map(item);
                p.couponCodeId = string.IsNullOrEmpty(item.ID) ? Guid.NewGuid() : new Guid(item.ID);
                couponCodes.Add(p);
            }
            return Ok(couponCodes);
        }
    }
}
