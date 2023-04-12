using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Tablet;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class TransferBL : BusinessHandler
    {
        public TransferBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public TransferBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public TransferBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public TransferBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public TransferBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public MongoDBResponse createTransferInMongoDB(Entity transferEntity)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateTransferInMongoDB", Method.POST);

            var transferData = this.buildTransferData(transferEntity);
            this.Trace("after build class");
            restSharpHelper.AddJsonParameter<TransferData>(transferData);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        public MongoDBResponse updateTransferInMongoDB(Entity transferEntity)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateTransferInMongoDB", Method.POST);

            var transferData = this.buildTransferData(transferEntity);
            this.Trace("after build class");
            restSharpHelper.AddJsonParameter<TransferData>(transferData);
            restSharpHelper.AddQueryParameter("id", transferEntity.GetAttributeValue<string>("rnt_mongodbid"));

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        private TransferData buildTransferData(Entity transfer)
        {
            return new TransferData
            {
                transferId = Convert.ToString(transfer.Id),
                transferName = transfer.GetAttributeValue<string>("rnt_name"),
                serviceName = transfer.Attributes.Contains("rnt_servicename") ? transfer.GetAttributeValue<string>("rnt_servicename") : string.Empty,
                description = transfer.Attributes.Contains("rnt_description") ? transfer.GetAttributeValue<string>("rnt_description") : string.Empty,
                transferNumber = transfer.Attributes.Contains("rnt_transfernumber") ? transfer.GetAttributeValue<string>("rnt_transfernumber") : string.Empty,
                equipmentId = transfer.Attributes.Contains("rnt_equipmentid") ? Convert.ToString(transfer.GetAttributeValue<EntityReference>("rnt_equipmentid").Id) : string.Empty,
                equipmentName = transfer.Attributes.Contains("rnt_equipmentid") ? transfer.GetAttributeValue<EntityReference>("rnt_equipmentid").Name : string.Empty,
                groupCodeId = transfer.Attributes.Contains("rnt_groupcodeid") ? Convert.ToString(transfer.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id) : string.Empty,
                groupCodeName = transfer.Attributes.Contains("rnt_groupcodeid") ? transfer.GetAttributeValue<EntityReference>("rnt_groupcodeid").Name : string.Empty,
                pickupFuelCode = transfer.Attributes.Contains("rnt_pickupfuel") ? transfer.GetAttributeValue<OptionSetValue>("rnt_pickupfuel").Value : 0,
                pickupKilometer = transfer.Attributes.Contains("rnt_pickupkilometer") ? transfer.GetAttributeValue<int>("rnt_pickupkilometer") : 0,
                dropoffFuelCode = transfer.Attributes.Contains("rnt_dropofffuel") ? transfer.GetAttributeValue<OptionSetValue>("rnt_dropofffuel").Value : 0,
                dropoffKilometer = transfer.Attributes.Contains("rnt_dropoffkilometer") ? transfer.GetAttributeValue<int>("rnt_dropoffkilometer") : 0,
                transferTypeCode = transfer.Attributes.Contains("rnt_transfertype") ? transfer.GetAttributeValue<OptionSetValue>("rnt_transfertype").Value : 0,
                pickupBranchId = transfer.Attributes.Contains("rnt_pickupbranchid") ? Convert.ToString(transfer.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id) : string.Empty,
                pickupBranchName = transfer.Attributes.Contains("rnt_pickupbranchid") ? transfer.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name : string.Empty,
                dropoffBranchId = transfer.Attributes.Contains("rnt_dropoffbranchid") ? Convert.ToString(transfer.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id) : string.Empty,
                dropoffBranchName = transfer.Attributes.Contains("rnt_dropoffbranchid") ? transfer.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Name : string.Empty,
                estimatedPickupDate = transfer.GetAttributeValue<DateTime>("rnt_estimatedpickupdate").AddMinutes(-StaticHelper.offset),
                estimatedDropoffDate = transfer.GetAttributeValue<DateTime>("rnt_estimateddropoffdate").AddMinutes(-StaticHelper.offset),
                actualPickupDate = transfer.Attributes.Contains("rnt_actualpickupdate") ? (DateTime?)transfer.GetAttributeValue<DateTime>("rnt_actualpickupdate").AddMinutes(-StaticHelper.offset) : null,
                actualDropoffDate = transfer.Attributes.Contains("rnt_actualdropoffdate") ? (DateTime?)transfer.GetAttributeValue<DateTime>("rnt_actualdropoffdate").AddMinutes(-StaticHelper.offset) : null,
                estimatedPickupDateTimeStamp = (transfer.GetAttributeValue<DateTime>("rnt_estimatedpickupdate").AddMinutes(-StaticHelper.offset)).converttoTimeStamp(),
                estimatedDropoffDateTimeStamp = (transfer.GetAttributeValue<DateTime>("rnt_estimateddropoffdate").AddMinutes(-StaticHelper.offset)).converttoTimeStamp(),
                actualPickupDateTimeStamp = transfer.Attributes.Contains("rnt_actualpickupdate") ? (long?)(transfer.GetAttributeValue<DateTime>("rnt_actualpickupdate").AddMinutes(-StaticHelper.offset)).converttoTimeStamp() : null,
                actualDropoffDateTimeStamp = transfer.Attributes.Contains("rnt_actualdropoffdate") ? (long?)(transfer.GetAttributeValue<DateTime>("rnt_actualdropoffdate").AddMinutes(-StaticHelper.offset)).converttoTimeStamp(): null,
                statecode = transfer.GetAttributeValue<OptionSetValue>("statecode").Value,
                statuscode = transfer.GetAttributeValue<OptionSetValue>("statuscode").Value,
                createdon = transfer.GetAttributeValue<DateTime>("createdon"),
                modifiedon = transfer.GetAttributeValue<DateTime>("modifiedon")
            };
        }
        public CreateTransferResult createTransfer(CreateTransferParameter createTransferParameter)
        {
            Entity e = new Entity("rnt_transfer");
            e.Attributes["rnt_servicename"] = createTransferParameter.serviceName;
            e.Attributes["rnt_description"] = createTransferParameter.description;
            e.Attributes["statuscode"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.WaitingForDelivery);
            e.Attributes["rnt_equipmentid"] = new EntityReference("rnt_equipment", createTransferParameter.equipmentInformation.equipmentId);
            e.Attributes["rnt_groupcodeid"] = new EntityReference("rnt_groupcodeinformations", createTransferParameter.equipmentInformation.groupCodeInformationId);
            e.Attributes["rnt_transfertype"] = new OptionSetValue(createTransferParameter.transferType);
            e.Attributes["rnt_pickupbranchid"] = new EntityReference("rnt_branch", createTransferParameter.pickupBranch.branchId.Value);
            e.Attributes["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", createTransferParameter.dropoffBranch.branchId.Value);
            e.Attributes["rnt_actualpickupdate"] = StaticHelper.converttoDateTime(createTransferParameter.estimatedPickupTimestamp).AddMinutes(StaticHelper.offset);
            e.Attributes["rnt_actualdropoffdate"] = StaticHelper.converttoDateTime(createTransferParameter.estimatedDropoffTimestamp).AddMinutes(StaticHelper.offset);
            e.Attributes["rnt_estimatedpickupdate"] = StaticHelper.converttoDateTime(createTransferParameter.estimatedPickupTimestamp).AddMinutes(StaticHelper.offset);
            e.Attributes["rnt_estimateddropoffdate"] = StaticHelper.converttoDateTime(createTransferParameter.estimatedDropoffTimestamp).AddMinutes(StaticHelper.offset);
            e.Attributes["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(15);
            e["rnt_externaluserid"] = new EntityReference("rnt_serviceuser", createTransferParameter.userInformation.userId);

            var transferId = this.createEntity(e);

            return new CreateTransferResult
            {
                transferId = transferId,
                Result = true
            };
        }
        public void updateTransferForDelivery(Guid transferId,
                                              int currentKM,
                                              int currentFuel,
                                              int statusCode)
        {
            //status reason transferred
            //pickup fields
            //actual fields datetime now
            Entity e = new Entity("rnt_transfer");
            e.Id = transferId;
            e.Attributes["statuscode"] = new OptionSetValue(statusCode);
            e.Attributes["rnt_pickupfuel"] = new OptionSetValue(currentFuel);
            e.Attributes["rnt_pickupkilometer"] = currentKM;
            e.Attributes["rnt_actualpickupdate"] = this.DateNow.AddMinutes(StaticHelper.offset);
            this.updateEntity(e);
        }
        public void updateTransferForReturn(Guid transferId,
                                            int currentKM,
                                            int currentFuel)
        {
            //status reason completed
            //dropoff field
            //actual field datetime now
            Entity e = new Entity("rnt_transfer");
            e.Id = transferId;
            e.Attributes["statuscode"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_transfer_StatusCode.Completed);
            e.Attributes["rnt_dropofffuel"] = new OptionSetValue(currentFuel);
            e.Attributes["rnt_dropoffkilometer"] = currentKM;
            e.Attributes["rnt_actualdropoffdate"] = this.DateNow.AddMinutes(StaticHelper.offset);
            this.updateEntity(e);
        }
    }
}
