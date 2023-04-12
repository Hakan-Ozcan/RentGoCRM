using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Activities;

namespace RntCar.CrmWorkflows
{
    public class SendSMSVIPCustomer : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            PluginInitializer initializer = new PluginInitializer(context);
            initializer.TraceMe("initializer.WorkflowContext.PrimaryEntityId : " + initializer.WorkflowContext.PrimaryEntityId);

            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(initializer.Service);
            var ind = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(initializer.WorkflowContext.PrimaryEntityId, new string[] { "mobilephone", "rnt_segmentcode" , "rnt_citizenshipid" ,"firstname","lastname" });

            string dailCode = ind.GetAttributeValue<string>("rnt_dialcode");
            string mobilePhone = ind.GetAttributeValue<string>("mobilephone");
            string mobilePhoneWithDialCode = dailCode + mobilePhone;
            var segmentCode = ind.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value;
            
            initializer.TraceMe("segmentCode : " + segmentCode);

            if(segmentCode == (int)rnt_CustomerSegment.VIP10 ||
               segmentCode == (int)rnt_CustomerSegment.VIP15)
            {

                var langId = 1055;

                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid").Split(';')[0];

                if(new Guid(turkeyGuid) != ind.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id)
                {
                    langId = 1033;
                }

                SMSContentBL smsContentBl = new SMSContentBL(initializer, initializer.Service, initializer.TracingService);
                
                var message = smsContentBl.getSMSContentByCodeandLangId(1100, 
                                            langId,
                                            mobilePhoneWithDialCode,
                                            string.Empty,
                                            string.Empty,
                                            ind.GetAttributeValue<string>("firstname"),
                                            ind.GetAttributeValue<string>("lastname"));

                initializer.TraceMe("message : " + message);

                SMSBL smsBl = new SMSBL(initializer.Service);
                smsBl.sendSMS(ind.GetAttributeValue<string>("mobilephone"), message);
            }

            

        }
    }
}
