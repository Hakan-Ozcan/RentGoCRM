using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Tablet;
using RntCar.Logger;
using RntCar.MongoDBHelper;
using RntCar.MongoDBHelper.Entities;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.RedisCacheHelper.CachedItems;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using RntCar.SDK.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI.Controllers
{
    //[BasicHttpAuthorizeAttribute_Local]
    public class ServiceController : ApiController
    {
        private string mongoDBHostName { get; set; }
        private string mongoDBDatabaseName { get; set; }

        private List<AdditonalProductDataTablet> otherAdditonalProductDataTablets = new List<AdditonalProductDataTablet>();
        private CalculateAdditionalProductResponse additonalProductResponse = new CalculateAdditionalProductResponse();

        public ServiceController()
        {
            mongoDBHostName = StaticHelper.GetConfiguration("MongoDBHostName");
            mongoDBDatabaseName = StaticHelper.GetConfiguration("MongoDBDatabaseName");
            //loggerHelper = new LoggerHelper();
        }

        [HttpGet]
        [Route(TabletRouteConfig.testme)]
        public string testme()
        {
            return "ok";
        }

        [HttpPost]
        [Route(TabletRouteConfig.getContractByEquipment)]
        public GetContractsByEquipmentResponse getContractByEquipment([FromBody] GetContractsByEquipmentParameters getContractsByEquipmentParameters)
        {
            try
            {
                string plateNumber = Convert.ToString(getContractsByEquipmentParameters.plateNumber);
                LoggerHelper loggerHelper;
                if (!string.IsNullOrWhiteSpace(plateNumber))
                {
                    loggerHelper = new LoggerHelper(plateNumber + "GCBE");
                }
                else
                {
                    loggerHelper = new LoggerHelper();
                }
                loggerHelper.traceInfo("getContractByEquipment process start");
                ContractItemBusiness contractItemBusiness = new ContractItemBusiness(mongoDBHostName, mongoDBDatabaseName);
                var rentalContracts = contractItemBusiness.getRentalContractByPlateNumber(getContractsByEquipmentParameters);
                loggerHelper.traceInputsInfo<DashboardRentalContractData>(rentalContracts, "getRentalContractByPlateNumber : ");

                if (rentalContracts == null)
                {
                    loggerHelper.traceInfo("rentalContracts is null");
                    CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    return new GetContractsByEquipmentResponse
                    {
                        //todo will read from xml due to langId
                        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(xrmHelper.GetXmlTagContentByGivenLangId("NotFoundLicencePlate", 1055, new HandlerBase().tabletXmlPath))
                    };
                }
                loggerHelper.traceInfo("getContractByEquipment process end");
                return new GetContractsByEquipmentResponse
                {
                    rentalContract = rentalContracts,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new GetContractsByEquipmentResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }


        }
        [HttpPost]
        [Route(TabletRouteConfig.getContractsByBranch)]
        public GetContractsByBranchResponse getContractsByBranch([FromBody] GetContractsByBranchParameters getContractsByBranchParameters)
        {
            try
            {
                string branchId = Convert.ToString(getContractsByBranchParameters.branchId);
                LoggerHelper loggerHelper;
                if (!string.IsNullOrWhiteSpace(branchId))
                {
                    loggerHelper = new LoggerHelper(branchId + "GCBB");
                }
                else
                {
                    loggerHelper = new LoggerHelper();
                }
                loggerHelper.traceInfo("getContractsByBranch process start");
                ContractItemBusiness contractItemBusiness = new ContractItemBusiness(mongoDBHostName, mongoDBDatabaseName);
                var waitingDeliveryContracts = contractItemBusiness.getDashboardWaitingforDeliveryContractsByBranchId(getContractsByBranchParameters);
                loggerHelper.traceInputsInfo<List<DashboardContractData>>(waitingDeliveryContracts, "waitingDeliveryContracts : ");
                var rentalContracts = contractItemBusiness.getDashboardRentalContractsByBranchId(getContractsByBranchParameters);
                loggerHelper.traceInputsInfo<List<DashboardRentalContractData>>(rentalContracts, "rentalContracts : ");

                loggerHelper.traceInfo("getContractsByBranch process end");

                return new GetContractsByBranchResponse
                {
                    deliveryContractList = waitingDeliveryContracts,
                    rentalContractList = rentalContracts,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new GetContractsByBranchResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }


        }
        [HttpPost]
        [Route(TabletRouteConfig.getContractDetails)]
        public GetContractDetailResponse getContractDetails([FromBody] GetContractDetailParameters getContractDetailParameters)
        {
            string contractId = Convert.ToString(getContractDetailParameters.contractId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(contractId))
            {
                loggerHelper = new LoggerHelper(contractId + "GCD");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getContractDetails process start");
                loggerHelper.traceInputsInfo<GetContractDetailParameters>(getContractDetailParameters, "getContractDetailParameters : ");

                var isEquipmentChanged = false;
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                #region Get Contract Detail and Check Digitail Signature

                ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                var contractEntity = contractRepository.getContractById(getContractDetailParameters.contractId, new string[] { "rnt_paymentmethodcode",
                                                                                                                               "rnt_contracttypecode",
                                                                                                                               "statuscode",
                                                                                                                               "rnt_digitalsignatureurl",
                                                                                                                               "rnt_corporateid",
                                                                                                                               "rnt_pricinggroupcodeid"});

                if (contractEntity.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)RntCar.ClassLibrary._Enums_1033.rnt_contract_StatusCode.WaitingForDelivery &&
                    string.IsNullOrEmpty(contractEntity.GetAttributeValue<string>("rnt_digitalsignatureurl")))
                {
                    XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                    HandlerBase handlerBase = new HandlerBase();
                    var message = xrmHelper.GetXmlTagContent(null, "NotSignedContract", handlerBase.tabletXmlPath);
                    return new GetContractDetailResponse
                    {
                        //todo will read from xml due to langId
                        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(message)
                    };
                }
                #endregion

                #region Get Contract Item
                //get related contract from mongodb
                loggerHelper.traceInfo("get related contract from mongodb");
                ContractItemBusiness contractItemBusiness = new ContractItemBusiness(mongoDBHostName, mongoDBDatabaseName);
                var contractItem = contractItemBusiness.getContractEquipment(Convert.ToString(getContractDetailParameters.contractId),
                                                                                              getContractDetailParameters.statusCode);
                loggerHelper.traceInputsInfo<GetContractDetailResponse>(contractItem, "related contractItem : ");
                #endregion
                //create crm service client


                #region Check Contract Items
                //check contract items               
                loggerHelper.traceInfo("Check Contract Items");
                ContractItemRepository contractItemRepository = new ContractItemRepository(crmServiceHelper.IOrganizationService);
                var contractItems = contractItemRepository.getContractEquipmentsByGivenColumns(getContractDetailParameters.contractId, new string[] { "statuscode" });

                var deliveredItems = contractItems.Where(p => p.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.WaitingForDelivery).ToList();
                var rentalItems = contractItems.Where(p => p.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Rental).ToList();
                var completedItems = contractItems.Where(p => p.GetAttributeValue<OptionSetValue>("statuscode").Value == (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.Completed).ToList();

                if (deliveredItems.Count > 0 &&
                    rentalItems.Count > 0)
                {
                    loggerHelper.traceInfo("deliveredItems.Count > 0 && rentalItems.Count > 0");
                    if (getContractDetailParameters.statusCode == (int)RntCar.ClassLibrary._Enums_1033.rnt_contractitem_StatusCode.WaitingForDelivery)
                    {
                        loggerHelper.traceInfo("status is WaitingForDelivery");
                        XrmHelper xrmHelper = new XrmHelper(crmServiceHelper.IOrganizationService);
                        HandlerBase handlerBase = new HandlerBase();
                        var message = xrmHelper.GetXmlTagContent(null, "ChangeOfCar", handlerBase.tabletXmlPath);
                        return new GetContractDetailResponse
                        {
                            //todo will read from xml due to langId
                            responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(string.Format(message, contractItem.contractNumber))
                        };
                    }
                    //if rental
                    isEquipmentChanged = true;

                }
                else if (deliveredItems.Count > 0 && completedItems.Count > 0)
                {
                    loggerHelper.traceInfo("deliveredItems.Count > 0 && completedItems.Count > 0");
                    isEquipmentChanged = true;
                }

                #endregion

                List<Task> _tasks = new List<Task>();

                #region Get GroupCodeInformation from RedisCache
                //Check Redis Cache for group code information
                loggerHelper.traceInfo("Get GroupCodeInformation from RedisCache task created");
                var groupCodeInformationTask = new Task(() =>
                {
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService, crmServiceHelper.CrmServiceClient);
                    var cacheKey = configurationBL.GetConfigurationByName("groupcode_cacheKey");
                    GroupCodeInformationCacheClient groupCodeInformationCacheClient = new GroupCodeInformationCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                                         crmServiceHelper.CrmServiceClient,
                                                                                                                         cacheKey);
                    var groupCodeData = groupCodeInformationCacheClient.getGroupCodeInformationDetailCache(contractItem.groupCodeInformation.groupCodeId);
                    loggerHelper.traceInputsInfo<GroupCodeInformationDetailData>(groupCodeData, "contractItems : ");

                    contractItem.groupCodeInformation.groupCodeDescription = groupCodeData.showRoomBrandName + " " +
                                                                             groupCodeData.showRoomModelName;
                    contractItem.groupCodeInformation.groupCodeImage = groupCodeData.image;
                    contractItem.groupCodeInformation.transmissionName = groupCodeData.gearboxcodeName;
                    contractItem.groupCodeInformation.fuelTypeName = groupCodeData.fueltypecodeName;
                    contractItem.groupCodeInformation.totalPrice = contractItem.totalPrice;
                    contractItem.groupCodeInformation.pricingGroupCodeId = contractEntity.GetAttributeValue<EntityReference>("rnt_pricinggroupcodeid").Id;
                });
                groupCodeInformationTask.Start();
                _tasks.Add(groupCodeInformationTask);
                loggerHelper.traceInfo("Get GroupCodeInformation from RedisCache task started");
                #endregion

                #region Get Individual Information
                loggerHelper.traceInfo("Get Individual Information task created");
                var individualCustomerTask = new Task(() =>
                {
                    IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                    var individualCustomer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(contractItem.customer.customerId,
                                                                                                                             new string[] { "fullname",
                                                                                                                                            "mobilephone",
                                                                                                                                            "emailaddress1",
                                                                                                                                            "rnt_drivinglicensenumber",
                                                                                                                                            "rnt_segmentcode"});
                    contractItem.customer.fullName = individualCustomer.GetAttributeValue<string>("fullname");
                    contractItem.customer.mobilePhone = individualCustomer.GetAttributeValue<string>("mobilephone");
                    contractItem.customer.email = individualCustomer.GetAttributeValue<string>("emailaddress1");
                    contractItem.customer.drivingLicenseNumber = individualCustomer.GetAttributeValue<string>("rnt_drivinglicensenumber");
                    contractItem.customer.segment = individualCustomer.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value;
                    contractItem.customer.contractType = contractEntity.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value;
                    contractItem.customer.paymentMethod = contractEntity.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
                    if (contractEntity.Attributes.Contains("rnt_corporateid"))
                    {
                        CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(crmServiceHelper.IOrganizationService);
                        var corp = corporateCustomerRepository.getCorporateCustomerById(contractEntity.GetAttributeValue<EntityReference>("rnt_corporateid").Id, new string[] { "rnt_pricecodeid" });
                        contractItem.customer.priceCodeId = corp.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id;
                    }

                });
                individualCustomerTask.Start();
                _tasks.Add(individualCustomerTask);
                loggerHelper.traceInfo("Get Individual Information task started");
                #endregion

                #region Check Equipment Information
                loggerHelper.traceInfo("Check Equipment Information task created");
                var equipmentTask = new Task(() =>
                {
                    if (contractItem.selectedEquipment?.equipmentId != Guid.Empty)
                    {
                        loggerHelper.traceInfo("selectedEquipment : " + contractItem.selectedEquipment.equipmentId);
                        //getting quipment detail from mongodb
                        MongoDBHelper.Repository.EquipmentRepository equipmentRepository = new MongoDBHelper.Repository.EquipmentRepository(mongoDBHostName, mongoDBDatabaseName);
                        var equipment = equipmentRepository.getEquipmentById(contractItem.selectedEquipment.equipmentId);
                        //loggerHelper.traceInputsInfo<EquipmentDataMongoDB>(equipment, "equipment data : ");
                        contractItem.selectedEquipment = new EquipmentData
                        {
                            brandName = equipment.brandName,
                            brandId = new Guid(equipment.brandId),
                            doorNumber = equipment.nofDoor,
                            fuelValue = equipment.fuel,
                            kmValue = equipment.CurrentKM,
                            luggageNumber = equipment.nofLuggage,
                            modelId = new Guid(equipment.modelId),
                            modelName = equipment.modelName,
                            plateNumber = equipment.PlateNumber,
                            seatNumber = equipment.nofSeat,
                            equipmentId = contractItem.selectedEquipment.equipmentId,
                            hgsNumber = contractItem.selectedEquipment.hgsNumber

                        };

                    }
                });
                equipmentTask.Start();
                _tasks.Add(equipmentTask);
                loggerHelper.traceInfo("Check Equipment Information task started");
                #endregion

                loggerHelper.traceInfo("get credit cards task created");
                List<CreditCardData> creditCards = new List<CreditCardData>();

                var creditCardTask = new Task(() =>
                {
                    PaymentRepository paymentRepository = new PaymentRepository(crmServiceHelper.IOrganizationService);
                    var payments = paymentRepository.getAllPaymentWithGivenColumns_Contract(getContractDetailParameters.contractId, new string[] { "rnt_customercreditcardid", "rnt_transactiontypecode" });
                    CreditCardBL creditCardBL = new CreditCardBL(crmServiceHelper.IOrganizationService);
                    foreach (var item in payments)
                    {
                        if (item.Attributes.Contains("rnt_customercreditcardid"))
                        {
                            var creditCardId = item.GetAttributeValue<EntityReference>("rnt_customercreditcardid").Id;
                            var creditCard = creditCardBL.getCustomerCreditCardByCreditCardId(creditCardId);
                            creditCard.cardType = item.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode").Value;
                            creditCards.Add(creditCard);
                        }

                    }
                    creditCards = creditCards.DistinctBy(p => p.creditCardNumber).ToList();
                });
                creditCardTask.Start();
                _tasks.Add(creditCardTask);
                loggerHelper.traceInfo("get credit cards task started");

                Task.WaitAll(_tasks.ToArray());
                loggerHelper.traceInfo("all task process end");

                contractItem.creditCards = creditCards;
                contractItem.isEquipmentChanged = isEquipmentChanged;
                contractItem.responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess();
                return contractItem;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetContractDetailResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getReservationsByBranch)]
        public GetReservationsByBranchResponse getReservationsByBranch([FromBody] GetReservationsByBranchParameter parameter)
        {
            string branchId = Convert.ToString(parameter.branchId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(branchId))
            {
                loggerHelper = new LoggerHelper(branchId + "GRBB");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getReservationsByBranch start");
                RntCar.MongoDBHelper.Entities.ReservationItemBusiness reservationItemBusiness = new ReservationItemBusiness(mongoDBHostName, mongoDBDatabaseName);
                var reservationList = reservationItemBusiness.getDashboardNewReservationsByBranchId(parameter.branchId.ToString());
                loggerHelper.traceInputsInfo<List<DashboardReservationData>>(reservationList, "reservationList : ");
                return new GetReservationsByBranchResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess(),
                    reservationList = reservationList
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo(ex.Message);
                return new GetReservationsByBranchResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getEquipmentsByGroupCode)]
        public GetBranchEquipmentResponse getEquipmentsByGroupCode([FromBody] GetBranchEquipmentParameters getBranchEquipmentParameters)
        {
            string branchId = Convert.ToString(getBranchEquipmentParameters.branchId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(branchId))
            {
                loggerHelper = new LoggerHelper(branchId + "GEBGC");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getEquipmentsByGroupCode start");
                MongoDBHelper.Repository.EquipmentRepository equipmentRepository = new MongoDBHelper.Repository.EquipmentRepository(mongoDBHostName, mongoDBDatabaseName);
                var equipments = equipmentRepository.getBranchEquipmentsByGroupCode(getBranchEquipmentParameters.branchId, getBranchEquipmentParameters.groupCodeId.ToString());
                //loggerHelper.traceInputsInfo<List<EquipmentDataMongoDB>>(equipments, "equipments : ");

                List<EquipmentData> equipmentDatas = new List<EquipmentData>();
                foreach (var item in equipments)
                {
                    EquipmentData equipmentData = new EquipmentData
                    {
                        brandId = new Guid(item.brandId),
                        brandName = item.brandName,
                        doorNumber = item.nofDoor,
                        fuelValue = item.fuel,
                        kmValue = item.CurrentKM,
                        luggageNumber = item.nofLuggage,
                        modelId = new Guid(item.modelId),
                        modelName = item.modelName,
                        plateNumber = item.PlateNumber,
                        seatNumber = item.nofSeat,
                        equipmentId = new Guid(item.EquipmentId),
                        statusReason = item.StatusCode
                    };
                    equipmentDatas.Add(equipmentData);
                }
                return new GetBranchEquipmentResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess(),
                    equipmentList = equipmentDatas
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetBranchEquipmentResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getEquipmentInventories)]
        public GetEquipmentInventoriesResponse getEquipmentInventories([FromBody] GetEquipmentInventoriesParameters getEquipmentInventoriesParameters)
        {
            string equipmentId = Convert.ToString(getEquipmentInventoriesParameters.equipmentId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(equipmentId))
            {
                loggerHelper = new LoggerHelper(equipmentId + "GEI");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getEquipmentInventories start");
                loggerHelper.traceInputsInfo<GetEquipmentInventoriesParameters>(getEquipmentInventoriesParameters, "getEquipmentInventoriesParameters : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                EquipmentInventoryBL equipmentInventoryBL = new EquipmentInventoryBL(crmServiceHelper.IOrganizationService);
                var data = equipmentInventoryBL.getLatestEquipmentInventoryInformation(getEquipmentInventoriesParameters.equipmentId, getEquipmentInventoriesParameters.langId);
                //loggerHelper.traceInputsInfo<List<EquipmentInventoryData>>(data, "EquipmentInventoryData : ");
                return new GetEquipmentInventoriesResponse
                {
                    equipmentInventoryData = data,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetEquipmentInventoriesResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getContractGroupCodeReport)]
        public GetContractGroupCodeReportResponse getContractGroupCodeReport([FromBody] GetContractGroupCodeReportParameters getContractGroupCodeReportParameters)
        {
            string branchId = Convert.ToString(getContractGroupCodeReportParameters.branchId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(branchId))
            {
                loggerHelper = new LoggerHelper(branchId + "GCGCR");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getContractGroupCodeReport start");
                loggerHelper.traceInputsInfo<GetContractGroupCodeReportParameters>(getContractGroupCodeReportParameters, "getContractGroupCodeReportParameters : ");
                ContractItemBusiness contractItemBusiness = new ContractItemBusiness(mongoDBHostName, mongoDBDatabaseName);
                var res = contractItemBusiness.getWaitingforDeliveryContractReport(getContractGroupCodeReportParameters);
                loggerHelper.traceInputsInfo<List<BranchCount>>(res, "BranchCountData : ");
                return new GetContractGroupCodeReportResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess(),
                    waitingContractsByGroupCode = res
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetContractGroupCodeReportResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getEquipmentsByBranch)]
        public GetBranchEquipmentResponse getEquipmentsByBranch([FromBody] GetBranchEquipmentParameters getBranchEquipmentParameters)
        {
            string branchId = Convert.ToString(getBranchEquipmentParameters.branchId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(branchId))
            {
                loggerHelper = new LoggerHelper(branchId + "GEBB");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getEquipmentsByBranch start");
                loggerHelper.traceInputsInfo<GetBranchEquipmentParameters>(getBranchEquipmentParameters, "getBranchEquipmentParameters : ");
                MongoDBHelper.Repository.EquipmentRepository equipmentRepository = new MongoDBHelper.Repository.EquipmentRepository(mongoDBHostName, mongoDBDatabaseName);
                var equipmentsData = equipmentRepository.getAllActiveEquipmentsByBranchId(getBranchEquipmentParameters.branchId);
                //loggerHelper.traceInputsInfo<List<EquipmentDataMongoDB>>(equipmentsData, "equipmentsData : ");

                var convertedEquipments = equipmentsData.ConvertAll(item => new EquipmentData
                {
                    equipmentId = Guid.Parse(item.EquipmentId),
                    groupCodeId = Guid.Parse(item.GroupCodeInformationId),
                    groupCodeName = item.GroupCodeInformationName,
                    brandId = Guid.Parse(item.brandId),
                    brandName = item.brandName,
                    modelId = Guid.Parse(item.modelId),
                    modelName = item.modelName,
                    plateNumber = item.PlateNumber,
                    seatNumber = item.nofSeat,
                    doorNumber = item.nofDoor,
                    luggageNumber = item.nofLuggage,
                    statusReason = item.StatusCode,
                    fuelValue = item.fuelType,
                    kmValue = item.CurrentKM
                });
                return new GetBranchEquipmentResponse
                {
                    equipmentList = convertedEquipments,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetBranchEquipmentResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getTabletMasterData)]
        public GetMasterDataResponse getTabletMasterData([FromBody] GetMasterDataParameter getMasterDataParameter)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo("getTabletMasterData start");
                loggerHelper.traceInputsInfo<GetMasterDataParameter>(getMasterDataParameter, "getMasterDataParameter : ");
                List<Task> _tasks = new List<Task>();
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                List<EquipmentPartData> _equipmentParts = new List<EquipmentPartData>();
                List<DamageSizeData> _damageSizes = new List<DamageSizeData>();
                List<DamageTypeData> _damageTypes = new List<DamageTypeData>();
                List<DamagePriceData> _damagePriceDatas = new List<DamagePriceData>();
                List<DamageDocumentData> _damageDocuments = new List<DamageDocumentData>();
                List<Branch> _branchs = new List<Branch>();
                List<GroupCodeInformation> _groupCodeInformation = new List<GroupCodeInformation>();

                loggerHelper.traceInfo("taskEquipmentPart created");
                var taskEquipmentPart = new Task(() =>
                {
                    EquipmentPartsBL equipmentPartsBL = new EquipmentPartsBL(crmServiceHelper.IOrganizationService);
                    _equipmentParts = equipmentPartsBL.getEquipmentParts();
                    loggerHelper.traceInfo("taskEquipmentPart end");
                });

                _tasks.Add(taskEquipmentPart);
                taskEquipmentPart.Start();
                loggerHelper.traceInfo("taskEquipmentPart started");

                loggerHelper.traceInfo("taskDamageSize created");
                var taskDamageSize = new Task(() =>
                {
                    DamageSizeBL damageSizeBL = new DamageSizeBL(crmServiceHelper.IOrganizationService);
                    _damageSizes = damageSizeBL.getDamageSizes();
                    loggerHelper.traceInfo("taskDamageSize end");
                });

                _tasks.Add(taskDamageSize);
                taskDamageSize.Start();
                loggerHelper.traceInfo("taskDamageSize started");

                loggerHelper.traceInfo("taskDamageType created");
                var taskDamageType = new Task(() =>
                {
                    DamageTypesBL damageTypeBL = new DamageTypesBL(crmServiceHelper.IOrganizationService);
                    _damageTypes = damageTypeBL.getDamageTypes();
                    loggerHelper.traceInfo("taskDamageType end");
                });

                _tasks.Add(taskDamageType);
                taskDamageType.Start();
                loggerHelper.traceInfo("taskDamageType started");

                loggerHelper.traceInfo("taskDamageDocument created");
                var taskDamageDocument = new Task(() =>
                {
                    DamageDocumentBL damageDocumentBL = new DamageDocumentBL(crmServiceHelper.IOrganizationService);
                    _damageDocuments = damageDocumentBL.getDamageDocuments();
                    loggerHelper.traceInfo("taskDamageDocument end");
                });

                _tasks.Add(taskDamageDocument);
                taskDamageDocument.Start();
                loggerHelper.traceInfo("taskDamageDocument started");

                loggerHelper.traceInfo("taskBranchs created");
                var taskBranchs = new Task(() =>
                {
                    BranchBL branchBL = new BranchBL(crmServiceHelper.IOrganizationService);
                    var branchs = branchBL.getActiveBranchs();
                    _branchs = branchs.ConvertAll(item => new Branch
                    {
                        branchId = Guid.Parse(item.BranchId),
                        branchName = item.BranchName
                    });
                    loggerHelper.traceInfo("taskBranchs end");
                });

                _tasks.Add(taskBranchs);
                taskBranchs.Start();
                loggerHelper.traceInfo("taskBranchs started");

                loggerHelper.traceInfo("taskGroupCodeInformation created");
                var taskGroupCodeInformation = new Task(() =>
                {
                    ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                    var cacheKey = configurationBL.GetConfigurationByName("groupcode_cacheKey");
                    //Check Redis Cache for group code information
                    GroupCodeInformationCacheClient groupCodeInformationCacheClient = new GroupCodeInformationCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                                          crmServiceHelper.CrmServiceClient,
                                                                                                                          cacheKey);
                    var groupCodeData = groupCodeInformationCacheClient.getAllGroupCodeInformationDetailCache(getMasterDataParameter.langId);
                    _groupCodeInformation = new GroupCodeInformationMapper().createTabletModelList(groupCodeData);
                    loggerHelper.traceInfo("taskGroupCodeInformation end");
                });

                _tasks.Add(taskGroupCodeInformation);
                taskGroupCodeInformation.Start();
                loggerHelper.traceInfo("taskGroupCodeInformation started");

                Task.WaitAll(_tasks.ToArray());

                loggerHelper.traceInfo("all task process completed");

                var optionSetData = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_equipment_StatusCode", getMasterDataParameter.langId);
                loggerHelper.traceInputsInfo<List<OptionSetModel>>(optionSetData, "optionSetData : ");

                optionSetData.ForEach(p =>
                {
                    if (p.label == "KaypCalnt_Inactive")
                    {
                        p.label = "Kayıp Çalıntı";
                    }
                    else if (p.label == "KaypCalnt_Active")
                    {
                        p.label = "Kayıp Çalıntı";
                    }
                    else if (p.label == "Ykamada")
                    {
                        p.label = "Yıkamada";
                    }
                    else if (p.label == "YaktDolumda")
                    {
                        p.label = "Yakıt Dolumda";
                    }
                    else if (p.label == "BakmBekliyor")
                    {
                        p.label = "Bakım Bekliyor";
                    }
                    else if (p.label == "Kullanlabilir")
                    {
                        p.label = "Kullanılabilir";
                    }
                });
                return new GetMasterDataResponse
                {
                    equipmentPartList = _equipmentParts,
                    damageSizeList = _damageSizes,
                    damageTypeList = _damageTypes,
                    damageDocumentList = _damageDocuments,
                    branchList = _branchs,
                    groupCodeList = _groupCodeInformation,
                    equipmentStatusOptionSetList = optionSetData,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetMasterDataResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getDamagesByEquipment)]
        public GetDamageDataReponse getDamagesByEquipment([FromBody] GetDamageDataParameters getDamageDataParameters)
        {
            string equipmentId = Convert.ToString(getDamageDataParameters.equipmentId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(equipmentId))
            {
                loggerHelper = new LoggerHelper(equipmentId + "GDBE");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getDamagesByEquipment start");
                loggerHelper.traceInputsInfo<GetDamageDataParameters>(getDamageDataParameters, "getDamageDataParameters : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                DamageBL damageBL = new DamageBL(crmServiceHelper.IOrganizationService);
                var damageDataList = damageBL.getDamageDataByEquipmentId(getDamageDataParameters.equipmentId);
                loggerHelper.traceInputsInfo<List<DamageData>>(damageDataList, "damageDataList : ");
                return new GetDamageDataReponse
                {
                    damageList = damageDataList,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetDamageDataReponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.updateEquipmentInformation)]
        public UpdateEquipmentInformationResponse updateEquipmentInformation([FromBody] UpdateEquipmentInformationParameter updateEquipmentInformationParameter)
        {
            string equipmentId = Convert.ToString(updateEquipmentInformationParameter.equipmentInformation.equipmentId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(equipmentId))
            {
                loggerHelper = new LoggerHelper(equipmentId + "UEI");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("updateEquipmentInformation start");
                loggerHelper.traceInputsInfo<UpdateEquipmentInformationParameter>(updateEquipmentInformationParameter, "updateEquipmentInformationParameter : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                ActionHelper actionHelper = new ActionHelper(crmServiceHelper.IOrganizationService);
                loggerHelper.traceInfo("calling updateEquipmentInformation action");
                var response = actionHelper.updateEquipmentInformation(updateEquipmentInformationParameter);
                loggerHelper.traceInputsInfo<ClassLibrary._Tablet.ResponseResult>(response, "response : ");
                if (response.result)
                {
                    return new UpdateEquipmentInformationResponse
                    {
                        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                    };
                }

                return new UpdateEquipmentInformationResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(response.exceptionDetail)
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new UpdateEquipmentInformationResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.updateEquipmentStatus)]
        public UpdateEquipmentInformationResponse updateEquipmentStatus([FromBody] UpdateEquipmentInformationParameter updateEquipmentStatusParameter)
        {
            string equipmentId = Convert.ToString(updateEquipmentStatusParameter.equipmentInformation.equipmentId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(equipmentId))
            {
                loggerHelper = new LoggerHelper(equipmentId + "UES");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("updateEquipmentStatus start");
                loggerHelper.traceInputsInfo<UpdateEquipmentInformationParameter>(updateEquipmentStatusParameter, "updateEquipmentStatusParameter : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                EquipmentBL equipmentBL = new EquipmentBL(crmServiceHelper.IOrganizationService);
                equipmentBL.updateEquipmentStatus(updateEquipmentStatusParameter.equipmentInformation.equipmentId, updateEquipmentStatusParameter.statusCode);
                loggerHelper.traceInfo("equipment status updated");
                return new UpdateEquipmentInformationResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new UpdateEquipmentInformationResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getCalculatedDamagesAmounts)]
        public GetCalculatedDamagesAmountsResponse getCalculatedDamagesAmounts([FromBody] GetCalculatedDamagesAmountsParameters parameters)
        {
            string contractId = Convert.ToString(parameters.contractId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(contractId))
            {
                loggerHelper = new LoggerHelper(contractId + "GCDA");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getCalculatedDamagesAmounts start");
                loggerHelper.traceInputsInfo<GetCalculatedDamagesAmountsParameters>(parameters, "GetCalculatedDamagesAmountsParameters : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                DamageBL damageBL = new DamageBL(crmServiceHelper.IOrganizationService);
                var damagePriceList = damageBL.getCalculatedDamagesAmounts(parameters.damageList, parameters.contractId);
                loggerHelper.traceInputsInfo<List<DamageData>>(damagePriceList, "damagePriceList : ");

                var isAllPricesCalculated = damagePriceList.All(p => p.isPriceCalculated == true);
                loggerHelper.traceInfo("isAllPricesCalculated : " + isAllPricesCalculated);

                AdditonalProductDataTablet additionalProductTablet = null;
                if (isAllPricesCalculated)
                {
                    var totalDamageAmount = damagePriceList.Sum(p => p.damageAmount);
                    loggerHelper.traceInfo("totalDamageAmount : " + totalDamageAmount);
                    if (totalDamageAmount > decimal.Zero)
                    {
                        ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
                        var productCode = configurationRepository.GetConfigurationByKey("additionalProduct_damageReflectionCost");
                        var additionalProductCacheKey = configurationRepository.GetConfigurationByKey("additionalProduct_cacheKey");

                        AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(crmServiceHelper.IOrganizationService, StaticHelper.prepareAdditionalProductCacheKey(productCode, additionalProductCacheKey));
                        var additionalProduct = additionalProductCacheClient.getAdditionalProductCache(productCode);

                        additionalProduct.actualAmount = totalDamageAmount;
                        additionalProduct.actualTotalAmount = totalDamageAmount.Value;
                        additionalProduct.tobePaidAmount = totalDamageAmount.Value;
                        additionalProductTablet = new AdditionalProductMapper().createTabletModel(additionalProduct);
                        loggerHelper.traceInputsInfo<AdditonalProductDataTablet>(additionalProductTablet, "additionalProductTablet : ");
                    }
                }
                loggerHelper.traceInfo("getCalculatedDamagesAmounts end");
                return new GetCalculatedDamagesAmountsResponse
                {
                    damageList = damagePriceList,
                    damageProduct = additionalProductTablet,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetCalculatedDamagesAmountsResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }

        }
        [HttpPost]
        [Route(TabletRouteConfig.getTransfersByBranch)]
        public GetTransfersByBranchResponse getTransfersByBranch([FromBody] GetTransfersByBranchParameter getTransfersByBranchParameter)
        {
            string branchId = Convert.ToString(getTransfersByBranchParameter.branchId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(branchId))
            {
                loggerHelper = new LoggerHelper(branchId + "GTBB");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getTransfersByBranch start");
                loggerHelper.traceInputsInfo<GetTransfersByBranchParameter>(getTransfersByBranchParameter, "getTransfersByBranchParameter : ");
                MongoDBHelper.Repository.TransferRepository transferRepository = new MongoDBHelper.Repository.TransferRepository(mongoDBHostName, mongoDBDatabaseName);
                MongoDBHelper.Repository.EquipmentRepository equipmentRepository = new MongoDBHelper.Repository.EquipmentRepository(mongoDBHostName, mongoDBDatabaseName);

                #region get delivery transfers
                loggerHelper.traceInfo("get delivery transfers");
                var deliveryTransferDataMongoDB = transferRepository.getDeliveryTransfersByBranch(getTransfersByBranchParameter.branchId);
                //loggerHelper.traceInputsInfo<List<TransferDataMongoDB>>(deliveryTransferDataMongoDB, "deliveryTransferDataMongoDB : ");

                List<GetTransfersByBranchData> deliveryTransferData = new List<GetTransfersByBranchData>();
                foreach (var item in deliveryTransferDataMongoDB)
                {
                    var equipment = equipmentRepository.getEquipmentById(Guid.Parse(item.equipmentId));
                    var selectedEquipmentData = new EquipmentData
                    {
                        equipmentId = Guid.Parse(equipment.EquipmentId),
                        brandName = equipment.brandName,
                        modelName = equipment.modelName,
                        plateNumber = equipment.PlateNumber,
                        seatNumber = equipment.nofSeat,
                        doorNumber = equipment.nofDoor,
                        luggageNumber = equipment.nofLuggage,
                        groupCodeId = Guid.Parse(equipment.GroupCodeInformationId),
                        statusReason = equipment.StatusCode,
                        fuelValue = equipment.fuel,
                        kmValue = equipment.CurrentKM
                    };
                    deliveryTransferData.Add(new GetTransfersByBranchData
                    {
                        transferId = Guid.Parse(item.transferId),
                        transferNumber = item.transferNumber,
                        selectedEquipment = selectedEquipmentData,
                        pickupBranch = new Branch
                        {
                            branchId = !string.IsNullOrEmpty(item.pickupBranchId) ? (Guid?)Guid.Parse(item.pickupBranchId) : null,
                            branchName = item.pickupBranchName
                        },
                        dropoffBranch = new Branch
                        {
                            branchId = !string.IsNullOrEmpty(item.dropoffBranchId) ? (Guid?)Guid.Parse(item.dropoffBranchId) : null,
                            branchName = item.dropoffBranchName
                        },
                        pickupTimestamp = item.actualPickupDateTimeStamp.HasValue ? (long?)item.actualPickupDateTimeStamp.Value : null,
                        dropoffTimestamp = item.actualDropoffDateTimeStamp.HasValue ? (long?)item.actualDropoffDateTimeStamp.Value : null,
                        serviceName = item.serviceName,
                        transferType = item.transferTypeCode,
                        statusCode = item.statuscode
                    });
                }
                loggerHelper.traceInputsInfo<List<GetTransfersByBranchData>>(deliveryTransferData, "deliveryTransferData : ");
                #endregion get delivery transfers

                #region get return transfers
                loggerHelper.traceInfo("get delivery transfers");
                var returnTransferDataMongoDB = transferRepository.getReturnTransfersByBranch(getTransfersByBranchParameter.branchId);
                //loggerHelper.traceInputsInfo<List<TransferDataMongoDB>>(returnTransferDataMongoDB, "returnTransferDataMongoDB : ");
                List<GetTransfersByBranchData> returnTransferData = new List<GetTransfersByBranchData>();
                foreach (var item in returnTransferDataMongoDB)
                {
                    var equipment = equipmentRepository.getEquipmentById(Guid.Parse(item.equipmentId));
                    var selectedEquipmentData = new EquipmentData
                    {
                        equipmentId = Guid.Parse(equipment.EquipmentId),
                        brandName = equipment.brandName,
                        modelName = equipment.modelName,
                        plateNumber = equipment.PlateNumber,
                        seatNumber = equipment.nofSeat,
                        doorNumber = equipment.nofDoor,
                        luggageNumber = equipment.nofLuggage,
                        groupCodeId = Guid.Parse(equipment.GroupCodeInformationId),
                        groupCodeName = equipment.GroupCodeInformationName,
                        statusReason = equipment.StatusCode,
                        fuelValue = equipment.fuel,
                        kmValue = equipment.CurrentKM
                    };
                    returnTransferData.Add(new GetTransfersByBranchData
                    {
                        transferId = Guid.Parse(item.transferId),
                        transferNumber = item.transferNumber,
                        selectedEquipment = selectedEquipmentData,
                        pickupBranch = new Branch
                        {
                            branchId = !string.IsNullOrEmpty(item.pickupBranchId) ? (Guid?)Guid.Parse(item.pickupBranchId) : null,
                            branchName = item.pickupBranchName
                        },
                        dropoffBranch = new Branch
                        {
                            branchId = !string.IsNullOrEmpty(item.dropoffBranchId) ? (Guid?)Guid.Parse(item.dropoffBranchId) : null,
                            branchName = item.dropoffBranchName
                        },
                        pickupTimestamp = item.actualPickupDateTimeStamp.HasValue ? (long?)item.actualPickupDateTimeStamp.Value : null,
                        dropoffTimestamp = item.actualDropoffDateTimeStamp.HasValue ? (long?)item.actualDropoffDateTimeStamp.Value : null,
                        serviceName = item.serviceName,
                        transferType = item.transferTypeCode,
                        statusCode = item.statuscode
                    });
                }
                loggerHelper.traceInputsInfo<List<GetTransfersByBranchData>>(returnTransferData, "returnTransferData : ");
                #endregion get return transfers

                return new GetTransfersByBranchResponse
                {
                    deliveryTransferList = deliveryTransferData,
                    returnTransferList = returnTransferData,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetTransfersByBranchResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.login)]
        public ClassLibrary._Tablet.LoginResponse login([FromBody] LoginParameters loginParameters)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            try
            {
                loggerHelper.traceInfo("login start");
                loggerHelper.traceInfo("Login parameters : " + JsonConvert.SerializeObject(loginParameters));
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                ServiceUserBL serviceUserBL = new ServiceUserBL(crmServiceHelper.IOrganizationService, crmServiceHelper.CrmServiceClient);
                var res = serviceUserBL.checkServiceUser(loginParameters);

                if (res.responseResult.result)
                {
                    Entity e = new Entity("rnt_serviceuser");
                    e.Id = res.userId;
                    e["rnt_tabletversion"] = loginParameters.version;
                    crmServiceHelper.IOrganizationService.Update(e);
                }
                return res;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new ClassLibrary._Tablet.LoginResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.createTransfer)]
        public CreateTransferResponse createTransfer([FromBody] CreateTransferParameter createTransferParameter)
        {
            string equipmentId = Convert.ToString(createTransferParameter.equipmentInformation.equipmentId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(equipmentId))
            {
                loggerHelper = new LoggerHelper(equipmentId + "CT");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("createTransfer start");
                loggerHelper.traceInputsInfo<CreateTransferParameter>(createTransferParameter, "createTransferParameter : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                RntCar.MongoDBHelper.Repository.TransferRepository transferRepository = new RntCar.MongoDBHelper.Repository.TransferRepository(mongoDBHostName, mongoDBDatabaseName);
                var t = transferRepository.getTransferDataByEquipmentId_Status(createTransferParameter.equipmentInformation.equipmentId);
                if (t != null)
                {
                    return new CreateTransferResponse
                    {
                        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(string.Format("Bu plakanın açık bir transferi bulunmaktadır , Yeni transfer açılabilmesi için öncelikle bu transferin kapatılması gerekmektedir.Transfer Numarası {0}",
                                                                                         t.transferNumber))
                    };
                }


                ContractItemRepository contractItemRepository = new ContractItemRepository(crmServiceHelper.IOrganizationService);
                var equipment = contractItemRepository.getRentalEquipment(createTransferParameter.equipmentInformation.equipmentId);
                if (equipment != null)
                {

                    if (StaticHelper.converttoDateTime(createTransferParameter.estimatedPickupTimestamp).AddMinutes(StaticHelper.offset)
                        .isBetween(equipment.GetAttributeValue<DateTime>("rnt_pickupdatetime"), equipment.GetAttributeValue<DateTime>("rnt_dropoffdatetime")) &&
                        StaticHelper.converttoDateTime(createTransferParameter.estimatedDropoffTimestamp).AddMinutes(StaticHelper.offset)
                        .isBetween(equipment.GetAttributeValue<DateTime>("rnt_pickupdatetime"), equipment.GetAttributeValue<DateTime>("rnt_dropoffdatetime")))
                    {
                        return new CreateTransferResponse
                        {
                            responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(string.Format("Bu plakanın {0} , {1} tarihleri arasında açık bir kirası bulunduğundan tranfer açılamaz",
                                                                                            equipment.GetAttributeValue<DateTime>("rnt_pickupdatetime").ToString("dd/MM/yyyy HH:mm"),
                                                                                            equipment.GetAttributeValue<DateTime>("rnt_dropoffdatetime").ToString("dd/MM/yyyy HH:mm")))
                        };

                    }
                }
                TransferBL transferBL = new TransferBL(crmServiceHelper.IOrganizationService, crmServiceHelper.CrmServiceClient);
                var response = transferBL.createTransfer(createTransferParameter);

                if (!response.Result)
                {
                    return new CreateTransferResponse
                    {
                        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(response.Message)
                    };
                }

                return new CreateTransferResponse
                {
                    transferId = response.transferId,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new CreateTransferResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.createDamage)]
        public CreateDamageResponse createDamage([FromBody] CreateDamageParameter createDamageParameter)
        {
            string equipmentId = Convert.ToString(createDamageParameter.equipmentId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(equipmentId))
            {
                loggerHelper = new LoggerHelper(equipmentId + "CD");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("createDamage start");
                loggerHelper.traceInputsInfo<CreateDamageParameter>(createDamageParameter, "createDamageParameter : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                ActionHelper actionHelper = new ActionHelper(crmServiceHelper.IOrganizationService);
                var response = actionHelper.createDamages(createDamageParameter);
                loggerHelper.traceInputsInfo<CreateDamageResponse>(response, "create damages response : ");
                return response;
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new CreateDamageResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.checkBeforeContractCreation)]
        public CheckBeforeContractCreationResponse checkBeforeContractCreation([FromBody] CheckBeforeContractCreationTabletParameters parameters)
        {
            try
            {
                LoggerHelper loggerHelper;
                if (parameters.reservationId != null && parameters.reservationId != Guid.Empty)
                {
                    string reservationId = Convert.ToString(parameters.reservationId);
                    reservationId = reservationId + "CBCC";
                    loggerHelper = new LoggerHelper(reservationId);
                }
                else
                {
                    loggerHelper = new LoggerHelper();
                }
                loggerHelper.traceInfo("checkBeforeContractCreation start");
                loggerHelper.traceInputsInfo<CheckBeforeContractCreationTabletParameters>(parameters, "CheckBeforeContractCreationTabletParameters : ");

                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                ActionHelper actionHelper = new ActionHelper(crmServiceHelper.IOrganizationService);

                // convert tablet class to business class 

                var convertedParams = new CheckBeforeContractCreationParameters
                {
                    contactId = parameters.contactId,
                    isQuickContract = true,
                    reservationId = parameters.reservationId,
                    pickupDateTime = parameters.pickupDateTimeStamp.converttoDateTime(),
                    langId = parameters.langId
                };

                loggerHelper.traceInputsInfo<CheckBeforeContractCreationParameters>(convertedParams, "convertedParams : ");

                var validationResponse = actionHelper.checkBeforeContractCreation(convertedParams);
                loggerHelper.traceInputsInfo<ContractValidationResponse>(validationResponse, "validationResponse : ");

                if (!validationResponse.ResponseResult.Result)
                {
                    return new CheckBeforeContractCreationResponse
                    {
                        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(validationResponse.ResponseResult.ExceptionDetail)
                    };
                }
                ReservationRepository reservationRepository = new ReservationRepository(crmServiceHelper.IOrganizationService);
                var res = reservationRepository.getReservationById(parameters.reservationId, new string[] { "rnt_ismonthly", "rnt_paymentmethodcode" });
                if (res.GetAttributeValue<bool>("rnt_ismonthly"))
                {
                    return new CheckBeforeContractCreationResponse
                    {
                        responseResult = RntCar.ClassLibrary._Tablet.ResponseResult.ReturnError("Aylık rezervasyonlar şubeden oluşturulmalıdır.")
                    };
                }

                return new CheckBeforeContractCreationResponse
                {
                    paymentMethod = res.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new CheckBeforeContractCreationResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }

        }
        [HttpPost]
        [Route(TabletRouteConfig.createQuickContract)]
        public CreateQuickContractResponse createQuickContract([FromBody] CreateQuickContractParameter createQuickContractParameter)
        {
            string reservationPNR = Convert.ToString(createQuickContractParameter.reservationPNR);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(reservationPNR))
            {
                loggerHelper = new LoggerHelper(reservationPNR + "CQC");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("createQuickContract start");
                loggerHelper.traceInputsInfo<CreateQuickContractParameter>(createQuickContractParameter, "createQuickContractParameter : ");

                #region Definitions
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                ActionHelper actionHelper = new ActionHelper(crmServiceHelper.IOrganizationService);
                loggerHelper.traceInfo("Definitions");
                #endregion

                #region Get reservation invoice
                loggerHelper.traceInfo("Get reservation invoice");

                ReservationRepository reservationRepository = new ReservationRepository(crmServiceHelper.IOrganizationService);
                var reservation = reservationRepository.getReservationById(Guid.Parse(createQuickContractParameter.reservationId), new string[] { "rnt_corporateid",
                                                                                                                                                   "rnt_reservationtypecode"});


                if (reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)RntCar.ClassLibrary._Enums_1033.rnt_ReservationTypeCode.Kurumsal)
                {

                    CorporateCustomerBL corporateCustomerBL = new CorporateCustomerBL(crmServiceHelper.IOrganizationService);
                    createQuickContractParameter.customerInformation.invoiceAddress = corporateCustomerBL.getCorporateCustomerAddress(reservation.GetAttributeValue<EntityReference>("rnt_corporateid").Id);
                }
                else
                {
                    InvoiceRepository invoiceRepository = new InvoiceRepository(crmServiceHelper.IOrganizationService);
                    var reservationInvoice = invoiceRepository.getInvoiceByReservationId(Guid.Parse(createQuickContractParameter.reservationId));
                    createQuickContractParameter.customerInformation.invoiceAddress = InvoiceHelper.buildInvoiceAddressDataFromInvoiceEntity(reservationInvoice);
                }
                #endregion



                #region Getting TRY from configuration
                loggerHelper.traceInfo("Getting TRY from configuration");
                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var currencyTRY = configurationBL.GetConfigurationByName("currency_TRY");
                createQuickContractParameter.currency = Guid.Parse(currencyTRY);
                #endregion

                #region Setting channel code
                loggerHelper.traceInfo("Setting channel code");
                createQuickContractParameter.channelCode = (int)ClassLibrary._Enums_1033.rnt_ReservationChannel.Tablet;
                #endregion

                var createResponse = actionHelper.createQuickContract(createQuickContractParameter);
                loggerHelper.traceInputsInfo<ContractCreateResponse>(createResponse, "createResponse: ");
                if (!createResponse.ResponseResult.Result)
                {
                    return new CreateQuickContractResponse
                    {
                        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(createResponse.ResponseResult.ExceptionDetail)
                    };
                }

                return new CreateQuickContractResponse
                {
                    contractId = createResponse.contractId,
                    contractPNR = createResponse.pnrNumber,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new CreateQuickContractResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.updateTransferForDelivery)]
        public UpdateTransferResponse updateTransferForDelivery([FromBody] UpdateTransferParameter updateTransferParameter)
        {
            string transferId = Convert.ToString(updateTransferParameter.transferId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(transferId))
            {
                loggerHelper = new LoggerHelper(transferId + "UTFD");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("updateTransferForDelivery start");
                loggerHelper.traceInputsInfo<UpdateTransferParameter>(updateTransferParameter, "updateTransferParameter : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                ActionHelper actionHelper = new ActionHelper(crmServiceHelper.CrmServiceClient, crmServiceHelper.IOrganizationService);
                loggerHelper.traceInfo("calling updateTransferForDelivery action");
                var response = actionHelper.updateTransferForDelivery(updateTransferParameter);

                return new UpdateTransferResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new UpdateTransferResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.updateTransferForReturn)]
        public UpdateTransferResponse updateTransferForReturn([FromBody] UpdateTransferParameter updateTransferParameter)
        {
            string transferId = Convert.ToString(updateTransferParameter.transferId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(transferId))
            {
                loggerHelper = new LoggerHelper(transferId + "UTFR");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("updateTransferForReturn start");
                loggerHelper.traceInputsInfo<UpdateTransferParameter>(updateTransferParameter, "updateTransferForReturnParameter : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                ActionHelper actionHelper = new ActionHelper(crmServiceHelper.CrmServiceClient, crmServiceHelper.IOrganizationService);
                loggerHelper.traceInfo("calling updateTransferForReturn action");
                var response = actionHelper.updateTransferForReturn(updateTransferParameter);

                //if (response.result)
                //{
                //    ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.CrmServiceClient);
                //    var connectionString = configurationRepository.GetConfigurationByKey("blobstorageconnectionstring");

                //    var containerName = updateTransferParameter.equipmentInformation.plateNumber;
                //    RntCar.MongoDBHelper.Repository.TransferRepository transferRepository =
                //                    new RntCar.MongoDBHelper.Repository.TransferRepository(mongoDBHostName, mongoDBDatabaseName);
                //    var transfer = transferRepository.getTransferById(updateTransferParameter.transferId);
                //    var blobName = transfer.transferNumber + "/rental";

                //    DamageBlobWrapper damageBlobWrapper = new DamageBlobWrapper(connectionString, containerName);
                //    damageBlobWrapper.createDamagePhotos(updateTransferParameter.damageList, blobName);
                //}

                return new UpdateTransferResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new UpdateTransferResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getAdditionalproducts)]
        public GetAdditionalProductsResponse getAdditionalProducts([FromBody] GetAdditionalProductsParameters getAdditionalProductsParameters)
        {
            string contractId = Convert.ToString(getAdditionalProductsParameters.contractId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(contractId))
            {
                loggerHelper = new LoggerHelper(contractId + "GAP");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getAdditionalProducts start");
                loggerHelper.traceInputsInfo<GetAdditionalProductsParameters>(getAdditionalProductsParameters, "getAdditionalProductsParameters : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                //prepare related parameters
                loggerHelper.traceInfo("getting groupcode information");
                GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(crmServiceHelper.IOrganizationService);
                var groupCodeEntity = groupCodeInformationRepository.getGroupCodeInformationById(getAdditionalProductsParameters.groupCodeId,
                                                                                                 new string[] {"rnt_youngdriverlicence",
                                                                                                               "rnt_youngdriverage",
                                                                                                               "rnt_minimumdriverlicence",
                                                                                                               "rnt_minimumage"});
                GroupCodeInformationCRMData groupCodeInformationCRMData = new GroupCodeInformationCRMData
                {
                    rnt_youngdriverlicence = groupCodeEntity.GetAttributeValue<int>("rnt_youngdriverlicence"),
                    rnt_youngdriverage = groupCodeEntity.GetAttributeValue<int>("rnt_youngdriverage"),
                    rnt_minimumage = groupCodeEntity.GetAttributeValue<int>("rnt_minimumage"),
                    rnt_minimumdriverlicence = groupCodeEntity.GetAttributeValue<int>("rnt_minimumdriverlicence")
                };
                loggerHelper.traceInputsInfo<GroupCodeInformationCRMData>(groupCodeInformationCRMData, "groupCodeInformationCRMData : ");

                loggerHelper.traceInfo("getting individualCustomer information");
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var individualEntity = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(getAdditionalProductsParameters.customerId,
                                                                                        new string[] {"birthdate",
                                                                                                      "rnt_drivinglicensedate"});
                IndividualCustomerDetailData individualCustomerDetailData = new IndividualCustomerDetailData
                {
                    birthDate = individualEntity.GetAttributeValue<DateTime>("birthdate"),
                    drivingLicenseDate = individualEntity.GetAttributeValue<DateTime>("rnt_drivinglicensedate")
                };

                loggerHelper.traceInputsInfo<IndividualCustomerDetailData>(individualCustomerDetailData, "individualCustomerDetailData : ");
                //get contractitem from mongodb
                loggerHelper.traceInfo("get contractitem from mongodb");
                ContractItemBusiness contractItemBusiness = new ContractItemBusiness(mongoDBHostName, mongoDBDatabaseName);
                var contractEquipment = contractItemBusiness.getContractEquipmentWaitingForDeliveryDateandBranchs(Convert.ToString(getAdditionalProductsParameters.contractId));

                ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                var contract = contractRepository.getContractById(getAdditionalProductsParameters.contractId, new string[] { "rnt_dropoffbranchid", "rnt_pickupbranchid" });

                ContractDateandBranchParameters contractDateandBranchParameters = new ContractDateandBranchParameters
                {
                    dropoffBranchId = contractEquipment.dropoffBranchId,
                    pickupBranchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                    dropoffDate = contractEquipment.dropoffDateTime,
                    pickupDate = contractEquipment.pickupDateTime
                };
                loggerHelper.traceInputsInfo<ContractDateandBranchParameters>(contractDateandBranchParameters, "contractDateandBranchParameters : ");
                //todo wwill read from somewhere generic
                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect(contractEquipment.pickupDateTime, contractEquipment.dropoffDateTime);
                loggerHelper.traceError("calculated total duration : " + totalDuration);

                AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(crmServiceHelper.IOrganizationService, crmServiceHelper.CrmServiceClient);
                var r = additionalProductsBL.GetAdditionalProductForUpdateContract_tablet(groupCodeInformationCRMData,
                                                                                          individualCustomerDetailData,
                                                                                          contractDateandBranchParameters,
                                                                                          Convert.ToString(getAdditionalProductsParameters.contractId),
                                                                                          totalDuration,
                                                                                          getAdditionalProductsParameters.langId);
                loggerHelper.traceInputsInfo<AdditionalProductResponse>(r, "AdditionalProductResponse : ");

                if (!r.ResponseResult.Result)
                {
                    return new GetAdditionalProductsResponse
                    {
                        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(r.ResponseResult.ExceptionDetail)
                    };
                }
                var _additonalProducts = r.AdditionalProducts.ConvertAll(p => new AdditionalProductMapper().createTabletModel(p)).ToList();

                AdditionalProductRuleRepository additionalProductRuleRepository = new AdditionalProductRuleRepository(crmServiceHelper.IOrganizationService);
                var additionalProductRulesEntity = additionalProductRuleRepository.getAdditonalProductRules();
                List<AdditionalProductRuleDataTablet> additionalProductRulesData = new List<AdditionalProductRuleDataTablet>();
                foreach (var item in additionalProductRulesEntity)
                {
                    additionalProductRulesData.Add(new AdditionalProductRuleDataTablet
                    {
                        parentProductId = item.GetAttributeValue<EntityReference>("rnt_parentproduct").Id,
                        parentProductName = item.GetAttributeValue<EntityReference>("rnt_parentproduct").Name,
                        productId = item.GetAttributeValue<EntityReference>("rnt_product").Id,
                        productName = item.GetAttributeValue<EntityReference>("rnt_product").Name,
                    });
                }

                loggerHelper.traceInputsInfo<List<AdditionalProductRuleDataTablet>>(additionalProductRulesData, "additionalProductRulesData : ");

                return new GetAdditionalProductsResponse
                {
                    additionalProductData = _additonalProducts,
                    additionalProductRuleData = additionalProductRulesData,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
                //return "i am ok";
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetAdditionalProductsResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getServiceadditionalproducts)]
        public GetAdditionalProductsResponse getServiceAdditionalProducts([FromBody] GetAdditionalProductsParameters getAdditionalProductsParameters)
        {
            string contractId = Convert.ToString(getAdditionalProductsParameters.contractId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(contractId))
            {
                loggerHelper = new LoggerHelper(contractId + "GSAP");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getAdditionalProducts start");
                loggerHelper.traceInputsInfo<GetAdditionalProductsParameters>(getAdditionalProductsParameters, "getAdditionalProductsParameters : ");
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                //prepare related parameters
                loggerHelper.traceInfo("getting groupcode information");
                GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(crmServiceHelper.IOrganizationService);
                var groupCodeEntity = groupCodeInformationRepository.getGroupCodeInformationById(getAdditionalProductsParameters.groupCodeId,
                                                                                                 new string[] {"rnt_youngdriverlicence",
                                                                                                               "rnt_youngdriverage",
                                                                                                               "rnt_minimumdriverlicence",
                                                                                                               "rnt_minimumage"});
                GroupCodeInformationCRMData groupCodeInformationCRMData = new GroupCodeInformationCRMData
                {
                    rnt_youngdriverlicence = groupCodeEntity.GetAttributeValue<int>("rnt_youngdriverlicence"),
                    rnt_youngdriverage = groupCodeEntity.GetAttributeValue<int>("rnt_youngdriverage"),
                    rnt_minimumage = groupCodeEntity.GetAttributeValue<int>("rnt_minimumage"),
                    rnt_minimumdriverlicence = groupCodeEntity.GetAttributeValue<int>("rnt_minimumdriverlicence")
                };
                loggerHelper.traceInputsInfo<GroupCodeInformationCRMData>(groupCodeInformationCRMData, "groupCodeInformationCRMData : ");

                loggerHelper.traceInfo("getting individualCustomer information");
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(crmServiceHelper.IOrganizationService);
                var individualEntity = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(getAdditionalProductsParameters.customerId,
                                                                                        new string[] {"birthdate",
                                                                                                      "rnt_drivinglicensedate"});
                IndividualCustomerDetailData individualCustomerDetailData = new IndividualCustomerDetailData
                {
                    birthDate = individualEntity.GetAttributeValue<DateTime>("birthdate"),
                    drivingLicenseDate = individualEntity.GetAttributeValue<DateTime>("rnt_drivinglicensedate")
                };

                loggerHelper.traceInputsInfo<IndividualCustomerDetailData>(individualCustomerDetailData, "individualCustomerDetailData : ");
                //get contractitem from mongodb
                loggerHelper.traceInfo("get contractitem from mongodb");
                ContractItemBusiness contractItemBusiness = new ContractItemBusiness(mongoDBHostName, mongoDBDatabaseName);
                var contractEquipment = contractItemBusiness.getContractEquipmentRentalDateandBranchs(Convert.ToString(getAdditionalProductsParameters.contractId));

                ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                var contract = contractRepository.getContractById(getAdditionalProductsParameters.contractId, new string[] { "rnt_dropoffbranchid", "rnt_pickupbranchid" });

                ContractDateandBranchParameters contractDateandBranchParameters = new ContractDateandBranchParameters
                {
                    dropoffBranchId = contractEquipment.dropoffBranchId,
                    pickupBranchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id,
                    dropoffDate = contractEquipment.dropoffDateTime,
                    pickupDate = contractEquipment.pickupDateTime
                };
                loggerHelper.traceInputsInfo<ContractDateandBranchParameters>(contractDateandBranchParameters, "contractDateandBranchParameters : ");
                //todo wwill read from somewhere generic
                DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
                var totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect(contractEquipment.pickupDateTime, contractEquipment.dropoffDateTime);
                loggerHelper.traceError("calculated total duration : " + totalDuration);

                AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(crmServiceHelper.IOrganizationService, crmServiceHelper.CrmServiceClient);
                var r = additionalProductsBL.GetServiceAdditionalProductForUpdateContract_tablet(groupCodeInformationCRMData,
                                                                                          individualCustomerDetailData,
                                                                                          contractDateandBranchParameters,
                                                                                          Convert.ToString(getAdditionalProductsParameters.contractId),
                                                                                          totalDuration,
                                                                                          getAdditionalProductsParameters.langId);
                loggerHelper.traceInputsInfo<AdditionalProductResponse>(r, "AdditionalProductResponse : ");

                if (!r.ResponseResult.Result)
                {
                    return new GetAdditionalProductsResponse
                    {
                        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(r.ResponseResult.ExceptionDetail)
                    };
                }
                var _additonalProducts = r.AdditionalProducts.ConvertAll(p => new AdditionalProductMapper().createTabletModel(p)).ToList();

                AdditionalProductRuleRepository additionalProductRuleRepository = new AdditionalProductRuleRepository(crmServiceHelper.IOrganizationService);
                var additionalProductRulesEntity = additionalProductRuleRepository.getAdditonalProductRules();
                List<AdditionalProductRuleDataTablet> additionalProductRulesData = new List<AdditionalProductRuleDataTablet>();
                foreach (var item in additionalProductRulesEntity)
                {
                    additionalProductRulesData.Add(new AdditionalProductRuleDataTablet
                    {
                        parentProductId = item.GetAttributeValue<EntityReference>("rnt_parentproduct").Id,
                        parentProductName = item.GetAttributeValue<EntityReference>("rnt_parentproduct").Name,
                        productId = item.GetAttributeValue<EntityReference>("rnt_product").Id,
                        productName = item.GetAttributeValue<EntityReference>("rnt_product").Name,
                    });
                }

                loggerHelper.traceInputsInfo<List<AdditionalProductRuleDataTablet>>(additionalProductRulesData, "additionalProductRulesData : ");

                return new GetAdditionalProductsResponse
                {
                    additionalProductData = _additonalProducts,
                    additionalProductRuleData = additionalProductRulesData,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
                //return "i am ok";
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetAdditionalProductsResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.updatecontractfordelivery)]
        public UpdateContractforDeliveryResponse updateContractforDelivery([FromBody] UpdateContractforDeliveryParameters updateContractforDeliveryParameters)
        {
            string contractId = Convert.ToString(updateContractforDeliveryParameters.contractInformation.contractId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(contractId))
            {
                loggerHelper = new LoggerHelper(contractId + "UCFD");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {

                loggerHelper.traceInfo("updateContractforDelivery start");
                loggerHelper.traceInputsInfo<UpdateContractforDeliveryParameters>(updateContractforDeliveryParameters, "updateContractforDeliveryParameters : ");

                RntCar.MongoDBHelper.Repository.ContractItemRepository contractItemRepository = new RntCar.MongoDBHelper.Repository.ContractItemRepository(mongoDBHostName, mongoDBDatabaseName);
                var contract = contractItemRepository.getContractItemEquipment(updateContractforDeliveryParameters.contractInformation.contractId.ToString());

                var utcNow = DateTime.Now;
                if (updateContractforDeliveryParameters.contractInformation.isManuelProcess)
                {
                    utcNow = updateContractforDeliveryParameters.contractInformation.manuelPickupDateTimeStamp.converttoDateTime();
                    updateContractforDeliveryParameters.contractInformation.PickupDateTimeStamp = contract.PickupTimeStamp.Value;
                    updateContractforDeliveryParameters.contractInformation.DropoffTimeStamp = contract.DropoffTimeStamp.Value;
                }
                updateContractforDeliveryParameters.dateNowTimeStamp = utcNow.converttoTimeStamp();

                if (updateContractforDeliveryParameters.changedEquipmentData != null &&
                   (updateContractforDeliveryParameters.changedEquipmentData.changeType == (int)rnt_ChangeType.Upsell ||
                   updateContractforDeliveryParameters.changedEquipmentData.changeType == (int)rnt_ChangeType.Downsell))
                {
                    MongoDBInstance mongoDBInstance = new MongoDBInstance(StaticHelper.GetConfiguration("MongoDBHostName"),
                                                                          StaticHelper.GetConfiguration("MongoDBDatabaseName"));

                    var availablityResCollection = mongoDBInstance._database.GetCollection<AvailabilityRes>("AvailabilityResponses")
                                                   .AsQueryable()
                                                   .Where(p => p.trackingNumber == updateContractforDeliveryParameters.changedEquipmentData.trackingNumber)
                                                   .FirstOrDefault();

                    var data = JsonConvert.DeserializeObject<List<RntCar.ClassLibrary.MongoDB.AvailabilityData>>(availablityResCollection.availabilityData);

                    var relatedData = data.Where(p => p.groupCodeInformationId == updateContractforDeliveryParameters.changedEquipmentData.groupCodeId).FirstOrDefault();
                    updateContractforDeliveryParameters.paymentPlans = relatedData.paymentPlanData;
                }

                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                ActionHelper actionHelper = new ActionHelper(crmServiceHelper.IOrganizationService);
                loggerHelper.traceInfo("calling UpdateContractforDelivery action");
                var res = actionHelper.UpdateContractforDelivery(updateContractforDeliveryParameters);

                RntCar.BusinessLibrary.ContractItemRepository itemRepository = new ContractItemRepository(crmServiceHelper.IOrganizationService);
                var rentalItem = itemRepository.getRentalEquipmentContractItemByGivenColumns(updateContractforDeliveryParameters.contractInformation.contractId,
                                                                                             new string[] { "rnt_pickupdatetime" });

                var collection = contractItemRepository.getCollection<ContractDailyPriceDataMongoDB>(StaticHelper.GetConfiguration("MongoDBContractDailyPrice"));
                RntCar.MongoDBHelper.Repository.ContractDailyPricesRepository priceCalculationRepository = new RntCar.MongoDBHelper.Repository.ContractDailyPricesRepository(mongoDBHostName, mongoDBDatabaseName);
                var prices = priceCalculationRepository.getContractDailyPricesByReservationItemId_str(rentalItem.Id);

                loggerHelper.traceInfo("getting MongoDBPriceCalculationSummary collection");

                var i = 0;
                foreach (var item in prices)
                {
                    item.priceDate = rentalItem.GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset).AddDays(i);
                    item.priceDateTimeStamp = new MongoDB.Bson.BsonTimestamp(rentalItem.GetAttributeValue<DateTime>("rnt_pickupdatetime").AddMinutes(-StaticHelper.offset).converttoTimeStamp());
                    var itemId = item._id.ToString();

                    var filter = Builders<ContractDailyPriceDataMongoDB>.Filter.Eq(p => p._id, item._id);
                    var result = collection.Replace(item, filter, new UpdateOptions { IsUpsert = false }, itemId, ErrorLogsHelper.GetCurrentMethod());
                    //loggerHelper.traceInputsInfo<PriceCalculationSummaryMongoDB>(item, "replaced price calculation : ");
                    //var result = collection.ReplaceOne(p => p._id == item._id, item, new UpdateOptions { IsUpsert = false });
                    i++;
                }

                return new UpdateContractforDeliveryResponse
                {
                    responseResult = res
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new UpdateContractforDeliveryResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.updateContractforRental)]
        public UpdateContractforRentalResponse updateContractforRental([FromBody] UpdateContractforRentalParameters updateContractforRentalParameters)
        {
            string contractId = Convert.ToString(updateContractforRentalParameters.contractInformation.contractId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(contractId))
            {
                loggerHelper = new LoggerHelper(contractId + "UCFR");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            var response = new List<PriceCalculationSummaryMongoDB>();

            try
            {
                loggerHelper.traceInfo("updateContractforRental start");
                loggerHelper.traceInfo("updateContractforRentalParameters : " + JsonConvert.SerializeObject(updateContractforRentalParameters));

                RntCar.MongoDBHelper.Repository.ContractItemRepository contractItemRepository = new RntCar.MongoDBHelper.Repository.ContractItemRepository(mongoDBHostName, mongoDBDatabaseName);
                var contract = contractItemRepository.getContractItemEquipment(updateContractforRentalParameters.contractInformation.contractId.ToString());

                MongoDBHelper.Repository.PriceCalculationSummariesRepository priceCalculationSummariesRepository = new MongoDBHelper.Repository.PriceCalculationSummariesRepository(mongoDBHostName, mongoDBDatabaseName);

                var trackingNumber = string.Empty;
                if (updateContractforRentalParameters.operationType.HasValue &&
                    updateContractforRentalParameters.operationType.Value == 50)
                {
                    trackingNumber = contract.trackingNumber;
                    updateContractforRentalParameters.trackingNumber = trackingNumber;
                }
                else
                {
                    trackingNumber = updateContractforRentalParameters.trackingNumber;
                }
                loggerHelper.traceInfo("trackingNumber " + trackingNumber);
                if (updateContractforRentalParameters.canUserStillHasCampaignBenefit)
                {
                    updateContractforRentalParameters.campaignId = contract.campaignId;
                }
                else
                {
                    var contractItems = contractItemRepository.getContractItemsEquipment(updateContractforRentalParameters.contractInformation.contractId.ToString());
                    var deliveredItems = contractItems.Where(p => p.statuscode == (int)rnt_contractitem_StatusCode.WaitingForDelivery).ToList();
                    var rentalItems = contractItems.Where(p => p.statuscode == (int)rnt_contractitem_StatusCode.Rental).ToList();
                    //araç değişikliğinde kullanabilirlik hesaplanmaz o yüzden sözleşmesini kampanyasını al
                    if (deliveredItems.Count == 1 && rentalItems.Count == 1)
                    {
                        updateContractforRentalParameters.campaignId = contract.campaignId;

                        if (contract.campaignId.HasValue && contract.campaignId.Value != Guid.Empty)
                        {
                            updateContractforRentalParameters.canUserStillHasCampaignBenefit = true;
                        }
                    }
                    else
                    {
                        //operastion type 50 olması 
                        //iadede para cıkarmaması ve iade de gün değişmiyorsa çektiğimiz parayı iade etmemek set edilir
                        //operation type 50  , kampanyadan önceden faydalanıyordu ve su an faydalanmıyorsa , 
                        //eski tracking number kampanyalı oldugunda o günlük fiyatları kampanyasız yeniden oluşturmak gerekiyor.
                        if (updateContractforRentalParameters.operationType.Value == 50 && contract.campaignId != null &&
                           !updateContractforRentalParameters.canUserStillHasCampaignBenefit)
                        {
                            response = priceCalculationSummariesRepository.getPriceCalculationSummariesforDailyPrices(contract.pricingGroupCodeId,
                                                                                                                      trackingNumber,
                                                                                                                      contract.campaignId);
                            trackingNumber = Guid.NewGuid().ToString();
                            foreach (var item in response)
                            {
                                MongoDBHelper.Entities.PriceCalculationSummariesBusiness priceCalculationSummariesBusiness
                                    = new PriceCalculationSummariesBusiness(mongoDBHostName, mongoDBDatabaseName);
                                item.trackingNumber = trackingNumber;
                                item.campaignId = Guid.Empty;
                                item._id = MongoDB.Bson.ObjectId.GenerateNewId();
                                priceCalculationSummariesBusiness.createPriceCalculationSummaryFromExisting(item);
                            }
                            updateContractforRentalParameters.trackingNumber = trackingNumber;
                        }
                        else if (updateContractforRentalParameters.operationType.Value == 0 && contract.campaignId != null &&
                           !updateContractforRentalParameters.canUserStillHasCampaignBenefit)
                        {
                            response.AddRange(priceCalculationSummariesRepository.getPriceCalculationSummariesforDailyPrices(contract.pricingGroupCodeId,
                                                                                                                             trackingNumber,
                                                                                                                             updateContractforRentalParameters.campaignId));
                        }
                        updateContractforRentalParameters.campaignId = null;

                    }
                }

                response.AddRange(priceCalculationSummariesRepository.getPriceCalculationSummariesforDailyPrices(contract.pricingGroupCodeId,
                                                                                                                 trackingNumber,
                                                                                                                 updateContractforRentalParameters.campaignId));


                loggerHelper.traceInfo("response length : " + response.Count);
                if (!updateContractforRentalParameters.equipmentInformation.isEquipmentChanged)
                {
                    ContractItemRepository contractItemRepositoryCrm = new ContractItemRepository(crmServiceHelper.IOrganizationService);
                    var manualDiscount = contractItemRepositoryCrm.getDiscountContractItem
                        (updateContractforRentalParameters.contractInformation.contractId, new string[] { "rnt_totalamount" });

                    if (manualDiscount != null)
                    {
                        var firstDayPrice = response.OrderBy(r => r.priceDateTimeStamp.Value).FirstOrDefault();
                        firstDayPrice.totalAmount += manualDiscount.GetAttributeValue<Money>("rnt_totalamount").Value;
                    }
                }
                var dropOffDateTime = DateTime.MinValue;
                if (updateContractforRentalParameters.contractInformation.isManuelProcess)
                {
                    dropOffDateTime = updateContractforRentalParameters.contractInformation.manuelDropoffTimeStamp.converttoDateTime();
                }
                else
                {
                    dropOffDateTime = DateTime.UtcNow;
                }

                #region //iadede para çıkarmaması için
                DurationHelper durationHelper = new DurationHelper(mongoDBHostName, mongoDBDatabaseName);
                var documentDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(contract.pickupDateTime.Value, contract.dropoffDateTime.Value);
                var paramDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(updateContractforRentalParameters.contractInformation.PickupDateTimeStamp.converttoDateTime(),
                                                                                                       dropOffDateTime);

                var tAmount = response.Sum(p => p.totalAmount);
                //erken getirmelerde para cıkarmamaı için
                if (documentDuration > paramDuration && tAmount > contract.totalAmount)
                {
                    loggerHelper.traceInfo("document duration is and amount bigger");

                    updateContractforRentalParameters.totalAmount = contract.totalAmount;
                }
                else
                {
                    loggerHelper.traceInfo("standaart pricing");

                    updateContractforRentalParameters.totalAmount = response.Sum(p => p.totalAmount);
                }
                #endregion

                //updateContractforRentalParameters.totalAmount = response.Sum(p => p.totalAmount);

                if (updateContractforRentalParameters.totalAmount == 0)
                {
                    ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                    var contractCRM = contractRepository.getContractById(new Guid(contract.contractId));
                    Money generalTotalAmount = contractCRM.GetAttributeValue<Money>("rnt_generaltotalamount");
                    if (generalTotalAmount.Value == 0)
                    {
                        if (!contract.isClosedAmountZero)
                        {
                            return new UpdateContractforRentalResponse
                            {
                                responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError("Sözleşme 0.00 TL olarak kapatılamaz. Kapatılması için lütfen sistem yöneticisi ile görüşün")
                            };
                        }
                    }
                    else
                    {
                        return new UpdateContractforRentalResponse
                        {
                            responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError("Fiyatlar hesaplanamadı, lütfen sistem yöneticisi ile görüşün")
                        };
                    }
                }
                loggerHelper.traceInfo("updateContractforRentalParameters.totalAmount: " + tAmount);

                ActionHelper actionHelper = new ActionHelper(crmServiceHelper.IOrganizationService);
                loggerHelper.traceInfo("calling UpdateContractforRental action");
                var res = actionHelper.UpdateContractforRental(updateContractforRentalParameters);

                return new UpdateContractforRentalResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);

                if (ex.Message.Contains(StaticHelper.paymentErrorPrefix))
                {
                    var errorMessage = ex.Message.Replace(StaticHelper.paymentErrorPrefix, string.Empty);
                    return new UpdateContractforRentalResponse
                    {
                        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(errorMessage),
                        hasPaymentError = true
                    };
                }

                return new UpdateContractforRentalResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };

            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getCreditCardsByCustomer)]
        public getCreditCardsByCustomerResponse getCreditCardsByCustomer([FromBody] getCustomerCreditCardsParameters getCustomerCreditCardsParameters)
        {
            string contractId = Convert.ToString(getCustomerCreditCardsParameters.contractId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(contractId))
            {
                loggerHelper = new LoggerHelper(contractId + "GCCBC");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getCreditCardsByCustomer start");
                loggerHelper.traceInputsInfo<getCustomerCreditCardsParameters>(getCustomerCreditCardsParameters, "getCustomerCreditCardsParameters : ");

                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
                CreditCardBL creditCardBL = new CreditCardBL(crmServiceHelper.IOrganizationService);
                var response = creditCardBL.getCustomerCreditCards(Convert.ToString(getCustomerCreditCardsParameters.customerId), getCustomerCreditCardsParameters.reservationId, getCustomerCreditCardsParameters.contractId, Guid.Empty);

                loggerHelper.traceInputsInfo<GetCustomerCreditCardsResponse>(response, "GetCustomerCreditCardsResponse : ");

                return new getCreditCardsByCustomerResponse
                {
                    creditCardList = response.creditCards,
                    selectedCreditCardId = response.selectedCreditCardId,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new getCreditCardsByCustomerResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.calculateContractRemainingAmount)]
        public CalculateContractRemainingAmountResponse calculateContractRemainingAmount([FromBody] CalculateContractRemainingAmountParameters calculateContractRemainingAmountParameter)
        {
            string equipmentId = Convert.ToString(calculateContractRemainingAmountParameter.equipmentInformation.equipmentId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(equipmentId))
            {
                loggerHelper = new LoggerHelper(equipmentId + "CCRA");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            loggerHelper.traceInputsInfo<CalculateContractRemainingAmountParameters>(calculateContractRemainingAmountParameter);

            List<Task> _tasks = new List<Task>();
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();

            ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(crmServiceHelper.IOrganizationService);

            var offset = StaticHelper.offset;
            var _dropoffDateTime = DateTime.MinValue;
            if (calculateContractRemainingAmountParameter.contractInformation.isManuelProcess)
            {
                _dropoffDateTime = calculateContractRemainingAmountParameter.contractInformation.manuelDropoffTimeStamp.converttoDateTime();
            }
            else
            {
                _dropoffDateTime = DateTime.UtcNow;
            }
            loggerHelper.traceInfo("_dropoffDateTime : " + _dropoffDateTime);
            loggerHelper.traceInfo("calculateContractRemainingAmountParameter.equipmentInformation.isEquipmentChanged : " + calculateContractRemainingAmountParameter.equipmentInformation.isEquipmentChanged);
            try
            {
                RntCar.MongoDBHelper.Repository.ContractItemRepository contractItemRepository =
                    new RntCar.MongoDBHelper.Repository.ContractItemRepository(mongoDBHostName, mongoDBDatabaseName);
                var contractItem = contractItemRepository.getContractItemEquipmentRental(Convert.ToString(calculateContractRemainingAmountParameter.contractInformation.contractId));

                ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                var contract = contractRepository.getContractById(calculateContractRemainingAmountParameter.contractInformation.contractId, new string[] { "rnt_pickupbranchid", "rnt_pickupdatetime", "rnt_dropoffdatetime" });

                if (!calculateContractRemainingAmountParameter.equipmentInformation.isEquipmentChanged)
                {
                    #region Calculate OneWay Fee         

                    var taskOneWay = new Task(() =>
                {
                    calculateOneWayFee(crmServiceHelper.IOrganizationService,
                                       calculateContractRemainingAmountParameter,
                                       contractItem,
                                       contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id);
                });
                    _tasks.Add(taskOneWay);
                    taskOneWay.Start();
                    #endregion
                }
                if (!calculateContractRemainingAmountParameter.equipmentInformation.isEquipmentChanged)
                {
                    #region Calculte KM Fee
                    var taskKm = new Task(() =>
                    {
                        loggerHelper.traceInfo("contractItem.kilometerLimit : " + contractItem.kilometerLimit);
                        loggerHelper.traceInfo("pickupDateTime for km : " + (contractItem.pickupDateTime_Header.HasValue ?
                                                                             contractItem.pickupDateTime_Header.Value.AddMinutes(-StaticHelper.offset) :
                                                                             contractItem.PickupTimeStamp.Value.converttoDateTime()));
                        calculateKM(crmServiceHelper.IOrganizationService,
                                    calculateContractRemainingAmountParameter,
                                    contractItem,
                                    _dropoffDateTime);
                    });
                    _tasks.Add(taskKm);
                    taskKm.Start();

                    #endregion
                }

                #region Equipment Transaction
                var taskEquipmentTransaction = new Task(() =>
                {
                    calculateEquipmentTransaction(crmServiceHelper.IOrganizationService,
                                       calculateContractRemainingAmountParameter,
                                       contractItem);
                });
                _tasks.Add(taskEquipmentTransaction);
                taskEquipmentTransaction.Start();

                #endregion

                #region Equipment Inventory
                //todo inventory history will be retrieved from mongodb for having the same logic.
                //in all webservice methods are retrieved from mongodb for equipment
                var taskCalculateEquipmentInventory = new Task(() =>
                {
                    calculateEquipmentInventory(crmServiceHelper.IOrganizationService,
                                               calculateContractRemainingAmountParameter,
                                               contractItem);
                });
                _tasks.Add(taskCalculateEquipmentInventory);
                taskCalculateEquipmentInventory.Start();

                #endregion

                #region Calculate Prices for Additional Products
                if (!calculateContractRemainingAmountParameter.equipmentInformation.isEquipmentChanged)
                {
                    var taskadditionalProducts = new Task(() =>
                {
                    calculateAdditionalProducts(crmServiceHelper.IOrganizationService,
                                                contractItem,
                                                _dropoffDateTime,
                                                calculateContractRemainingAmountParameter);
                });
                    _tasks.Add(taskadditionalProducts);
                    taskadditionalProducts.Start();
                }
                #endregion

                var response = new CalculatePricesForUpdateContractResponse();
                response.trackingNumber = contractItem.trackingNumber;
                response.ResponseResult = ClassLibrary.ResponseResult.ReturnSuccess();

                if (!calculateContractRemainingAmountParameter.equipmentInformation.isEquipmentChanged)
                {
                    #region Availability

                    //check it is corporate
                    var customerType = 0;
                    //fucking stupid enum wrong using!
                    //broker == 3
                    //acenta == 4
                    if (contractItem.contractType == (int)rnt_ReservationTypeCode.Broker)
                    {
                        customerType = (int)rnt_ReservationTypeCode.Acente / 10;
                    }
                    else if (contractItem.contractType == (int)rnt_ReservationTypeCode.Acente)
                    {
                        customerType = (int)rnt_ReservationTypeCode.Broker / 10;
                    }
                    else
                    {
                        customerType = contractItem.contractType / 10;
                    }
                    string accountGroup = string.Empty;
                    if (!string.IsNullOrEmpty(contractItem.corporateCustomerId))
                    {
                        CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(crmServiceHelper.IOrganizationService);
                        var corporateCustomer = corporateCustomerRepository.getCorporateCustomerById(new Guid(contractItem.corporateCustomerId),
                                                                                                     new string[] { "rnt_pricefactorgroupcode" });
                        if (corporateCustomer.Contains("rnt_pricefactorgroupcode"))
                        {
                            accountGroup = corporateCustomer.GetAttributeValue<OptionSetValue>("rnt_pricefactorgroupcode").Value.ToString();
                        }
                    }
                    DurationHelper durationHelper = new DurationHelper(mongoDBHostName, mongoDBDatabaseName);
                    var paramDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(contractItem.pickupDateTime.Value, _dropoffDateTime);
                    var documentDuration = durationHelper.calculateDocumentDurationByPriceHourEffect_Decimal(contractItem.pickupDateTime.Value, contractItem.dropoffDateTime.Value);


                    loggerHelper.traceInfo("param pickup : " + contractItem.pickupDateTime.Value);
                    loggerHelper.traceInfo("param dropoff : " + _dropoffDateTime);

                    loggerHelper.traceInfo("document pickup : " + contractItem.pickupDateTime.Value);
                    loggerHelper.traceInfo("document dropoff : " + contractItem.dropoffDateTime.Value);

                    loggerHelper.traceInfo("paramDuration : " + paramDuration);
                    loggerHelper.traceInfo("documentDuration : " + documentDuration);
                    var availabilityResponse = new RntCar.ClassLibrary.MongoDB.AvailabilityResponse();
                    MongoDBHelper.Entities.AvailabilityBusiness availabilityBusiness = new AvailabilityBusiness(new ClassLibrary.MongoDB.AvailabilityParameters
                    {
                        accountGroup = accountGroup,
                        channel = (int)GlobalEnums.Channel.Branch,
                        //option set values are 10-20-30-40 respectively
                        customerType = customerType,
                        contractId = contractItem.contractId,
                        individualCustomerId = calculateContractRemainingAmountParameter.contractInformation.contactId.ToString(),
                        corporateCustomerId = contractItem.corporateCustomerId,
                        dropOffBranchId = calculateContractRemainingAmountParameter.dropoffBranch.branchId.Value,
                        dropoffDateTime = _dropoffDateTime,
                        pickupDateTime = contractItem.PickupTimeStamp.Value.converttoDateTime(),
                        pickupBranchId = new Guid(contractItem.pickupBranchId),
                        segment = calculateContractRemainingAmountParameter.contractInformation.segment,
                        //todo enum will be consider
                        operationType = 50,
                        priceCodeId = calculateContractRemainingAmountParameter.contractInformation.priceCodeId,
                        processIndividualPrices_broker = contractItem.processIndividualPrices,
                        checkGroupClosure = false,
                        month_pickupdatetime = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                        month_dropoffdatetime = contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime")
                    });
                    availabilityResponse.availabilityData = availabilityBusiness.calculateAvailability();
                    availabilityResponse.trackingNumber = availabilityBusiness.trackingNumber;
                    availabilityResponse.availabilityData = availabilityResponse.availabilityData.Where(p => p.groupCodeInformationId.Equals(new Guid(contractItem.pricingGroupCodeId))).ToList();

                    AvailibilityBL availibilityBL = new AvailibilityBL(crmServiceHelper.IOrganizationService);
                    availibilityBL.processErrorMessages(availabilityResponse, calculateContractRemainingAmountParameter.langId);

                    response = availibilityBL.calculatePricesUpdateContract(availabilityResponse,
                                                                            new Guid(contractItem.pricingGroupCodeId), new Guid(contractItem.contractId));
                    #endregion
                }

                Task.WaitAll(_tasks.ToArray());

                var otherCostProductCode = configurationRepository.GetConfigurationByKey("additionalProduct_otherkey");
                var additionalProductCacheKey = configurationRepository.GetConfigurationByKey("additionalProduct_cacheKey");

                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(crmServiceHelper.IOrganizationService, StaticHelper.prepareAdditionalProductCacheKey(otherCostProductCode, additionalProductCacheKey));
                var otherCostProduct = additionalProductCacheClient.getAdditionalProductCache(otherCostProductCode);
                var otherCostProductData = new AdditionalProductMapper().createTabletModel(otherCostProduct);


                if (!response.ResponseResult.Result)
                {
                    return new CalculateContractRemainingAmountResponse
                    {
                        responseResult = new ClassLibrary._Tablet.ResponseResult
                        {
                            exceptionDetail = response.ResponseResult.ExceptionDetail,
                            result = false
                        }
                    };
                }

                var res = new CalculateContractRemainingAmountResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess(),
                    otherAdditionalProductData = otherAdditonalProductDataTablets,
                    additionalProductResponse = additonalProductResponse,
                    otherCostAdditionalProductData = otherCostProductData,
                    calculatePricesForUpdateContractResponse = response,
                    dateNow = _dropoffDateTime.converttoTimeStamp()
                };

                loggerHelper.traceInfo("response : " + JsonConvert.SerializeObject(res));
                return res;

            }
            catch (Exception ex)
            {
                loggerHelper.traceInfo("error  : " + ex.Message);
                return new CalculateContractRemainingAmountResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.calculateAvailability)]
        public CalculateAvailabilityResponse calculateAvailability(CalculateAvailabilityParameters calculateAvailabilityParameters)
        {
            LoggerHelper loggerHelper = new LoggerHelper();
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            try
            {
                loggerHelper.traceInfo("calculateAvailability start");
                loggerHelper.traceInputsInfo<CalculateAvailabilityParameters>(calculateAvailabilityParameters, "calculateAvailabilityParameters : ");

                RntCar.MongoDBHelper.Repository.ContractItemRepository contractItemRepository =
                   new RntCar.MongoDBHelper.Repository.ContractItemRepository(mongoDBHostName, mongoDBDatabaseName);
                var contractItem = contractItemRepository.getContractItemEquipmentWaitingForDelivery(Convert.ToString(calculateAvailabilityParameters.contractInformation.contractId));

                var customerType = 0;
                //fucking stupid enum wrong using!
                if (contractItem.contractType == (int)rnt_ReservationTypeCode.Broker)
                {
                    customerType = (int)rnt_ReservationTypeCode.Acente / 10;
                }
                else if (contractItem.contractType == (int)rnt_ReservationTypeCode.Acente)
                {
                    customerType = (int)rnt_ReservationTypeCode.Broker / 10;
                }
                else
                {
                    customerType = contractItem.contractType / 10;
                }

                loggerHelper.traceInfo("customerType : " + customerType);

                var availabilityResponse = new RntCar.ClassLibrary.MongoDB.AvailabilityResponse();

                loggerHelper.traceInfo("create availability business object");
                string accountGroup = string.Empty;
                Guid monthlyPriceCodeId = Guid.Empty;

                if (!string.IsNullOrEmpty(contractItem.corporateCustomerId))
                {
                    CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(crmServiceHelper.IOrganizationService);
                    var corporateCustomer = corporateCustomerRepository.getCorporateCustomerById(new Guid(contractItem.corporateCustomerId),
                                                                                                 new string[] { "rnt_pricefactorgroupcode", "rnt_monthlypricecodeid" });
                    if (corporateCustomer.Contains("rnt_pricefactorgroupcode"))
                    {
                        accountGroup = corporateCustomer.GetAttributeValue<OptionSetValue>("rnt_pricefactorgroupcode").Value.ToString();
                    }
                    if (contractItem.isMonthly)
                    {
                        monthlyPriceCodeId = corporateCustomer.GetAttributeValue<EntityReference>("rnt_monthlypricecodeid").Id;
                    }
                }

                MongoDBHelper.Entities.AvailabilityBusiness availabilityBusiness = new AvailabilityBusiness(new ClassLibrary.MongoDB.AvailabilityParameters
                {
                    isMonthly = contractItem.isMonthly,
                    monthValue = contractItem.monthValue,
                    channel = (int)GlobalEnums.Channel.Branch,
                    //option set values are 10-20-30-40 respectively
                    customerType = customerType,
                    contractId = contractItem.contractId,
                    individualCustomerId = calculateAvailabilityParameters.contractInformation.contactId.ToString(),
                    corporateCustomerId = contractItem.corporateCustomerId,
                    dropOffBranchId = calculateAvailabilityParameters.dropoffBranch.branchId.Value,
                    dropoffDateTime = contractItem.DropoffTimeStamp.Value.converttoDateTime(),
                    pickupDateTime = contractItem.PickupTimeStamp.Value.converttoDateTime(),
                    pickupBranchId = new Guid(contractItem.pickupBranchId),
                    segment = calculateAvailabilityParameters.contractInformation.segment,
                    priceCodeId = contractItem.isMonthly ? monthlyPriceCodeId : calculateAvailabilityParameters.contractInformation.priceCodeId,
                    accountGroup = accountGroup,
                    checkGroupClosure = false,
                    operationType = 30
                });
                loggerHelper.traceInfo("calculate availability");
                availabilityResponse.availabilityData = availabilityBusiness.calculateAvailability();
                loggerHelper.traceInputsInfo<List<ClassLibrary.MongoDB.AvailabilityData>>(availabilityResponse.availabilityData, "availabilityResponse.availabilityData : ");
                availabilityResponse.trackingNumber = availabilityBusiness.trackingNumber;
                loggerHelper.traceInfo("availabilityResponse.trackingNumber : " + availabilityResponse.trackingNumber);
                availabilityResponse.availabilityData = availabilityResponse.availabilityData;

                AvailibilityBL availibilityBL = new AvailibilityBL(crmServiceHelper.IOrganizationService);
                availibilityBL.processErrorMessages(availabilityResponse, calculateAvailabilityParameters.langId);

                var currentGroupCodeAvailability = availabilityResponse.availabilityData.Where(p => p.groupCodeInformationId.Equals(new Guid(contractItem.pricingGroupCodeId))).ToList();
                loggerHelper.traceInputsInfo<List<ClassLibrary.MongoDB.AvailabilityData>>(currentGroupCodeAvailability, "currentGroupCodeAvailability : ");
                var otherGroupCodesAvailability = availabilityResponse.availabilityData.Where(p => p.groupCodeInformationId != new Guid(contractItem.pricingGroupCodeId)).ToList();
                loggerHelper.traceInputsInfo<List<ClassLibrary.MongoDB.AvailabilityData>>(otherGroupCodesAvailability, "otherGroupCodesAvailability : ");
                var result = availibilityBL.decideChangedGroupCodeStatus(currentGroupCodeAvailability.FirstOrDefault(), otherGroupCodesAvailability);
                loggerHelper.traceInputsInfo<ClassLibrary.MongoDB.AvailabilityResponse>(result, "AvailabilityResponse : ");

                var resultCurrent = availibilityBL.decideChangedGroupCodeStatus(currentGroupCodeAvailability.FirstOrDefault(), currentGroupCodeAvailability);
                result.availabilityData.AddRange(resultCurrent.availabilityData);

                GroupCodeInformationBL groupCodeInformationBL = new GroupCodeInformationBL(crmServiceHelper.IOrganizationService);
                var groupCodeInformationDetail = groupCodeInformationBL.getGroupCodeInformationDetailForDocument(currentGroupCodeAvailability.FirstOrDefault().groupCodeInformationId);

                currentGroupCodeAvailability.FirstOrDefault().upgradeGroupCodes = groupCodeInformationDetail.upgradeGroupCodes;

                result.availabilityData = result.availabilityData.Where(p => p.isPriceCalculatedSafely == true && p.hasError == false).ToList();
                loggerHelper.traceInputsInfo<List<ClassLibrary.MongoDB.AvailabilityData>>(result.availabilityData, "new availabilityData : ");

                ConfigurationBL configurationBL = new ConfigurationBL(crmServiceHelper.IOrganizationService);
                var cacheKey = configurationBL.GetConfigurationByName("groupcode_cacheKey");

                List<TabletAvailabilityData> availabilityDatas = new List<TabletAvailabilityData>();

                #region checking deposit amount

                Guid customer = calculateAvailabilityParameters.contractInformation.contactId;
                Entity contract = crmServiceHelper.IOrganizationService.Retrieve("rnt_contract", calculateAvailabilityParameters.contractInformation.contractId, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_paymentmethodcode"));
                var paymentCode = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;

                bool isCustomerPersonnal = true;
                bool isDepositZero = false;

                if (customer != Guid.Empty)
                {
                    IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(crmServiceHelper.IOrganizationService);
                    isCustomerPersonnal = individualCustomerBL.checkUserISVIPorStaff(customer);
                }
                if (isCustomerPersonnal &&
                    (paymentCode == (int)rnt_PaymentMethodCode.Corporate || paymentCode == (int)rnt_PaymentMethodCode.FullCredit || paymentCode == (int)rnt_PaymentMethodCode.Current))
                {
                    isDepositZero = true;
                }

                #endregion

                loggerHelper.traceInfo("prepare availability data for tablet with group code information");
                foreach (var item in result.availabilityData)
                {
                    //Check Redis Cache for group code information
                    GroupCodeInformationCacheClient groupCodeInformationCacheClient = new GroupCodeInformationCacheClient(crmServiceHelper.IOrganizationService,
                                                                                                                          crmServiceHelper.CrmServiceClient,
                                                                                                                          cacheKey);
                    var groupCode = groupCodeInformationCacheClient.getGroupCodeInformationDetailCache(item.groupCodeInformationId);

                    if (groupCode.stateCode != (int)GlobalEnums.StateCode.Active)
                    {
                        continue;
                    }
                    TabletAvailabilityData availabilityData = new TabletAvailabilityData
                    {
                        groupCodeId = groupCode.groupCodeInformationId,
                        groupCodeName = groupCode.groupCodeInformationName,
                        groupCodeImage = groupCode.image,
                        totalPrice = item.totalPrice == decimal.MinValue ? decimal.Zero : item.totalPrice,
                        amountToBePaid = (item.totalPrice == decimal.MinValue ? currentGroupCodeAvailability.FirstOrDefault().totalPrice : item.totalPrice) - currentGroupCodeAvailability.FirstOrDefault().totalPrice,
                        fuelTypeName = groupCode.fueltypecodeName,
                        transmissionName = groupCode.gearboxcodeName,
                        //todo by langId
                        displayText = groupCode.showRoomModelName + " ve benzeri",
                        isDowngrade = item.isDowngrade,
                        isUpgrade = item.isUpgrade,
                        isUpsell = item.isUpsell,
                        isDownsell = item.isDownsell,
                        upgradeGroupCodes= item.upgradeGroupCodes,
                        depositAmount = isDepositZero ? 0 : groupCode.deposit,
                        isPriceCalculatedSafely = item.isPriceCalculatedSafely

                    };
                    availabilityDatas.Add(availabilityData);
                }
                loggerHelper.traceInfo("order by descending availabilityData");
                availabilityDatas = availabilityDatas.OrderByDescending(p => p.totalPrice).ToList();
                CrmConfigurationBusiness crmConfigurationBusiness = new CrmConfigurationBusiness(mongoDBHostName, mongoDBDatabaseName);
                var upgradeenabledmonthly = crmConfigurationBusiness.getCrmConfigurationByKey<string>("upgradedisabledmonthly");
                loggerHelper.traceInfo(string.Format("upgradedisabledmonthly : {0}", upgradeenabledmonthly));


                if (contractItem.isMonthly && upgradeenabledmonthly == "1")
                {
                    availabilityDatas = availabilityDatas.Where(p => p.groupCodeId == new Guid(contractItem.pricingGroupCodeId)).ToList();
                }
                return new CalculateAvailabilityResponse
                {
                    trackingNumber = availabilityResponse.trackingNumber,
                    availabilityData = availabilityDatas,
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new CalculateAvailabilityResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        [HttpPost]
        [Route(TabletRouteConfig.getLogs)]
        public List<LogDetail> getLogs(LogParameters logParameters)
        {

            RntCar.MongoDBHelper.Repository.LogRepository logRepository =
               new RntCar.MongoDBHelper.Repository.LogRepository(mongoDBHostName, mongoDBDatabaseName);

            var logs = logRepository.getLogs(logParameters.messageName, logParameters.keyWord);

            var convertedLogs = logs.ConvertAll(p => new LogDetail().Map(p));

            return convertedLogs.OrderByDescending(p => p.createdOn).ToList();
        }
        [HttpPost]
        [Route("api/service/getTabletLogs")]
        public List<LogDetail> getTabletLogs(LogParameters logParameters)
        {

            RntCar.MongoDBHelper.Repository.LogRepository logRepository =
                new RntCar.MongoDBHelper.Repository.LogRepository(mongoDBHostName, mongoDBDatabaseName);
            var logs = logRepository.getTabletLogs(logParameters.messageName, logParameters.keyWord);
            var convertedLogs = logs.ConvertAll(p => new LogDetail
            {
                messageName = string.Join(", ", p.message.ToArray()),
                typeName = p.method,
                messageBlock = p.controller,
                createdOn = Convert.ToDateTime(p.date),

            }).ToList(); ;
            return convertedLogs.OrderByDescending(p => p.createdOn).ToList();
        }



        private void calculateOneWayFee(IOrganizationService organizationService,
                                        CalculateContractRemainingAmountParameters calculateContractRemainingAmountParameters,
                                        ContractItemDataMongoDB contractInformation,
                                        Guid pickupBranchId)
        {
            // if equipment changed, no need to calculate one way
            if (!calculateContractRemainingAmountParameters.equipmentInformation.isEquipmentChanged)
            {
                ConfigurationRepository configurationRepository = new ConfigurationRepository(organizationService);
                var oneWayFeeCode = configurationRepository.GetConfigurationByKey("OneWayFeeCode");
                var additionalProductCacheKey = configurationRepository.GetConfigurationByKey("additionalProduct_cacheKey");
                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(organizationService, StaticHelper.prepareAdditionalProductCacheKey(oneWayFeeCode, additionalProductCacheKey));
                var data = additionalProductCacheClient.getAdditionalProductCache(oneWayFeeCode);

                ContractItemRepository contractItemRepository = new ContractItemRepository(organizationService);
                // check contract has one way fee product
                var contractItemOneWayFee = contractItemRepository.getContractItemAdditionalProductByContractIdandAdditionalProductIdByGivenColumns(new Guid(contractInformation.contractId),
                                                                                                                                                    data.productId,
                                                                                                                                                    new string[] { "rnt_totalamount" });

                OneWayFeeBL oneWayFeeBL = new OneWayFeeBL(organizationService);
                var oneWayFeeAmount = oneWayFeeBL.getOneWayFee(pickupBranchId,
                                                               calculateContractRemainingAmountParameters.dropoffBranch.branchId.Value);
                // if contract has one way fee 
                if (contractItemOneWayFee != null)
                {
                    var contractItemOneWayFeeTotalAmount = contractItemOneWayFee.GetAttributeValue<Money>("rnt_totalamount").Value;

                    // if one way fee amount grather then zero and value does not eq the contract amount, calculate new amount
                    if ((oneWayFeeAmount > decimal.Zero) && oneWayFeeAmount != contractItemOneWayFeeTotalAmount)
                    {
                        if (new Guid(contractInformation.dropoffBranchId) != calculateContractRemainingAmountParameters.dropoffBranch.branchId)
                        {
                            data.tobePaidAmount = oneWayFeeAmount - contractItemOneWayFeeTotalAmount;
                            data.actualAmount = oneWayFeeAmount;
                            data.actualTotalAmount = oneWayFeeAmount;
                            //add to global list
                            otherAdditonalProductDataTablets.Add(new AdditionalProductMapper().createTabletModel(data));
                        }
                    }
                    // if there is no one way fee anymore (pickupbranch eq to dropoffbranch), refund to contract one way fee 
                    else if (contractInformation.pickupBranchId.Equals(calculateContractRemainingAmountParameters.dropoffBranch.branchId.Value.ToString()))
                    {
                        data.tobePaidAmount = (contractItemOneWayFeeTotalAmount * -1);
                        data.actualAmount = contractItemOneWayFeeTotalAmount;
                        data.actualTotalAmount = contractItemOneWayFeeTotalAmount;
                        //add to global list
                        otherAdditonalProductDataTablets.Add(new AdditionalProductMapper().createTabletModel(data));
                    }
                }
                else if (oneWayFeeAmount > decimal.Zero)
                {
                    data.tobePaidAmount = oneWayFeeAmount;
                    data.actualAmount = oneWayFeeAmount;
                    data.actualTotalAmount = oneWayFeeAmount;
                    //add to global list
                    otherAdditonalProductDataTablets.Add(new AdditionalProductMapper().createTabletModel(data));
                }
            }
        }

        private void calculateKM(IOrganizationService organizationService,
                                 CalculateContractRemainingAmountParameters calculateContractRemainingAmountParameters,
                                 ContractItemDataMongoDB contractInformation,
                                 DateTime droppoffDateTime)
        {
            var kmDiff = calculateContractRemainingAmountParameters.equipmentInformation.currentKmValue -
                         calculateContractRemainingAmountParameters.equipmentInformation.firstKmValue;

            EquipmentTransactionBL equipmentTransactionBL = new EquipmentTransactionBL(organizationService);
            var kilometerDiffSum = equipmentTransactionBL.getCompletedEquipmentTransactionHistoryByContractId(new Guid(contractInformation.contractId));

            var totalKm = kmDiff + kilometerDiffSum;
            //check km 
            ContractHelper contractHelper = new ContractHelper(organizationService);
            var currentDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(contractInformation.pickupDateTime_Header.HasValue ?
                                                                                                  contractInformation.pickupDateTime_Header.Value.AddMinutes(-StaticHelper.offset) :
                                                                                                  contractInformation.PickupTimeStamp.Value.converttoDateTime(),
                                                                                                  contractInformation.DropoffTimeStamp.Value.converttoDateTime());

            var expectedDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(contractInformation.pickupDateTime_Header.HasValue ?
                                                                                                   contractInformation.pickupDateTime_Header.Value.AddMinutes(-StaticHelper.offset) :
                                                                                                   contractInformation.PickupTimeStamp.Value.converttoDateTime(),
                                                                                                   droppoffDateTime);

            var totalContractKMLimit = decimal.Zero;
            var extraKM = contractHelper.calculateContractAdditionalProductKM(new Guid(contractInformation.contractId));
            if (currentDuration == expectedDuration)
            {
                totalContractKMLimit = contractInformation.kilometerLimit;
                extraKM = 0;
            }
            else
            {
                KilometerLimitBL kilometerLimitBL = new KilometerLimitBL(organizationService);
                var kilometer = kilometerLimitBL.getKilometerLimitForGivenDurationandGroupCode(expectedDuration, new Guid(contractInformation.pricingGroupCodeId));
                //kısaltma senaryosunda manual müdaheleyi es geç
                if (currentDuration > expectedDuration)
                {
                    totalContractKMLimit = kilometer;
                }
                else
                {
                    //manuel müdahale varsa , girilen km'nin alınması için
                    if (contractInformation.kilometerLimit > kilometer + extraKM)
                    {
                        totalContractKMLimit = contractInformation.kilometerLimit;

                    }
                    else
                    {
                        totalContractKMLimit = kilometer;
                    }
                }

            }

            SystemParameterBL systemParameterBL = new SystemParameterBL(organizationService);
            var kilometerLimitTolerance = systemParameterBL.getKilometerLimitTolerance();

            totalContractKMLimit += extraKM;
            //calculate km prices
            if (totalKm > totalContractKMLimit + kilometerLimitTolerance)
            {
                //calculate the extra km price   
                var calculatedKm = totalKm - totalContractKMLimit - kilometerLimitTolerance;
                var calculatedKmPrice = calculatedKm * contractInformation.overKilometerPrice;
                ConfigurationRepository configurationRepository = new ConfigurationRepository(organizationService);
                var overKilometerCode = configurationRepository.GetConfigurationByKey("additionalProduct_overKilometerLimitCode");
                var additionalProductCacheKey = configurationRepository.GetConfigurationByKey("additionalProduct_cacheKey");
                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(organizationService, StaticHelper.prepareAdditionalProductCacheKey(overKilometerCode, additionalProductCacheKey));
                var data = additionalProductCacheClient.getAdditionalProductCache(overKilometerCode);
                data.actualTotalAmount = calculatedKmPrice;
                data.actualAmount = calculatedKmPrice;
                data.tobePaidAmount = calculatedKmPrice;
                otherAdditonalProductDataTablets.Add(new AdditionalProductMapper().createTabletModel(data));
            }
        }

        private void calculateEquipmentTransaction(IOrganizationService organizationService,
                                 CalculateContractRemainingAmountParameters calculateContractRemainingAmountParameters,
                                 ContractItemDataMongoDB contractInformation)
        {
            //calcualte fuel difference
            //need to learn the thank capacity              
            var fuelDiff = calculateContractRemainingAmountParameters.equipmentInformation.firstFuelValue -
                calculateContractRemainingAmountParameters.equipmentInformation.currentFuelValue;
            //todo will read from parameter
            FuelPriceBL fuelPriceBL = new FuelPriceBL(organizationService);
            var price = fuelPriceBL.calculateFuelPriceByEquipmentId(calculateContractRemainingAmountParameters.equipmentInformation.equipmentId,
                                                                    fuelDiff);

            if (price.HasValue && price.Value > 0)
            {
                ConfigurationRepository configurationRepository = new ConfigurationRepository(organizationService);
                var missingFuelCode = configurationRepository.GetConfigurationByKey("additionalProduct_missingFuelCode");
                var fuelFillingServiceCode = configurationRepository.GetConfigurationByKey("additionalProduct_fuelFillingService");
                var additionalProductCacheKey = configurationRepository.GetConfigurationByKey("additionalProduct_cacheKey");

                AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(organizationService, StaticHelper.prepareAdditionalProductCacheKey(missingFuelCode, additionalProductCacheKey));
                var data = additionalProductCacheClient.getAdditionalProductCache(missingFuelCode);

                data.actualTotalAmount = price.Value;
                data.actualAmount = price.Value;
                data.tobePaidAmount = price.Value;
                otherAdditonalProductDataTablets.Add(new AdditionalProductMapper().createTabletModel(data));

                AdditionalProductCacheClient additionalProductCacheClientForFuelFillingService = new AdditionalProductCacheClient(organizationService, StaticHelper.prepareAdditionalProductCacheKey(fuelFillingServiceCode, additionalProductCacheKey));
                var fuelFillingService = additionalProductCacheClientForFuelFillingService.getAdditionalProductCacheWithFixedPrice(fuelFillingServiceCode);
                if (fuelFillingService.tobePaidAmount > 0)
                    otherAdditonalProductDataTablets.Add(new AdditionalProductMapper().createTabletModel(fuelFillingService));
            }
        }

        private void calculateEquipmentInventory(IOrganizationService organizationService,
                                                 CalculateContractRemainingAmountParameters calculateContractRemainingAmountParameters,
                                                 ContractItemDataMongoDB contractInformation)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(organizationService);
            var otherAdditonalProductCode = configurationRepository.GetConfigurationByKey("additionalProduct_otherkey");
            var additionalProductCacheKey = configurationRepository.GetConfigurationByKey("additionalProduct_cacheKey");

            AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(organizationService, StaticHelper.prepareAdditionalProductCacheKey(otherAdditonalProductCode, additionalProductCacheKey));
            var data = additionalProductCacheClient.getAdditionalProductCache(otherAdditonalProductCode);

            EquipmentInventoryBL equipmentInventoryBL = new EquipmentInventoryBL(organizationService);
            var equipmentData = equipmentInventoryBL.getLatestEquipmentInventoryInformation(new Guid(contractInformation.equipmentId),
                                                                                            calculateContractRemainingAmountParameters.langId);

            var missingInventory = equipmentInventoryBL.calculateMissingInventoryPriceByGroupCodeInformation(new Guid(contractInformation.groupCodeInformationsId),
                                                                                      equipmentData,
                                                                                      calculateContractRemainingAmountParameters.equipmentInformation.equipmentInventoryData);
            foreach (var item in missingInventory)
            {
                //todo text field will be added in contract item
                data.actualAmount = item.price;
                data.actualTotalAmount = item.price;
                data.tobePaidAmount = item.price;
                data.productName = item.inventoryName;
                otherAdditonalProductDataTablets.Add(new AdditionalProductMapper().createTabletModel(data));
            }
        }

        private void calculateAdditionalProducts(IOrganizationService organizationService,
                                                 ContractItemDataMongoDB contractInformation,
                                                 DateTime _dropoffDateTime,
                                                 CalculateContractRemainingAmountParameters calculateContractRemainingAmountParameters)
        {
            DurationHelper durationHelper = new DurationHelper(this.mongoDBHostName, this.mongoDBDatabaseName);
            var totalDuration = durationHelper.calculateDocumentDurationByPriceHourEffect(contractInformation.PickupTimeStamp.Value.converttoDateTime(), _dropoffDateTime);

            AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(organizationService);
            var additionalProductsResponse = additionalProductsBL.GetAdditionalProductForUpdateContract(Convert.ToString(calculateContractRemainingAmountParameters.contractInformation.contractId),
                                                                                                        new ContractDateandBranchParameters
                                                                                                        {
                                                                                                            dropoffDate = _dropoffDateTime,
                                                                                                            pickupDate = contractInformation.pickupDateTimeStamp_Header.Value.converttoDateTime(),
                                                                                                            pickupBranchId = new Guid(contractInformation.pickupBranchId),
                                                                                                            dropoffBranchId = calculateContractRemainingAmountParameters.dropoffBranch.branchId.Value,
                                                                                                        },
                                                                                                        totalDuration,
                                                                                                        calculateContractRemainingAmountParameters.langId);
            //remove one way fee from additional products because it i already added for other additional products
            ConfigurationRepository configurationRepository = new ConfigurationRepository(organizationService);
            var oneWayFeeCode = configurationRepository.GetConfigurationByKey("OneWayFeeCode");
            if (additionalProductsResponse.AdditionalProducts != null)
            {
                var oneWayFee = additionalProductsResponse.AdditionalProducts.Where(p => p.productCode == oneWayFeeCode).FirstOrDefault();
                if (oneWayFee != null)
                {
                    additionalProductsResponse.totaltobePaidAmount = additionalProductsResponse.totaltobePaidAmount - oneWayFee.tobePaidAmount;
                    additionalProductsResponse.AdditionalProducts = additionalProductsResponse.AdditionalProducts.Where(p => p.productCode != oneWayFeeCode).ToList();
                }
            }


            additonalProductResponse = new CalculateAdditionalProductResponse
            {
                additionalProducts = additionalProductsResponse.AdditionalProducts?.Where(p => p.value > 0)
                                    .ToList()
                                    .ConvertAll(p => new AdditionalProductMapper().createTabletModel(p)).ToList(),
                totaltobePaidAmount = additionalProductsResponse.totaltobePaidAmount

            };
        }
    }

}

