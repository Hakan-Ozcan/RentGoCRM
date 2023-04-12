
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Tablet;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.Reservation;
using RntCar.MongoDBHelper;
using RntCar.MongoDBHelper.Entities;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.PaymentHelper;
using RntCar.PaymentHelper.iyzico;
using RntCar.PaymentHelper.NetTahsilat;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using zRntCar.TestProject.adapter;
using zRntCar.TestProject.Decorator;
using zRntCar.TestProject.observer;
using RazorEngine;
using RazorEngine.Templating;
using RazorEngine.Configuration;
using RntCar.RedisCacheHelper.CachedItems;
using RntCar.RedisCacheHelper;
using RntCar.AzureBlobStorage;
using System.Net;
using OfficeOpenXml;
using RntCar.IntegrationHelper;
using GemBox.Document;
using RntCar.ClassLibrary._Web;
using RntCar.ClassLibrary.Report;
using RntCar.ExternalServicesWebAPI.Controllers;
using static RntCar.ClassLibrary.GlobalEnums;
using static RntCar.ClassLibrary.ContractItemEnums;
using RntCar.Logger;
using RntCar.MongoDBHelper.Repository;
using MongoDB.Driver.Linq;

namespace zRntCar.TestProject
{
    public class Program
    {

        static string mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
        static string mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");
        //İSTANBUL ATATÜRK DOMESTIC TERMINAL
        static Guid _pickupBranchId = new Guid("04F8934D-9AAF-E811-9410-000C293D97F8");
        static Guid _dropoffBranchId = new Guid("04F8934D-9AAF-E811-9410-000C293D97F8");

        //amasya
        //static Guid _pickupBranchId = new Guid("AF8D5D16-17B1-E811-9410-000C293D97F8");
        //static Guid _dropoffBranchId = new Guid("AF8D5D16-17B1-E811-9410-000C293D97F8");


        //times
        static DateTime _pickupTime = new DateTime(2018, 10, 6, 8, 00, 00);
        static DateTime _dropoffTime = new DateTime(2018, 10, 7, 8, 00, 00);
        public static ReservationItemDataMongoDB selectedReservation { get; set; }

        public class eIhlalEquipmentInfo
        {
            [XmlElement("arac")]
            public string equipment { get; set; }
            [XmlElement("tutanak_seri")]
            public string reportInfo { get; set; }
            [XmlElement("tutanak_sira_no")]
            public string reportNo { get; set; }
            [XmlElement("ceza_maddesi")]
            public string penalClause { get; set; }
            [XmlElement("ceza_tutari")]
            public string penalAmount { get; set; }
            [XmlElement("kesildigi_yer")]
            public string penalPlace { get; set; }
            [XmlElement("ceza_tarihi")]
            public string penalDate { get; set; }
            [XmlElement("ceza_saat")]
            public string penalTime { get; set; }
            [XmlElement("duzenleyen_birim")]
            public string organizingUnit { get; set; }
            [XmlElement("il_ilce")]
            public string addressInfo { get; set; }
            [XmlElement("teblig_tarihi")]
            public string communiqueDate { get; set; }
        }

        public class eIhlalEquipmentInfo1
        {
            [XmlElement("plaka")]
            public string plateNumber { get; set; }
            [XmlElement("ihlalno")]
            public string reportNo { get; set; }
            [XmlElement("aracsinif")]
            public string equipmentClassNo { get; set; }
            [XmlElement("giris_istasyon")]
            public string entranceStation { get; set; }
            [XmlElement("cikis_istasyon")]
            public string exitStation { get; set; }
            [XmlElement("giris_zaman")]
            public string entranceDate { get; set; }
            [XmlElement("cikis_zaman")]
            public string exitDate { get; set; }
            [XmlElement("cezasiz_son_odeme")]
            public string dueDate { get; set; }
            [XmlElement("normalgecisucreti")]
            public string defaultAmount { get; set; }
            [XmlElement("cezatutari")]
            public string penalAmount { get; set; }
            [XmlElement("kurum")]
            public string corporation { get; set; }
            [XmlElement("silindi")]
            public string deleted { get; set; }
        }
        public class eIhlalResponse
        {
            [XmlElement("arac_list")]
            public List<eIhlalEquipmentInfo> equipmentList { get; set; }
            [XmlElement("sistem")]
            public bool result { get; set; }
        }
        public class XmlSerializerHelper
        {
            public T Deserialize<T>(string input, string xmlRootName) where T : class
            {
                System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T), new XmlRootAttribute(xmlRootName));

