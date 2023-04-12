using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.PriceFactor.Validation;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class PriceFactorBL : BusinessHandler
    {
        #region CONSTRUCTORS
        public PriceFactorBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public PriceFactorBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        #endregion

        #region METHODS
        public MongoDBResponse createPriceFactorInMongoDB(Entity priceFactor)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreatePriceFactorInMongoDB", Method.POST);

            var priceFactorData = this.buildPriceFactorData(priceFactor);
            this.TracingService.Trace("after build class");
            restSharpHelper.AddJsonParameter<PriceFactorData>(priceFactorData);

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }
        public MongoDBResponse updatePriceFactorInMongoDB(Entity priceFactor)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdatePriceFactorInMongoDB", Method.POST);

            var priceFactorData = this.buildPriceFactorData(priceFactor);
            this.Trace("after build class");
            restSharpHelper.AddJsonParameter<PriceFactorData>(priceFactorData);
            restSharpHelper.AddQueryParameter("id", priceFactor.GetAttributeValue<string>("rnt_mongodbid"));

            var response = restSharpHelper.Execute<MongoDBResponse>();

            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }

        private PriceFactorData buildPriceFactorData(Entity priceFactor)
        {
            var reservationChannelValues = priceFactor.Attributes.Contains("rnt_reservationchannelcode") ?
                JsonConvert.SerializeObject(priceFactor.GetAttributeValue<OptionSetValueCollection>("rnt_reservationchannelcode").Select(p=>p.Value).ToList()) : string.Empty;
            var weekDaysValues = priceFactor.Attributes.Contains("rnt_weekdayscode") ?
                JsonConvert.SerializeObject(priceFactor.GetAttributeValue<OptionSetValueCollection>("rnt_weekdayscode").Select(p => p.Value).ToList()) : string.Empty;
            var segments = priceFactor.Attributes.Contains("rnt_segmentcode") ?
               JsonConvert.SerializeObject(priceFactor.GetAttributeValue<OptionSetValueCollection>("rnt_segmentcode").Select(p => p.Value).ToList()) : string.Empty;
           

            MultiSelectMappingRepository multiSelectRepository = new MultiSelectMappingRepository(this.OrgService);
            var branchValues = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_branch",
                priceFactor.Attributes.Contains("rnt_branchcode") ? priceFactor.GetAttributeValue<OptionSetValueCollection>("rnt_branchcode") : null);

            this.Trace("rnt_groupcodeinformationcode : " + JsonConvert.SerializeObject(priceFactor.GetAttributeValue<OptionSetValueCollection>("rnt_groupcodeinformationcode")));

            var groupCodeValues = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_groupcode",
                priceFactor.Attributes.Contains("rnt_groupcodeinformationcode") ? priceFactor.GetAttributeValue<OptionSetValueCollection>("rnt_groupcodeinformationcode") : null);


            this.Trace("groupcodevalues : " + JsonConvert.SerializeObject(groupCodeValues));
            this.Trace("branchValues : " + JsonConvert.SerializeObject(branchValues));

            var accountGroups = priceFactor.Contains("rnt_accountpricefactorgroupcode") ?
                                priceFactor.GetAttributeValue<OptionSetValueCollection>("rnt_accountpricefactorgroupcode").Select(p => p.Value.ToString()).ToList() :
                                new List<string>();

            FactorDatesRepository factorDatesRepository = new FactorDatesRepository(this.OrgService);
            var factorDates = factorDatesRepository.getFactorDatesByPriceFactorId(priceFactor.Id);

            List<PriceFactorDatesData> factorDatesList = new List<PriceFactorDatesData>();
            foreach (var item in factorDates)
            {
                PriceFactorDatesData d = new PriceFactorDatesData
                {
                    beginDate = item.GetAttributeValue<DateTime>("rnt_begindate").converttoTimeStamp(),
                    endDate = item.GetAttributeValue<DateTime>("rnt_enddate").converttoTimeStamp(),
                };
                factorDatesList.Add(d);
            }
            PriceFactorData priceFactorData = new PriceFactorData
            {
                accountGroups = JsonConvert.SerializeObject(accountGroups),
                type = JsonConvert.SerializeObject(priceFactor.GetAttributeValue<OptionSetValueCollection>("rnt_type").Select(p => p.Value).ToList()),
                priceFactorId = Convert.ToString(priceFactor.Id),
                name = priceFactor.Attributes.Contains("rnt_name") ? priceFactor.GetAttributeValue<string>("rnt_name") : string.Empty,
                priceFactorType = priceFactor.Attributes.Contains("rnt_pricefactortypecode") ? priceFactor.GetAttributeValue<OptionSetValue>("rnt_pricefactortypecode").Value : 0,
                payMethod = priceFactor.Attributes.Contains("rnt_paymethodcode") ? priceFactor.GetAttributeValue<OptionSetValue>("rnt_paymethodcode").Value : 0,
                reservationChannel = reservationChannelValues,
                weekDays = weekDaysValues,
                groupCodes = JsonConvert.SerializeObject(groupCodeValues),
                branchs = JsonConvert.SerializeObject(branchValues),
                priceChangeType = priceFactor.Attributes.Contains("rnt_pricechangetypecode") ? priceFactor.GetAttributeValue<OptionSetValue>("rnt_pricechangetypecode").Value : 0,
                value = priceFactor.Attributes.Contains("rnt_value") ? priceFactor.GetAttributeValue<decimal>("rnt_value") : 0,
                beginDate = priceFactor.Attributes.Contains("rnt_begindate") ? priceFactor.GetAttributeValue<DateTime>("rnt_begindate") : (DateTime?)null,
                endDate = priceFactor.Attributes.Contains("rnt_enddate") ? priceFactor.GetAttributeValue<DateTime>("rnt_enddate") : (DateTime?)null,
                createdby = Convert.ToString(priceFactor.GetAttributeValue<EntityReference>("createdby").Id),
                modifiedby = Convert.ToString(priceFactor.GetAttributeValue<EntityReference>("modifiedby").Id),
                segments = segments,
                createdon = priceFactor.GetAttributeValue<DateTime>("createdon"),
                modifiedon = priceFactor.GetAttributeValue<DateTime>("modifiedon"),
                statuscode = priceFactor.GetAttributeValue<OptionSetValue>("statuscode").Value,
                statecode = priceFactor.GetAttributeValue<OptionSetValue>("statecode").Value,
                dates = factorDatesList
            };
            return priceFactorData;
        }

        // Tolga AYKURT - 07.03.2019
        /// <summary>
        /// Price factor ile ilgili validasyonları Create mesajında yürütür.
        /// </summary>
        /// <param name="priceFactorValidationInput"></param>
        /// <returns>Verilen parametreler validasyondan geçemezlerse hata mesajı dönülür.</returns>
        public string PriceFactorValidationForCreate(PriceFactorValidationInput priceFactorValidationInput)
        {
            this.Trace("priceFactorValidationInput : " + JsonConvert.SerializeObject(priceFactorValidationInput));
            var xrmHelper = new XrmHelper(OrgService);
            string errorMessage = string.Empty;
            string nameOfExistingPriceFactorName = null;
            bool isOK = true;
            
            //priceFactorValidationInput.PriceFactorType != 1 in payment types we dont have any date validation
            if (priceFactorValidationInput.PriceFactorType != 1 && priceFactorValidationInput.PriceFactorType != 9 &&
                DateValidation(priceFactorValidationInput.BeginDate, priceFactorValidationInput.EndDate) == false )
            {
                isOK = false;
                errorMessage = xrmHelper.GetXmlTagContent(priceFactorValidationInput.PluginInitializerUserId, "BeginDateDontBeGreaterThanEndDate");
            }
            //priceFactorValidationInput.PriceFactorType != 1 in payment types we dont have any date validation
            if (IsExistPriceFactorForCreate(priceFactorValidationInput, out nameOfExistingPriceFactorName) == true && 
                isOK == true)
            {
                errorMessage = String.Format(xrmHelper.GetXmlTagContent(priceFactorValidationInput.PluginInitializerUserId, "ExistAnotherRecord"), nameOfExistingPriceFactorName);
            }

            return errorMessage;
        }

        // Tolga AYKURT - 12.03.2019
        /// <summary>
        /// Price factor ile ilgili validasyonları Update mesajında yürütür.
        /// </summary>
        /// <param name="priceFactorValidationInput"></param>
        /// <returns>Verilen parametreler validasyondan geçemezlerse hata mesajı dönülür.</returns>
        public string PriceFactorValidationForUpdate(PriceFactorValidationInput priceFactorValidationInput, Guid recordId)
        {
            this.Trace("priceFactorValidationInput : " + JsonConvert.SerializeObject(priceFactorValidationInput));
            var xrmHelper = new XrmHelper(OrgService);
            string errorMessage = string.Empty;
            string nameOfExistingPriceFactorName = null;
            bool isOK = true;
            //priceFactorValidationInput.PriceFactorType != 1 in payment types we dont have any date validation
            if (priceFactorValidationInput.PriceFactorType != 1 && 
                DateValidation(priceFactorValidationInput.BeginDate, priceFactorValidationInput.EndDate) == false)
            {
                isOK = false;
                errorMessage = xrmHelper.GetXmlTagContent(priceFactorValidationInput.PluginInitializerUserId, "BeginDateDontBeGreaterThanEndDate");
            }
            //priceFactorValidationInput.PriceFactorType != 1 in payment types we dont have any date validation
            if (IsExistPriceFactorForUpdate(priceFactorValidationInput, recordId, out nameOfExistingPriceFactorName) == true && 
                isOK == true)
            {
                errorMessage = String.Format(xrmHelper.GetXmlTagContent(priceFactorValidationInput.PluginInitializerUserId, "ExistAnotherRecord"), nameOfExistingPriceFactorName);
            }

            return errorMessage;
        }
        #endregion

        #region HELPER METHODS
        // Tolga AYKURT - 07.03.2019
        private bool IsExistPriceFactorForCreate(PriceFactorValidationInput priceFactorValidationInput, out string nameOfExistingPriceFactor)
        {
            var priceFactorRepository = new PriceFactorRepository(OrgService);
            var priceFactors = new EntityCollection();
            if (priceFactorValidationInput.PriceFactorType == (int)ClassLibrary._Enums_1033.rnt_PriceFactorType.PayMethod ||
                priceFactorValidationInput.PriceFactorType == 9)
            {
                priceFactors = priceFactorRepository.GetPriceFactorsWihtOutDateConditions(priceFactorValidationInput);
            }
            else
            {
                priceFactors = priceFactorRepository.GetPriceFactors(priceFactorValidationInput);
            }
            
            nameOfExistingPriceFactor = string.Empty;

            if (priceFactors != null && priceFactors.Entities.Count > 0)
                nameOfExistingPriceFactor = priceFactors.Entities[0].GetAttributeValue<string>("rnt_name");

            return priceFactors != null && priceFactors.Entities.Count > 0;
        }

        // Tolga AYKURT - 12.03.2019
        private bool IsExistPriceFactorForUpdate(PriceFactorValidationInput priceFactorValidationInput, Guid recordId, out string nameOfExistingPriceFactor)
        {
            var priceFactorRepository = new PriceFactorRepository(OrgService);
            var priceFactors = new EntityCollection();
            if (priceFactorValidationInput.PriceFactorType == (int)ClassLibrary._Enums_1033.rnt_PriceFactorType.PayMethod)
            {
                priceFactors = priceFactorRepository.GetPriceFactorsWihtOutDateConditions(priceFactorValidationInput);
            }
            else
            {
                priceFactors = priceFactorRepository.GetPriceFactors(priceFactorValidationInput);
            }
            nameOfExistingPriceFactor = string.Empty;

            if (priceFactors != null && priceFactors.Entities.Count > 0)
            {
                this.Trace("priceFactors count " + priceFactors.Entities.Count);
                var recordEntity = priceFactors.Entities.Where(pf => pf.Id.Equals(recordId)).FirstOrDefault();

                if(recordEntity != null && recordEntity.Id != default(Guid))
                {
                    priceFactors.Entities.Remove(recordEntity);

                    if(priceFactors.Entities.Count > 0)
                    {
                        nameOfExistingPriceFactor = priceFactors.Entities[0].GetAttributeValue<string>("rnt_name");
                    }
                }
            }

            return priceFactors != null && priceFactors.Entities.Count > 0;
        }

        // Tolga AYKURT - 07.03.2019
        private bool DateValidation(DateTime beginDate, DateTime endDate)
        {
            return endDate >= beginDate;
        }
        #endregion
    }
}
