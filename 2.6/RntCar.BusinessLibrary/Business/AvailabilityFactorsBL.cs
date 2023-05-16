using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class AvailabilityFactorsBL : BusinessHandler
    {
        public AvailabilityFactorsBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public AvailabilityFactorsBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public MongoDBResponse createAvailabilityFactorInMongoDB(Entity availabilityFactor)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);//"ConfigurationBL" sınıfı, MongoDB URL'sini almak için kullanılır 
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateAvailabilityFactorInMongoDB", RestSharp.Method.POST); // "RestSharpHelper" sınıfı, bu URL'ye ve bir HTTP isteği türüne ("POST") dayalı olarak bir REST isteği oluşturur.

            var availabilityFactorData = this.buildAvailabilityFactorData(availabilityFactor);//"availabilityFactorData" adlı bir değişken oluşturulur ve "buildAvailabilityFactorData" adlı bir metotla doldurulur. Bu metot, "availabilityFactor" parametresinden geçerli bir "AvailabilityFactorData" nesnesi oluşturur.

            restSharpHelper.AddJsonParameter<AvailabilityFactorData>(availabilityFactorData);//"restSharpHelper" nesnesi, "availabilityFactorData" nesnesini JSON biçiminde parametre olarak ekler.

            var response = restSharpHelper.Execute<MongoDBResponse>();//Sonrasında "restSharpHelper" nesnesi, "Execute" yöntemiyle bir REST çağrısı yapar ve "MongoDBResponse" nesnesi ile yanıtı döndürür. "MongoDBResponse" nesnesi, bir "Result" ve bir "Id" özelliği içerir.
            this.TracingService.Trace("responsresult : " + response.Result);
            //Son iki satır, yöntemin çağrılması sırasında oluşabilecek hataları yakalar ve hataları bir "MongoDBResponse" nesnesi içinde döndürür.
            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }
            this.TracingService.Trace("respons id : " + response.Id);
            return response;
        }
        public MongoDBResponse updateAvailabilityFactorInMongoDB(Entity availabilityFactor)// Bu metot bir MongoDB veritabanındaki AvailabilityFactor varlığının verilerini güncellemek için kullanılır.
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);//"ConfigurationBL" sınıfı, MongoDB URL'sini almak için kullanılır 
            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateAvailabilityFactorInMongoDB", RestSharp.Method.POST); // "RestSharpHelper" sınıfı, bu URL'ye ve bir HTTP isteği türüne ("POST") dayalı olarak bir REST isteği oluşturur.

            var availabilityFactorData = this.buildAvailabilityFactorData(availabilityFactor);//"availabilityFactorData" adlı bir değişken oluşturulur ve "buildAvailabilityFactorData" adlı bir metotla doldurulur. Bu metot, "availabilityFactor" parametresinden geçerli bir "AvailabilityFactorData" nesnesi oluşturur.

            restSharpHelper.AddJsonParameter<AvailabilityFactorData>(availabilityFactorData);//"restSharpHelper" nesnesi, "availabilityFactorData" nesnesini JSON biçiminde parametre olarak ekler.
            restSharpHelper.AddQueryParameter("id", availabilityFactor.GetAttributeValue<string>("rnt_mongodbid"));//RestSharp kütüphanesi kullanılarak bir sorgu parametresi ekler. Bu parametre, MongoDB'de güncellenecek AvailabilityFactor belgesinin benzersiz kimliğini belirtir

            var response = restSharpHelper.Execute<MongoDBResponse>();
            //Son iki satır, yöntemin çağrılması sırasında oluşabilecek hataları yakalar ve hataları bir "MongoDBResponse" nesnesi içinde döndürür.
            if (!response.Result)
            {
                return MongoDBResponse.ReturnError(response.ExceptionDetail);
            }

            return response;
        }

        public AvailabilityFactorData buildAvailabilityFactorData(Entity availabilityFactor)
        {//Bu metot, bir Entity nesnesi alır ve AvailabilityFactorData tipinde bir nesne döndürür. Entity nesnesi, Dynamics 365 gibi bir CRM uygulamasındaki bir kaydı temsil eder ve AvailabilityFactorData nesnesi, bu kaydın MongoDB veritabanına kaydedilmesi için kullanılan verileri içerir.

            //Bu metot, birden fazla sorgu yürütmek için farklı repository'ler kullanır ve bunları kullanarak AvailabilityFactorData nesnesinin farklı özelliklerine değer atar. Bu özellikler, kaydın türü, adı, başlangıç ve bitiş tarihleri, grup kodları, hesap grupları, kapasite oranı vb. özellikleri içerir. Bu bilgiler, kaydın MongoDB'deki doğru şekilde saklanmasını sağlamak için gereklidir.
                        MultiSelectMappingRepository multiSelectRepository = new MultiSelectMappingRepository(this.OrgService);
            var branchCode = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_branch",
                availabilityFactor.Attributes.Contains("rnt_multiselectbranchcode") ? availabilityFactor.GetAttributeValue<OptionSetValueCollection>("rnt_multiselectbranchcode") : null);

            List<AvailabilityFactorGroupCodes> availabilityFactorGroupCodes = new List<AvailabilityFactorGroupCodes>();
            var groupCodes = new List<Guid>();
            if (availabilityFactor.Attributes.Contains("rnt_multiselectgroupcodeinformation"))
            {
                groupCodes = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_groupcode",
                availabilityFactor.Attributes.Contains("rnt_multiselectgroupcodeinformation") ? availabilityFactor.GetAttributeValue<OptionSetValueCollection>("rnt_multiselectgroupcodeinformation") : null);


                GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(this.OrgService);
                var groupCodeNames = groupCodeInformationRepository.getGroupCodeInformationByGivenIds(groupCodes.Select(s => s.ToString()).ToList(), new string[] { "rnt_name" });


                foreach (var item in groupCodeNames)
                {
                    AvailabilityFactorGroupCodes groupCode = new AvailabilityFactorGroupCodes
                    {
                        id = item.Id.ToString(),
                        name = item.GetAttributeValue<string>("rnt_name")

                    };
                    availabilityFactorGroupCodes.Add(groupCode);
                }
                this.Trace("groupCodeNames : " + JsonConvert.SerializeObject(groupCodeNames));

            }
            var accountGroups = availabilityFactor.Contains("rnt_accountpricefactorgroupcode") ?
                                availabilityFactor.GetAttributeValue<OptionSetValueCollection>("rnt_accountpricefactorgroupcode").Select(p => p.Value.ToString()).ToList() :
                                new List<string>();

            var type = availabilityFactor.Contains("rnt_type") ?
                                availabilityFactor.GetAttributeValue<OptionSetValueCollection>("rnt_type").Select(p => p.Value.ToString()).ToList() :
                                new List<string>();
            

            var channels = availabilityFactor.GetAttributeValue<OptionSetValueCollection>("rnt_channelcode");
            AvailabilityFactorData availabilityFactorData = new AvailabilityFactorData
            {
                accountGroups = accountGroups,
                type = type,
                availabilityFactorId = Convert.ToString(availabilityFactor.Id),
                name = availabilityFactor.GetAttributeValue<string>("rnt_name"),
                availabilityFactorType = availabilityFactor.GetAttributeValue<OptionSetValue>("rnt_availabilityfactortypecode").Value,
                channelValues = JsonConvert.SerializeObject(channels.Select(p => p.Value).ToList()),
                branchValues = JsonConvert.SerializeObject(branchCode),
                groupCodeValues = JsonConvert.SerializeObject(groupCodes),
                groupCodeList = JsonConvert.SerializeObject(availabilityFactorGroupCodes),
                createdby = Convert.ToString(availabilityFactor.GetAttributeValue<EntityReference>("createdby").Id),
                modifiedby = Convert.ToString(availabilityFactor.GetAttributeValue<EntityReference>("modifiedby").Id),
                createdon = availabilityFactor.GetAttributeValue<DateTime>("createdon"),
                modifiedon = availabilityFactor.GetAttributeValue<DateTime>("modifiedon"),
                statecode = availabilityFactor.GetAttributeValue<OptionSetValue>("statecode").Value,
                statuscode = availabilityFactor.GetAttributeValue<OptionSetValue>("statuscode").Value,
                beginDate = availabilityFactor.GetAttributeValue<DateTime>("rnt_begindate"),
                endDate = availabilityFactor.GetAttributeValue<DateTime>("rnt_enddate"),
                minimumReservationDuration = availabilityFactor.GetAttributeValue<int>("rnt_minimumreservationday"),
                capacityRatio = availabilityFactor.GetAttributeValue<int>("rnt_capacityratio")

            };

            return availabilityFactorData;
        }
    }
}
