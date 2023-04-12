using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary.Campaign.Pegasus;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Business
{
    public class PegasusBL : BusinessHandler
    {
        public PegasusBL()
        {
        }

        public PegasusBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public PegasusBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public PegasusMemberResponse checkMemberShip(CheckPegasusMembershipRequest checkPegasusMembershipRequest)
        {

            this.Trace("authBaseUrl  :" + checkPegasusMembershipRequest.authBaseUrl);
            RestSharpHelper helper = new RestSharpHelper(checkPegasusMembershipRequest.authBaseUrl, "protocol/openid-connect/token", Method.POST);
            helper.AddParameter("grant_type", "password");
            helper.AddParameter("client_id", "account");
            helper.AddParameter("client_secret", checkPegasusMembershipRequest.PegasusAuthValues.Split(';')[0]);
            helper.AddParameter("username", checkPegasusMembershipRequest.PegasusAuthValues.Split(';')[1]);
            helper.AddParameter("password", checkPegasusMembershipRequest.PegasusAuthValues.Split(';')[2]);

            var response = helper.Execute<PegasusAuthReponse>();


            this.Trace("bolbolurl  :" + checkPegasusMembershipRequest.bolbolurl);

            RestSharpHelper memberHelper = new RestSharpHelper(checkPegasusMembershipRequest.bolbolurl, "MembershipDataConfirmation", Method.POST);
            memberHelper.RestRequest.AddHeader("kc-token", response.access_token);

            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
            var contact = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(checkPegasusMembershipRequest.customerId, new string[] { "firstname", "lastname", "mobilephone" });

            PegasusMembershipRequest pegasusMembershipRequest = new PegasusMembershipRequest
            {
                memberDataConfirmationType = new MemberDataConfirmationType
                {
                    memberContactNumberType = new MemberContactNumberType
                    {
                        phoneNumber = contact.GetAttributeValue<string>("mobilephone")
                    },
                    name = contact.GetAttributeValue<string>("firstname"),
                    surname = contact.GetAttributeValue<string>("lastname")
                },
            };
            this.Trace("member request  :" + JsonConvert.SerializeObject(pegasusMembershipRequest));
            memberHelper.AddJsonParameter<PegasusMembershipRequest>(pegasusMembershipRequest);
            return memberHelper.Execute<PegasusMemberResponse>();

        }
    }
}
