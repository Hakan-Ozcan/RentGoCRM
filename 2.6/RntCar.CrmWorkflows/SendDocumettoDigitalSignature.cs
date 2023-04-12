using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System.Activities;

namespace RntCar.CrmWorkflows
{
    public class SendDocumettoDigitalSignature : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            try
            {
                initializer.TraceMe("lets start");
                initializer.TraceMe("initializer.WorkflowContext.PrimaryEntityId : " + initializer.WorkflowContext.PrimaryEntityId);

                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service, initializer.TracingService);
                var url = configurationBL.GetConfigurationByName("ExternalCrmWebApiUrl_Document");

                initializer.TraceMe("webservice url : " + url);
                RestSharpHelper restSharpHelper = new RestSharpHelper(url, "execute", RestSharp.Method.POST);
                DocumentOperationParameters documentOperationParameters = new DocumentOperationParameters
                {
                    contractId = initializer.WorkflowContext.PrimaryEntityId,
                    fileName = initializer.WorkflowContext.PrimaryEntityId.ToString().Replace("-", "")
                };
                restSharpHelper.AddJsonParameter<DocumentOperationParameters>(documentOperationParameters);
                var response = restSharpHelper.Execute();

                initializer.TraceMe("response " + JsonConvert.SerializeObject(response));
            }
            catch (System.Exception ex)
            {

                initializer.TraceMe("exception detail " + ex.Message);
            }
            

        }
    }
}
