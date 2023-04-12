﻿using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Report.Actions
{
    public class ExecuteGetComparedPlateNumbersReport : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer pluginInitializer = new PluginInitializer(serviceProvider);

            try
            {
                string branchId;
                pluginInitializer.PluginContext.GetContextInputParameter<string>("branchId", out branchId);
                
                string statusCodes;
                pluginInitializer.PluginContext.GetContextInputParameter<string>("statusCodes", out statusCodes);

                ReportBL reportBL = new ReportBL(pluginInitializer.Service);
                var serializedStatusCodes = JsonConvert.DeserializeObject<List<int>>(statusCodes);
                var response = reportBL.callComparePlateNumbersService(branchId, serializedStatusCodes);
                pluginInitializer.TraceMe("serializedStatusCodes :" + serializedStatusCodes);
                pluginInitializer.PluginContext.OutputParameters["ServiceResponse"] = JsonConvert.SerializeObject(response);
            }
            catch (Exception ex)
            {
                pluginInitializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
