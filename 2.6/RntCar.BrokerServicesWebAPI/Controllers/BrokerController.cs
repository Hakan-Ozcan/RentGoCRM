using Microsoft.Ajax.Utilities;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BrokerServicesWebAPI.Security;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Web;
using RntCar.Logger;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.MongoDBHelper.Repository;
using RntCar.RedisCacheHelper.CachedItems;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace RntCar.BrokerServicesWebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "X-My-Header")]
    [BasicHttpAuthorizeAttribute]
    public class BrokerController : ApiController
    {
        private string mongoDBHostName { get; set; }
        private string mongoDBDatabaseName { get; set; }
        private string webErrorMessagePath { get; set; }

        public BrokerController()
        {
            mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
            mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");
            webErrorMessagePath = new HandlerBase().webXmlPath;
         }

        [HttpGet]
        [Route("api/broker/testme")]
        public string testme()
        {
            return "ok";
        }

        /// <summary>
        /// Ana veri servisi
        /// </summary>
        /// <param name="getMasterDataRequest_Broker"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/broker/login")]
        public LoginResponse_Broker login([FromBody] LoginRequest_Broker LoginRequest_Broker)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            try
            {
                loggerHelper.traceInfo("login start");
                BusinessLibrary.Repository.CorporateCustomerRepository corporateCustomerRepository = new BusinessLibrary.Repository.CorporateCustomerRepository(crmServiceHelper.IOrganizationService);
                var customer = corporateCustomerRepository.getAgencyByUserNamePassword(LoginRequest_Broker.username, LoginRequest_Broker.password);
                if (customer == null)
                {
                    return new LoginResponse_Broker
                    {
                        responseResult = ClassLibrary._Broker.ResponseResult.ReturnError("Kullanıcı adı veya Şifre yanlış.")
                    };
                }
                var taxNumber = customer.GetAttributeValue<string>("rnt_taxnumber");
                double balance = 0;
                try
                {
                    LogoHelper logoHelper = new LogoHelper(crmServiceHelper.IOrganizationService);
                    balance = logoHelper.getAccountBalance(taxNumber);
                }
                catch
                {

                }


                return new LoginResponse_Broker
                {
                    agencyInformation = new AccountMapper().createBrokerAccountData(customer, balance),
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("login error : " + ex.Message);
                return new LoginResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        /// <summary>
        /// Ana veri servisi
        /// </summary>
        /// <param name="getMasterDataRequest_Broker"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/broker/getmasterdata")]
        public GetMasterDataResponse_Broker getMasterData([FromBody] GetMasterDataRequest_Broker getMasterDataRequest_Broker)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo("getMasterData start : " + JsonConvert.SerializeObject(getMasterDataRequest_Broker));
                #region Parameters
                List<Task> _tasks = new List<Task>();
                CrmServiceHelper_Broker crmServiceHelper = new CrmServiceHelper_Broker();
                List<GroupCodeInformation_Broker> _groupCodeInformation = new List<GroupCodeInformation_Broker>();
                List<CountryData> _countries = new List<CountryData>();
                List<CityData> _cities = new List<CityData>();
                List<DistrictData> _districts = new List<DistrictData>();
                List<TaxOfficeData> _taxOffice = new List<TaxOfficeData>();
                List<Branch_Broker> _branchs = new List<Branch_Broker>();
                List<WorkingHour_Broker> _workingHour = new List<WorkingHour_Broker>();
                #endregion

                #region district task
                var districtTask = new Task(() =>
                {
                    loggerHelper.traceInfo("district start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("district_cachekey");
                    //Check Redis Cache for group code information
                    DistrictCacheClient districtCacheClient = new DistrictCacheClient(crmServiceHelper.IOrganizationService,
                                                                                      cacheKey);
                    _districts = districtCacheClient.getDistricts();

                    loggerHelper.traceInfo("show room end");
                });
                _tasks.Add(districtTask);
                districtTask.Start();
                #endregion

                #region GroupCodes task
                var groupCodeInformationTask = new Task(() =>
                {
                    loggerHelper.traceInfo("groupCodeInformationTask start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("groupcode_cacheKey");
                    //Check Redis Cache for group code information
                    GroupCodeInformationCacheClient groupCodeInformationCacheClient = new GroupCodeInformationCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                                          cacheKey);
                    var groupCodeData = groupCodeInformationCacheClient.getAllGroupCodeInformationDetailCache(getMasterDataRequest_Broker.langId);

                    MongoDBHelper.Repository.CorporateCustomerRepository corporateCustomerRepository = new MongoDBHelper.Repository.CorporateCustomerRepository(mongoDBHostName, mongoDBDatabaseName);
                    var account = corporateCustomerRepository.getCorporateCustomerByBrokerCode(getMasterDataRequest_Broker.brokerCode);

                    MultiSelectMappingRepository multiSelectMappingRepository = new MultiSelectMappingRepository(crmServiceHelper.IOrganizationService);
                    var brokerMappings = multiSelectMappingRepository.getBrokerMappings(account.corporateCustomerId);
                    _groupCodeInformation = new GroupCodeInformationMapper().createBrokerGroupCodeList(groupCodeData, brokerMappings, getMasterDataRequest_Broker.langId);
                    loggerHelper.traceInfo("groupCodeInformationTask end");
                });
                _tasks.Add(groupCodeInformationTask);
                groupCodeInformationTask.Start();
                #endregion

                #region Countries Task
                var countriesTask = new Task(() =>
                {
                    loggerHelper.traceInfo("countriesTask start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("country_cacheKey");

                    CountryCacheClient countryCacheClient = new CountryCacheClient(crmServiceHelper.IOrganizationService,
                                                                                   cacheKey);
                    _countries = countryCacheClient.getCountryCache();
                    loggerHelper.traceInfo("countriesTask end");

                });
                _tasks.Add(countriesTask);
                countriesTask.Start();
                #endregion

                #region Cities Task
                var citiesTask = new Task(() =>
                {
                    loggerHelper.traceInfo("citiesTask start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("city_cachekey");

                    CityCacheClient cityCacheClient = new CityCacheClient(crmServiceHelper.IOrganizationService,
                                                                          cacheKey);
                    _cities = cityCacheClient.getCityCache();
                    loggerHelper.traceInfo("citiesTask end");

                });
                _tasks.Add(citiesTask);
                citiesTask.Start();
                #endregion

                #region Branch Task
                var branchsTask = new Task(() =>
                {
                    loggerHelper.traceInfo("branchsTask start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("branch_CacheKey");

                    BranchCacheClient branchCacheClient = new BranchCacheClient(crmServiceHelper.IOrganizationService, mongoDBHostName, mongoDBDatabaseName, cacheKey);
                    var branches = branchCacheClient.getBranchCache(getMasterDataRequest_Broker.brokerCode, 70, true);
                    _branchs = new BranchMapper().createBrokerBranchList(branches);
                    loggerHelper.traceInfo("branchsTask end");

                });
                _tasks.Add(branchsTask);
                branchsTask.Start();
                #endregion

                #region Working Hours Task
                var workingHoursTask = new Task(() =>
                {
                    loggerHelper.traceInfo("workingHoursTask start");

                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("workingHour_CacheKey");

                    WorkingHourCacheClient workingHourCacheClient = new WorkingHourCacheClient(crmServiceHelper.IOrganizationService, mongoDBHostName, mongoDBDatabaseName, cacheKey);
                    var workingHours = workingHourCacheClient.getWorkingHourCache(getMasterDataRequest_Broker.brokerCode, 70, true);
                    _workingHour = new WorkingHourMapper().createBrokerWorkingHourList(workingHours);
                    loggerHelper.traceInfo("workingHoursTask end");

                });
                _tasks.Add(workingHoursTask);
                workingHoursTask.Start();
                #endregion

                #region Tax Offices Task
                var taxOfficeTax = new Task(() =>
                {
                    loggerHelper.traceInfo("_taxOfficeTask star");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("taxoffice_cachekey");

                    TaxOfficeCacheClient taxOfficeCacheClient = new TaxOfficeCacheClient(crmServiceHelper.IOrganizationService,
                                                                                         cacheKey);
                    _taxOffice = taxOfficeCacheClient.getTaxOfficeCache();
                    loggerHelper.traceInfo("_taxOfficeTask end");

                });
                _tasks.Add(taxOfficeTax);
                taxOfficeTax.Start();
                #endregion

                Task.WaitAll(_tasks.ToArray());

                return new GetMasterDataResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnSuccess(),
                    branchs = _branchs,
                    cities = _cities,
                    countries = _countries,
                    groupCodeInformation = _groupCodeInformation,
                    workingHours = _workingHour,
                    taxOffices = _taxOffice,
                    districts = _districts
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getMasterData error : " + ex.Message);
                return new GetMasterDataResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        /// <summary>
        /// Kullanılabilirlik hesapla
        /// </summary>
        /// <remarks></remarks>
        /// <param name="availabilityParameters_Broker"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/broker/calculateavailability")]
        public AvailabilityResponse_Broker calculateAvailability([FromBody] AvailabilityParameters_Broker availabilityParameters_Broker)
        {
            var _now = DateTime.UtcNow;
            StringBuilder broker = new StringBuilder();
            //LoggerHelper loggerHelper = new LoggerHelper();
            broker.AppendLine("availabilityParameters_Broker : " + JsonConvert.SerializeObject(availabilityParameters_Broker));
            var random = StaticHelper.GenerateString(15);
            broker.AppendLine("starts : " + random + " " + DateTime.Now);
            try
            {
                var pickupBranchId = availabilityParameters_Broker.queryParameters.pickupBranchId;
                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                availabilityParameters_Broker.queryParameters = virtualBranchHelper.buildVirtualBrachParameters(availabilityParameters_Broker.queryParameters);

                broker.AppendLine("shift duration retrieve start : " + random + " " + DateTime.Now);
                #region Shift Duration - System Parameter              
                var shiftDuration = Convert.ToInt32(StaticHelper.GetConfiguration("ShiftDuration"));
                #endregion
                broker.AppendLine("shift duration retrieve start : " + random + " " + DateTime.Now);
                broker.AppendLine("corp customer retrieve start : " + random + " " + DateTime.Now);
                #region Corporate Customer
                MongoDBHelper.Repository.CorporateCustomerRepository corporateCustomerRepository = new MongoDBHelper.Repository.CorporateCustomerRepository(mongoDBHostName, mongoDBDatabaseName);
                var corporateCustomer = corporateCustomerRepository.getCorporateCustomerByBrokerCode(availabilityParameters_Broker.brokerCode);

                if (corporateCustomer == null)
                {
                    return new AvailabilityResponse_Broker
                    {
                        responseResult = ClassLibrary._Broker.ResponseResult.ReturnError("Bu broker koduna ait kayıt bulunamadı")
                    };
                }
                #endregion
                #region Price Code, Customer Id
                var corporateCustomerId = new Guid(corporateCustomer.corporateCustomerId);
                var priceCodeId = corporateCustomer.priceCodeId;
                var processIndividualPrices = !corporateCustomer.processIndividualPrices.HasValue ? false : corporateCustomer.processIndividualPrices.Value;

                #endregion
                broker.AppendLine("corp customer retrieve end : " + random + " " + DateTime.Now);

                broker.AppendLine("Calculate Prices from MongoDB start : " + random + " " + DateTime.Now);
                #region Calculate Prices from MongoDB
                var availabilityParam = new AvailabilityMapper().createAvailabilityParameter_Broker(availabilityParameters_Broker,
                                                                                                    (int)rnt_ReservationChannel.Web,
                                                                                                     corporateCustomer.accountTypeCode == (int)rnt_AccountTypeCode.Broker ?
                                                                                                     (int)GlobalEnums.CustomerType.Broker :
                                                                                                     (int)GlobalEnums.CustomerType.Agency,
                                                                                                     shiftDuration,
                                                                                                     priceCodeId,
                                                                                                     corporateCustomerId,
                                                                                                     processIndividualPrices);
                availabilityParam.corporateType = corporateCustomer.accountTypeCode;
                if (corporateCustomer.priceFactorGroupCode.HasValue && corporateCustomer.priceFactorGroupCode != 0)
                {
                    availabilityParam.accountGroup = corporateCustomer.priceFactorGroupCode.Value.ToString();
                }
                MongoDBHelper.Entities.CrmConfigurationBusiness crmConfigurationBusiness = new MongoDBHelper.Entities.CrmConfigurationBusiness(mongoDBHostName, mongoDBDatabaseName);
                var cacheKey = crmConfigurationBusiness.getCrmConfigurationByKey<string>("branch_CacheKey");

                BranchCacheClient branchCacheClient = new BranchCacheClient(mongoDBHostName, mongoDBDatabaseName, cacheKey);
                var branchs = branchCacheClient.getBranchCache(availabilityParameters_Broker.brokerCode, 70, true);
                broker.AppendLine("branches retrieved : " + random + " " + DateTime.Now);

                var pickUpBranch = branchs.Where(x => x.BranchId == pickupBranchId.ToString()).FirstOrDefault();
                if (pickUpBranch.earlistPickupTime.HasValue && pickUpBranch.earlistPickupTime != 0)
                {
                    availabilityParam.earlistPickupTime = pickUpBranch.earlistPickupTime;
                }

                MongoDBHelper.Entities.AvailabilityFactorBusiness availabilityFactorBusiness = new MongoDBHelper.Entities.AvailabilityFactorBusiness(mongoDBHostName, mongoDBDatabaseName);
                var r = availabilityFactorBusiness.checkAvailability(availabilityParam, availabilityParameters_Broker.langId, (int)rnt_ReservationChannel.Web, availabilityParam.customerType, availabilityParam.accountGroup);
                if (!r.ResponseResult.Result)
                {
                    return new AvailabilityResponse_Broker
                    {
                        responseResult = ClassLibrary._Broker.ResponseResult.ReturnError(r.ResponseResult.ExceptionDetail)
                    };
                }

                MongoDBHelper.Entities.AvailabilityBusiness availabilityBusiness = new MongoDBHelper.Entities.AvailabilityBusiness(availabilityParam);
                var calculateAvailability = availabilityBusiness.calculateAvailability();
                #endregion
                #region Remove not calculated groups

                var calculatedGroups = calculateAvailability.Where(p => p.isPriceCalculatedSafely == true && p.hasError == false).ToList();
                var response = new AvailabilityMapper().createBrokerAvailabilityList(calculatedGroups);

                var oneWayFee = decimal.Zero;
                decimal exchangeRate = 1.0M;
                if (calculateAvailability.Where(p => p.currencyId != null).FirstOrDefault() != null)
                {
                    MongoDBHelper.Repository.CurrencyRepository currencyRepository = new MongoDBHelper.Repository.CurrencyRepository(mongoDBHostName, mongoDBDatabaseName);
                    var c = currencyRepository.GetCurrency(new Guid(calculateAvailability.Where(p => p.currencyId != null).FirstOrDefault().currencyId));
                    if (c != null)
                    {
                        exchangeRate = c.exchangerate;
                    }
                }



                if (availabilityParameters_Broker.queryParameters.pickupBranchId != availabilityParameters_Broker.queryParameters.dropoffBranchId)
                {
                    RntCar.MongoDBHelper.Repository.OneWayFeeRepository repository = new MongoDBHelper.Repository.OneWayFeeRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                    var fee = repository.getOneWayFee(availabilityParameters_Broker.queryParameters.pickupBranchId.ToString(),
                                                      availabilityParameters_Broker.queryParameters.dropoffBranchId.ToString());

                    oneWayFee = fee != null ? fee.Price : decimal.Zero;

                }

                #endregion
                broker.AppendLine("Calculate Prices from MongoDB end : " + random + " " + DateTime.Now);
                #region Duration Calculation
                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var duration = durationHelper.calculateDocumentDurationByPriceHourEffect(availabilityParameters_Broker.queryParameters.pickupDateTime,
                                                                                         availabilityParameters_Broker.queryParameters.dropoffDateTime);
                #endregion
                broker.AppendLine("km limit start : " + random + " " + DateTime.Now);
                #region KmLimits
                List<GroupCodeInformationDetailData> groupCodeData = new List<GroupCodeInformationDetailData>();
                response.ForEach(p => groupCodeData.Add(new GroupCodeInformationDetailData { groupCodeInformationId = p.groupCodeId }));

                List<KmLimitData_Web> kmLimitData_Webs = new List<KmLimitData_Web>();
                foreach (var item in groupCodeData)
                {
                    MongoDBHelper.Entities.KilometerLimitBusiness kilometerLimitBusiness = new MongoDBHelper.Entities.KilometerLimitBusiness(this.mongoDBHostName, this.mongoDBDatabaseName);
                    var limit = kilometerLimitBusiness.getKilometerLimitForGivenDurationandGroupCode(duration, item.groupCodeInformationId);

                    KmLimitData_Web kmLimitData_Web = new KmLimitData_Web
                    {
                        groupCodeInformationId = item.groupCodeInformationId,
                        kmLimit = limit
                    };
                    kmLimitData_Webs.Add(kmLimitData_Web);
                }
                foreach (var item in response)
                {
                    var l = kmLimitData_Webs.Where(p => p.groupCodeInformationId == item.groupCodeId).FirstOrDefault();
                    if (l != null)
                    {
                        item.kmLimit = l.kmLimit;
                    }
                }
                var _d = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(availabilityParameters_Broker.queryParameters.pickupDateTime,
                                                                                                      availabilityParameters_Broker.queryParameters.dropoffDateTime);
                response.ForEach(p => decimal.Round(p.dailyAmount = p.payAmount / _d, 2));
                #endregion
                broker.AppendLine("availabilityBusiness.trackingNumber: " + availabilityBusiness.trackingNumber + " " + DateTime.Now);
                broker.AppendLine("km limit start end: " + random + " " + DateTime.Now);
                broker.AppendLine("random ends : " + random + " " + DateTime.Now);
                MongoDBHelper.Entities.BrokerAvailabilityLogBusiness business = new MongoDBHelper.Entities.BrokerAvailabilityLogBusiness(this.mongoDBHostName, this.mongoDBDatabaseName);
                var l1 = new BrokerAvailabilityLog
                {
                    content = broker.ToString(),
                    startTime = _now,
                    endTime = DateTime.UtcNow,
                    brokerCode = availabilityParameters_Broker.brokerCode,
                };

                l1.totalSeconds = Convert.ToDecimal((l1.endTime - l1.startTime).TotalSeconds);
                business.createBrokerAvailabilityLog(l1);
                return new AvailabilityResponse_Broker
                {
                    totalDuration = _d,
                    exchangeRate = exchangeRate,
                    oneWayFeeAmount = decimal.Round(oneWayFee * exchangeRate, 2),
                    availabilityData = response,
                    trackingNumber = availabilityBusiness.trackingNumber,
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {

                return new AvailabilityResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        /// <summary>
        /// Rezervasyon oluştur
        /// </summary>
        /// <remarks></remarks>
        /// <param name="reservationCreateParameters_Broker"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/broker/createreservation")]
        public ReservationCreateResponse_Broker createReservation([FromBody] ReservationCreateParameters_Broker reservationCreateParameters_Broker)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Broker crmServiceHelper = new CrmServiceHelper_Broker();
            try
            {
                loggerHelper.traceInputsInfo<ReservationCreateParameters_Broker>(reservationCreateParameters_Broker);

                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                reservationCreateParameters_Broker.reservationQueryParameters = virtualBranchHelper.buildVirtualBrachParameters(reservationCreateParameters_Broker.reservationQueryParameters);

                #region Corporate Customer
                MongoDBHelper.Repository.CorporateCustomerRepository corporateCustomerRepository = new MongoDBHelper.Repository.CorporateCustomerRepository(mongoDBHostName, mongoDBDatabaseName);
                var corporateCustomer = corporateCustomerRepository.getCorporateCustomerByBrokerCode(reservationCreateParameters_Broker.reservationCustomerParameters.brokerCode);
                var corporateCustomerId = new Guid(corporateCustomer.corporateCustomerId);
                #endregion

                var customerParameters = new IndividualCustomerMapper().buildReservationCustomerParameters(reservationCreateParameters_Broker.reservationCustomerParameters, corporateCustomerId, reservationCreateParameters_Broker.reservationPriceParameters);

                var reservationDateandBranchParameters = new ReservationDateandBranchParameters
                {
                    dropoffBranchId = reservationCreateParameters_Broker.reservationQueryParameters.dropoffBranchId,
                    dropoffDate = reservationCreateParameters_Broker.reservationQueryParameters.dropoffDateTime,
                    pickupBranchId = reservationCreateParameters_Broker.reservationQueryParameters.pickupBranchId,
                    pickupDate = reservationCreateParameters_Broker.reservationQueryParameters.pickupDateTime
                };

                if (reservationCreateParameters_Broker.reservationPriceParameters.paymentMethodCode == (int)rnt_PaymentMethodCode.LimitedCredit)
                {
                    if (reservationCreateParameters_Broker.reservationEquimentParameters.billingType == null)
                    {
                        return new ReservationCreateResponse_Broker
                        {
                            responseResult = ClassLibrary._Broker.ResponseResult.ReturnError("Araç ödeme tipi boş bırakılamaz.")
                        };
                    }

                    if (reservationCreateParameters_Broker.reservationAdditionalProducts.Any(a => a.billingType == null))
                    {
                        return new ReservationCreateResponse_Broker
                        {
                            responseResult = ClassLibrary._Broker.ResponseResult.ReturnError("Ek ürün ödeme tipi boş bırakılamaz.")
                        };
                    }
                }

                GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(crmServiceHelper.IOrganizationService);
                var groupCodeInformation = groupCodeInformationRepository.getGroupCodeInformationById(reservationCreateParameters_Broker.reservationEquimentParameters.groupCodeId);

                var reservationEquipmentParameters = new GroupCodeInformationMapper().buildReservationEquipmentParameter(groupCodeInformation, reservationCreateParameters_Broker.reservationPriceParameters, reservationCreateParameters_Broker.reservationEquimentParameters);

                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect(reservationCreateParameters_Broker.reservationQueryParameters.pickupDateTime,
                                                                                              reservationCreateParameters_Broker.reservationQueryParameters.dropoffDateTime);

                ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
                var currency = configurationRepository.GetConfigurationByKey("currency_TRY");
                var cacheKey = configurationRepository.GetConfigurationByKey("additionalProduct_cacheKey");
                var oneWayFeeCode = configurationRepository.GetConfigurationByKey("OneWayFeeCode");

                BusinessLibrary.Repository.PriceListRepository priceListRepository = new BusinessLibrary.Repository.PriceListRepository(crmServiceHelper.IOrganizationService);
                var list = priceListRepository.getPriceListByPriceCodeId(corporateCustomer.priceCodeId, reservationCreateParameters_Broker.reservationQueryParameters.pickupDateTime, new string[] { "transactioncurrencyid" });

                Guid? currencyId = list.GetAttributeValue<EntityReference>("transactioncurrencyid").Id;
                if (new Guid(currency) == currencyId)
                {
                    currencyId = null;
                }
                cacheKey += corporateCustomerId;

                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(crmServiceHelper.IOrganizationService, StaticHelper.prepareAdditionalProductCacheKey(totalDuration.ToString(), cacheKey));
                var activeAdditionalProducts = additionalProductCacheClient.getAdditionalProductsCache(totalDuration, reservationDateandBranchParameters.pickupBranchId, currencyId);

                List<AdditionalProductData> filteredAdditionalProducts = new List<AdditionalProductData>();
                if (reservationCreateParameters_Broker.reservationAdditionalProducts != null)
                {
                    reservationCreateParameters_Broker.reservationAdditionalProducts.ForEach(i =>
                    {
                        filteredAdditionalProducts.Add(activeAdditionalProducts.Where(p => p.productId == i.productId).FirstOrDefault());
                    });
                }


                var additionalProducts = new AdditionalProductMapper().buildReservationAdditionalProducts(filteredAdditionalProducts, reservationCreateParameters_Broker.reservationAdditionalProducts, totalDuration, reservationCreateParameters_Broker.reservationPriceParameters);
                //check one way fee
                PriceCalculationSummariesRepository priceCalculationSummariesRepository = new PriceCalculationSummariesRepository(this.mongoDBHostName, this.mongoDBDatabaseName);

                var prices = priceCalculationSummariesRepository.getPriceCalculationSummariesforDailyPrices(reservationCreateParameters_Broker.reservationEquimentParameters.groupCodeId.ToString(),
                                                                                                            reservationCreateParameters_Broker.reservationPriceParameters.trackingNumber,
                                                                                                            null);

                prices = priceCalculationSummariesRepository.checkDailyPricesForMainCampaign(prices,                                                                                  
                                                                                    reservationCreateParameters_Broker.reservationPriceParameters.trackingNumber,
                                                                                    reservationCreateParameters_Broker.reservationEquimentParameters.groupCodeId.ToString());

                var totalAmount = prices.Sum(p => p.payLaterAmount);
                var priceParameters = new PriceMapper().buildReservationPriceParameter(reservationCreateParameters_Broker.reservationPriceParameters,
                                                                                       totalAmount,
                                                                                       customerParameters.contactId);
              

                loggerHelper.traceInfo("prices retrieved " + JsonConvert.SerializeObject(priceParameters));

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_createreservation");
                organizationRequest["SelectedCustomer"] = JsonConvert.SerializeObject(customerParameters);
                organizationRequest["SelectedDateAndBranch"] = JsonConvert.SerializeObject(reservationDateandBranchParameters);
                organizationRequest["PriceParameters"] = JsonConvert.SerializeObject(priceParameters);
                organizationRequest["SelectedEquipment"] = JsonConvert.SerializeObject(reservationEquipmentParameters);
                organizationRequest["LangId"] = reservationCreateParameters_Broker.langId;
                organizationRequest["TrackingNumber"] = reservationCreateParameters_Broker.reservationPriceParameters.trackingNumber;
                organizationRequest["TotalDuration"] = totalDuration;
                organizationRequest["ReservationChannel"] = (int)rnt_ReservationChannel.Web;
                organizationRequest["ReservationTypeCode"] = (reservationCreateParameters_Broker.reservationPriceParameters.paymentMethodCode == (int)rnt_PaymentMethodCode.PayBroker)
                                                                ? (int)rnt_ReservationTypeCode.Broker
                                                                : (int)rnt_ReservationTypeCode.Acente;
                organizationRequest["SelectedAdditionalProducts"] = JsonConvert.SerializeObject(additionalProducts);
                organizationRequest["Currency"] = currency;

                var res = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var reservationResponse = new ReservationCreateResponse();
                if (res.Results.Contains("ReservationResponse"))
                {
                    reservationResponse = JsonConvert.DeserializeObject<ReservationCreateResponse>(Convert.ToString(res.Results["ReservationResponse"]));
                    loggerHelper.traceInfo(Convert.ToString(res.Results["ReservationResponse"]));
                }
                //bazı durumlarda action response vermiyor--> sebebinisu an için bilmiyoruz
                else
                {
                    loggerHelper.traceInfo("stupid action reponse is somehow null lets save the date");
                    BusinessLibrary.Repository.ReservationItemRepository reservationItemRepository = new BusinessLibrary.Repository.ReservationItemRepository(crmServiceHelper.IOrganizationService);
                    var item = reservationItemRepository.getReservationEquipmentItemByTrackingNumber(reservationCreateParameters_Broker.reservationPriceParameters.trackingNumber);

                    if (item == null)
                    {
                        throw new Exception("Sistemsel bir hata oluştu");
                    }

                    BusinessLibrary.Repository.ReservationRepository reservationRepository = new ReservationRepository(crmServiceHelper.IOrganizationService);
                    var reservation = reservationRepository.getReservationById(item.GetAttributeValue<EntityReference>("rnt_reservationid").Id);

                    reservationResponse = new ReservationCreateResponse
                    {
                        pnrNumber = reservation.GetAttributeValue<string>("rnt_pnrnumber"),
                        reservationId = reservation.Id
                    };
                    loggerHelper.traceInfo(JsonConvert.SerializeObject(reservationResponse));
                }

                var createResponse_Broker = new ReservationCreateResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnSuccess()
                };
                createResponse_Broker.Map(reservationResponse);
                loggerHelper.traceInfo("all operations finished in peace");

                return createResponse_Broker;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("createReservation error : " + ex.Message);
                return new ReservationCreateResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        /// <summary>
        /// Rezervasyon iptali
        /// </summary>
        /// <param name="cancelReservationParameters_Broker"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/broker/cancelreservation")]
        public CancelReservationResponse_Broker cancelReservation([FromBody] CancelReservationParameters_Broker cancelReservationParameters_Broker)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Broker crmServiceHelper = new CrmServiceHelper_Broker();
            try
            {

                loggerHelper.traceInfo(JsonConvert.SerializeObject(cancelReservationParameters_Broker));
                BusinessLibrary.Repository.ReservationRepository reservationRepository = new BusinessLibrary.Repository.ReservationRepository(crmServiceHelper.IOrganizationService);
                var reservation = reservationRepository.getReservationByPnrNumber(cancelReservationParameters_Broker.pnrNumber);

                var cancelReservationParameters = new CancelReservationMapper().buildCancelReservationParameters(cancelReservationParameters_Broker);
                loggerHelper.traceInfo("reservationId " + reservation.Id);

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CancelReservation");
                organizationRequest["langId"] = cancelReservationParameters.langId;
                organizationRequest["reservationId"] = Convert.ToString(reservation.Id);
                organizationRequest["pnrNumber"] = Convert.ToString(cancelReservationParameters.pnrNumber);
                organizationRequest["cancellationReason"] = (int)rnt_reservation_StatusCode.CancelledByCustomer;
                organizationRequest["cancellationDescription"] = "Müşteri tarafından iptal - Broker Servis tarafından";
                organizationRequest["cancellationSubReason"] = 100000008;
                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var reservationCancellationResponse = JsonConvert.DeserializeObject<ReservationCancellationResponse>(Convert.ToString(response.Results["ReservationCancellationResponse"]));

                if (reservationCancellationResponse.ResponseResult.Result)
                {
                    return new CancelReservationResponse_Broker
                    {
                        responseResult = ClassLibrary._Broker.ResponseResult.ReturnSuccess()
                    };
                }

                return new CancelReservationResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnError(reservationCancellationResponse.ResponseResult.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("cancelReservation error : " + ex.Message);
                return new CancelReservationResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        /// <summary>
        /// Ek ürün listesi
        /// </summary>
        /// <param name="cancelReservationParameters_Broker"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/broker/getadditionalproducts")]
        public AdditionalProductResponse_Broker getAdditionalProducts([FromBody] AdditionalProductParameters_Broker additionalProductParameters_Broker)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            loggerHelper.traceInfo(JsonConvert.SerializeObject(additionalProductParameters_Broker));

            try
            {
                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                additionalProductParameters_Broker.queryParameters = virtualBranchHelper.buildVirtualBrachParameters(additionalProductParameters_Broker.queryParameters);

                CrmServiceHelper_Broker crmServiceHelper = new CrmServiceHelper_Broker();
                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var cacheKey = configurationBL.GetConfigurationByName("additionalProduct_cacheKey");

                var dummyCustomerId = StaticHelper.GetConfiguration("DummyCustomerId");

                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect(additionalProductParameters_Broker.queryParameters.pickupDateTime,
                                                                                              additionalProductParameters_Broker.queryParameters.dropoffDateTime);

                Guid? currencyId = null;
                if (!string.IsNullOrEmpty(additionalProductParameters_Broker.brokerCode))
                {

                    MongoDBHelper.Repository.CorporateCustomerRepository corporateCustomerRepository = new MongoDBHelper.Repository.CorporateCustomerRepository(mongoDBHostName, mongoDBDatabaseName);
                    var corporateCustomer = corporateCustomerRepository.getCorporateCustomerByBrokerCode(additionalProductParameters_Broker.brokerCode);

                    BusinessLibrary.Repository.PriceListRepository priceListRepository = new BusinessLibrary.Repository.PriceListRepository(crmServiceHelper.IOrganizationService);
                    var list = priceListRepository.getPriceListByPriceCodeId(corporateCustomer.priceCodeId, additionalProductParameters_Broker.queryParameters.pickupDateTime, new string[] { "transactioncurrencyid" });

                    currencyId = list.GetAttributeValue<EntityReference>("transactioncurrencyid").Id;
                }


                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                             StaticHelper.prepareAdditionalProductCacheKey(totalDuration.ToString(), cacheKey + "_" + additionalProductParameters_Broker.brokerCode));
                var products = additionalProductCacheClient.getAdditionalProductsCache(totalDuration, additionalProductParameters_Broker.queryParameters.pickupBranchId, currencyId);

                AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(crmServiceHelper.IOrganizationService);
                var dateandBranchParameters = new AdditionalProductMapper().buildAdditionalProductDateandBranchNeccessaryParameters(additionalProductParameters_Broker);

                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var individualCustomer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(new Guid(dummyCustomerId), new string[] { "birthdate", "rnt_drivinglicensedate" });

                var individualParameter = new AdditionalProductMapper().buildAdditionalProductIndividualNeccessaryParameters(individualCustomer.GetAttributeValue<DateTime>("birthdate"),
                                                                                                                             individualCustomer.GetAttributeValue<DateTime>("rnt_drivinglicensedate"));

                GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(crmServiceHelper.IOrganizationService);
                var groupCodeEntity = groupCodeInformationRepository.getGroupCodeInformationById(additionalProductParameters_Broker.groupCodeId.Value);

                var groupCodeInformationParameter = new AdditionalProductMapper().buildAdditionalProductGroupCodeInformationNeccessaryParameters(groupCodeEntity.GetAttributeValue<int>("rnt_youngdriverage"),
                                                                                                                                                 groupCodeEntity.GetAttributeValue<int>("rnt_minimumage"),
                                                                                                                                                 groupCodeEntity.GetAttributeValue<int>("rnt_youngdriverlicence"),
                                                                                                                                                 groupCodeEntity.GetAttributeValue<int>("rnt_minimumdriverlicence"));

                var response = additionalProductsBL.additionalProductMandatoryOperations(products,
                                                                                         groupCodeInformationParameter,
                                                                                         individualParameter,
                                                                                         dateandBranchParameters,
                                                                                         totalDuration);

                if (!response.ResponseResult.Result)
                {
                    return new AdditionalProductResponse_Broker
                    {
                        responseResult = ClassLibrary._Broker.ResponseResult.ReturnError(response.ResponseResult.ExceptionDetail)
                    };
                }
                var r = new AdditionalProductMapper().buildAdditionalProductsforBroker(response.AdditionalProducts);
                r.ForEach(p => p.dailyAmount = p.actualAmount);
                r.ForEach(p =>
                {
                    if (p.priceCalculationType == (int)rnt_PriceCalculationTypeCode.DependedonDuration)
                    {

                        p.actualAmount = p.actualAmount * totalDuration;
                    }
                });

                return new AdditionalProductResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnSuccess(),
                    additionalProducts = r
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getAdditionalProducts error : " + ex.Message);
                return new AdditionalProductResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/broker/getReservations")]
        public GetCustomerReservationsResponse_Broker getReservations([FromBody] GetReservationsRequest_Broker getReservationsRequest_Broker)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var reservations = reservationItemRepository.getReservationsByCorporateId(getReservationsRequest_Broker.accountId);
                loggerHelper.traceInfo(JsonConvert.SerializeObject(getReservationsRequest_Broker));

                var reservationDatas = new List<ClassLibrary.MongoDB.ReservationItemData>();
                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                foreach (var item in reservations)
                {
                    ClassLibrary.MongoDB.ReservationItemData r = new ClassLibrary.MongoDB.ReservationItemData();
                    r.Map(item);
                    reservationDatas.Add(r);
                }

                var data = new ReservationMapper().buildBrokerReservationData(reservationDatas, getReservationsRequest_Broker.langId);

                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);

                data.ForEach(p =>
                {
                    p.totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(p.PickupTime, p.DropoffTime);
                });
                data = data.OrderByDescending(p => p.PickupTime).ToList();
                return new GetCustomerReservationsResponse_Broker
                {
                    reservations = data,
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCustomerReservations error : " + ex.Message);
                return new GetCustomerReservationsResponse_Broker
                {
                    responseResult = ClassLibrary._Broker.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
    }
}