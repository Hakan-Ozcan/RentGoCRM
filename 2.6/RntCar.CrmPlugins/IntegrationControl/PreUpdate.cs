using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RntCar.ClassLibrary.GlobalEnums;

namespace RntCar.CrmPlugins.IntegrationControl
{
    public class PreUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                Entity target;
                Entity preImage;


                initializer.PluginContext.GetContextInputEntity(initializer.TargetKey, out target);
                initializer.PluginContext.GetContextPostImages(initializer.PreImgKey, out preImage);

                if (target.Contains("statuscode"))
                {
                    OptionSetValue statecode = target.GetAttributeValue<OptionSetValue>("statuscode");
                    if (statecode.Value == (int)IntegrationStatusCode.Testing)
                    {
                        Entity integrationControl = initializer.Service.Retrieve(target.LogicalName, target.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("statuscode", "rnt_url", "rnt_methodname", "rnt_methodtype", "rnt_retrycountdefiniton", "rnt_retrycount", "rnt_emaillist", "rnt_periodtime", "rnt_lasttime", "rnt_extrafields"));
                        string URL = integrationControl.GetAttributeValue<string>("rnt_url");
                        string methodName = integrationControl.GetAttributeValue<string>("rnt_methodname");
                        int retryCountDefiniton = integrationControl.GetAttributeValue<int>("rnt_retrycountdefiniton");
                        int retryCount = integrationControl.GetAttributeValue<int>("rnt_retrycount");
                        string emailList = integrationControl.GetAttributeValue<string>("rnt_emaillist");
                        OptionSetValue methodType = integrationControl.GetAttributeValue<OptionSetValue>("rnt_methodtype");
                        int periodTime = integrationControl.GetAttributeValue<int>("rnt_periodtime");
                        DateTime lastTime = integrationControl.GetAttributeValue<DateTime>("rnt_lasttime");
                        string extraFields = integrationControl.GetAttributeValue<string>("rnt_extrafields");

                        IntegrationControlBL integrationControlBL = new IntegrationControlBL(initializer.Service, initializer.TracingService);
                        var response = integrationControlBL.checkIntegrationMethod(URL, methodName, methodType.Value, extraFields);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                           response.StatusCode == System.Net.HttpStatusCode.Created ||
                           response.StatusCode == System.Net.HttpStatusCode.Accepted ||
                           response.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            target["rnt_lasttime"] = DateTime.UtcNow;
                            target["rnt_nexttime"] = DateTime.UtcNow.AddMinutes(periodTime);
                            target["statuscode"] = new OptionSetValue((int)IntegrationStatusCode.Success);
                            target["rnt_responsedetail"] = $"StatusCode: {Convert.ToString(response.StatusCode)}";
                            target["rnt_retrycount"] = 0;
                        }
                        else
                        {
                            retryCount++;
                            target["rnt_lasttime"] = DateTime.UtcNow;
                            target["rnt_nexttime"] = DateTime.UtcNow.AddMinutes(periodTime);
                            target["statuscode"] = new OptionSetValue((int)IntegrationStatusCode.Failed);
                            target["rnt_responsedetail"] = $"StatusCode: {Convert.ToString(response.StatusCode)} Value: {(int)response.StatusCode}  ExceptionDetail :{response.ErrorMessage}";
                            target["rnt_retrycount"] = retryCount;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
