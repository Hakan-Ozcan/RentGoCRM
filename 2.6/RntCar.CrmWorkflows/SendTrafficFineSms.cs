using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Business;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmWorkflows
{
    public class SendTrafficFineSms : CodeActivity
    {
        [Input("Customer")]
        [ReferenceTarget("contact")]
        public InArgument<EntityReference> Customer { get; set; }
        
        [Input("PNR Number")]
        public InArgument<string> PNRNumber { get; set; }

        [Input("Amount")]
        public InArgument<Money> Amount { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                PluginInitializer initializer = new PluginInitializer(context);
                int contentCode = 0;
                int langId = 0;


                Dictionary<string, string> requirementList = new Dictionary<string, string>();


                EntityReference contactRef = Customer.Get(context);
                string pnrNumber = PNRNumber.Get(context);
                Money amount = Amount.Get(context);


                Entity contact = initializer.Service.Retrieve(contactRef.LogicalName, contactRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("firstname","lastname","mobilephone", "rnt_isturkishcitizen"));
                string firstName = contact.GetAttributeValue<string>("firstname");
                string lastName = contact.GetAttributeValue<string>("lastname");
                string mobilePhone = contact.GetAttributeValue<string>("mobilephone");
                bool isTurkishCitizen = contact.GetAttributeValue<bool>("rnt_isturkishcitizen");
                if (!string.IsNullOrWhiteSpace(mobilePhone))
                {
                    requirementList.Add("@MobilePhone", mobilePhone);
                    requirementList.Add("@FirstName", firstName);
                    requirementList.Add("@LastName", lastName);
                    requirementList.Add("@PNRNumber", pnrNumber);
                    requirementList.Add("@Amount", Convert.ToString(amount.Value));

                    contentCode = 2500;
                    if(isTurkishCitizen)
                    {
                        langId = 1055;
                    }
                    else
                    {
                        langId = 1033;
                    }

                    //IBU - getSMSContentByCodeandLangId üzerinde mesajımız hazırlanıp bize dönüyor.
                    SMSContentBL smsContentBL = new SMSContentBL(initializer.Service);
                    var message = smsContentBL.getSMSContentByCodeandLangId(contentCode, langId, requirementList);

                    //IBU - ExecuteSendSMS Action 
                    OrganizationRequest request = new OrganizationRequest("rnt_executesendsms");
                    request["Message"] = message;
                    request["MobilePhone"] = mobilePhone;
                    request["LangId"] = langId;

                    var response = initializer.Service.Execute(request);

                    initializer.TraceMe("end");
                }
                
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
