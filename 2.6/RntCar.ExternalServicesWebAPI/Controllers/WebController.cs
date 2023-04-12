using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Web;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.Reservation;
using RntCar.ExternalServices.Security;
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
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using CampaignRepository = RntCar.MongoDBHelper.Repository.CampaignRepository;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "X-My-Header")]
    [BasicHttpAuthorizeAttribute("Web")]
    public class WebController : ApiController
    {
        private string mongoDBHostName { get; set; }
        private string mongoDBDatabaseName { get; set; }
        private string webErrorMessagePath { get; set; }
        private string couponCodeXmlPath { get; set; }

        public WebController()
        {
            mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
            mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");
            webErrorMessagePath = new HandlerBase().webXmlPath;
            couponCodeXmlPath = new HandlerBase().couponCodeXmlPath;
        }
        [HttpGet]
        [Route("api/web/testme")]
        public string testme()
        {
            return "ok";
        }

        [HttpPost]
        [Route("api/web/getmasterdata")]
        public GetMasterDataResponse_Web getMasterData(GetMasterDataRequest_Web getMasterDataRequest_Web)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo("getMasterData start :" + JsonConvert.SerializeObject(getMasterDataRequest_Web));
                #region Parameters
                List<Task> _tasks = new List<Task>();
                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
                List<GroupCodeInformation_Web> _groupCodeInformation = new List<GroupCodeInformation_Web>();
                List<ShowRoomProduct_Web> _showroomProduct = new List<ShowRoomProduct_Web>();
                List<CountryData> _countries = new List<CountryData>();
                List<CityData> _cities = new List<CityData>();
                List<DistrictData> _districts = new List<DistrictData>();
                List<TaxOfficeData> _taxOffice = new List<TaxOfficeData>();
                List<Branch_Web> _branchs = new List<Branch_Web>();
                List<WorkingHour_Web> _workingHour = new List<WorkingHour_Web>();
                List<AdditionalProductRule_Web> _additionalProductRules = new List<AdditionalProductRule_Web>();
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

                #region showRoomTask task
                var showRoomTask = new Task(() =>
                {
                    loggerHelper.traceInfo("show room start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("groupcode_cacheKey");
                    //Check Redis Cache for group code information
                    ShowRoomProductCacheClient showRoomProductCacheClient = new ShowRoomProductCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                           cacheKey + "_showRoom" + getMasterDataRequest_Web.langId);
                    var showRoomData = showRoomProductCacheClient.getAllShowRoomDetailCache(getMasterDataRequest_Web.langId);
                    _showroomProduct = new ShowRoomProductMapper().createWebShowRoomProductList(showRoomData);
                    loggerHelper.traceInfo("show room end");
                });
                _tasks.Add(showRoomTask);
                showRoomTask.Start();
                #endregion

                #region GroupCodes task
                var groupCodeInformationTask = new Task(() =>
                {
                    loggerHelper.traceInfo("groupCodeInformationTask start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("groupcode_cacheKey");
                    var segmentCacheKey = configurationBL.GetConfigurationByName("segment_cacheKey");
                    //Check Redis Cache for group code information
                    GroupCodeInformationCacheClient groupCodeInformationCacheClient = new GroupCodeInformationCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                                          cacheKey);
                    var groupCodeData = groupCodeInformationCacheClient.getAllGroupCodeInformationDetailCache(getMasterDataRequest_Web.langId);
                    var segmentNameData = groupCodeInformationCacheClient.getSegmentNameCache(segmentCacheKey, getMasterDataRequest_Web.langId);
                    _groupCodeInformation = new GroupCodeInformationMapper().createWebGroupCodeList(groupCodeData, segmentNameData, getMasterDataRequest_Web.langId);
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
                    var branchs = branchCacheClient.getBranchCache("web", (int)rnt_ReservationChannel.Web, true);
                    _branchs = new BranchMapper().createWebBranchList(branchs);
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
                    var workingHours = workingHourCacheClient.getWorkingHourCache("web", (int)rnt_ReservationChannel.Web, true);
                    _workingHour = new WorkingHourMapper().createWebWorkingHourList(workingHours);
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

                #region AdditionalProductRule Task
                var additionalProductRule = new Task(() =>
                {
                    loggerHelper.traceInfo("additionalProductRule start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("additionalProductRules_cacheKey");

                    AdditionalProductRuleCacheClient additionalProductRuleCacheClient = new AdditionalProductRuleCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                                 cacheKey);
                    _additionalProductRules = additionalProductRuleCacheClient.getAdditionalProductRuleCache();
                    loggerHelper.traceInfo("additionalProductRule end");

                });
                _tasks.Add(additionalProductRule);
                additionalProductRule.Start();
                #endregion

                Task.WaitAll(_tasks.ToArray());

                return new GetMasterDataResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess(),
                    branchs = _branchs.OrderBy(p => p.branchName).ToList(),
                    cities = _cities,
                    countries = _countries,
                    groupCodeInformation = _groupCodeInformation,
                    workingHours = _workingHour,
                    taxOffices = _taxOffice,
                    districts = _districts,
                    additionalProductRules = _additionalProductRules,
                    showRoomProducts = _showroomProduct
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getMasterData error : " + ex.Message);
                return new GetMasterDataResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/login")]
        public LoginResponse_Web login([FromBody] LoginParameters_Web loginParameters_Web)
        {
            string emailAddress = Convert.ToString(loginParameters_Web.emailaddress);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                emailAddress = emailAddress.Replace("@", "");
                emailAddress = emailAddress.Replace(".com", "");
                loggerHelper = new LoggerHelper(emailAddress);
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            loggerHelper.traceInputsInfo<LoginParameters_Web>(loginParameters_Web);
            try
            {
                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();

                #region Individual Customer Retrieve
                loggerHelper.traceInfo("individual customer retrieve start");
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var individualCustomer = individualCustomerRepository.getWebUserInformation(loginParameters_Web.emailaddress, loginParameters_Web.password);
                loggerHelper.traceInfo("individual customer retrieve end");

                if (individualCustomer == null)
                {
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    return new LoginResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError(xrmHelper.GetXmlTagContentByGivenLangId("MissingUser", loginParameters_Web.langId, webErrorMessagePath))
                    };
                }
                #endregion

                List<Task> _tasks = new List<Task>();
                var _marketingPermissions = new MarketingPermission_Web();
                var _individualAddress = new List<IndividualAddressData_Web>();
                var _invoiceAddress = new List<InvoiceAddressData_Web>();
                var _customerCreditCards = new List<CreditCardData_Web>();
                var _customerAccounts = new List<CustomerAccountData_Web>();

                #region Account Information
                var accountInformationTask = new Task(() =>
                  {
                      loggerHelper.traceInfo("Account information retrieve start");
                      ConnectionRepository connectionRepository = new ConnectionRepository(crmServiceHelper.IOrganizationService);
                      var connections = connectionRepository.getConnectionsByIndividualCustomerId(individualCustomer.Id);

                      if (connections != null)
                      {
                          BusinessLibrary.Repository.CorporateCustomerRepository corporateCustomerRepository = new BusinessLibrary.Repository.CorporateCustomerRepository(crmServiceHelper.IOrganizationService);
                          connections.ForEach(conn =>
                          {
                              var relation = Enum.GetName(typeof(ClassLibrary._Enums_1033.rnt_connection_rnt_relationcode), conn.GetAttributeValue<OptionSetValue>("rnt_relationcode").Value);
                              var corporateCustomerId = conn.GetAttributeValue<EntityReference>("rnt_accountid").Id;
                              var columns = new string[]
                              {
                                "name",
                                "accountid",
                                "rnt_accounttypecode",
                                "rnt_adressdetail",
                                "rnt_cityid",
                                "rnt_countryid",
                                "rnt_districtid",
                                "rnt_paymentmethodcode",
                                "rnt_pricecodeid",
                                "rnt_taxnumber",
                                "rnt_taxoffice",
                                "telephone1",
                                "emailaddress1"
                              };
                              var account = corporateCustomerRepository.getCorporateCustomerById(corporateCustomerId, columns);
                              var accountDatas = new AccountMapper().createWebAccountData(account, relation);
                              _customerAccounts.Add(accountDatas);
                          });
                      }
                      loggerHelper.traceInfo("Account information retrieve end");
                  });

                _tasks.Add(accountInformationTask);
                accountInformationTask.Start();
                #endregion

                #region Marketing Permission Task
                var marketingPermissionTask = new Task(() =>
                {
                    loggerHelper.traceInfo("markettingPermissionsRepository retrieve start");
                    MarkettingPermissionsRepository markettingPermissionsRepository = new MarkettingPermissionsRepository(crmServiceHelper.IOrganizationService);
                    var marketingPermissions = markettingPermissionsRepository.getMarkettingPermissionByContactId(individualCustomer.Id);
                    if (marketingPermissions != null)
                        _marketingPermissions = new MarketingPermissionMapper().createWebMarketingPermissionData(marketingPermissions);
                    loggerHelper.traceInfo("markettingPermissionsRepository retrieve end");

                });
                _tasks.Add(marketingPermissionTask);
                marketingPermissionTask.Start();
                #endregion

                #region IndividualAdress Task
                var individualAdressTask = new Task(() =>
                {
                    loggerHelper.traceInfo("_individualAddress retrieve start");
                    IndividualAddressRepository individualAddressRepository = new IndividualAddressRepository(crmServiceHelper.IOrganizationService);
                    var individualAddress = individualAddressRepository.getIndividualAddressesByCustomerId(individualCustomer.Id);
                    _individualAddress = new IndividualAddressMapper().createWebIndividualAddressList(individualAddress);
                    loggerHelper.traceInfo("_individualAddress retrieve end");

                });
                _tasks.Add(individualAdressTask);
                individualAdressTask.Start();
                #endregion

                #region Invoice Address
                var invoiceAddressTask = new Task(() =>
                {
                    loggerHelper.traceInfo("invoiceAddressTask retrieve start");
                    InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(crmServiceHelper.IOrganizationService);
                    var columns = new string[]
                    {
                        "rnt_invoicetypecode",
                        "rnt_government",
                        "rnt_firstname",
                        "rnt_lastname",
                        "rnt_taxofficeid",
                        "rnt_taxnumber",
                        "rnt_districtid",
                        "rnt_countryid",
                        "rnt_companyname",
                        "rnt_cityid",
                        "rnt_addressdetail",
                        "rnt_name"
                    };
                    var invoiceAddress = invoiceAddressRepository.getInvoiceAddressByCustomerIdByGivenColumns(individualCustomer.Id, columns);
                    _invoiceAddress = new InvoiceAddressMapper().createWebInvoiceAddressList(invoiceAddress);
                    loggerHelper.traceInfo("invoiceAddressTask retrieve end");

                });
                _tasks.Add(invoiceAddressTask);
                invoiceAddressTask.Start();
                #endregion

                #region Customer Credit Card

                var customerCreditCard = new Task(() =>
                {
                    loggerHelper.traceInfo("customerCreditCard retrieve start");
                    CreditCardRepository creditCardRepository = new CreditCardRepository(crmServiceHelper.IOrganizationService);
                    var columns = new string[]
                    {
                        "rnt_name",
                        "rnt_carduserkey",
                        "rnt_cardtoken"
                    };
                    var creditCards = creditCardRepository.getCreditCardsByCustomerId(individualCustomer.Id, columns);
                    _customerCreditCards = new CreditCardMapper().createWebCreditCardList(creditCards);
                    loggerHelper.traceInfo("customerCreditCard retrieve end");

                });
                _tasks.Add(customerCreditCard);
                customerCreditCard.Start();

                #endregion

                Task.WaitAll(_tasks.ToArray());

                LoginResponse_Web loginResponse_Web = new LoginResponse_Web();
                loginResponse_Web.responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess();
                loginResponse_Web.customerInformation = new IndividualCustomerMapper().createWebIndividualCustomerData(individualCustomer, loginParameters_Web.langId);
                loginResponse_Web.marketingPermission = _marketingPermissions;
                loginResponse_Web.individualAddressInformation = _individualAddress;
                loginResponse_Web.invoiceAddressInformation = _invoiceAddress;
                loginResponse_Web.customerCreditCards = _customerCreditCards;
                loginResponse_Web.customerAccounts = _customerAccounts;
                return loginResponse_Web;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("login error : " + ex.Message);
                return new LoginResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/updatemarketingpermission")]
        public MarketingPermissionResponse_Web marketingPermission([FromBody] MarketingPermissionParameters_Web marketingPermissionParamaters)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            loggerHelper.traceInputsInfo<MarketingPermissionParameters_Web>(marketingPermissionParamaters);

            try
            {
                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_UpdateMarketingPermission");
                organizationRequest["contactId"] = Convert.ToString(marketingPermissionParamaters.contactId);
                organizationRequest["emailPermission"] = marketingPermissionParamaters.emailPermission;
                organizationRequest["notificationPermission"] = marketingPermissionParamaters.notificationPermission;
                organizationRequest["smsPermission"] = marketingPermissionParamaters.smsPermission;
                organizationRequest["channelCode"] = (int)rnt_PermissionChannelCode.Web;
                organizationRequest["operationType"] = (int)rnt_marketingpermissions_rnt_operationtype.MyAccountUpdate; //(int)rnt_OperationType.CreatingReservation;

                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                var parsedResponse = JsonConvert.DeserializeObject<MarketingPermissionResponse_Web>(Convert.ToString(response.Results["UpdatingMarketingPermissionResponse"]));

                return new MarketingPermissionResponse_Web()
                {
                    marketingPermissionId = parsedResponse.marketingPermissionId,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new MarketingPermissionResponse_Web()
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/web/getindividualcustomerdetail")]
        public LoginResponse_Web getIndividualCustomerDetail([FromBody] LoginParameters_Web loginParameters_Web)
        {
            string emailAddress = Convert.ToString(loginParameters_Web.emailaddress);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                emailAddress = emailAddress.Replace("@", "");
                emailAddress = emailAddress.Replace(".com", "");
                loggerHelper = new LoggerHelper(emailAddress);
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            loggerHelper.traceInputsInfo<LoginParameters_Web>(loginParameters_Web);
            try
            {
                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
                #region Individual Customer Retrieve
                loggerHelper.traceInfo("individual customer retrieve start");
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                Entity individualCustomer = null;
                if (!string.IsNullOrEmpty(loginParameters_Web.emailaddress))
                {
                    individualCustomer = individualCustomerRepository.getIndividualCustomerByWebEmailAddress(loginParameters_Web.emailaddress, new string[] { });

                    if (individualCustomer == null)
                    {
                        individualCustomer = individualCustomerRepository.getIndividualCustomerByEmailAddress(loginParameters_Web.emailaddress, new string[] { });
                    }
                }
                else if (!string.IsNullOrEmpty(loginParameters_Web.mobilePhone))
                {
                    individualCustomer = individualCustomerRepository.getCustomerByMobilePhoneWithGivenColumns(loginParameters_Web.mobilePhone, new string[] { });
                    if (individualCustomer != null && string.IsNullOrEmpty(individualCustomer.GetAttributeValue<string>("rnt_webemailaddress")))
                    {
                        individualCustomer = null;
                    }
                }

                loggerHelper.traceInfo("individual customer retrieve end");

                if (individualCustomer == null)
                {
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    return new LoginResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError(xrmHelper.GetXmlTagContentByGivenLangId("MissingUser", loginParameters_Web.langId, webErrorMessagePath))
                    };
                }
                #endregion

                List<Task> _tasks = new List<Task>();
                var _marketingPermissions = new MarketingPermission_Web();
                var _individualAddress = new List<IndividualAddressData_Web>();
                var _invoiceAddress = new List<InvoiceAddressData_Web>();
                var _customerCreditCards = new List<CreditCardData_Web>();

                #region Marketing Permission Task
                var marketingPermissionTask = new Task(() =>
                {
                    loggerHelper.traceInfo("markettingPermissionsRepository retrieve start");
                    MarkettingPermissionsRepository markettingPermissionsRepository = new MarkettingPermissionsRepository(crmServiceHelper.IOrganizationService);
                    var marketingPermissions = markettingPermissionsRepository.getMarkettingPermissionByContactId(individualCustomer.Id);
                    if (marketingPermissions != null)
                        _marketingPermissions = new MarketingPermissionMapper().createWebMarketingPermissionData(marketingPermissions);
                    loggerHelper.traceInfo("markettingPermissionsRepository retrieve end");

                });
                _tasks.Add(marketingPermissionTask);
                marketingPermissionTask.Start();
                #endregion

                #region IndividualAdress Task
                var individualAdressTask = new Task(() =>
                {
                    loggerHelper.traceInfo("_individualAddress retrieve start");
                    IndividualAddressRepository individualAddressRepository = new IndividualAddressRepository(crmServiceHelper.IOrganizationService);
                    var individualAddress = individualAddressRepository.getIndividualAddressesByCustomerId(individualCustomer.Id);
                    _individualAddress = new IndividualAddressMapper().createWebIndividualAddressList(individualAddress);
                    loggerHelper.traceInfo("_individualAddress retrieve end");

                });
                _tasks.Add(individualAdressTask);
                individualAdressTask.Start();
                #endregion

                #region Invoice Address
                var invoiceAddressTask = new Task(() =>
                {
                    loggerHelper.traceInfo("invoiceAddressTask retrieve start");
                    InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(crmServiceHelper.IOrganizationService);
                    var columns = new string[]
                    {
                        "rnt_invoicetypecode",
                        "rnt_government",
                        "rnt_firstname",
                        "rnt_lastname",
                        "rnt_taxofficeid",
                        "rnt_taxnumber",
                        "rnt_districtid",
                        "rnt_countryid",
                        "rnt_companyname",
                        "rnt_cityid",
                        "rnt_addressdetail",
                        "rnt_name"
                    };
                    var invoiceAddress = invoiceAddressRepository.getInvoiceAddressByCustomerIdByGivenColumns(individualCustomer.Id, columns);
                    _invoiceAddress = new InvoiceAddressMapper().createWebInvoiceAddressList(invoiceAddress);
                    loggerHelper.traceInfo("invoiceAddressTask retrieve end");

                });
                _tasks.Add(invoiceAddressTask);
                invoiceAddressTask.Start();
                #endregion

                #region Customer Credit Card

                var customerCreditCard = new Task(() =>
                {
                    loggerHelper.traceInfo("customerCreditCard retrieve start");
                    CreditCardRepository creditCardRepository = new CreditCardRepository(crmServiceHelper.IOrganizationService);
                    var columns = new string[]
                    {
                        "rnt_name",
                        "rnt_carduserkey",
                        "rnt_cardtoken"
                    };
                    var creditCards = creditCardRepository.getCreditCardsByCustomerId(individualCustomer.Id, columns);
                    _customerCreditCards = new CreditCardMapper().createWebCreditCardList(creditCards);
                    loggerHelper.traceInfo("customerCreditCard retrieve end");

                });
                _tasks.Add(customerCreditCard);
                customerCreditCard.Start();

                #endregion

                Task.WaitAll(_tasks.ToArray());

                LoginResponse_Web loginResponse_Web = new LoginResponse_Web();
                loginResponse_Web.responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess();
                loginResponse_Web.customerInformation = new IndividualCustomerMapper().createWebIndividualCustomerData(individualCustomer, loginParameters_Web.langId);
                loginResponse_Web.marketingPermission = _marketingPermissions;
                loginResponse_Web.individualAddressInformation = _individualAddress;
                loginResponse_Web.invoiceAddressInformation = _invoiceAddress;
                loginResponse_Web.customerCreditCards = _customerCreditCards;
                return loginResponse_Web;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("login error : " + ex.Message);
                return new LoginResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/createcustomer")]
        public IndividualCustomerCreateResponse_Web createCustomer(IndividualCustomerCreateParameter_Web individualCustomerCreateParameter_Web)
        {
            string emailAddress = Convert.ToString(individualCustomerCreateParameter_Web.emailAddress);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(emailAddress))
            {
                emailAddress = emailAddress.Replace("@", "");
                emailAddress = emailAddress.Replace(".com", "");
                loggerHelper = new LoggerHelper(emailAddress);
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            try
            {
                loggerHelper.traceInfo("IndividualCustomerCreateParameter_Web : " + JsonConvert.SerializeObject(individualCustomerCreateParameter_Web));

                var isTurkishCitizen = true;
                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid");
                if (new Guid(turkeyGuid.Split(';')[0]) != individualCustomerCreateParameter_Web.citizenShipId)
                {
                    isTurkishCitizen = false;
                }

                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var result = individualCustomerRepository.getExistingCustomerIdByGovernmentIdOrPassportNumberOrEmailAddress(individualCustomerCreateParameter_Web.governmentId,
                                                                                                                            individualCustomerCreateParameter_Web.emailAddress,
                                                                                                                            new string[] { "rnt_webemailaddress", "rnt_webpassword", "rnt_distributionchannelcode" });

                if (result == null)
                {
                    var customerParameter = new IndividualCustomerMapper().buildCreateIndividualCustomerParameter(individualCustomerCreateParameter_Web, isTurkishCitizen);
                    OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CreateIndividualCustomerandAddress");
                    organizationRequest["LangId"] = Convert.ToString(individualCustomerCreateParameter_Web.langId);
                    organizationRequest["CustomerInformation"] = JsonConvert.SerializeObject(customerParameter);
                    var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);


                    var parsedResponse = JsonConvert.DeserializeObject<ClassLibrary.ResponseResult>(Convert.ToString(response.Results["ResponseResult"]));
                    if (parsedResponse.Result)
                    {
                        loggerHelper.traceInfo("response : " + JsonConvert.SerializeObject(parsedResponse));
                        return new IndividualCustomerCreateResponse_Web
                        {
                            responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess(),
                            individualAddressId = new Guid(Convert.ToString(response.Results["IndividualAddressId"])),
                            individualCustomerId = new Guid(Convert.ToString(response.Results["IndividualCustomerId"])),
                            invoiceAddressId = new Guid(Convert.ToString(response.Results["InvoiceAddressId"]))
                        };
                    }
                    return new IndividualCustomerCreateResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError(parsedResponse.ExceptionDetail)
                    };
                }
                else if (result != null && (!result.Contains("rnt_webemailaddress") || !result.Contains("rnt_webpassword")))
                {
                    var customerParameter = new IndividualCustomerMapper().buildUpdatendividualCustomerParameter(individualCustomerCreateParameter_Web, isTurkishCitizen);
                    customerParameter.individualCustomerId = result.Id;
                    customerParameter.distributionChannelCode = result.GetAttributeValue<OptionSetValue>("rnt_distributionchannelcode").Value;
                    IndividualAddressRepository individualAddressRepository = new IndividualAddressRepository(crmServiceHelper.IOrganizationService);
                    var address = individualAddressRepository.getIndividualAddressesByCustomerId(result.Id);
                    if (address.Count == 0)
                    {
                        IndividualAddressBL individualAddressBL = new IndividualAddressBL(crmServiceHelper.IOrganizationService);

                        var individualAdressParams = new IndividualAddressCreateParameters();
                        if (customerParameter.addressCity != null)
                        {
                            individualAdressParams.addressCityId = new Guid(customerParameter.addressCity.value);
                        }
                        if (customerParameter.addressDistrict != null)
                        {
                            individualAdressParams.addressDistrictId = new Guid(customerParameter.addressDistrict.value);
                        }
                        individualAdressParams.addressCountryId = new Guid(customerParameter.addressCountry.value);
                        individualAdressParams.addressDetail = customerParameter.addressDetail;
                        individualAdressParams.individualCustomerId = result.Id;
                        customerParameter.individualAddressId = individualAddressBL.createDefaultIndividualAddress(individualAdressParams);
                    }
                    else
                    {
                        customerParameter.individualAddressId = address.FirstOrDefault().individualAddressId;
                    }


                    OrganizationRequest organizationRequest = new OrganizationRequest("rnt_UpdateIndividualCustomerandAddress");
                    organizationRequest["CustomerInformation"] = JsonConvert.SerializeObject(customerParameter);
                    var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                    var parsedResponse = JsonConvert.DeserializeObject<IndividualCustomerUpdateResponse>(Convert.ToString(response.Results["ExecutionResult"]));
                    if (parsedResponse.ResponseResult.Result)
                    {
                        //todo will move into update action
                        Entity e = new Entity("contact");
                        e.Id = result.Id;
                        e["rnt_webemailaddress"] = customerParameter.email;
                        e["rnt_webpassword"] = customerParameter.password;
                        crmServiceHelper.IOrganizationService.Update(e);
                        return new IndividualCustomerCreateResponse_Web
                        {
                            responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess(),
                        };
                    }
                }
                else if (result != null && result.Contains("rnt_webemailaddress") && result.Contains("rnt_webpassword"))
                {
                    var email = result.GetAttributeValue<string>("rnt_webemailaddress");
                    var splitted = email.Split('@');
                    var str = "";
                    for (int i = 0; i < splitted[0].Length; i++)
                    {
                        if (i == 0 || i == 1)
                        {
                            str += splitted[0][i];
                        }
                        else
                        {
                            str += "*";
                        }
                    }
                    str += "@" + splitted[1];
                    var message = string.Format("Sistemde {0} mail adresine ait bir kaydınız bulunmaktadır.", str);
                    return new IndividualCustomerCreateResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError(message)
                    };
                }
                return new IndividualCustomerCreateResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess(),
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("createCustomer error : " + ex.Message);
                //todo make a generic method
                var splitted = ex.Message.Replace("CustomErrorMessagefinder:", "").Split(new string[] { "System.Exception:" }, StringSplitOptions.None);

                return new IndividualCustomerCreateResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(splitted.Length == 1 ? splitted[0] : splitted[1])
                };
            }
        }
        [HttpPost]
        [Route("api/web/updatecustomer")]
        public IndividualCustomerUpdateResponseWeb updateCustomer(IndividualCustomerUpdateParameter_Web individualCustomerUpdateParameter_Web)
        {

            LoggerHelper loggerHelper;
            if (individualCustomerUpdateParameter_Web.individualCustomerId != null && individualCustomerUpdateParameter_Web.individualCustomerId != Guid.Empty)
            {
                string individualCustomerId = Convert.ToString(individualCustomerUpdateParameter_Web.individualCustomerId);
                loggerHelper = new LoggerHelper(individualCustomerId + "UC");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            try
            {
                loggerHelper.traceInputsInfo<IndividualCustomerUpdateParameter_Web>(individualCustomerUpdateParameter_Web);

                var isTurkishCitizen = true;
                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid");
                if (new Guid(turkeyGuid.Split(';')[0]) != individualCustomerUpdateParameter_Web.citizenShipId)
                {
                    isTurkishCitizen = false;
                }
                var customerParameter = new IndividualCustomerMapper().buildUpdatendividualCustomerParameter(individualCustomerUpdateParameter_Web, isTurkishCitizen);
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_UpdateIndividualCustomerandAddress");
                organizationRequest["LangId"] = individualCustomerUpdateParameter_Web.langId;
                organizationRequest["CustomerInformation"] = JsonConvert.SerializeObject(customerParameter);
                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                var parsedResponse = JsonConvert.DeserializeObject<IndividualCustomerUpdateResponse>(Convert.ToString(response.Results["ExecutionResult"]));
                if (parsedResponse.ResponseResult.Result)
                {
                    return new IndividualCustomerUpdateResponseWeb
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess(),
                    };
                }
                return new IndividualCustomerUpdateResponseWeb
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(parsedResponse.ResponseResult.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("updateCustomer error : " + ex.Message);
                //todo make a generic method
                var splitted = ex.Message.Replace("CustomErrorMessagefinder:", "").Split(new string[] { "System.Exception:" }, StringSplitOptions.None);

                return new IndividualCustomerUpdateResponseWeb
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(splitted.Length == 1 ? splitted[0].TrimStart() : splitted[1].TrimStart())
                };
            }
        }
        [HttpPost]
        [Route("api/web/sendsmsvericationindividualcustomercreation")]
        public SendSmsVericationIndividualCustomerCreationResponse_Web sendSmsVericationIndividualCustomerCreation(SendSmsVericationIndividualCustomerCreationRequest_Web sendSmsVericationIndividualCustomerCreationRequest)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            loggerHelper.traceInputsInfo<SendSmsVericationIndividualCustomerCreationRequest_Web>(sendSmsVericationIndividualCustomerCreationRequest);
            string dialCode = "90";
            if (string.IsNullOrEmpty(sendSmsVericationIndividualCustomerCreationRequest.nationalityId))
            {
                sendSmsVericationIndividualCustomerCreationRequest.langId = 1055;
            }
            else
            {
                sendSmsVericationIndividualCustomerCreationRequest.langId = 1033;
                CountryRepository countryRepository = new CountryRepository(crmServiceHelper.IOrganizationService);
                CountryData countryData = countryRepository.GetActiveCountriesWithId(new Guid(sendSmsVericationIndividualCustomerCreationRequest.nationalityId));
                dialCode = countryData.countryDialCode;
            }


            string fullMobilePhone = dialCode + sendSmsVericationIndividualCustomerCreationRequest.mobilePhone;

            try
            {
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_GenerateAndSendSMS");
                organizationRequest["FirstName"] = sendSmsVericationIndividualCustomerCreationRequest.firstName;
                organizationRequest["LastName"] = sendSmsVericationIndividualCustomerCreationRequest.lastName;
                organizationRequest["MobilePhone"] = fullMobilePhone;
                organizationRequest["LangId"] = sendSmsVericationIndividualCustomerCreationRequest.langId;
                organizationRequest["SMSContentCode"] = (int)GlobalEnums.SmsContentCode.ContactCreateSms;
                organizationRequest["email"] = sendSmsVericationIndividualCustomerCreationRequest.email;
                organizationRequest["VerificationCode"] = sendSmsVericationIndividualCustomerCreationRequest.verificationCode;
                crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                return new SendSmsVericationIndividualCustomerCreationResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("sendSmsVericationIndividualCustomerCreation error : " + ex.Message);
                return new SendSmsVericationIndividualCustomerCreationResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/sendsmsvericationindividualcustomerupdate")]
        public SendSmsVericationIndividualCustomerUpdateResponse_Web sendSmsVericationIndividualCustomerUpdate(SendSmsVericationIndividualCustomerUpdateRequest_Web sendSmsVericationIndividualCustomerUpdateRequest_Web)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            loggerHelper.traceInputsInfo<SendSmsVericationIndividualCustomerUpdateRequest_Web>(sendSmsVericationIndividualCustomerUpdateRequest_Web);
            try
            {
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(sendSmsVericationIndividualCustomerUpdateRequest_Web.individualCustomerId,
                                                                                                      new string[] { "rnt_dialcode",
                                                                                                                     "emailaddress1",
                                                                                                                     "firstname",
                                                                                                                     "lastname" });

                string dialCode = customer.GetAttributeValue<string>("rnt_dialcode");
                if (customer.Contains("rnt_dialcode") && customer.GetAttributeValue<string>("rnt_dialcode") != "90")
                {
                    sendSmsVericationIndividualCustomerUpdateRequest_Web.langId = 1033;
                }
                else
                {
                    sendSmsVericationIndividualCustomerUpdateRequest_Web.langId = 1055;
                }
                string fullMobilePhone = dialCode + sendSmsVericationIndividualCustomerUpdateRequest_Web.mobilePhone;
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_GenerateAndSendSMS");
                organizationRequest["FirstName"] = customer.GetAttributeValue<string>("firstname");
                organizationRequest["LastName"] = customer.GetAttributeValue<string>("lastname");
                organizationRequest["MobilePhone"] = fullMobilePhone;
                organizationRequest["LangId"] = sendSmsVericationIndividualCustomerUpdateRequest_Web.langId;
                organizationRequest["SMSContentCode"] = (int)GlobalEnums.SmsContentCode.ContactUpdateSms;
                organizationRequest["email"] = customer.GetAttributeValue<string>("emailaddress1");

                organizationRequest["VerificationCode"] = sendSmsVericationIndividualCustomerUpdateRequest_Web.verificationCode;
                crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                return new SendSmsVericationIndividualCustomerUpdateResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("sendSmsVericationIndividualCustomerUpdate error : " + ex.Message);
                return new SendSmsVericationIndividualCustomerUpdateResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/calculateavailability")]
        public AvailabilityResponse_Web calculateAvailability([FromBody] AvailabilityParameters_Web availabilityParameters_Web)
        {
            string searchCustomer = "";
            if (availabilityParameters_Web.corporateCustomerId.HasValue && availabilityParameters_Web.corporateCustomerId != Guid.Empty)
            {
                searchCustomer = Convert.ToString(availabilityParameters_Web.corporateCustomerId);
            }
            else if (availabilityParameters_Web.individualCustomerId.HasValue && availabilityParameters_Web.individualCustomerId != Guid.Empty)
            {
                searchCustomer = Convert.ToString(availabilityParameters_Web.individualCustomerId);
            }
            else
            {
                searchCustomer = Convert.ToString(availabilityParameters_Web.queryParameters.pickupBranchId);
            }

            LoggerHelper loggerHelper = new LoggerHelper(searchCustomer);
            loggerHelper.traceInputsInfo<AvailabilityParameters_Web>(availabilityParameters_Web);
            try
            {
                var pickupBranchId = Convert.ToString(availabilityParameters_Web.queryParameters.pickupBranchId);
                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                availabilityParameters_Web.queryParameters = virtualBranchHelper.buildVirtualBrachParameters(availabilityParameters_Web.queryParameters);
                var availabilityParam = new AvailabilityMapper().createAvailabilityParameter_Web(availabilityParameters_Web, (int)rnt_ReservationChannel.Web, (int)GlobalEnums.CustomerType.Individual, 0);

                MongoDBHelper.Entities.CrmConfigurationBusiness crmConfigurationBusiness = new MongoDBHelper.Entities.CrmConfigurationBusiness(mongoDBHostName, mongoDBDatabaseName);
                var cacheKey = crmConfigurationBusiness.getCrmConfigurationByKey<string>("branch_CacheKey");

                loggerHelper.traceInfo("cache key : " + cacheKey);
                BranchCacheClient branchCacheClient = new BranchCacheClient(mongoDBHostName, mongoDBDatabaseName, cacheKey);
                var branchs = branchCacheClient.getBranchCache("web", (int)rnt_ReservationChannel.Web, true);
                loggerHelper.traceInfo("branches retrieved : ");

                var pickUpBranch = branchs.Where(x => x.BranchId == pickupBranchId).FirstOrDefault();


                var _customerType = availabilityParameters_Web.corporateCustomerId.HasValue && availabilityParameters_Web.corporateCustomerId != Guid.Empty
                                        ? (int)GlobalEnums.CustomerType.Corporate
                                        : (int)GlobalEnums.CustomerType.Individual;
                loggerHelper.traceInfo("_customerType: " + _customerType);
                #region Shift Duration - System Parameter                
                var shiftDuration = Convert.ToInt32(StaticHelper.GetConfiguration("ShiftDuration"));
                loggerHelper.traceInfo("shiftDuration : " + shiftDuration);

                availabilityParam = new AvailabilityMapper().createAvailabilityParameter_Web(availabilityParameters_Web,
                                                                                            (int)rnt_ReservationChannel.Web,
                                                                                            _customerType,
                                                                                            shiftDuration);

                if (pickUpBranch.earlistPickupTime.HasValue && pickUpBranch.earlistPickupTime != 0)
                {
                    availabilityParam.earlistPickupTime = pickUpBranch.earlistPickupTime;
                }

                if (_customerType == (int)GlobalEnums.CustomerType.Corporate)
                {
                    MongoDBHelper.Repository.CorporateCustomerRepository corporateCustomerRepository = new MongoDBHelper.Repository.CorporateCustomerRepository(mongoDBHostName, mongoDBDatabaseName);
                    var corporateCustomer = corporateCustomerRepository.getCustomerById(Convert.ToString(availabilityParameters_Web.corporateCustomerId));
                    if (corporateCustomer.priceFactorGroupCode.HasValue && corporateCustomer.priceFactorGroupCode != 0)
                    {
                        availabilityParam.accountGroup = corporateCustomer.priceFactorGroupCode.Value.ToString();
                    }
                }
                #endregion

                MongoDBHelper.Entities.AvailabilityFactorBusiness availabilityFactorBusiness = new MongoDBHelper.Entities.AvailabilityFactorBusiness(mongoDBHostName, mongoDBDatabaseName);
                var r = availabilityFactorBusiness.checkAvailability(availabilityParam, availabilityParameters_Web.langId, (int)rnt_ReservationChannel.Web, _customerType, availabilityParam.accountGroup);
                if (!r.ResponseResult.Result)
                {
                    return new AvailabilityResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError(r.ResponseResult.ExceptionDetail)
                    };
                }
                loggerHelper.traceInfo("check availabiltiy ended : ");

                loggerHelper.traceInfo("availabilityParam : " + JsonConvert.SerializeObject(availabilityParam));

                #region Calculate Prices from MongoDB

                MongoDBHelper.Entities.AvailabilityBusiness availabilityBusiness = new MongoDBHelper.Entities.AvailabilityBusiness(availabilityParam);
                var calculateAvailability = availabilityBusiness.calculateAvailability();

                loggerHelper.traceInfo("availabilitye end ");

                #endregion

                #region Remove not calculated groups
                loggerHelper.traceInfo("Remove not calculated groups start ");

                var calculatedGroups = calculateAvailability.Where(p => p.isPriceCalculatedSafely == true && p.hasError == false).ToList();
                var response = new AvailabilityMapper().createWebAvailabilityList(calculatedGroups);
                List<AvailabilityData_Web> removedItems = new List<AvailabilityData_Web>();

                if (availabilityParameters_Web.campaignId.HasValue && availabilityParameters_Web.campaignId.Value != Guid.Empty)
                {
                    MongoDBHelper.Entities.CampaignBusiness campaignBusiness = new MongoDBHelper.Entities.CampaignBusiness(this.mongoDBHostName, this.mongoDBDatabaseName);
                    foreach (var item in response)
                    {
                        var prices = campaignBusiness.calculateCampaignPrices(new CreateCampaignPricesRequest
                        {
                            campaignParameters = new CampaignParameters
                            {
                                beginingDate = availabilityParameters_Web.queryParameters.pickupDateTime,
                                endDate = availabilityParameters_Web.queryParameters.dropoffDateTime,
                                branchId = Convert.ToString(availabilityParameters_Web.queryParameters.pickupBranchId),
                                calculatedPricesTrackingNumber = availabilityBusiness.trackingNumber,
                                customerType = _customerType,
                                groupCodeInformationId = Convert.ToString(item.groupCodeId),
                                reservationChannelCode = Convert.ToString((int)rnt_ReservationChannel.Web)
                            }
                        },
                        availabilityParameters_Web.campaignId.Value.ToString());

                        if (!prices.ResponseResult.Result)
                        {
                            removedItems.Add(item);
                            continue;
                        }
                        item.campaign_payNowTotalAmount = Decimal.Round(prices.calculatedCampaignPrices.FirstOrDefault().payNowDailyPrice.Value, 2);
                        item.campaign_payLaterTotalAmount = Decimal.Round(prices.calculatedCampaignPrices.FirstOrDefault().payLaterDailyPrice.Value, 2);
                    }

                }
                response = response.Except(removedItems).ToList();
                loggerHelper.traceInfo("Remove not calculated groups end ");

                #endregion

                #region Get Duration
                loggerHelper.traceInfo("Get Duration start ");

                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var duration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(availabilityParameters_Web.queryParameters.pickupDateTime, availabilityParameters_Web.queryParameters.dropoffDateTime);
                loggerHelper.traceInfo("Get Duration end ");

                if (response == null || response.Count == 0)
                {
                    CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("NullEquipmentList", availabilityParameters_Web.langId, this.webErrorMessagePath);

                    loggerHelper.traceInfo("response null " + message);
                    return new AvailabilityResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError(message)
                    };
                }


                #endregion

                return new AvailabilityResponse_Web
                {
                    availabilityData = response,
                    trackingNumber = availabilityBusiness.trackingNumber,
                    duration = duration,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("calculateAvailability error : " + ex.Message);
                return new AvailabilityResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/getadditionalproducts")]
        public AdditionalProductResponse_Web getAdditionalProducts([FromBody] AdditionalProductParameters_Web additionalProductParameters_Web)
        {
            LoggerHelper loggerHelper;
            if (additionalProductParameters_Web.individualCustomerId.HasValue && additionalProductParameters_Web.individualCustomerId != null)
            {
                string individualCustomerId = Convert.ToString(additionalProductParameters_Web.individualCustomerId.Value);
                loggerHelper = new LoggerHelper(individualCustomerId + "GAP");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            loggerHelper.traceInfo(JsonConvert.SerializeObject(additionalProductParameters_Web));

            try
            {
                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                additionalProductParameters_Web.queryParameters = virtualBranchHelper.buildVirtualBrachParameters(additionalProductParameters_Web.queryParameters);

                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var cacheKey = configurationBL.GetConfigurationByName("additionalProduct_cacheKey");


                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect(additionalProductParameters_Web.queryParameters.pickupDateTime,
                                                                                              additionalProductParameters_Web.queryParameters.dropoffDateTime);

                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                             StaticHelper.prepareAdditionalProductCacheKey(totalDuration.ToString(), cacheKey));
                var products = additionalProductCacheClient.getAdditionalProductsCache(totalDuration, additionalProductParameters_Web.queryParameters.pickupBranchId);

                AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(crmServiceHelper.IOrganizationService);
                var dateandBranchParameters = new AdditionalProductMapper().buildAdditionalProductDateandBranchNeccessaryParameters(additionalProductParameters_Web);

                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var individualCustomer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(additionalProductParameters_Web.individualCustomerId.Value, new string[] { "birthdate", "rnt_drivinglicensedate" });

                var individualParameter = new AdditionalProductMapper().buildAdditionalProductIndividualNeccessaryParameters(individualCustomer.GetAttributeValue<DateTime>("birthdate"),
                                                                                                                             individualCustomer.GetAttributeValue<DateTime>("rnt_drivinglicensedate"));

                GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(crmServiceHelper.IOrganizationService);
                var groupCodeEntity = groupCodeInformationRepository.getGroupCodeInformationById(additionalProductParameters_Web.groupCodeId.Value);

                var groupCodeInformationParameter = new AdditionalProductMapper().buildAdditionalProductGroupCodeInformationNeccessaryParameters(groupCodeEntity.GetAttributeValue<int>("rnt_youngdriverage"),
                                                                                                                                                 groupCodeEntity.GetAttributeValue<int>("rnt_minimumage"),
                                                                                                                                                 groupCodeEntity.GetAttributeValue<int>("rnt_youngdriverlicence"),
                                                                                                                                                 groupCodeEntity.GetAttributeValue<int>("rnt_minimumdriverlicence"));

                var response = additionalProductsBL.additionalProductMandatoryOperations(products,
                                                                                         groupCodeInformationParameter,
                                                                                         individualParameter,
                                                                                         dateandBranchParameters,
                                                                                         totalDuration);

                #region Additional Product Rules
                List<AdditionalProductRule_Web> additionalProductRules = new List<AdditionalProductRule_Web>();
                var additionalProductRulesCacheKey = configurationBL.GetConfigurationByName("additionalProductRules_cacheKey");

                AdditionalProductRuleCacheClient additionalProductRuleCacheClient = new AdditionalProductRuleCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                             additionalProductRulesCacheKey);
                additionalProductRules = additionalProductRuleCacheClient.getAdditionalProductRuleCache();
                #endregion   

                if (!response.ResponseResult.Result)
                {
                    return new AdditionalProductResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError(response.ResponseResult.ExceptionDetail)
                    };
                }
                return new AdditionalProductResponse_Web
                {
                    duration = totalDuration,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess(),
                    additionalProducts = new AdditionalProductMapper().buildAdditionalProductsforWeb(response.AdditionalProducts),
                    additionalProductRules = additionalProductRules
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getAdditionalProducts error : " + ex.Message);
                return new AdditionalProductResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/createreservation")]
        public ReservationCreateResponse_Web createReservation([FromBody] ReservationCreateParameters_Web reservationCreateParameters_Web)
        {
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(reservationCreateParameters_Web.reservationPriceParameters.trackingNumber))
            {
                string trackingNumber = Convert.ToString(reservationCreateParameters_Web.reservationPriceParameters.trackingNumber);
                loggerHelper = new LoggerHelper(trackingNumber + "CR");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            try
            {
                loggerHelper.traceInputsInfo<ReservationCreateParameters_Web>(reservationCreateParameters_Web);

                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                reservationCreateParameters_Web.reservationQueryParameters = virtualBranchHelper.buildVirtualBrachParameters(reservationCreateParameters_Web.reservationQueryParameters);

                var customerParameters = new IndividualCustomerMapper().buildReservationCustomerParameters(reservationCreateParameters_Web.reservationCustomerParameters);

                var reservationDateandBranchParameters = new ReservationDateandBranchParameters
                {
                    dropoffBranchId = reservationCreateParameters_Web.reservationQueryParameters.dropoffBranchId,
                    dropoffDate = reservationCreateParameters_Web.reservationQueryParameters.dropoffDateTime,
                    pickupBranchId = reservationCreateParameters_Web.reservationQueryParameters.pickupBranchId,
                    pickupDate = reservationCreateParameters_Web.reservationQueryParameters.pickupDateTime
                };

                var reservationEquipmentParameters = new GroupCodeInformationMapper().buildReservationEquipmentParameter(reservationCreateParameters_Web.reservationEquimentParameters);

                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect(reservationCreateParameters_Web.reservationQueryParameters.pickupDateTime,
                                                                                              reservationCreateParameters_Web.reservationQueryParameters.dropoffDateTime);

                var additionalProducts = new AdditionalProductMapper().buildReservationAdditionalProducts(reservationCreateParameters_Web.reservationAdditionalProducts, totalDuration);

                PriceCalculationSummariesRepository priceCalculationSummariesRepository = new PriceCalculationSummariesRepository(this.mongoDBHostName, this.mongoDBDatabaseName);

                var prices = priceCalculationSummariesRepository.getPriceCalculationSummariesforDailyPrices(reservationCreateParameters_Web.reservationEquimentParameters.groupCodeId.ToString(),
                                                                                                            reservationCreateParameters_Web.reservationPriceParameters.trackingNumber,
                                                                                                            reservationCreateParameters_Web.reservationPriceParameters.campaignId);


                prices = priceCalculationSummariesRepository.checkDailyPricesForMainCampaign(prices,
                                                                                             reservationCreateParameters_Web.reservationPriceParameters.trackingNumber,
                                                                                             reservationCreateParameters_Web.reservationEquimentParameters.groupCodeId.ToString());

                var totalAmount = decimal.Zero;
                if (reservationCreateParameters_Web.reservationPriceParameters.paymentChoice == (int)rnt_reservation_rnt_paymentchoicecode.PayNow)
                {
                    totalAmount = prices.Sum(p => p.payNowAmount);
                }
                else
                {
                    totalAmount = prices.Sum(p => p.payLaterAmount);
                }

                List<CreditCardData> creditCardDatas = new List<CreditCardData>();
                if (reservationCreateParameters_Web.reservationPriceParameters.customerCreditCard != null)
                {
                    var creditCardData = new CreditCardMapper().buildCreditCardData(reservationCreateParameters_Web.reservationPriceParameters.customerCreditCard);
                    creditCardDatas.Add(creditCardData);
                }
                ContractHelper contractHelper = new ContractHelper(crmServiceHelper.IOrganizationService);
                var vPosResponse = new VirtualPosResponse { virtualPosId = 0 };//contractHelper.getVPosIdforGivenCardNumber(creditCardData);
                var priceParameters = new PriceMapper().buildReservationPriceParameter(customerParameters,
                                                                                       reservationCreateParameters_Web.reservationPriceParameters,
                                                                                       creditCardDatas,
                                                                                       totalAmount,
                                                                                       customerParameters.contactId,
                                                                                       vPosResponse.virtualPosId);

                var cType = reservationCreateParameters_Web.reservationCustomerParameters.corporateCustomerId.HasValue &&
                            reservationCreateParameters_Web.reservationCustomerParameters.corporateCustomerId.Value != Guid.Empty ?
                            (int)rnt_ReservationTypeCode.Kurumsal :
                            (int)rnt_ReservationTypeCode.Bireysel;


                if (!string.IsNullOrEmpty(reservationCreateParameters_Web.CouponCode))
                {
                    MongoDBHelper.Repository.CouponCodeRepository codeDefinitionRepository = new MongoDBHelper.Repository.CouponCodeRepository(mongoDBHostName, mongoDBDatabaseName);
                    var d = codeDefinitionRepository.getCouponCodeDetailsByCouponCode(reservationCreateParameters_Web.CouponCode);

                    if (d.FirstOrDefault() != null)
                    {
                        BusinessLibrary.Repository.CouponCodeDefinitionRepository couponCodeDefinitionRepository = new BusinessLibrary.Repository.CouponCodeDefinitionRepository(crmServiceHelper.IOrganizationService);
                        CouponCodeDefinitionValidations couponCodeDefinitionValidations = new CouponCodeDefinitionValidations(crmServiceHelper.IOrganizationService);
                        var copuonCodeDefinition = couponCodeDefinitionRepository.getCouponCodeDefinitionById(new Guid(d.FirstOrDefault().couponCodeDefinitionId));
                        var validationResponse = couponCodeDefinitionValidations.checkCuponCodeDefinitionReservationDate(copuonCodeDefinition, reservationCreateParameters_Web.reservationQueryParameters.pickupDateTime);
                        if (!validationResponse)
                        {
                            XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);

                            var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponInvalidDefinationDate", reservationCreateParameters_Web.langId, couponCodeXmlPath);
                            throw new Exception(message);
                        }
                        if (copuonCodeDefinition != null)
                        {
                            decimal paynowdiscountvalue = copuonCodeDefinition.GetAttributeValue<decimal>("rnt_paynowdiscountvalue");
                            decimal paylaterdiscountvalue = copuonCodeDefinition.GetAttributeValue<decimal>("rnt_paylaterdiscountvalue");
                            decimal discountAmount = 0;
                            if (reservationCreateParameters_Web.reservationPriceParameters.paymentChoice == (int)rnt_reservation_rnt_paymentchoicecode.PayNow)
                            {
                                discountAmount = paynowdiscountvalue;
                            }
                            else
                            {
                                discountAmount = paylaterdiscountvalue;
                            }
                            var t = copuonCodeDefinition.GetAttributeValue<OptionSetValue>("rnt_type").Value;
                            if (t == 0)
                            {
                                priceParameters.discountAmount = (totalAmount * discountAmount) / 100;
                            }
                            else
                            {
                                priceParameters.discountAmount = discountAmount;
                            }

                        }

                    }

                }

                ConfigurationBL crmConfigurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var cur = crmConfigurationBL.GetConfigurationByName("currency_TRY");
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CreateReservation");
                organizationRequest["SelectedCustomer"] = JsonConvert.SerializeObject(customerParameters);
                organizationRequest["SelectedDateAndBranch"] = JsonConvert.SerializeObject(reservationDateandBranchParameters);
                organizationRequest["CouponCode"] = reservationCreateParameters_Web.CouponCode;
                organizationRequest["PriceParameters"] = JsonConvert.SerializeObject(priceParameters);
                organizationRequest["SelectedEquipment"] = JsonConvert.SerializeObject(reservationEquipmentParameters);
                organizationRequest["LangId"] = reservationCreateParameters_Web.langId;
                organizationRequest["TrackingNumber"] = reservationCreateParameters_Web.reservationPriceParameters.trackingNumber;
                organizationRequest["TotalDuration"] = totalDuration;
                organizationRequest["ReservationChannel"] = (int)rnt_ReservationChannel.Web;
                organizationRequest["ReservationTypeCode"] = cType;
                organizationRequest["SelectedAdditionalProducts"] = JsonConvert.SerializeObject(additionalProducts);
                organizationRequest["Currency"] = cur;

                var res = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var reservationResponse = JsonConvert.DeserializeObject<ReservationCreateResponse>(Convert.ToString(res.Results["ReservationResponse"]));


                var createResponse_Web = new ReservationCreateResponse_Web
                {

                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()

                };
                createResponse_Web.Map(reservationResponse);
                loggerHelper.traceInputsInfo<ReservationCreateResponse_Web>(createResponse_Web);
                return createResponse_Web;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("createReservation error : " + ex.Message);
                return new ReservationCreateResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/forgotpassword")]
        public ForgotPasswordResponse_Web forgotPassword(ForgotPasswordRequest_Web forgotPasswordRequest_Web)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(forgotPasswordRequest_Web));

                if (forgotPasswordRequest_Web.newPassword != forgotPasswordRequest_Web.newPasswordAgain)
                {
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("PasswordMismatch", forgotPasswordRequest_Web.langId, this.webErrorMessagePath);

                    return new ForgotPasswordResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError(message)
                    };

                }
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var customer = individualCustomerRepository.getIndividualCustomerByWebEmailAddress(forgotPasswordRequest_Web.emailAddress);
                if (customer == null)
                {
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("MissingUser", forgotPasswordRequest_Web.langId, this.webErrorMessagePath);

                    return new ForgotPasswordResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError(message)
                    };
                }


                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(crmServiceHelper.IOrganizationService);
                individualCustomerBL.updateWebPassword(customer.Id, forgotPasswordRequest_Web.newPasswordAgain);

                return new ForgotPasswordResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("forgotPassword error : " + ex.Message);
                return new ForgotPasswordResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpGet]
        [Route("api/web/getcampaignlist")]
        public GetCampaignListResponse_Web getCampaignList()
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                MongoDBHelper.Repository.CampaignRepository campaignRepository = new CampaignRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var campaigns = campaignRepository.getActiveCampaigns(DateTime.UtcNow.AddMinutes(StaticHelper.offset), (int)rnt_ReservationChannel.Web);
                loggerHelper.traceInfo("date utc now " + DateTime.UtcNow.AddMinutes(StaticHelper.offset));

                var campaignDatas = new List<CampaignData>();

                CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();

                foreach (var item in campaigns)
                {
                    CampaignData c = new CampaignData();
                    c.Map(item);
                    campaignDatas.Add(c);
                }
                var data = new CampaignMapper().buildWebCampaignData(campaignDatas);

                CampaignDetailRepository campaignDetailRepository = new CampaignDetailRepository(crmServiceHelper_Web.IOrganizationService);
                var cmsCampaigns = campaignDetailRepository.getCMSCampaigns(new string[] { "rnt_order", "rnt_campaignimageurl", "rnt_campaignid" });

                List<CampaignData_Web> excluded = new List<CampaignData_Web>();
                foreach (var item in data)
                {
                    var camp = cmsCampaigns.Where(p => p.Contains("rnt_campaignid") &&
                                                      p.GetAttributeValue<EntityReference>("rnt_campaignid").Id.Equals(item.campaignId)).FirstOrDefault();
                    if (camp != null)
                    {
                        item.campaignImageURL = camp.GetAttributeValue<string>("rnt_campaignimageurl");
                        item.order = camp.GetAttributeValue<int>("rnt_order");
                    }
                    else
                    {
                        excluded.Add(item);
                    }
                }
                data = data.Except(excluded).ToList();
                data = data.OrderByDescending(p => p.order).ToList();
                return new GetCampaignListResponse_Web
                {
                    campaigns = data,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCampaignList error : " + ex.Message);
                return new GetCampaignListResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/getcampaigndetail")]
        public GetCampaignDetailResponse_Web getCampaignDetail(GetCampaignDetailRequest_Web getCampaignDetailRequest_Web)
        {
            string campaignId = Convert.ToString(getCampaignDetailRequest_Web.campaignId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(campaignId))
            {
                loggerHelper = new LoggerHelper(campaignId + "GCD");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }

            try
            {
                loggerHelper.traceInfo("getCampaignDetailRequest_Web : " + JsonConvert.SerializeObject(getCampaignDetailRequest_Web));
                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
                CampaignDetailBL campaignDetailBL = new CampaignDetailBL(crmServiceHelper.IOrganizationService);
                var camp = campaignDetailBL.getCampaignDetail(getCampaignDetailRequest_Web.campaignId, getCampaignDetailRequest_Web.langId);
                return new GetCampaignDetailResponse_Web
                {
                    campaignDetail = new CampaignMapper().buildWebCampaignDetailData(camp),
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("getCampaignDetail error : " + ex.Message);
                return new GetCampaignDetailResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/web/getcampaignbranchlist")]
        public GetCampaignDetailResponse_Web getCampaignBranchList(GetCampaignDetailRequest_Web getCampaignDetailRequest_Web)
        {
            string campaignId = Convert.ToString(getCampaignDetailRequest_Web.campaignId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(campaignId))
            {
                loggerHelper = new LoggerHelper(campaignId + "GCBL");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }

            try
            {
                loggerHelper.traceInfo("getCampaignDetailRequest_Web : " + JsonConvert.SerializeObject(getCampaignDetailRequest_Web));
                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
                CampaignDetailBL campaignDetailBL = new CampaignDetailBL(crmServiceHelper.IOrganizationService);
                var camp = campaignDetailBL.getCampaignBranchList(getCampaignDetailRequest_Web.campaignId, getCampaignDetailRequest_Web.langId);

                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                var activeVirtualBranches = virtualBranchHelper.getActiveBranchesByChannelCode((int)rnt_ReservationChannel.Web);

                CampaignDetailData campaignDetailData = new CampaignDetailData();
                campaignDetailData.campaignBranchId = new List<string>();

                foreach (var item in camp.campaignBranchId)
                {
                    var temp = activeVirtualBranches.Where(x => x.branch.ToString() == item).FirstOrDefault();
                    if (temp != null)
                    {
                        campaignDetailData.campaignBranchId.Add(temp.virtualBranchId);
                    }
                }

                return new GetCampaignDetailResponse_Web
                {
                    campaignDetail = new CampaignMapper().buildWebCampaignDetailData(campaignDetailData),
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("getCampaignBranchList error : " + ex.Message);
                return new GetCampaignDetailResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/getcustomerreservations")]
        public GetCustomerReservationsResponse_Web getCustomerReservations([FromBody] GetCustomerReservationsRequest_Web getCustomerReservationsRequest_Web)
        {
            string individualCustomerId = Convert.ToString(getCustomerReservationsRequest_Web.individualCustomerId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(individualCustomerId))
            {
                loggerHelper = new LoggerHelper(individualCustomerId + "GCR");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var reservations = reservationItemRepository.getReservationsByCustomerId(Convert.ToString(getCustomerReservationsRequest_Web.individualCustomerId));
                loggerHelper.traceInfo(JsonConvert.SerializeObject(getCustomerReservationsRequest_Web));

                var reservationDatas = new List<ReservationItemData>();
                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                foreach (var item in reservations)
                {
                    ReservationItemData r = new ReservationItemData();
                    r.Map(item);
                    reservationDatas.Add(r);
                }

                var data = new ReservationMapper().buildWebReservationData(reservationDatas, getCustomerReservationsRequest_Web.langId);

                data = data.OrderByDescending(p => p.PickupTime).ToList();
                return new GetCustomerReservationsResponse_Web
                {
                    reservations = data,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCustomerReservations error : " + ex.Message);
                return new GetCustomerReservationsResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/getcorporatereservations")]
        public GetCorporateReservationsResponse_Web getCorporateReservations([FromBody] GetCorporateReservationsRequest_Web getCorporateReservationsRequest_Web)
        {
            string corporateCustomerId = Convert.ToString(getCorporateReservationsRequest_Web.corporateCustomerId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(corporateCustomerId))
            {
                loggerHelper = new LoggerHelper(corporateCustomerId + "GCR");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var reservations = reservationItemRepository.getReservationsByCustomerId(Convert.ToString(getCorporateReservationsRequest_Web.corporateCustomerId));
                loggerHelper.traceInfo(JsonConvert.SerializeObject(getCorporateReservationsRequest_Web));

                var reservationDatas = new List<ReservationItemData>();
                foreach (var item in reservations)
                {
                    ReservationItemData r = new ReservationItemData();
                    r.Map(item);
                    reservationDatas.Add(r);
                }

                var data = new ReservationMapper().buildWebReservationData(reservationDatas, getCorporateReservationsRequest_Web.langId);

                data = data.OrderByDescending(p => p.PickupTime).ToList();
                return new GetCorporateReservationsResponse_Web
                {
                    reservations = data,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCustomerReservations error : " + ex.Message);
                return new GetCorporateReservationsResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/processcustomerinvoiceaddress")]
        public ProcessCustomerInvoiceAddressResponse_Web processCustomerInvoiceAddress([FromBody] ProcessCustomerInvoiceAddressRequest_Web processCustomerInvoiceAddressRequest_Web)
        {
            string individualCustomerId = Convert.ToString(processCustomerInvoiceAddressRequest_Web.individualCustomerId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(individualCustomerId))
            {
                loggerHelper = new LoggerHelper(individualCustomerId + "PCIA");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            InvoiceCreationValidation invoiceCreationValidation = new InvoiceCreationValidation(crmServiceHelper.IOrganizationService);
            var key = processCustomerInvoiceAddressRequest_Web.invoiceType == (int)rnt_invoice_rnt_invoicetypecode.Individual ?
                                                                               processCustomerInvoiceAddressRequest_Web.governmentId :
                                                                               processCustomerInvoiceAddressRequest_Web.taxNumber;
            var r = invoiceCreationValidation.checkInvoiceIdentiyKey(processCustomerInvoiceAddressRequest_Web.invoiceType, key);
            if (!r.result)
            {
                return new ProcessCustomerInvoiceAddressResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(r.exceptionDetail)
                };
            }
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(processCustomerInvoiceAddressRequest_Web));

                var invoiceAddressParameters = new ProcessInvoiceMapper().buildInvoiceAddressCreateParameters(processCustomerInvoiceAddressRequest_Web);
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_ProcessCustomerInvoiceAddress");
                organizationRequest["invoiceAddressParameters"] = JsonConvert.SerializeObject(invoiceAddressParameters);
                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var invoiceAddressResponse = JsonConvert.DeserializeObject<InvoiceAddressProcessResponse>(Convert.ToString(response.Results["invoiceAddressResponse"]));

                if (invoiceAddressResponse.ResponseResult.Result)
                {
                    return new ProcessCustomerInvoiceAddressResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess(),
                        invoiceAddressId = invoiceAddressResponse.invoiceAddressId
                    };
                }

                return new ProcessCustomerInvoiceAddressResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(invoiceAddressResponse.ResponseResult.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("processCustomerInvoiceAddress error : " + ex.Message);
                return new ProcessCustomerInvoiceAddressResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/calculatecancelfineamount")]
        public CalculateCancelFineAmountResponse_Web calculateCancelFineAmount([FromBody] CalculateCancelFineAmountParameters_Web calculateCancelFineAmountParameters_Web)
        {
            string pnrNumber = Convert.ToString(calculateCancelFineAmountParameters_Web.pnrNumber);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(pnrNumber))
            {
                loggerHelper = new LoggerHelper(pnrNumber + "CCF");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();

            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(calculateCancelFineAmountParameters_Web));
                var cancelFineAmountParameters = new CancelFineAmountMapper().buildCancelFineAmountParameters(calculateCancelFineAmountParameters_Web);

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CalculateFineAmountReservation");
                organizationRequest["langId"] = calculateCancelFineAmountParameters_Web.langId;
                organizationRequest["reservationId"] = Convert.ToString(calculateCancelFineAmountParameters_Web.reservationId);
                organizationRequest["pnrNumber"] = Convert.ToString(calculateCancelFineAmountParameters_Web.pnrNumber);

                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var reservationFineAmountResponse = JsonConvert.DeserializeObject<ReservationCancellationResponse>(Convert.ToString(response.Results["ReservationFineAmountResponse"]));

                if (reservationFineAmountResponse.ResponseResult.Result)
                {
                    return new CalculateCancelFineAmountResponse_Web
                    {
                        fineAmount = reservationFineAmountResponse.fineAmount,
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                    };
                }

                return new CalculateCancelFineAmountResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(reservationFineAmountResponse.ResponseResult.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {

                loggerHelper.traceError("calculateCancelFineAmount error : " + ex.Message);
                return new CalculateCancelFineAmountResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/cancelreservation")]
        public CancelReservationResponse_Web cancelReservation([FromBody] CancelReservationParameters_Web cancelReservationParameters_Web)
        {
            string pnrNumber = Convert.ToString(cancelReservationParameters_Web.pnrNumber);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(pnrNumber))
            {
                loggerHelper = new LoggerHelper(pnrNumber + "CR");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();

            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(cancelReservationParameters_Web));
                MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var reservations = reservationItemRepository.getReservationByPnrNumber(cancelReservationParameters_Web.pnrNumber);

                var cancelReservationParameters = new CancelReservationMapper().buildCancelReservationParameters(cancelReservationParameters_Web);

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CancelReservation");
                organizationRequest["langId"] = cancelReservationParameters.langId;
                organizationRequest["reservationId"] = Convert.ToString(reservations.ReservationId);
                organizationRequest["pnrNumber"] = Convert.ToString(cancelReservationParameters.pnrNumber);
                organizationRequest["cancellationReason"] = (int)rnt_reservation_StatusCode.CancelledByCustomer;
                organizationRequest["cancellationSubReason"] = 100000008;
                organizationRequest["cancellationDescription"] = "Web Tarafından Iptal";
                organizationRequest["cancellationSubReason"] = 100000008;
                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var reservationCancellationResponse = JsonConvert.DeserializeObject<ReservationCancellationResponse>(Convert.ToString(response.Results["ReservationCancellationResponse"]));

                if (reservationCancellationResponse.ResponseResult.Result)
                {
                    return new CancelReservationResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                    };
                }

                return new CancelReservationResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(reservationCancellationResponse.ResponseResult.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("cancelReservation error : " + ex.Message);
                return new CancelReservationResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/getbanners")]
        public GetBannersResponse_Web getBanners([FromBody] GetBannersRequest_Web getBannersRequest_Web)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(getBannersRequest_Web));
                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();

                BannerBL bannerBL = new BannerBL(crmServiceHelper.IOrganizationService);
                var banners = bannerBL.getBanners();
                return new GetBannersResponse_Web
                {
                    bannerDatas = new BannerMapper().buildWebBannerData(banners),
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("getBanners error : " + ex.Message);
                return new GetBannersResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/getadditionalproductscontent")]
        public GetAdditionalProductsContentResponse_Web getAdditionalProductsContent([FromBody] GetAdditionalProductsContentRequest_Web getAdditionalProductsContentRequest_Web)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(getAdditionalProductsContentRequest_Web));
                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();

                CMSAdditionalProductBL cmsAdditionalProductBL = new CMSAdditionalProductBL(crmServiceHelper.IOrganizationService);
                var products = cmsAdditionalProductBL.GetCMSAdditionalProducts();

                var cmsProducts = new GetAdditionalProductsContentResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess(),
                    additionalProducts = new CMSAdditionalProductMapper().buildCMSAdditionalProducts(products)
                };

                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var cacheKey = configurationBL.GetConfigurationByName("additionalProduct_cacheKey");

                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(crmServiceHelper.IOrganizationService, cacheKey + "__1");
                var productsCache = additionalProductCacheClient.getAdditionalProductsCache(1, Guid.Empty);

                foreach (var item in cmsProducts.additionalProducts)
                {
                    var _product = productsCache.Where(p => p.productId == item.additionalProductId).FirstOrDefault();
                    if (_product != null)
                    {
                        item.additionalProductIcon = _product.webIconURL;
                    }
                }
                return cmsProducts;
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("getBanners error : " + ex.Message);
                return new GetAdditionalProductsContentResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/getAccountContacts")]
        public GetAccountContactsResponse_Web getAccountContacts([FromBody] GetAccountContactsRequest_Web getAccountContactsRequest_Web)
        {
            try
            {
                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
                ConnectionRepository connectionRepository = new ConnectionRepository(crmServiceHelper.IOrganizationService);
                IndividualAddressRepository individualAddressRepository = new IndividualAddressRepository(crmServiceHelper.IOrganizationService);
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);

                var connections = connectionRepository.getConnectionsByAccountId(getAccountContactsRequest_Web.accountId);
                var individualAddresses = new List<Entity>();
                var individualCustomers = new List<Entity>();
                if (connections.Count > 0)
                {
                    individualAddresses = individualAddressRepository
                                          .getAllIndividualAddressesByGivenCustomerIds(connections.Select(p => p.GetAttributeValue<EntityReference>("rnt_contactid").Id.ToString())
                                          .ToList());
                    individualCustomers = individualCustomerRepository
                                          .getIndividualCustomerByGivenIds(connections.Select(p => (object)p.GetAttributeValue<EntityReference>("rnt_contactid").Id).ToArray());
                }


                List<AccountContactsData_Web> accountContactsDatas = new List<AccountContactsData_Web>();

                if (connections == null)
                {
                    return new GetAccountContactsResponse_Web
                    {
                        accountContactsData = null,
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError("The account you have specified does not have any contacts")
                    };
                }

                connections.ForEach(conn =>
                {
                    var contactId = conn.GetAttributeValue<EntityReference>("rnt_contactid").Id;
                    var relationCode = conn.GetAttributeValue<OptionSetValue>("rnt_relationcode").Value;
                    var relation = Enum.GetName(typeof(ClassLibrary._Enums_1033.rnt_connection_rnt_relationcode), relationCode);

                    var individualAddress = individualAddresses.Where(a => a.GetAttributeValue<EntityReference>("rnt_contactid").Id == contactId).ToList();
                    var individualAddressData = new IndividualAddressMapper().buildIndividualAddressData(individualAddress);
                    var individualAddressDataWeb = new IndividualAddressMapper().createWebIndividualAddressList(individualAddressData);

                    var contact = individualCustomers.Where(c => c.Id == contactId).FirstOrDefault();
                    if (contact != null)
                    {
                        var contactData = new IndividualCustomerMapper().createWebIndividualCustomerData(contact, getAccountContactsRequest_Web.langId);
                        if (!string.IsNullOrEmpty(contactData.emailAddress))
                        {
                            accountContactsDatas.Add(new AccountContactsData_Web
                            {
                                individualCustomerId = contactId,
                                relation = relation.insertSpaceBetweenWords(),
                                birthdate = contactData.birthDate,
                                citizenship = contactData.citizenShipName,
                                drivingLicenseClassName = contactData.licenseClassName,
                                drivingLicenseDate = contactData.licenseDate,
                                drivingLicenseNumber = contactData.licenseNumber,
                                drivingLicensePlace = contactData.licensePlace,
                                emailAddress = contactData.emailAddress,
                                firstname = contactData.firstName,
                                governmentId = contactData.governmentId,
                                lastname = contactData.lastName,
                                phoneNumber = contactData.mobilePhone,
                                individualAddressDatas = individualAddressDataWeb
                            });
                        }

                    }
                });

                return new GetAccountContactsResponse_Web
                {
                    accountContactsData = accountContactsDatas,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new GetAccountContactsResponse_Web
                {
                    accountContactsData = null,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/createCorporateRelation")]
        public CreateCorporateRelationResponse_Web createCorporateRelation([FromBody] CreateCorporateRelationRequest_Web createCorporateRelationRequest_Web)
        {
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(createCorporateRelationRequest_Web));
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var customerId = individualCustomerRepository.getExistingCustomerIdByGovernmentIdOrPassportNumberOrEmailAddress(createCorporateRelationRequest_Web.governmentId,
                                                                                                                                createCorporateRelationRequest_Web.governmentId,
                                                                                                                                new string[] { });
                if (customerId == null)
                {
                    throw new Exception("Girdiğiniz bilgilere ait kullanıcı bulunamadı.");
                }

                ConnectionBL connectionBL = new ConnectionBL(crmServiceHelper.IOrganizationService);

                if (createCorporateRelationRequest_Web.breakRelation != null && createCorporateRelationRequest_Web.breakRelation == true)
                {
                    ConnectionRepository connectionRepository = new ConnectionRepository(crmServiceHelper.IOrganizationService);
                    var connection = connectionRepository.getConnectionByGivenCriterias(createCorporateRelationRequest_Web.accountId, customerId.Id, createCorporateRelationRequest_Web.relationType);

                    if (connection == null)
                    {
                        return new CreateCorporateRelationResponse_Web
                        {
                            responseResult = ClassLibrary._Web.ResponseResult.ReturnError("The connection you specified is already passive. Please change your parameters")
                        };
                    }

                    connectionBL.deactivateConnection(connection.Id);

                    return new CreateCorporateRelationResponse_Web
                    {
                        relationId = connection.GetAttributeValue<Guid>("rnt_connectionid"),
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                    };
                }

                var connectionId = connectionBL.createConnection(customerId.Id, createCorporateRelationRequest_Web.accountId, createCorporateRelationRequest_Web.relationType);

                return new CreateCorporateRelationResponse_Web
                {
                    relationId = connectionId,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new CreateCorporateRelationResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/getKilometerLimits")]
        public GetKilometerLimitsResponse_Web getKilometerLimits(GetKilometerLimitsRequest_Web getKilometerLimitsRequest_Web)
        {
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            try
            {
                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var duration = durationHelper.calculateDocumentDurationByPriceHourEffect(getKilometerLimitsRequest_Web.pickupDateTime, getKilometerLimitsRequest_Web.dropoffDateTime);

                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var cacheKey = configurationBL.GetConfigurationByName("groupcode_cacheKey");

                GroupCodeInformationCacheClient groupCodeInformationCacheClient = new GroupCodeInformationCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                                      cacheKey);
                var groupCodeData = groupCodeInformationCacheClient.getAllGroupCodeInformationDetailCache(getKilometerLimitsRequest_Web.langId);

                var kmLimitCacheKey = configurationBL.GetConfigurationByName("kmlimit_cachekey");

                KilometerLimitCacheClient kilometerLimitCacheClient = new KilometerLimitCacheClient(crmServiceHelper.IOrganizationService, kmLimitCacheKey);
                var limits = kilometerLimitCacheClient.getKilometerLimitCache(groupCodeData, duration);

                return new GetKilometerLimitsResponse_Web
                {
                    kmLimits = limits,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new GetKilometerLimitsResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/getcorporatecustomerreservations")]
        public GetCustomerReservationsResponse_Web getCorporateCustomerReservations([FromBody] GetCustomerReservationsRequest_Web getCustomerReservationsRequest_Web)
        {
            CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                var reservations = new List<ReservationItemDataMongoDB>();

                //first check user is the master user
                ConnectionRepository connectionRepository = new ConnectionRepository(crmServiceHelper_Web.IOrganizationService);
                var connection = connectionRepository.getConnectionByGivenCriterias(getCustomerReservationsRequest_Web.corporateCustomerId,
                                                                                    getCustomerReservationsRequest_Web.individualCustomerId,
                                                                                    (int)rnt_connection_rnt_relationcode.ResponsibleEmployeeforCorporateReservations);

                if (connection != null)
                {
                    MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                    reservations = reservationItemRepository.getCorporateReservations(getCustomerReservationsRequest_Web.corporateCustomerId);
                }
                else
                {
                    connection = connectionRepository.getConnectionByGivenCriterias(getCustomerReservationsRequest_Web.corporateCustomerId,
                                                                                   getCustomerReservationsRequest_Web.individualCustomerId,
                                                                                   (int)rnt_connection_rnt_relationcode.EmployeeForCorporateReservations);
                    if (connection != null)
                    {
                        MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                        reservations = reservationItemRepository.getCorporateReservationsByCustomer(getCustomerReservationsRequest_Web.corporateCustomerId, getCustomerReservationsRequest_Web.individualCustomerId);
                    }
                }

                loggerHelper.traceInfo(JsonConvert.SerializeObject(getCustomerReservationsRequest_Web));

                var reservationDatas = new List<ReservationItemData>();
                foreach (var item in reservations)
                {
                    ReservationItemData r = new ReservationItemData();
                    r.Map(item);
                    reservationDatas.Add(r);
                }

                var data = new ReservationMapper().buildWebReservationData(reservationDatas, getCustomerReservationsRequest_Web.langId);

                data = data.OrderByDescending(p => p.PickupTime).ToList();
                return new GetCustomerReservationsResponse_Web
                {
                    reservations = data,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCustomerReservations error : " + ex.Message);
                return new GetCustomerReservationsResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/retrieveinstallments")]
        public RetrieveInstallmentsResponse_Web retrieveInstallments([FromBody] RetrieveInstallmentParameters_Web retrieveInstallmentParameters_Web)
        {
            CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(retrieveInstallmentParameters_Web));
                CreditCardBL creditCardBL = new CreditCardBL(crmServiceHelper_Web.IOrganizationService);
                var installments = creditCardBL.retrieveInstallmentforGivenCard(new CreditCardMapper().buildInstallmentData(retrieveInstallmentParameters_Web));

                if (!installments.ResponseResult.Result)
                {
                    return new RetrieveInstallmentsResponse_Web
                    {
                        responseResult = ClassLibrary._Web.ResponseResult.ReturnError(installments.ResponseResult.ExceptionDetail)
                    };

                }
                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper_Web.IOrganizationService);
                var maturityDifferenceCode = configurationBL.GetConfigurationByName("additionalProduct_MaturityDifference");

                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(crmServiceHelper_Web.IOrganizationService, maturityDifferenceCode);
                var additionalProductData = additionalProductCacheClient.getAdditionalProductCache(maturityDifferenceCode);

                var additionalProductData_Web = new AdditionalProductMapper().buildAdditionalProductforWeb(additionalProductData);
                var web_installments = new CreditCardMapper().buildInstallmentData(installments.installmentData.FirstOrDefault());
                return new RetrieveInstallmentsResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess(),
                    additionalProduct = additionalProductData_Web,
                    installmentData = web_installments
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCustomerReservations error : " + ex.Message);
                return new RetrieveInstallmentsResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/getcustomerinvoices")]
        public GetCustomerInvoicesResponse_Web getCustomerInvoices([FromBody] GetCustomerInvoicesRequest_Web getCustomerInvoicesRequest_Web)
        {
            CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();
            try
            {
                InvoiceBL invoiceBL = new InvoiceBL(crmServiceHelper_Web.IOrganizationService);
                var invoices = invoiceBL.getCustomerInvoices(getCustomerInvoicesRequest_Web.individualCustomerId);

                InvoiceMapper invoiceMapper = new InvoiceMapper();
                var webInvoices = invoiceMapper.builWebInvoiceData(invoices);
                webInvoices = webInvoices.OrderByDescending(p => p.invoiceDate).ToList();

                return new GetCustomerInvoicesResponse_Web
                {
                    customerInvoices = webInvoices,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {

                return new GetCustomerInvoicesResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/generateinvoicedocument")]
        public GenerateInvoiceDocumentResponse_Web generateInvoiceDocument([FromBody] GenerateInvoiceDocumentRequest_Web generateInvoiceDocumentRequest_Web)
        {
            try
            {

                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_GetInvoiceDocument");
                organizationRequest["logoInvoiceNumber"] = Convert.ToString(generateInvoiceDocumentRequest_Web.logoInvoiceNumber);

                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                var parsedResponse = Convert.ToString(response.Results["logoInvoiceResponse"]);

                return new GenerateInvoiceDocumentResponse_Web
                {
                    documentContent = parsedResponse,
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new GenerateInvoiceDocumentResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/executecouponcodeoperations")]
        public CouponCodeOperationsResponse_Web executeCouponCodeOperations([FromBody] CouponCodeOperationsRequest_Web couponCodeOperationsRequest_Web)
        {
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(couponCodeOperationsRequest_Web.reservationId))
            {
                string reservationId = Convert.ToString(couponCodeOperationsRequest_Web.reservationId);
                reservationId = reservationId + "ECCO";
                loggerHelper = new LoggerHelper(reservationId);
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                string message;
                loggerHelper.traceInfo(JsonConvert.SerializeObject(couponCodeOperationsRequest_Web));
                CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
                XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);

                CouponCodeRepository couponCodeRepository = new CouponCodeRepository(mongoDBHostName, mongoDBDatabaseName);
                var c = couponCodeRepository.getCouponCodeDetailsByCouponCode(couponCodeOperationsRequest_Web.couponCode);
                if (c.Count == 0)
                {
                    message = xrmHelper.GetXmlTagContentByGivenLangId("CouponCodeIsNotFound", couponCodeOperationsRequest_Web.langId, this.couponCodeXmlPath);
                }
                else
                {
                    loggerHelper.traceInfo(JsonConvert.SerializeObject(c));
                    var couponCode = c.Where(x => x.statusCode == (int)GlobalEnums.CouponCodeStatusCode.Generated).FirstOrDefault();
                    if (couponCode != null || !string.IsNullOrWhiteSpace(couponCode.couponCode))
                    {
                        couponCodeOperationsRequest_Web.statusCode = (int)GlobalEnums.CouponCodeStatusCode.Generated;
                    }
                    else
                    {
                        couponCodeOperationsRequest_Web.statusCode = c.FirstOrDefault().statusCode;
                    }


                    VirtualBranchRepository virtualBranchRepository = new VirtualBranchRepository(mongoDBHostName, mongoDBDatabaseName);
                    var branch = virtualBranchRepository.getBranchByVirtualBranchId(couponCodeOperationsRequest_Web.pickupBranchId).branch;

                    couponCodeOperationsRequest_Web.pickupBranchId = branch;
                    if (!couponCodeOperationsRequest_Web.groupCodeInformationId.HasValue)
                    {
                        couponCodeOperationsRequest_Web.groupCodeInformationId = Guid.Empty;
                    }

                    loggerHelper.traceInfo(JsonConvert.SerializeObject(couponCodeOperationsRequest_Web));
                    OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CouponCodeOperations");
                    organizationRequest["CouponCodeOperationsParameter"] = JsonConvert.SerializeObject(couponCodeOperationsRequest_Web);
                    organizationRequest["langId"] = couponCodeOperationsRequest_Web.langId;

                    var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                    var parsedResponse = JsonConvert.DeserializeObject<CouponCodeOperationsResponse>(Convert.ToString(response.Results["CouponCodeOperationsResponse"]));
                    if (!parsedResponse.ResponseResult.Result)
                    {
                        message = parsedResponse.ResponseResult.ExceptionDetail;
                    }
                    else
                    {
                        if (parsedResponse.couponCodeData != null)
                        {
                            MongoDBHelper.Repository.CouponCodeDefinitionRepository codeDefinitionRepository = new MongoDBHelper.Repository.CouponCodeDefinitionRepository(mongoDBHostName, mongoDBDatabaseName);
                            var definition = codeDefinitionRepository.getCouponCodeDefinitionById(parsedResponse.couponCodeData.couponCodeDefinitionId);

                            BusinessLibrary.Repository.CouponCodeDefinitionRepository couponCodeDefinitionRepository = new BusinessLibrary.Repository.CouponCodeDefinitionRepository(crmServiceHelper.IOrganizationService);
                            var crmDefinition = couponCodeDefinitionRepository.getCouponCodeDefinitionById(new Guid(parsedResponse.couponCodeData.couponCodeDefinitionId));
                            return new CouponCodeOperationsResponse_Web
                            {
                                couponCodeName = crmDefinition.GetAttributeValue<string>("rnt_name"),
                                couponCodeData = parsedResponse.couponCodeData,
                                definitionType = crmDefinition.GetAttributeValue<OptionSetValue>("rnt_type").Value,// ratio - amount
                                definitionPayLaterDiscountValue = crmDefinition.GetAttributeValue<decimal>("rnt_paylaterdiscountvalue"),
                                definitionPayNowDiscountValue = crmDefinition.GetAttributeValue<decimal>("rnt_paynowdiscountvalue"),
                                groupCodes = definition.groupCodeInformations,
                                responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                            };
                        }
                        message = xrmHelper.GetXmlTagContentByGivenLangId("SystemError", couponCodeOperationsRequest_Web.langId, this.webErrorMessagePath);
                    }
                }
                return new CouponCodeOperationsResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(message)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("executeCouponCodeOperations error : " + ex.Message);
                return new CouponCodeOperationsResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/setreservation3dpaymentreturn")]
        public Reservation3dPaymentReturnResponse_Web setReservation3dPaymentReturn(Reservation3dPaymentReturnRequest_Web reservation3DPaymentReturnRequest_Web)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            loggerHelper.traceInfo("reservation3DPaymentReturnRequest_Web Parameter : " + JsonConvert.SerializeObject(reservation3DPaymentReturnRequest_Web));
            if (reservation3DPaymentReturnRequest_Web.reservationId == Guid.Empty)
            {
                return new Reservation3dPaymentReturnResponse_Web()
                { responseResult = new ClassLibrary._Web.ResponseResult() { result = false, exceptionDetail = "Rezervasyon id boş olamaz." } };
            }
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            try
            {
                loggerHelper.traceInfo("create return parameters");
                Payment3dReturnParameters payment3DReturnParameters = new Payment3dReturnParameters()
                {
                    reservationId = reservation3DPaymentReturnRequest_Web.reservationId,
                    contractId = Guid.Empty,
                    paymentId = Guid.Empty,
                    providerPaymentId = reservation3DPaymentReturnRequest_Web.providerPaymentId,
                    conversationData = reservation3DPaymentReturnRequest_Web.conversationData,
                    conversationId = reservation3DPaymentReturnRequest_Web.conversationId,
                    success = reservation3DPaymentReturnRequest_Web.success == 1 ? true : false,// ? (int)ReservationEnums.StatusCode.New : (int)ReservationEnums.StatusCode.Waitingfor3D,
                    detail = reservation3DPaymentReturnRequest_Web.detail
                };
                loggerHelper.traceInfo("create organizationRequest");
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_Execute3dPaymentReturn");
                loggerHelper.traceInfo("action parameters " + JsonConvert.SerializeObject(payment3DReturnParameters));
                organizationRequest["Payment3dReturnParameters"] = JsonConvert.SerializeObject(payment3DReturnParameters);

                loggerHelper.traceInfo("execute plugin");
                var res = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var reservationResponse = JsonConvert.DeserializeObject<ReservationCreateResponse>(Convert.ToString(res.Results["ReservationPaymentReturnResponse"]));
                loggerHelper.traceInfo(Convert.ToString(res.Results["ReservationPaymentReturnResponse"]));
                loggerHelper.traceInfo("end plugin");
                var createResponse_Web = new Reservation3dPaymentReturnResponse_Web();
                if (reservationResponse.ResponseResult != null && reservationResponse.ResponseResult.Result)
                {
                    createResponse_Web.responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess();
                }
                else
                {
                    createResponse_Web.responseResult = ClassLibrary._Web.ResponseResult.ReturnError("İşlem onaylanmadı. Kart bilgilerini kontrol edin veya bankanız ile iletişime geçiniz.");
                }
                createResponse_Web.Map(reservationResponse);

                return createResponse_Web;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("setReservation3dPaymentReturn error : " + ex.Message);
                return new Reservation3dPaymentReturnResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/web/executeCustomerAnontmization")]
        public IndividualCustomerAnonymizationResponse executeCustomerAnontmization([FromBody] IndividualCustomerAnonymizationRequest customer)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();
            try
            {
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL();
                DateTime expireRequestDate = DateTime.UtcNow.AddHours(+3);
                DateTime expireDate = individualCustomerBL.RequestIndividualCustomerAnonymization(customer.individualCustomerId, expireRequestDate, customer.langId);

                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper_Web.IOrganizationService);
                Entity contact = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(customer.individualCustomerId, new string[] { "fullname" });
                string fullName = contact.GetAttributeValue<string>("fullname");

                XrmHelper xrmHelper = new XrmHelper(crmServiceHelper_Web.IOrganizationService);
                return new IndividualCustomerAnonymizationResponse
                {
                    anonymizationMessage = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("AnonymizationRequest", customer.langId, this.webErrorMessagePath), fullName),
                    expireRequestDate = expireRequestDate,
                    expireDate = expireDate,
                    ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("executeCustomerAnontmization error : " + ex.Message);
                return new IndividualCustomerAnonymizationResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/web/cancelCustomerAnontmization")]
        public IndividualCustomerAnonymizationResponse cancelCustomerAnontmization([FromBody] IndividualCustomerAnonymizationRequest customer)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Web crmServiceHelper_Web = new CrmServiceHelper_Web();
            try
            {
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL();
                individualCustomerBL.CancelIndividualCustomerAnonymization(customer.individualCustomerId);

                XrmHelper xrmHelper = new XrmHelper(crmServiceHelper_Web.IOrganizationService);
                return new IndividualCustomerAnonymizationResponse
                {
                    anonymizationMessage = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("CancelAnonymizationRequest", customer.langId, this.webErrorMessagePath)),
                    ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("cancelCustomerAnontmization error : " + ex.Message);
                return new IndividualCustomerAnonymizationResponse
                {
                    ResponseResult = ClassLibrary.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/web/checkreservastionpaymentstatusfor3d")]
        public ReservationPaymentCheck3dStatusResponse_Web checkReservastionPaymentStatusFor3D(ReservationPaymentCheck3dStatusRequest_Web reservationPaymentCheck3DStatusRequest_Web)
        {
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            ReservationRepository reservationRepository = new ReservationRepository(crmServiceHelper.IOrganizationService);
            Entity reservation = null;

            if (reservationPaymentCheck3DStatusRequest_Web.reservationId != null)
                reservation = reservationRepository.getReservationById(new Guid(reservationPaymentCheck3DStatusRequest_Web.reservationId));
            else if (reservationPaymentCheck3DStatusRequest_Web.PNRNumber != null)
                reservation = reservationRepository.getReservationByPnrNumber(reservationPaymentCheck3DStatusRequest_Web.PNRNumber);

            if (reservation == null)
            {
                return new ReservationPaymentCheck3dStatusResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError("reservationId or PNR Number Wrong!")
                };
            }


            var reservationStatusCode = reservation.GetAttributeValue<OptionSetValue>("statuscode").Value;
            if (reservationStatusCode == (int)rnt_reservation_StatusCode.New)
            {
                return new ReservationPaymentCheck3dStatusResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnSuccess()
                };
            }
            else if (reservationStatusCode == (int)rnt_reservation_StatusCode.Waitingfor3D)
            {
                return new ReservationPaymentCheck3dStatusResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError("Reservation status is WaitingFor3D")
                };
            }
            else
            {
                return new ReservationPaymentCheck3dStatusResponse_Web
                {
                    responseResult = ClassLibrary._Web.ResponseResult.ReturnError($"Reservation status is {reservationStatusCode.ToString()}")
                };
            }
        }
    }
}
