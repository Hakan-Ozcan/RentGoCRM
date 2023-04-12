using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.EnableCors();

            config.Routes.MapHttpRoute(
               name: "ActionApi",
               routeTemplate: "api/{controller}/{action}");
        }
    }
}
