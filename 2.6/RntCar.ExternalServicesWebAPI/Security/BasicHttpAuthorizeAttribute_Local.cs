using RntCar.SDK.Common;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.Filters;

namespace RntCar.ExternalServices.Security
{
    public class BasicHttpAuthorizeAttribute_Local : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            if (actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            else
            {
                // Gets header parameters  
                string authenticationString = actionContext.Request.Headers.Authorization.Parameter;
                string originalString = Encoding.UTF8.GetString(Convert.FromBase64String(authenticationString));

                // Gets username and password  
                string username = originalString.Split(':')[0];
                string password = originalString.Split(':')[1];

                // Validate username and password  
                if (!this.validateUser(username, password))
                {
                    // returns unauthorized error  
                    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
            }

            base.OnAuthorization(actionContext);
        }
        private bool validateUser(string userName, string password)
        {
            var _userName = StaticHelper.GetConfiguration("service_username");
            var _password = StaticHelper.GetConfiguration("service_password");
            if(userName.Equals(_userName) && password.Equals(_password))
            {
                return true;
            }
            return false;
        }
    }
}