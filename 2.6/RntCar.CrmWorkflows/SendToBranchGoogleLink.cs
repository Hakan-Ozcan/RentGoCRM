using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;
using System.Activities;
using System.Collections;
using System.Collections.Generic;

namespace RntCar.CrmWorkflows
{
    public class SendToBranchGoogleLink : CodeActivity
    {
        [Input("PickupBranch")]
        [ReferenceTarget("rnt_branch")]
        public InArgument<EntityReference> PickupBranch { get; set; }

        [Input("Contract PnrNumber")]
        public InArgument<string> PnrNumber { get; set; }

        [Input("Contract Number")]
        public InArgument<string> ContractNumber { get; set; }

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
                var contractId = initializer.WorkflowContext.PrimaryEntityId;

                var pnrNumber = PnrNumber.Get<string>(context);
                var firstName = FirstName.Get<string>(context);
                var lastName = LastName.Get<string>(context);
                var mobilePhone = MobilePhone.Get<string>(context);
                var isTurkishCitizen = IsTurkishCitizen.Get<bool>(context);
                var pickupBranchRef = PickupBranch.Get<EntityReference>(context);
                var contractNumber = PickupBranch.Get<string>(context);

                int contentCode = 0;
                int langId = 1055;
                if (!isTurkishCitizen)
                {
                    langId = 1033;
                }

                initializer.TraceMe("started : " + contractId);

                Dictionary<string, string> requirementList = new Dictionary<string, string>();

                //IBU Primary Entity Contract - İhtiyacımız olan bilgilere contract üzerinden ulaşacağız.
                ContractRepository contractRepository = new ContractRepository(initializer.Service);

                //IBU - PnrNumber Dictionary içerisine ekliyoruz. ContractNumber'a ihtiyaç olabileceği bildirildi.
                requirementList.Add("PnrNumber", pnrNumber);
                requirementList.Add("ContractNumber", contractNumber);

                initializer.TraceMe(contractId.ToString());

                //IBU - Contract verilerini kullanarak pickup ve dropoff branch bilgilerini alıyoruz. Burası geliştirilecek..
                BranchRepository branchRepository = new BranchRepository(initializer.Service);
                var pickupBranch = branchRepository.getBranchById(pickupBranchRef.Id);

                initializer.TraceMe("Customer Search");

                //IBU - Contract verilerini kullanarak Customer bilgisine ulaşıyoruz. Burası geliştirilecek..
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(initializer.Service);


                initializer.TraceMe("Field Mapping");

                //IBU - Müşteri(Contact) bilgilerini alıp Dictionary içerisine ekliyoruz.
                requirementList.Add("@MobilePhone", mobilePhone);
                requirementList.Add("@FirstName", firstName);
                requirementList.Add("@LastName", lastName);


                var oneGoogleUrl = pickupBranch.GetAttributeValue<string>("rnt_googlelink");
                requirementList.Add("@OneGoogleUrl", oneGoogleUrl);
                contentCode = 2300;

                initializer.TraceMe("Generate SMS Message");
                //IBU - getSMSContentByCodeandLangId üzerinde mesajımız hazırlanıp bize dönüyor.
                SMSContentBL smsContentBL = new SMSContentBL(initializer.Service,initializer.TracingService);
                var message = smsContentBL.getSMSContentByCodeandLangId(contentCode, langId, requirementList);

                initializer.TraceMe("Execute SMS");
                //IBU - ExecuteSendSMS Action 
                OrganizationRequest request = new OrganizationRequest("rnt_executesendsms");
                request["Message"] = message;
                request["MobilePhone"] = mobilePhone;
                request["LangId"] = langId;

                var response = initializer.Service.Execute(request);

                initializer.TraceMe("end");
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
