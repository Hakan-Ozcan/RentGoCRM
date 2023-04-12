using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Mobile.Account;
using RntCar.ClassLibrary._Mobile.AdditionalProductRules;
using RntCar.ClassLibrary._Mobile.DeviceToken;
using RntCar.ClassLibrary._Mobile.MarketingPermission;
using RntCar.ClassLibrary._Web;
using RntCar.ClassLibrary.MarkettingPermission;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.Reservation;
using RntCar.ExternalServices.Security;
using RntCar.Logger;
using RntCar.MongoDBHelper.Entities;
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
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using CampaignRepository = RntCar.MongoDBHelper.Repository.CampaignRepository;

namespace RntCar.ExternalServicesMAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*", exposedHeaders: "X-My-Header")]
    //[BasicHttpAuthorizeAttribute("Mobile")]
    public class MobileController : ApiController
    {
        private string mongoDBHostName { get; set; }
        private string mongoDBDatabaseName { get; set; }
        private string mobileErrorMessagePath { get; set; }
        private string couponCodeXmlPath { get; set; }

        private readonly FirebaseMessaging messaging;

        public MobileController()
        {
            mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
            mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");
            mobileErrorMessagePath = new HandlerBase().mobileXmlPath;
            couponCodeXmlPath = new HandlerBase().couponCodeXmlPath;
            System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", HttpContext.Current.Server.MapPath("~") + @"/Security/rentgo-7b5a5-firebase-adminsdk-5h9bz-e1ea0b0deb.json");
            try
            {
                var app = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.GetApplicationDefault()
                });
                messaging = FirebaseMessaging.GetMessaging(app);
            }
            catch
            {
            }
        }

        [HttpGet]
        [Route("api/mobile/testme")]
        public string testme()
        {
            return "ok";
        }

        [HttpPost]
        [Route("api/mobile/getReservationDetail")]
        public ReservationDetailResponse_Mobile GetReservationDetail(GetReservationDetailRequest_Mobile getReservationDetailRequest_Mobile)
        {
            CrmServiceHelper_Mobile crmServiceHelper_Mobile = new CrmServiceHelper_Mobile();
            try
            {
                //Will develop mongo
                BusinessLibrary.Repository.ReservationItemRepository reservationItemRepository = new BusinessLibrary.Repository.ReservationItemRepository(crmServiceHelper_Mobile.IOrganizationService);
                ReservationRepository reservationRepository = new ReservationRepository(crmServiceHelper_Mobile.IOrganizationService);
                Entity reservation = null;

                if (getReservationDetailRequest_Mobile.reservationId != null)
                    reservation = reservationRepository.getReservationById(new Guid(getReservationDetailRequest_Mobile.reservationId));
                else if (getReservationDetailRequest_Mobile.PNRNumber != null)
                    reservation = reservationRepository.getReservationByPnrNumber(getReservationDetailRequest_Mobile.PNRNumber);

                if (reservation == null)
                {
                    return new ReservationDetailResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError("reservationId or PNR Number Wrong!")
                    };
                }

                var reservationItems = reservationItemRepository.getActiveReservationItemsByReservationIdWithAdditionalProduct(reservation.Id);
                var response = new ReservationMapper().buildMobileReservationDetail(reservation, reservationItems);

                return new ReservationDetailResponse_Mobile
                {
                    reservationDetail = response,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new ReservationDetailResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/setdevicetoken")]
        public DeviceTokenResponse_Mobile SetDeviceToken(DeviceTokenRequest_Mobile deviceTokenRequest_Mobile)
        {
            try
            {
                if (string.IsNullOrEmpty(deviceTokenRequest_Mobile.deviceToken))
                {
                    return new DeviceTokenResponse_Mobile()
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError("Device Token is required!")
                    };
                }
                DeviceTokenBusiness deviceTokenBusiness = new DeviceTokenBusiness(mongoDBHostName, mongoDBDatabaseName);
                DeviceTokenRepository deviceTokenRepository = new DeviceTokenRepository(mongoDBHostName, mongoDBDatabaseName);
                MongoDBResponse response;

                var deviceToken = new DeviceTokenData()
                {
                    token = deviceTokenRequest_Mobile.deviceToken,
                    uId = deviceTokenRequest_Mobile.userId != null ? deviceTokenRequest_Mobile.userId : string.Empty
                };
                var deviceTokenData = deviceTokenRepository.GetDeviceTokenDataByDeviceToken(deviceToken.token);

                if (deviceTokenData == null)
                    response = deviceTokenBusiness.CreateDeviceToken(deviceToken);
                else
                    response = deviceTokenBusiness.UpdateDeviceToken(deviceToken, deviceTokenData._id.ToString());

                return new DeviceTokenResponse_Mobile()
                {
                    deviceToken = response.Result ? deviceToken.token : string.Empty,
                    id = response.Id,
                    responseResult = response.Result ? ClassLibrary._Mobile.ResponseResult.ReturnSuccess() : ClassLibrary._Mobile.ResponseResult.ReturnError(response.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {
                return new DeviceTokenResponse_Mobile()
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/getnotifications")]
        public GetNotificationResponse_Mobile GetNotifications(GetNotificationRequest_Mobile notificationRequest_Mobile)
        {
            NotificationBusiness notificationBusiness = new NotificationBusiness(mongoDBHostName, mongoDBDatabaseName);
            try
            {
                if (string.IsNullOrEmpty(notificationRequest_Mobile.deviceToken))
                {
                    return new GetNotificationResponse_Mobile()
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError("Device Token is required!")
                    };
                }
                var notifications = new List<NotificationData>();
                NotificationRepository notificationRepository = new NotificationRepository(mongoDBHostName, mongoDBDatabaseName);
                var notificationsByToken = notificationRepository.GetNotificationsByDeviceToken(notificationRequest_Mobile.deviceToken);
                var publicNottifications = notificationRepository.GetPublicNotifications();
                var result = notificationsByToken.Concat(publicNottifications).ToList();

                //genel tipli olanlar
                foreach (var notification in result)
                {
                    var notify = new NotificationData()
                    {
                        deviceToken = notification.dTkn,
                        isRead = notification.isRead,
                        notificationContent = notification.nCnt,
                        notificationType = notification.nType,
                        regardingObjectId = notification.rObjId,
                        regardingObjectName = notification.rObjName
                    };

                    notifications.Add(notify);
                }

                return new GetNotificationResponse_Mobile()
                {
                    notifications = notifications,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new GetNotificationResponse_Mobile()
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/sendNotification")]
        public async Task<SendNotificationResponse_Mobile> SendNotificationAsync(SendNotificationRequest_Mobile notificationRequest_Mobile)
        {
            if (notificationRequest_Mobile.deviceTokens == null || notificationRequest_Mobile.notification == null)
            {
                return new SendNotificationResponse_Mobile()
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError("All parameter is required!")
                };
            }

            var message = new MulticastMessage()
            {
                Notification = new Notification()
                {
                    Body = notificationRequest_Mobile.notification.notificationContent
                },
                Tokens = notificationRequest_Mobile.deviceTokens,
            };
            // to do will paging
            var response = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);

            if (response.FailureCount == notificationRequest_Mobile.deviceTokens.Count)
            {
                return new SendNotificationResponse_Mobile()
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError("Failed!Process successful but device token has error!")
                };
            }

            NotificationBusiness notificationBusiness = new NotificationBusiness(mongoDBHostName, mongoDBDatabaseName);
            var notification = notificationRequest_Mobile.notification;

            //todo will add condition notifaction type  and enums
            if (notificationRequest_Mobile.notification.notificationType == 1000) // 1000 eq Public notification
            {
                notificationBusiness.CreateNotification(notification);
            }
            else
            {
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    if (response.Responses[i].IsSuccess)
                    {
                        notification.deviceToken = notificationRequest_Mobile.deviceTokens[i];
                        var result = notificationBusiness.CreateNotification(notification);
                    }
                }
            }

            return new SendNotificationResponse_Mobile()
            {
                FailureCount = response.FailureCount,
                SuccessCount = response.SuccessCount,
                responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
            };
        }
        [HttpPost]
        [Route("api/mobile/getmasterdata")]
        public GetMasterDataResponse_Mobile getMasterData(GetMasterDataRequest_Mobile masterDataRequest_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();

            try
            {
                loggerHelper.traceInfo("getMasterData start :" + JsonConvert.SerializeObject(masterDataRequest_Mobile));

                #region Parameters
                List<Task> _tasks = new List<Task>();
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
                List<GroupCodeInformation_Mobile> _groupCodeInformation = new List<GroupCodeInformation_Mobile>();
                List<GroupCodeImage_Mobile> _groupCodeImages = new List<GroupCodeImage_Mobile>();
                List<ShowRoomProduct_Mobile> _showroomProduct = new List<ShowRoomProduct_Mobile>();
                List<CountryData> _countries = new List<CountryData>();
                List<CityData> _cities = new List<CityData>();
                List<DistrictData> _districts = new List<DistrictData>();
                List<TaxOfficeData> _taxOffice = new List<TaxOfficeData>();
                List<Branch_Mobile> _branchs = new List<Branch_Mobile>();
                List<WorkingHour_Mobile> _workingHour = new List<WorkingHour_Mobile>();
                List<AdditionalProductRule_Mobile> _additionalProductRules = new List<AdditionalProductRule_Mobile>();
                #endregion

                #region district task
                var districtTask = new Task(() =>
                {
                    loggerHelper.traceInfo("district start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("district_cachekey");

                    DistrictCacheClient districtCacheClient = new DistrictCacheClient(crmServiceHelper.IOrganizationService,
                                                                                      cacheKey);
                    _districts = districtCacheClient.getDistricts();

                    loggerHelper.traceInfo("district end");
                });
                _tasks.Add(districtTask);
                districtTask.Start();
                #endregion

                #region show room task
                var showRoomTask = new Task(() =>
                {
                    loggerHelper.traceInfo("Show room start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("groupcode_cacheKey");

                    ShowRoomProductCacheClient showRoomProductCacheClient = new ShowRoomProductCacheClient(crmServiceHelper.IOrganizationService, cacheKey + "_showRoom"+ masterDataRequest_Mobile.langId);
                    var showRoomData = showRoomProductCacheClient.getAllShowRoomDetailCache(masterDataRequest_Mobile.langId);
                    _showroomProduct = new ShowRoomProductMapper().createMobileShowRoomProductList(showRoomData);

                    loggerHelper.traceInfo("show room end");
                });
                _tasks.Add(showRoomTask);
                showRoomTask.Start();
                #endregion

                #region GroupCodes Task
                var groupCodeInformationTask = new Task(() =>
                {
                    loggerHelper.traceInfo("groupCodeInformationTask start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("groupcode_cacheKey");
                    var segmentCacheKey = configurationBL.GetConfigurationByName("segment_cacheKey");


                    GroupCodeInformationCacheClient groupCodeInformationCacheClient = new GroupCodeInformationCacheClient(crmServiceHelper.IOrganizationService, cacheKey);

                    var groupCodeData = groupCodeInformationCacheClient.getAllGroupCodeInformationDetailCache(masterDataRequest_Mobile.langId);
                    var segmentNameData = groupCodeInformationCacheClient.getSegmentNameCache(segmentCacheKey, masterDataRequest_Mobile.langId);
                    _groupCodeInformation = new GroupCodeInformationMapper().createMobileGroupCodeList(groupCodeData, segmentNameData, masterDataRequest_Mobile.langId);

                    loggerHelper.traceInfo("groupCodeInformationTask end");
                });
                _tasks.Add(groupCodeInformationTask);
                groupCodeInformationTask.Start();
                #endregion

                #region GroupCodeImages Task
                var groupCodeImagesTask = new Task(() =>
                {
                    loggerHelper.traceInfo("groupCodeImagesTask start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var groupCodeImageCacheKey = configurationBL.GetConfigurationByName("groupcodeimage_cacheKey");

                    GroupCodeInformationCacheClient groupCodeInformationCacheClient = new GroupCodeInformationCacheClient(crmServiceHelper.IOrganizationService);
                    var groupCodeImages = groupCodeInformationCacheClient.GetGroupCodeImageDatas(groupCodeImageCacheKey);

                    _groupCodeImages = new GroupCodeInformationMapper().createMobileGroupCodeImageList(groupCodeImages);

                    loggerHelper.traceInfo("groupCodeImagesTask end");
                });

                _tasks.Add(groupCodeImagesTask);
                groupCodeImagesTask.Start();
                #endregion

                #region Countries Task
                var countriesTask = new Task(() =>
                {
                    loggerHelper.traceInfo("countriesTask start");
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("country_cacheKey");

                    CountryCacheClient countryCacheClient = new CountryCacheClient(crmServiceHelper.IOrganizationService, cacheKey);

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
                    //todo will make a fix
                    var branchs = branchCacheClient.getBranchCache("mobile", (int)rnt_ReservationChannel.Mobile, true);
                    _branchs = new BranchMapper().createMobileBranchList(branchs);
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
                    var workingHours = workingHourCacheClient.getWorkingHourCache("mobile", (int)rnt_ReservationChannel.Mobile, true);
                    _workingHour = new WorkingHourMapper().createMobileWorkingHourList(workingHours);
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
                    _additionalProductRules = additionalProductRuleCacheClient.getMobileAdditionalProductRuleCache();
                    loggerHelper.traceInfo("additionalProductRule end");

                });
                _tasks.Add(additionalProductRule);
                additionalProductRule.Start();
                #endregion

                Task.WaitAll(_tasks.ToArray());

                return new GetMasterDataResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess(),
                    branchs = _branchs.OrderBy(x => x.branchName).ToList(),
                    cities = _cities,
                    countries = _countries,
                    groupCodeInformation = _groupCodeInformation,
                    groupCodeImages = _groupCodeImages,
                    workingHours = _workingHour,
                    additionalProductRules = _additionalProductRules,
                    districts = _districts,
                    showRoomProducts = _showroomProduct,
                    taxOffices = _taxOffice
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getMasterData error: " + ex.Message);

                return new GetMasterDataResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/updatemarketingpermission")]
        public MarketingPermissionResponse_Mobile marketingPermission([FromBody] MarketingPermissionParameters_Mobile marketingPermissionParamaters)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            loggerHelper.traceInputsInfo<MarketingPermissionParameters_Mobile>(marketingPermissionParamaters);

            try
            {
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_UpdateMarketingPermission");
                organizationRequest["contactId"] = Convert.ToString(marketingPermissionParamaters.contactId);
                organizationRequest["emailPermission"] = marketingPermissionParamaters.emailPermission;
                organizationRequest["notificationPermission"] = marketingPermissionParamaters.notificationPermission;
                organizationRequest["smsPermission"] = marketingPermissionParamaters.smsPermission;
                organizationRequest["channelCode"] = (int)ClassLibrary._Enums_1033.rnt_PermissionChannelCode.Mobile;
                organizationRequest["operationType"] = (int)rnt_marketingpermissions_rnt_operationtype.MyAccountUpdate;

                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                var parsedResponse = JsonConvert.DeserializeObject<UpdatingMarketingPermissionResponse>(Convert.ToString(response.Results["UpdatingMarketingPermissionResponse"]));

                return new MarketingPermissionResponse_Mobile()
                {
                    marketingPermissionId = parsedResponse.marketingPermissionId,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new MarketingPermissionResponse_Mobile()
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/login")]
        public LoginResponse_Mobile login([FromBody] LoginParameters_Mobile loginParameters_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            loggerHelper.traceInputsInfo<LoginParameters_Mobile>(loginParameters_Mobile);

            try
            {
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
                #region Individual Customer Retrieve
                loggerHelper.traceInfo("individual customer retrieve start");
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var individualCustomer = individualCustomerRepository.getMobileUserInformation(loginParameters_Mobile.emailaddress, loginParameters_Mobile.password);
                loggerHelper.traceInfo("individual customer retrieve end");

                if (individualCustomer == null)
                {
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    return new LoginResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(xrmHelper.GetXmlTagContentByGivenLangId("MissingUser", loginParameters_Mobile.langId, mobileErrorMessagePath))
                    };
                }
                #endregion

                List<Task> _tasks = new List<Task>();
                var _marketingPermissions = new MarketingPermission_Mobile();
                var _individualAddress = new List<IndividualAddressData_Mobile>();
                var _invoiceAddress = new List<InvoiceAddressData_Mobile>();
                var _customerCreditCards = new List<CreditCardData_Mobile>();
                var _customerAccounts = new List<CustomerAccountData_Mobile>();

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
                            var accountDatas = new AccountMapper().createMobileAccountData(account, relation);
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
                        _marketingPermissions = new MarketingPermissionMapper().createMobileMarketingPermissionData(marketingPermissions);
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
                    _individualAddress = new IndividualAddressMapper().createMobileIndividualAddressList(individualAddress);
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
                    _invoiceAddress = new InvoiceAddressMapper().createMobileInvoiceAddressList(invoiceAddress);
                    loggerHelper.traceInfo("invoiceAddressTask retrieve end");

                });
                _tasks.Add(invoiceAddressTask);
                invoiceAddressTask.Start();
                #endregion

                #region Customer Credit Card

                var customerCreditCardTask = new Task(() =>
                {
                    loggerHelper.traceInfo("customerCreditCard retrieve start");
                    CreditCardRepository creditCardRepository = new CreditCardRepository(crmServiceHelper.IOrganizationService);
                    var columns = new string[]
                    {
                        "rnt_creditcardnumber",
                        "rnt_carduserkey",
                        "rnt_cardtoken",
                        "rnt_expiremonthcode",
                        "rnt_expireyear",
                        "rnt_cardtypecode",
                        "rnt_bank",
                        "rnt_name"
                    };
                    var creditCards = creditCardRepository.getCreditCardsByCustomerId(individualCustomer.Id, columns);
                    _customerCreditCards = new CreditCardMapper().createMobileCreditCardList(creditCards);
                    loggerHelper.traceInfo("customerCreditCard retrieve end");

                });
                _tasks.Add(customerCreditCardTask);
                customerCreditCardTask.Start();

                #endregion
                Task.WaitAll(_tasks.ToArray());

                LoginResponse_Mobile loginResponse_Mobile = new LoginResponse_Mobile();
                loginResponse_Mobile.customerAccounts = _customerAccounts;
                loginResponse_Mobile.customerCreditCards = _customerCreditCards;
                loginResponse_Mobile.customerInformation = new IndividualCustomerMapper().createMobileIndividualCustomerData(individualCustomer, loginParameters_Mobile.langId);
                loginResponse_Mobile.individualAddressInformation = _individualAddress;
                loginResponse_Mobile.invoiceAddressInformation = _invoiceAddress;
                loginResponse_Mobile.marketingPermission = _marketingPermissions;
                loginResponse_Mobile.responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess();
                return loginResponse_Mobile;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("login error : " + ex.Message);
                return new LoginResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/getindividualcustomerdetail")]
        public LoginResponse_Mobile getIndividualCustomerDetail([FromBody] LoginParameters_Mobile loginParameters_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            loggerHelper.traceInputsInfo<LoginParameters_Mobile>(loginParameters_Mobile);
            try
            {
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
                #region Individual Customer Retrieve
                loggerHelper.traceInfo("individual customer retrieve start");
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                Entity individualCustomer = null;
                if (!string.IsNullOrEmpty(loginParameters_Mobile.emailaddress))
                {
                    individualCustomer = individualCustomerRepository.getIndividualCustomerByMobileEmailAddress(loginParameters_Mobile.emailaddress, new string[] { });
                }
                else if (!string.IsNullOrEmpty(loginParameters_Mobile.mobilePhone))
                {
                    individualCustomer = individualCustomerRepository.getCustomerByMobilePhoneWithGivenColumns(loginParameters_Mobile.mobilePhone, new string[] { });
                    if (individualCustomer != null && string.IsNullOrEmpty(individualCustomer.GetAttributeValue<string>("rnt_webemailaddress")))
                    {
                        individualCustomer = null;
                    }
                }

                loggerHelper.traceInfo("individual customer retrieve end");

                if (individualCustomer == null)
                {
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    return new LoginResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(xrmHelper.GetXmlTagContentByGivenLangId("MissingUser", loginParameters_Mobile.langId, mobileErrorMessagePath))
                    };
                }
                #endregion

                List<Task> _tasks = new List<Task>();
                var _marketingPermissions = new MarketingPermission_Mobile();
                var _individualAddress = new List<IndividualAddressData_Mobile>();
                var _invoiceAddress = new List<InvoiceAddressData_Mobile>();
                var _customerCreditCards = new List<CreditCardData_Mobile>();

                #region Marketing Permission Task
                var marketingPermissionTask = new Task(() =>
                {
                    loggerHelper.traceInfo("markettingPermissionsRepository retrieve start");
                    MarkettingPermissionsRepository markettingPermissionsRepository = new MarkettingPermissionsRepository(crmServiceHelper.IOrganizationService);
                    var marketingPermissions = markettingPermissionsRepository.getMarkettingPermissionByContactId(individualCustomer.Id);
                    if (marketingPermissions != null)
                        _marketingPermissions = new MarketingPermissionMapper().createMobileMarketingPermissionData(marketingPermissions);
                    loggerHelper.traceInfo("markettingPermissionsRepository retrieve end");

                });
                _tasks.Add(marketingPermissionTask);
                marketingPermissionTask.Start();
                #endregion

                #region IndividualAdress Task
                var individualAddressTask = new Task(() =>
                {
                    loggerHelper.traceInfo("individualAddress retrivie start");
                    IndividualAddressRepository individualAddressRepository = new IndividualAddressRepository(crmServiceHelper.IOrganizationService);
                    var individualAddress = individualAddressRepository.getIndividualAddressesByCustomerId(individualCustomer.Id);
                    _individualAddress = new IndividualAddressMapper().createMobileIndividualAddressList(individualAddress);

                    loggerHelper.traceInfo("individualAddress retrivie end");
                });
                _tasks.Add(individualAddressTask);
                individualAddressTask.Start();
                #endregion

                #region Invoice Address
                var invoiceAddressTask = new Task(() =>
                {
                    loggerHelper.traceInfo("invoiceAddressTask retrieve start");
                    InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(crmServiceHelper.IOrganizationService);
                    string[] columns = new string[]
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
                    _invoiceAddress = new InvoiceAddressMapper().createMobileInvoiceAddressList(invoiceAddress);
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
                        "rnt_creditcardnumber",
                        "rnt_carduserkey",
                        "rnt_cardtoken",
                        "rnt_expiremonthcode",
                        "rnt_expireyear",
                        "rnt_cardtypecode",
                        "rnt_bank",
                        "rnt_name"
                    };

                    var creditCards = creditCardRepository.getCreditCardsByCustomerId(individualCustomer.Id, columns);
                    _customerCreditCards = new CreditCardMapper().createMobileCreditCardList(creditCards);
                    loggerHelper.traceInfo("customerCreditCard retrieve end");
                });
                _tasks.Add(customerCreditCard);
                customerCreditCard.Start();

                #endregion

                Task.WaitAll(_tasks.ToArray());

                LoginResponse_Mobile loginResponse_Mobile = new LoginResponse_Mobile();
                loginResponse_Mobile.responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess();
                loginResponse_Mobile.customerInformation = new IndividualCustomerMapper().createMobileIndividualCustomerData(individualCustomer, loginParameters_Mobile.langId);
                loginResponse_Mobile.marketingPermission = _marketingPermissions;
                loginResponse_Mobile.individualAddressInformation = _individualAddress;
                loginResponse_Mobile.invoiceAddressInformation = _invoiceAddress;
                loginResponse_Mobile.customerCreditCards = _customerCreditCards;
                return loginResponse_Mobile;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("individual customer detail error" + ex.Message);

                return new LoginResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/createcreditcard")]
        public CreateCreditCardResponse_Mobile createCreditCard(CreateCreditCardParameters_Mobile creditCardParameters_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
            try
            {
                loggerHelper.traceInfo("CreateCreditCardParameters_Mobile : " + JsonConvert.SerializeObject(creditCardParameters_Mobile));
                var creditCardParamters = new CreditCardMapper().buildCreateCreditCardParameters(creditCardParameters_Mobile);

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CreateCustomerCreditCard");
                organizationRequest["creditCardParameters"] = Convert.ToString(JsonConvert.SerializeObject(creditCardParamters));

                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                var parsedResponse = JsonConvert.DeserializeObject<CreateCreditCardResponse>(Convert.ToString(response.Results["ExecutionResult"]));

                var creditCardResponse = new CreditCardMapper().buildCreditCardResponse_Mobile(parsedResponse);
                creditCardResponse.responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess();

                return creditCardResponse;
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("createCustomer error : " + ex.Message);
                //todo make a generic method
                var splitted = ex.Message.Split(new string[] { "System.Exception:" }, StringSplitOptions.None);

                return new CreateCreditCardResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(splitted.Length == 1 ? splitted[0] : splitted[1])
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/createcustomer")]
        public IndividualCustomerCreateResponse_Mobile createCustomer(IndividualCustomerCreateParameter_Mobile individualCustomerCreateParameter_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
            try
            {
                loggerHelper.traceInfo("IndividualCustomerCreateParameter_Mobile : " + JsonConvert.SerializeObject(individualCustomerCreateParameter_Mobile));

                var isTurkishCitizen = true;
                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid");
                if (new Guid(turkeyGuid.Split(';')[0]) != individualCustomerCreateParameter_Mobile.citizenShipId)
                {
                    isTurkishCitizen = false;
                }
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var result = individualCustomerRepository.getExistingCustomerIdByGovernmentIdOrPassportNumberOrEmailAddress(individualCustomerCreateParameter_Mobile.governmentId,
                                                                                                                            individualCustomerCreateParameter_Mobile.emailAddress,
                                                                                                                            new string[] { "rnt_webemailaddress", "rnt_webpassword", "rnt_distributionchannelcode" });


                if (result == null)
                {
                    var customerParameter = new IndividualCustomerMapper().buildCreateIndividualCustomerParameter(individualCustomerCreateParameter_Mobile, isTurkishCitizen);
                    customerParameter.distributionChannelCode = (int)rnt_DistributionChannelCode.Mobile;
                    OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CreateIndividualCustomerandAddress");
                    organizationRequest["LangId"] = Convert.ToString(individualCustomerCreateParameter_Mobile.langId);
                    organizationRequest["CustomerInformation"] = JsonConvert.SerializeObject(customerParameter);
                    var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);


                    var parsedResponse = JsonConvert.DeserializeObject<ClassLibrary.ResponseResult>(Convert.ToString(response.Results["ResponseResult"]));
                    if (parsedResponse.Result)
                    {
                        loggerHelper.traceInfo("response : " + JsonConvert.SerializeObject(parsedResponse));
                        return new IndividualCustomerCreateResponse_Mobile
                        {
                            responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess(),
                            individualAddressId = new Guid(Convert.ToString(response.Results["IndividualAddressId"])),
                            individualCustomerId = new Guid(Convert.ToString(response.Results["IndividualCustomerId"])),
                            invoiceAddressId = new Guid(Convert.ToString(response.Results["InvoiceAddressId"]))
                        };
                    }
                    return new IndividualCustomerCreateResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(parsedResponse.ExceptionDetail)
                    };
                }
                else if (result != null && (!result.Contains("rnt_webemailaddress") || !result.Contains("rnt_webpassword")))
                {
                    var customerParameter = new IndividualCustomerMapper().buildUpdateIndividualCustomerParameter(individualCustomerCreateParameter_Mobile, isTurkishCitizen);

                    customerParameter.individualCustomerId = result.Id;
                    customerParameter.distributionChannelCode = result.GetAttributeValue<OptionSetValue>("rnt_distributionchannelcode").Value;
                    IndividualAddressRepository individualAddressRepository = new IndividualAddressRepository(crmServiceHelper.IOrganizationService);
                    var address = individualAddressRepository.getIndividualAddressesByCustomerId(result.Id);

                    if (address.Count == 0)
                    {
                        IndividualAddressBL individualAddressBL = new IndividualAddressBL(crmServiceHelper.IOrganizationService);

                        var individualAdressParams = new IndividualAddressCreateParameters();
                        if (customerParameter.addressCity != null && customerParameter.addressCity.value != null)
                        {
                            individualAdressParams.addressCityId = new Guid(customerParameter.addressCity.value);
                        }
                        if (customerParameter.addressDistrict != null && customerParameter.addressDistrict.value != null)
                        {
                            individualAdressParams.addressDistrictId = new Guid(customerParameter.addressDistrict.value);
                        }
                        if (customerParameter.addressCountry != null && customerParameter.addressCountry.value != null)
                        {
                            individualAdressParams.addressCountryId = new Guid(customerParameter.addressCountry.value);
                        }
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
                        return new IndividualCustomerCreateResponse_Mobile
                        {
                            individualCustomerId = result.Id,
                            responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess(),
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
                    return new IndividualCustomerCreateResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(message)
                    };
                }
                return new IndividualCustomerCreateResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess(),
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("createCustomer error : " + ex.Message);
                //todo make a generic method
                var splitted = ex.Message.Replace("CustomErrorMessagefinder:", "").Split(new string[] { "System.Exception:" }, StringSplitOptions.None);

                return new IndividualCustomerCreateResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(splitted.Length == 1 ? splitted[0].TrimStart() : splitted[1].TrimStart())
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/updatecustomer")]
        public IndividualCustomerUpdateResponse_Mobile updateCustomer(IndividualCustomerUpdateParameter_Mobile individualCustomerUpdateParameter_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
            try
            {
                loggerHelper.traceInputsInfo<IndividualCustomerUpdateParameter_Mobile>(individualCustomerUpdateParameter_Mobile);

                var isTurkishCitizen = true;
                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var turkeyGuid = configurationBL.GetConfigurationByName("turkeyGuid");
                if (new Guid(turkeyGuid.Split(';')[0]) != individualCustomerUpdateParameter_Mobile.citizenShipId)
                {
                    isTurkishCitizen = false;
                }
                var customerParameter = new IndividualCustomerMapper().buildUpdatendividualCustomerParameter(individualCustomerUpdateParameter_Mobile, isTurkishCitizen);
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_UpdateIndividualCustomerandAddress");
                organizationRequest["LangId"] = individualCustomerUpdateParameter_Mobile.langId;
                organizationRequest["CustomerInformation"] = JsonConvert.SerializeObject(customerParameter);
                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                var parsedResponse = JsonConvert.DeserializeObject<IndividualCustomerUpdateResponse>(Convert.ToString(response.Results["ExecutionResult"]));
                if (parsedResponse.ResponseResult.Result)
                {
                    return new IndividualCustomerUpdateResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess(),
                    };
                }
                return new IndividualCustomerUpdateResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(parsedResponse.ResponseResult.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("updateCustomer error : " + ex.Message);
                //todo make a generic method
                var splitted = ex.Message.Replace("CustomErrorMessagefinder:", "").Split(new string[] { "System.Exception:" }, StringSplitOptions.None);

                return new IndividualCustomerUpdateResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(splitted.Length == 1 ? splitted[0].TrimStart() : splitted[1].TrimStart())
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/sendsmsvericationindividualcustomercreation")]
        public SendSmsVericationIndividualCustomerCreationResponse_Mobile sendSmsVericationIndividualCustomerCreation(SendSmsVericationIndividualCustomerCreationRequest_Mobile sendSmsVericationIndividualCustomerCreationRequest)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
            loggerHelper.traceInputsInfo<SendSmsVericationIndividualCustomerCreationRequest_Mobile>(sendSmsVericationIndividualCustomerCreationRequest);
            try
            {
                string dialCode = "90";
                string fullMobilePhone = dialCode + sendSmsVericationIndividualCustomerCreationRequest.mobilePhone;

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_GenerateAndSendSMS");
                organizationRequest["FirstName"] = sendSmsVericationIndividualCustomerCreationRequest.firstName;
                organizationRequest["LastName"] = sendSmsVericationIndividualCustomerCreationRequest.lastName;
                organizationRequest["MobilePhone"] = fullMobilePhone;
                organizationRequest["LangId"] = sendSmsVericationIndividualCustomerCreationRequest.langId;
                organizationRequest["SMSContentCode"] = (int)GlobalEnums.SmsContentCode.ContactCreateSms;
                organizationRequest["VerificationCode"] = sendSmsVericationIndividualCustomerCreationRequest.verificationCode;
                crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                return new SendSmsVericationIndividualCustomerCreationResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("sendSmsVericationIndividualCustomerCreation error : " + ex.Message);
                return new SendSmsVericationIndividualCustomerCreationResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [Route("api/mobile/sendsmsvericationindividualcustomerupdate")] //IBU
        public SendSmsVericationIndividualCustomerUpdateResponse_Mobile sendSmsVericationIndividualCustomerUpdate(SendSmsVericationIndividualCustomerUpdateRequest_Mobile sendSmsVericationIndividualCustomerUpdateRequest_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Web crmServiceHelper = new CrmServiceHelper_Web();
            loggerHelper.traceInputsInfo<SendSmsVericationIndividualCustomerUpdateRequest_Mobile>(sendSmsVericationIndividualCustomerUpdateRequest_Mobile);
            try
            {
                string dialCode = "90";

                string fullMobilePhone = dialCode + sendSmsVericationIndividualCustomerUpdateRequest_Mobile.mobilePhone;
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(sendSmsVericationIndividualCustomerUpdateRequest_Mobile.individualCustomerId, new string[] { "firstname", "lastname" });
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_GenerateAndSendSMS");
                organizationRequest["FirstName"] = customer.GetAttributeValue<string>("firstname");
                organizationRequest["LastName"] = customer.GetAttributeValue<string>("lastname");
                organizationRequest["MobilePhone"] = fullMobilePhone;
                organizationRequest["LangId"] = sendSmsVericationIndividualCustomerUpdateRequest_Mobile.langId;
                organizationRequest["SMSContentCode"] = (int)GlobalEnums.SmsContentCode.ContactUpdateSms;
                organizationRequest["VerificationCode"] = sendSmsVericationIndividualCustomerUpdateRequest_Mobile.verificationCode;
                crmServiceHelper.IOrganizationService.Execute(organizationRequest);

                return new SendSmsVericationIndividualCustomerUpdateResponse_Mobile()
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("sendSmsVericationIndividualCustomerUpdate error : " + ex.Message);
                return new SendSmsVericationIndividualCustomerUpdateResponse_Mobile()
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/calculateavailability")]
        public AvailabilityResponse_Mobile calculateAvailability([FromBody] AvailabilityParameters_Mobile availabilityParameters_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            var pickupBranchId = Convert.ToString(availabilityParameters_Mobile.queryParameters.pickupBranchId);
            loggerHelper.traceInputsInfo<AvailabilityParameters_Mobile>(availabilityParameters_Mobile);
            try
            {
                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                availabilityParameters_Mobile.queryParameters = virtualBranchHelper.buildVirtualBrachParameters(availabilityParameters_Mobile.queryParameters);
                var availabilityParam = new AvailabilityMapper().createAvailabilityParameter_Mobile(availabilityParameters_Mobile, (int)rnt_ReservationChannel.Mobile, (int)GlobalEnums.CustomerType.Individual, 0);
                MongoDBHelper.Entities.AvailabilityFactorBusiness availabilityFactorBusiness = new MongoDBHelper.Entities.AvailabilityFactorBusiness(mongoDBHostName, mongoDBDatabaseName);

                MongoDBHelper.Entities.CrmConfigurationBusiness crmConfigurationBusiness = new MongoDBHelper.Entities.CrmConfigurationBusiness(mongoDBHostName, mongoDBDatabaseName);
                var cacheKey = crmConfigurationBusiness.getCrmConfigurationByKey<string>("branch_CacheKey");

                BranchCacheClient branchCacheClient = new BranchCacheClient(mongoDBHostName, mongoDBDatabaseName, cacheKey);
                var branchs = branchCacheClient.getBranchCache("mobile", (int)rnt_ReservationChannel.Mobile, true);

                var pickUpBranch = branchs.Where(x => x.BranchId == pickupBranchId).FirstOrDefault();

                var _customerType = availabilityParameters_Mobile.corporateCustomerId.HasValue && availabilityParameters_Mobile.corporateCustomerId != Guid.Empty
                                          ? (int)GlobalEnums.CustomerType.Corporate
                                          : (int)GlobalEnums.CustomerType.Individual;


                #region Shift Duration - System Parameter
                //SystemParameterBL systemParameterBL = new SystemParameterBL(crmServiceHelper.IOrganizationService);
                var shiftDuration = Convert.ToInt32(StaticHelper.GetConfiguration("ShiftDuration"));

                availabilityParam = new AvailabilityMapper().createAvailabilityParameter_Mobile(availabilityParameters_Mobile,
                                                                                            (int)rnt_ReservationChannel.Mobile,
                                                                                            _customerType,
                                                                                            shiftDuration);
                #endregion

                if (pickUpBranch.earlistPickupTime.HasValue && pickUpBranch.earlistPickupTime != 0)
                {
                    availabilityParam.earlistPickupTime = pickUpBranch.earlistPickupTime;
                }

                if (_customerType == (int)GlobalEnums.CustomerType.Corporate)
                {
                    MongoDBHelper.Repository.CorporateCustomerRepository corporateCustomerRepository = new MongoDBHelper.Repository.CorporateCustomerRepository(mongoDBHostName, mongoDBDatabaseName);
                    var corporateCustomer = corporateCustomerRepository.getCustomerById(Convert.ToString(availabilityParameters_Mobile.corporateCustomerId));
                    if (corporateCustomer.priceFactorGroupCode.HasValue && corporateCustomer.priceFactorGroupCode != 0)
                    {
                        availabilityParam.accountGroup = corporateCustomer.priceFactorGroupCode.Value.ToString();
                    }
                }

                var r = availabilityFactorBusiness.checkAvailability(availabilityParam, availabilityParameters_Mobile.langId, (int)rnt_ReservationChannel.Mobile, _customerType, availabilityParam.accountGroup);
                if (!r.ResponseResult.Result)
                {
                    return new AvailabilityResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(r.ResponseResult.ExceptionDetail)
                    };
                }


                #region Calculate Prices from MongoDB
                MongoDBHelper.Entities.AvailabilityBusiness availabilityBusiness = new MongoDBHelper.Entities.AvailabilityBusiness(availabilityParam);
                var calculateAvailability = availabilityBusiness.calculateAvailability();
                #endregion

                #region Remove not calculated groups

                var calculatedGroups = calculateAvailability.Where(p => p.isPriceCalculatedSafely == true && p.hasError == false).ToList();
                var response = new AvailabilityMapper().createMobileAvailabilityList(calculatedGroups);
                List<AvailabilityData_Mobile> removedItems = new List<AvailabilityData_Mobile>();

                if (availabilityParameters_Mobile.campaignId.HasValue && availabilityParameters_Mobile.campaignId.Value != Guid.Empty)
                {
                    MongoDBHelper.Entities.CampaignBusiness campaignBusiness = new MongoDBHelper.Entities.CampaignBusiness(this.mongoDBHostName, this.mongoDBDatabaseName);
                    foreach (var item in response)
                    {
                        var prices = campaignBusiness.calculateCampaignPrices(new CreateCampaignPricesRequest
                        {
                            campaignParameters = new CampaignParameters
                            {
                                beginingDate = availabilityParameters_Mobile.queryParameters.pickupDateTime,
                                endDate = availabilityParameters_Mobile.queryParameters.dropoffDateTime,
                                branchId = Convert.ToString(availabilityParameters_Mobile.queryParameters.pickupBranchId),
                                calculatedPricesTrackingNumber = availabilityBusiness.trackingNumber,
                                customerType = _customerType,
                                groupCodeInformationId = Convert.ToString(item.groupCodeId),
                                reservationChannelCode = Convert.ToString((int)rnt_ReservationChannel.Mobile)
                            }
                        },
                        availabilityParameters_Mobile.campaignId.Value.ToString());

                        if (!prices.ResponseResult.Result)
                        {
                            removedItems.Add(item);
                            continue;
                        }
                        item.campaign_payNowTotalAmount = Decimal.Round(prices.calculatedCampaignPrices.FirstOrDefault().payNowDailyPrice.Value, 2);
                        item.campaign_payLaterTotalAmount = Decimal.Round(prices.calculatedCampaignPrices.FirstOrDefault().payLaterDailyPrice.Value, 2);
                    }

                }
                response = response.Except(removedItems).ToList(); //IBU

                response = response.Except(removedItems).ToList();
                loggerHelper.traceInfo("Remove not calculated groups end ");

                #endregion

                #region Get Duration
                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var duration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(availabilityParameters_Mobile.queryParameters.pickupDateTime, availabilityParameters_Mobile.queryParameters.dropoffDateTime);
                #endregion

                //IBU
                if (response == null || response.Count == 0)
                {
                    CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("NullEquipmentList", availabilityParameters_Mobile.langId, this.mobileErrorMessagePath);

                    return new AvailabilityResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(message)
                    };
                }



                return new AvailabilityResponse_Mobile
                {
                    availabilityData = response,
                    trackingNumber = availabilityBusiness.trackingNumber,
                    duration = duration,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("calculateAvailability error : " + ex.Message);
                return new AvailabilityResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/getadditionalproducts")]
        public AdditionalProductResponse_Mobile getAdditionalProducts([FromBody] AdditionalProductParameters_Mobile additionalProductParameters_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            loggerHelper.traceInfo(JsonConvert.SerializeObject(additionalProductParameters_Mobile));

            try
            {
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var cacheKey = configurationBL.GetConfigurationByName("additionalProduct_cacheKey");


                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect(additionalProductParameters_Mobile.queryParameters.pickupDateTime,
                                                                                              additionalProductParameters_Mobile.queryParameters.dropoffDateTime);

                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                             StaticHelper.prepareAdditionalProductCacheKey(totalDuration.ToString(), cacheKey));
                var products = additionalProductCacheClient.getAdditionalProductsCache(totalDuration, additionalProductParameters_Mobile.queryParameters.pickupBranchId);

                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                additionalProductParameters_Mobile.queryParameters = virtualBranchHelper.buildVirtualBrachParameters(additionalProductParameters_Mobile.queryParameters);

                AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(crmServiceHelper.IOrganizationService);
                var dateandBranchParameters = new AdditionalProductMapper().buildAdditionalProductDateandBranchNeccessaryParameters(additionalProductParameters_Mobile);

                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var individualCustomer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(additionalProductParameters_Mobile.individualCustomerId.Value, new string[] { "birthdate", "rnt_drivinglicensedate" });

                var individualParameter = new AdditionalProductMapper().buildAdditionalProductIndividualNeccessaryParameters(individualCustomer.GetAttributeValue<DateTime>("birthdate"),
                                                                                                                             individualCustomer.GetAttributeValue<DateTime>("rnt_drivinglicensedate"));

                GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(crmServiceHelper.IOrganizationService);
                var groupCodeEntity = groupCodeInformationRepository.getGroupCodeInformationById(additionalProductParameters_Mobile.groupCodeId.Value);

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
                List<AdditionalProductRule_Mobile> additionalProductRules = new List<AdditionalProductRule_Mobile>();
                var additionalProductRulesCacheKey = configurationBL.GetConfigurationByName("additionalProductRules_cacheKey");

                AdditionalProductRuleCacheClient additionalProductRuleCacheClient = new AdditionalProductRuleCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                             additionalProductRulesCacheKey);
                additionalProductRules = additionalProductRuleCacheClient.getMobileAdditionalProductRuleCache();
                #endregion   

                if (!response.ResponseResult.Result)
                {
                    return new AdditionalProductResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(response.ResponseResult.ExceptionDetail)
                    };
                }
                return new AdditionalProductResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess(),
                    additionalProducts = new AdditionalProductMapper().buildAdditionalProductsforMobile(response.AdditionalProducts),
                    additionalProductRules = additionalProductRules
                };

            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getAdditionalProducts error : " + ex.Message);
                return new AdditionalProductResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/createreservation")]
        public ReservationCreateResponse_Mobile createReservation([FromBody] ReservationCreateParameters_Mobile reservationCreateParameters_Mobile) //IBU
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            try
            {
                loggerHelper.traceInputsInfo<ReservationCreateParameters_Mobile>(reservationCreateParameters_Mobile);

                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                reservationCreateParameters_Mobile.reservationQueryParameters = virtualBranchHelper.buildVirtualBrachParameters(reservationCreateParameters_Mobile.reservationQueryParameters);
                var customerParameters = new IndividualCustomerMapper().buildReservationCustomerParameters(reservationCreateParameters_Mobile.reservationCustomerParameters);

                var reservationDateandBranchParameters = new ReservationDateandBranchParameters
                {
                    dropoffBranchId = reservationCreateParameters_Mobile.reservationQueryParameters.dropoffBranchId,
                    dropoffDate = reservationCreateParameters_Mobile.reservationQueryParameters.dropoffDateTime,
                    pickupBranchId = reservationCreateParameters_Mobile.reservationQueryParameters.pickupBranchId,
                    pickupDate = reservationCreateParameters_Mobile.reservationQueryParameters.pickupDateTime
                };

                var reservationEquipmentParameters = new GroupCodeInformationMapper().buildReservationEquipmentParameter(reservationCreateParameters_Mobile.reservationEquimentParameters);

                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect(reservationCreateParameters_Mobile.reservationQueryParameters.pickupDateTime,
                                                                                              reservationCreateParameters_Mobile.reservationQueryParameters.dropoffDateTime);

                var additionalProducts = new AdditionalProductMapper().buildReservationAdditionalProducts(reservationCreateParameters_Mobile.reservationAdditionalProducts, totalDuration);

                PriceCalculationSummariesRepository priceCalculationSummariesRepository = new PriceCalculationSummariesRepository(this.mongoDBHostName, this.mongoDBDatabaseName);

                var prices = priceCalculationSummariesRepository.getPriceCalculationSummariesforDailyPrices(reservationCreateParameters_Mobile.reservationEquimentParameters.groupCodeId.ToString(),
                                                                                                            reservationCreateParameters_Mobile.reservationPriceParameters.trackingNumber,
                                                                                                            reservationCreateParameters_Mobile.reservationPriceParameters.campaignId);

                prices = priceCalculationSummariesRepository.checkDailyPricesForMainCampaign(prices,
                                                                                 reservationCreateParameters_Mobile.reservationPriceParameters.trackingNumber,
                                                                                 reservationCreateParameters_Mobile.reservationEquimentParameters.groupCodeId.ToString());

                var totalAmount = decimal.Zero;
                if (reservationCreateParameters_Mobile.reservationPriceParameters.paymentChoice == (int)rnt_reservation_rnt_paymentchoicecode.PayNow)
                {
                    totalAmount = prices.Sum(p => p.payNowAmount);
                }
                else
                {
                    totalAmount = prices.Sum(p => p.payLaterAmount);
                }

                List<CreditCardData> creditCardDatas = new List<CreditCardData>();
                if (reservationCreateParameters_Mobile.reservationPriceParameters.customerCreditCard != null)
                {
                    var creditCardData = new CreditCardMapper().buildCreditCardData(reservationCreateParameters_Mobile.reservationPriceParameters.customerCreditCard);
                    creditCardDatas.Add(creditCardData);
                }
                ContractHelper contractHelper = new ContractHelper(crmServiceHelper.IOrganizationService);
                var vPosResponse = new VirtualPosResponse { virtualPosId = 0 };//contractHelper.getVPosIdforGivenCardNumber(creditCardData);
                var priceParameters = new PriceMapper().buildReservationPriceParameter(customerParameters,
                                                                                       reservationCreateParameters_Mobile.reservationPriceParameters,
                                                                                       creditCardDatas,
                                                                                       totalAmount,
                                                                                       customerParameters.contactId,
                                                                                       vPosResponse.virtualPosId);

                var cType = reservationCreateParameters_Mobile.reservationCustomerParameters.corporateCustomerId.HasValue &&
                            reservationCreateParameters_Mobile.reservationCustomerParameters.corporateCustomerId.Value != Guid.Empty ?
                            (int)rnt_ReservationTypeCode.Kurumsal :
                            (int)rnt_ReservationTypeCode.Bireysel;

                //IBU
                if (!string.IsNullOrEmpty(reservationCreateParameters_Mobile.CouponCode))
                {
                    MongoDBHelper.Repository.CouponCodeRepository codeDefinitionRepository = new MongoDBHelper.Repository.CouponCodeRepository(mongoDBHostName, mongoDBDatabaseName);
                    var d = codeDefinitionRepository.getCouponCodeDetailsByCouponCode(reservationCreateParameters_Mobile.CouponCode);
                    if (d.FirstOrDefault() != null)
                    {
                        BusinessLibrary.Repository.CouponCodeDefinitionRepository couponCodeDefinitionRepository = new BusinessLibrary.Repository.CouponCodeDefinitionRepository(crmServiceHelper.IOrganizationService);
                        CouponCodeDefinitionValidations couponCodeDefinitionValidations = new CouponCodeDefinitionValidations(crmServiceHelper.IOrganizationService);
                        var copuonCodeDefinition = couponCodeDefinitionRepository.getCouponCodeDefinitionById(new Guid(d.FirstOrDefault().couponCodeDefinitionId));
                        var validationResponse = couponCodeDefinitionValidations.checkCuponCodeDefinitionReservationDate(copuonCodeDefinition, reservationCreateParameters_Mobile.reservationQueryParameters.pickupDateTime);
                        if (!validationResponse)
                        {
                            XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);

                            var message = xrmHelper.GetXmlTagContentByGivenLangId("CouponInvalidDefinationDate", reservationCreateParameters_Mobile.langId, couponCodeXmlPath);
                            throw new Exception(message);
                        }
                        if (copuonCodeDefinition != null)
                        {
                            decimal paynowdiscountvalue = copuonCodeDefinition.GetAttributeValue<decimal>("rnt_paynowdiscountvalue");
                            decimal paylaterdiscountvalue = copuonCodeDefinition.GetAttributeValue<decimal>("rnt_paylaterdiscountvalue");
                            decimal discountAmount = 0;
                            if (reservationCreateParameters_Mobile.reservationPriceParameters.paymentChoice == (int)rnt_reservation_rnt_paymentchoicecode.PayNow)
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
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_createreservation");
                organizationRequest["SelectedCustomer"] = JsonConvert.SerializeObject(customerParameters);
                organizationRequest["SelectedDateAndBranch"] = JsonConvert.SerializeObject(reservationDateandBranchParameters);
                organizationRequest["CouponCode"] = reservationCreateParameters_Mobile.CouponCode; //IBU
                organizationRequest["PriceParameters"] = JsonConvert.SerializeObject(priceParameters);
                organizationRequest["SelectedEquipment"] = JsonConvert.SerializeObject(reservationEquipmentParameters);
                organizationRequest["LangId"] = reservationCreateParameters_Mobile.langId;
                organizationRequest["TrackingNumber"] = reservationCreateParameters_Mobile.reservationPriceParameters.trackingNumber;
                organizationRequest["TotalDuration"] = totalDuration;
                organizationRequest["ReservationChannel"] = (int)rnt_ReservationChannel.Mobile;
                organizationRequest["ReservationTypeCode"] = cType;
                organizationRequest["SelectedAdditionalProducts"] = JsonConvert.SerializeObject(additionalProducts);
                organizationRequest["Currency"] = cur;

                var res = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var reservationResponse = JsonConvert.DeserializeObject<ReservationCreateResponse>(Convert.ToString(res.Results["ReservationResponse"]));


                var createResponse_Mobile = new ReservationCreateResponse_Mobile
                {

                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()

                };
                createResponse_Mobile.Map(reservationResponse);

                return createResponse_Mobile;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("createReservation error : " + ex.Message);
                return new ReservationCreateResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/forgotpassword")]
        public ForgotPasswordResponse_Mobile forgotPassword(ForgotPasswordRequest_Mobile forgotPasswordRequest_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(forgotPasswordRequest_Mobile));

                if (forgotPasswordRequest_Mobile.newPassword != forgotPasswordRequest_Mobile.newPasswordAgain)
                {
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("PasswordMismatch", forgotPasswordRequest_Mobile.langId, this.mobileErrorMessagePath);

                    return new ForgotPasswordResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(message)
                    };

                }
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var customer = individualCustomerRepository.getIndividualCustomerByMobileEmailAddress(forgotPasswordRequest_Mobile.emailAddress);
                if (customer == null)
                {
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("MissingUser", forgotPasswordRequest_Mobile.langId, this.mobileErrorMessagePath);

                    return new ForgotPasswordResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(message)
                    };
                }


                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(crmServiceHelper.IOrganizationService);
                individualCustomerBL.updateMobilePassword(customer.Id, forgotPasswordRequest_Mobile.newPasswordAgain);

                return new ForgotPasswordResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("forgotPassword error : " + ex.Message);
                return new ForgotPasswordResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpGet]
        [Route("api/mobile/getcampaignlist")]
        public GetCampaignListResponse_Mobile getCampaignList()
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                MongoDBHelper.Repository.CampaignRepository campaignRepository = new CampaignRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var campaigns = campaignRepository.getActiveCampaigns(DateTime.UtcNow.AddMinutes(StaticHelper.offset), (int)rnt_ReservationChannel.Mobile);
                loggerHelper.traceInfo("date utc now " + DateTime.UtcNow.AddMinutes(StaticHelper.offset));

                var campaignDatas = new List<CampaignData>();

                CrmServiceHelper_Mobile crmServiceHelper_Mobile = new CrmServiceHelper_Mobile();

                foreach (var item in campaigns)
                {
                    CampaignData c = new CampaignData();
                    c.Map(item);
                    campaignDatas.Add(c);
                }
                var data = new CampaignMapper().buildMobileCampaignData(campaignDatas);

                CampaignDetailRepository campaignDetailRepository = new CampaignDetailRepository(crmServiceHelper_Mobile.IOrganizationService);
                var cmsCampaigns = campaignDetailRepository.getCMSCampaigns(new string[] { "rnt_popupcontent", "rnt_capmaignbannerurl", "rnt_campaignmobileimageurl", "rnt_campaignimageurl", "rnt_campaignid" });

                foreach (var item in data)
                {
                    var camp = cmsCampaigns.Where(p => p.Contains("rnt_campaignid") &&
                                                      p.GetAttributeValue<EntityReference>("rnt_campaignid").Id.Equals(item.campaignId) &&
                                                      p.Attributes.Contains("rnt_campaignimageurl")).FirstOrDefault();
                    if (camp != null)
                    {
                        item.campaignImageURL = camp.Attributes.Contains("rnt_campaignimageurl") ? camp.GetAttributeValue<string>("rnt_campaignimageurl") : string.Empty;
                        item.campaignMobileImageURL = camp.Attributes.Contains("rnt_campaignmobileimageurl") ? camp.GetAttributeValue<string>("rnt_campaignmobileimageurl") : string.Empty;
                        item.campaignBannerURL = camp.Attributes.Contains("rnt_capmaignbannerurl") ? camp.GetAttributeValue<string>("rnt_capmaignbannerurl") : string.Empty;
                        item.campaignTerms = camp.Attributes.Contains("rnt_popupcontent") ? camp.GetAttributeValue<string>("rnt_popupcontent") : string.Empty;
                    }
                }
                return new GetCampaignListResponse_Mobile
                {
                    campaigns = data,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCampaignList error : " + ex.Message);
                return new GetCampaignListResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/getcampaigndetail")]
        public GetCampaignDetailResponse_Mobile getCampaignDetail(GetCampaignDetailRequest_Mobile getCampaignDetailRequest_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo("getCampaignDetailRequest_Mobile : " + JsonConvert.SerializeObject(getCampaignDetailRequest_Mobile));
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
                CampaignDetailBL campaignDetailBL = new CampaignDetailBL(crmServiceHelper.IOrganizationService);
                var camp = campaignDetailBL.getCampaignDetail(getCampaignDetailRequest_Mobile.campaignId, getCampaignDetailRequest_Mobile.langId);

                var campaignDetail = new CampaignMapper().buildMobileCampaignDetailData(camp);
                campaignDetail.campaignContent = camp.popupContent;
                return new GetCampaignDetailResponse_Mobile
                {
                    campaignDetail = campaignDetail,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("getCampaignDetail error : " + ex.Message);
                return new GetCampaignDetailResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/getcampaignbranchlist")]
        public GetCampaignDetailResponse_Mobile getCampaignBranchList(GetCampaignDetailRequest_Mobile getCampaignDetailRequest_Mobile) //IBU
        {
            string campaignId = Convert.ToString(getCampaignDetailRequest_Mobile.campaignId);
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
                loggerHelper.traceInfo("getCampaignDetailRequest_Mobile : " + JsonConvert.SerializeObject(getCampaignDetailRequest_Mobile));
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
                CampaignDetailBL campaignDetailBL = new CampaignDetailBL(crmServiceHelper.IOrganizationService);
                var camp = campaignDetailBL.getCampaignBranchList(getCampaignDetailRequest_Mobile.campaignId, getCampaignDetailRequest_Mobile.langId);

                VirtualBranchHelper virtualBranchHelper = new VirtualBranchHelper(mongoDBHostName, mongoDBDatabaseName);
                var activeVirtualBranches = virtualBranchHelper.getActiveBranchesByChannelCode((int)rnt_ReservationChannel.Mobile); //IBU

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

                return new GetCampaignDetailResponse_Mobile
                {
                    campaignDetail = new CampaignMapper().buildMobileCampaignDetailData(campaignDetailData), //IBU
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("getCampaignBranchList error : " + ex.Message);
                return new GetCampaignDetailResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/getcustomerreservations")]
        public GetCustomerReservationsResponse_Mobile getCustomerReservations([FromBody] GetCustomerReservationsRequest_Mobile getCustomerReservationsRequest_mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var reservations = reservationItemRepository.getReservationsByCustomerId(Convert.ToString(getCustomerReservationsRequest_mobile.individualCustomerId));
                loggerHelper.traceInfo(JsonConvert.SerializeObject(getCustomerReservationsRequest_mobile));

                var reservationDatas = new List<ReservationItemData>();
                foreach (var item in reservations)
                {
                    ReservationItemData r = new ReservationItemData();
                    r.Map(item);
                    reservationDatas.Add(r);
                }

                var data = new ReservationMapper().buildMobileReservationData(reservationDatas, getCustomerReservationsRequest_mobile.langId);

                data = data.OrderByDescending(p => p.PickupTime).ToList();
                return new GetCustomerReservationsResponse_Mobile
                {
                    reservations = data,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCustomerReservations error : " + ex.Message);
                return new GetCustomerReservationsResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/getcorporatereservations")]
        public GetCorporateReservationsResponse_Mobile getCorporateReservations([FromBody] GetCorporateReservationsRequest_Mobile getCorporateReservationsRequest_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var reservations = reservationItemRepository.getReservationsByCustomerId(Convert.ToString(getCorporateReservationsRequest_Mobile.corporateCustomerId));
                loggerHelper.traceInfo(JsonConvert.SerializeObject(getCorporateReservationsRequest_Mobile));

                var reservationDatas = new List<ReservationItemData>();
                foreach (var item in reservations)
                {
                    ReservationItemData r = new ReservationItemData();
                    r.Map(item);
                    reservationDatas.Add(r);
                }

                var data = new ReservationMapper().buildMobileReservationData(reservationDatas, getCorporateReservationsRequest_Mobile.langId);

                data = data.OrderByDescending(p => p.PickupTime).ToList();
                return new GetCorporateReservationsResponse_Mobile
                {
                    reservations = data,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCustomerReservations error : " + ex.Message);
                return new GetCorporateReservationsResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/processcustomerinvoiceaddress")]
        public ProcessCustomerInvoiceAddressResponse_Mobile processCustomerInvoiceAddress([FromBody] ProcessCustomerInvoiceAddressRequest_Mobile processCustomerInvoiceAddressRequest_mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(processCustomerInvoiceAddressRequest_mobile));

                var invoiceAddressParameters = new ProcessInvoiceMapper().buildInvoiceAddressCreateParameters(processCustomerInvoiceAddressRequest_mobile);
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_ProcessCustomerInvoiceAddress");
                organizationRequest["invoiceAddressParameters"] = JsonConvert.SerializeObject(invoiceAddressParameters);
                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var invoiceAddressResponse = JsonConvert.DeserializeObject<InvoiceAddressProcessResponse>(Convert.ToString(response.Results["invoiceAddressResponse"]));

                if (invoiceAddressResponse.ResponseResult.Result)
                {
                    return new ProcessCustomerInvoiceAddressResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess(),
                        invoiceAddressId = invoiceAddressResponse.invoiceAddressId
                    };
                }

                return new ProcessCustomerInvoiceAddressResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(invoiceAddressResponse.ResponseResult.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("processCustomerInvoiceAddress error : " + ex.Message);
                return new ProcessCustomerInvoiceAddressResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/calculatecancelfineamount")]
        public CalculateCancelFineAmountResponse_Mobile calculateCancelFineAmount([FromBody] CalculateCancelFineAmountParameters_Mobile calculateCancelFineAmountParameters_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();

            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(calculateCancelFineAmountParameters_Mobile));
                var cancelFineAmountParameters = new CancelFineAmountMapper().buildCancelFineAmountParameters(calculateCancelFineAmountParameters_Mobile);

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CalculateFineAmountReservation");
                organizationRequest["langId"] = calculateCancelFineAmountParameters_Mobile.langId;
                organizationRequest["reservationId"] = Convert.ToString(calculateCancelFineAmountParameters_Mobile.reservationId);
                organizationRequest["pnrNumber"] = Convert.ToString(calculateCancelFineAmountParameters_Mobile.pnrNumber);

                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var reservationFineAmountResponse = JsonConvert.DeserializeObject<ReservationCancellationResponse>(Convert.ToString(response.Results["ReservationFineAmountResponse"]));

                if (reservationFineAmountResponse.ResponseResult.Result)
                {
                    return new CalculateCancelFineAmountResponse_Mobile
                    {
                        fineAmount = reservationFineAmountResponse.fineAmount,
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                    };
                }

                return new CalculateCancelFineAmountResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(reservationFineAmountResponse.ResponseResult.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {

                loggerHelper.traceError("calculateCancelFineAmount error : " + ex.Message);
                return new CalculateCancelFineAmountResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/cancelreservation")]
        public CancelReservationResponse_Mobile cancelReservation([FromBody] CancelReservationParameters_Mobile cancelReservationParameters_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();

            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(cancelReservationParameters_Mobile));
                MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                var reservations = reservationItemRepository.getReservationByPnrNumber(cancelReservationParameters_Mobile.pnrNumber);

                var cancelReservationParameters = new CancelReservationMapper().buildCancelReservationParameters(cancelReservationParameters_Mobile);

                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CancelReservation");
                organizationRequest["langId"] = cancelReservationParameters.langId;
                organizationRequest["reservationId"] = Convert.ToString(reservations.ReservationId);
                organizationRequest["pnrNumber"] = Convert.ToString(cancelReservationParameters.pnrNumber);
                organizationRequest["cancellationReason"] = (int)rnt_reservation_StatusCode.CancelledByCustomer;
                organizationRequest["cancellationDescription"] = cancelReservationParameters.cancellationDescription;
                organizationRequest["cancellationSubReason"] = 100000008;
                var response = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var reservationCancellationResponse = JsonConvert.DeserializeObject<ReservationCancellationResponse>(Convert.ToString(response.Results["ReservationCancellationResponse"]));

                if (reservationCancellationResponse.ResponseResult.Result)
                {
                    return new CancelReservationResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                    };
                }

                return new CancelReservationResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(reservationCancellationResponse.ResponseResult.ExceptionDetail)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("cancelReservation error : " + ex.Message);
                return new CancelReservationResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/getbanners")]
        public GetBannersResponse_Mobile getBanners([FromBody] GetBannersRequest_Mobile getBannersRequest_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(getBannersRequest_Mobile));
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();

                BannerBL bannerBL = new BannerBL(crmServiceHelper.IOrganizationService);
                var banners = bannerBL.getBanners();
                return new GetBannersResponse_Mobile
                {
                    bannerDatas = new BannerMapper().buildMobileBannerData(banners),
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("getBanners error : " + ex.Message);
                return new GetBannersResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/getadditionalproductscontent")]
        public GetAdditionalProductsContentResponse_Mobile getAdditionalProductsContent([FromBody] GetAdditionalProductsContentRequest_Mobile getAdditionalProductsContentRequest_Mobile)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(getAdditionalProductsContentRequest_Mobile));
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();

                CMSAdditionalProductBL cmsAdditionalProductBL = new CMSAdditionalProductBL(crmServiceHelper.IOrganizationService);
                var products = cmsAdditionalProductBL.GetCMSAdditionalProducts();

                return new GetAdditionalProductsContentResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess(),
                    additionalProducts = new CMSAdditionalProductMapper().buildCMSAdditionalProducts_Mobile(products)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("getBanners error : " + ex.Message);
                return new GetAdditionalProductsContentResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/getAccountContacts")]
        public GetAccountContactsResponse_Mobile getAccountContacts([FromBody] GetAccountContactsRequest_Mobile accountContactsRequest_Mobile)
        {
            try
            {
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
                ConnectionRepository connectionRepository = new ConnectionRepository(crmServiceHelper.IOrganizationService);
                var connections = connectionRepository.getConnectionsByAccountId(accountContactsRequest_Mobile.accountId);

                List<AccountContactsData_Mobile> accountContactsDatas = new List<AccountContactsData_Mobile>();

                if (connections == null)
                {
                    return new GetAccountContactsResponse_Mobile
                    {
                        accountContactsData = null,
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError("The account you have specified does not have any contacts")
                    };
                }

                connections.ForEach(conn =>
                {
                    var contactId = conn.GetAttributeValue<EntityReference>("rnt_contactid").Id;
                    var relationCode = conn.GetAttributeValue<OptionSetValue>("rnt_relationcode").Value;
                    var relation = Enum.GetName(typeof(ClassLibrary._Enums_1033.rnt_connection_rnt_relationcode), relationCode);

                    IndividualAddressRepository individualAddressRepository = new IndividualAddressRepository(crmServiceHelper.IOrganizationService);
                    var individualAddress = individualAddressRepository.getIndividualAddressesByCustomerId(contactId);
                    var individualAddressData = new IndividualAddressMapper().createMobileIndividualAddressList(individualAddress);

                    IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                    var contact = individualCustomerRepository.getIndividualCustomerById(contactId);
                    var contactData = new IndividualCustomerMapper().createMobileIndividualCustomerData(contact, accountContactsRequest_Mobile.langId);

                    accountContactsDatas.Add(new AccountContactsData_Mobile
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
                        individualAddressDatas = individualAddressData
                    });
                });
                return new GetAccountContactsResponse_Mobile
                {
                    accountContactsData = accountContactsDatas,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new GetAccountContactsResponse_Mobile
                {
                    accountContactsData = null,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/createCorporateRelation")]
        public CreateCorporateRelationResponse_Mobile createCorporateRelation([FromBody] CreateCorporateRelationRequest_Mobile createCorporateRelationRequest_Mobile)
        {
            CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(createCorporateRelationRequest_Mobile));
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var customerId = individualCustomerRepository.getExistingCustomerIdByGovernmentIdOrPassportNumberOrEmailAddress(createCorporateRelationRequest_Mobile.governmentId,
                                                                                                                                createCorporateRelationRequest_Mobile.governmentId,
                                                                                                                                new string[] { });
                if (customerId == null)
                {
                    throw new Exception("Girdiğiniz bilgilere ait kullanıcı bulunamadı.");
                }

                ConnectionBL connectionBL = new ConnectionBL(crmServiceHelper.IOrganizationService);

                if (createCorporateRelationRequest_Mobile.breakRelation != null && createCorporateRelationRequest_Mobile.breakRelation == true)
                {
                    ConnectionRepository connectionRepository = new ConnectionRepository(crmServiceHelper.IOrganizationService);
                    var connection = connectionRepository.getConnectionByGivenCriterias(createCorporateRelationRequest_Mobile.accountId, customerId.Id, createCorporateRelationRequest_Mobile.relationType);

                    if (connection == null)
                    {
                        return new CreateCorporateRelationResponse_Mobile
                        {
                            responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError("The connection you specified is already passive. Please change your parameters")
                        };
                    }

                    connectionBL.deactivateConnection(connection.Id);

                    return new CreateCorporateRelationResponse_Mobile
                    {
                        relationId = connection.GetAttributeValue<Guid>("rnt_connectionid"),
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                    };
                }

                var connectionId = connectionBL.createConnection(customerId.Id, createCorporateRelationRequest_Mobile.accountId, createCorporateRelationRequest_Mobile.relationType);

                return new CreateCorporateRelationResponse_Mobile
                {
                    relationId = connectionId,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new CreateCorporateRelationResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/getKilometerLimits")]
        public GetKilometerLimitsResponse_Mobile getKilometerLimits(GetKilometerLimitsRequest_Mobile getKilometerLimitsRequest_Mobile)
        {
            CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
            try
            {
                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var duration = durationHelper.calculateDocumentDurationByPriceHourEffect(getKilometerLimitsRequest_Mobile.pickupDateTime, getKilometerLimitsRequest_Mobile.dropoffDateTime);

                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var cacheKey = configurationBL.GetConfigurationByName("groupcode_cacheKey");

                GroupCodeInformationCacheClient groupCodeInformationCacheClient = new GroupCodeInformationCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                                      cacheKey);
                var groupCodeData = groupCodeInformationCacheClient.getAllGroupCodeInformationDetailCache(getKilometerLimitsRequest_Mobile.langId);

                var kmLimitCacheKey = configurationBL.GetConfigurationByName("kmlimit_cachekey");

                KilometerLimitCacheClient kilometerLimitCacheClient = new KilometerLimitCacheClient(crmServiceHelper.IOrganizationService, kmLimitCacheKey);
                var limits = kilometerLimitCacheClient.getKilometerLimitCache_Mobile(groupCodeData, duration);

                return new GetKilometerLimitsResponse_Mobile
                {
                    kmLimits = limits,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new GetKilometerLimitsResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/getcorporatecustomerreservations")]
        public GetCustomerReservationsResponse_Mobile getCorporateCustomerReservations([FromBody] GetCustomerReservationsRequest_Mobile getCustomerReservationsRequest_Mobile)
        {
            CrmServiceHelper_Mobile crmServiceHelper_Mobile = new CrmServiceHelper_Mobile();
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                var reservations = new List<ReservationItemDataMongoDB>();

                //first check user is the master user
                ConnectionRepository connectionRepository = new ConnectionRepository(crmServiceHelper_Mobile.IOrganizationService);
                var connection = connectionRepository.getConnectionByGivenCriterias(getCustomerReservationsRequest_Mobile.corporateCustomerId,
                                                                                    getCustomerReservationsRequest_Mobile.individualCustomerId,
                                                                                    (int)rnt_connection_rnt_relationcode.ResponsibleEmployeeforCorporateReservations);

                if (connection != null)
                {
                    MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                    reservations = reservationItemRepository.getCorporateReservations(getCustomerReservationsRequest_Mobile.corporateCustomerId);
                }
                else
                {
                    connection = connectionRepository.getConnectionByGivenCriterias(getCustomerReservationsRequest_Mobile.corporateCustomerId,
                                                                                   getCustomerReservationsRequest_Mobile.individualCustomerId,
                                                                                   (int)rnt_connection_rnt_relationcode.EmployeeForCorporateReservations);
                    if (connection != null)
                    {
                        MongoDBHelper.Repository.ReservationItemRepository reservationItemRepository = new MongoDBHelper.Repository.ReservationItemRepository(this.mongoDBHostName, this.mongoDBDatabaseName);
                        reservations = reservationItemRepository.getCorporateReservationsByCustomer(getCustomerReservationsRequest_Mobile.corporateCustomerId, getCustomerReservationsRequest_Mobile.individualCustomerId);
                    }
                }

                loggerHelper.traceInfo(JsonConvert.SerializeObject(getCustomerReservationsRequest_Mobile));

                var reservationDatas = new List<ReservationItemData>();
                foreach (var item in reservations)
                {
                    ReservationItemData r = new ReservationItemData();
                    r.Map(item);
                    reservationDatas.Add(r);
                }

                var data = new ReservationMapper().buildMobileReservationData(reservationDatas, getCustomerReservationsRequest_Mobile.langId);

                data = data.OrderByDescending(p => p.PickupTime).ToList();
                return new GetCustomerReservationsResponse_Mobile
                {
                    reservations = data,
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCustomerReservations error : " + ex.Message);
                return new GetCustomerReservationsResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route("api/mobile/retrieveinstallments")]
        public RetrieveInstallmentsResponse_Mobile retrieveInstallments([FromBody] RetrieveInstallmentParameters_Mobile retrieveInstallmentParameters_Mobile)
        {
            CrmServiceHelper_Mobile crmServiceHelper_Mobile = new CrmServiceHelper_Mobile();
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo(JsonConvert.SerializeObject(retrieveInstallmentParameters_Mobile));
                CreditCardBL creditCardBL = new CreditCardBL(crmServiceHelper_Mobile.IOrganizationService);
                var installments = creditCardBL.retrieveInstallmentforGivenCard(new CreditCardMapper().buildInstallmentData_Mobile(retrieveInstallmentParameters_Mobile));

                if (!installments.ResponseResult.Result)
                {
                    return new RetrieveInstallmentsResponse_Mobile
                    {
                        responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(installments.ResponseResult.ExceptionDetail)
                    };

                }
                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper_Mobile.IOrganizationService);
                var maturityDifferenceCode = configurationBL.GetConfigurationByName("additionalProduct_MaturityDifference");

                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(crmServiceHelper_Mobile.IOrganizationService, maturityDifferenceCode);
                var additionalProductData = additionalProductCacheClient.getAdditionalProductCache(maturityDifferenceCode);

                var additionalProductData_Mobile = new AdditionalProductMapper().buildAdditionalProductforMobile(additionalProductData);
                var mobile_installments = new CreditCardMapper().buildInstallmentData_Mobile(installments.installmentData.FirstOrDefault());
                return new RetrieveInstallmentsResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess(),
                    additionalProduct = additionalProductData_Mobile,
                    installmentData = mobile_installments
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("getCustomerReservations error : " + ex.Message);
                return new RetrieveInstallmentsResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/executecouponcodeoperations")]
        public CouponCodeOperationsResponse_Mobile executeCouponCodeOperations([FromBody] CouponCodeOperationsRequest_Mobile couponCodeOperationsRequest_Mobile) //IBU
        {
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(couponCodeOperationsRequest_Mobile.reservationId))
            {
                string reservationId = Convert.ToString(couponCodeOperationsRequest_Mobile.reservationId);
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
                loggerHelper.traceInfo(JsonConvert.SerializeObject(couponCodeOperationsRequest_Mobile));
                CrmServiceHelper_Mobile crmServiceHelper = new CrmServiceHelper_Mobile();
                XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);

                CouponCodeRepository couponCodeRepository = new CouponCodeRepository(mongoDBHostName, mongoDBDatabaseName);
                var c = couponCodeRepository.getCouponCodeDetailsByCouponCode(couponCodeOperationsRequest_Mobile.couponCode);
                if (c.Count == 0)
                {
                    message = xrmHelper.GetXmlTagContentByGivenLangId("CouponCodeIsNotFound", couponCodeOperationsRequest_Mobile.langId, this.couponCodeXmlPath);
                }
                else
                {
                    loggerHelper.traceInfo(JsonConvert.SerializeObject(c));
                    var couponCode = c.Where(x => x.statusCode == (int)GlobalEnums.CouponCodeStatusCode.Generated).FirstOrDefault();
                    if (couponCode != null)
                    {
                        couponCodeOperationsRequest_Mobile.statusCode = (int)GlobalEnums.CouponCodeStatusCode.Generated;
                    }
                    else
                    {
                        couponCodeOperationsRequest_Mobile.statusCode = c.FirstOrDefault().statusCode;
                    }

                    VirtualBranchRepository virtualBranchRepository = new VirtualBranchRepository(mongoDBHostName, mongoDBDatabaseName);
                    var branch = virtualBranchRepository.getBranchByVirtualBranchId(couponCodeOperationsRequest_Mobile.pickupBranchId).branch;

                    couponCodeOperationsRequest_Mobile.pickupBranchId = branch;
                    if (!couponCodeOperationsRequest_Mobile.groupCodeInformationId.HasValue)
                    {
                        couponCodeOperationsRequest_Mobile.groupCodeInformationId = Guid.Empty;
                    }

                    loggerHelper.traceInfo(JsonConvert.SerializeObject(couponCodeOperationsRequest_Mobile));
                    OrganizationRequest organizationRequest = new OrganizationRequest("rnt_CouponCodeOperations");
                    organizationRequest["CouponCodeOperationsParameter"] = JsonConvert.SerializeObject(couponCodeOperationsRequest_Mobile);
                    organizationRequest["langId"] = couponCodeOperationsRequest_Mobile.langId;

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
                            return new CouponCodeOperationsResponse_Mobile
                            {
                                couponCodeName = crmDefinition.GetAttributeValue<string>("rnt_name"),
                                couponCodeData = parsedResponse.couponCodeData,
                                definitionType = crmDefinition.GetAttributeValue<OptionSetValue>("rnt_type").Value,// ratio - amount
                                definitionPayLaterDiscountValue = crmDefinition.GetAttributeValue<decimal>("rnt_paylaterdiscountvalue"),
                                definitionPayNowDiscountValue = crmDefinition.GetAttributeValue<decimal>("rnt_paynowdiscountvalue"),
                                groupCodes = definition.groupCodeInformations,
                                responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()
                            };
                        }
                        message = xrmHelper.GetXmlTagContentByGivenLangId("SystemError", couponCodeOperationsRequest_Mobile.langId, this.mobileErrorMessagePath);
                    }
                }


                return new CouponCodeOperationsResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(message)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("executeCouponCodeOperations error : " + ex.Message);
                return new CouponCodeOperationsResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route("api/mobile/executeCustomerAnontmization")]
        public IndividualCustomerAnonymizationResponse executeCustomerAnontmization([FromBody] IndividualCustomerAnonymizationRequest customer)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Mobile crmServiceHelper_Mobile = new CrmServiceHelper_Mobile();
            try
            {
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL();
                DateTime expireRequestDate = DateTime.UtcNow.AddHours(+3);
                DateTime expireDate = individualCustomerBL.RequestIndividualCustomerAnonymization(customer.individualCustomerId, expireRequestDate, customer.langId);

                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper_Mobile.IOrganizationService);
                Entity contact = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(customer.individualCustomerId, new string[] { "fullname" });
                string fullName = contact.GetAttributeValue<string>("fullname");

                XrmHelper xrmHelper = new XrmHelper(crmServiceHelper_Mobile.IOrganizationService);
                return new IndividualCustomerAnonymizationResponse
                {
                    anonymizationMessage = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("AnonymizationRequest", customer.langId, this.mobileErrorMessagePath), fullName),
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
        [Route("api/mobile/cancelCustomerAnontmization")]
        public IndividualCustomerAnonymizationResponse cancelCustomerAnontmization([FromBody] IndividualCustomerAnonymizationRequest customer) //IBU
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper_Mobile crmServiceHelper_Mobile = new CrmServiceHelper_Mobile();
            try
            {
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL();
                individualCustomerBL.CancelIndividualCustomerAnonymization(customer.individualCustomerId);

                XrmHelper xrmHelper = new XrmHelper(crmServiceHelper_Mobile.IOrganizationService);
                return new IndividualCustomerAnonymizationResponse
                {
                    anonymizationMessage = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("CancelAnonymizationRequest", customer.langId, this.mobileErrorMessagePath)),
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
        [Route("api/mobile/setreservation3dpaymentreturn")]
        public Reservation3dPaymentReturnResponse_Mobile setReservation3dPaymentReturn(Reservation3dPaymentReturnRequest_Mobile reservation3DPaymentReturnRequest_Mobile)
        {
            if (reservation3DPaymentReturnRequest_Mobile.reservationId == Guid.Empty)
            {
                return new Reservation3dPaymentReturnResponse_Mobile() 
                        { responseResult = new ClassLibrary._Mobile.ResponseResult() { result = false, exceptionDetail="Rezervasyon id boş olamaz." } };
            }
            LoggerHelper loggerHelper = new LoggerHelper("Payment3dReturn_" + reservation3DPaymentReturnRequest_Mobile.reservationId.ToString());
            loggerHelper.traceInfo($"reservation3DPaymentReturnRequest_Mobile : {JsonConvert.SerializeObject(reservation3DPaymentReturnRequest_Mobile)}");
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            try
            {
                loggerHelper.traceInfo("create return parameters");
                Payment3dReturnParameters payment3DReturnParameters = new Payment3dReturnParameters()
                {
                    reservationId = reservation3DPaymentReturnRequest_Mobile.reservationId,
                    contractId = Guid.Empty,
                    paymentId = Guid.Empty,
                    providerPaymentId = reservation3DPaymentReturnRequest_Mobile.providerPaymentId,
                    conversationData = reservation3DPaymentReturnRequest_Mobile.conversationData,
                    conversationId = reservation3DPaymentReturnRequest_Mobile.conversationId,
                    success = reservation3DPaymentReturnRequest_Mobile.success == 1 ? true : false, //== "success" ? (int)ReservationEnums.StatusCode.New : (int)ReservationEnums.StatusCode.Waitingfor3D,
                    detail = reservation3DPaymentReturnRequest_Mobile.detail
                };

                loggerHelper.traceInfo("create organizationRequest");
                OrganizationRequest organizationRequest = new OrganizationRequest("rnt_Execute3dPaymentReturn");
                loggerHelper.traceInfo("action parameters " + JsonConvert.SerializeObject(payment3DReturnParameters));
                organizationRequest["Payment3dReturnParameters"] = JsonConvert.SerializeObject(payment3DReturnParameters);

                loggerHelper.traceInfo("execute plugin");
                var res = crmServiceHelper.IOrganizationService.Execute(organizationRequest);
                var reservationResponse = JsonConvert.DeserializeObject<ReservationCreateResponse>(Convert.ToString(res.Results["ReservationPaymentReturnResponse"]));
                loggerHelper.traceInfo("end plugin");

                var createResponse_Mobile = new Reservation3dPaymentReturnResponse_Mobile
                {

                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnSuccess()

                };
                createResponse_Mobile.Map(reservationResponse);

                return createResponse_Mobile;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError("setReservation3dPaymentReturn error : " + ex.Message);
                return new Reservation3dPaymentReturnResponse_Mobile
                {
                    responseResult = ClassLibrary._Mobile.ResponseResult.ReturnError(ex.Message)
                };
            }
        }


    }
}