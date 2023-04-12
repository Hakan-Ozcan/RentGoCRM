using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace RntCar.ExternalServicesWebAPI.Security
{
    public class LogoBasicHttpAuthorizeAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if(actionContext.Request.Headers.Authorization == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }
            else
            {
                string authenticationString = actionContext.Request.Headers.Authorization.Parameter;
                string originalString = Encoding.UTF8.GetString(Convert.FromBase64String(authenticationString));

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
            var _userName = StaticHelper.GetConfiguration("logoServiceUserName");
            var _password = StaticHelper.GetConfiguration("logoServicePassword");
            if (userName.Equals(_userName) && password.Equals(_password))
            {
                return true;
            }
            return false;
        }
    }
}