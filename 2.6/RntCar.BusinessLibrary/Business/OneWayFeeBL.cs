using Microsoft.Xrm.Sdk;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Business
{
    public class OneWayFeeBL : BusinessHandler
    {
        public OneWayFeeBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public OneWayFeeBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public MongoDBResponse CreateOneWayFeeInMongoDB(Entity entity)
        {
            Entity oneWayFee= entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateOneWayFeeInMongoDB", Method.POST);

            var oneWayFeeData = this.BuildOneWayFeeData(oneWayFee);
            this.Trace("after build class");
            restSharpHelper.AddJsonParameter<OneWayFeeData>(oneWayFeeData);

            var responseGroupCodeListPrice = restSharpHelper.Execute<MongoDBResponse>();

            if (!responseGroupCodeListPrice.Result)
            {
                return MongoDBResponse.ReturnError(responseGroupCodeListPrice.ExceptionDetail);
            }

            return responseGroupCodeListPrice;
        }
        public MongoDBResponse UpdateOneWayFeeInMongoDB(Entity entity)
        {
            Entity oneWayFee = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateOneWayFeeInMongoDB", Method.POST);

            var oneWayFeeData = this.BuildOneWayFeeData(oneWayFee);
            this.Trace("after build class");
            restSharpHelper.AddJsonParameter<OneWayFeeData>(oneWayFeeData);
            restSharpHelper.AddQueryParameter("id", entity.GetAttributeValue<string>("rnt_mongodbid"));

            var responseGroupCodeListPrice = restSharpHelper.Execute<MongoDBResponse>();

            if (!responseGroupCodeListPrice.Result)
            {
                return MongoDBResponse.ReturnError(responseGroupCodeListPrice.ExceptionDetail);
            }

            return responseGroupCodeListPrice;
        }

        public decimal getOneWayFee(Guid pickupBranchId, Guid dropoffBranchId)
        {
            OneWayFeeRepository oneWayFeeRepository = new OneWayFeeRepository(this.OrgService);

            var entity = oneWayFeeRepository.getOneWayFeeByPickupandDropoffBranch(pickupBranchId, dropoffBranchId);
            if(entity == null)
            {
                return decimal.Zero;
            }
            var amount = entity.Attributes.Contains("rnt_price") ?
                         entity.GetAttributeValue<Money>("rnt_price").Value :
                         decimal.Zero;

            return amount;
        }
        public OneWayFeeData BuildOneWayFeeData(Entity oneWayFee)
        {

            OneWayFeeData oneWayFeeData = new OneWayFeeData
            {
                beginDate = oneWayFee.GetAttributeValue<DateTime>("rnt_begindate"),
                endDate = oneWayFee.GetAttributeValue<DateTime>("rnt_enddate"),
                isEnabled = oneWayFee.GetAttributeValue<bool>("rnt_isenabled"),
                OneWayFeeId = Convert.ToString(oneWayFee.Id),
                Name = oneWayFee.GetAttributeValue<string>("rnt_name"),
                CreatedBy = Convert.ToString(oneWayFee.GetAttributeValue<EntityReference>("createdby").Id),
                ModifiedBy = Convert.ToString(oneWayFee.GetAttributeValue<EntityReference>("modifiedby").Id),
                CreatedOn = oneWayFee.GetAttributeValue<DateTime>("createdon"),
                ModifiedOn = oneWayFee.GetAttributeValue<DateTime>("modifiedon"),
                DropoffBranchId = Convert.ToString(oneWayFee.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id),
                DropoffBranchName = oneWayFee.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Name,
                PickUpBranchId = Convert.ToString(oneWayFee.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id),
                PickUpBranchName = oneWayFee.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name,
                StatusCode = oneWayFee.GetAttributeValue<OptionSetValue>("statuscode").Value,
                StateCode = oneWayFee.GetAttributeValue<OptionSetValue>("statecode").Value,
                Price = oneWayFee.GetAttributeValue<Money>("rnt_price").Value,
            };
            return oneWayFeeData;
        }
    }
}