                using (StringReader sr = new StringReader(System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(input))))
                {
                    return (T)ser.Deserialize(sr);
                }
            }
            public T Deserialize<T>(byte[] xmlByte, string xmlRootName)
            {
                XmlSerializer ser = new XmlSerializer(typeof(T), new XmlRootAttribute(xmlRootName));

                using (StringReader sr = new StringReader(System.Text.Encoding.GetEncoding("Windows-1254").GetString(xmlByte)))
                {
                    return (T)ser.Deserialize(sr);
                }
            }
            public string Serialize<T>(T ObjectToSerialize)
            {
                XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());

                using (StringWriter textWriter = new StringWriter())
                {
                    xmlSerializer.Serialize(textWriter, ObjectToSerialize);
                    return textWriter.ToString();
                }
            }
        }
        public class plaka
        {
            public string documentContent { get; set; }
        }

        public static void deletereservationitems()
        {
            var s = new CrmServiceHelper().IOrganizationService;
            QueryExpression queryExpression = new QueryExpression("rnt_reservationitem");
            var result = new CrmServiceHelper().IOrganizationService.RetrieveMultiple(queryExpression);
            foreach (var item in result.Entities)
            {
                try
                {
                    s.Delete("rnt_reservationitem", item.Id);
                }
                catch (Exception)
                {

                    continue;
                }

            }
        }
        public static void deletecontractitems()
        {
            var s = new CrmServiceHelper().IOrganizationService;
            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            var result = new CrmServiceHelper().IOrganizationService.RetrieveMultiple(queryExpression);
            foreach (var item in result.Entities)
            {
                try
                {
                    s.Delete("rnt_contractitem", item.Id);
                }
                catch (Exception)
                {

                    continue;
                }

            }
        }
        public static void deletecontracts()
        {
            var s = new CrmServiceHelper().IOrganizationService;
            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            // queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, "fc83634c-c94c-e911-a959-000d3a454e11");
            var result = new CrmServiceHelper().IOrganizationService.RetrieveMultiple(queryExpression);
            foreach (var item in result.Entities)
            {
                try
                {
                    s.Delete("rnt_contract", item.Id);
                }
                catch (Exception ex)
                {

                    continue;
                }

            }
        }
        public static void deletereservations()
        {
            var s = new CrmServiceHelper().IOrganizationService;
            QueryExpression queryExpression = new QueryExpression("rnt_reservation");
            var result = new CrmServiceHelper().IOrganizationService.RetrieveMultiple(queryExpression);
            foreach (var item in result.Entities)
            {
                try
                {
                    s.Delete("rnt_reservation", item.Id);
                }
                catch (Exception)
                {

                    continue;
                }

            }
        }
        public static void deletepayment()
        {

            var s = new CrmServiceHelper().IOrganizationService;
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            var result = new CrmServiceHelper().IOrganizationService.RetrieveMultiple(queryExpression);
            foreach (var item in result.Entities)
            {
                try
                {
                    s.Delete("rnt_payment", item.Id);
                }
                catch (Exception)
                {

                    continue;
                }

            }
        }
        public class questions : calculation
        {
            public int MyProperty { get; set; }
        }

        //reservationparameters
        public class calculation
        {
            public DateTime pick { get; set; }
            public DateTime dropp { get; set; }
        }
        public static void syncOneWay()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_onewayfee");
            queryExpression.ColumnSet = new ColumnSet(true);
            var e = new CrmServiceHelper().IOrganizationService.RetrieveMultiple(queryExpression);
            OneWayFeeBL oneWayFeeBL = new OneWayFeeBL(new CrmServiceHelper().IOrganizationService);
            foreach (var item in e.Entities)
            {
                if (item.Contains("rnt_mongodbid"))
                {
                    //oneWayFeeBL.UpdateOneWayFeeInMongoDB(item);
                    //oneWayFeeBL.UpdateMongoDBUpdateRelatedFields(item);
                }
                else
                {
                    var actionResponse = oneWayFeeBL.CreateOneWayFeeInMongoDB(item);
                    oneWayFeeBL.updateMongoDBCreateRelatedFields(item, actionResponse.Id);
                }
            }
        }

        public static void getTodaysContracts()
        {
            var client = new MongoClient(StaticHelper.GetConfiguration("MongoDBHostName"));
            var database = client.GetDatabase(StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var collection = database.GetCollection<UploadSignedDocumentParameter>("SignedDocument");

            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.Criteria.AddCondition("createdon", ConditionOperator.Today);
            queryExpression.ColumnSet = new ColumnSet(true);
            var res = new CrmServiceHelper().IOrganizationService.RetrieveMultiple(queryExpression);

            foreach (var item in res.Entities)
            {
                AzureBlobStorageHelper azureBlobStorageHelper = new AzureBlobStorageHelper("DefaultEndpointsProtocol=https;AccountName=rentgoproductionstorage;AccountKey=d3zPZ07IAZROSuLKQDYHH9jPzrqz8GJMGbbUojhSJONNDPEc8mpyloGZnrOoTDGATqrx+rpyU2k7RWZ7ZiZ5kw==;EndpointSuffix=core.windows.net");
                var url = azureBlobStorageHelper.getGetDocumentsUrlByContainerandDirectory("digitalsignaturecontracts", item.GetAttributeValue<string>("rnt_name"));

                foreach (var iUrl in url)
                {
                    if (!iUrl.Contains("unsigned"))
                    {
                        using (WebClient wc = new WebClient())
                        {
                            byte[] data = wc.DownloadData(iUrl);

                            if (data.Length == 0)
                            {
                                var document = database.GetCollection<UploadSignedDocumentParameter>("SignedDocument")
                                               .AsQueryable()
                                               .Where(p => p.fileName.Contains(item.GetAttributeValue<string>("rnt_name")) &&
                                                           !string.IsNullOrEmpty(p.documentContent)).FirstOrDefault();

                                if (document != null)
                                {
                                    var par = new UploadSignedDocumentParameter
                                    {
                                        fileName = document.fileName,
                                        documentContent = document.documentContent
                                    };

                                    RestSharpHelper restSharpHelper = new RestSharpHelper("http://localhost:6060/api/document", "uploadsigneddocument", RestSharp.Method.POST);


                                    restSharpHelper.AddJsonParameter<UploadSignedDocumentParameter>(par);

                                    var response = restSharpHelper.Execute<UploadSignedDocumentResponse>();
                                }


                            }
                        }
                    }

                }


            }
        }

        public static void sendreservationItemtoMongo()
        {
            var s = new CrmServiceHelper().IOrganizationService;
            var contractItem = s.Retrieve("rnt_reservationitem", new Guid("62a8dbc1-dddb-eb11-bacb-000d3abfc705"), new ColumnSet(true));
            ReservationItemBL contractItemBL = new ReservationItemBL(s);
            var response = contractItemBL.CreateReservationItemInMongoDB(contractItem);


            contractItemBL.updateMongoDBCreateRelatedFields(contractItem, response.Id);
        }
        public class Poo { }
        public class RadioactivePoo : Poo { }

        interface IAnimal
        {
            Poo Excrement { get; }
        }

        public class BaseAnimal<PooType> : IAnimal
            where PooType : Poo, new()
        {
            Poo IAnimal.Excrement { get { return (Poo)this.Excrement; } }

            public PooType Excrement
            {
                get { return new PooType(); }
            }
        }
        public class Dog : BaseAnimal<Poo> { }
        public class Cat : BaseAnimal<RadioactivePoo> { }
        public static void updateDailyPrices(string groupCodeId, string trackingNumber, string contractItemId)
        {


            //RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository repor = new RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository(mongoDBHostName, mongoDBDatabaseName);
            //var prices = repor.getPricesCalculationSummariesByTrackingNumberandGroupCode(groupCodeId, trackingNumber);
            ////var prices = repor.getPricesCalculationSummariesByTrackingNumberCampaignIdandGroupCode(groupCodeId, trackingNumber, new Guid("66929f5a-4ab4-e911-a84d-000d3a2ddb03"));
            //var s = new CrmServiceHelper().IOrganizationService;
            //ContractItemRepository contractItemRepository = new ContractItemRepository(s);
            //var c = contractItemRepository.getContractItemId(new Guid(contractItemId));
            //var totalAmount = c.GetAttributeValue<Money>("rnt_totalamount").Value;
            //var totalDuration = c.GetAttributeValue<int>("rnt_contractduration");
            //RntCar.MongoDBHelper.Entities.PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(mongoDBHostName, mongoDBDatabaseName);
            //var date = new DateTime(2021, 10, 21, 10, 09, 0);
            //var tracking = Guid.NewGuid();
            //for (int i = 0; i < 66; i++)
            //{
            //    var v = prices.FirstOrDefault();
            //    v.priceDate = date.AddDays(i);
            //    //v.
            //    v._id = ObjectId.GenerateNewId();
            //    v.trackingNumber = tracking.ToString().Replace("{", "").Replace("}", "");
            //    v.payLaterAmount = 9220.26M / 66;
            //    v.payNowAmount = 9220.26M / 66;
            //    v.totalAmount = 9220.26M / 66;
            //    v.priceDateTimeStamp = new BsonTimestamp(v.priceDate.converttoTimeStamp());
            //    priceCalculationSummariesBusiness.createPriceCalculationSummaryFromExisting(v);
            //}


            //foreach (var item in prices)
            //{
            //    item.payLaterAmount = totalAmount / totalDuration;
            //    item.payNowAmount = totalAmount / totalDuration;
            //    item.totalAmount = totalAmount / totalDuration;
            //    //item.campaignId = new Guid("63f64104-6536-eb11-a813-000d3a38a128");

            //    var collection = repor.getCollection<PriceCalculationSummaryMongoDB>("PriceCalculationSummaries");


            //    collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });


            //    //collection.DeleteOne(p => p._id == item._id);

            //}
        }

        public static void craetePriceCalculation(string groupCodeId, string trackingNumber, string newTrackingNumber)
        {
            RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository repor = new RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository(mongoDBHostName, mongoDBDatabaseName);
            var prices = repor.getPricesCalculationSummariesByTrackingNumberandGroupCode(groupCodeId, trackingNumber);
            for (int i = 0; i < 8; i++)
            {
                foreach (var item in prices)
                {
                    var timeStamp = item.priceDate.AddDays(i * 30).converttoTimeStamp();
                    var priceDayTimeStamp = new BsonTimestamp(timeStamp);

                    PriceCalculationSummaryMongoDB priceCalculationSummaryMongoDB = new PriceCalculationSummaryMongoDB
                    {
                        ID = Guid.NewGuid().ToString(),
                        _id = ObjectId.GenerateNewId(),
                        priceDate = item.priceDate.AddDays(i * 30),
                        trackingNumber = newTrackingNumber,
                        userId = string.Empty,//todo will handle from parameter
                        userEntityLogicalName = string.Empty,//todo will handle from parameter
                        priceDateTimeStamp = priceDayTimeStamp,
                        selectedPriceListId = item.selectedAvailabilityPriceListId,
                        totalAmount = item.totalAmount,
                        payLaterAmount = item.payLaterAmount,
                        payNowAmount = item.payNowAmount,
                        selectedGroupCodePriceListId = item.selectedGroupCodePriceListId,
                        selectedGroupCodeAmount = item.selectedGroupCodeAmount,
                        selectedAvailabilityPriceListId = item.selectedAvailabilityPriceListId,
                        selectedAvailabilityPriceRate = item.selectedAvailabilityPriceRate != null ? item.selectedAvailabilityPriceRate : decimal.Zero,
                        relatedGroupCodeId = item.relatedGroupCodeId,
                        relatedGroupCodeName = item.relatedGroupCodeName,
                        availabilityRate = item.availabilityRate,
                        priceAfterAvailabilityFactor = item.priceAfterAvailabilityFactor,
                        priceAfterChannelFactor = item.priceAfterChannelFactor,
                        priceAfterCustomerFactor = item.priceAfterCustomerFactor,
                        priceAfterSpecialDaysFactor = item.priceAfterSpecialDaysFactor,
                        priceAfterWeekDaysFactor = item.priceAfterWeekDaysFactor,
                        priceAfterBranchFactor = item.priceAfterBranchFactor,
                        priceAfterBranch2Factor = item.priceAfterBranch2Factor,
                        payLaterWithoutTickDayAmount = item.payLaterWithoutTickDayAmount,
                        priceAfterEqualityFactor = item.priceAfterEqualityFactor,
                        payNowWithoutTickDayAmount = item.payNowWithoutTickDayAmount,
                        isTickDay = item.isTickDay,
                        priceAfterPayMethodPayLater = item.priceAfterPayMethodPayLater,
                        priceAfterPayMethodPayNow = item.priceAfterPayMethodPayNow
                    };
                    if (item.campaignId != Guid.Empty)
                        priceCalculationSummaryMongoDB.campaignId = item.campaignId;

                    var methodName = ErrorLogsHelper.GetCurrentMethod();
                    var itemId = priceCalculationSummaryMongoDB._id.ToString();

                    var collection = repor.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
                    collection.Insert(priceCalculationSummaryMongoDB, itemId, methodName);
                }
            }

        }

        public static void updateDailyPricesCampign(string groupCodeId, string trackingNumber, string campignId = null)
        {
            RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository repor = new RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository(mongoDBHostName, mongoDBDatabaseName);
            var prices = repor.getPricesCalculationSummariesByTrackingNumberandGroupCode(groupCodeId, trackingNumber);
            var s = new CrmServiceHelper().IOrganizationService;
            RntCar.BusinessLibrary.ContractItemRepository contractItemRepository = new RntCar.BusinessLibrary.ContractItemRepository(s);
            foreach (var item in prices)
            {
                if (!string.IsNullOrWhiteSpace(campignId))
                {
                    item.campaignId = new Guid(campignId);
                }
                else
                {
                    item.campaignId = Guid.Empty;
                }

                var collection = repor.getCollection<PriceCalculationSummaryMongoDB>("PriceCalculationSummaries");


                collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });

            }
        }
        public static void createDailyPrices(string contractItemId)
        {
            IOrganizationService orgService = new CrmServiceHelper().IOrganizationService;
            ContractItemBL contractItemBL = new ContractItemBL(orgService);
            Entity contractItem = orgService.Retrieve("rnt_contractitem", new Guid(contractItemId), new ColumnSet(true));
            var contractItemData = contractItemBL.buildMongoDBContractItemData(contractItem);
            ContractDailyPrices contractDailyPrices = new ContractDailyPrices(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            contractDailyPrices.createContractDailyPrices(contractItemData, true);
        }

        public static void createDailyPricesReservationItem(string reservationItemId)
        {
            //IOrganizationService orgService = new CrmServiceHelper().IOrganizationService;
            //ReservationBL reservationItemBL = new ReservationBL(orgService);
            //Entity reservationItem = orgService.Retrieve("rnt_reservationitem", new Guid(reservationItemId), new ColumnSet(true));
            //var reservationItemData = reservationItemBL(contractItem);
            //ContractDailyPrices contractDailyPrices = new ContractDailyPrices(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            //contractDailyPrices.createContractDailyPrices(contractItemData, true);
        }

        public static void createPriceSummaryPrices(string contractItemId)
        {
            IOrganizationService orgService = new CrmServiceHelper().IOrganizationService;
            ContractItemBL contractItemBL = new ContractItemBL(orgService);
            Entity contractItem = orgService.Retrieve("rnt_contractitem", new Guid(contractItemId), new ColumnSet(true));
            var contractItemData = contractItemBL.buildMongoDBContractItemData(contractItem);
            ContractDailyPrices contractDailyPrices = new ContractDailyPrices(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            contractDailyPrices.createContractDailyPrices(contractItemData, true);
        }

        public static void updatePriceCalc()
        {
            RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository repor = new RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository(mongoDBHostName, mongoDBDatabaseName);
            var prices = repor.getPricesCalculationSummariesByTrackingNumberandGroupCode("d88886b7-9fb2-e911-a850-000d3a2ddcbb", "a6fbe1cf-b4dd-4ae1-a70e-6f5bd54b530c");

            var date = Convert.ToDateTime("01.01.2021 12:06");
            int i = 0;
            foreach (var item in prices)
            {

                item.priceDate = date.AddDays(i);
                item.priceDateTimeStamp = new BsonTimestamp(date.AddDays(i).converttoTimeStamp());
                var collection = repor.getCollection<PriceCalculationSummaryMongoDB>("PriceCalculationSummaries");


                collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });

                i++;
            }
        }

        public static void Main(string[] args)
        {

            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            IOrganizationService orgService = crmServiceHelper.IOrganizationService;
            ContractRepository contractRepository = new ContractRepository(orgService);

            //Price Calculation Summaries verirlerini burada çekiyorum
            PriceCalculationSummariesRepository priceCalculationSummariesRepository = new PriceCalculationSummariesRepository("mongodb://localhost:27017", "RentgoMongdoDB");
            IMongoQueryable<PriceCalculationSummaryMongoDB> PriceCalculationSummaryMongoDB = priceCalculationSummariesRepository.getPriceCalculationSummaries();
            Console.WriteLine(PriceCalculationSummaryMongoDB);

            //Availability Query Analyzer verilerini burada çekiyorum
            AvailabilityQueryAnalyzerRepository availabilityQueryAnalyzerRepository = new AvailabilityQueryAnalyzerRepository("mongodb://localhost:27017", "RentgoMongdoDB");
            List<AvailabilityQueryAnalyzer> availabilityQueryAnalyzers = availabilityQueryAnalyzerRepository.getAllAvailabilityQueryAnalyzer();
            Console.WriteLine(availabilityQueryAnalyzers);

            //AvailabilityResponse verilerini burada çekiyorum
            AvailabilityResponse availabilityResponse = new AvailabilityResponse();





            string[] columns = { "rnt_name", "rnt_dropoffdatetime", "rnt_pnrnumber", "rnt_customerid", "rnt_totalamount", "rnt_netpayment", "rnt_depositblockage" };
            var contracts = contractRepository.getContractsByDateFilterWithGivenStatusAndColumns("", 1, columns, (int)rnt_contract_StatusCode.Completed);
            DateTime processDate = new DateTime(2023, 1, 1);
            while (processDate < DateTime.Now.AddDays(1))
            {
                DateTime starTimeToUse = new DateTime(processDate.Year, processDate.Month, processDate.Day, 4, 0, 0);
                DateTime endTimeToUse = new DateTime(processDate.Year, processDate.Month, processDate.Day, 23, 0, 0);

                RntCar.MongoDBHelper.Repository.EquipmentRepository equipmentRepository = new RntCar.MongoDBHelper.Repository.EquipmentRepository(mongoDBHostName, mongoDBDatabaseName);
                List<FleetReportData> fleetReportData = new List<FleetReportData>();

                var reportDatas = equipmentRepository.GetEquipmentForFleetReport(starTimeToUse.Ticks, endTimeToUse.Ticks);

                Console.WriteLine(processDate + " " + reportDatas.Count);
                if (reportDatas.Count > 0)
                {
                    var removeDataList = reportDatas.Where(x => x.publishDate == processDate.AddHours(5).Ticks).ToList();
                    removeDataList.AddRange(reportDatas.Where(x => x.publishDate == processDate.AddHours(8).Ticks).ToList());
                    removeDataList.AddRange(reportDatas.Where(x => x.publishDate == processDate.AddHours(11).Ticks).ToList());
                    removeDataList.AddRange(reportDatas.Where(x => x.publishDate == processDate.AddHours(14).Ticks).ToList());
                    removeDataList.AddRange(reportDatas.Where(x => x.publishDate == processDate.AddHours(17).Ticks).ToList());
                    removeDataList.AddRange(reportDatas.Where(x => x.publishDate == processDate.AddHours(20).Ticks).ToList());
                    removeDataList.AddRange(reportDatas.Where(x => x.publishDate == processDate.AddHours(23).Ticks).ToList());
                    foreach (var removeData in removeDataList)
                    {
                        reportDatas.Remove(removeData);
                    }
                    equipmentRepository.BulkDeleteEquipmentForFleetReport(reportDatas);
                    Console.WriteLine(processDate + " " + removeDataList.Count + " " + reportDatas.Count);
                }

                processDate = processDate.AddDays(1);
            }

            //sendInvoicetoLogo();
            //CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            //var orgService = crmServiceHelper.IOrganizationService;

            //            string query = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
            //  <entity name='rnt_payment'>
            //    <attribute name='rnt_paymentid' />
            //    <attribute name='rnt_creditcardslipid' />
            //    <attribute name='rnt_contactid' />
            //    <attribute name='rnt_amount' />
            //    <attribute name='rnt_transactiontypecode' />
            //    <attribute name='rnt_transactionresult' />
            //    <attribute name='createdon' />
            //    <attribute name='rnt_contractid' />
            //    <attribute name='rnt_installment' />
            //    <attribute name='rnt_paymentprovider' />
            //    <attribute name='rnt_virtualposid' />
            //    <order attribute='createdon' descending='true' />
            //    <filter type='and'>
            //      <condition attribute='rnt_transactionresult' operator='in'>
            //        <value>1</value>
            //        <value>4</value>
            //      </condition>
            //      <condition attribute='rnt_paymentprovider' operator='eq' value='2' />
            //    </filter>
            //    <link-entity name='rnt_reservation' from='rnt_reservationid' to='rnt_reservationid' visible='false' link-type='outer' alias='a_80eeb86dc7a8e911a840000d3a2ddc03'>
            //      <attribute name='rnt_reservationnumber' />
            //    </link-entity>
            //    <link-entity name='rnt_contract' from='rnt_contractid' to='rnt_contractid' visible='false' link-type='outer' alias='a_e4a99d67c7a8e911a840000d3a2ddc03'>
            //      <attribute name='rnt_pickupbranchid' />
            //    </link-entity>
            //    <link-entity name='rnt_creditcardslip' from='rnt_creditcardslipid' to='rnt_creditcardslipid' link-type='inner' alias='aa'>
            //      <attribute name='rnt_currentaccountcodeoutput' />
            //      <attribute name='statuscode' />
            //      <attribute name='rnt_name' />
            //      <filter type='and'>
            //        <condition attribute='statuscode' operator='in'>
            //          <value>100000000</value>
            //          <value>100000002</value>
            //        </condition>
            //      </filter>
            //    </link-entity>
            //  </entity>
            //</fetch>";

            //            EntityCollection entityCollection = orgService.RetrieveMultiple(new FetchExpression(query));
            //            foreach (var item in entityCollection.Entities)
            //            {
            //                EntityReference creditCardSlipRef = item.GetAttributeValue<EntityReference>("rnt_creditcardslipid");

            //                ExecuteWorkflowRequest updateInvoiceReuqest = new ExecuteWorkflowRequest()
            //                {
            //                    WorkflowId = new Guid("89A9D254-F9C6-48D4-B494-F192F5082B64"),
            //                    EntityId = creditCardSlipRef.Id
            //                };
            //                orgService.Execute(updateInvoiceReuqest);
            //                //Entity entity = new Entity(item.LogicalName, item.Id);
            //                //entity["rnt_virtualposid"] = "3";
            //                //orgService.Update(entity);
            //            }

            //            return;

            //            LoggerHelper loggerHelper = new LoggerHelper();
            //            ContractItemRepository contractItemRepository = new ContractItemRepository(orgService);
            //            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(orgService);
            //            DateTime startDate = DateTime.Now.AddDays(-10);


            //            RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository priceCalculationSummariesRepository = new RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository(mongoDBHostName, mongoDBDatabaseName);
            //            PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(mongoDBHostName, mongoDBDatabaseName);
            //            var activeContractItems = contractItemRepository.getActiveContractItemBetweenGivenDates(startDate, new string[] { "rnt_mongodbtrackingnumber", "rnt_contractduration", "rnt_contractid", "rnt_dropoffdatetime", "rnt_campaignid" });
            //            foreach (var activeContractItem in activeContractItems.Entities)
            //            {
            //                Console.WriteLine($"Entity {activeContractItems.Entities.Count} ContractItemId Index {activeContractItems.Entities.IndexOf(activeContractItem)}");
            //                string mongodbtrackingnumber = activeContractItem.GetAttributeValue<string>("rnt_mongodbtrackingnumber");
            //                DateTime dropoffDatetime = activeContractItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
            //                int duration = activeContractItem.GetAttributeValue<int>("rnt_contractduration");
            //                EntityReference contractRef = activeContractItem.GetAttributeValue<EntityReference>("rnt_contractid");
            //                EntityReference campaignRef = activeContractItem.GetAttributeValue<EntityReference>("rnt_campaignid");

            //                AliasedValue pricingGroupcodeAlised = activeContractItem.GetAttributeValue<AliasedValue>("contractLink.rnt_pricinggroupcodeid");
            //                EntityReference pricingGroupCodeRef = (EntityReference)pricingGroupcodeAlised.Value;
            //                var crmPriceCalculationSummariesTrackingNumbers = priceCalculationSummariesRepository.getPriceCalculationSummariesByTrackingNumber(mongodbtrackingnumber);
            //                var mongoList = crmPriceCalculationSummariesTrackingNumbers.Where(x => x.relatedGroupCodeId == Convert.ToString(pricingGroupCodeRef.Id)).ToList();
            //                if (campaignRef != null && campaignRef.Id != Guid.Empty)
            //                {
            //                    mongoList = mongoList.Where(x => x.campaignId == campaignRef.Id || x.campaignId == StaticHelper.dummyCampaignId).ToList();

            //                    var grouped = mongoList
            //                                 .GroupBy(n => n.priceDateTimeStamp)
            //                                 .Select(c => new { Key = c.Key, total = c.Count() })
            //                                 .Where(p => p.total > 1)
            //                                 .Select(p => p.Key)
            //                                 .ToList();

            //                    var duplicateDates = mongoList
            //                                        .Where(p => grouped.Contains(p.priceDateTimeStamp))
            //                                        .ToList();

            //                    var willDuplicateItems = duplicateDates.Where(p => p.campaignId.Equals(StaticHelper.dummyCampaignId)).ToList();
            //                    mongoList = mongoList.Except(willDuplicateItems).ToList();

            //                }
            //                else
            //                {
            //                    mongoList = mongoList.Where(x => x.campaignId == Guid.Empty).ToList();
            //                }
            //                if (mongoList.Count != duration && mongoList.Count != duration + 1)
            //                {
            //                    if (mongoList.Count > duration + 1)
            //                    {
            //                        Console.WriteLine($"Fazla ContactNumber {contractRef.Name} ContractItemId {activeContractItem.Id} DropoffDate {dropoffDatetime}");
            //                        loggerHelper.traceInfo($"Fazla ContactNumber {contractRef.Name} ContractItemId {activeContractItem.Id} DropoffDate {dropoffDatetime}");
            //                        Entity updateContractItemEntity = new Entity(activeContractItem.LogicalName, activeContractItem.Id);
            //                        updateContractItemEntity.Attributes["rnt_mongodbintegrationtrigger"] = "9999";
            //                        orgService.Update(updateContractItemEntity);
            //                    }
            //                    else if (mongoList.Count < duration - 1)
            //                    {
            //                        Console.WriteLine($"Eksik ContactNumber {contractRef.Name} ContractItemId {activeContractItem.Id} DropoffDate {dropoffDatetime}");
            //                        loggerHelper.traceInfo($"Eksik ContactNumber {contractRef.Name} ContractItemId {activeContractItem.Id} DropoffDate {dropoffDatetime}");
            //                    }
            //                }
            //            }
        }

        public static void contractItemProcess(EntityReference contractRef, IOrganizationService orgService)
        {
            if (contractRef != null)
            {
                RntCar.BusinessLibrary.ContractItemRepository contractItemRepository = new RntCar.BusinessLibrary.ContractItemRepository(orgService);
                var items = contractItemRepository.getCompletedContractItemsByContractId(contractRef.Id);

                ContractRepository contractRepository = new ContractRepository(orgService);
                var contract = contractRepository.getContractById(contractRef.Id, new string[] {"rnt_contracttypecode",
                                                                                                "rnt_paymentmethodcode",
                                                                                                "rnt_corporateid",
                                                                                                "transactioncurrencyid" });
                var paymentMethodCode = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
                var contractType = contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value;

                XrmHelper xrmHelper = new XrmHelper(orgService);

                foreach (var item in items)
                {
                    InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(orgService);

                    var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemId(item.Id);

                    if (invoiceItem == null)
                    {
                        Entity createInvoiceItem = xrmHelper.initializeFromRequest("rnt_contractitem", item.Id, "rnt_invoiceitem");

                        Entity invoice = null;

                        if (paymentMethodCode != (int)rnt_PaymentMethodCode.PayBroker &&
                            paymentMethodCode != (int)rnt_PaymentMethodCode.LimitedCredit)
                        {
                            InvoiceRepository invoiceRepository = new InvoiceRepository(orgService);
                            invoice = invoiceRepository.getFirstAvailableInvoiceByContractId(contractRef.Id).FirstOrDefault();
                            if (invoice != null)
                            {
                                createInvoiceItem["rnt_invoiceid"] = new EntityReference(invoice.LogicalName, invoice.Id);
                                createInvoiceItem["rnt_name"] = item.GetAttributeValue<string>("rnt_name");
                            }
                        }
                        else if (paymentMethodCode == (int)rnt_PaymentMethodCode.LimitedCredit)
                        {
                            //default fatura bireyseldir.
                            if (item.GetAttributeValue<OptionSetValue>("rnt_billingtype").Value == (int)rnt_BillingTypeCode.Corporate)
                            {
                                invoice = corporateOperations(orgService, contract);
                            }
                            else
                            {
                                InvoiceRepository invoiceRepository = new InvoiceRepository(orgService);
                                invoice = invoiceRepository.getFirstAvailableInvoiceByContractId(contractRef.Id).FirstOrDefault();
                            }
                        }
                        else
                        {
                            if (item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment)
                            {
                                InvoiceRepository invoiceRepository = new InvoiceRepository(orgService);
                                invoice = invoiceRepository.getFirstAvailableInvoiceByContractId(contractRef.Id).FirstOrDefault();
                            }
                            else if (item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.PriceDifference &&
                                    paymentMethodCode == (int)rnt_PaymentMethodCode.PayBroker)
                            {
                                invoice = corporateOperations(orgService, contract);
                            }
                            else
                            {
                                InvoiceRepository invoiceRepository = new InvoiceRepository(orgService);
                                invoice = invoiceRepository.getFirstAvailableInvoiceByContractId(contractRef.Id).FirstOrDefault();
                            }
                        }

                        if (invoice != null)
                        {
                            createInvoiceItem["rnt_invoiceid"] = new EntityReference(invoice.LogicalName, invoice.Id);
                            createInvoiceItem["rnt_name"] = item.GetAttributeValue<string>("rnt_name");
                            //createInvoiceItem["rnt_invoicedate"] = item.Attributes.Contains("rnt_invoicedate") ? item.GetAttributeValue<DateTime>("rnt_invoicedate").Date : DateTime.UtcNow.AddMinutes(StaticHelper.offset);
                            createInvoiceItem["rnt_netamount"] = item.GetAttributeValue<Money>("rnt_netamount").Value;
                            orgService.Create(createInvoiceItem);
                        }
                    }
                }
            }
        }

        private static void CreditCardSlipSendToLogo(CrmServiceHelper crmServiceHelper)
        {
            string creditCardSlipQuery = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='rnt_creditcardslip'>
    <attribute name='statuscode' />
    <attribute name='createdon' />
    <attribute name='rnt_paymentid' />
    <order attribute='createdon' descending='true' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='statuscode' operator='in'>
        <value>100000002</value>
        <value>100000000</value>
      </condition>
    </filter>
    <link-entity name='rnt_payment' from='rnt_paymentid' to='rnt_paymentid' visible='false' link-type='outer' alias='paymentAlias'>
      <attribute name='rnt_contactid' />
      <attribute name='rnt_customercreditcardid' />
      <attribute name='rnt_transactiontypecode' />
      <attribute name='rnt_transactionresult' />
    </link-entity>
  </entity>
</fetch>";
            EntityCollection creditCardSlipList = crmServiceHelper.IOrganizationService.RetrieveMultiple(new FetchExpression(creditCardSlipQuery));
            foreach (var creditCardSlip in creditCardSlipList.Entities)
            {
                Console.WriteLine(creditCardSlipList.Entities.IndexOf(creditCardSlip));
                Entity updateCreditCardSlip = new Entity(creditCardSlip.LogicalName, creditCardSlip.Id);
                updateCreditCardSlip["rnt_currentaccountcodeinput"] = null;
                updateCreditCardSlip["rnt_currentaccountcodeoutput"] = null;
                updateCreditCardSlip["rnt_creditcardslipinput"] = null;
                updateCreditCardSlip["rnt_creditcardslipoutput"] = null;
                updateCreditCardSlip["rnt_internalerrormessage"] = null;
                updateCreditCardSlip["rnt_plugreference"] = null;
                updateCreditCardSlip["rnt_plugnumber"] = null;
                crmServiceHelper.IOrganizationService.Update(updateCreditCardSlip);

                EntityReference paymentRef = creditCardSlip.GetAttributeValue<EntityReference>("rnt_paymentid");
                if (paymentRef != null && paymentRef.Id != Guid.Empty)
                {
                    Entity updatePayment = new Entity(paymentRef.LogicalName, paymentRef.Id);
                    updatePayment["rnt_transactionresult"] = null;
                    crmServiceHelper.IOrganizationService.Update(updatePayment);


                    updatePayment["rnt_transactionresult"] = new OptionSetValue(1);
                    crmServiceHelper.IOrganizationService.Update(updatePayment);

                }
            }
        }

        public static void updateCampaings()
        {


            RntCar.MongoDBHelper.Repository.CampaignRepository campaignRepository =
                new RntCar.MongoDBHelper.Repository.CampaignRepository(mongoDBHostName, mongoDBDatabaseName);

            var mongoFactors = campaignRepository.getAllCampaings();


            foreach (var item in mongoFactors)
            {
                var c = campaignRepository.getCollection<CampaignDataMongoDB>("Campaigns");

                c.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });

            }

        }
        public static void CalculateAvability()
        {
            string calculatePricesforUpdateContractParameters = @"{'contractId':'a9c04de9-5156-eb11-a812-0022487f45c1','dropoffDateTime':'2021-11-09T10:21:00.000Z','dropoffBranchId':'2f15ad20-39d9-e911-a854-000d3a2dd1b4','passValidation':false,'langId':1033}";
            //string calculatePricesforUpdateContractParameters = @"{ 'contractId':'b46bc5bc-a2dd-eb11-bacb-000d3abfc705','dropoffDateTime':'2021-07-12T15:24:04.000Z','dropoffBranchId':'211fa0ef-2111-ea11-a811-000d3a2c0643','passValidation':true,'langId':1033}";
            var param = JsonConvert.DeserializeObject<CalculatePricesForUpdateContractParameters>(calculatePricesforUpdateContractParameters);
            IOrganizationService orgService = new CrmServiceHelper().IOrganizationService;
            ContractRepository contractRepository = new ContractRepository(orgService);
            var contractInformation = contractRepository.getContractById(param.contractId,
                                                                         new string[] {  "rnt_contracttypecode",
                                                                                              "rnt_corporateid",
                                                                                              "rnt_pickupbranchid",
                                                                                              "rnt_pickupdatetime",
                                                                                              "rnt_dropoffdatetime",
                                                                                              "rnt_ismonthly",
                                                                                              "rnt_groupcodeid",
                                                                                              "rnt_offset",
                                                                                              "rnt_customerid",
                                                                                              "rnt_pricinggroupcodeid"});

            ContractUpdateValidation contractUpdateValidation = new ContractUpdateValidation(orgService);
            var updateRes = contractUpdateValidation.checkMonthlyValidations(contractInformation, param.dropoffDateTime.AddMinutes(StaticHelper.offset), param.passValidation, false);

            if (!updateRes.ResponseResult.Result)
            {
            }

            AvailabilityValidation availabilityValidation = new AvailabilityValidation(orgService);
            var r = availabilityValidation.checkBrokerReservation_Contract(new AvailabilityParameters
            {
                contractId = contractInformation.Id.ToString(),
                pickupDateTime = contractInformation.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                dropoffDateTime = param.dropoffDateTime.AddMinutes(StaticHelper.offset)
            }, 1055);

            if (!r.ResponseResult.Result)
            {
            }
            RntCar.BusinessLibrary.ContractItemRepository contractItemRepository = new RntCar.BusinessLibrary.ContractItemRepository(orgService);
            var equipment = contractItemRepository.getRentalandWaitingforDeliveryEquipmentContractItemsByContractId(param.contractId, new string[] {"rnt_dropoffdatetime",
                                                                                                                                                        "rnt_pickupdatetime" }).FirstOrDefault();

            //araç değişikliğinde saat uyusmazlıgı fix
            //araç değişikliğinde teslim ederken arada olusan dakika uyusmazlıgını gidermek için
            //ayrıca sayfa ilk yüklendiğinde araç değişikliği varsa fiyat farkı cıkarmaması için
            if (param.dropoffDateTime.AddMinutes(StaticHelper.offset) == contractInformation.GetAttributeValue<DateTime>("rnt_dropoffdatetime"))
            {
                param.dropoffDateTime = equipment.GetAttributeValue<DateTime>("rnt_dropoffdatetime").AddMinutes(-StaticHelper.offset);
            }


            var utcpickupTime = equipment.GetAttributeValue<DateTime>("rnt_pickupdatetime");
            //these fields are timezone independent so we need to make them timezone
            utcpickupTime = utcpickupTime.AddMinutes(-StaticHelper.offset);

            AvailabilityHelper availabilityHelper = new AvailabilityHelper(orgService);
            var response = availabilityHelper.calculateAvailability_Contract(contractInformation,
                                                                             param.dropoffBranchId,
                                                                             param.dropoffDateTime,
                                                                             utcpickupTime,
                                                                             param.langId,
                                                                             param.isMonthly);
        }

        public static void UpdateDate(string contractId, ContractDateandBranchParameters dateandBranchParameter)
        {
            IOrganizationService orgService = new CrmServiceHelper().IOrganizationService;
            RntCar.BusinessLibrary.Business.ContractBL contractBL = new RntCar.BusinessLibrary.Business.ContractBL(orgService);

            var response = contractBL.getContractDataForUpdate(contractId, dateandBranchParameter, 1055);
            response.additionalProducts.ForEach(p => p.productDescription = null);
            //sözleşme güncelleşme ekranında gözükmesini istemiyor ama sözleşmeye eklenmişse.
            foreach (var item in response.additionalProducts)
            {
                if (item.showOnContractUpdate == false && item.value >= 1)
                {
                    item.isMandatory = true;
                }
            }
            response.additionalProducts = response.additionalProducts.Where(p => p.showOnContractUpdate == true || (p.showOnContractUpdate == false && p.isMandatory == true)).ToList();

            ContractHelper contractHelper = new ContractHelper(orgService);
            var oneWayFeeResponse = contractHelper.calculateNewOneWayFeeAmount(new Guid(contractId),
                                                                               dateandBranchParameter.dropoffBranchId);

            if (oneWayFeeResponse.amount != decimal.Zero)
            {
                var oneWayProduct = response.additionalProducts.Where(p => p.productId == oneWayFeeResponse.additionalProductId).FirstOrDefault();
                oneWayProduct.actualAmount = oneWayFeeResponse.amount;
                oneWayProduct.actualTotalAmount = oneWayFeeResponse.amount;
            }

            ContractRepository contractRepository = new ContractRepository(orgService);
            var con = contractRepository.getContractById(new Guid(contractId), new string[] { "rnt_ismonthly", "rnt_howmanymonths", "rnt_cancontinuewithmonthly" });

            response.canContinueMonthly = con.Contains("rnt_cancontinuewithmonthly") ? con.GetAttributeValue<bool>("rnt_cancontinuewithmonthly") : false;
            response.isMonthly = con.Contains("rnt_ismonthly") ? con.GetAttributeValue<bool>("rnt_ismonthly") : false;
            response.howManyMonths = con.Contains("rnt_howmanymonths") ? con.GetAttributeValue<OptionSetValue>("rnt_howmanymonths").Value : 0;
            //aylıklarda hiçbir ek ürün eklenemez 
            List<AdditionalProductData> excluded = new List<AdditionalProductData>();
            if (response.isMonthly)
            {
                foreach (var item in response.additionalProducts)
                {
                    if (item.value == 0)
                    {
                        excluded.Add(item);
                    }
                }
                response.additionalProducts = response.additionalProducts.Except(excluded).ToList();
            }
        }
        public static void additionalproductsforrentals()
        {
            var updateContractforRentalParametersSerialized = JsonConvert.DeserializeObject<UpdateContractforRentalParameters>(" {'dateNow':1612703160,'equipmentInformation':{'equipmentId':'3d363a99-3409-eb11-a812-000d3a2c0274','groupCodeInformationId':'8fa24119-27b2-e911-a851-000d3a2dd1b4','plateNumber':'34DIA603','currentKmValue':5675,'firstKmValue':178,'currentFuelValue':0,'firstFuelValue':0,'isEquipmentChanged':false,'equipmentInventoryData':[{'isExist':true,'inventoryName':'Ruhsat','logicalName':'rnt_license','equipmentInventoryId':'dc82c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':true,'inventoryName':'İlk Yardım Çantası','logicalName':'rnt_firstaidkit','equipmentInventoryId':'de82c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':true,'inventoryName':'Paspas','logicalName':'rnt_floormat','equipmentInventoryId':'e082c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Küllük','logicalName':'rnt_ashtray','equipmentInventoryId':'e282c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Bebek Koltuğu','logicalName':'rnt_babyseat','equipmentInventoryId':'e482c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Çakmak','logicalName':'rnt_lighter','equipmentInventoryId':'e682c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':true,'inventoryName':'Sigorta Poliçesi','logicalName':'rnt_insurancepolicy','equipmentInventoryId':'e882c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Navigasyon Paketi','logicalName':'rnt_navigationpack','equipmentInventoryId':'ea82c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Yangın Söndürme Tüpü','logicalName':'rnt_fireextinguisher','equipmentInventoryId':'ec82c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':true,'inventoryName':'Avadanlık (Yedek Lastik, Kriko, Bijon Anahtarı)','logicalName':'rnt_toolset','equipmentInventoryId':'ee82c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':true,'inventoryName':'Plaka','logicalName':'rnt_plate','equipmentInventoryId':'f082c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Anten','logicalName':'rnt_aerial','equipmentInventoryId':'403ff9c2-3fb2-e911-a851-000d3a2dd1b4','price':0.0},{'isExist':false,'inventoryName':'Pandizot','logicalName':'rnt_baffleboard','equipmentInventoryId':'9356f79e-4ab8-e911-a83f-000d3a2dd73b','price':0.0}]},'userInformation':{'userId':'cf4b9a11-75b3-e911-a851-000d3a2dd1b4','branchId':'289f81e9-0a12-ea11-a811-000d3a2d09d1'},'contractInformation':{'contractId':'062506a3-aa1a-eb11-a813-000d3a2c0643','contactId':'bb27a2b9-ad96-ea11-a811-000d3a2c0643','dropoffBranch':{'branchId':'289f81e9-0a12-ea11-a811-000d3a2d09d1','branchName':'İzmir Adnan Menderes Havalimanı'},'manuelPickupDateTimeStamp':0,'manuelDropoffTimeStamp':1612703160,'PickupDateTimeStamp':1604060816,'DropoffTimeStamp':1612703160,'isManuelProcess':true},'additionalProducts':[{'productId':'7bbfd297-fdf3-e911-a813-000d3a2d09d1','productName':'Taksit Farkı','productType':10,'productCode':'SRV026','maxPieces':1,'showonWeb':false,'webRank':0,'productDescription':'Taksitli yapılan işlemlerdeki vade farkı','actualAmount':0.0,'isChecked':true,'value':1,'actualTotalAmount':0.0,'isMandatory':true,'priceCalculationType':2,'monthlyPackagePrice':0.0,'tobePaidAmount':0.0,'isServiceFee':false,'billingType':10}],'otherAdditionalProducts':[],'paymentInformation':{'creditCardData':[{'creditCardId':'e41317ea-ad96-ea11-a811-000d3a2c0643','binNumber':'557023','conversationId':'1746185238','externalId':'109351','cardUserKey':'fcMmSgNTY7IKQHeHv3FJwXkB1Oo=','cardToken':'QkDjMW0kX6aw6ZuNVGmBL6IDaeQ=','creditCardNumber':'557023******0218','expireYear':2022,'expireMonth':3,'cardHolderName':'BURAK METİN - 557023******0218','cvc':null,'cardType':10}]},'damageData':[{'equipmentPart':{'equipmentPartId':'8ee5ea2a-b3b4-e911-a851-000d3a2dd1b4','equipment}");


            //PartId':'27000000','equipmentSubPartId':'27000001','equipmentSubPartName':'Diğer Hasarlar'},'damageType':{'damageTypeId':'e0a2d570-9db3-e911-a841-000d3a2ddc03','damageTypeName':'Ezik'},'damageSize':{'damageSizeId':'ae52b849-840f-e911-a950-000d3a454330','damageSizeName':'Minör'},'damageInfo':{'damageDate':0,'damageBranch':{'branchId':'289f81e9-0a12-ea11-a811-000d3a2d09d1','branchName':'İzmir Adnan Menderes Havalimanı'}},'damageDocument':{'damageDocumentId':'ba852c76-9eb3-e911-a840-000d3a2cd4e8','damageDocumentName':'Evrak Yok','damageDocumentType':2},'damageAmount':0.0,'isPriceCalculated':false,'damageId':'9cd3dfea-76c9-4641-9e07-0bfbad8374da','isRepaired':false,'blobStoragePath':'https://rentgoproductionstorage.blob.core.windows.net/equipments/34dıa603/20039806/rental/9cd3dfea-76c9-4641-9e07-0bfbad8374da'}],'otherDocuments':null,'transits':[],'fineList':[],'trackingNumber':'a3221c9b-60a6-4d73-adea-16d3b5099e65','operationType':0,'canUserStillHasCampaignBenefit':true,'campaignId':'ca743ec6-4db4-e911-a84d-000d3a2ddb03','totalAmount':14392.948586900000000000000000,'contractDeptStatus':10,'langId':1055,'channelCode':40}");
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            InvoiceRepository invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
            var invoice = invoiceRepository.getFirstInvoiceByContractId(updateContractforRentalParametersSerialized.contractInformation.contractId);

            ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
            var contract = contractRepository.getContractById(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                              new string[] { "rnt_pickupbranchid",
                                                                                  "rnt_dropoffbranchid" ,
                                                                                  "rnt_reservationid" ,
                                                                                  "rnt_totalamount",
                                                                                  "rnt_pickupdatetime",
                                                                                  "transactioncurrencyid",
                                                                                  "rnt_contracttypecode",
                                                                                  "rnt_groupcodeid",
                                                                                  "rnt_customerid",
                                                                                  "rnt_pnrnumber",
                                                                                  "statuscode",
                                                                                  "rnt_ismonthly",
                                                                                  "rnt_contracttypecode",
                                                                                  "rnt_paymentmethodcode"
                                                                           });
            RntCar.BusinessLibrary.ContractItemRepository contractItemRepository = new RntCar.BusinessLibrary.ContractItemRepository(crmServiceHelper.IOrganizationService);
            var items = contractItemRepository.getRentalContractItemsByContractIdByGivenColumns(new Guid("062506a3-aa1a-eb11-a813-000d3a2c0643"),
                                                                                                       new string[] { "rnt_pickupdatetime", "rnt_itemtypecode", "rnt_additionalproductid" });
            //initializer.TraceMe("ContractItemRepository end");
            ContractHelper contractHelper = new ContractHelper(crmServiceHelper.IOrganizationService);
            var duration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(updateContractforRentalParametersSerialized.contractInformation.PickupDateTimeStamp.converttoDateTime(),
                                                                                           updateContractforRentalParametersSerialized.contractInformation.DropoffTimeStamp.converttoDateTime());
            //additional product operations                
            var contractUpdateParameter = new AdditionalProductMapper()
                                         .buildUpdateAdditionalProductsForContractParameter(contract,
                                          updateContractforRentalParametersSerialized,
                                          false,
                                          true,
                                          (int)ContractItemEnums.ChangeReason.CustomerDemand,
                                          duration);
            //initializer.TraceMe("contractUpdateParameter" + contractUpdateParameter);

            contractUpdateParameter.contractItemStatusCode = (int)rnt_contractitem_StatusCode.Completed;
            AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(crmServiceHelper.IOrganizationService);
            additionalProductsBL.updateAdditionalProductsForContract(
            contractUpdateParameter,
            items,
            invoice.Id,
            updateContractforRentalParametersSerialized.channelCode,
            null);
        }
        public static void updatehgs()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            string builder = "";
            List<Entity> result = new List<Entity>();
            int queryCount = 5000;
            int pageNumber = 1;
            string pagingCookie = null;

            while (true)
            {



                QueryExpression query = new QueryExpression("rnt_hgstransitlist");
                query.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };

                query.ColumnSet = new ColumnSet(true);
                query.Criteria.AddCondition("createdon", ConditionOperator.LastXMonths, 4);
                var res = crmServiceHelper.IOrganizationService.RetrieveMultiple(query);

                if (res.MoreRecords)
                {
                    pageNumber++;
                    pagingCookie = res.PagingCookie;
                    result.AddRange(res.Entities.ToList());
                }
                else
                {
                    result.AddRange(res.Entities.ToList());
                    break;
                }
            }
            var i = 1;
            foreach (var item in result)
            {
                Entity e = new Entity("rnt_hgstransitlist");
                e.Id = item.Id;

                var entry = item.GetAttributeValue<string>("rnt_entrylocation");
                var exit = item.GetAttributeValue<string>("rnt_exitlocation");

                if (!string.IsNullOrEmpty(entry))
                {
                    entry = entry.Replace("- ONL. AUTH.", "").Replace(" - ONL. AUTH.", "");
                }
                if (!string.IsNullOrEmpty(exit))
                {
                    exit = exit.Replace("- ONL. AUTH.", "").Replace(" - ONL. AUTH.", "");
                }
                e["rnt_entrylocation"] = entry;
                e["rnt_exitlocation"] = exit;
                crmServiceHelper.IOrganizationService.Update(e);
                builder += item.Id;
                builder += Environment.NewLine;
                Console.WriteLine(i.ToString());
                i++;
            }
            File.WriteAllText("a.txt", builder);
        }
        public static void getWebServiceLogs()
        {
            var client = new MongoClient(StaticHelper.GetConfiguration("MongoDBHostName"));
            var database = client.GetDatabase(StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var collection = database.GetCollection<LogData>("WebServiceLogs");

            var l = collection
            .AsQueryable()
            .Where(p => p.message.Contains("219a72c3-0a62-eb11-a812-000d3a275764")).ToList();

        }
        public static void importOlddata()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            EquipmentAvailabilityBusiness equipmentAvailabilityBusiness = new EquipmentAvailabilityBusiness(mongoDBHostName, mongoDBDatabaseName, new string[] { });

            var collect = equipmentAvailabilityBusiness.getCollection<EquipmentAvailabilityOldDataMongoDB>("oldequipmentdata");

            using (var package = new ExcelPackage(new FileInfo(@"C:\Users\RentGo\Desktop\betodata.xlsx")))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var firstSheet = package.Workbook.Worksheets["Sheet1"];

                int colCount = firstSheet.Dimension.End.Column;  //get Column Count
                int rowCount = firstSheet.Dimension.End.Row;     //get row count
                for (int row = 1; row < rowCount; row++)
                {
                    var branch = firstSheet.Cells[row + 1, 1].Text;
                    QueryExpression queryExpression = new QueryExpression("rnt_branch");
                    queryExpression.Criteria.AddCondition("rnt_name", ConditionOperator.Like, "%" + branch + "%");
                    queryExpression.ColumnSet = new ColumnSet(true);
                    var r = crmServiceHelper.IOrganizationService.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();


                    var equipmentAvailabilityItem = new EquipmentAvailabilityOldDataMongoDB();
                    equipmentAvailabilityItem._id = ObjectId.GenerateNewId();
                    equipmentAvailabilityItem.publishDate = Convert.ToDateTime(Convert.ToString(firstSheet.Cells[row + 1, 2].Text));
                    equipmentAvailabilityItem.publishDateTimeStamp = Convert.ToDateTime(Convert.ToString(firstSheet.Cells[row + 1, 2].Text)).Ticks;
                    equipmentAvailabilityItem.CurrentBranch = r.GetAttributeValue<string>("rnt_name");
                    equipmentAvailabilityItem.CurrentBranchId = r.Id;
                    equipmentAvailabilityItem.revenue = Convert.ToDecimal(Convert.ToString(firstSheet.Cells[row + 1, 3].Text));
                    equipmentAvailabilityItem.externalId = Convert.ToString(firstSheet.Cells[row + 1, 4].Text);
                    equipmentAvailabilityItem.totalDays = Convert.ToInt32(Convert.ToString(firstSheet.Cells[row + 1, 5].Text));
                    collect.Insert(equipmentAvailabilityItem, Convert.ToString(equipmentAvailabilityItem._id), "");


                }
            }
        }
        public static void ExecuteGetContractDataForUpdate()
        {
            var contractId = new Guid("c8d88ef2-2653-eb11-a812-0022487f45c1");
            var langId = 1055;
            var dateandBranchParameter = JsonConvert.DeserializeObject<ContractDateandBranchParameters>("{'pickupBranchId':'e4835251-0712-ea11-a811-000d3a2d09d1','pickupDate':'2021-01-10T09:45:58.000Z','customerCriteria':'MIKHAIL SVINAREV','dropoffBranchId':'e4835251-0712-ea11-a811-000d3a2d09d1','dropoffDate':'2021-01-23T09:45:58.000Z'}");
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            ContractBL contractBL = new ContractBL(crmServiceHelper.IOrganizationService);

            var response = contractBL.getContractDataForUpdate(contractId.ToString(), dateandBranchParameter, langId);
            response.additionalProducts.ForEach(p => p.productDescription = null);
            //sözleşme güncelleşme ekranında gözükmesini istemiyor ama sözleşmeye eklenmişse.
            foreach (var item in response.additionalProducts)
            {
                if (item.showOnContractUpdate == false && item.value >= 1)
                {
                    item.isMandatory = true;
                }
            }
            response.additionalProducts = response.additionalProducts.Where(p => p.showOnContractUpdate == true || (p.showOnContractUpdate == false && p.isMandatory == true)).ToList();

            ContractHelper contractHelper = new ContractHelper(crmServiceHelper.IOrganizationService);
            var oneWayFeeResponse = contractHelper.calculateNewOneWayFeeAmount(contractId,
                                                                               dateandBranchParameter.dropoffBranchId);

            if (oneWayFeeResponse.amount != decimal.Zero)
            {
                var oneWayProduct = response.additionalProducts.Where(p => p.productId == oneWayFeeResponse.additionalProductId).FirstOrDefault();
                oneWayProduct.actualAmount = oneWayFeeResponse.amount;
                oneWayProduct.actualTotalAmount = oneWayFeeResponse.amount;
            }

            ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
            var con = contractRepository.getContractById(contractId, new string[] { "rnt_ismonthly", "rnt_howmanymonths", "rnt_cancontinuewithmonthly" });

            response.canContinueMonthly = con.Contains("rnt_cancontinuewithmonthly") ? con.GetAttributeValue<bool>("rnt_cancontinuewithmonthly") : false;
            response.isMonthly = con.Contains("rnt_ismonthly") ? con.GetAttributeValue<bool>("rnt_ismonthly") : false;
            response.howManyMonths = con.Contains("rnt_howmanymonths") ? con.GetAttributeValue<OptionSetValue>("rnt_howmanymonths").Value : 0;
            //aylıklarda hiçbir ek ürün eklenemez 
            List<AdditionalProductData> excluded = new List<AdditionalProductData>();
            if (response.isMonthly)
            {
                foreach (var item in response.additionalProducts)
                {
                    if (item.value == 0)
                    {
                        excluded.Add(item);
                    }
                }
                response.additionalProducts = response.additionalProducts.Except(excluded).ToList();
            }

        }
        public static void invoiceFineItems()
        {
            var crmServiceHelper = new CrmServiceHelper();
            HGSHelper hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService);
            RntCar.BusinessLibrary.ContractItemRepository contractItemRepository = new RntCar.BusinessLibrary.ContractItemRepository(crmServiceHelper.IOrganizationService);
            ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
            InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(crmServiceHelper.IOrganizationService);
            InvoiceBL invoiceBL = new InvoiceBL(crmServiceHelper.IOrganizationService);
            InvoiceRepository invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
            BranchRepository branchRepository = new BranchRepository(crmServiceHelper.IOrganizationService);
            XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
            var contract = contractRepository.getContractById(new Guid("da32cbd1-9038-eb11-a813-000d3a38a128"));

            //hgsHelper.processHGSBatch("0b9f4371-8546-eb11-a812-000d3a38a128".ToString());
            var documentBranchInfo = branchRepository.getBranchById(contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id);

            //hgsler işlendikten sonra bu kalemleri faturalayalım
            var rentalItems = contractItemRepository.getRentalFineItems(contract.Id);
            Dictionary<Guid, List<Entity>> dictionarycontractItems = new Dictionary<Guid, List<Entity>>();
            foreach (var cItem in rentalItems)
            {
                var invoiceItem = invoiceItemRepository.getDraftInvoiceItemsByContractItemId(cItem.Id);
                if (invoiceItem != null)
                {
                    var iId = invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id;

                    List<Entity> checkEntity = new List<Entity>();
                    dictionarycontractItems.TryGetValue(iId, out checkEntity);
                    if (checkEntity == null)
                    {
                        checkEntity = new List<Entity>();
                        checkEntity.Add(cItem);
                        dictionarycontractItems.Add(iId, checkEntity);
                    }
                    else
                    {
                        checkEntity.Add(cItem);
                        dictionarycontractItems[iId] = checkEntity;
                    }
                }
                else
                {
                    var invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contract.Id).FirstOrDefault();
                    Entity createInvoiceItem = xrmHelper.initializeFromRequest("rnt_contractitem", cItem.Id, "rnt_invoiceitem");
                    createInvoiceItem["rnt_invoiceid"] = new EntityReference(invoice.LogicalName, invoice.Id);
                    createInvoiceItem["rnt_name"] = cItem.GetAttributeValue<string>("rnt_name");
                    //createInvoiceItem["rnt_invoicedate"] = item.Attributes.Contains("rnt_invoicedate") ? item.GetAttributeValue<DateTime>("rnt_invoicedate").Date : DateTime.UtcNow.AddMinutes(StaticHelper.offset);
                    createInvoiceItem["rnt_netamount"] = cItem.GetAttributeValue<Money>("rnt_netamount").Value;
                    crmServiceHelper.IOrganizationService.Create(createInvoiceItem);

                }
            }
            if (dictionarycontractItems.Count > 0)
            {
                foreach (var invoice in dictionarycontractItems)
                {
                    invoiceBL.documentBranchInfo = documentBranchInfo;
                    invoiceBL.getDocumentInfo(contract.Id, null);
                    var response = invoiceBL.logoOperations(invoice.Value, contract.Id, invoice.Key, 1055, false, true, true);
                    if (response.ResponseResult.Result)
                    {
                        foreach (var conItem in invoice.Value)
                        {
                            Entity e1 = new Entity(conItem.LogicalName);
                            e1["statuscode"] = new OptionSetValue((int)rnt_contractitem_StatusCode.Completed);
                            e1.Id = conItem.Id;
                            crmServiceHelper.IOrganizationService.Update(e1);
                        }
                    }
                }

            }
        }
        public static void invoiceforrentalitems()
        {
            var service = new CrmServiceHelper().IOrganizationService;
            InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(service);
            InvoiceRepository invoiceRepository = new InvoiceRepository(service);
            InvoiceBL invoiceBL = new InvoiceBL(service);
            ContractHelper contractHelper = new ContractHelper(service);

            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.ColumnSet = new ColumnSet(true);
            //queryExpression.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contract_StatusCode.Rental);
            //queryExpression.Criteria.AddCondition("rnt_ismonthly", ConditionOperator.Equal, true);
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, new Guid("a700d92b-292f-eb11-a813-0022487f45c1"));
            HGSHelper hgsHelper = new HGSHelper(service);
            var ent = service.RetrieveMultiple(queryExpression);
            var i = 1;
            foreach (var item in ent.Entities)
            {

                var d = contractHelper.calculateTotalDuration_ContractByPriceHourEffect_Decimal(item.GetAttributeValue<DateTime>("rnt_pickupdatetime"), DateTime.Now.AddMinutes(StaticHelper.offset));
                if (d < 30)
                {
                    continue;
                }
                Dictionary<Guid, List<Entity>> contractItems = new Dictionary<Guid, List<Entity>>();
                QueryExpression queryItem = new QueryExpression("rnt_contractitem");
                queryItem.ColumnSet = new ColumnSet(true);
                queryItem.Criteria.AddCondition("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Rental);
                queryItem.Criteria.AddCondition("rnt_itemtypecode", ConditionOperator.Equal, (int)rnt_contractitem_rnt_itemtypecode.Fine);
                queryItem.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, item.Id);

                var items = service.RetrieveMultiple(queryItem);

                foreach (var cItem in items.Entities)
                {
                    var invoiceItem = invoiceItemRepository.getDraftInvoiceItemsByContractItemId(cItem.Id);
                    if (invoiceItem != null)
                    {
                        var iId = invoiceItem.GetAttributeValue<EntityReference>("rnt_invoiceid").Id;

                        List<Entity> checkEntity = new List<Entity>();
                        contractItems.TryGetValue(iId, out checkEntity);
                        if (checkEntity == null)
                        {
                            checkEntity = new List<Entity>();
                            checkEntity.Add(cItem);
                            contractItems.Add(iId, checkEntity);
                        }
                        else
                        {
                            checkEntity.Add(cItem);
                            contractItems[iId] = checkEntity;
                        }

                    }

                }
                BranchRepository branchRepository = new BranchRepository(service);
                var documentBranchInfo = branchRepository.getBranchById(item.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id);
                if (contractItems.Count > 0)
                {
                    foreach (var invoice in contractItems)
                    {
                        invoiceBL.documentBranchInfo = documentBranchInfo;
                        invoiceBL.getDocumentInfo(item.Id, null);
                        var response = invoiceBL.logoOperations(invoice.Value, item.Id, invoice.Key, 1055, false, true, true);
                        if (response.ResponseResult.Result)
                        {
                            foreach (var conItem in invoice.Value)
                            {
                                Entity e = new Entity(conItem.LogicalName);
                                e["statuscode"] = new OptionSetValue((int)rnt_contractitem_StatusCode.Completed);
                                e.Id = conItem.Id;
                                service.Update(e);
                            }
                        }
                    }

                }
                Console.WriteLine(i);
                i++;

            }
        }
        public static void manuelinforeport()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            EquipmentAvailabilityBusiness equipmentAvailabilityBusiness = new EquipmentAvailabilityBusiness(mongoDBHostName, mongoDBDatabaseName, new string[] { });

            var collect = equipmentAvailabilityBusiness.getCollection<EquipmentAvailabilityDataMongoDB>(StaticHelper.GetConfiguration("MongoDBEquipmentAvailabilityCollectionName"));

            using (var package = new ExcelPackage(new FileInfo(@"C:\Users\RentGo\Desktop\betodata.xlsx")))
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var firstSheet = package.Workbook.Worksheets["Sayfa1"];

                int colCount = firstSheet.Dimension.End.Column;  //get Column Count
                int rowCount = firstSheet.Dimension.End.Row;     //get row count
                for (int row = 1; row < rowCount; row++)
                {
                    var branch = firstSheet.Cells[row + 1, 1].Text;
                    QueryExpression queryExpression = new QueryExpression("rnt_branch");
                    queryExpression.Criteria.AddCondition("rnt_name", ConditionOperator.Like, "%" + branch + "%");
                    queryExpression.ColumnSet = new ColumnSet(true);
                    var r = crmServiceHelper.IOrganizationService.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();

                    for (int col = 2; col <= colCount; col = col + 2)
                    {

                        var x = Convert.ToDouble(firstSheet.Cells[row, col].Text);
                        var y = Convert.ToDouble(firstSheet.Cells[row, col + 1].Text);
                        var equipmentAvailabilityItem = new EquipmentAvailabilityDataMongoDB();
                        equipmentAvailabilityItem._id = ObjectId.GenerateNewId();
                        equipmentAvailabilityItem.PublishDate = Convert.ToDateTime("23.01.2020").Ticks;
                        equipmentAvailabilityItem.RentalCount = y;

                        equipmentAvailabilityItem.AvailableCount = x - y;
                        equipmentAvailabilityItem.CurrentBranch = r.GetAttributeValue<string>("rnt_name");
                        equipmentAvailabilityItem.CurrentBranchId = r.Id;
                        equipmentAvailabilityItem.GroupCodeInformationId = new Guid("6ada7ad1-23b2-e911-a851-000d3a2dd1b4");

                        equipmentAvailabilityItem.Availability = (Convert.ToDouble(x) / Convert.ToDouble(y)) * 100;
                        collect.Insert(equipmentAvailabilityItem, Convert.ToString(equipmentAvailabilityItem._id), "");


                        Console.WriteLine(" Row:" + row + " column:" + col + " Value:" + firstSheet.Cells[row, col].Value?.ToString().Trim());
                    }

                    for (int i = 0; i < colCount; i++)
                    {

                    }

                }
            }
        }
        public static void getBrokerLogs()
        {
            RntCar.MongoDBHelper.Entities.BrokerAvailabilityLogBusiness business = new RntCar.MongoDBHelper.Entities.BrokerAvailabilityLogBusiness(mongoDBHostName, mongoDBDatabaseName);
            var res = business.getCollection<BrokerAvailabilityLog>("brokeravailabilitylogs")
            .AsQueryable()
            .Where(p => p.brokerCode.Equals("YOLCU360") &&
            (p.content.Contains("a3a0198f-9fdd-ea11-a813-000d3a38a128") || p.content.Contains("7a40bbc6-d4f2-ea11-a815-000d3a2c0643"))).ToList();

            // var r = res.Where(p => p.totalSeconds >= 5).ToList();
            var x = res.Where(p => p.startTime.Date >= DateTime.UtcNow.Date).ToList();
            foreach (var item in x)
            {
                Console.WriteLine(JsonConvert.SerializeObject(item));
                Console.WriteLine("*******");

            }
            Console.ReadKey();
        }

        public static void fillNetAmount()
        {
            var s = new CrmServiceHelper().IOrganizationService;
            var fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='rnt_invoiceitem'>
                            <attribute name='rnt_invoiceitemid' />
                            <attribute name='rnt_totalamount' />
                            <attribute name='createdon' />
                            <order attribute='rnt_name' descending='false' />
                            <filter type='and'>
                              <condition attribute='rnt_netamount' operator='null' />
                            </filter>
                          </entity>
                        </fetch>";
            var res = s.RetrieveMultiple(new FetchExpression(fetch));
            foreach (var item in res.Entities)
            {
                try
                {
                    Entity e = new Entity("rnt_invoiceitem");
                    e.Id = item.Id;
                    e["rnt_netamount"] = item.GetAttributeValue<Money>("rnt_totalamount").Value / 1.18M;
                    s.Update(e);
                }
                catch (Exception)
                {
                    continue;
                }

            }
        }
        public static void InvoiceCalculatorForHGS()
        {
            IOrganizationService OrgService = new CrmServiceHelper().IOrganizationService;
            string diffFecth = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                   <entity name='rnt_contract'>
                                     <attribute name='rnt_contractnumber' />
                                     <attribute name='rnt_contractid' />
                                     <attribute name='rnt_corporatetotalamount' />
                                     <attribute name='rnt_billedamount' />
                                     <attribute name='rnt_totalamount' />
                                     <attribute name='rnt_generaltotalamount' />
                                     <attribute name='createdon' />
                                     <order attribute='createdon' descending='true' />
                                     <filter type='and'>
                                       <condition attribute='statuscode' operator='eq' value='100000001' />
                                       <condition attribute='rnt_billedamount' operator='lt' valueof='rnt_generaltotalamount' />
                                       <condition attribute='rnt_billedamount' operator='gt' value='0' />
                                       <condition attribute='rnt_dropoffdatetime' operator='on-or-before' value='2021-08-16' />
                                     </filter>
                                   </entity>
                                 </fetch>";
            EntityCollection diffContractList = OrgService.RetrieveMultiple(new FetchExpression(diffFecth));
            foreach (var diffContract in diffContractList.Entities)
            {

                string contractNumber = diffContract.GetAttributeValue<string>("rnt_contractnumber");
                Money corporateTotalAmount = diffContract.GetAttributeValue<Money>("rnt_corporatetotalamount");
                Money billedAmount = diffContract.GetAttributeValue<Money>("rnt_billedamount");
                Money totalAmount = diffContract.GetAttributeValue<Money>("rnt_totalamount");
                Money generalTotalAmount = diffContract.GetAttributeValue<Money>("rnt_generaltotalamount");
                if (billedAmount.Value >= generalTotalAmount.Value)
                {
                    continue;
                }

                CalculateRollupFieldRequest calculateRollupFieldRequest = new CalculateRollupFieldRequest
                {
                    Target = new EntityReference(diffContract.LogicalName, diffContract.Id),
                    FieldName = "rnt_billedamount" // Rollup Field Name
                };
                CalculateRollupFieldResponse response = (CalculateRollupFieldResponse)OrgService.Execute(calculateRollupFieldRequest);

                Entity tempEntity = OrgService.Retrieve(diffContract.LogicalName, diffContract.Id, new ColumnSet("rnt_corporatetotalamount", "rnt_billedamount", "rnt_totalamount", "rnt_generaltotalamount"));

                corporateTotalAmount = tempEntity.GetAttributeValue<Money>("rnt_corporatetotalamount");
                billedAmount = tempEntity.GetAttributeValue<Money>("rnt_billedamount");
                totalAmount = tempEntity.GetAttributeValue<Money>("rnt_totalamount");
                generalTotalAmount = tempEntity.GetAttributeValue<Money>("rnt_generaltotalamount");

                if (billedAmount.Value < generalTotalAmount.Value)
                {
                    Console.WriteLine(contractNumber);
                    Console.WriteLine(diffContractList.Entities.IndexOf(diffContract));
                    ///Kontrat'a bağlı default invoice çekilir yoksa bu kayıt geçilir. Default fatura wf nin çalıştığı faturadır. Bu nedenle deafult fatura üzerinde işlem yapılır.
                    string invoiceFecth = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                              <entity name='rnt_invoice'>
                                                <attribute name='createdon' />
                                                <attribute name='rnt_taxofficeid' />
                                                <attribute name='rnt_taxnumber' />
                                                <attribute name='rnt_mobilephone' />
                                                <attribute name='rnt_lastname' />
                                                <attribute name='rnt_invoicetypecode' />
                                                <attribute name='rnt_govermentid' />
                                                <attribute name='rnt_firstname' />
                                                <attribute name='rnt_email' />
                                                <attribute name='rnt_districtid' />
                                                <attribute name='rnt_customerinvoiceaddressid' />
                                                <attribute name='rnt_countryid' />
                                                <attribute name='rnt_contractid' />
                                                <attribute name='rnt_companyname' />
                                                <attribute name='rnt_cityid' />
                                                <attribute name='rnt_addressdetail' />
                                                <attribute name='rnt_defaultinvoice' />
                                                <order attribute='createdon' descending='false' />
                                                <filter type='and'>
                                                  <condition attribute='rnt_contractid' operator='eq' value='{0}'/>
                                                  <condition attribute='rnt_defaultinvoice' operator='eq' value='1'/>
                                                </filter>
                                              </entity>
                                            </fetch>";


                    EntityCollection invoiceList = OrgService.RetrieveMultiple(new FetchExpression(string.Format(invoiceFecth, diffContract.Id)));
                    Entity invoice = new Entity();
                    if (invoiceList.Entities.Count == 1)
                    {
                        invoice = invoiceList.Entities[0];
                    }
                    else
                    {
                        continue;
                    }


                    ///Kontrat'a bağlı default invoice dışında draft statüde fatura varsa bu çekilir.
                    string invoiceCheckFecth = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='rnt_invoice'>
    <attribute name='createdon' />
    <attribute name='rnt_defaultinvoice' />
    <order attribute='createdon' descending='false' />
    <filter type='and'>
      <condition attribute='rnt_contractid' operator='eq' value='{0}'/>
      <condition attribute='statuscode' operator='eq' value='1' />
    </filter>
  </entity>
</fetch>";

                    EntityCollection invoiceCheckList = OrgService.RetrieveMultiple(new FetchExpression(string.Format(invoiceCheckFecth, diffContract.Id)));

                    Entity newInvoice = new Entity();
                    if (invoiceCheckList.Entities.Count == 1)
                    {
                        //draft varsa bu kayıt üzerinden işlem yapılır. Default Invoice olarak işaretlenir.
                        newInvoice = invoiceCheckList.Entities[0];
                        newInvoice.Attributes["rnt_defaultinvoice"] = true;
                        OrgService.Update(newInvoice);


                        string invoiceItemFecth = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                                      <entity name='rnt_invoiceitem'>
                                                        <attribute name='rnt_invoiceitemid' />
                                                        <attribute name='rnt_name' />
                                                        <attribute name='createdon' />
                                                        <order attribute='rnt_name' descending='false' />
                                                        <filter type='and'>
                                                          <condition attribute='statuscode' operator='eq' value='1' />
                                                          <condition attribute='rnt_contractid' operator='eq' value='{0}' />
                                                        </filter>
                                                      </entity>
                                                    </fetch>";

                        EntityCollection invoiceItemList = OrgService.RetrieveMultiple(new FetchExpression(string.Format(invoiceItemFecth, diffContract.Id)));
                        foreach (var invoiceItem in invoiceItemList.Entities)
                        {
                            invoiceItem.Attributes["rnt_invoiceid"] = new EntityReference(newInvoice.LogicalName, newInvoice.Id);
                            OrgService.Update(invoiceItem);
                        }
                    }
                    else
                    {
                        //draft yoksa yeni draft kayıt oluşturulur.
                        newInvoice = new Entity(invoice.LogicalName);
                        foreach (var invoiceAttributes in invoice.Attributes)
                        {
                            if (invoiceAttributes.Key != "rnt_invoiceid" && invoiceAttributes.Key != "createdon")
                                newInvoice[invoiceAttributes.Key] = invoiceAttributes.Value;
                        }
                        newInvoice.Id = Guid.Empty;
                        newInvoice.Id = OrgService.Create(newInvoice);
                    }



                    //default fatura false olarak update edilir.
                    Entity oldInvoice = new Entity(invoice.LogicalName, invoice.Id);
                    oldInvoice.Attributes["rnt_defaultinvoice"] = false;
                    OrgService.Update(oldInvoice);

                    ExecuteWorkflowRequest updateInvoiceReuqest = new ExecuteWorkflowRequest()
                    {
                        WorkflowId = new Guid("AC45A087-6A39-41C3-B794-0E60537A164C"),
                        EntityId = diffContract.Id
                    };
                    OrgService.Execute(updateInvoiceReuqest);

                    //İşlem yapılan fatura tekrar normale çekilir..
                    Entity updateInvoice = new Entity(newInvoice.LogicalName, newInvoice.Id);
                    updateInvoice["rnt_defaultinvoice"] = false;
                    OrgService.Update(updateInvoice);


                    //Default fatura tekrar işaretlenir.
                    oldInvoice["rnt_defaultinvoice"] = true;
                    OrgService.Update(invoice);

                    ExecuteWorkflowRequest sendToInvoiceReuqest = new ExecuteWorkflowRequest()
                    {
                        WorkflowId = new Guid("1C8E21D1-DEA0-4C59-BF76-8D386EBEA9D6"),
                        EntityId = updateInvoice.Id
                    };
                    OrgService.Execute(sendToInvoiceReuqest);

                    CalculateRollupFieldRequest calculateRollupFieldRequestLast = new CalculateRollupFieldRequest
                    {
                        Target = new EntityReference(diffContract.LogicalName, diffContract.Id),
                        FieldName = "rnt_billedamount" // Rollup Field Name
                    };
                    CalculateRollupFieldResponse responseLast = (CalculateRollupFieldResponse)OrgService.Execute(calculateRollupFieldRequestLast);
                }
            }
        }
        public class crmpayments
        {
            public string id { get; set; }
            public string transactionId { get; set; }

        }
        public class cc
        {
            public string branchId { get; set; }
            public string branchName { get; set; }
        }
        public static void checkInvoiceAddress()
        {
            var s = new CrmServiceHelper().IOrganizationService;
            QueryExpression queryExpression = new QueryExpression("rnt_invoiceaddress");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition("rnt_invoicetypecode", ConditionOperator.Equal, (int)rnt_invoice_rnt_invoicetypecode.Individual);
            queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            var r = s.RetrieveMultiple(queryExpression);
            var res = r.Entities.Where(p => p.GetAttributeValue<string>("rnt_government")?.Length != 11).ToList();
            foreach (var item in res)
            {
                Console.WriteLine(item.Id);
            }
            Console.ReadLine();
        }

        public static void updateReservation()
        {
            var s = new CrmServiceHelper().IOrganizationService;
            var ReservationId = new Guid("6ad6e892-2df8-ea11-a815-000d3a2e9b6c");
            ReservationRepository reservationRepository = new ReservationRepository(s);
            var reservation = reservationRepository.getReservationById(ReservationId, new string[] { "rnt_groupcodeid",
                                                                                                                     "rnt_doublecreditcard",
                                                                                                                     "rnt_minimumage",
                                                                                                                     "rnt_minimumdriverlicience",
                                                                                                                     "rnt_youngdriverage",
                                                                                                                     "rnt_youngdriverlicence",
                                                                                                                     "rnt_paymentchoicecode",
                                                                                                                     "transactioncurrencyid",
                                                                                                                     "rnt_pricinggroupcodeid",
                                                                                                                     "rnt_reservationtypecode",
                                                                                                                     "rnt_paymentmethodcode"});
            #region Update Reservation

            var selectedCustomerObject = JsonConvert.DeserializeObject<ReservationCustomerParameters>("{'invoiceAddress':{'addressDetail':'myaddress','individualCustomerId':'e7f13c12-0e25-ea11-a810-000d3a4abb82','firstName':'polat','lastName':'aydıntest','governmentId':'37645491122','companyName':'','taxNumber':'','taxOfficeId':'00000000-0000-0000-0000-000000000000','invoiceType':10,'addressCountryId':'c2e4665c-9aaf-e811-9410-000c293d97f8','addressCountryName':'Türkiye','addressCityId':'d0eab25a-4ec6-e811-9413-000c293d97f8','addressCityName':'Adana','addressDistrictId':'f1165296-4ec6-e811-9413-000c293d97f8','addressDistrictName':'Ceyhan','invoiceAddressId':'4ac96918-0e25-ea11-a810-000d3a4abb82','name':'','email':null,'mobilePhone':null,'defaultInvoice':false},'contactId':'e7f13c12-0e25-ea11-a810-000d3a4abb82','customerType':3}");
            var selectedDateandBranchObject = JsonConvert.DeserializeObject<ReservationDateandBranchParameters>("{'pickupBranchId':'b38d5d16-17b1-e811-9410-000c293d97f8','dropoffBranchId':'b38d5d16-17b1-e811-9410-000c293d97f8','pickupDate':'2020-09-16T15:00:00.000Z','dropoffDate':'2020-09-21T15:00:00.000Z'}    ");
            var selectedEquipmentObject = JsonConvert.DeserializeObject<ReservationEquipmentParameters>("{'groupCodeInformationId':'c2aaa3d5-7cb3-e811-9410-000c293d97f8','groupCodeInformationName':'A','segment':10,'depositAmount':500,'itemName':'Clio veya benzeri','billingType':20}");
            var selectedPriceObject = JsonConvert.DeserializeObject<ReservationPriceParameters>("{'creditCardData':[{'creditCardId':'23e5cacf-29f8-ea11-a815-000d3a2e9b6c','cardUserKey':'yY5D5rlxPCtdqVHiVVYzewzmA6A=','cardToken':'p+hqiTqaQJ+wZc2nWaY2LVMRgoM=','cardType':10},{'creditCardId':'ace24ccb-42f3-ea11-a815-000d3aafb97e','cardUserKey':'HEAr/DlgRnR3rgLLDEbOkATA2Gg=','cardToken':'vavtxaAZUl/2sSzVjQRyToDwOKM=','cardType':30}],'paymentType':20,'pricingType':50,'reservationPaidAmount':0,'virtualPosId':null,'installment':1,'installmentTotalAmount':35,'price':450}");
            var selectedAdditionalProducts = JsonConvert.DeserializeObject<List<ReservationAdditionalProductParameters>>("[{'productId':'1fe0b589-59e8-e811-a971-000d3a26c57a','productName':'Mini Hasar Güvencesi','productType':10,'showOnContractUpdate':false,'productCode':'SRV002','maxPieces':1,'showonWeb':true,'showonWebsite':true,'showonMobile':false,'webRank':2,'mobileRank':null,'productDescription':'Kaza ve hasar durumunda kiralanan araç grubuna bağlı olarak değişen çarpışmaya bağlı olmadan meydana gelen 600 TLye kadar olan hasarlarda kiracı sorumluluğunu ortadan kaldırır ve teminat kapsamına alır.','actualAmount':8,'isChecked':true,'value':0,'actualTotalAmount':35,'paidAmount':0,'tobePaidAmount':35,'isMandatory':false,'priceCalculationType':1,'monthlyPackagePrice':35,'webIconURL':'https://rentgostorage.blob.core.windows.net/cmsimages/100000003/693c7458-97d9-407e-bce9-648bb1734a71','mobileIconURL':null,'currencySymbol':'$'}]");


            var Currency = "3D7A8CFD-6EF3-E811-A950-000D3A4546AF";
            ReservationRelatedParameters reservationRelatedParameter = new ReservationRelatedParameters
            {
                reservationChannel = 10,
                reservationTypeCode = (int)rnt_ReservationTypeCode.Broker,
                reservationId = ReservationId,
                statusCode = 100000006
            };


            ReservationBL reservationBL = new ReservationBL(s);
            var reservationUpdateResponse = reservationBL.updateReservation(selectedCustomerObject,
                                                                            selectedDateandBranchObject,
                                                                            selectedEquipmentObject,
                                                                            selectedPriceObject,
                                                                            reservationRelatedParameter,
                                                                            selectedAdditionalProducts,
                                                                            new Guid(Currency),
                                                                            5,
                                                                            "f160e905-a9b0-4d3d-9077-1b2d7937a51d",
                                                                            10,
                                                                            true);

            //initializer.TraceMe("updateReservation end");
            #endregion

            #region Update Invoice start
            // initializer.TraceMe("invoice start");

            InvoiceRepository invoiceRepository = new InvoiceRepository(s);
            var invoice = invoiceRepository.getInvoiceByReservationId(ReservationId);

            InvoiceBL invoiceBL = new InvoiceBL(s);
            if (invoice != null)
            {
                invoiceBL.updateInvoice(selectedCustomerObject.invoiceAddress,
                                        invoice.Id,
                                        ReservationId,
                                        null,
                                        Guid.Parse(Currency));
            }
            //initializer.TraceMe("invoice end");
            #endregion

            if (true)
            {
                var paymentStatus = JsonConvert.DeserializeObject<PaymentStatus>("{'isReservationPaid':false,'isDepositPaid':false}");

                #region Get Active reservationitems Guids
                ReservationItemBL reservationItemBL = new ReservationItemBL(s);
                var reservationItems = reservationItemBL.getActiveReservationItemsGuidsByReservationId(ReservationId);

                #endregion

                #region create contract header
                ContractBL contractBL = new ContractBL(s);
                var createContractResponse = contractBL.createContractWithInitializeFromRequest(ReservationId, reservationUpdateResponse.pnrNumber, 10, selectedPriceObject.campaignId.HasValue ? selectedPriceObject.campaignId.Value : Guid.Empty);
                #endregion

                #region create invoice
                var invoiceId = invoiceBL.createInvoice(selectedCustomerObject.invoiceAddress,
                                                   null,
                                                   createContractResponse.contractId,
                                                   new Guid(Currency));
                #endregion

                #region Create Contract items                    

                ContractItemBL contractItemBL = new ContractItemBL(s);
                var createdContractItems = contractItemBL.createContractItemsWithInitializeFromRequest(reservationItems, createContractResponse.contractId, invoiceId, 10, null);

                var res = reservationRepository.getReservationById(reservationRelatedParameter.reservationId, new string[] { "rnt_paymentmethodcode", "createdon" });
                if (res.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker)
                {

                    RntCar.BusinessLibrary.Repository.ReservationItemRepository reservationItemRepository = new RntCar.BusinessLibrary.Repository.ReservationItemRepository(s);
                    var resItems = reservationItemRepository.getCompletedEquipmentandPriceDifferenceReservationItemsWithGivenColumns(res.Id, new string[] { });
                    if (resItems.Count > 0)
                    {
                        var l = new List<Guid>();
                        foreach (var item in resItems)
                        {
                            l.Add(item.Id);
                        }
                        ContractItemBL contractItemBL1 = new ContractItemBL(s);
                        var resContractItems = contractItemBL1.createContractItemsWithInitializeFromRequest(l, createContractResponse.contractId, invoiceId, 10, null);

                        foreach (var cItems in resContractItems)
                        {
                            //items.FirstOrDefault().contractItemId
                            Entity e = new Entity("rnt_contractitem");
                            e["statecode"] = new OptionSetValue((int)GlobalEnums.StateCode.Active);
                            e["statuscode"] = new OptionSetValue((int)rnt_contractitem_StatusCode.Completed);
                            e.Id = cItems.contractItemId;
                            s.Update(e);
                            if (cItems.itemTypeCode == (int)rnt_contractitem_rnt_itemtypecode.Equipment)
                            {
                                createdContractItems.Add(cItems);
                            }
                        }

                    }


                }
                #endregion

                #region Update ReservationPayments With Contract

                contractBL.updateReservationPaymentsWithContract(createContractResponse.contractId, ReservationId);

                #endregion

                #region Update Reservation Credit Card Slip With contract

                contractBL.updateReservationCreditCardSlipsWithContract(createContractResponse.contractId, ReservationId);

                #endregion

                #region complete reservation header

                reservationBL.completeReservationHeaderandItemsForContract(ReservationId);

                #endregion


                #region Update reservation contract header
                reservationBL.updateReservationHeaderForContract(ReservationId, createContractResponse.contractId);
                #endregion

                #region Update reservation is walk-in
                reservationBL.updateReservationIsWalkin(res);
                #endregion

                try
                {
                    ContractHelper contractHelper = new ContractHelper(s);
                    var corpContracts = contractHelper.checkMakePayment_CorporateContracts(reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value,
                                                                                           reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value);


                    if (!corpContracts)
                    {

                        #region collect cards from parameters
                        var paymentCard = selectedPriceObject.creditCardData.Where(x => x.cardType == (int)PaymentEnums.PaymentTransactionType.SALE &&
                                                             (x.creditCardId != null || !string.IsNullOrEmpty(x.creditCardNumber))).FirstOrDefault();
                        var depositCard = selectedPriceObject.creditCardData.Where(x => x.cardType == (int)PaymentEnums.PaymentTransactionType.DEPOSIT &&
                                                                (x.creditCardId != null || !string.IsNullOrEmpty(x.creditCardNumber))).FirstOrDefault();
                        #endregion

                        #region get reservation(contract) amount difference

                        if (reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Broker ||
                            reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Acente)
                        {
                            ConfigurationRepository configurationRepository = new ConfigurationRepository(s);
                            var currency = configurationRepository.GetConfigurationByKey("currency_TRY");
                            if (new Guid(currency) != reservation.GetAttributeValue<EntityReference>("transactioncurrencyid").Id)
                            {

                                contractBL.updateContractItemsandContractTurkishCurrency(createContractResponse.contractId, new Guid(currency));

                                ContractRepository contractRepository = new ContractRepository(s);
                                var c = contractRepository.getContractById(createContractResponse.contractId, new string[] { "rnt_corporatetotalamount" });
                                reservationUpdateResponse.corporateAmount = c.GetAttributeValue<Money>("rnt_corporatetotalamount").Value * -1;
                            }

                        }

                        if (reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value != (int)rnt_ReservationTypeCode.Broker)
                        {
                            reservationUpdateResponse.corporateAmount = 0;
                        }

                        var diff = contractBL.getContractDiffAmount(createContractResponse.contractId);
                        var depositAmount = contractBL.getContractDepositAmount(createContractResponse.contractId);


                        #endregion

                        #region check double credit card

                        var creditCardResponse = contractHelper.checkDoubleCreditCard(diff.Value,
                                                                                      depositAmount.Value,
                                                                                      paymentCard,
                                                                                      depositCard,
                                                                                      true,
                                                                                      selectedCustomerObject.contactId,
                                                                                      1055,
                                                                                      10);


                        if (!string.IsNullOrEmpty(creditCardResponse))
                        {

                        }
                        if (depositAmount > 0)
                        {
                            var resCard = contractHelper.checkPaymentCardandDepositCardIsSame(true,
                                                                                              reservationRelatedParameter.reservationId,
                                                                                              paymentCard,
                                                                                              depositCard,
                                                                                              1055);
                            if (!string.IsNullOrEmpty(resCard))
                            {

                            }
                        }


                        #endregion

                        #region make payment                  

                        // For upsell or extend date
                        diff = diff - reservationUpdateResponse.corporateAmount;
                        if (diff < 1)
                        {
                            diff = decimal.Zero;
                        }


                        var contractPaymentResponse = contractBL.makeContractPayment(diff,
                                                                                     depositAmount,
                                                                                     paymentCard,
                                                                                     depositCard,
                                                                                     selectedCustomerObject.invoiceAddress,
                                                                                     Guid.Parse(Currency),
                                                                                     selectedCustomerObject.contactId,
                                                                                     createContractResponse.contractId,
                                                                                     ReservationId,
                                                                                     reservationUpdateResponse.pnrNumber,
                                                                                     paymentStatus,
                                                                                     1055,
                                                                                     selectedPriceObject.virtualPosId,
                                                                                     selectedPriceObject.installment,
                                                                                     PaymentMapper.mapDocumentChanneltoPaymentChannel(reservationRelatedParameter.reservationChannel),
                                                                                     selectedPriceObject.use3DSecure,
                                                                                     selectedPriceObject.callBackUrl);
                        if (!contractPaymentResponse.ResponseResult.Result)
                        {

                        }
                        #endregion
                    }

                    else
                    {
                        //fullcredit senaryosu için
                        if (reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Broker ||
                            reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Acente)
                        {


                            ConfigurationRepository configurationRepository = new ConfigurationRepository(s);
                            var currency = configurationRepository.GetConfigurationByKey("currency_TRY");
                            if (new Guid(currency) != reservation.GetAttributeValue<EntityReference>("transactioncurrencyid").Id)
                            {

                                contractBL.updateContractItemsandContractTurkishCurrency(createContractResponse.contractId, new Guid(currency));

                                ContractRepository contractRepository = new ContractRepository(s);
                                var c = contractRepository.getContractById(createContractResponse.contractId, new string[] { "rnt_corporatetotalamount" });
                                reservationUpdateResponse.corporateAmount = c.GetAttributeValue<Money>("rnt_corporatetotalamount").Value * -1;
                            }

                        }
                    }

                    #region sending equipment type item to mongodb


                    try
                    {
                        var equipmentItems = createdContractItems.Where(p => p.itemTypeCode == (int)rnt_contractitem_rnt_itemtypecode.Equipment).ToList();
                        foreach (var item in equipmentItems)
                        {

                        }

                    }
                    catch (Exception ex)
                    {

                    }

                    #endregion

                }
                catch (Exception ex)
                {

                }
            }
        }

        public class LogData
        {
            public ObjectId _id { get; set; }
            public string date { get; set; }
            public string type { get; set; }
            public string controller { get; set; }
            public string method { get; set; }
            public List<string> message { get; set; }
        }


        public static void updateEmptyPaymentRecords()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            QueryExpression queryExpression = new QueryExpression("rnt_payment");
            queryExpression.Criteria.AddCondition("rnt_customercreditcardid", ConditionOperator.Null);
            queryExpression.Criteria.AddCondition("rnt_transfertypecode", ConditionOperator.Null);

            queryExpression.ColumnSet = new ColumnSet(true);
            var results = crmServiceHelper.IOrganizationService.RetrieveMultiple(queryExpression);

            foreach (var item in results.Entities)
            {
                if (item.Contains("rnt_contractid"))
                {
                    PaymentRepository paymentRepository = new PaymentRepository(crmServiceHelper.IOrganizationService);
                    var lastPayment = paymentRepository.getLastPayment_Contract_CreditCardIsNotEmpty(item.GetAttributeValue<EntityReference>("rnt_contractid").Id);
                    if (lastPayment != null)
                    {
                        Entity e = new Entity("rnt_payment");
                        e["rnt_customercreditcardid"] = lastPayment["rnt_customercreditcardid"];
                        e.Id = item.Id;
                        crmServiceHelper.IOrganizationService.Update(e);
                    }
                    else
                    {
                        lastPayment = paymentRepository.getDeposit_Contract(item.GetAttributeValue<EntityReference>("rnt_contractid").Id);
                        if (lastPayment != null && lastPayment.Contains("rnt_customercreditcardid"))
                        {
                            Entity e = new Entity("rnt_payment");
                            e["rnt_customercreditcardid"] = lastPayment["rnt_customercreditcardid"];
                            e.Id = item.Id;
                            crmServiceHelper.IOrganizationService.Update(e);
                        }
                    }

                }
                else if (item.Contains("rnt_reservationid"))
                {
                    PaymentRepository paymentRepository = new PaymentRepository(crmServiceHelper.IOrganizationService);
                    var lastPayment = paymentRepository.getLastPayment_Reservation_CreditCardIsNotEmpty(item.GetAttributeValue<EntityReference>("rnt_reservationid").Id);

                    if (lastPayment != null)
                    {
                        Entity e = new Entity("rnt_payment");
                        e["rnt_customercreditcardid"] = lastPayment["rnt_customercreditcardid"];
                        e.Id = item.Id;
                        crmServiceHelper.IOrganizationService.Update(e);
                    }
                }
            }

            results = crmServiceHelper.IOrganizationService.RetrieveMultiple(queryExpression);

            foreach (var item in results.Entities)
            {
            }
        }
        public static void createCrmConfigurationsInMongoDB()
        {
            QueryExpression queryExpression = new QueryExpression("rnt_configuration");
            queryExpression.ColumnSet = new ColumnSet(true);
            var configurations = new CrmServiceHelper().IOrganizationService.RetrieveMultiple(queryExpression).Entities.ToList();

            CrmConfigurationBusiness crmConfigurationBusiness = new CrmConfigurationBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            var crmConfigurationDatas = new List<CrmConfigurationData>();

            configurations.ForEach(c =>
            {
                crmConfigurationDatas.Add(new CrmConfigurationData
                {
                    crmConfigurationId = c.Id.ToString(),
                    name = c.GetAttributeValue<string>("rnt_name"),
                    value = c.GetAttributeValue<string>("rnt_value"),
                    statecode = c.GetAttributeValue<OptionSetValue>("statecode").Value
                });
            });

            crmConfigurationDatas.ForEach(c =>
            {
                crmConfigurationBusiness.createConfiguration(c, c.crmConfigurationId);
            });
        }

        public static void sendHgsEmail()
        {
            string contactName = "Bravo Six";
            DateTime pickupDatetime = DateTime.Now;
            List<HGSTransitData> hGSTransitDatas = new List<HGSTransitData>() { new HGSTransitData { amount = 10,
                                                                                              entryDateTime = DateTime.Now.Ticks,
                                                                                              entryLocation = "Hong Kong",
                                                                                              exitDateTime = DateTime.Now.Ticks,
                                                                                              exitLocation = "Taiwan",
                                                                                              _entryDateTime = DateTime.Now,
                                                                                              _exitDateTime = DateTime.Now,
                                                                                              plateNumber = "34 AAA 01"} };

            var config = new TemplateServiceConfiguration();
            config.DisableTempFileLocking = true;
            var service = RazorEngineService.Create(config);
            string template = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, @"Resources\HGSEmail.html"));
            var result = service.RunCompile(template, "templateKey", null, new { ContactName = contactName, PickupDateTime = pickupDatetime, HGSTransits = hGSTransitDatas });

            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            Entity fromActivityParty = new Entity("activityparty");
            Entity toActivityParty = new Entity("activityparty");

            var contactId = new Guid("28c4e82d-d8d7-ea11-a813-000d3a2e9b6c");
            fromActivityParty["partyid"] = new EntityReference("systemuser", new Guid("c0eeae60-9945-42bb-a6d0-a2a85a4e7947"));
            toActivityParty["partyid"] = new EntityReference("contact", contactId);

            Entity email = new Entity("email");
            email["from"] = new Entity[] { fromActivityParty };
            email["to"] = new Entity[] { toActivityParty };
            email["regardingobjectid"] = new EntityReference("contact", contactId);
            email["subject"] = "This is the subject";
            email["description"] = result;
            email["directioncode"] = true;
            Guid emailId = crmServiceHelper.IOrganizationService.Create(email);

            SendEmailRequest sendEmailRequest = new SendEmailRequest
            {
                EmailId = emailId,
                TrackingToken = "",
                IssueSend = true
            };

            SendEmailResponse sendEmailresp = (SendEmailResponse)crmServiceHelper.IOrganizationService.Execute(sendEmailRequest);
        }
        public static void syncFactors()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            AvailabilityFactorsRepository availabilityFactorsRepository = new AvailabilityFactorsRepository(crmServiceHelper.IOrganizationService);
            AvailabilityFactorsBL availabilityFactorsBL = new AvailabilityFactorsBL(crmServiceHelper.IOrganizationService);
            var crmFactors = availabilityFactorsRepository.getActiveAvailabilityFactors();

            foreach (var item in crmFactors)
            {
                var updateResponse = availabilityFactorsBL.updateAvailabilityFactorInMongoDB(item);
                if (!updateResponse.Result)
                {
                    continue;
                }
                availabilityFactorsBL.UpdateMongoDBUpdateRelatedFields(item);
            }
        }
        public static void updateFactors()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            AvailabilityFactorsRepository availabilityFactorsRepository = new AvailabilityFactorsRepository(crmServiceHelper.IOrganizationService);
            var crmFactors = availabilityFactorsRepository.getActiveAvailabilityFactors();

            RntCar.MongoDBHelper.Repository.AvailabilityFactorRepository availabilityFactorRepository =
                new RntCar.MongoDBHelper.Repository.AvailabilityFactorRepository(mongoDBHostName, mongoDBDatabaseName);

            var mongoFactors = availabilityFactorRepository.getActiveAvailabilityFactors();

            List<AvailabilityFactorDataMongoDB> exceptFactors = new List<AvailabilityFactorDataMongoDB>();
            foreach (var item in mongoFactors)
            {
                var a = crmFactors.Where(p => p.Id == new Guid(item.availabilityFactorId)).FirstOrDefault();
                if (a == null)
                {
                    var c = availabilityFactorRepository.getCollection<AvailabilityFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityFactorCollectionName"));
                    item.statecode = 0;
                    item.statuscode = 1;
                    c.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
                }
            }

        }
        public static void updateDurations()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            List<Entity> reservations = new List<Entity>();
            List<Entity> reservationItems = new List<Entity>();
            List<Entity> contracts = new List<Entity>();
            List<Entity> contractItems = new List<Entity>();

            List<Task> tasks = new List<Task>();
            var reservationTask = new Task(() =>
            {
                int queryCount = 5000;
                int pageNumber = 1;

                string pagingCookie = null;

                while (true)
                {
                    QueryExpression query = new QueryExpression("rnt_reservation");
                    query.ColumnSet = new ColumnSet(new string[] { "rnt_pickupdatetime", "rnt_dropoffdatetime", "rnt_duration" });
                    query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
                    query.PageInfo = new PagingInfo
                    {
                        PageNumber = pageNumber,
                        Count = queryCount,
                        PagingCookie = pagingCookie
                    };
                    query.ColumnSet = new ColumnSet(true);
                    var results = crmServiceHelper.IOrganizationService.RetrieveMultiple(query);
                    reservations.AddRange(results.Entities.ToList());
                    if (results.MoreRecords)
                    {
                        pageNumber++;
                        pagingCookie = results.PagingCookie;
                    }
                    else
                    {
                        break;
                    }
                }
            });
            tasks.Add(reservationTask);
            reservationTask.Start();

            var reservationItemTask = new Task(() =>
            {
                int queryCount = 5000;
                int pageNumber = 1;

                string pagingCookie = null;

                while (true)
                {
                    QueryExpression query = new QueryExpression("rnt_reservationitem");
                    query.ColumnSet = new ColumnSet(new string[] { "rnt_pickupdatetime", "rnt_dropoffdatetime", "rnt_reservationduration" });
                    query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
                    query.PageInfo = new PagingInfo
                    {
                        PageNumber = pageNumber,
                        Count = queryCount,
                        PagingCookie = pagingCookie
                    };
                    query.ColumnSet = new ColumnSet(true);
                    var results = crmServiceHelper.IOrganizationService.RetrieveMultiple(query);
                    reservationItems.AddRange(results.Entities.ToList());
                    if (results.MoreRecords)
                    {
                        pageNumber++;
                        pagingCookie = results.PagingCookie;
                    }
                    else
                    {
                        break;
                    }
                }
            });
            tasks.Add(reservationItemTask);
            reservationItemTask.Start();

            var contractTask = new Task(() =>
            {
                int queryCount = 5000;
                int pageNumber = 1;

                string pagingCookie = null;

                while (true)
                {
                    QueryExpression query = new QueryExpression("rnt_contract");
                    query.ColumnSet = new ColumnSet(new string[] { "rnt_pickupdatetime", "rnt_dropoffdatetime", "rnt_contractduration" });
                    query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
                    query.PageInfo = new PagingInfo
                    {
                        PageNumber = pageNumber,
                        Count = queryCount,
                        PagingCookie = pagingCookie
                    };
                    query.ColumnSet = new ColumnSet(true);
                    var results = crmServiceHelper.IOrganizationService.RetrieveMultiple(query);
                    contracts.AddRange(results.Entities.ToList());
                    if (results.MoreRecords)
                    {
                        pageNumber++;
                        pagingCookie = results.PagingCookie;
                    }
                    else
                    {
                        break;
                    }
                }
            });
            tasks.Add(contractTask);
            contractTask.Start();

            var contractItemTask = new Task(() =>
            {
                int queryCount = 5000;
                int pageNumber = 1;

                string pagingCookie = null;

                while (true)
                {
                    QueryExpression query = new QueryExpression("rnt_contractitem");
                    query.ColumnSet = new ColumnSet(new string[] { "rnt_pickupdatetime", "rnt_dropoffdatetime", "rnt_contractduration" });
                    query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
                    query.PageInfo = new PagingInfo
                    {
                        PageNumber = pageNumber,
                        Count = queryCount,
                        PagingCookie = pagingCookie
                    };
                    query.ColumnSet = new ColumnSet(true);
                    var results = crmServiceHelper.IOrganizationService.RetrieveMultiple(query);
                    contractItems.AddRange(results.Entities.ToList());
                    if (results.MoreRecords)
                    {
                        pageNumber++;
                        pagingCookie = results.PagingCookie;
                    }
                    else
                    {
                        break;
                    }
                }
            });
            tasks.Add(contractItemTask);
            contractItemTask.Start();

            Task.WaitAll(tasks.ToArray());

            try
            {
                var contractHelper = new RntCar.BusinessLibrary.Helpers.ContractHelper(crmServiceHelper.IOrganizationService);
                reservations.ForEach(res =>
                {
                    var pickupDateTime = res.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                    var dropoffDateTime = res.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                    var currentDuration = res.GetAttributeValue<decimal>("rnt_duration");
                    var newDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect_Decimal(pickupDateTime, dropoffDateTime);

                    if (currentDuration != newDuration)
                    {
                        Entity e = new Entity("rnt_reservation");
                        e.Id = res.Id;
                        e["rnt_duration"] = newDuration;

                        crmServiceHelper.IOrganizationService.Update(e);
                    }
                });

                Console.WriteLine("res itemstarted");

                reservationItems.ForEach(resItem =>
                {
                    var pickupDateTime = resItem.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                    var dropoffDateTime = resItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                    var currentDuration = resItem.GetAttributeValue<int>("rnt_reservationduration");
                    var newDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(pickupDateTime, dropoffDateTime);

                    if (currentDuration != newDuration)
                    {
                        Entity e = new Entity("rnt_reservationitem");
                        e.Id = resItem.Id;
                        e["rnt_reservationduration"] = newDuration;

                        crmServiceHelper.IOrganizationService.Update(e);
                    }
                });

                Console.WriteLine("res item end");
                Console.WriteLine("contract start");

                contracts.ForEach(ctr =>
                {
                    var pickupDateTime = ctr.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                    var dropoffDateTime = ctr.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                    var currentDuration = ctr.GetAttributeValue<decimal>("rnt_contractduration");
                    var newDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect_Decimal(pickupDateTime, dropoffDateTime);

                    if (currentDuration != newDuration)
                    {
                        Entity e = new Entity("rnt_contract");
                        e.Id = ctr.Id;
                        e["rnt_contractduration"] = newDuration;

                        crmServiceHelper.IOrganizationService.Update(e);
                    }
                });
                Console.WriteLine("contract end");
                Console.WriteLine("contract item start");

                contractItems.ForEach(ctrItem =>
                {
                    var pickupDateTime = ctrItem.GetAttributeValue<DateTime>("rnt_pickupdatetime");
                    var dropoffDateTime = ctrItem.GetAttributeValue<DateTime>("rnt_dropoffdatetime");
                    var currentDuration = ctrItem.GetAttributeValue<int>("rnt_contractduration");
                    var newDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(pickupDateTime, dropoffDateTime);

                    if (currentDuration != newDuration)
                    {
                        Entity e = new Entity("rnt_contractitem");
                        e.Id = ctrItem.Id;
                        e["rnt_contractduration"] = newDuration;

                        crmServiceHelper.IOrganizationService.Update(e);
                    }
                });
                Console.WriteLine("contract item end");

            }
            catch (Exception ex)
            {

                throw;
            }

            var a = 0;
        }
        public static void getEquipmentAvailabilityReport(EquipmentAvailabilityRequest reportRequest)
        {

            RntCar.MongoDBHelper.Repository.EquipmentAvailabilityRepository equipmentAvailabilityRepository =
                new RntCar.MongoDBHelper.Repository.EquipmentAvailabilityRepository(mongoDBHostName, mongoDBDatabaseName);
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            EquipmentAvailabilityBL equipmentAvailabilityBL = new EquipmentAvailabilityBL(crmServiceHelper.IOrganizationService);
            ReportBL reportBL = new ReportBL(crmServiceHelper.IOrganizationService);

            try
            {
                List<EquipmentAvailabilityData> equipmentAvailabilityDatas = new List<EquipmentAvailabilityData>();

                // If query date includes today, feth realtime data from CRM
                if (DateTime.Now.Date >= new DateTime(reportRequest.StartDate) && new DateTime(reportRequest.EndDate) >= DateTime.Now.Date)
                {
                    var todaysData = equipmentAvailabilityBL.calculateEquipmentAvailability(new string[] { });

                    if (new DateTime(reportRequest.StartDate) == DateTime.Now.Date)
                    {
                        equipmentAvailabilityDatas = todaysData;
                    }
                    else
                    {
                        // Önceki günlere ait datayı Mongo'dan çek
                        var previousDaysData = equipmentAvailabilityRepository.getEquipmentAvailabilityByDate(reportRequest.StartDate, new DateTime(reportRequest.EndDate).Date.AddDays(-1).Ticks);
                        equipmentAvailabilityDatas = todaysData.Concat(previousDaysData).ToList();
                    }
                }
                else
                {
                    var equipmentAvailabilities = equipmentAvailabilityRepository.getEquipmentAvailabilityByDate(reportRequest.StartDate, reportRequest.EndDate);

                    equipmentAvailabilities.ForEach(item =>
                    {
                        equipmentAvailabilityDatas.Add(new EquipmentAvailabilityData
                        {
                            Availability = item.Availability,
                            AvailableCount = item.AvailableCount,
                            CurrentBranch = item.CurrentBranch,
                            CurrentBranchId = item.CurrentBranchId,
                            GroupCode = item.GroupCode,
                            GroupCodeInformationId = item.GroupCodeInformationId,
                            InTransferCount = item.InTransferCount,
                            LongTermTransferCount = item.LongTermTransferCount,
                            LostStolenCount = item.LostStolenCount,
                            MissingInventoriesCount = item.MissingInventoriesCount,
                            PertCount = item.PertCount,
                            PublishDate = item.PublishDate,
                            RentalCount = item.RentalCount,
                            SecondHandTransferCount = item.SecondHandTransferCount,
                            SecondHandTransferWaitingConfirmationCount = item.SecondHandTransferWaitingConfirmationCount,
                            FurtherReservationCount = item.FurtherReservationCount,
                            OutgoingContractCount = item.OutgoingContractCount,
                            RentalContractCount = item.RentalContractCount,
                            //ReservationCount = item.ReservationCount,
                            Total = item.Total
                        });
                    });
                }

                var revenueItems = reportBL.GetRevenueItems(new DateTime(reportRequest.StartDate), new DateTime(reportRequest.EndDate));
                var days = (new DateTime(reportRequest.EndDate) - new DateTime(reportRequest.StartDate)).Days;
                var result = reportBL.buildEquipmentAvailabilities(new EquipmentAvailabilityResponse { EquipmentAvailabilityDatas = equipmentAvailabilityDatas }, revenueItems, days + 1);

                //return result;

                //return new EquipmentAvailabilityResponse
                //{
                //    EquipmentAvailabilityDatas = equipmentAvailabilityDatas,
                //    ResponseResult = ResponseResult.ReturnSuccess()
                //};
            }
            catch (Exception ex)
            {
                //return new EquipmentAvailabilityResponse
                //{
                //    ResponseResult = ResponseResult.ReturnError(ex.Message)
                //};
            }
        }

        public static void createContact()
        {
            Entity e = new Entity("contact");
            e["firstname"] = "SATIS";
            e["lastname"] = "RENTGO";
            e["emailaddress1"] = "satis@rentgo.com";
            new CrmServiceHelper().IOrganizationService.Create(e);
        }
        public static void createVirtualBranchInMongoDB()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            VirtualBranchBL virtualBranchBL = new VirtualBranchBL(crmServiceHelper.IOrganizationService);

            try
            {
                QueryExpression queryExpression = new QueryExpression("rnt_virtualbranch");
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

                var corporateCustomers = crmServiceHelper.CrmServiceClient.RetrieveMultiple(queryExpression).Entities.ToList();

                corporateCustomers.ForEach(item =>
                {
                    try
                    {
                        virtualBranchBL.createVirtualBranchInMongoDB(item);

                    }
                    catch (Exception ex)
                    {

                    }

                });
            }
            catch (Exception ex)
            {

            }
        }
        public static void createCorporateCustomerInMongoDB()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            CorporateCustomerBL corporateCustomerBL = new CorporateCustomerBL(crmServiceHelper.IOrganizationService);

            try
            {
                QueryExpression queryExpression = new QueryExpression("account");
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

                var corporateCustomers = crmServiceHelper.CrmServiceClient.RetrieveMultiple(queryExpression).Entities.ToList();

                corporateCustomers.ForEach(item =>
                {
                    try
                    {
                        corporateCustomerBL.createCorporateCustomerInMongoDB(item);

                    }
                    catch (Exception)
                    {

                    }

                });
            }
            catch (Exception ex)
            {

            }
        }
        public static void createKilometerLimitInMongoDB()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(crmServiceHelper.IOrganizationService);

            try
            {
                QueryExpression queryExpression = new QueryExpression("rnt_kilometerlimit");
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);

                var kilometerLimits = crmServiceHelper.CrmServiceClient.RetrieveMultiple(queryExpression).Entities.ToList();

                kilometerLimits.ForEach(item =>
                {
                    kilometerLimitBL.createKilometerLimitInMongoDB(item);
                });
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public static void processAdapter()
        {
            Car car = new Car() { Model = "Astra", Brand = "Opel", Price = 35.1m, Description = "New car added." };
            car.PrintDetail();

            //nesnemize airbag özelliği ekleniyor
            AirbagDecarotor carWithairbag = new AirbagDecarotor(car);
            carWithairbag.PrintDetail();

            //nesnemize abs özelliği ekleniyor
            ABSDecorator carWithABS = new ABSDecorator(car);
            carWithABS.PrintDetail();

            Console.ReadLine();
        }
        public static void CreateAdditionalProductPriceByCurrency()
        {
            var service = new CrmServiceHelper().CrmServiceClient;
            RntCar.BusinessLibrary.Repository.CurrencyRepository currencyRepository = new RntCar.BusinessLibrary.Repository.CurrencyRepository(new CrmServiceHelper().IOrganizationService);
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(new CrmServiceHelper().IOrganizationService);
            AdditionalProductPriceRepository additionalProductPriceRepository = new AdditionalProductPriceRepository(new CrmServiceHelper().IOrganizationService);

            var additionalProducts = additionalProductRepository.getActiveAdditionalProducts();
            var currencies = currencyRepository.getAllCurrencies();
            var additionalProductPrices = additionalProductPriceRepository.getActiveAdditionalProductPrices();

            additionalProducts.ForEach(item =>
            {
                if (additionalProductPrices.Where(p => p.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id == item.Id).ToList().Count > 0 || !item.Attributes.Contains("rnt_monthlypackageprice") || !item.Attributes.Contains("rnt_price"))
                    return;

                currencies.ForEach(currency =>
                {
                    var newPrice = item.GetAttributeValue<Money>("rnt_price").Value * currency.GetAttributeValue<decimal>("exchangerate");
                    var newMonthlyPrice = item.GetAttributeValue<Money>("rnt_monthlypackageprice").Value * currency.GetAttributeValue<decimal>("exchangerate");

                    Entity e = new Entity("rnt_additionalproductprice");
                    e["rnt_additionalproductid"] = new EntityReference("rnt_additionalproduct", item.Id);
                    e["rnt_monthlytotalprice"] = new Money(newMonthlyPrice);
                    e["rnt_name"] = item.GetAttributeValue<string>("rnt_name");
                    e["rnt_price"] = new Money(newPrice);
                    e["transactioncurrencyid"] = new EntityReference("transactioncurrency", currency.Id);

                    service.Create(e);
                });
            });
        }
        public static void CreateInvoiceItemsfromContractItem()
        {
            var service = new CrmServiceHelper().IOrganizationService;
            var contractId = new Guid("fc14695f-2266-ea11-a811-000d3a2d0476");
            RntCar.BusinessLibrary.ContractItemRepository contractItemRepository = new RntCar.BusinessLibrary.ContractItemRepository(service);
            var items = contractItemRepository.getCompletedContractItemsByContractId(contractId);
            items = items.OrderBy(p => p.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value).ToList();
            ContractRepository contractRepository = new ContractRepository(service);
            var contract = contractRepository.getContractById(contractId, new string[] { "rnt_paymentmethodcode", "rnt_corporateid", "transactioncurrencyid" });
            var paymentMethodCode = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;

            XrmHelper xrmHelper = new XrmHelper(service);
            foreach (var item in items)
            {
                InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(service);

                var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemId(item.Id);


                if (invoiceItem == null)
                {
                    Entity createInvoiceItem = xrmHelper.initializeFromRequest("rnt_contractitem", item.Id, "rnt_invoiceitem");

                    Entity invoice = null;

                    if (paymentMethodCode != (int)rnt_PaymentMethodCode.CreditCard &&
                        paymentMethodCode != (int)rnt_PaymentMethodCode.PayBroker)
                    {
                        InvoiceRepository invoiceRepository = new InvoiceRepository(service);
                        invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contractId).FirstOrDefault();
                        if (invoice != null)
                        {
                            createInvoiceItem["rnt_invoiceid"] = new EntityReference(invoice.LogicalName, invoice.Id);
                            createInvoiceItem["rnt_name"] = item.GetAttributeValue<string>("rnt_name");
                        }
                    }
                    else
                    {
                        if (item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.Equipment)
                        {
                            if (paymentMethodCode == (int)rnt_PaymentMethodCode.CreditCard)
                            {
                                InvoiceRepository invoiceRepository = new InvoiceRepository(service);
                                invoice = invoiceRepository.getCorporateNotIntegratedInvoiceByContractId(contract.Id, false);
                                if (invoice == null)
                                {
                                    RntCar.BusinessLibrary.Repository.CorporateCustomerRepository corporateCustomerRepository = new RntCar.BusinessLibrary.Repository.CorporateCustomerRepository(service);
                                    var account = corporateCustomerRepository.getCorporateCustomerById(contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id);
                                    var invoiceData = InvoiceHelper.buildInvoiceAddressDataFromAccountEntity(account);
                                    InvoiceBL invoiceBL = new InvoiceBL(service);
                                    var _id = invoiceBL.createInvoice(invoiceData,
                                    null,
                                    contract.Id,
                                    contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);

                                    Entity e = new Entity("rnt_invoice");
                                    e["rnt_defaultinvoice"] = false;
                                    e.Id = _id;
                                    service.Update(e);
                                    invoice = new InvoiceRepository(service).getInvoiceById(_id);
                                    //test commenti

                                }
                            }
                            else if (paymentMethodCode == (int)rnt_PaymentMethodCode.PayBroker)
                            {
                                InvoiceRepository invoiceRepository = new InvoiceRepository(service);
                                invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contractId).FirstOrDefault();
                            }
                        }
                        else if (item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value == (int)rnt_contractitem_rnt_itemtypecode.PriceDifference &&
                                paymentMethodCode == (int)rnt_PaymentMethodCode.PayBroker)
                        {
                            InvoiceRepository invoiceRepository = new InvoiceRepository(service);
                            invoice = invoiceRepository.getCorporateNotIntegratedInvoiceByContractId(contract.Id, false);
                            if (invoice == null)
                            {
                                RntCar.BusinessLibrary.Repository.CorporateCustomerRepository corporateCustomerRepository = new RntCar.BusinessLibrary.Repository.CorporateCustomerRepository(service);
                                var account = corporateCustomerRepository.getCorporateCustomerById(contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id);
                                var invoiceData = InvoiceHelper.buildInvoiceAddressDataFromAccountEntity(account);
                                InvoiceBL invoiceBL = new InvoiceBL(service);
                                var _id = invoiceBL.createInvoice(invoiceData,
                                null,
                                contract.Id,
                                contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);

                                Entity e = new Entity("rnt_invoice");
                                e["rnt_defaultinvoice"] = false;
                                e.Id = _id;
                                service.Update(e);
                                invoice = new InvoiceRepository(service).getInvoiceById(_id);

                            }
                        }
                        else
                        {
                            InvoiceRepository invoiceRepository = new InvoiceRepository(service);
                            invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contractId).FirstOrDefault();
                        }
                    }
                    if (invoice != null)
                    {
                        createInvoiceItem["rnt_invoiceid"] = new EntityReference(invoice.LogicalName, invoice.Id);
                        createInvoiceItem["rnt_name"] = item.GetAttributeValue<string>("rnt_name");
                        service.Create(createInvoiceItem);
                    }
                }

            }

        }
        private static Entity corpInvoiceOperations(IOrganizationService service, Entity contract, bool isDefault)
        {
            InvoiceRepository invoiceRepository = new InvoiceRepository(service);
            Entity invoice = invoiceRepository.getCorporateNotIntegratedInvoiceByContractId(contract.Id, isDefault);

            if (invoice == null)
            {
                RntCar.BusinessLibrary.Repository.CorporateCustomerRepository corporateCustomerRepository = new RntCar.BusinessLibrary.Repository.CorporateCustomerRepository(service);
                var account = corporateCustomerRepository.getCorporateCustomerById(contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id);
                var invoiceData = InvoiceHelper.buildInvoiceAddressDataFromAccountEntity(account);
                InvoiceBL invoiceBL = new InvoiceBL(service);
                var _id = invoiceBL.createInvoice(invoiceData,
                null,
                contract.Id,
                contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);

                Entity e = new Entity("rnt_invoice");
                e["rnt_defaultinvoice"] = false;
                e.Id = _id;
                service.Update(e);
                invoice = new InvoiceRepository(service).getInvoiceById(_id);
            }
            return invoice;


        }
        public static void sendCreditCardSlipReservation()
        {
            var invoiceAddressId = new Guid("9ebd622b-3a7f-eb11-a812-000d3a4b4f1f");
            var reservationID = new Guid("22c6e7d5-b07e-eb11-a812-000d3a4a7dd6");
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            InvoiceHelper invoiceHelper = new InvoiceHelper(crmServiceHelper.IOrganizationService);
            var invoiceID = invoiceHelper.createInvoiceFromInvoiceAddress_Reservation(reservationID, invoiceAddressId);

            CreditCardSlipRepository creditCardSlipRepository = new CreditCardSlipRepository(crmServiceHelper.IOrganizationService);
            var slips = creditCardSlipRepository.getFailedIntegrationCreditCardSlips_Reservation(reservationID);
            foreach (var item in slips)
            {
                CreditCardSlipBL creditCardSlipBL = new CreditCardSlipBL(crmServiceHelper.IOrganizationService);

                var resRef =
                new EntityReference
                {
                    LogicalName = "rnt_reservation",
                    Id = reservationID
                };
                var paymentRef = new EntityReference
                {
                    Id = item.GetAttributeValue<EntityReference>("rnt_paymentid").Id,
                    LogicalName = "rnt_payment"
                };
                var response = creditCardSlipBL.handleCreateCreditCardSlipWithLogo(null, resRef, paymentRef, item.Id);
            }
        }

        public static void sendCreditCardSlip()
        {
            var paymentId = "ecfe9807-00df-4528-95b1-2cd0f4c016bd";
            var contractId = "398c1fe1-3e1f-eb11-a813-000d3a2c0643";
            var slipId = "4eca0454-9598-eb11-b1ac-000d3a2dd951";

            var s = new CrmServiceHelper().IOrganizationService;
            CreditCardSlipRepository creditCardSlipRepository = new CreditCardSlipRepository(s);
            //var creditCardSlip = creditCardSlipRepository.getCreditCardSlipByPaymentId(new Guid(paymentId));

            CreditCardSlipBL creditCardSlipBL = new CreditCardSlipBL(s);
            //var creditCardSlipId = Guid.Empty;

            ////creditCardSlipId = creditCardSlipBL.createCreditCarSlip(new Guid(reservationId), null, new Guid(paymentId));
            //creditCardSlipId = creditCardSlipBL.createCreditCarSlip(null, new Guid(contractId), new Guid(paymentId));



            var contractRef =
                new EntityReference
                {
                    LogicalName = "rnt_contract",
                    Id = new Guid(contractId)
                };

            var paymentRef = new EntityReference
            {
                Id = new Guid(paymentId),
                LogicalName = "rnt_payment"
            };
            //var slipId = new Guid("df4dc1de-4cd6-ea11-a813-000d3a2d01f6");

            var response = creditCardSlipBL.handleCreateCreditCardSlipWithLogo(contractRef, null, paymentRef, new Guid(slipId));
        }

        public static void sendInvoicetoLogo()
        {

            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            var fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='rnt_invoice'>
                                <attribute name='rnt_invoiceid' />
                                <attribute name='rnt_invoicenumber' />
                                <attribute name='rnt_logoinvoicenumber' />
                                <attribute name='statuscode' />
                                <attribute name='rnt_contractid' />
                                <attribute name='rnt_totalamount' />
                                <attribute name='rnt_invoicetypecode' />
                                <attribute name='rnt_reservationid' />
                                <attribute name='rnt_companyname' />
                                <order attribute='rnt_invoicenumber' descending='true' />
                                <filter type='and'>
                                  <condition attribute='rnt_invoicenumber' operator='eq' value='INV00709859' />
                                </filter>
                              </entity>
                            </fetch>";

            var res = crmServiceHelper.IOrganizationService.RetrieveMultiple(new FetchExpression(fetch));
            var i = 1;
            foreach (var item in res.Entities)
            {
                var contractRef = new EntityReference
                {
                    Id = item.GetAttributeValue<EntityReference>("rnt_contractid").Id,
                    LogicalName = "rnt_contract"
                };



                InvoiceBL invoiceBL1 = new InvoiceBL(crmServiceHelper.IOrganizationService);
                var contractResponse = invoiceBL1.handleCreateInvoiceWithLogoProcessForContract(contractRef.Id, 1055, item.Id);
                Console.WriteLine(i);
                i++;
            }
            Console.ReadLine();

        }

        private static Entity corporateOperations(IOrganizationService Service, Entity contract)
        {

            InvoiceRepository invoiceRepository = new InvoiceRepository(Service);
            var invoice = invoiceRepository.getCorporateNotIntegratedInvoiceByContractId(contract.Id, false);
            if (invoice == null)
            {
                RntCar.BusinessLibrary.Repository.CorporateCustomerRepository corporateCustomerRepository = new RntCar.BusinessLibrary.Repository.CorporateCustomerRepository(Service);
                var account = corporateCustomerRepository.getCorporateCustomerById(contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id);
                var invoiceData = InvoiceHelper.buildInvoiceAddressDataFromAccountEntity(account);
                InvoiceBL invoiceBL = new InvoiceBL(Service);
                var _id = invoiceBL.createInvoice(invoiceData,
                null,
                contract.Id,
                contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id);

                Entity e = new Entity("rnt_invoice");
                e["rnt_defaultinvoice"] = false;
                e.Id = _id;
                Service.Update(e);
                invoice = new InvoiceRepository(Service).getInvoiceById(_id);
            }
            return invoice;
        }

        public static void updateEquipmentsinMongoDB()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            EquipmentBL equipmentBL = new EquipmentBL(crmServiceHelper.IOrganizationService);
            var str = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                          <entity name='rnt_equipment'>
                            <attribute name='rnt_equipmentid' />
                            <attribute name='rnt_name' />
                            <attribute name='createdon' />
                            <order attribute='rnt_name' descending='false' />
                            <link-entity name='rnt_product' from='rnt_productid' to='rnt_product' link-type='inner' alias='ad'>
                              <filter type='and'>
                                <condition attribute='rnt_groupcodeid' operator='eq' uiname='OD' uitype='rnt_groupcodeinformations' value='{8C1280D3-25B2-E911-A851-000D3A2DD1B4}' />
                              </filter>
                            </link-entity>
                          </entity>
                        </fetch>";
            var results = crmServiceHelper.IOrganizationService.RetrieveMultiple(new FetchExpression(str));
            foreach (var item in results.Entities)
            {
                var e = crmServiceHelper.IOrganizationService.Retrieve("rnt_equipment", item.Id, new ColumnSet(true));
                var actionResponse = equipmentBL.sendEquipmenttoMongoDB("update", e);
            }
        }
        public static void getEquipmentChangedRecords()
        {
            var s = new CrmServiceHelper();

            QueryExpression queryExpression = new QueryExpression("rnt_contractitem");
            queryExpression.ColumnSet = new ColumnSet(true);
            queryExpression.Criteria.AddCondition(new ConditionExpression("rnt_itemtypecode", ConditionOperator.Equal, 1));
            FilterExpression filterExpression = new FilterExpression(LogicalOperator.Or);
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Completed));
            filterExpression.AddCondition(new ConditionExpression("statuscode", ConditionOperator.Equal, (int)rnt_contractitem_StatusCode.Rental));

            queryExpression.Criteria.AddFilter(filterExpression);
            var a = s.IOrganizationService.RetrieveMultiple(queryExpression);
            var rres = a.Entities.GroupBy(p => p.GetAttributeValue<EntityReference>("rnt_contractid")).Select(group => new
            {
                Metric = group.Key,
                Count = group.Count()
            }).ToList();

            var eqChanged = rres.Where(p => p.Count > 1).ToList();
            foreach (var item in eqChanged)
            {
                Console.WriteLine("item :" + item.Metric.Id);
            }
        }
        public static ComparePlateNumberResponse getComparedPlateNumbers()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            try
            {
                var statusCodes = new List<int> { 100000004 };
                ReportBL reportBL = new ReportBL(crmServiceHelper.IOrganizationService);

                var response1 = reportBL.callComparePlateNumbersService("918d5d16-17b1-e811-9410-000c293d97f8", statusCodes);


                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_Report_GetComparedPlateNumbersReport");
                organizationRequest["branchId"] = "918d5d16-17b1-e811-9410-000c293d97f8";
                organizationRequest["statusCodes"] = JsonConvert.SerializeObject(statusCodes);

                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var comparePlateNumberResponse = JsonConvert.DeserializeObject<ComparePlateNumberResponse>(Convert.ToString(response.Results["ServiceResponse"]));

                if (comparePlateNumberResponse.ResponseResult.Result)
                {
                    return new ComparePlateNumberResponse
                    {
                        equipmentItemsCompared = comparePlateNumberResponse.equipmentItemsCompared
                    };
                }

                return new ComparePlateNumberResponse
                {
                    ResponseResult = RntCar.ClassLibrary.ResponseResult.ReturnError(comparePlateNumberResponse.ResponseResult.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {
                return new ComparePlateNumberResponse
                {
                    ResponseResult = RntCar.ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        public static void createInvoiceItemsfromContractItems()
        {
            var service = new CrmServiceHelper().IOrganizationService;
            var contractRef = new EntityReference
            {
                LogicalName = "rnt_contract",
                Id = new Guid("6ab327d8-6704-ea11-a811-000d3a2d0171")
            };
            RntCar.BusinessLibrary.ContractItemRepository contractItemRepository = new RntCar.BusinessLibrary.ContractItemRepository(service);
            var items = contractItemRepository.getCompletedContractItemsByContractId(contractRef.Id);

            XrmHelper xrmHelper = new XrmHelper(service);

            foreach (var item in items)
            {
                InvoiceItemRepository invoiceItemRepository = new InvoiceItemRepository(service);

                var invoiceItem = invoiceItemRepository.getInvoiceItemsByContractItemId(item.Id);


                if (invoiceItem == null)
                {
                    Entity createInvoiceItem = xrmHelper.initializeFromRequest("rnt_contractitem", item.Id, "rnt_invoiceitem");
                    InvoiceRepository invoiceRepository = new InvoiceRepository(service);

                    Entity invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contractRef.Id).FirstOrDefault();
                    if (invoice != null)
                    {

                        createInvoiceItem["rnt_invoiceid"] = new EntityReference(invoice.LogicalName, invoice.Id);
                        createInvoiceItem["rnt_name"] = item.GetAttributeValue<string>("rnt_name");

                        service.Create(createInvoiceItem);
                    }

                }
            }
        }
        static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();
            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"SELECT * FROM Win32_PnPEntity where DeviceID Like ""USB%"""))
                collection = searcher.Get();
            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("DeviceID"),
                (string)device.GetPropertyValue("PNPDeviceID"),
                (string)device.GetPropertyValue("Description")
                ));
            }
            collection.Dispose();
            return devices;
        }
        public class USBDeviceInfo
        {
            public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
            {
                this.DeviceID = deviceID;
                this.PnpDeviceID = pnpDeviceID;
                this.Description = description;
            }
            public string DeviceID { get; private set; }
            public string PnpDeviceID { get; private set; }
            public string Description { get; private set; }
        }
        public static void CreateCreditCardParameters()
        {
            var service = new CrmServiceHelper().IOrganizationService;
            ConfigurationBL configurationBL = new ConfigurationBL();
            var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");

            var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
            IyzicoHelper iyzicoHelper = new IyzicoHelper(configs);
            var creditCardParams = new CreateCreditCardParameters
            {
                cardAlias = "KHALID AMER",
                cardHolderName = "ALKAABI",
                creditCardNumber = "5241505720600044",
                cvc = "184",
                expireMonth = 7,
                expireYear = 2021,
                individualCustomerId = "efbcf8ce-2920-ea11-a810-000d3a2c0643",
                langId = 1055
            };
            CreditCardBL creditCardBL = new CreditCardBL(service);

            var responseCard = creditCardBL.createCreditCard(creditCardParams);
        }
        public static void paymentCreditCardSlip()
        {
            var service = new CrmServiceHelper().IOrganizationService;

            //QueryExpression queryExpression = new QueryExpression("rnt_payment");
            //var results = service.RetrieveMultiple(queryExpression);
            //foreach (var item in results.Entities)
            //{
            //    Entity e = new Entity("rnt_payment");
            //    e["rnt_creditcardslipid"] = null;
            //    e.Id = item.Id;
            //    service.Update(e);
            //}

            QueryExpression slipQuery = new QueryExpression("rnt_creditcardslip");
            slipQuery.ColumnSet = new ColumnSet("rnt_paymentid");
            var slips = service.RetrieveMultiple(slipQuery);
            foreach (var item in slips.Entities)
            {
                Entity e = new Entity("rnt_payment");
                e["rnt_creditcardslipid"] = new EntityReference("rnt_creditcardslip", item.Id);
                if (item.GetAttributeValue<EntityReference>("rnt_paymentid") != null)
                {

                    e.Id = item.GetAttributeValue<EntityReference>("rnt_paymentid").Id;
                    service.Update(e);
                }
            }
        }
        public static void makeContractPayment()
        {
            var service = new CrmServiceHelper().IOrganizationService;
            ContractBL contractBL = new ContractBL(service);
            var creditCardData = new CreditCardData
            {
                cardToken = "349f0ac6-ab6f-40a2-8243-18b277bd9a07",
                cardUserKey = "349f0ac6-ab6f-40a2-8243-18b277bd9a07"
            };
            var invoiceAdress = new InvoiceAddressData
            {

            };
            //966.86)
            var d = Convert.ToDecimal(100);
            var createPaymentParameters = new CreatePaymentParameters
            {
                contractId = new Guid("f55a6fda-16d6-e911-a84e-000d3a2ddb03"),
                transactionCurrencyId = new Guid("036024dd-e8a5-e911-a847-000d3a2bd64e"),
                individualCustomerId = new Guid("a9c31482-12d5-e911-a853-000d3a2ddcbb"),
                conversationId = "4V8G0H",
                langId = 1055,
                paymentTransactionType = (int)PaymentEnums.PaymentTransactionType.SALE,
                creditCardData = creditCardData,
                installment = 1, // installment can not be selected during contract creation
                paidAmount = d,
                invoiceAddressData = invoiceAdress,
                virtualPosId = 5,
                paymentChannelCode = rnt_PaymentChannelCode.BRANCH
            };
            var paymentBL = new PaymentBL(service);
            var response = paymentBL.callMakePaymentAction(createPaymentParameters);
        }

        public static void makeRefund_Deposit()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            PaymentBL paymentBL = new PaymentBL(crmServiceHelper.IOrganizationService);

            QueryExpression queryExpression = new QueryExpression("rnt_contract");
            queryExpression.Criteria.AddCondition("rnt_contractid", ConditionOperator.Equal, "22122936-dfda-e911-a840-000d3a2dd73b");
            queryExpression.ColumnSet = new ColumnSet(true);
            var contract = crmServiceHelper.IOrganizationService.RetrieveMultiple(queryExpression).Entities.FirstOrDefault();

            var totalAmount = contract.GetAttributeValue<Money>("rnt_totalamount").Value;
            var netPayment = contract.GetAttributeValue<Money>("rnt_netpayment").Value;
            var diff = totalAmount - netPayment;
            if (diff < 0)
            {

                paymentBL.createRefund(new CreateRefundParameters
                {
                    isDepositRefund = true,
                    refundAmount = ((decimal)-1 * diff),
                    contractId = contract.Id
                });
            }
        }
        public static void checkBeforeContractCreation()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            var param = JsonConvert.DeserializeObject<CheckBeforeContractCreationParameters>("{'reservationId':'b3583507-13d5-e911-a853-000d3a2ddcbb','isQuickContract':true,'pickupDateTimeStamp':1568268000,'contactId':'a9c31482-12d5-e911-a853-000d3a2ddcbb','langId':1055}");


            ReservationRepository reservationRepository = new ReservationRepository(crmServiceHelper.IOrganizationService);
            var reservation = reservationRepository.getReservationById(param.reservationId, new string[] { "rnt_paymentchoicecode",
                                                                                                                         "createdon",
                                                                                                                         "rnt_pickupdatetime",
                                                                                                                         "statuscode",
                                                                                                                         "statecode",
                                                                                                                         "rnt_pickupbranchid",
                                                                                                                         "rnt_customerid"});

            ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
            var adminId = new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid"));
            //TODO: //tablet kullanıcılarına ayrı bir logic tasarlanacak
            if (crmServiceHelper.CrmServiceClient.GetMyCrmUserId() == adminId)
            {

                return;
            }

            ContractBL contractBL = new ContractBL(crmServiceHelper.IOrganizationService);


        }
        public static void sendSliptologo()
        {
            var contractRef = new EntityReference
            {
                Id = new Guid("f08e8253-bbd0-ea11-a812-000d3a38a128"),
                LogicalName = "rnt_contract"
            }; var rezRef = new EntityReference
            {
                Id = new Guid("57d1aff6-a8d5-ea11-a813-000d3a2c0643"),
                LogicalName = "rnt_reservation"
            };
            var paymentRef = new EntityReference
            {
                Id = new Guid("384ed78c-b8d7-ea11-a813-000d3a2c0643"),
                LogicalName = "rnt_payment"
            };
            CreditCardSlipBL creditCardSlipBL = new CreditCardSlipBL(new CrmServiceHelper().IOrganizationService);
            var response = creditCardSlipBL.handleCreateCreditCardSlipWithLogo(null, null, paymentRef, new Guid("cd5de08f-b8d7-ea11-a813-000d3a2d0732"));
        }

        public static void calculatePricesupdate()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            IOrganizationService orgService = crmServiceHelper.IOrganizationService;

            Entity contract = orgService.Retrieve("rnt_contract", new Guid("e3e3712f-5ed3-eb11-bacc-000d3a2dfe7d"), new ColumnSet(true));

            var availabilityParam = new AvailabilityParameters
            {
                dropOffBranchId = contract.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id,
                dropoffDateTime = contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                contractId = Convert.ToString(contract.Id),
                customerType = contract.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value / 10,
                corporateCustomerId = contract.Attributes.Contains("rnt_corporateid") ?
                           Convert.ToString(contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id) :
                           null,
                pickupBranchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                pickupDateTime = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime")

            };


            var availabilityResponse = new AvailabilityResponse();
            AvailabilityBusiness availabilityBusiness = new AvailabilityBusiness(availabilityParam);
            availabilityResponse.availabilityData = availabilityBusiness.calculateAvailability();

            //var param = JsonConvert.DeserializeObject<CalculatePricesForUpdateContractParameters>("{'pickupBranchId':'d2177c8f-1cb2-e911-a851-000d3a2dd1b4','dropOffBranchId':'d2177c8f-1cb2-e911-a851-000d3a2dd1b4','pickupDateTime':'2019-08-08T12:58:13','dropoffDateTime':'2019-08-10T15:58:13Z','reservationId':null,'contractId':'e7479981-d6b9-e911-a841-000d3a2ddc03','shiftDuration':0,'channel':0,'customerType':1,'priceCodeId':'00000000-0000-0000-0000-000000000000','corporateCustomerId':null,'individualCustomerId':null,'segment':null}");

            //CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            //ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
            //var contractInformation = contractRepository.getContractById(param.contractId,
            //                                                             new string[] {  "rnt_contracttypecode",
            //                                                                              "rnt_corporateid",
            //                                                                              "rnt_pickupbranchid",
            //                                                                              "rnt_pickupdatetime",
            //                                                                              "rnt_groupcodeid",
            //                                                                              "rnt_offset",
            //                                                                              "rnt_pricinggroupcodeid"});




            //ActionHelper actionHelper = new ActionHelper(initializer.Service);
            //todo offset is in reservation item
            //var utcpickupTime = contractInformation.GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset);
            //utcpickupTime = utcpickupTime.AddMinutes(-StaticHelper.offset);

            //AvailibilityBL availibilityBL = new AvailibilityBL(crmServiceHelper.IOrganizationService);

            //todo offset with different timezones will work unproperly
            //var availabilityParam = new AvailabilityParameters
            //{
            //    dropOffBranchId = param.dropoffBranchId,
            //    dropoffDateTime = param.dropoffDateTime,
            //    contractId = Convert.ToString(param.contractId),
            //    customerType = contractInformation.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value / 10,
            //    corporateCustomerId = contractInformation.Attributes.Contains("rnt_corporateid") ?
            //                           Convert.ToString(contractInformation.GetAttributeValue<EntityReference>("rnt_corporateid").Id) :
            //                           null,
            //    pickupBranchId = contractInformation.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
            //    pickupDateTime = utcpickupTime,
            //    other parameters will be filled in calculate action.

            //};


            //var availabilityResponse = new AvailabilityResponse();
            //AvailabilityBusiness availabilityBusiness = new AvailabilityBusiness(availabilityParam);
            //availabilityResponse.availabilityData = availabilityBusiness.calculateAvailability();

            //availabilityResponse.trackingNumber = availabilityBusiness.trackingNumber;

            //var serializedResponse = JsonConvert.DeserializeObject<AvailabilityResponse>(JsonConvert.SerializeObject(availabilityResponse));


            //var calculatePricesForUpdateContractResponse = availibilityBL.calculatePricesUpdateContract(serializedResponse, contractInformation.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id, param.contractId);

        }

        public static void updatePersonalSettings()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            //    QueryExpression queryExpression = new QueryExpression("systemuser");
            //    queryExpression.Criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
            //    var users = crmServiceHelper.IOrganizationService.RetrieveMultiple(queryExpression);

            //    foreach (var item in users.Entities)
            //    {
            Entity userSettingsEntity = new Entity("usersettings");
            userSettingsEntity["timezonecode"] = 134;//istanbul
            UpdateUserSettingsSystemUserRequest req = new UpdateUserSettingsSystemUserRequest
            {
                UserId = new Guid("c395d758-a3a8-e911-a857-000d3a2ddd00"),
                Settings = userSettingsEntity
            };
            crmServiceHelper.IOrganizationService.Execute(req);
            // }

        }

        public static void calculateAvailabilityForRental()
        {
            //CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            //var calculateContractRemainingAmountParameter = JsonConvert.DeserializeObject<CalculateContractRemainingAmountParameters>("{'dropoffBranch':{'branchId':'d2177c8f-1cb2-e911-a851-000d3a2dd1b4','branchName':'Sabiha Gökçen Havalimanı'},'contractInformation':{'segment':2,'contractId':'f7007cb0-c8b5-e911-a841-000d3a2ddc03','contactId':'10175d58-c7b5-e911-a841-000d3a2ddc03','dropoffBranch':null,'manuelPickupDateTimeStamp':0,'manuelDropoffTimeStamp':0,'PickupDateTimeStamp':0,'DropoffTimeStamp':0,'isManuelProcess':false},'equipmentInformation':{'equipmentId':'6df751a3-dcb5-e911-a841-000d3a2ddc03','groupCodeInformationId':'00000000-0000-0000-0000-000000000000','plateNumber':'34ST4620','currentKmValue':96925,'firstKmValue':96557,'currentFuelValue':2,'firstFuelValue':2,'isEquipmentChanged':false,'equipmentInventoryData':[{'isExist':true,'inventoryName':'Ruhsat','logicalName':'rnt_license','equipmentInventoryId':'dc82c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'İlk Yardım Çantası','logicalName':'rnt_firstaidkit','equipmentInventoryId':'de82c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':true,'inventoryName':'Paspas','logicalName':'rnt_floormat','equipmentInventoryId':'e082c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Küllük','logicalName':'rnt_ashtray','equipmentInventoryId':'e282c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Bebek Koltuğu','logicalName':'rnt_babyseat','equipmentInventoryId':'e482c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Çakmak','logicalName':'rnt_lighter','equipmentInventoryId':'e682c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':true,'inventoryName':'Sigorta Poliçesi','logicalName':'rnt_insurancepolicy','equipmentInventoryId':'e882c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Navigasyon Paketi','logicalName':'rnt_navigationpack','equipmentInventoryId':'ea82c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':true,'inventoryName':'Yangın Söndürme Tüpü','logicalName':'rnt_fireextinguisher','equipmentInventoryId':'ec82c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':false,'inventoryName':'Avadanlık','logicalName':'rnt_toolset','equipmentInventoryId':'ee82c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':true,'inventoryName':'Plaka','logicalName':'rnt_plate','equipmentInventoryId':'f082c4ef-3fb2-e911-a849-000d3a2bd64e','price':0.0},{'isExist':true,'inventoryName':'Anten','logicalName':'rnt_aerial','equipmentInventoryId':'403ff9c2-3fb2-e911-a851-000d3a2dd1b4','price':0.0}]},'langId':1055,'channelCode':40}");
            //RntCar.MongoDBHelper.Repository.ContractItemRepository contractItemRepository =
            //        new RntCar.MongoDBHelper.Repository.ContractItemRepository(mongoDBHostName, mongoDBDatabaseName);
            //var contractItem = contractItemRepository.getContractItemEquipmentRental(Convert.ToString(calculateContractRemainingAmountParameter.contractInformation.contractId));
            //var customerType = contractItem.contractType / 10;

            //contractItem.dropoffDateTime = contractItem.pickupDateTime.Value.AddDays(2);
            //contractItem.DropoffTimeStamp = new MongoDB.Bson.BsonTimestamp(contractItem.dropoffDateTime.Value.converttoTimeStamp());
            ////todo corporate will implement later
            //var availabilityResponse = new RntCar.ClassLibrary.MongoDB.AvailabilityResponse();

            //var _dropoffDateTime = Convert.ToDateTime("4.08.2019 18:00:31");
            //AvailabilityBusiness availabilityBusiness = new AvailabilityBusiness(new AvailabilityParameters
            //{
            //    channel = (int)GlobalEnums.Channel.Branch,
            //    //option set values are 10-20-30-40 respectively
            //    customerType = customerType,
            //    contractId = contractItem.contractId,
            //    individualCustomerId = calculateContractRemainingAmountParameter.contractInformation.contactId.ToString(),
            //    corporateCustomerId = contractItem.corporateCustomerId,
            //    dropOffBranchId = calculateContractRemainingAmountParameter.dropoffBranch.branchId.Value,
            //    dropoffDateTime = _dropoffDateTime,
            //    pickupDateTime = contractItem.PickupTimeStamp.Value.converttoDateTime(),
            //    pickupBranchId = new Guid(contractItem.pickupBranchId),
            //    segment = calculateContractRemainingAmountParameter.contractInformation.segment,
            //});
            //availabilityResponse.availabilityData = availabilityBusiness.calculateAvailability();
            //availabilityResponse.trackingNumber = availabilityBusiness.trackingNumber;
            //availabilityResponse.availabilityData = availabilityResponse.availabilityData.Where(p => p.groupCodeInformationId.Equals(new Guid(contractItem.pricingGroupCodeId))).ToList();

            //AvailibilityBL availibilityBL = new AvailibilityBL(crmServiceHelper.IOrganizationService);
            //availibilityBL.processErrorMessages(availabilityResponse, calculateContractRemainingAmountParameter.langId);

            //var response = availibilityBL.calculatePricesUpdateContract(availabilityResponse,
            //                                                            new Guid(contractItem.pricingGroupCodeId), new Guid(contractItem.contractId));
        }
        public static void sendCancelSms()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            SMSBL sMSBL = new SMSBL(crmServiceHelper.IOrganizationService);
            sMSBL.sendSMS("5524780803",
                          "Sayın Hasan Yılmaz , 9D9T4K PNR nolu 3758 TL tutarındaki rezervasyonunuz iptal edilmiştir.Paranızın hesabınıza geçmesi 3-5 iş gününü bulabilir.Rentgo olarak iyi günler dileriz.");

        }
        public static void createRes()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            PaymentBL paymentBL = new PaymentBL(crmServiceHelper.IOrganizationService);
            ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);

            var createPaymentResponse = new CreatePaymentResponse();
            //this parameter must be false after successfull payment 3rd party integration
            var rollBackSystem = true;
            string createPaymentParameters;

            try
            {

                var param = JsonConvert.DeserializeObject<CreatePaymentParameters>("{'id':null,'reservationId':'fe9039c9-90b3-e911-a841-000d3a2ddc03','contractId':null,'individualCustomerId':'2a3e9979-90b3-e911-a841-000d3a2ddc03','transactionCurrencyId':'036024dd-e8a5-e911-a847-000d3a2bd64e','installment':1,'langId':1033,'comissionAmount':0.0,'paidAmount':431.6000000000,'installmentAmount':431.6,'invoiceAddressData':{'addressDetail':'ömerpaşa mah. inci apt no :55 d:3','individualCustomerId':'2a3e9979-90b3-e911-a841-000d3a2ddc03','firstName':'Polat','lastName':'Aydın','governmentId':'37645491122','companyName':'','taxNumber':'','taxOfficeId':'00000000-0000-0000-0000-000000000000','invoiceType':10,'addressCountryId':'c2e4665c-9aaf-e811-9410-000c293d97f8','addressCountryName':'Türkiye','addressCityId':'12ebb25a-4ec6-e811-9413-000c293d97f8','addressCityName':'İstanbul','addressDistrictId':'5d1a5296-4ec6-e811-9413-000c293d97f8','addressDistrictName':'Kadıköy','invoiceAddressId':'f975b17f-90b3-e911-a841-000d3a2ddc03','name':'','email':null,'mobilePhone':null},'creditCardData':{'creditCardId':null,'binNumber':null,'conversationId':null,'externalId':null,'cardUserKey':null,'cardToken':null,'creditCardNumber':'5289 3931 9017 0019','expireYear':21,'expireMonth':1,'cardHolderName':'Polat Aydın','cvc':'256','cardType':null},'paymentTransactionType':10,'conversationId':'9Q4C6H','userName':null,'password':null,'virtualPosId':4,'apikey':null,'secretKey':null,'baseurl':null,'paymentChannelCode':10}");
                var provider = paymentBL.getProviderCode(param.reservationId, param.contractId);
                var id = paymentBL.createPayment(param, provider);
                createPaymentResponse.paymentId = Convert.ToString(id);

                //get the payment provider namespace               
                //initializer.TraceMe("paymentprovider_namespace : " + provider);
                //initiate the related payment provider. 
                IPaymentProvider paymentProvider = null;

                if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
                {
                    paymentProvider = Activator.CreateInstance(typeof(IyzicoProvider)) as IPaymentProvider;
                    var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                    var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                    //setting auth info
                    param.baseurl = configs.iyzico_baseUrl;
                    param.secretKey = configs.iyzico_secretKey;
                    param.apikey = configs.iyzico_apiKey;
                }
                else if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
                {
                    paymentProvider = Activator.CreateInstance(typeof(NetTahsilatProvider)) as IPaymentProvider;
                    var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");
                    //initializer.TraceMe("before getting parameter ");

                    var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);
                    //setting auth info
                    param.userName = configs.nettahsilat_username;
                    param.password = configs.nettahsilat_password;
                    param.baseurl = configs.nettahsilat_url;
                    param.vendorCode = configs.nettahsilat_vendorcode;
                    param.vendorId = configs.nettahsilat_vendorid;
                    //initializer.TraceMe("param.nettahsilat_vendorid " + param.apikey);
                    //initializer.TraceMe("param.userName" + param.userName);
                    //initializer.TraceMe("param.password " + param.password);

                }

                //initializer.TraceMe("instance ready");
                //initializer.TraceMe("payment parameters : " + JsonConvert.SerializeObject(param));

                var response = paymentProvider.makePayment(param);

                //initializer.TraceMe("payment response" + JsonConvert.SerializeObject(response));

                var formattedResponse = (PaymentResponse)response;

                if (formattedResponse.status)
                {
                    if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
                    {
                        //create credit card if success 
                        if (formattedResponse.creditCardSaveSafely.HasValue && formattedResponse.creditCardSaveSafely.Value)
                        {
                            try
                            {
                                var creditCardParameters = new CreateCreditCardParameters
                                {
                                    cardAlias = param.creditCardData.cardHolderName,
                                    creditCardNumber = param.creditCardData.creditCardNumber.removeEmptyCharacters(),
                                    cardHolderName = param.creditCardData.cardHolderName,
                                    cvc = Convert.ToString(param.creditCardData.cvc),
                                    individualCustomerId = Convert.ToString(param.individualCustomerId),
                                    expireMonth = param.creditCardData.expireMonth,
                                    expireYear = param.creditCardData.expireYear,
                                    langId = param.langId
                                };
                                //initializer.TraceMe("creditCardParameters" + JsonConvert.SerializeObject(creditCardParameters));
                                CreditCardBL creditCardBL = new CreditCardBL(crmServiceHelper.IOrganizationService);
                                var creditCardResponse = creditCardBL.createCreditCard_nettahsilat(creditCardParameters,
                                formattedResponse);
                                paymentBL.updatePaymentCreditCard(id, creditCardResponse);
                            }
                            catch (Exception ex)
                            {
                                //if we couldnt save credit card our system , we can not roll back our system.
                                //we need to implement a logic here
                                //initializer.TraceMe("credit card creation error " + ex.Message);
                                //initializer.TraceMe("credit card creation InnerException " + ex.InnerException);
                            }
                        }
                    }
                    rollBackSystem = false;
                    createPaymentResponse.externalPaymentId = formattedResponse.paymentId;
                    createPaymentResponse.externalPaymentTransactionId = formattedResponse.paymentTransactionId;

                    try
                    {
                        paymentBL.updatePaymentResult(id, (PaymentResponse)formattedResponse);
                    }
                    catch (Exception ex)
                    {
                        //initializer.TraceMe("internal exception" + ex.Message);
                        //update record with internal Message
                        paymentBL.updatePaymentRecordInternalError(id, ex.Message);
                    }
                    //createPaymentResponse.ResponseResult = ResponseResult.ReturnSuccess();
                    //initializer.PluginContext.OutputParameters["paymentResponse"] = JsonConvert.SerializeObject(createPaymentResponse);
                    //initializer.PluginContext.OutputParameters["ExecutionResult"] = string.Empty;
                    //initializer.TraceMe("all operations are finished in peace");
                }
                else
                {
                    //initializer.TraceMe("payment error" + response.errorMessage);
                    //will read from xml
                    throw new Exception(response.errorMessage);
                }


            }
            catch (Exception ex)
            {
                if (rollBackSystem)
                {
                    // initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
                }
                else
                {
                    //initializer.PluginContext.OutputParameters["ExecutionResult"] = ex.Message;
                }

            }
        }
        public class Contact
        {
            public string Mobilephone { get; set; }
            public string Email { get; set; }
        }
        public static MongoDBResponse sendContractItemtoMongoDB(string a)
        {
            return new MongoDBResponse
            {
                Id = StaticHelper.GenerateString(10)
            };
        }
        public static T RetryMethod<T>(Func<T> method, int numRetries, int retryTimeout) where T : MongoDBResponse
        {
            T retval = default(T);

            do
            {
                try
                {
                    retval = method();
                    if (retval.Result)
                        return retval;

                    Thread.Sleep(retryTimeout);
                }
                catch
                {

                    Thread.Sleep(retryTimeout);
                }
            } while (numRetries-- > 0);

            return retval;
        }

        public static void generateAndSendSMS()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            var GenerateAndSendSMS = new OrganizationRequest()
            {
                RequestName = "rnt_GenerateAndSendSMS"
            };

            GenerateAndSendSMS.Parameters.Add("MobilePhone", "5343656937");
            //cancelReservationCustomAction.Parameters.Add("offset", (int)item.Offset);
            GenerateAndSendSMS.Parameters.Add("VerificationCode", "123");
            GenerateAndSendSMS.Parameters.Add("LangId", (int)1033);
            GenerateAndSendSMS.Parameters.Add("PnrNumber", "ABC123");
            GenerateAndSendSMS.Parameters.Add("SMSContentCode", (int)100);

            OrganizationResponse response = crmServiceHelper.IOrganizationService.Execute(GenerateAndSendSMS);
        }
        public static void calculateFineAmountforContract()
        {
            ContractBL contractBL = new ContractBL(new CrmServiceHelper().IOrganizationService);
            var validationResponse = contractBL.checkBeforeContractCancellation(new ContractCancellationParameters
            {
                pnrNumber = "1P2C7Q",
                contractId = new Guid("18181762-5d98-e911-a828-000d3a47cb1d")
            }, 1033);

            if (!validationResponse.ResponseResult.Result)
            {
                return;
            }

            validationResponse = contractBL.calculateCancellationAmountForGivenContract(validationResponse,
                                                                                        new Guid("18181762-5d98-e911-a828-000d3a47cb1d"),
                                                                                        validationResponse.willChargeFromUser,
                                                                                        1033);
        }
        public static void calculatePriceContract()
        {
            //CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            //var param = JsonConvert.DeserializeObject<CalculatePricesForUpdateContractParameters>("{'contractId':'470e9600-5298-e911-a828-000d3a47cb1d','dropoffDateTime':'2019-06-29T00:15:00.000Z','dropoffBranchId':'b78d5d16-17b1-e811-9410-000c293d97f8','langId':1033}");

            //ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
            //var contractInformation = contractRepository.getContractById(param.contractId,
            //                                                                           new string[] {  "rnt_contracttypecode",
            //                                                                                           "rnt_corporateid",
            //                                                                                           "rnt_pickupbranchid",
            //                                                                                           "rnt_pickupdatetime",
            //                                                                                           "rnt_groupcodeid",
            //                                                                                           "rnt_offset",
            //                                                                                           "rnt_pricinggroupcodeid"});


            ////ActionHelper actionHelper = new ActionHelper(initializer.Service);
            ////todo offset is in reservation item
            //var utcpickupTime = contractInformation.GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset);
            //// utcpickupTime = utcpickupTime.AddMinutes(-StaticHelper.offset);
            //AvailibilityBL availibilityBL = new AvailibilityBL(crmServiceHelper.IOrganizationService);
            ////todo offset with different timezones will work unproperly
            //var availabilityParam = new AvailabilityParameters
            //{
            //    dropOffBranchId = param.dropoffBranchId,
            //    dropoffDateTime = param.dropoffDateTime,
            //    contractId = Convert.ToString(param.contractId),
            //    customerType = contractInformation.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value / 10,
            //    corporateCustomerId = contractInformation.Attributes.Contains("rnt_corporateid") ?
            //                           Convert.ToString(contractInformation.GetAttributeValue<EntityReference>("rnt_corporateid").Id) :
            //                           null,
            //    pickupBranchId = contractInformation.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
            //    pickupDateTime = utcpickupTime,
            //    //other parameters will be filled in calculate action.

            //};

            //var response = availibilityBL.calculateAvailibility(availabilityParam, param.langId);
            //var serializedResponse = JsonConvert.DeserializeObject<AvailabilityResponse>(response);

            //var calculatePricesForUpdateContractResponse = availibilityBL.calculatePricesUpdateContract(serializedResponse, contractInformation.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id, contractInformation.Id);
            //;

        }
        public static void decideGroupCodeChanges()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            AvailibilityBL availibilityBL = new AvailibilityBL(crmServiceHelper.IOrganizationService);
            var currentGroupCode = JsonConvert.DeserializeObject<RntCar.ClassLibrary.MongoDB.AvailabilityData>("{'groupCodeInformationId':'38dc8dee-7eb3-e811-9410-000c293d97f8','groupCodeInformationName':'H','ratio':0,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':80,'payLaterTotalPrice':80,'totalPrice':80,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':-80,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null}");
            var changedGroupCode = JsonConvert.DeserializeObject<List<RntCar.ClassLibrary.MongoDB.AvailabilityData>>("[{'groupCodeInformationId':'c2aaa3d5-7cb3-e811-9410-000c293d97f8','groupCodeInformationName':'A','ratio':0,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':80,'payLaterTotalPrice':80,'totalPrice':80,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':-80,'equipmentCount':12,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'666132f4-7cb3-e811-9410-000c293d97f8','groupCodeInformationName':'K','ratio':10,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':80,'payLaterTotalPrice':80,'totalPrice':80,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':-80,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'2827937d-7eb3-e811-9410-000c293d97f8','groupCodeInformationName':'B','ratio':0,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':80,'payLaterTotalPrice':80,'totalPrice':80,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':-80,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'f2eeff92-7eb3-e811-9410-000c293d97f8','groupCodeInformationName':'F','ratio':0,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':80,'payLaterTotalPrice':80,'totalPrice':80,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':-80,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'0a256ea9-7eb3-e811-9410-000c293d97f8','groupCodeInformationName':'C','ratio':10,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':80,'payLaterTotalPrice':80,'totalPrice':80,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':-80,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'38dc8dee-7eb3-e811-9410-000c293d97f8','groupCodeInformationName':'H','ratio':0,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':80,'payLaterTotalPrice':80,'totalPrice':80,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':-80,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'19e64d33-7fb3-e811-9410-000c293d97f8','groupCodeInformationName':'D','ratio':0,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':80,'payLaterTotalPrice':80,'totalPrice':80,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':-80,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'19ced4bd-7cb3-e811-9410-000c293d97f8','groupCodeInformationName':'AD','ratio':20,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':96,'payLaterTotalPrice':96,'totalPrice':96,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':-64,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'d6c8c90c-83b3-e811-9410-000c293d97f8','groupCodeInformationName':'P','ratio':0,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':160,'payLaterTotalPrice':160,'totalPrice':160,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':0,'equipmentCount':11,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'dbf1844d-7fb3-e811-9410-000c293d97f8','groupCodeInformationName':'OD','ratio':8,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':160,'payLaterTotalPrice':160,'totalPrice':160,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':0,'equipmentCount':12,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'ef9671cc-82b3-e811-9410-000c293d97f8','groupCodeInformationName':'G','ratio':0,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':160,'payLaterTotalPrice':160,'totalPrice':160,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':0,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'090a6b37-83b3-e811-9410-000c293d97f8','groupCodeInformationName':'N','ratio':0,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':160,'payLaterTotalPrice':160,'totalPrice':160,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':0,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null},{'groupCodeInformationId':'893e1252-83b3-e811-9410-000c293d97f8','groupCodeInformationName':'M','ratio':0,'isUpgrade':false,'isDowngrade':false,'payNowTotalPrice':160,'payLaterTotalPrice':160,'totalPrice':160,'totalPaidPrice':160,'documentEquipmentPrice':160,'paidPriceEquipment':160,'amounttobePaid_Equipment':0,'equipmentCount':10,'totalDuration':1,'isPriceCalculatedSafely':true,'priceErrorMessage':null,'hasError':false,'errorMessage':null,'canUserStillHasCampaignBenefit':false,'documentHasCampaignBefore':false,'CampaignInfo':null}]");

            var response = availibilityBL.decideChangedGroupCodeStatus(currentGroupCode, changedGroupCode);
        }

        public static void updateContractt()
        {
            //var s = "{'contractId':'fd3e622f-1674-ea11-a811-000d3a2e9b6c','contactId':'f8ce5028-b2fb-e811-a94f-000d3a454977','groupCodeInformationId':'f2eeff92-7eb3-e811-9410-000c293d97f8','currency':'3d7a8cfd-6ef3-e811-a950-000d3a4546af','totalDuration':2,'customerType':0,'changedReason':20,'contractStatusCode':100000000,'contractItemStatusCode':100000001,'isDateorBranchChanged':true,'isCarChanged':false,'trackingNumber':'1700504169','additionalDrivers':[],'dateAndBranch':{'pickupBranchId':'b38d5d16-17b1-e811-9410-000c293d97f8','dropoffBranchId':'9f8d5d16-17b1-e811-9410-000c293d97f8','pickupDate':'2020-04-01T12:45:32Z','dropoffDate':'2020-04-03T12:45:32Z'},'additionalProduct':{'selectedAdditionalProductsData':[{'productId':'703d0ad5-5fe8-e811-a971-000d3a26c57a','productName':'Tek Yön','productType':10,'productCode':'SRV006','maxPieces':1,'totalDuration':0,'actualTotalAmount':200.0,'actualAmount':200.0000,'value':1,'priceCalculationType':2,'monthlyPackagePrice':0.0}]},'addressData':{'addressDetail':'Merkez Mahallesi Reha Yurdakul Sokak Yumurcak Apt. No:30 D:7','individualCustomerId':'f8ce5028-b2fb-e811-a94f-000d3a454977','firstName':'Ceyhun','lastName':'Kalınoğlu','governmentId':'17471446206','companyName':'','taxNumber':'','taxOfficeId':'00000000-0000-0000-0000-000000000000','invoiceType':10,'addressCountryId':'c2e4665c-9aaf-e811-9410-000c293d97f8','addressCountryName':'Türkiye','addressCityId':'12ebb25a-4ec6-e811-9413-000c293d97f8','addressCityName':'İstanbul','addressDistrictId':'671a5296-4ec6-e811-9413-000c293d97f8','addressDistrictName':'Şişli','invoiceAddressId':'78cf5028-b2fb-e811-a94f-000d3a454977','name':'','email':null,'mobilePhone':null,'defaultInvoice':false},'priceParameters':{'reservationPaidAmount':0.0,'initialPrice':0.0,'price':298.48,'paymentType':0,'pricingType':null,'depositAmount':0.0,'installment':1,'virtualPosId':null,'installmentTotalAmount':0.0,'amounttobePaid':0.0,'transactionCurrencyId':'00000000-0000-0000-0000-000000000000','campaignId':null,'creditCardData':[]},'channelCode':20}";
            //var deserializedContractParameters = JsonConvert.DeserializeObject<ContractUpdateParameters>(s);
            //List<ContractItemResponse> contractItemResponses = new List<ContractItemResponse>();
            //ContractBL contractBL = new ContractBL(new CrmServiceHelper().IOrganizationService);
            //var response = contractBL.updateContract(deserializedContractParameters, contractItemResponses);
        }

        public static void UpdateCampaign()
        {
            CampaignData campaignData = new CampaignData
            {
                beginingDate = Convert.ToDateTime("2019-06-06T23:00:00"),
                endDate = Convert.ToDateTime("2019-10-23T23:00:00"),
                name = "Fenerbahçe Kart Sahiplerine %30 İndirim",
                description = "Fenerbahçe Kart Sahiplerine %30 İndirim",
                campaignId = "7c638427-4967-e911-a81b-000d3a47c8cb",
            };
            CampaignBusiness campaignBusiness = new CampaignBusiness("mongodb://localhost:27017", "RentgoMongdoDB");
            campaignBusiness.updateCampaign(campaignData, "5cc19308d511fc356c252153");
        }
        public static void CreateInvoiceItem()
        {
            PaymentBL a = new PaymentBL();
            InvoiceItemBL InvoiceItemBL = new InvoiceItemBL(a.OrgService);
            Entity e = new Entity("rnt_invoiceitem");
            e["rnt_reservationitemid"] = new EntityReference("rnt_reservationitem", new Guid("18d63211-5091-e911-a825-000d3a47c97c"));
            e["rnt_invoiceid"] = new EntityReference("rnt_invoice", new Guid("1ad63211-5091-e911-a825-000d3a47c97c"));

            e["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("3d7a8cfd-6ef3-e811-a950-000d3a4546af"));

            e["rnt_totalamount"] = new Money(155);
            var response = InvoiceItemBL.OrgService.Create(e);
        }
        public static List<Entity> getPluginTraceLogs()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            QueryExpression query = new QueryExpression("plugintracelog");
            query.ColumnSet = new ColumnSet("exceptiondetails");
            query.Criteria.AddCondition("messagename", ConditionOperator.Equal, "rnt_CreateEquipmentInMongoDB");
            query.Criteria.AddCondition("exceptiondetails", ConditionOperator.NotEqual, "");
            return crmServiceHelper.IOrganizationService.RetrieveMultiple(query).Entities.ToList();
        }

        public static void makeNetTahsilat()
        {
            PaymentBL paymentBL = new PaymentBL();
            ConfigurationBL configurationBL = new ConfigurationBL(paymentBL.OrgService);

            var createPaymentResponse = new CreatePaymentResponse();
            //this parameter must be false after successfull payment 3rd party integration
            var rollBackSystem = true;
            string createPaymentParameters = "{'reservationId':'93b8aa9a-708d-e911-a824-000d3a47c97c','contractId':null,'individualCustomerId':'dfd443ec-6d8d-e911-a824-000d3a47c97c','transactionCurrencyId':'3d7a8cfd-6ef3-e811-a950-000d3a4546af','installment':1,'langId':1033,'paidAmount':400.0000000000,'invoiceAddressData':{'addressDetail':'myaddress','individualCustomerId':'00000000-0000-0000-0000-000000000000','firstName':'test','lastName':'adres','governmentId':'50642353828','companyName':'','taxNumber':'','taxOfficeId':'00000000-0000-0000-0000-000000000000','invoiceType':10,'addressCountryId':'c2e4665c-9aaf-e811-9410-000c293d97f8','addressCountryName':'Türkiye','addressCityId':'daeab25a-4ec6-e811-9413-000c293d97f8','addressCityName':'Ankara','addressDistrictId':'7d175296-4ec6-e811-9413-000c293d97f8','addressDistrictName':'Şereflikoçhisar','invoiceAddressId':'b5bc2f13-6e8d-e911-a824-000d3a47c97c','name':''},'creditCardData':{'creditCardId':'e37ac847-708d-e911-a824-000d3a47ca4c','binNumber':null,'conversationId':null,'externalId':null,'cardUserKey':'457398e8-316f-475b-bbd6-5e0a2b9a9371','cardToken':'457398e8-316f-475b-bbd6-5e0a2b9a9371','creditCardNumber':null,'expireYear':null,'expireMonth':null,'cardHolderName':null,'cvc':null,'cardType':null},'paymentTransactionType':10,'paymentChannel':50,'apikey':null,'secretKey':null,'baseurl':null,'conversationId':'3M8Q7O','name':'Ecozum','surname':'Test','gsmNumber':'5524780803','email':'ecozum@ecozum.com','id':'00005190','registrationAddress':'registration_address','identityNumber':'50642353828','city':'willadded','country':'willadded','userName':null,'password':null}";


            try
            {
                var param = JsonConvert.DeserializeObject<CreatePaymentParameters>(createPaymentParameters);

                var provider = paymentBL.getProviderCode(param.reservationId, param.contractId);
                //todo will set from input
                param.installment = 1;
                //todo will implement in validation class                 
                var id = paymentBL.createPayment(param, provider);
                createPaymentResponse.paymentId = Convert.ToString(id);

                //get the payment provider namespace               

                //initiate the related payment provider. 
                IPaymentProvider paymentProvider = null;

                if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
                {
                    paymentProvider = Activator.CreateInstance(typeof(IyzicoProvider)) as IPaymentProvider;
                    var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                    var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                    //setting auth info
                    param.baseurl = configs.iyzico_baseUrl;
                    param.secretKey = configs.iyzico_secretKey;
                    param.apikey = configs.iyzico_apiKey;
                }
                else if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
                {
                    paymentProvider = Activator.CreateInstance(typeof(NetTahsilatProvider)) as IPaymentProvider;

                    var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");
                    var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);
                    //setting auth info
                    param.userName = configs.nettahsilat_username;
                    param.password = configs.nettahsilat_password;
                    param.baseurl = configs.nettahsilat_url;
                    param.vendorCode = configs.nettahsilat_vendorcode;
                    param.vendorId = configs.nettahsilat_vendorid;
                }

                var response = paymentProvider.makePayment(param);

                var formattedResponse = (PaymentResponse)response;

                if (formattedResponse.status)
                {
                    if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
                    {
                        //create credit card if success 
                        if (formattedResponse.creditCardSaveSafely.HasValue && formattedResponse.creditCardSaveSafely.Value)
                        {
                            try
                            {
                                CreditCardBL creditCardBL = new CreditCardBL(paymentBL.OrgService);
                                var creditCardResponse = creditCardBL.createCreditCard_nettahsilat(new CreateCreditCardParameters
                                {
                                    cardAlias = param.creditCardData.cardHolderName,
                                    creditCardNumber = param.creditCardData.creditCardNumber.removeEmptyCharacters(),
                                    cardHolderName = param.creditCardData.cardHolderName,
                                    cvc = Convert.ToString(param.creditCardData.cvc),
                                    individualCustomerId = Convert.ToString(param.individualCustomerId),
                                    expireMonth = param.creditCardData.expireMonth,
                                    expireYear = param.creditCardData.expireYear,
                                    langId = param.langId
                                },
                                formattedResponse);
                                paymentBL.updatePaymentCreditCard(id, creditCardResponse);
                            }
                            catch (Exception ex)
                            {
                                //if we couldnt save credit card our system , we can not roll back our system.
                                //we need to implement a logic here
                            }
                        }
                    }
                    rollBackSystem = false;
                    createPaymentResponse.externalPaymentId = formattedResponse.paymentId;
                    createPaymentResponse.externalPaymentTransactionId = formattedResponse.paymentTransactionId;

                    try
                    {
                        paymentBL.updatePaymentResult(id, (PaymentResponse)formattedResponse);

                    }
                    catch (Exception ex)
                    {

                        //update record with internal Message
                        paymentBL.updatePaymentRecordInternalError(id, ex.Message);

                    }
                    createPaymentResponse.ResponseResult = RntCar.ClassLibrary.ResponseResult.ReturnSuccess();

                }
                else
                {
                    //will read from xml
                    throw new Exception(response.errorMessage);
                }


            }
            catch (Exception ex)
            {


            }
        }

        static void logoIntegration()
        {
            //RentgoService.RentGoService rentGoService = new RentgoService.RentGoService();
            //rentGoService.Login()
        }
        static void EkranaYazdir(string taskAdi)
        {
            for (int k = 0; k < 100; k++)
            {
                Console.WriteLine(taskAdi + " - " + k);
                Thread.Sleep(50);
            }
        }
        public static string Encrypt(string cipherText, string encryptkey)
        {
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(cipherText);
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(encryptkey, new byte[] { 61, 23 });
            byte[] encryptedData = Encrypt(clearBytes, pdb.GetBytes(32),
                pdb.GetBytes(16));
            return Convert.ToBase64String(encryptedData);
        }
        private static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            MemoryStream ms = new MemoryStream();
            Rijndael alg = Rijndael.Create();
            alg.Key = Key; alg.IV = IV; CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(clearData, 0, clearData.Length);
            cs.Close();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
        }

        public static void getAdditionalProductforUpdate()
        {
            var service = new CrmServiceHelper().IOrganizationService;
            var groupCodeData = JsonConvert.DeserializeObject<GroupCodeInformationCRMData>("{'rnt_name':'B','rnt_groupcodeinformationsid':'4b492b14-24b2-e911-a851-000d3a2dd1b4','rnt_minimumage':21,'rnt_minimumdriverlicence':1,'rnt_segment':20,'rnt_youngdriverage':21,'rnt_youngdriverlicence':1,'changeType':10}");
            var individualCustomerData = JsonConvert.DeserializeObject<IndividualCustomerDetailData>("{'firstname':'MEHMET SIDDIK','lastname':'AKŞİT','birthdate':'01/11/1966','individualCustomerId':'1e77dc63-2752-ea11-a812-000d3a2d0476','distributionChannelCode':20,'corporateCustomerId':'00000000-0000-0000-0000-000000000000'}");
            var selectedDateAndBranchData = JsonConvert.DeserializeObject<ReservationDateandBranchParameters>("{'pickupBranchId':'b9a176f3-2211-ea11-a811-000d3a2c0643','dropoffBranchId':'b9a176f3-2211-ea11-a811-000d3a2c0643','pickupDate':'2020-02-19T05:30:00.000Z','dropoffDate':'2020-02-19T18:30:00.000Z'}");
            AdditionalProductValidation additionalProductValidation = new AdditionalProductValidation(service);

            var r = additionalProductValidation.checkFindeks(new Guid(groupCodeData.rnt_groupcodeinformationsid),
                                                             individualCustomerData.individualCustomerId,
                                                             individualCustomerData.pricingType,
                                                             new Guid("bbae8f49-2852-ea11-a812-000d3a2d0476"),
                                                             individualCustomerData.corporateCustomerId,
                                                             groupCodeData.changeType,
                                                             1055);
            //if (!r.ResponseResult.Result)
            //    AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(service);
            //var result = additionalProductsBL.GetAdditionalProductForUpdate(groupCodeData, individualCustomerData, selectedDateAndBranchData, "bbae8f49-2852-ea11-a812-000d3a2d0476", 1055);
        }
        public static void getAdditionalProducts_tablet()
        {


            GetAdditionalProductsParameters getAdditionalProductsParameters = new GetAdditionalProductsParameters
            {
                contractId = new Guid("caf63245-9748-e911-a957-000d3a454330"),
                customerId = new Guid("df7517f0-fefa-e811-a94f-000d3a454330"),
                groupCodeId = new Guid("090a6b37-83b3-e811-9410-000c293d97f8"),
                langId = 1055
            };
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(crmServiceHelper.IOrganizationService);
            //prepare related parameters
            GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(crmServiceHelper.IOrganizationService);
            var groupCodeEntity = groupCodeInformationRepository.getGroupCodeInformationById(getAdditionalProductsParameters.groupCodeId,
                                                                                             new string[] {"rnt_youngdriverlicence",
                                                                                                           "rnt_youngdriverage",
                                                                                                           "rnt_minimumage"});
            GroupCodeInformationCRMData groupCodeInformationCRMData = new GroupCodeInformationCRMData
            {
                rnt_youngdriverlicence = groupCodeEntity.GetAttributeValue<int>("rnt_youngdriverlicence"),
                rnt_youngdriverage = groupCodeEntity.GetAttributeValue<int>("rnt_youngdriverage"),
                rnt_minimumage = groupCodeEntity.GetAttributeValue<int>("rnt_minimumage")
            };

            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
            var individualEntity = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(getAdditionalProductsParameters.customerId,
                                                                                    new string[] {"birthdate",
                                                                                                      "rnt_drivinglicensedate"});
            IndividualCustomerDetailData individualCustomerDetailData = new IndividualCustomerDetailData
            {
                birthDate = individualEntity.GetAttributeValue<DateTime>("birthdate"),
                drivingLicenseDate = individualEntity.GetAttributeValue<DateTime>("rnt_drivinglicensedate")
            };
            //get contractitem from mongodb
            ContractItemBusiness contractItemBusiness = new ContractItemBusiness(mongoDBHostName, mongoDBDatabaseName);
            var contract = new ContractDateandBranchData();//contractItemBusiness.getContractEquipmentDateandBranchs(Convert.ToString(getAdditionalProductsParameters.contractId));

            ContractDateandBranchParameters contractDateandBranchParameters = new ContractDateandBranchParameters
            {
                dropoffBranchId = contract.dropoffBranchId,
                pickupBranchId = contract.pickupBranchId,
                dropoffDate = contract.dropoffDateTime,
                pickupDate = contract.pickupDateTime
            };
            //todo wwill read from somewhere generic
            var totalDuration = (contract.dropoffDateTime - contract.pickupDateTime).TotalDays;
            var r = additionalProductsBL.GetAdditionalProductForUpdateContract_tablet(groupCodeInformationCRMData,
                                                                            individualCustomerDetailData,
                                                                            contractDateandBranchParameters,
                                                                            Convert.ToString(getAdditionalProductsParameters.contractId),
                                                                            Convert.ToInt32(totalDuration),
                                                                            getAdditionalProductsParameters.langId);
        }
        public static void getAdditionalProducts()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            var p = JsonConvert.DeserializeObject<IndividualCustomerDetailData>("{'firstname':'MUTLU','lastname':'KOPARAN','birthdate':'1997-03-10T00:00:00','drivinglicensedate':'2015-12-23T00:00:00','individualCustomerId':'28e6eab3-4873-ea11-a811-000d3a2c0643','distributionChannelCode':30,'pricingType':'28e6eab3-4873-ea11-a811-000d3a2c0643'}");
            var g = JsonConvert.DeserializeObject<GroupCodeInformationCRMData>("{'rnt_name':'C','rnt_groupcodeinformationsid':'1d4b5330-25b2-e911-a851-000d3a2dd1b4','rnt_minimumage':25,'rnt_minimumdriverlicence':2,'rnt_segment':40,'rnt_youngdriverage':21,'rnt_youngdriverlicence':1,'currencyId':'036024dd-e8a5-e911-a847-000d3a2bd64e'}");
            var t = JsonConvert.DeserializeObject<ReservationDateandBranchParameters>("{'pickupBranchId':'72245d31-1dd0-e911-a853-000d3a2ddcbb','dropoffBranchId':'72245d31-1dd0-e911-a853-000d3a2ddcbb','pickupDate':'2020-04-01T23:15:00.152Z','dropoffDate':'2020-04-02T23:15:00.000Z'}");
            AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(crmServiceHelper.IOrganizationService);
            var response = additionalProductsBL.GetAdditionalProducts(g, p, t, 1);

        }
        public static void updateTransfersForMongo()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            QueryExpression queryExpression = new QueryExpression("rnt_transfer");
            queryExpression.ColumnSet = new ColumnSet(true);
            var entities = crmServiceHelper.IOrganizationService.RetrieveMultiple(queryExpression).Entities.ToList();
            foreach (var item in entities)
            {
                Entity entity = new Entity("rnt_transfer");
                entity.Id = item.Id;
                entity["rnt_servicename"] = string.Empty;
                crmServiceHelper.IOrganizationService.Update(entity);
            }
        }

        public static void transferRecords()
        {
            transfercountries();
            //transfercities();
            //transferdistrict();
            //transferTaxOffices();
            //transferBrand();
            //transferModel();
            //transferEngineVolume();
            //transferHorsePower();
            //transferGroupCodeInfo();
            //transferBranch();
            //transferWorkingHour();
            transferKmLimits();
            //transferProducts();
            //transferEquipments();
            transferCrmConfiguration();
        }
        public static void updatePriceLists()
        {
            RntCar.MongoDBHelper.Repository.PriceListRepository priceListRepository = new RntCar.MongoDBHelper.Repository.PriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                               StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var res = priceListRepository.getActivePriceLists();
            var collection = priceListRepository.getCollection<PriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceListCollectionName"));
            foreach (var item in res)
            {
                //DailyPricesBusiness priceCalculationSummariesBusiness = new DailyPricesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), 
                //                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
            }

        }
        public static void updateAvailabilityPrices()
        {
            RntCar.MongoDBHelper.Repository.AvailabilityPriceListRepository contractItemRepository = new RntCar.MongoDBHelper.Repository.AvailabilityPriceListRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                                       StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var res = contractItemRepository.getAvailabilityPriceByPriceLists();
            var collection = contractItemRepository.getCollection<AvailabilityPriceListDataMongoDB>(StaticHelper.GetConfiguration("MongoDBAvailabilityPriceListListCollectionName"));
            foreach (var item in res)
            {
                //DailyPricesBusiness priceCalculationSummariesBusiness = new DailyPricesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), 
                //                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
            }

        }
        public static void updateContractItems()
        {
            RntCar.MongoDBHelper.Repository.ContractItemRepository contractItemRepository = new RntCar.MongoDBHelper.Repository.ContractItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                                       StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var res = contractItemRepository.getContractItems();
            var collection = contractItemRepository.getCollection<ContractItemDataMongoDB>(StaticHelper.GetConfiguration("MongoDBContractItemCollectionName"));
            foreach (var item in res)
            {
                //DailyPricesBusiness priceCalculationSummariesBusiness = new DailyPricesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), 
                //                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
            }

        }
        public static void updateEquipmentAvailability()
        {

            RntCar.MongoDBHelper.Repository.EquipmentAvailabilityRepository equipmentAvailabilityRepository = new RntCar.MongoDBHelper.Repository.EquipmentAvailabilityRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                                       StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var res = equipmentAvailabilityRepository.getEquipmentAvailability();
            var collection = equipmentAvailabilityRepository.getCollection<EquipmentAvailabilityDataMongoDB>("EquipmentAvailability");
            foreach (var item in res)
            {
                //DailyPricesBusiness priceCalculationSummariesBusiness = new DailyPricesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), 
                //                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                item.DailyReservationCount = item.ReservationCount;


                collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
            }

        }
        public static void updatePriceFactors()
        {
            RntCar.MongoDBHelper.Repository.PriceFactorRepository priceFactorRepository = new RntCar.MongoDBHelper.Repository.PriceFactorRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                                     StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var res = priceFactorRepository.getPriceFactors();
            var collection = priceFactorRepository.getCollection<PriceFactorDataMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceFactorCollectionName"));
            foreach (var item in res)
            {

                collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
            }

        }
        public static void updateReservationItems()
        {
            RntCar.MongoDBHelper.Repository.ReservationItemRepository contractItemRepository = new RntCar.MongoDBHelper.Repository.ReservationItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                                             StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var res = contractItemRepository.getAllReservationItems();
            var collection = contractItemRepository.getCollection<ReservationItemDataMongoDB>("ReservationItems");
            foreach (var item in res)
            {
                //DailyPricesBusiness priceCalculationSummariesBusiness = new DailyPricesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), 
                //                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
            }
        }
        public static void updateReservationItemsLastXHours(int lastXHours)
        {
            IOrganizationService orgService = new CrmServiceHelper().IOrganizationService;
            RntCar.BusinessLibrary.Repository.ReservationItemRepository reservationItemRepository = new RntCar.BusinessLibrary.Repository.ReservationItemRepository(orgService);
            var reservationItems = reservationItemRepository.getReservationItemsByXHours(new string[] { "statecode", "statuscode" }, lastXHours);

            RntCar.MongoDBHelper.Repository.ReservationItemRepository reservationItemRepositoryMongo = new RntCar.MongoDBHelper.Repository.ReservationItemRepository(mongoDBHostName, mongoDBDatabaseName);
            var reservationItemsMongo = reservationItemRepositoryMongo.getAllReservationItems_ALL();

            var collectionRes = reservationItemRepositoryMongo.getCollection<ReservationItemDataMongoDB>("ReservationItems");
            int i = 0;
            foreach (var item in reservationItems)
            {
                try
                {
                    var reservationItemId = item.Id;
                    var statecode = item.GetAttributeValue<OptionSetValue>("statecode").Value;
                    var statuscode = item.GetAttributeValue<OptionSetValue>("statuscode").Value;

                    var document = reservationItemsMongo.Where(p => p.ReservationItemId == Convert.ToString(reservationItemId)).FirstOrDefault();
                    if (document != null)
                    {
                        document.StateCode = statecode;
                        document.StatusCode = statuscode;

                        var filter = Builders<ReservationItemDataMongoDB>.Filter.Eq(s => s.ReservationItemId, Convert.ToString(reservationItemId));
                        var result = collectionRes.Replace(document, filter, new UpdateOptions { IsUpsert = false }, document.ReservationItemId);
                    }
                    i++;
                    Console.WriteLine(i);
                }
                catch
                {
                    continue;
                }
            }
        }
        public static void checkCreditCard()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            CreditCardValidation creditCardValidation = new CreditCardValidation(crmServiceHelper.IOrganizationService);
            var a = JsonConvert.DeserializeObject<ReservationPriceParameters>("{'customerCreditCardId':'e6834c99-a630-e911-a957-000d3a454e11','cardUserKey':'CZ0V0xNV7Ky9QfR30vzo8yWtoM8=','cardToken':'d/+D2Ptu5oDHDq7Mpw13tjAii4o=','price':932.05,'paymentType':10,'initialPrice':796.89,'reservationPaidAmount':796.89}");
            //var res = creditCardValidation.checkCreditCard_BeforeReservation(a, 1033);
        }



        public static void updateContractDailyPrices()
        {
            RntCar.MongoDBHelper.Repository.ContractDailyPricesRepository dailyPricesRepository = new RntCar.MongoDBHelper.Repository.ContractDailyPricesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var res = dailyPricesRepository.getContractDailyPricesByReservationItemId_str(new Guid("48f4bc87-5c0c-eb11-a813-000d3a2c0643"));
            var collection = dailyPricesRepository.getCollection<ContractDailyPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBContractDailyPrice"));
            foreach (var item in res)
            {
                //DailyPricesBusiness priceCalculationSummariesBusiness = new DailyPricesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), 
                //                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));
                item.totalAmount = item.totalAmount + 129.63M;
                //collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
            }

        }
        public static void deletePriceCalculationSummaries(int month, int year)
        {
            MongoDBInstance mongoDBInstance = new MongoDBInstance(mongoDBHostName, mongoDBDatabaseName);
            var collection = mongoDBInstance.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummaries"));
            collection.DeleteMany(p => new DateTime(1970, 1, 1).AddSeconds(p.priceDateTimeStamp.Timestamp).Month == month &&
                new DateTime(1970, 1, 1).AddSeconds(p.priceDateTimeStamp.Timestamp).Year == year);
        }
        public static async Task deletePriceCalculationSummaries()
        {
            RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository priceCalculationSummariesRepository = new RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var res = priceCalculationSummariesRepository.getPriceCalculationSummaries();
            var collection = priceCalculationSummariesRepository.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));

            RntCar.MongoDBHelper.Repository.ContractDailyPricesRepository dailyPricesRepository = new RntCar.MongoDBHelper.Repository.ContractDailyPricesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            RntCar.MongoDBHelper.Repository.DailyPricesRepository priceRepo = new RntCar.MongoDBHelper.Repository.DailyPricesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));

            var i = 1;
            List<string> trackings = new List<string>();
            foreach (var item in res)
            {
                try
                {
                    if (item.priceDate < new DateTime(2021, 01, 1))
                    {

                        var r = trackings.Where(p => p == item.trackingNumber).FirstOrDefault();
                        if (r == null)
                        {
                            var contractPrices = dailyPricesRepository.getContractDailyPricesTrackingNumber(item.trackingNumber);
                            var resPrices = priceRepo.getDailyPricesByTrackingNumber(item.trackingNumber);

                            if (contractPrices.Count == 0 && resPrices.Count == 0)
                            {
                                trackings.Add(item.trackingNumber);
                                trackings = trackings.DistinctBy(p => p).ToList();
                                Console.WriteLine(trackings.Count + " total count : " + i);
                                i++;
                                if (trackings.Count > 150000)
                                {
                                    break;
                                }
                            }

                        }
                        else
                        {
                            i++;
                            Console.WriteLine("same " + trackings.Count + " total count : " + i);
                        }

                    }

                }
                catch (Exception)
                {

                    continue;
                }

            }

            MongoDBInstance mongoDBInstance = new MongoDBInstance(mongoDBHostName, mongoDBDatabaseName);
            var filter = Builders<PriceCalculationSummaryMongoDB>.Filter.In("trackingNumber", trackings.Select(k => k));
            await collection.DeleteManyAsync(filter);


        }
        public static void updatePriceCalculationSummaries()
        {
            RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository priceCalculationSummariesRepository = new RntCar.MongoDBHelper.Repository.PriceCalculationSummariesRepository(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                                                                                StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            var res = priceCalculationSummariesRepository.getPriceCalculationSummaries();
            var collection = priceCalculationSummariesRepository.getCollection<PriceCalculationSummaryMongoDB>(StaticHelper.GetConfiguration("MongoDBPriceCalculationSummary"));
            foreach (var item in res)
            {
                PriceCalculationSummariesBusiness priceCalculationSummariesBusiness = new PriceCalculationSummariesBusiness(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
            }

        }
        public static void priceOperations()
        {
            //PriceCalculator priceCalculator = new PriceCalculator(Guid.NewGuid(), "huhu");
            //priceCalculator.applyChannel(DateTime.Now.converttoTimeStamp(), 100, 100, 10);
            //priceCalculator.applyWeekdays(DateTime.Now.converttoTimeStamp(), 100, priceCalculator.Prices.priceAfterChannelFactor);
        }
        public static void searchContract()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            var paramss = JsonConvert.DeserializeObject<ContractSearchParameters>("{'contractNumber':null,'pickupBranchId':null,'pickupDate':null,'dropoffBranchId':null,'dropoffDate':null,'customerId':'seda','plateNumber':'34abc01'}");
            ContractBL contractBL = new ContractBL(crmServiceHelper.IOrganizationService);
            var response = contractBL.searchContractByParameters(paramss, 1033);
            //OrganizationRequest request = new OrganizationRequest("rnt_SearchContract");
            //var searchParam = new ContractSearchParameters
            //{
            //    plateNumber = "34ABC01"
            //};
            //request["ContractSearchParameters"] = JsonConvert.SerializeObject(searchParam);
            //request["langId"] = 1033;
            //request["offset"] = 180;

            //var response = crmServiceHelper.IOrganizationService.Execute(request);



        }
        public static void UpdateContract()
        {
            //string contractParameters;

            //int langId = 1033;            
            //var deserializedContractParameters = JsonConvert.DeserializeObject<ContractUpdateParameters>(contractParameters);

            //initializer.TraceMe("contractParameters : " + JsonConvert.SerializeObject(deserializedContractParameters));
            ////initializer.TraceMe("getContractByIdByGivenColumns start");
            //ContractRepository contractRepository = new ContractRepository(initializer.Service);
            //var contract = contractRepository.getContractByIdByGivenColumns(deserializedContractParameters.contractId,
            //                                                               new string[] { "rnt_totalamount" });
            ////initializer.TraceMe("getContractByIdByGivenColumns end");
            //var initialAmount = contract.Attributes.Contains("rnt_totalamount") ?
            //                    contract.GetAttributeValue<Money>("rnt_totalamount").Value :
            //                    decimal.Zero;

            ////initializer.TraceMe("contract price parameters: " + deserializedContractParameters.priceParameters.price);

            //var paymentCard = deserializedContractParameters.priceParameters.creditCardData.Where(x => x.cardType == (int)PaymentEnums.PaymentTransactionType.SALE &&
            //                        (x.creditCardId != null || !string.IsNullOrEmpty(x.creditCardNumber))).FirstOrDefault();
            //if (deserializedContractParameters.priceParameters.amounttobePaid > decimal.Zero)
            //{
            //    CreditCardValidation creditCardValidation = new CreditCardValidation(initializer.Service);
            //    var creditCardResponse = creditCardValidation.checkCreditCard(paymentCard, deserializedContractParameters.contactId, langId);
            //    if (!creditCardResponse.ResponseResult.Result)
            //    {
            //        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(creditCardResponse.ResponseResult.ExceptionDetail);
            //    }
            //}
            ////initializer.TraceMe("contract amounttobepaid parameters: " + deserializedContractParameters.priceParameters.amounttobePaid);
            //if (deserializedContractParameters.isCarChanged)
            //{

            //    ContractUpdateValidation contractUpdateValidation = new ContractUpdateValidation(initializer.Service, initializer.TracingService);
            //    var validationResponse = contractUpdateValidation.checkContractStatusForCarChangeRequest(deserializedContractParameters.contractId, langId);
            //    if (!validationResponse.ResponseResult.Result)
            //    {
            //        initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(validationResponse.ResponseResult.ExceptionDetail);
            //    }
            //}
            ////initializer.TraceMe("contractParameters deserialization end");

            //ContractBL contractBL = new ContractBL(initializer.Service, initializer.TracingService);

            ////initializer.TraceMe("contract update start");

            //var response = contractBL.updateContract(deserializedContractParameters);

            ////initializer.TraceMe("createAdditionalDrivers start");
            //if (deserializedContractParameters.additionalDrivers.Count > 0 && deserializedContractParameters.additionalDrivers != null)
            //{
            //    AdditionalDriversBL additionalDriversBL = new AdditionalDriversBL(initializer.Service, initializer.TracingService);
            //    additionalDriversBL.createAdditionalDrivers(deserializedContractParameters.additionalDrivers, response.contractId);
            //}
            ////initializer.TraceMe("createAdditionalDrivers end");

            ////initializer.TraceMe("contract update end");

            //var latestContract = contractRepository.getContractByIdByGivenColumns(deserializedContractParameters.contractId,
            //                                                                      new string[] { "rnt_totalamount" });

            //var latestAmount = latestContract.GetAttributeValue<Money>("rnt_totalamount").Value;

            //var differenceAmount = latestAmount - initialAmount;

            ////initializer.TraceMe("payment strat");
            //contractBL.makeContractPayment(differenceAmount,
            //         decimal.Zero,
            //         paymentCard,
            //         null,
            //         deserializedContractParameters.addressData,
            //         deserializedContractParameters.currency,
            //         deserializedContractParameters.contactId,
            //         deserializedContractParameters.contractId,
            //         Guid.Empty,
            //         response.pnrNumber,
            //         new PaymentStatus
            //         {
            //             isDepositPaid = true,
            //             isReservationPaid = false
            //         },
            //         langId);
        }

        public static void updateContractforRental()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            var updateContractforRentalParametersSerialized = JsonConvert.DeserializeObject<UpdateContractforRentalParameters>("{'equipmentInformation':{'equipmentId':'a2cf3457-5898-e911-a828-000d3a47cb1d','groupCodeInformationId':'00000000-0000-0000-0000-000000000000','plateNumber':'34AD05','currentKmValue':100,'firstKmValue':19,'currentFuelValue':8,'firstFuelValue':8,'isEquipmentChanged':false,'equipmentInventoryData':[{'isExist':true,'inventoryName':'Anten','logicalName':'rnt_aerial','equipmentInventoryId':'e47a9e9a-5a30-e911-a957-000d3a454e11','price':0.0},{'isExist':true,'inventoryName':'Ruhsat','logicalName':'rnt_license','equipmentInventoryId':'1e53a1a0-5a30-e911-a957-000d3a454e11','price':0.0},{'isExist':true,'inventoryName':'İlk Yardım Çantası','logicalName':'rnt_firstaidkit','equipmentInventoryId':'b7a699a6-5a30-e911-a957-000d3a454e11','price':0.0},{'isExist':true,'inventoryName':'Paspas','logicalName':'rnt_floormat','equipmentInventoryId':'c91292b2-5a30-e911-a957-000d3a454e11','price':0.0},{'isExist':true,'inventoryName':'Küllük','logicalName':'rnt_ashtray','equipmentInventoryId':'ed138cb8-5a30-e911-a957-000d3a454e11','price':0.0},{'isExist':true,'inventoryName':'Çakmak','logicalName':'rnt_lighter','equipmentInventoryId':'d20190be-5a30-e911-a957-000d3a454e11','price':0.0},{'isExist':true,'inventoryName':'Sigorta Poliçesi','logicalName':'rnt_insurancepolicy','equipmentInventoryId':'c8c29aca-5a30-e911-a957-000d3a454e11','price':0.0},{'isExist':true,'inventoryName':'Yangın Söndürme Tüpü','logicalName':'rnt_fireextinguisher','equipmentInventoryId':'9be0d766-5b30-e911-a957-000d3a454e11','price':0.0},{'isExist':true,'inventoryName':'Avadanlık','logicalName':'rnt_toolset','equipmentInventoryId':'7befd46c-5b30-e911-a957-000d3a454e11','price':0.0},{'isExist':true,'inventoryName':'Bebek Koltuğu','logicalName':'rnt_babyseat','equipmentInventoryId':'c20ecf31-5f30-e911-a957-000d3a454e11','price':0.0},{'isExist':true,'inventoryName':'Navigasyon Paketi','logicalName':'rnt_navigationpack','equipmentInventoryId':'ddd8b349-5f30-e911-a957-000d3a454e11','price':0.0}]},'userInformation':{'userId':'964a10c5-2e5d-e911-a95c-000d3a454e11','branchId':'b78d5d16-17b1-e811-9410-000c293d97f8'},'contractInformation':{'contractId':'470e9600-5298-e911-a828-000d3a47cb1d','contactId':'f3561c25-1690-e911-a825-000d3a47cb1d','dropoffBranch':{'branchId':'b78d5d16-17b1-e811-9410-000c293d97f8','branchName':'DALAMAN ŞEHİR OFİSİ'},'manuelPickupDateTimeStamp':0,'manuelDropoffTimeStamp':0,'PickupDateTimeStamp':1561584301,'DropoffTimeStamp':1561757101,'isManuelProcess':false},'additionalProducts':[],'paymentInformation':{'creditCardData':[]},'damageData':[],'otherDocuments':null,'langId':1055}");

            ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
            var contract = contractRepository.getContractById(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                           new string[] { "rnt_pickupbranchid",
                                                                                           "rnt_dropoffbranchid" ,
                                                                                           "rnt_reservationid" ,
                                                                                           "rnt_totalamount",
                                                                                           "rnt_pickupdatetime",
                                                                                           "transactioncurrencyid",
                                                                                           "rnt_groupcodeid",
                                                                                           "rnt_customerid",
                                                                                           "rnt_pnrnumber"
                                                                           });

            var initialAmount = contract.Attributes.Contains("rnt_totalamount") ?
                               contract.GetAttributeValue<Money>("rnt_totalamount").Value :
                               decimal.Zero;

            #region createEquipmentInventoryHistoryfor Rental
            var inventoryList = updateContractforRentalParametersSerialized.equipmentInformation.equipmentInventoryData.
                              ConvertAll(x => new CreateEquipmentInventoryHistoryParameter
                              {
                                  isExists = x.isExist.Value,
                                  logicalName = x.logicalName
                              }).ToList();
            EquipmentInventoryBL equipmentInventoryBL = new EquipmentInventoryBL(crmServiceHelper.IOrganizationService);
            var equipmentInventoryId = equipmentInventoryBL.createEquipmentInventoryHistoryforRental(updateContractforRentalParametersSerialized.equipmentInformation.equipmentId,
                                                                                                     updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                                     null,
                                                                                                     inventoryList);

            #endregion

            RntCar.BusinessLibrary.Repository.EquipmentRepository equipmentRepository = new RntCar.BusinessLibrary.Repository.EquipmentRepository(crmServiceHelper.IOrganizationService);
            var currentEquipmentTransaction = equipmentRepository.getEquipmentByIdByGivenColumns(updateContractforRentalParametersSerialized.equipmentInformation.equipmentId,
                                                                                                 new string[] { "rnt_equipmenttransactionid" });


            #region Equipment Transaction Operations
            EquipmentTransactionBL equipmentTransactionBL = new EquipmentTransactionBL(crmServiceHelper.IOrganizationService);
            equipmentTransactionBL.updateEquipmentTransactionHistoryforRental(new CreateEquipmentTransactionHistoryRentalParameters
            {
                contractId = updateContractforRentalParametersSerialized.contractInformation.contractId,
                firstFuelValue = updateContractforRentalParametersSerialized.equipmentInformation.firstFuelValue,
                firstKmValue = updateContractforRentalParametersSerialized.equipmentInformation.firstKmValue,
                equipmentId = updateContractforRentalParametersSerialized.equipmentInformation.equipmentId,
                rentalFuelValue = updateContractforRentalParametersSerialized.equipmentInformation.currentFuelValue,
                rentalKmValue = updateContractforRentalParametersSerialized.equipmentInformation.currentKmValue,
                equipmentTransactionId = currentEquipmentTransaction.GetAttributeValue<EntityReference>("rnt_equipmenttransactionid").Id
            });
            #endregion

            ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
            var oneWayFeeCode = configurationRepository.GetConfigurationByKey("OneWayFeeCode");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(crmServiceHelper.IOrganizationService);
            var product = additionalProductRepository.getAdditionalProductByProductCode(oneWayFeeCode);


            RntCar.BusinessLibrary.ContractItemRepository contractItemRepository = new RntCar.BusinessLibrary.ContractItemRepository(crmServiceHelper.IOrganizationService);
            var contractOneWayFee = contractItemRepository.getContractItemByAdditionalProductIdandContractId(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                                                             product.Id);
            ContractItemBL contractItemBL = new ContractItemBL(crmServiceHelper.IOrganizationService);

            if (contractOneWayFee != null)
            {
                var oneWayFeeProduct = updateContractforRentalParametersSerialized.additionalProducts.Where(p => p.productCode == oneWayFeeCode).FirstOrDefault();
                if (oneWayFeeProduct != null)
                {
                    // first disable contract one way fee
                    //contractItemBL.deactiveContractItemItemById(contractOneWayFee.Id, (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.CanceledByCustomer_Inactive);

                    var diff = contractOneWayFee.GetAttributeValue<Money>("rnt_totalamount").Value + oneWayFeeProduct.tobePaidAmount;
                    // decide create new product or not
                    if (diff == decimal.Zero)
                    {
                        // if value eq zero, disable contract one way fee and remove from create list
                        updateContractforRentalParametersSerialized.additionalProducts.Remove(oneWayFeeProduct);
                    }
                }
            }

            #region get contarct invoice
            InvoiceRepository invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
            var invoice = invoiceRepository.getFirstInvoiceByContractId(updateContractforRentalParametersSerialized.contractInformation.contractId);
            #endregion

            foreach (var item in updateContractforRentalParametersSerialized.additionalProducts)
            {
                var additionalProductData = new AdditionalProductData();
                //copy reservationItemData to ReservationItemDataMongoDB
                additionalProductData = additionalProductData.Map(item);
                //todo will make helper

                //contractItemBL.createContractItemfromAdditionalProductData(additionalProductData,
                //new ContractItemRequiredData
                //{
                //    contactId = updateContractforRentalParametersSerialized.contractInformation.contactId,//new Guid("7F824836-97F6-E811-A94F-000D3A454330"),
                //    contractId = updateContractforRentalParametersSerialized.contractInformation.contractId,
                //    statuscode = (int)ContractItemEnums.StatusCode.Completed,
                //    transactionCurrencyId = contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id,
                //    dropoffBranchId = updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId.Value,
                //    pickupBranchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                //    pickupDateTime = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                //    groupCodeInformationId = contract.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id,
                //    dropoffDateTime = DateTime.UtcNow.AddMinutes(StaticHelper.offset),
                //    itemTypeCode = (int)GlobalEnums.ItemTypeCode.Fine
                //},
                //invoice.Id);
            }
            ContractBL contractBL = new ContractBL(crmServiceHelper.IOrganizationService);
            contractBL.updateContractRentalRelatedFields(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                         updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId,
                                                         updateContractforRentalParametersSerialized.contractInformation.manuelDropoffTimeStamp,
                                                         updateContractforRentalParametersSerialized.contractInformation.isManuelProcess,
                                                         null);


            //contractItemBL.updateContractItemsRentalRelatedFields(updateContractforRentalParametersSerialized.contractInformation.contractId,
            //                                                      updateContractforRentalParametersSerialized.contractInformation.dropoffBranch.branchId,
            //                                                      updateContractforRentalParametersSerialized.contractInformation.manuelDropoffTimeStamp,
            //                                                      updateContractforRentalParametersSerialized.contractInformation.isManuelProcess);

            EquipmentBL equipmentBL = new EquipmentBL(crmServiceHelper.IOrganizationService);
            equipmentBL.updateEquipmentforRental(updateContractforRentalParametersSerialized.equipmentInformation.equipmentId,
                                                 equipmentInventoryId,
                                                 currentEquipmentTransaction.GetAttributeValue<EntityReference>("rnt_equipmenttransactionid").Id,
                                                 updateContractforRentalParametersSerialized.userInformation.branchId,
                                                 updateContractforRentalParametersSerialized.equipmentInformation.currentKmValue,
                                                 updateContractforRentalParametersSerialized.equipmentInformation.currentFuelValue);

            #region Create Damages
            var damageParameters = new CreateDamageParameter
            {
                contractId = updateContractforRentalParametersSerialized.contractInformation.contractId,
                equipmentId = updateContractforRentalParametersSerialized.equipmentInformation.equipmentId,
                damageData = updateContractforRentalParametersSerialized.damageData
            };
            DamageBL damageBL = new DamageBL(crmServiceHelper.IOrganizationService);
            var response = damageBL.createDamages(damageParameters);
            if (!response.responseResult.result)
            {
                //initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(response.responseResult.exceptionDetail);
            }
            #endregion

            var latestContract = contractRepository.getContractById(updateContractforRentalParametersSerialized.contractInformation.contractId,
                                                                    new string[] { "rnt_totalamount" });

            var latestAmount = latestContract.GetAttributeValue<Money>("rnt_totalamount").Value;


            //contractBL.makeContractPayment(latestAmount - initialAmount,
            //         decimal.Zero,
            //         updateContractforRentalParametersSerialized.paymentInformation.creditCardData.FirstOrDefault(),
            //         null,
            //         //todo will be parametric
            //         new InvoiceAddressData
            //         {
            //             addressDetail = "deneme",
            //             firstName = "deneme",
            //             lastName = "deneme",
            //             addressCountryId = new Guid("c2e4665c-9aaf-e811-9410-000c293d97f8"),
            //             addressCountryName = "Türkiye",
            //             addressCityId = new Guid("12ebb25a-4ec6-e811-9413-000c293d97f8"),
            //             addressCityName = "İstanbul",
            //             addressDistrictId = new Guid("5d1a5296-4ec6-e811-9413-000c293d97f8"),
            //             addressDistrictName = "Kadıköy",
            //             invoiceAddressId = new Guid("0d15a3d9-c4f5-e811-a94f-000d3a454e11")
            //         },
            //         contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id,
            //         contract.GetAttributeValue<EntityReference>("rnt_customerid").Id,
            //         latestContract.Id,
            //         Guid.Empty,
            //        contract.GetAttributeValue<string>("rnt_pnrnumber"),
            //         new PaymentStatus
            //         {
            //             isDepositPaid = true,
            //             isReservationPaid = false
            //         },
            //         updateContractforRentalParametersSerialized.langId);

            //initializer.PluginContext.OutputParameters["UpdateContractforDeliveryResponse"] = JsonConvert.SerializeObject(ClassLibrary._Tablet.ResponseResult.ReturnSuccess());
        }
        public static void searchReservation()
        {
            var a = "{'reservationNumber':null,'pickupBranchId':'a78d5d16-17b1-e811-9410-000c293d97f8','dropoffBranchId':null,'pickupDate':null,'dropoffDate':null,'customerId':null}";
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            ReservationBL reservationBL = new ReservationBL(crmServiceHelper.IOrganizationService);
            //var response = reservationBL.searchReservationByParameters(a, 180, 1033);
        }
        public static void updateMongoDBWithNewFieldSample_ContractItem()
        {

            //ContractItemRepository contractItemRepository = new ContractItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            //var collection = contractItemRepository.getCollection<ContractItemDataMongoDB>(StaticHelper.GetConfiguration("MongoDBContractItemCollectionName"));
            //var d = contractItemRepository.getContractItems();
            //foreach (var item in d)
            //{

            //    var filter1 = Builders<ContractItemDataMongoDB>.Filter.Eq("contractItemId", item.contractItemId);


            //    var filter = filter1;
            //    collection.UpdateMany(filter,
            //                          Builders<ContractItemDataMongoDB>.Update.Set("PickupTimeStamp", new BsonTimestamp(item.pickupDateTime.Value.converttoTimeStamp())));

            //    collection.UpdateMany(filter,
            //                         Builders<ContractItemDataMongoDB>.Update.Set("DropoffTimeStamp", new BsonTimestamp(item.dropoffDateTime.Value.converttoTimeStamp())));

            //}

        }

        public static void updateMongoDBWithNewFieldSample_ReservationItem()
        {
            //ReservationItemRepository reservationItemRepository = new ReservationItemRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            //var collection = reservationItemRepository.getCollection<ReservationItemDataMongoDB>(StaticHelper.GetConfiguration("MongoDBReservationCollectionName"));
            //var d = reservationItemRepository.getAllReservationItems();
            //foreach (var item in d)
            //{
            //    //var filter1 = Builders<ReservationItemDataMongoDB>.Filter.Eq("ReservationItemId", item.ReservationItemId);

            //    //var filter = filter1;
            //    //collection.UpdateMany(filter,
            //    //                      Builders<ReservationItemDataMongoDB>.Update.Set("CancellationTimeStamp",
            //    //                                                                      item.CancellationTimeStamp.HasValue ? new BsonTimestamp(item.CancellationTimeStamp.Value) : null));

            //    //collection.UpdateMany(filter,
            //    //                      Builders<ReservationItemDataMongoDB>.Update.Set("NoShowTimeStamp",
            //    //                                                                      item.NoShowTimeStamp.HasValue ? new BsonTimestamp(item.NoShowTimeStamp.Value) : null));
            //}
        }
        public static void updateMongoDBWithNewFieldSample()
        {

            //DailyPricesRepository dailyPricesRepository = new DailyPricesRepository(StaticHelper.GetConfiguration("MongoDBHostName"), StaticHelper.GetConfiguration("MongoDBDatabaseName"));
            //var collection = dailyPricesRepository.getCollection<DailyPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBDailyPrice"));
            //var d = dailyPricesRepository.getDailyPrices();
            //foreach (var item in d)
            //{
            //    var filter1 = Builders<DailyPriceDataMongoDB>.Filter.Eq("reservationItemId", item.reservationItemId);

            //    var filter = filter1;
            //    collection.UpdateMany(filter,
            //                          Builders<DailyPriceDataMongoDB>.Update.Set("reservationItemId_str", item.reservationItemId.ToString()));

            //}

        }
        public static void installment()
        {
            CrmServiceHelper h = new CrmServiceHelper();


            var installmentParameter = new RetrieveInstallmentParameters
            {
                amount = 150,
                cardBin = "415565"
            };
            //CreditCardBL creditCardBL = new CreditCardBL(h.IOrganizationService);
            //creditCardBL.retrieveInstallmentforGivenCard(installmentParameter);

        }
        public static void makeRefund()
        {
            var refundParams = @"{'reservationId':'c46937ec-3631-e911-a954-000d3a454330',
                                'contractId':null,'paymentChannel':0,'baseurl':'https://sandbox-api.iyzipay.com','apikey':
                                'sandbox-FjY5OOBFaUs3lQHaQECz4nI5Qf1FvTd2','secretKey':'sandbox-We6z9FconUpnxrBqvqysz2UuIR9iXYnZ',
                                'conversationId':'4S8Q0J','paymentTransactionId':'11900404','refundAmount':411.4000000000}";
            var par = JsonConvert.DeserializeObject<CreateRefundParameters>(refundParams);
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            PaymentBL paymentBL = new PaymentBL(crmServiceHelper.IOrganizationService);
            paymentBL.createRefund(par);
        }
        public static void retrieveInstallmentAction()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            CreditCardBL creditCardBL = new CreditCardBL(crmServiceHelper.IOrganizationService);
            var response = creditCardBL.retrieveInstallmentforGivenCard(new RetrieveInstallmentParameters
            {
                amount = 150,
                cardBin = "557113"
            });

            //CrmServiceHelper h = new CrmServiceHelper();
            //OrganizationRequest request = new OrganizationRequest("rnt_RetrieveInstallment");
            //var installmentParameter = new RetrieveInstallmentParameters
            //{
            //    amount = 150,
            //    cardBin = "557113"
            //};
            //request["retrieveInstallmentParameters"] = JsonConvert.SerializeObject(installmentParameter);
            //var response = h.IOrganizationService.Execute(request);
            //var r = response.Results["retrieveInstallmentResponse"];
            //var res = JsonConvert.DeserializeObject<InstallmentResponse>(r.ToString());
        }

        public static void processCustomerAddress()
        {

            CrmServiceHelper h = new CrmServiceHelper();
            OrganizationRequest request = new OrganizationRequest("rnt_RetrieveInstallment");
            var InvoiceAddressCreateParameters = new InvoiceAddressCreateParameters
            {
                addressCityId = new Guid("12EBB25A-4EC6-E811-9413-000C293D97F8"),
                addressCountryId = new Guid("C2E4665C-9AAF-E811-9410-000C293D97F8"),
                addressDetail = "my detail",
                addressDistrictId = new Guid("E51A5296-4EC6-E811-9413-000C293D97F8"),
                firstName = "polat",
                lastName = "aydın",
                governmentId = "37645491122",
                individualCustomerId = new Guid("A4B27D58-A12B-E911-A95C-000D3A454F67"),
                invoiceType = 10,
                invoiceAddressId = new Guid("e0da6ffd-7e2f-e911-a954-000d3a454330")

            };
            var str = JsonConvert.SerializeObject(InvoiceAddressCreateParameters);
            request["invoiceAddressParameters"] = JsonConvert.SerializeObject(InvoiceAddressCreateParameters);
            var response = h.IOrganizationService.Execute(request);
            var r = response.Results["InvoiceAddressResponse"];
            var res = JsonConvert.DeserializeObject<InvoiceAddressProcessResponse>(r.ToString());
        }

        public static void getCustomerInvoiceAddress()
        {
            CrmServiceHelper h = new CrmServiceHelper();
            OrganizationRequest request = new OrganizationRequest("rnt_GetCustomerInvoiceAddresses");
            request["individualCustomerId"] = "7F824836-97F6-E811-A94F-000D3A454330";
            var response = h.IOrganizationService.Execute(request);
            var r = response.Results["InvoiceAddressResponse"];
            var res = JsonConvert.DeserializeObject<List<InvoiceAddressData>>(r.ToString());
        }
        public static void makePayment()
        {
            var service = new CrmServiceHelper().IOrganizationService;
            var createPaymentResponse = new CreatePaymentResponse();
            #region payment parameters
            var createPaymentParameters = @"{'reservationId':null,'contractId':'05f692da-560a-ea11-a811-000d3a2c0643','individualCustomerId':'73ec127a-560a-ea11-a811-000d3a2c0643','transactionCurrencyId':'036024dd-e8a5-e911-a847-000d3a2bd64e','installment':1,'langId':1033,'installmentRatio':0.0,'paidAmount':100.0,'installmentAmount':null,'invoiceAddressData':{'addressDetail':null,'individualCustomerId':'00000000-0000-0000-0000-000000000000','firstName':null,'lastName':null,'governmentId':null,'companyName':null,'taxNumber':null,'taxOfficeId':null,'invoiceType':0,'addressCountryId':'00000000-0000-0000-0000-000000000000','addressCountryName':null,'addressCityId':null,'addressCityName':null,'addressDistrictId':null,'addressDistrictName':null,'invoiceAddressId':null,'name':null,'email':null,'mobilePhone':null,'defaultInvoice':false},'invoiceId':null,'creditCardData':{'creditCardId':'91887dbc-560a-ea11-a811-000d3a2c0643','binNumber':null,'conversationId':null,'externalId':null,'cardUserKey':'YMtVNIsNbrdDdLpMcYLZgcpOots=','cardToken':'c4Bo+9pR6d6PlIehu/LZU8JUE/4=','creditCardNumber':null,'expireYear':null,'expireMonth':null,'cardHolderName':null,'cvc':null,'cardType':null},'paymentTransactionType':10,'conversationId':'5O8Z8N','userName':null,'password':null,'virtualPosId':0,'apikey':null,'secretKey':null,'baseurl':null,'paymentChannelCode':10,'rollbackOperation':null}";
            #endregion

            var param = JsonConvert.DeserializeObject<CreatePaymentParameters>(createPaymentParameters);
            PaymentBL paymentBL = new PaymentBL(service);
            ConfigurationBL configurationBL = new ConfigurationBL(service);

            //this parameter must be false after successfull payment 3rd party integration
            var rollBackSystem = true;


            try
            {
                //get the payment provider namespace               
                var provider = paymentBL.getProviderCode(param.reservationId, param.contractId);

                //for installments
                if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
                {

                    RntCar.BusinessLibrary.Helpers.PaymentHelper paymentHelper = new PaymentHelper(service);
                    var ratio = paymentHelper.getInstallmentRatio(param.creditCardData, param.installment == 0 ? 1 : param.installment);
                    param.installmentRatio = ratio;
                }

                var id = paymentBL.createPayment(param, provider);

                createPaymentResponse.paymentId = Convert.ToString(id);
                //initiate the related payment provider. 
                IPaymentProvider paymentProvider = null;

                if (provider == (int)GlobalEnums.PaymentProvider.iyzico)
                {
                    paymentProvider = Activator.CreateInstance(typeof(IyzicoProvider)) as IPaymentProvider;
                    var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");

                    var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                    //setting auth info
                    param.baseurl = configs.iyzico_baseUrl;
                    param.secretKey = configs.iyzico_secretKey;
                    param.apikey = configs.iyzico_apiKey;

                    //also check customer credit card
                    if (string.IsNullOrEmpty(param.creditCardData.cardToken) && string.IsNullOrEmpty(param.creditCardData.cardUserKey))
                    {
                        CreditCardRepository creditCardRepository = new CreditCardRepository(service);
                        string _expireYear = string.Empty;
                        if (param.creditCardData.expireYear.Value.ToString().Length == 2)
                        {
                            _expireYear = "20" + param.creditCardData.expireYear.Value;
                        }
                        else
                        {
                            _expireYear = param.creditCardData.expireYear.Value.ToString();
                        }
                        var card = creditCardRepository.getCustomerCreditCardsByGivenParametersByGivenColumns(param.individualCustomerId,
                                                                                                              CommonHelper.formatCreditCardNumber(param.creditCardData.creditCardNumber.removeEmptyCharacters()),
                                                                                                              Convert.ToInt32(_expireYear),
                                                                                                              param.creditCardData.expireMonth.Value,
                                                                                                              new string[] { "rnt_carduserkey", "rnt_cardtoken" });
                        //we found the card so no need to create again
                        if (card != null)
                        {

                            param.creditCardData.cardUserKey = card.GetAttributeValue<string>("rnt_carduserkey");
                            param.creditCardData.cardToken = card.GetAttributeValue<string>("rnt_cardtoken");

                            paymentBL.updatePaymentCreditCard(id, card.Id);

                        }
                        else
                        {

                            IyzicoHelper iyzicoHelper = new IyzicoHelper(configs);
                            var creditCardParams = new CreateCreditCardParameters
                            {
                                cardAlias = param.creditCardData.creditCardNumber,
                                cardHolderName = param.creditCardData.cardHolderName,
                                creditCardNumber = param.creditCardData.creditCardNumber.removeEmptyCharacters(),
                                cvc = param.creditCardData.cvc,
                                expireMonth = param.creditCardData.expireMonth,
                                expireYear = param.creditCardData.expireYear,
                                individualCustomerId = param.individualCustomerId.ToString(),
                                langId = param.langId
                            };

                            try
                            {
                                CreditCardBL creditCardBL = new CreditCardBL(service);
                                var responseCard = creditCardBL.createCreditCard(creditCardParams);
                                paymentBL.updatePaymentCreditCard(id, responseCard.creditCardId);


                                param.creditCardData.cardUserKey = responseCard.cardUserKey;
                                param.creditCardData.cardToken = responseCard.cardToken;


                                if (param.invoiceAddressData != null)
                                {
                                    param.invoiceAddressData.email = responseCard.emailAddress;
                                }

                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    }
                    else
                    {
                        CreditCardRepository creditCardRepository = new CreditCardRepository(service);
                        var existingCard = creditCardRepository.getCreditCardByUserKeyandCreditCardTokenByGivenColumns(param.creditCardData.cardUserKey, param.creditCardData.cardToken, new string[] { });
                        paymentBL.updatePaymentCreditCard(id, existingCard.Id);
                    }

                    if (param.invoiceAddressData == null || (param.invoiceAddressData != null && !param.invoiceAddressData.invoiceAddressId.HasValue))
                    {

                        if (param.contractId.HasValue)
                        {
                            InvoiceRepository invoiceRepository = new InvoiceRepository(service);
                            var invoice = invoiceRepository.getInvoicesByContractId(param.contractId.Value);

                            if (invoice.FirstOrDefault() != null)
                            {
                                param.invoiceAddressData = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoice.FirstOrDefault());
                            }
                            //means invoice is  completed
                            else
                            {
                                invoice = invoiceRepository.getFirstActiveInvoiceByContractId(param.contractId.Value);
                                param.invoiceAddressData = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoice.FirstOrDefault());
                            }

                        }
                        else if (param.reservationId.HasValue)
                        {
                            InvoiceRepository invoiceRepository = new InvoiceRepository(service);
                            var invoice = invoiceRepository.getInvoiceByReservationId(param.contractId.Value);
                            param.invoiceAddressData = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(invoice);
                        }
                    }

                    if (string.IsNullOrEmpty(param.invoiceAddressData.email))
                    {
                        param.invoiceAddressData.email = "pay@rentgo.com";
                    }
                    if (string.IsNullOrEmpty(param.invoiceAddressData.addressCityName))
                    {
                        param.invoiceAddressData.addressCityName = "Rentgo Merkez";
                    }
                    if (param.installment == 0)
                    {
                        param.installment = 1;
                    }
                }
                else if (provider == (int)GlobalEnums.PaymentProvider.nettahsilat)
                {
                    paymentProvider = Activator.CreateInstance(typeof(NetTahsilatProvider)) as IPaymentProvider;
                    var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");


                    var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);
                    //setting auth info
                    param.userName = configs.nettahsilat_username;
                    param.password = configs.nettahsilat_password;
                    param.baseurl = configs.nettahsilat_url;
                    param.vendorCode = configs.nettahsilat_vendorcode;
                    param.vendorId = configs.nettahsilat_vendorid;


                }


            }

            catch (Exception ex)
            {

            }

        }
        public static void getCustomerCreditCard()
        {
            CrmServiceHelper h = new CrmServiceHelper();
            OrganizationRequest request = new OrganizationRequest("rnt_GetCustomerCreditCards");
            request["customerId"] = "7F824836-97F6-E811-A94F-000D3A454330";
            var response = h.IOrganizationService.Execute(request);
            var r = response.Results["CreditCardResponse"];
            var res = JsonConvert.DeserializeObject<List<CreditCardData>>(r.ToString());
        }

        public static void retrieveBinRequest()
        {
            CrmServiceHelper h = new CrmServiceHelper();
            ConfigurationBL configurationBL = new ConfigurationBL(h.IOrganizationService);
            var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
            var splittedInfo = iyzicoInfo.Split(';');

            Options options = new Options();
            options.ApiKey = splittedInfo[1];
            options.SecretKey = splittedInfo[2];
            options.BaseUrl = splittedInfo[0];

            RetrieveBinNumberRequest retrieveBinNumberRequest = new RetrieveBinNumberRequest
            {
                BinNumber = "589004",
                ConversationId = "6668456276"
            };
            var retrieved = BinNumber.Retrieve(retrieveBinNumberRequest, options);
        }
        public static void createCreditCardandSendIyzico()
        {
            var creditCardParameters = new CreateCreditCardParameters
            {
                cardAlias = "my alias",
                cardHolderName = "POLAT AYDIN",
                email = "polat.aydin@creatifsoftware.com",
                creditCardNumber = "375629994130063",
                expireMonth = 04,
                expireYear = 2019,
                individualCustomerId = "7F824836-97F6-E811-A94F-000D3A454330",
                langId = 1055
            };

            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            //CreditCardBL creditCardBL = new CreditCardBL(crmServiceHelper.IOrganizationService);
            //var response1 = creditCardBL.createCreditCard("7F824836-97F6-E811-A94F-000D3A454330", 1033, creditCardParameters);

            OrganizationRequest request = new OrganizationRequest("rnt_CreateCustomerCreditCard");

            request["creditCardParameters"] = JsonConvert.SerializeObject(creditCardParameters);
            var d = DateTime.Now;
            var response = crmServiceHelper.IOrganizationService.Execute(request);
            var d1 = DateTime.Now;
            var total = (d1 - d).TotalSeconds;
            var deserializedResponse = JsonConvert.DeserializeObject<CreateCreditCardResponse>(Convert.ToString(response.Results["ExecutionResult"]));
        }

        public static void IyzicoRetrievepaymentSample()
        {
            CrmServiceHelper h = new CrmServiceHelper();
            ConfigurationBL configurationBL = new ConfigurationBL(h.IOrganizationService);
            var apikey = configurationBL.GetConfigurationByName("iyzico_apiKey");
            var secretKey = configurationBL.GetConfigurationByName("iyzico_secretKey");
            var baseUrl = configurationBL.GetConfigurationByName("iyzico_baseUrl");

            Options options = new Options();
            options.ApiKey = apikey;
            options.SecretKey = secretKey;
            options.BaseUrl = baseUrl;

            RetrievePaymentRequest request = new RetrievePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = "7235358039";
            //request.PaymentConversationId = "7235358039";
            request.PaymentId = "11229565";
            Payment payment = Payment.Retrieve(request, options);
        }
        public static void IyzicoRefundSample()
        {
            CrmServiceHelper h = new CrmServiceHelper();
            ConfigurationBL configurationBL = new ConfigurationBL(h.IOrganizationService);
            var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
            var splittedInfo = iyzicoInfo.Split(';');

            Options options = new Options();
            options.ApiKey = splittedInfo[1];
            options.SecretKey = splittedInfo[2];
            options.BaseUrl = splittedInfo[0];

            var r = createRefundRequest();
            var res = Refund.Create(r, options);
        }
        public static void IyzicoCreatePaymentSample()
        {
            CrmServiceHelper h = new CrmServiceHelper();
            ConfigurationBL configurationBL = new ConfigurationBL(h.IOrganizationService);

            var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
            var splittedInfo = iyzicoInfo.Split(';');

            Options options = new Options();
            options.ApiKey = splittedInfo[1];
            options.SecretKey = splittedInfo[2];
            options.BaseUrl = splittedInfo[0];

            var request = createPaymentRequest();

            Payment payment = Payment.Create(request, options);
        }
        public static void createCard()
        {
            CrmServiceHelper h = new CrmServiceHelper();
            ConfigurationBL configurationBL = new ConfigurationBL(h.IOrganizationService);
            var apikey = configurationBL.GetConfigurationByName("iyzico_apiKey");
            var secretKey = configurationBL.GetConfigurationByName("iyzico_secretKey");
            var baseUrl = configurationBL.GetConfigurationByName("iyzico_baseUrl");

            Options options = new Options();
            options.ApiKey = apikey;
            options.SecretKey = secretKey;
            options.BaseUrl = baseUrl;

            //CreateCardRequest request = new CreateCardRequest();
            //request.Locale = Locale.TR.ToString();
            //request.ConversationId = "123456789";
            //request.Email = "polattt@gmail.com";
            //request.ExternalId = "ext230294053";
            ////request.CardUserKey = "439758934758973458734";

            //CardInformation cardInformation = new CardInformation();
            //cardInformation.CardAlias = "card alias";
            //cardInformation.CardHolderName = "John Doe";
            //cardInformation.CardNumber = "5528790000000008";
            //cardInformation.ExpireMonth = "12";
            //cardInformation.ExpireYear = "2030";
            //request.Card = cardInformation;

            //Card card = Card.Create(request, options);


            var request = createPaymentRequest();

            Payment payment = Payment.Create(request, options);

            //var request = createRefundRequest();
            //Refund refund = Refund.Create(request, options);


        }

        public static void createCardCRM()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            //first create the card in draft status
            Entity e = new Entity("rnt_customercreditcard");

            e["rnt_contactid"] = new EntityReference("contact", new Guid("cfcda658-b15c-ec11-8f8f-000d3a2cc829"));
            e["rnt_creditcardnumber"] = "123456*****67";


            e["rnt_expireyear"] = new OptionSetValue(2025);
            e["rnt_expiremonthcode"] = new OptionSetValue(3);
            e["rnt_name"] = "test";
            e["rnt_binnumber"] = "123456";
            e["rnt_bank"] = "HSBC";
            e["rnt_cardtypecode"] = new OptionSetValue(20);
            e["rnt_cardorganizationtype"] = new OptionSetValue(2);
            e["rnt_carduserkey"] = "123123dsdewd231";
            e["rnt_cardtoken"] = "123123dsdewd231";
            e["rnt_provider"] = new OptionSetValue((int)GlobalEnums.PaymentProvider.iyzico);
            crmServiceHelper.IOrganizationService.Create(e);

            CreditCardBL CreditCardBL = new CreditCardBL(crmServiceHelper.IOrganizationService);
            CreditCardBL.checkFraudCustomerControl(new Guid("cfcda658-b15c-ec11-8f8f-000d3a2cc829"), 1);

        }
        public static CreateRefundRequest createRefundRequest()
        {
            CreateRefundRequest request = new CreateRefundRequest();
            request.ConversationId = "123456789";
            request.Locale = Locale.TR.ToString();
            request.PaymentTransactionId = "11886711";
            request.Price = "4";
            //request.Ip = "92.44.83.900";
            request.Currency = Currency.TRY.ToString();
            return request;
        }


        public static CreatePaymentRequest createPaymentRequest()
        {
            CreatePaymentRequest request = new CreatePaymentRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = StaticHelper.RandomDigits(10);
            request.Price = "6.5";
            request.PaidPrice = "6.5";
            request.Currency = Currency.TRY.ToString();
            request.Installment = 1;
            request.BasketId = "B67832";
            request.PaymentChannel = PaymentChannel.WEB.ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();


            PaymentCard paymentCard = new PaymentCard();
            //paymentCard.CardUserKey = "oPXALBu4I9tT8ti2dBUBZqHF9eo=";
            //paymentCard.CardToken = "N3fva04+kkiW98lNO59cmf5xXSM=";
            paymentCard.CardHolderName = "John Doe";
            paymentCard.CardNumber = "5890040000000016"; // test card - akbank
            paymentCard.ExpireMonth = "12";
            paymentCard.ExpireYear = "2030";
            paymentCard.Cvc = "123";
            paymentCard.RegisterCard = 1;
            request.PaymentCard = paymentCard;

            Buyer buyer = new Buyer();
            buyer.Id = "BY789";
            buyer.Name = "John";
            buyer.Surname = "Doe";
            buyer.GsmNumber = "+905350000000";
            buyer.Email = "email@email.com";
            buyer.IdentityNumber = "74300864791";
            //buyer.LastLoginDate = "2015-10-05 12:43:35";
            //buyer.RegistrationDate = "2013-04-21 15:12:09";
            buyer.RegistrationAddress = "TEST";
            buyer.Ip = "92.44.83.900";
            buyer.City = "Istanbul";
            buyer.Country = "Turkey";
            //buyer.ZipCode = "34732";
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = "Ozan Murat";
            shippingAddress.City = "Istanbul";
            shippingAddress.Country = "Turkey";
            shippingAddress.Description = "TEST";
            shippingAddress.ZipCode = "34742";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = "Ozan Murat";
            billingAddress.City = "Istanbul";
            billingAddress.Country = "Turkey";
            billingAddress.Description = "TEST";
            billingAddress.ZipCode = "34742";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem firstBasketItem = new BasketItem();

            firstBasketItem.Id = "BI101";
            firstBasketItem.Name = "Binocular";
            firstBasketItem.Category1 = "Collectibles";
            firstBasketItem.Category2 = "Accessories";
            firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            firstBasketItem.Price = "2";

            basketItems.Add(firstBasketItem);

            BasketItem secondBasketItem = new BasketItem();
            secondBasketItem.Id = "BI102";
            secondBasketItem.Name = "Binocular";
            secondBasketItem.Category1 = "Collectibles";
            secondBasketItem.Category2 = "Accessories";
            secondBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            secondBasketItem.Price = "4.5";
            basketItems.Add(secondBasketItem);
            request.BasketItems = new List<BasketItem>();

            request.BasketItems.AddRange(basketItems);
            return request;
        }
        public static void CreatePriceCalculation()
        {
            CrmServiceHelper h = new CrmServiceHelper();
            var priceListParameter = "{'rnt_name' : 'Ozan Test Bireysel', 'rnt_pricetype':'1','rnt_begindate':'12.23.2018','rnt_enddate':'12.24.2018'}";
            var priceList = JsonConvert.DeserializeObject<PriceListRelationData>(priceListParameter);
            var groupCodeParameter = "[{'rnt_groupcodeinformationsId':'19ced4bd-7cb3-e811-9410-000c293d97f8','rnt_name':'AD','rnt_price':'200'},{'rnt_groupcodeinformationsId':'c2aaa3d5-7cb3-e811-9410-000c293d97f8','rnt_name':'A','rnt_price':'100'},{'rnt_groupcodeinformationsId':'666132f4-7cb3-e811-9410-000c293d97f8','rnt_name':'K','rnt_price':'03'},{'rnt_groupcodeinformationsId':'2827937d-7eb3-e811-9410-000c293d97f8','rnt_name':'B','rnt_price':'04'},{'rnt_groupcodeinformationsId':'f2eeff92-7eb3-e811-9410-000c293d97f8','rnt_name':'F','rnt_price':'05'},{'rnt_groupcodeinformationsId':'0a256ea9-7eb3-e811-9410-000c293d97f8','rnt_name':'C','rnt_price':'0'},{'rnt_groupcodeinformationsId':'38dc8dee-7eb3-e811-9410-000c293d97f8','rnt_name':'H','rnt_price':'0'},{'rnt_groupcodeinformationsId':'19e64d33-7fb3-e811-9410-000c293d97f8','rnt_name':'D','rnt_price':'0'},{'rnt_groupcodeinformationsId':'dbf1844d-7fb3-e811-9410-000c293d97f8','rnt_name':'OD','rnt_price':'0'},{'rnt_groupcodeinformationsId':'ef9671cc-82b3-e811-9410-000c293d97f8','rnt_name':'G','rnt_price':'0'},{'rnt_groupcodeinformationsId':'d6c8c90c-83b3-e811-9410-000c293d97f8','rnt_name':'P','rnt_price':'0'},{'rnt_groupcodeinformationsId':'090a6b37-83b3-e811-9410-000c293d97f8','rnt_name':'N','rnt_price':'0'},{'rnt_groupcodeinformationsId':'893e1252-83b3-e811-9410-000c293d97f8','rnt_name':'M','rnt_price':'0'}]";
            var groupCode = JsonConvert.DeserializeObject<List<GroupCodeRelationData>>(groupCodeParameter);
            var groupCodeListPriceParameter = "[{'rnt_groupcodepricelisttemplateId':'f02542d7-5a04-e911-a952-000d3a454977','rnt_minimumday':'1','rnt_maximumday':'2','rnt_ratio':'10'},{'rnt_groupcodepricelisttemplateId':'b9ef3fdd-5a04-e911-a952-000d3a454977','rnt_minimumday':'3','rnt_maximumday':'6','rnt_ratio':'034'},{'rnt_groupcodepricelisttemplateId':'91697adf-5a04-e911-a950-000d3a454330','rnt_minimumday':'7','rnt_maximumday':'13','rnt_ratio':'056'},{'rnt_groupcodepricelisttemplateId':'ef7c1de0-5a04-e911-a951-000d3a454f67','rnt_minimumday':'14','rnt_maximumday':'29','rnt_ratio':'078'}]";
            var groupCodeListPrice = JsonConvert.DeserializeObject<List<GroupCodeListPriceRelationData>>(groupCodeListPriceParameter);
            var availabilityPriceListParameter = "[{'rnt_availabilitypricelisttemplateId':'f079ba02-1605-e911-a951-000d3a454f67','rnt_maximumavailability':10,'rnt_minimumavailability':0,'rnt_ratio':'0123'},{'rnt_availabilitypricelisttemplateId':'7e0e0507-1605-e911-a953-000d3a454e11','rnt_maximumavailability':20,'rnt_minimumavailability':11,'rnt_ratio':'0123'},{'rnt_availabilitypricelisttemplateId':'65428009-1605-e911-a950-000d3a454330','rnt_maximumavailability':30,'rnt_minimumavailability':21,'rnt_ratio':'0123'},{'rnt_availabilitypricelisttemplateId':'81deb308-1605-e911-a951-000d3a454f67','rnt_maximumavailability':40,'rnt_minimumavailability':31,'rnt_ratio':'0123'},{'rnt_availabilitypricelisttemplateId':'66428009-1605-e911-a950-000d3a454330','rnt_maximumavailability':50,'rnt_minimumavailability':41,'rnt_ratio':'0'},{'rnt_availabilitypricelisttemplateId':'6b428009-1605-e911-a950-000d3a454330','rnt_maximumavailability':60,'rnt_minimumavailability':51,'rnt_ratio':'0'},{'rnt_availabilitypricelisttemplateId':'6f428009-1605-e911-a950-000d3a454330','rnt_maximumavailability':70,'rnt_minimumavailability':61,'rnt_ratio':'0'},{'rnt_availabilitypricelisttemplateId':'08262209-1605-e911-a952-000d3a454977','rnt_maximumavailability':80,'rnt_minimumavailability':71,'rnt_ratio':'0'},{'rnt_availabilitypricelisttemplateId':'87deb308-1605-e911-a951-000d3a454f67','rnt_maximumavailability':90,'rnt_minimumavailability':81,'rnt_ratio':'0'},{'rnt_availabilitypricelisttemplateId':'7c428009-1605-e911-a950-000d3a454330','rnt_maximumavailability':100,'rnt_minimumavailability':91,'rnt_ratio':'0'}]";
            var availabilityPriceList = JsonConvert.DeserializeObject<List<AvailabilityPriceListRelationData>>(availabilityPriceListParameter);

            PriceCalculationBL priceCalculationBL = new PriceCalculationBL(h.IOrganizationService);
            priceCalculationBL.executeCreatePriceCalculation(priceListParameter, groupCodeParameter, groupCodeListPriceParameter, availabilityPriceListParameter);
        }
        public class login
        {
            public string Email { get; set; }
            public string PassWord { get; set; }

        }
        public class myCustomClass
        {
            public string prop1 { get; set; }
            public string prop2 { get; set; }

        }

        public class Attribute
        {
            public string Label { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public object Options { get; set; }
            public string RefEntity { get; set; }
        }

        public class TableMetadata
        {
            public string Label { get; set; }
            public string Name { get; set; }
            public List<Attribute> Attributes { get; set; }
        }

        public class InvSalesGroupId
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public string LogicalName { get; set; }
        }

        public class InvBrandId
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public string LogicalName { get; set; }
        }

        public class InvMainGroupId
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public string LogicalName { get; set; }
        }

        public class InvSubGroupId
        {
            public string Id { get; set; }
            public string Text { get; set; }
            public string LogicalName { get; set; }
        }

        public class Product
        {
            public string ProductId { get; set; }
            public string ProductNumber { get; set; }
            public string Name { get; set; }
            public string inv_BillDefinition { get; set; }
            public InvSalesGroupId inv_SalesGroupId { get; set; }
            public InvBrandId inv_BrandId { get; set; }
            public InvMainGroupId inv_MainGroupId { get; set; }
            public InvSubGroupId inv_SubGroupId { get; set; }
        }

        public class RootObject
        {
            public bool Success { get; set; }
            public string ErrorMsg { get; set; }
            public TableMetadata TableMetadata { get; set; }
            public List<Product> Products { get; set; }
        }

        public class TransactionHistory
        {

            public int documentNumber { get; set; }
            public DateTime beginingDate { get; set; }
            public DateTime endDate { get; set; }
            public DateTime processDate { get; set; }
            public decimal documentAmonunt { get; set; }
            public decimal paymentAmount { get; set; }
            public decimal deposit { get; set; }
            public Guid id { get; set; }
            public string name { get; set; }
        }
        public static void MultiSelectMapping()
        {

            CrmServiceHelper h = new CrmServiceHelper();

            QueryExpression querytest = new QueryExpression("rnt_campaign");
            querytest.ColumnSet = new ColumnSet("rnt_branchcode");
            querytest.Criteria.AddCondition("rnt_campaignid", ConditionOperator.Equal, "042F001B-1AFE-E811-A952-000D3A454E11");
            var campaign = h.IOrganizationService.RetrieveMultiple(querytest).Entities.FirstOrDefault();
            var value = campaign.GetAttributeValue<OptionSetValueCollection>("rnt_branchcode");
            List<object> test = new List<object>();
            foreach (var item in value)
            {
                test.Add(item.Value);
            }
            var a = test.ToArray();
            QueryExpression query = new QueryExpression("rnt_multiselectmapping");
            query.ColumnSet = new ColumnSet("rnt_branch");
            query.Criteria.AddCondition("rnt_optionvalue", ConditionOperator.In, test.ToArray());
            var coll = h.IOrganizationService.RetrieveMultiple(query);

            //QueryExpression query = new QueryExpression("rnt_branch");
            //query.ColumnSet = new ColumnSet("rnt_name");
            //var collection = h.IOrganizationService.RetrieveMultiple(query);
            //var count = 1;
            //foreach (var item in collection.Entities)
            //{
            //    Entity entity = new Entity("rnt_multiselectmapping");
            //    entity.Attributes["rnt_branch"] = new EntityReference(item.LogicalName, item.Id);
            //    entity.Attributes["rnt_optionvalue"] = count;
            //    h.IOrganizationService.Create(entity);
            //    count += 1;
            //}

        }

        public static void SetGlobalOptionSet()
        {
            CrmServiceHelper h = new CrmServiceHelper();

            QueryExpression query = new QueryExpression("rnt_groupcodeinformations");
            query.ColumnSet = new ColumnSet("rnt_name");
            var collection = h.IOrganizationService.RetrieveMultiple(query);
            var count = 1;

            foreach (var item in collection.Entities)
            {
                InsertOptionValueRequest insertOptionValueRequest =
                    new InsertOptionValueRequest
                    {
                        OptionSetName = "rnt_groupcode",
                        Label = new Label(item.GetAttributeValue<string>("rnt_name"), 1033),
                        Value = count
                    };
                var response = (InsertOptionValueResponse)h.IOrganizationService.Execute(insertOptionValueRequest);
                count += 1;
            }
        }
        public static void checkFineAmount()
        {
            CrmServiceHelper s = new CrmServiceHelper();
            ReservationBL reservationBL = new ReservationBL(s.IOrganizationService);
            var validationResponse = reservationBL.checkBeforeReservationCancellation(new RntCar.ClassLibrary.ReservationCancellationParameters
            {
                pnrNumber = "1V8A9D",
                reservationId = new Guid("23ad3fc6-7500-e911-a951-000d3a454f67")
            },
            1033);

            if (!validationResponse.ResponseResult.Result)
            {
                //initializer.PluginContext.OutputParameters["ReservationFineAmountResponse"] = JsonConvert.SerializeObject(validationResponse);
                return;
            }
            //now check the fine amount for the reservation.
            // if it is pay later no need to 
            //if (validationResponse.reservationPaymetType == (int)PaymentEnums.PaymentType.PayNow &&
            //   validationResponse.willChargeFromUser)
            //{
            //    var fineResponse = reservationBL.calculateFineAmountForGivenReservation(new Guid("23ad3fc6-7500-e911-a951-000d3a454f67"), 1033);
            //    //if first day price couldnt found into system no need to continue
            //    if (!fineResponse.ResponseResult.Result)
            //    {
            //        validationResponse.ResponseResult = fineResponse.ResponseResult;
            //        //initializer.PluginContext.OutputParameters["ReservationFineAmountResponse"] = JsonConvert.SerializeObject(validationResponse);
            //        return;
            //    }
            //    validationResponse.fineAmount = fineResponse.firstDayAmount;
            //}
        }
        public static void GetAdditionalProductsForUpdate()
        {
            var customerParameter = "{'firstname':'Darth Vader','lastname':'','individualCustomerId':'f3561c25-1690-e911-a825-000d3a47cb1d','distributionChannelCode':20,'corporateCustomerId':'909be88b-2927-ea11-a810-000d3a4ab4c2','pricingType':'20'}";
            var groupCodeParameter = "{'rnt_name':'M','rnt_groupcodeinformationsid':'090a6b37-83b3-e811-9410-000c293d97f8','rnt_minimumage':32,'rnt_minimumdriverlicence':4,'rnt_segment':90,'rnt_youngdriverage':25,'rnt_youngdriverlicence':2}";
            var dateandbranchParameter = "{'pickupBranchId':'b38d5d16-17b1-e811-9410-000c293d97f8','dropoffBranchId':'b38d5d16-17b1-e811-9410-000c293d97f8','pickupDate':'2020-02-19T01:45:00.000Z','dropoffDate':'2020-02-23T01:45:00.000Z'}";
            var customerData = JsonConvert.DeserializeObject<IndividualCustomerDetailData>(customerParameter);
            var groupCodeData = JsonConvert.DeserializeObject<GroupCodeInformationCRMData>(groupCodeParameter);
            var dateandbranchData = JsonConvert.DeserializeObject<ReservationDateandBranchParameters>(dateandbranchParameter);

            CrmServiceHelper h = new CrmServiceHelper();
            AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(h.IOrganizationService);
            var response = additionalProductsBL.GetAdditionalProductForUpdate(groupCodeData, customerData, dateandbranchData, "17d965aa-9f52-ea11-a812-000d3a2e9b6c", 1033);
        }
        public static void updateIsTurkishCitizen()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.CrmServiceClient);
            var guid = configurationBL.GetConfigurationByName("TurkeyGuid");

            //QueryExpression queryExpression = new QueryExpression("contact");
            //queryExpression.Criteria.AddCondition("rnt_citizenshipid", ConditionOperator.Equal, guid.Split(';')[0]);
            //queryExpression.Criteria.AddCondition("rnt_isturkishcitizen", ConditionOperator.NotEqual, true);
            //var exp = crmServiceHelper.CrmServiceClient.RetrieveMultiple(queryExpression);

            QueryExpression queryExpression = new QueryExpression("contact");
            queryExpression.Criteria.AddCondition("rnt_citizenshipid", ConditionOperator.NotEqual, guid.Split(';')[0]);
            queryExpression.Criteria.AddCondition("rnt_isturkishcitizen", ConditionOperator.Equal, true);
            var exp = crmServiceHelper.CrmServiceClient.RetrieveMultiple(queryExpression);
            foreach (var item in exp.Entities)
            {
                Entity e = new Entity("contact");
                e.Id = item.Id;
                e["rnt_isturkishcitizen"] = false;
                crmServiceHelper.CrmServiceClient.Update(e);
            }
        }
        public static void ExportSolution()
        {
            CrmServiceHelper h = new CrmServiceHelper();
            ExportSolutionRequest exportSolutionRequest = new ExportSolutionRequest();
            //here i am exporting solution as unmanaged , if you want to export as managed means put true.
            exportSolutionRequest.Managed = false;
            //give solution unique name
            exportSolutionRequest.SolutionName = "RentaCarPlugins";

            ExportSolutionResponse exportSolutionResponse = (ExportSolutionResponse)h.IOrganizationService.Execute(exportSolutionRequest);

            byte[] exportXml = exportSolutionResponse.ExportSolutionFile;
            string filename = "RentaCarPlugins.zip";
            //give path where the solkution file need to store
            System.IO.File.WriteAllBytes(filename, exportXml);
        }
        public static void testAvailibility(IOrganizationService service)
        {
            var a = "'{'pickupDateTime':'2018-11-09T12:06:56.450Z','dropoffDateTime':1541851617615,'pickupBranchId':'AF8D5D16-17B1-E811-9410-000C293D97F8','dropOffBranchId':'AF8D5D16-17B1-E811-9410-000C293D97F8'}'";
            OrganizationRequest request = new OrganizationRequest();
            request.RequestName = "rnt_CalculateAvailibility";
            request["AvailibilityParameters"] = a;
            service.Execute(request);
        }
        public static void sendEquipmentsToMongoDB()
        {
            CrmServiceHelper helper = new CrmServiceHelper();
            QueryExpression exp = new QueryExpression("rnt_equipment");
            exp.ColumnSet = new ColumnSet(true);
            exp.Criteria.AddCondition("rnt_platenumber", ConditionOperator.Equal, "61 JD 366");
            var result = helper.IOrganizationService.RetrieveMultiple(exp);


            foreach (var item in result.Entities)
            {
                item["rnt_mongodbintegrationtrigger"] = StaticHelper.RandomDigits(10);
                helper.IOrganizationService.Update(item);
            }

        }

        public static void Tahsilat()
        {
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
            var netTahsilatInfo = configurationBL.GetConfigurationByName("nettahsilatconfiguration");

            var configs = NetTahsilatHelper.setConfigurationValues(netTahsilatInfo);
            //setting auth info

            PaymentService.CustomDynamicData[] customDynamicData = new PaymentService.CustomDynamicData[2];
            customDynamicData[0] = new PaymentService.CustomDynamicData { FormCode = "PlateNumberInfo", IntegrationId = "PlateNumber", Value = "34ABC24" };
            customDynamicData[1] = new PaymentService.CustomDynamicData { FormCode = "PlateNumberInfo", IntegrationId = "PNrNumber", Value = "35EFG23" };

            PaymentService.PaymentWebServiceClient PayServ = new PaymentService.PaymentWebServiceClient();

            var list = PayServ.GetPaymentSetList(new PaymentService.AuthenticationInfo()
            {
                UserName = configs.nettahsilat_username,      /**  sitede açılmış kullanıcı adı  **/
                Password = configs.nettahsilat_password,              /**                 kullanıcı şifre  **/
            });

            var virtualPosts = list.PaymentSets.FirstOrDefault().VirtualPosList;
            var virtualPosID = virtualPosts.FirstOrDefault().VPosId;

            var salesResponse = PayServ.ProcessPayment(    /**  Ödeme işlemini nethasilat üzerinden gerçekleşme kontrolünü yapmak için kullanılan methoddur.  **/
             new PaymentService.AuthenticationInfo()
             {
                 UserName = configs.nettahsilat_username,      /**  sitede açılmış kullanıcı adı  **/
                 Password = configs.nettahsilat_password,              /**                 kullanıcı şifre  **/
             },
             new PaymentService.ProcessSaleParameters    /**  Ödeme işlemi için gerekli Kredi Kartı ve ödeme detay bilgileri yer almaktadır. **/
             {
                 CreditCard = new PaymentService.CreditCard1()
                 {
                     //MerchantSafeKey = "5c121540-2dba-47d1-a1a3-76255c2a8d54", /**  Kayıtlı kart kullanılacaksa; karta ait kaydedilen TokenGuid bilgisi atanır.  **/

                     CardNumber = "5571135571135575", /**  Kayıtlı kart kullanılacaksa;bu alanlar BOŞ gönderilir. **/
                     CardHolderName = "TEST TEST",
                     ExpYear = 2018,
                     ExpMonth = 12,
                     CvcNumber = "000",
                     SaveCreditCard = true
                 },
                 UseSafeKey = false, /**  Kayıtlı karta ait TokenGuid kullanılsın mı?  **/
                 CustomData = customDynamicData,
                 TransactionType = PaymentService.TransactionType1.Sale,
                 Amount = 130,
                 Installment = 1,
                 ClientReferenceCode = "STS000000000424",
                 VirtualPosId = 3160,   /**  Ödemenin gerçekleşeceği sanap pos'un sistemde kayıtlı olduğu id bilgisini gösterir **/
                 Use3d = false,      /**  Seçili sanal pos'un 3D özelliği zorunlu ise bu değer false olarak atanmış olsa bile 3D işlem için banka yönlendirmesi yapılır. **/
                 ReturnUrl = "http://localhost:62225/Return.aspx", /** İşlemin 3D veya normal olarak gerçekleşmesi veya reddedilmesi sonrası ilerlenecek sayfa bilgisini gösterir. **/
                 VendorId = Convert.ToInt16(configs.nettahsilat_vendorid),  /** Ödeme yapacak bayinin Id değeri **/
             });

            CrmServiceHelper h = new CrmServiceHelper();

            //first credit card
            Entity creditCard = new Entity("rnt_customercreditcard");
            creditCard["rnt_creditcardnumber"] = "5571135571135575";
            creditCard["rnt_binnumber"] = "557113";
            creditCard["rnt_expireyear"] = new OptionSetValue(2018);
            creditCard["rnt_expiremonthcode"] = new OptionSetValue(12);
            var cardTypeVal = 0;
            switch (salesResponse.CardBin.CardType)
            {
                case "Master":
                    cardTypeVal = 10;
                    break;
                case "Visa":
                    cardTypeVal = 100000001;
                    break;
                case "Amex":
                    cardTypeVal = 30;
                    break;
                case "Other":
                    cardTypeVal = 40;
                    break;
                default:
                    break;
            }
            var cardProgram = 0;
            switch (salesResponse.CardBin.CardProgram)
            {
                case "Axxes":
                    cardProgram = 1;
                    break;
                case "Bonus":
                    cardProgram = 2;
                    break;

                default:
                    break;
            }
            var cardOrganization = 0;
            switch (salesResponse.CardBin.CardOrgranizationType)
            {
                case "Credit":
                    cardOrganization = 1;
                    break;
                case "Debit":
                    cardOrganization = 2;
                    break;
                case "Business  ":
                    cardOrganization = 3;
                    break;
                default:
                    break;
            }
            creditCard["rnt_cardtypecode"] = new OptionSetValue(cardTypeVal);
            creditCard["rnt_cardprogramcode"] = new OptionSetValue(cardProgram);
            creditCard["rnt_cardorganizationtype"] = new OptionSetValue(cardOrganization);
            creditCard["rnt_bank"] = salesResponse.CardBin.Group;
            creditCard["rnt_isbusinesscard"] = salesResponse.CardBin.IsBusinessCard;
            creditCard["rnt_contactid"] = new EntityReference("contact", new Guid("32A593EB-E6C0-E811-9412-000C293D97F8"));

            if (salesResponse.TokenResult.IsSuccess)
            {
                creditCard["rnt_merchantsafekey"] = salesResponse.TokenResult.SaveCreditCardDetail.FirstOrDefault().MerchantSafeKey;
            }
            var a = h.IOrganizationService.Create(creditCard);
            QueryExpression e1 = new QueryExpression("rnt_customercreditcard");
            e1.ColumnSet = new ColumnSet(new String[] { "rnt_merchantsafekey" });
            e1.Criteria.AddCondition(new ConditionExpression("rnt_merchantsafekey", ConditionOperator.Equal, "5c121540-2dba-47d1-a1a3-76255c2a8d54"));
            var queryRes = h.IOrganizationService.RetrieveMultiple(e1);

            Entity e = new Entity("rnt_payment");
            e["rnt_reservationid"] = new EntityReference("rnt_reservation", new Guid("B0690619-09BE-E811-9412-000C293D97F8"));
            e["rnt_contactid"] = new EntityReference("contact", new Guid("32A593EB-E6C0-E811-9412-000C293D97F8"));
            // e["rnt_customercreditcardid"] = new EntityReference("rnt_customercreditcard", queryRes.Entities.FirstOrDefault().Id);
            e["rnt_customercreditcardid"] = new EntityReference("rnt_customercreditcard", a);

            e["rnt_installment"] = 1;
            e["rnt_amount"] = new Money(130);
            e["rnt_bankerrorcode"] = Convert.ToString(salesResponse.BankErrorCode);
            e["rnt_bankmessage"] = salesResponse.BankMessage;
            e["rnt_internalmessage"] = salesResponse.InternalMessage;
            e["rnt_transactionid"] = Convert.ToString(salesResponse.TransactionId);
            e["rnt_transactionresult"] = salesResponse.IsSuccess ? new OptionSetValue(1) : new OptionSetValue(2);
            e["rnt_transactiontypecode"] = new OptionSetValue(10);
            e["rnt_transactionstatusid"] = Convert.ToString(salesResponse.TransactionStatusId);
            e["rnt_authcode"] = Convert.ToString(salesResponse.AuthCode);
            e["rnt_clientreferencecode"] = salesResponse.ClientReferenceCode;
            e["rnt_referencecode"] = salesResponse.ReferenceCode;

            h.IOrganizationService.Create(e);


        }

        public static void MernisCheck()
        {
            //CitizenshipService.KPSPublicSoapClient citizenShipClient= new CitizenshipService.KPSPublicSoapClient();
            //var resp = citizenShipClient.TCKimlikNoDogrula(37645491122, "POLAT", "AYDIN", 1986);
        }

        public static void CreateReservationItem(IOrganizationService service)
        {
            Entity e = new Entity("rnt_reservationitem");
            e["rnt_totalamount"] = new Money(700);
            e["rnt_taxratio"] = 18M;
            e["rnt_groupcodeinformationsid"] = new EntityReference("rnt_groupcodeinformations", new Guid("0A256EA9-7EB3-E811-9410-000C293D97F8")); //C
            e["statuscode"] = new OptionSetValue(100000001);
            e["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("81F0651F-00A0-E811-940C-000C293D97F8"));
            e["rnt_dropoffbranchid"] = new EntityReference("rnt_branch", new Guid("04F8934D-9AAF-E811-9410-000C293D97F8"));//atatürk
            e["rnt_pickupbranchid"] = new EntityReference("rnt_branch", new Guid("04F8934D-9AAF-E811-9410-000C293D97F8"));//atatürk

            e["rnt_pickupdatetime"] = TimeRoundUp(DateTime.Now).AddDays(-1);
            e["rnt_dropoffdatetime"] = TimeRoundUp(Convert.ToDateTime(e["rnt_pickupdatetime"]).AddDays(1));
            e["rnt_equipmentid"] = new EntityReference("rnt_equipment", new Guid("57652710-97B3-E811-9410-000C293D97F8"));
            e["rnt_reservationid"] = new EntityReference("rnt_reservation", new Guid("B0690619-09BE-E811-9412-000C293D97F8"));
            e["rnt_mongodbintegrationtrigger"] = "huhu1243578";
            service.Create(e);

            //e.Id = new Guid("C64E701D-1CBE-E811-9412-000C293D97F8");
            //service.Update(e);
        }

        public static DateTime TimeRoundUp(DateTime input)

        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute <= 15 ? 15 : 30, 0);
        }


        public DateTime ChangeDateTimeForGivenTimeZone(DateTime time, TimeZoneInfo zone)
        {

            return TimeZoneInfo.ConvertTime(time, zone);
        }
        public static void transferConfiguration()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();
            QueryExpression e = new QueryExpression("rnt_configuration");
            e.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transfersystemparameter()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();
            QueryExpression e = new QueryExpression("rnt_systemparameter");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferinventories()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();



            QueryExpression e = new QueryExpression("rnt_inventory");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transfercountries()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();



            QueryExpression e = new QueryExpression("rnt_country");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transfercities()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();



            QueryExpression e = new QueryExpression("rnt_city");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }

        public static void transferdistrict()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();



            QueryExpression e = new QueryExpression("rnt_district");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferInventory()
        {
            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_inventory");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferDamageSize()
        {
            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_demagesize");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }

        public static void transferCarParts()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();



            QueryExpression e = new QueryExpression("rnt_carpart");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferTaxOffices()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();



            QueryExpression e = new QueryExpression("rnt_taxoffice");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferBrand()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();



            QueryExpression e = new QueryExpression("rnt_brand");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferModel()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();



            QueryExpression e = new QueryExpression("rnt_model");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferEngineVolume()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();



            QueryExpression e = new QueryExpression("rnt_enginevolume");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferHorsePower()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();



            QueryExpression e = new QueryExpression("rnt_horsepower");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferGroupCodeInfo()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_groupcodeinformations");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                item["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("8B8574FD-8B08-EB11-A812-000D3A49E095"));
                destService.Create(item);
            }
        }
        public static void transferBranch()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_branch");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferWorkingHour()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_workinghour");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferKmLimits()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_kilometerlimit");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                try
                {
                    destService.Create(item);
                }
                catch (Exception ex)
                {
                    continue;
                }

            }
        }
        public static void transferdamagetypes()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_damagetype");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                try
                {
                    destService.Create(item);
                }
                catch (Exception ex)
                {
                    continue;
                }

            }
        }
        public static void transfetdamagesize()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_demagesize");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                try
                {
                    destService.Create(item);
                }
                catch (Exception ex)
                {
                    continue;
                }

            }
        }
        public static void transfetdamageprice()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_damageprice");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                try
                {
                    destService.Create(item);
                }
                catch (Exception ex)
                {
                    continue;
                }

            }
        }
        public static void transfercarpart()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_carpart");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                try
                {
                    destService.Create(item);
                }
                catch (Exception ex)
                {
                    continue;
                }

            }
        }
        public static void transferdamagedocument()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_damagedocument");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                try
                {
                    destService.Create(item);
                }
                catch (Exception ex)
                {
                    continue;
                }

            }
        }
        public static void transferProducts()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_product");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferEquipments()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_equipment");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                item.Attributes.Remove("rnt_equipmentinventoryid");
                destService.Create(item);
            }
        }
        public static void transferCrmConfiguration()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_configuration");
            e.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferAdditionalProducts()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_additionalproduct");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                item["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("3D7A8CFD-6EF3-E811-A950-000D3A4546AF"));
                destService.Create(item);
            }
        }
        public static void transferAdditionalProductRules()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_additionalproductrules");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                destService.Create(item);
            }
        }
        public static void transferGroupCodeListPrices()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_listprice");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                item["rnt_pricelistid"] = new EntityReference("rnt_pricelist", new Guid("94C804EC-72F5-E811-A94F-000D3A454E11"));
                item["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("3D7A8CFD-6EF3-E811-A950-000D3A4546AF"));
                destService.Create(item);
            }
        }
        public static void transferavailabilitypricelists()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_availabilitypricelist");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                item["rnt_pricelistid"] = new EntityReference("rnt_pricelist", new Guid("94C804EC-72F5-E811-A94F-000D3A454E11"));
                item.Attributes.Remove("ownerid");
                destService.Create(item);
            }
        }
        public static void transferronewayfee()
        {

            var sourceservice = CreateSourceService();
            var destService = CreateDestinationService();

            QueryExpression e = new QueryExpression("rnt_onewayfee");
            e.ColumnSet = new ColumnSet(true);
            var a = sourceservice.RetrieveMultiple(e);
            foreach (var item in a.Entities)
            {
                item.Attributes.Remove("ownerid");
                item["transactioncurrencyid"] = new EntityReference("transactioncurrency", new Guid("3D7A8CFD-6EF3-E811-A950-000D3A4546AF"));
                destService.Create(item);
            }
        }

        public static void bonusTransactionCreate()
        {
            var s = new CrmServiceHelper().IOrganizationService;

            ActionHelper actionHelper = new ActionHelper(s);

            var birdId = "251834";
            decimal billAmount = decimal.Zero;

            NotifyCheckoutRequest notifyCheckoutParameters = new NotifyCheckoutRequest();

            notifyCheckoutParameters.birdId = long.Parse(birdId);
            notifyCheckoutParameters.merchantCode = "RentGo";
            notifyCheckoutParameters.storeCode = "rentgo";
            notifyCheckoutParameters.transactionId = "190a56ea-2e30-ed11-9db1-000d3a23986q";
            notifyCheckoutParameters.dateTime = DateTime.UtcNow.AddHours(3);

            AmountDetails campaignFreePayment = new AmountDetails(); // Kampanya kapsamlarına dahil olmayan, vergi gruplarına göre ayrılmış alışveriş tutarları.
            campaignFreePayment.amount = billAmount;
            campaignFreePayment.percent = 18;

            notifyCheckoutParameters.campaignFreePaymentDetails = new AmountDetails[]
            {
                  campaignFreePayment
            };

            AmountDetails subTotal = new AmountDetails();
            subTotal.amount = billAmount;
            subTotal.percent = 18;

            notifyCheckoutParameters.subtotalDetails = new AmountDetails[] // İndirimden önceki ürün tutarı
            {
                  subTotal
            };

            AmountDetails payment = new AmountDetails();
            payment.amount = billAmount;
            payment.percent = 18;

            notifyCheckoutParameters.paymentDetails = new AmountDetails[]  //Vergi gruplarına göre ayrılmış alışveriş tutarı.
            {
                  payment
            };

            UsedCampaignDetails campaign = new UsedCampaignDetails(); //Alışveriş kapsamında üyenin faydalandığı kampanyalar.

            AmountDetails campaignPayment = new AmountDetails();
            campaignPayment.amount = decimal.Zero;
            campaignPayment.percent = decimal.Zero;

            campaign.amountDetails = new AmountDetails[]
            {
                   campaignPayment
            };
            campaign.campaignCode = "RENTGO_10";

            ////Paracık kazanma durumu
            campaign.benefit = new BenefitDetails();

            //% İndirim kazanma durumu
            AmountDetails discount = new AmountDetails();
            discount.amount = decimal.Zero;
            discount.percent = decimal.Zero;
            //campaign.benefit.Items = new object[] { discount };

            notifyCheckoutParameters.usedCampaignDetails = new UsedCampaignDetails[]
            {
                  campaign
            };

            //TransactionInfo transactionData = new TransactionInfo();
            //transactionData.barcode = "XYZ";
            //transactionData.amount = 90;
            //transactionData.quantity = 1;
            //transactionData.campaign = new[] { "" };

            //notifyCheckoutParameters.transactionInfos = new TransactionInfo[] 
            //{
            //transactionData
            //};

            var notifyCheckoutResponse = actionHelper.HopiSales(notifyCheckoutParameters);


        }
        private static IOrganizationService CreateSourceService()
        {
            IOrganizationService _service11;
            ClientCredentials c = new ClientCredentials();

            c.UserName.UserName = @"admin@mnboto.onmicrosoft.com";
            c.UserName.Password = "Rentgo123!?";
            using (var _serviceproxy = new OrganizationServiceProxy(new Uri("https://rentuna.api.crm4.dynamics.com/XRMServices/2011/Organization.svc"),
                                                                null, c, null))
            {
                _service11 = (IOrganizationService)_serviceproxy;
                _serviceproxy.ServiceConfiguration.CurrentServiceEndpoint.Behaviors.Add(new ProxyTypesBehavior());

            }

            return _service11;
        }
        private static IOrganizationService CreateDestinationService()
        {
            IOrganizationService _service11;
            ClientCredentials c = new ClientCredentials();

            c.UserName.UserName = @"admin@filonova.onmicrosoft.com";
            c.UserName.Password = "Ps24062018!";
            using (var _serviceproxy = new OrganizationServiceProxy(new Uri("https://novacardev.api.crm4.dynamics.com/XRMServices/2011/Organization.svc"),
                                                                null, c, null))
            {
                _service11 = (IOrganizationService)_serviceproxy;
                _serviceproxy.ServiceConfiguration.CurrentServiceEndpoint.Behaviors.Add(new ProxyTypesBehavior());

            }

            return _service11;
        }

    }


    public class pa
    {
        public string customerId { get; set; }
    }
    public class Equipment
    {
        public string equipmentId { get; set; }
        public string equipmentName { get; set; }
        public string groupCodeInfoId { get; set; }
        public string groupCodeInfoName { get; set; }
        public string platenumber { get; set; }

    }
    public class availabilityResults
    {
        public string branchName { get; set; }
        public List<availability> availabilities { get; set; }
    }
    public class availability
    {
        public string groupCode { get; set; }
        public decimal ratio { get; set; }

    }

    public class url
    {
        public bool isTest { get; set; }
        public string productionURL { get; set; }
        public string testURL { get; set; }
    }
}
