using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using RntCar.ClassLibrary.odata;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBWebAPI.Models.odata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace RntCar.MongoDBWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
               name: "ActionApi",
               routeTemplate: "api/{controller}/{action}");

            config.MapODataServiceRoute(routeName: "ODataRoute",
            routePrefix: "odata",
            model: GetEdmModel());
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
            config.EnsureInitialized();
        }
        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "odata";
            builder.ContainerName = "default";            
            builder.EntitySet<DailyPrice>("DailyPrices");
            builder.EntitySet<DailyPrice>("ContractDailyPrices");
            builder.EntitySet<CouponCode>("GetCouponCodes");

            builder.EntitySet<CouponCode>("GetCouponCodesByReservationId");
            builder.EntitySet<CouponCode>("GetCouponCodesByContractId");
            builder.EntitySet<CouponCode>("GetCouponCodesByDefinitionId");
            builder.EntitySet<CouponCode>("GetCouponCodesByContactId");
            builder.EntitySet<CouponCode>("GetCouponCodesByAccountId");

            return builder.GetEdmModel();
        }
    }
}
