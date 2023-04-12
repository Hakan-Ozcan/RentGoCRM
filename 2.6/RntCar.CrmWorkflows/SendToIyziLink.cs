using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using RntCar.BusinessLibrary.Business;
using RntCar.PaymentHelper.iyzico;
using RntCar.PaymentHelper.iyzico.Model;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmWorkflows
{
    public class SendToIyziLink : CodeActivity
    {
        [Input("Total Amount")]
        public InArgument<Money> TotalAmount { get; set; }

        [Input("Contract PnrNumber")]
        public InArgument<string> PnrNumber { get; set; }
        
        [Input("Customer Name")]
        public InArgument<string> FirstName { get; set; }

        [Input("Customer LastName")]
        public InArgument<string> LastName { get; set; }

        [Input("Customer Mobile Phone")]
        public InArgument<string> MobilePhone { get; set; }

        [Input("Is TurkishvCitizen")]
        public InArgument<bool> IsTurkishCitizen { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            try
            {
                PluginInitializer initializer = new PluginInitializer(context);

                var pnrNumber = PnrNumber.Get<string>(context);
                var totalAmount = TotalAmount.Get<Money>(context);
                var firstName = FirstName.Get<string>(context);
                var lastName = LastName.Get<string>(context);
                var mobilePhone = MobilePhone.Get<string>(context);
                var isTurkishCitizen = IsTurkishCitizen.Get<bool>(context);

                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                var iyziLinkDescriptionTR = configurationBL.GetConfigurationByName("iyziLinkDescriptionTR");
                var iyziLinkDescriptionEN = configurationBL.GetConfigurationByName("iyziLinkDescriptionEN");
                var iyziLinkPicture = configurationBL.GetConfigurationByName("iyziLinkPicture");
                //parse configs
                var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                IyzicoHelper iyzicoHelper = new IyzicoHelper(configs);

                var description = iyziLinkDescriptionTR.Replace("@PnrNumber", pnrNumber);
                int langId = 1055;
                if (!isTurkishCitizen)
                {
                    description = iyziLinkDescriptionEN.Replace("@PnrNumber", pnrNumber);
                    langId = 1033;
                }


                string iyziLinkURL = string.Empty;
                string converstionId = string.Empty;

                QueryExpression getIyziLinkQuery = new QueryExpression("rnt_iyzilinktransaction");
                getIyziLinkQuery.ColumnSet = new ColumnSet(true);
                getIyziLinkQuery.Criteria.AddCondition("rnt_paymentid",ConditionOperator.Null);
                getIyziLinkQuery.Criteria.AddCondition("rnt_amount", ConditionOperator.Equal, totalAmount.Value);
                EntityCollection iyziLinkList = initializer.Service.RetrieveMultiple(getIyziLinkQuery);
                if(iyziLinkList.Entities.Count>0)
                {
                    Entity temp = iyziLinkList.Entities[0];
                    iyziLinkURL = temp.GetAttributeValue<string>("rnt_iyzilink");
                    converstionId = temp.GetAttributeValue<string>("rnt_conversationid");
                }
                else
                {
                    FastLinkRequest fastLinkRequest = new FastLinkRequest()
                    {
                        ConversationId = StaticHelper.RandomDigits(10),
                        Name = pnrNumber,
                        Description = description,
                        Base64EncodedImage = iyziLinkPicture,
                        Price = Convert.ToString(totalAmount.Value),
                        Currency = "TRY",
                    };
                    var response = iyzicoHelper.CreateFastLink(fastLinkRequest);
                    iyziLinkURL = response.url;
                    converstionId = fastLinkRequest.ConversationId;


                    Entity iyziLinkTransaction = new Entity("rnt_iyzilinktransaction");
                    iyziLinkTransaction.Attributes["rnt_conversationid"] = converstionId;
                    iyziLinkTransaction.Attributes["rnt_amount"] = new Money(totalAmount.Value);
                    iyziLinkTransaction.Attributes["rnt_iyzilink"] = iyziLinkURL;
                    iyziLinkTransaction.Attributes["rnt_contractid"] = new EntityReference("rnt_contract", initializer.PrimaryId);
                    initializer.Service.Create(iyziLinkTransaction);
                }
                

                
                if (!string.IsNullOrWhiteSpace(iyziLinkURL))
                {
                    Dictionary<string, string> requirementList = new Dictionary<string, string>();
                    requirementList.Add("@FirstName", firstName);
                    requirementList.Add("@LastName", lastName);
                    requirementList.Add("@TotalAmount", Convert.ToString(totalAmount.Value));
                    requirementList.Add("@Iyzilink", iyziLinkURL);


                    //IBU - getSMSContentByCodeandLangId üzerinde mesajımız hazırlanıp bize dönüyor.
                    SMSContentBL smsContentBL = new SMSContentBL(initializer.Service);
                    var message = smsContentBL.getSMSContentByCodeandLangId(2400, langId, requirementList);

                    //IBU - ExecuteSendSMS Action 
                    OrganizationRequest request = new OrganizationRequest("rnt_executesendsms");
                    request["Message"] = message;
                    request["MobilePhone"] = mobilePhone;
                    request["LangId"] = langId;
                    var responseSms = initializer.Service.Execute(request);
                }
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
