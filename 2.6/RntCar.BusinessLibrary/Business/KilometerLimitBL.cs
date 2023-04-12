using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class KilometerLimitBL : BusinessHandler
    {
        public KilometerLimitBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public KilometerLimitBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public MongoDBResponse createKilometerLimitInMongoDB(Entity kilometerLimit)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateKilometerLimitInMongoDB", Method.POST);

            var kilometerLimitData = this.buildKilometerLimitData(kilometerLimit);
            restSharpHelper.AddJsonParameter<KilometerLimitData>(kilometerLimitData);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        public MongoDBResponse updateKilometerLimitInMongoDB(Entity kilometerLimit)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");
            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateKilometerLimitInMongoDB", Method.POST);

            var kilometerLimitData = this.buildKilometerLimitData(kilometerLimit);
            restSharpHelper.AddJsonParameter<KilometerLimitData>(kilometerLimitData);
            restSharpHelper.AddQueryParameter("id", Convert.ToString(kilometerLimit.Id));

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        public int getKilometerLimitForGivenDurationandGroupCode(int duration, Guid groupCodeInformationId)
        {
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var systemKilometerLimit = systemParameterBL.getMonthlyKilometerLimit();
            var kilometerLimit = 0;
            this.Trace("duration" + duration);
           
            //means it is monthly
            while (true)
            {
                if (duration >= systemKilometerLimit)
                {
                    KilometerLimitRepository kilometerLimitRepository = new KilometerLimitRepository(this.OrgService);
                    var entity = kilometerLimitRepository.getMonthlyKilometerLimitsByGroupCode(groupCodeInformationId);
                    if (entity != null)
                    {
                        kilometerLimit += entity.GetAttributeValue<int>("rnt_kmlimit");
                    }

                }
                //means daily
                else
                {

                    KilometerLimitRepository kilometerLimitRepository = new KilometerLimitRepository(this.OrgService);
                    var entity = kilometerLimitRepository.getDailyKilometerLimitsByReservationDayandGroupCode(duration, groupCodeInformationId);
                    if (entity != null)
                    {
                        kilometerLimit += entity.GetAttributeValue<int>("rnt_kmlimit");
                    }
                    this.Trace("daily" + kilometerLimit);
                }
                var quotient = Convert.ToInt32(duration / 30);
                if (quotient == 0)
                    break;
                quotient -= 1;
                duration -= 30;
            }
           
            return kilometerLimit;
        }

        private KilometerLimitData buildKilometerLimitData(Entity kilometerLimit)
        {
            KilometerLimitData kilometerLimitData = new KilometerLimitData
            {
                durationCode = kilometerLimit.GetAttributeValue<OptionSetValue>("rnt_durationcode").Value,
                kilometerLimit = kilometerLimit.GetAttributeValue<int>("rnt_kmlimit"),
                groupCodeInformationId = kilometerLimit.GetAttributeValue<EntityReference>("rnt_groupcodeinformationid").Id,
                kilometerLimitId = Convert.ToString(kilometerLimit.Id),
                maximumDay = kilometerLimit.GetAttributeValue<int>("rnt_maximumday"),
                name = kilometerLimit.Attributes.Contains("rnt_name") ? kilometerLimit.GetAttributeValue<string>("rnt_name") : string.Empty,
                createdby = Convert.ToString(kilometerLimit.GetAttributeValue<EntityReference>("createdby").Id),
                modifiedby = Convert.ToString(kilometerLimit.GetAttributeValue<EntityReference>("modifiedby").Id),
                createdon = kilometerLimit.GetAttributeValue<DateTime>("createdon"),
                modifiedon = kilometerLimit.GetAttributeValue<DateTime>("modifiedon"),
                statuscode = kilometerLimit.GetAttributeValue<OptionSetValue>("statuscode").Value,
                statecode = kilometerLimit.GetAttributeValue<OptionSetValue>("statecode").Value,
            };

            return kilometerLimitData;
        }
    }
}
