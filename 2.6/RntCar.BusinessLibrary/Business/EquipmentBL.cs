using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using MongoDB.Bson;
using Newtonsoft.Json;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.Report;
using RntCar.MongoDBHelper.Helper;
using RntCar.MongoDBHelper.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class EquipmentBL : BusinessHandler
    {
        public EquipmentBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public EquipmentBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        public EquipmentBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }
        public MongoDBResponse sendEquipmenttoMongoDB(string messageName, Entity equipment)
        {
            var res = new MongoDBResponse();
            if (messageName.ToLower() == GlobalEnums.MessageNames.Create.ToString().ToLower())
            {
                res = this.CreateEquipmentInMongoDB(equipment);

                this.Trace("mongodb create response :  " + JsonConvert.SerializeObject(res));
                if (res.Result)
                {
                    this.updateMongoDBCreateRelatedFields(equipment, res.Id);
                }
            }
            else if (messageName.ToLower() == GlobalEnums.MessageNames.Update.ToString().ToLower())
            {
                res = this.UpdateEquipmentInMongoDB(equipment);
                this.Trace("mongodb update response :  " + JsonConvert.SerializeObject(res));
                if (res.Result)
                {
                    this.UpdateMongoDBUpdateRelatedFields(equipment);
                }
            }

            return res;
        }
        public MongoDBResponse ExecuteEquipmentActionInMongoDB(Entity entity, string messageName)
        {
            try
            {

                OrganizationRequest request = new OrganizationRequest("rnt_CreateEquipmentInMongoDB");
                request["MessageName"] = messageName;
                request["EquipmentEntity"] = entity;

                this.TracingService.Trace("calling action");

                var response = this.OrgService.Execute(request);

                var result = Convert.ToString(response.Results["ExecutionResult"]);
                var mongodbId = Convert.ToString(response.Results["ID"]);

                if (!string.IsNullOrEmpty(result))
                {
                    return MongoDBResponse.ReturnError(result);
                }

                return MongoDBResponse.ReturnSuccessWithId(mongodbId);

            }
            catch (Exception ex)
            {
                return MongoDBResponse.ReturnError(ex.Message);
            }
        }

        public MongoDBResponse CreateEquipmentInMongoDB(Entity entity)
        {
            Entity equipment = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "CreateEquipmentInMongoDB", Method.POST);

            var equipmentData = this.BuildMongoDBEquipmentData(equipment);
            restSharpHelper.AddJsonParameter<EquipmentData>(equipmentData);

            var responseEquipment = restSharpHelper.Execute<MongoDBResponse>();

            if (!responseEquipment.Result)
            {
                return MongoDBResponse.ReturnError(responseEquipment.ExceptionDetail);
            }

            return responseEquipment;
        }

        public MongoDBResponse UpdateEquipmentInMongoDB(Entity entity)
        {
            Entity equipment = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("MongoDBWepAPIURL");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "UpdateEquipmentInMongoDB", Method.POST);

            var equipmentData = this.BuildMongoDBEquipmentData(equipment);
            this.Trace("statuscode before sending webapi" + equipmentData.StatusCode);
            this.Trace("rnt_mongodbid before sending webapi" + equipment.GetAttributeValue<string>("rnt_mongodbid"));

            restSharpHelper.AddJsonParameter<EquipmentData>(equipmentData);
            restSharpHelper.AddQueryParameter("id", equipment.GetAttributeValue<string>("rnt_mongodbid"));

            var responseEquipment = restSharpHelper.Execute<MongoDBResponse>();
            if (!responseEquipment.Result)
            {
                return MongoDBResponse.ReturnError(responseEquipment.ExceptionDetail);
            }
            return responseEquipment;
        }

        public void updateEquipmentStatus(Guid equipmentId, int statusCode)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            xrmHelper.setState("rnt_equipment", equipmentId, (int)GlobalEnums.StateCode.Active, statusCode);
            this.Trace("Update Equipment For Transfer StatusCode " + statusCode);
        }

        public void updateEquipmentTransferType(Guid equipmentId, int transferType)
        {
            Entity e = new Entity("rnt_equipment");
            e.Id = equipmentId;
            if (transferType == -1)//if transfer is completed
            {
                this.Trace("transfer completed");
                e["rnt_transfertype"] = null;
            }
            else
            {
                e["rnt_transfertype"] = new OptionSetValue(transferType);
            }

            this.OrgService.Update(e);
        }

        public void updateEquipmentforDelivery(Guid equipmentId,
                                               Guid equipmentInventoryId,
                                               Guid equipmentTransactionId,
                                               int currentKM,
                                               int currentFuel)
        {
            Entity e = new Entity("rnt_equipment");
            e.Id = equipmentId;
            e["statuscode"] = new OptionSetValue((int)EquipmentEnums.StatusCode.Rental);
            e["rnt_currentkm"] = currentKM;
            e["rnt_kmlastupdate"] = DateTime.Now.AddMinutes(StaticHelper.offset);
            e["rnt_fuelcode"] = new OptionSetValue(currentFuel);
            e["rnt_fuellastupdate"] = DateTime.Now.AddMinutes(StaticHelper.offset);
            e["rnt_equipmentinventoryid"] = new EntityReference("rnt_equipmentinventoryhistory", equipmentInventoryId);
            e["rnt_equipmenttransactionid"] = new EntityReference("rnt_equipmenttransactionhistory", equipmentTransactionId);

            this.OrgService.Update(e);
        }
        public void updateEquipmentforRental(Guid equipmentId,
                                             Guid equipmentInventoryId,
                                             Guid equipmentTransactionId,
                                             Guid branchId,
                                             int currentKM,
                                             int currentFuel)
        {
            Entity e = new Entity("rnt_equipment");
            e.Id = equipmentId;
            e["statuscode"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.Available);
            e["rnt_currentkm"] = currentKM;
            e["rnt_kmlastupdate"] = DateTime.Now.AddMinutes(StaticHelper.offset);
            e["rnt_fuelcode"] = new OptionSetValue(currentFuel);
            e["rnt_fuellastupdate"] = DateTime.Now.AddMinutes(StaticHelper.offset);
            e["rnt_equipmentinventoryid"] = new EntityReference("rnt_equipmentinventoryhistory", equipmentInventoryId);
            e["rnt_equipmenttransactionid"] = new EntityReference("rnt_equipmenttransactionhistory", equipmentTransactionId);
            e["rnt_currentbranchid"] = new EntityReference("rnt_branch", branchId);

            this.OrgService.Update(e);
        }
        public void updateEquipmentInformation(Guid equipmentId,
                                               Guid? equipmentInventoryId,
                                               Guid? equipmentTransactionId,
                                               int currentKM,
                                               int currentFuel,
                                               int statusCode)
        {
            Entity e = new Entity("rnt_equipment");
            e.Id = equipmentId;
            e["statuscode"] = new OptionSetValue(statusCode);
            e["rnt_currentkm"] = currentKM;
            e["rnt_kmlastupdate"] = DateTime.Now.AddMinutes(StaticHelper.offset);
            e["rnt_fuelcode"] = new OptionSetValue(currentFuel);
            e["rnt_fuellastupdate"] = DateTime.Now.AddMinutes(StaticHelper.offset);
            if (equipmentInventoryId.HasValue)
                e["rnt_equipmentinventoryid"] = new EntityReference("rnt_equipmentinventoryhistory", equipmentInventoryId.Value);
            if (equipmentTransactionId.HasValue)
                e["rnt_equipmenttransactionid"] = new EntityReference("rnt_equipmenttransactionhistory", equipmentTransactionId.Value);

            this.OrgService.Update(e);
        }

        public bool updateEquipmentForVehicleInspection(Entity equipment, Entity PreEquipment)
        {
            bool isUpdate = false;
            DateTime nullableCheck = new DateTime();
            if (equipment.Contains("statuscode"))
            {
                var equipmentStatus = equipment.GetAttributeValue<OptionSetValue>("statuscode").Value;
                this.Trace("equipmentStatus: " + equipmentStatus);

                if (equipmentStatus == (int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.Available)
                {
                    this.Trace("equipmentStatus is available.");

                    SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
                    var maintenanceParameters = systemParameterBL.getMaintenanceRelatedSystemParameters();

                    var maintenanceInformDay = maintenanceParameters.maintenanceInformDay;
                    this.Trace("maintenanceInformDay: " + maintenanceInformDay);

                    DateTime today = DateTime.UtcNow.Date;

                    var preNextMaintenanceDay = PreEquipment.GetAttributeValue<DateTime>("rnt_inspectionexpiredate");
                    if (preNextMaintenanceDay != nullableCheck)
                    {
                        var checkDayDifference = Math.Abs((preNextMaintenanceDay - today).TotalDays);
                        isUpdate = maintenanceInformDay >= checkDayDifference;

                    }

                }
            }
            if (equipment.Contains("rnt_inspectionexpiredate"))
            {
                var preInspectionExpire = PreEquipment.GetAttributeValue<DateTime>("rnt_inspectionexpiredate");
                if (preInspectionExpire != nullableCheck)
                {
                    equipment["rnt_lastvehicleinspection"] = preInspectionExpire;
                }
            }
            return isUpdate;

        }

        public void updateEquipmentforMaintenance(Entity equipment)
        {
            var equipmentStatus = equipment.GetAttributeValue<OptionSetValue>("statuscode").Value;

            if (equipmentStatus == (int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.Rental)
            {
                return;
            }

            ProductRepository productRepository = new ProductRepository(this.OrgService);
            var product = productRepository.getProductByEquipmentId(equipment.Id, new string[] { "rnt_maintenanceperiod" });
            var currentKm = equipment.GetAttributeValue<int>("rnt_currentkm");

            if (product.Contains("rnt_maintenanceperiod"))
            {
                var period = product.GetAttributeValue<OptionSetValue>("rnt_maintenanceperiod").Value;
                this.Trace("period : " + period);
                this.Trace("currentKm : " + currentKm);
                SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
                var maintenanceParameters = systemParameterBL.getMaintenanceRelatedSystemParameters();

                TransferRepository transferRepository = new TransferRepository(this.OrgService);
                var transfers = transferRepository.getMaintenanceTransfersByEquipmentId(equipment.Id, new string[] { "rnt_maintenancekm" });

                var mod = currentKm % period;
                bool updateStatus = false;
                int lastMaintenanceKM = 0;

                if (transfers.Count != 0)
                {
                    lastMaintenanceKM = transfers.OrderByDescending(p => p.GetAttributeValue<int>("rnt_maintenancekm")).FirstOrDefault().GetAttributeValue<int>("rnt_maintenancekm");
                }
                this.Trace("lastMaintenanceKM : " + lastMaintenanceKM);
                this.Trace("mod : " + mod);
                this.Trace("maintenanceParameters.maintenanceInformKm : " + maintenanceParameters.maintenanceInformKm);
                this.Trace("maintenanceParameters.maintenanceLimitKm : " + maintenanceParameters.maintenanceLimitKm);

                if (period - mod <= maintenanceParameters.maintenanceInformKm)
                {
                    ExecuteWorkflowRequest request = new ExecuteWorkflowRequest()
                    {
                        EntityId = equipment.Id,
                        WorkflowId = StaticHelper.sendEquipmentMaintenanceMailWorkflowId
                    };
                    ExecuteWorkflowResponse response = (ExecuteWorkflowResponse)this.OrgService.Execute(request);
                }

                // Daha önce bakım yapıldıysa
                if (currentKm - mod == lastMaintenanceKM || currentKm + (period - mod) == lastMaintenanceKM)
                {
                    // Bakım km'si geldiyse
                    if (period - mod <= maintenanceParameters.maintenanceLimitKm)
                    {
                        updateStatus = true;
                    }
                    else
                    {
                        updateStatus = false;
                    }
                }
                else
                {
                    updateStatus = true;
                }

                if (updateStatus)
                {
                    Entity e = new Entity("rnt_equipment");
                    e.Id = equipment.Id;
                    e["statuscode"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_equipment_StatusCode.WaitingMaintenance);
                    this.OrgService.Update(e);
                }
            }

        }
        public EquipmentData BuildMongoDBEquipmentData(Entity equipment)
        {
            ProductRepository productRepository = new ProductRepository(this.OrgService);
            var product = productRepository.getProductByIdWithGivenColumns(equipment.GetAttributeValue<EntityReference>("rnt_product").Id,
                                                                                                                        new string[] { "rnt_groupcodeid",
                                                                                                                                        "rnt_numberofdoorcode",
                                                                                                                                        "rnt_numberofluggagecode",
                                                                                                                                        "rnt_numberofseatcode",
                                                                                                                                        "rnt_brandid",
                                                                                                                                        "rnt_modelid"});

            GroupCodeInformationRepository groupCodeInformationRepository = new GroupCodeInformationRepository(this.OrgService);
            var groupCodeInformaton = groupCodeInformationRepository.getGroupCodeInformationById(product.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id);

            EquipmentData equipmentData = new EquipmentData
            {
                EquipmentId = Convert.ToString(equipment.Id),
                Name = equipment.GetAttributeValue<string>("rnt_name"),
                ChassisNumber = equipment.GetAttributeValue<string>("rnt_chassisnumber"),
                PlateNumber = equipment.GetAttributeValue<string>("rnt_platenumber"),
                LicenseNumber = equipment.GetAttributeValue<string>("rnt_licensenumber"),
                HGSNumber = equipment.GetAttributeValue<string>("rnt_hgsnumber"),
                CurrentKM = equipment.GetAttributeValue<int>("rnt_currentkm"),
                ProductId = Convert.ToString(equipment.GetAttributeValue<EntityReference>("rnt_product").Id),
                OwnerBranchId = Convert.ToString(equipment.GetAttributeValue<EntityReference>("rnt_ownerbranchid").Id),
                CurrentBranchId = equipment.Attributes.Contains("rnt_currentbranchid") ?
                                    Convert.ToString(equipment.GetAttributeValue<EntityReference>("rnt_currentbranchid").Id) :
                                    null,
                ProductName = equipment.GetAttributeValue<EntityReference>("rnt_product").Name,
                OwnerBranchName = equipment.GetAttributeValue<EntityReference>("rnt_ownerbranchid").Name,
                CurrentBranchName = equipment.Attributes.Contains("rnt_currentbranchid") ?
                                    equipment.GetAttributeValue<EntityReference>("rnt_currentbranchid").Name :
                                    null,
                CreatedBy = Convert.ToString(equipment.GetAttributeValue<EntityReference>("createdby").Id),
                ModifiedBy = Convert.ToString(equipment.GetAttributeValue<EntityReference>("modifiedby").Id),
                CreatedOn = equipment.GetAttributeValue<DateTime>("createdon"),
                ModifiedOn = equipment.GetAttributeValue<DateTime>("modifiedon"),
                GroupCodeInformationId = Convert.ToString(product.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id),
                GroupCodeInformationName = product.GetAttributeValue<EntityReference>("rnt_groupcodeid").Name,
                GroupCodeSegmentValue = groupCodeInformaton.GetAttributeValue<OptionSetValue>("rnt_segment").Value,
                MinimumAge = groupCodeInformaton.GetAttributeValue<int>("rnt_minimumage"),
                MinimumDriverLcense = groupCodeInformaton.GetAttributeValue<int>("rnt_minimumdriverlicence"),
                YoungDriverAge = groupCodeInformaton.GetAttributeValue<int>("rnt_youngdriverage"),
                YoungDriverLicense = groupCodeInformaton.GetAttributeValue<int>("rnt_youngdriverlicence"),
                Deposit = groupCodeInformaton.GetAttributeValue<decimal>("rnt_deposit"),
                CarImage = groupCodeInformaton.GetAttributeValue<string>("rnt_image"),
                StatusCode = equipment.GetAttributeValue<OptionSetValue>("statuscode").Value,
                StateCode = equipment.GetAttributeValue<OptionSetValue>("statecode").Value,
                fuel = equipment.Attributes.Contains("rnt_fuelcode") ? equipment.GetAttributeValue<OptionSetValue>("rnt_fuelcode").Value : 0,
                nofDoor = product.Attributes.Contains("rnt_numberofdoorcode") ? product.GetAttributeValue<OptionSetValue>("rnt_numberofdoorcode").Value : 0,
                nofLuggage = product.Attributes.Contains("rnt_numberofluggagecode") ? product.GetAttributeValue<OptionSetValue>("rnt_numberofluggagecode").Value : 0,
                nofSeat = product.Attributes.Contains("rnt_numberofseatcode") ? product.GetAttributeValue<OptionSetValue>("rnt_numberofseatcode").Value : 0,
                brandId = product.Attributes.Contains("rnt_brandid") ?
                          product.GetAttributeValue<EntityReference>("rnt_brandid").Id.ToString() :
                          null,
                brandName = product.Attributes.Contains("rnt_brandid") ?
                          product.GetAttributeValue<EntityReference>("rnt_brandid").Name :
                          null,
                modelId = product.Attributes.Contains("rnt_modelid") ?
                          product.GetAttributeValue<EntityReference>("rnt_modelid").Id.ToString() :
                          null,
                modelName = product.Attributes.Contains("rnt_modelid") ?
                          product.GetAttributeValue<EntityReference>("rnt_modelid").Name :
                          null,
                fuelType = groupCodeInformaton.Attributes.Contains("rnt_fueltypecode") ? groupCodeInformaton.GetAttributeValue<OptionSetValue>("rnt_fueltypecode").Value : 0,
                tranmissionType = groupCodeInformaton.Attributes.Contains("rnt_gearboxcode") ? groupCodeInformaton.GetAttributeValue<OptionSetValue>("rnt_gearboxcode").Value : 0,

            };
            return equipmentData;
        }

        public EntityCollection getEquipmentWithHGSLabel()
        {
            EntityCollection hgsLabelList = new EntityCollection();
            int queryCount = 5000;
            // Initialize the page number.
            int pageNumber = 1;

            string pagingCookie = null;
            while (true)
            {
                QueryExpression getHGSLabelQuery = new QueryExpression("rnt_equipment");
                getHGSLabelQuery.ColumnSet = new ColumnSet("rnt_hgslabelid", "rnt_equipmentid", "rnt_platenumber");
                getHGSLabelQuery.Criteria.AddCondition("rnt_hgslabelid", ConditionOperator.NotNull);
                getHGSLabelQuery.PageInfo = new PagingInfo
                {
                    PageNumber = pageNumber,
                    Count = queryCount,
                    PagingCookie = pagingCookie
                };
                EntityCollection results = this.OrgService.RetrieveMultiple(getHGSLabelQuery);
                hgsLabelList.Entities.AddRange(results.Entities);

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
            return hgsLabelList;
        }

        public List<FleetReportData> getEquipmentDataForFleetReport(DateTime? startDate, int? hour)
        {
            ProductRepository productRepository = new ProductRepository(this.OrgService);
            EquipmentRepository equipmentRepository = new EquipmentRepository(this.OrgService);

            var publishDate = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour);

            if (startDate.HasValue)
            {
                publishDate = startDate.Value.Date.AddHours(hour.Value);
            }

            try
            {
                List<FleetReportData> fleetReportData = new List<FleetReportData>();

                var equipments = equipmentRepository.getAllActiveEquipmentsWithProducts();
                
                foreach (var item in equipments)
                {
                    try
                    {
                        fleetReportData.Add(new FleetReportData
                        {
                            currentBranch = item.GetAttributeValue<EntityReference>("rnt_currentbranchid") != null ? item.GetAttributeValue<EntityReference>("rnt_currentbranchid").Name : string.Empty,
                            groupCode = item.GetAttributeValue<AliasedValue>("products.rnt_groupcodeid") != null ? ((EntityReference)item.GetAttributeValue<AliasedValue>("products.rnt_groupcodeid").Value).Name : string.Empty,
                            name = item.GetAttributeValue<string>("rnt_name"),
                            brand = item.GetAttributeValue<AliasedValue>("products.rnt_brandid") != null ?((EntityReference)item.GetAttributeValue<AliasedValue>("products.rnt_brandid").Value).Name : string.Empty,
                            model = item.GetAttributeValue<AliasedValue>("products.rnt_modelid") != null ?((EntityReference)item.GetAttributeValue<AliasedValue>("products.rnt_modelid").Value).Name : string.Empty,
                            product = item.GetAttributeValue<AliasedValue>("products.rnt_name") != null ?((string)item.GetAttributeValue<AliasedValue>("products.rnt_name").Value) : string.Empty,
                            chassisNumber = item.GetAttributeValue<string>("rnt_chassisnumber"),
                            modelYearCode = item.GetAttributeValue<OptionSetValue>("rnt_modelyearcode") != null ? item.GetAttributeValue<OptionSetValue>("rnt_modelyearcode").Value : 0,
                            modelYearName = item.GetAttributeValue<OptionSetValue>("rnt_modelyearcode") != null ? item.FormattedValues["rnt_modelyearcode"] : string.Empty,
                            gearBoxCode = item.GetAttributeValue<AliasedValue>("products.rnt_gearbox") != null ? ((OptionSetValue)item.GetAttributeValue<AliasedValue>("products.rnt_gearbox").Value).Value : 0,
                            gearBoxName = item.GetAttributeValue<AliasedValue>("products.rnt_gearbox") != null ? item.FormattedValues["products.rnt_gearbox"] : string.Empty,
                            statusCode = item.GetAttributeValue<OptionSetValue>("statuscode").Value,
                            statusName = item.FormattedValues["statuscode"],
                            transferTypeCode = item.GetAttributeValue<OptionSetValue>("rnt_transfertype") != null ? item.GetAttributeValue<OptionSetValue>("rnt_transfertype").Value : 0,
                            transferTypeName = item.GetAttributeValue<OptionSetValue>("rnt_transfertype") != null ? item.FormattedValues["rnt_transfertype"] : string.Empty,
                            maintenancePeriodCode = item.GetAttributeValue<AliasedValue>("products.rnt_maintenanceperiod") != null ?((OptionSetValue)item.GetAttributeValue<AliasedValue>("products.rnt_maintenanceperiod").Value).Value : 0,
                            maintenancePeriodName = item.GetAttributeValue<AliasedValue>("products.rnt_maintenanceperiod") != null ? item.FormattedValues["products.rnt_maintenanceperiod"] : string.Empty,
                            currentKm = item.GetAttributeValue<int>("rnt_currentkm"),
                            fuelCode = item.GetAttributeValue<OptionSetValue>("rnt_fuelcode") != null ? item.GetAttributeValue<OptionSetValue>("rnt_fuelcode").Value : 0,
                            fuelName = item.GetAttributeValue<OptionSetValue>("rnt_fuelcode") != null ? item.FormattedValues["rnt_fuelcode"] : string.Empty,
                            licenseNumber = item.GetAttributeValue<string>("rnt_licensenumber"),
                            vehicleNo = item.GetAttributeValue<string>("rnt_vehicleno"),
                            equipmentColor = item.GetAttributeValue<EntityReference>("rnt_equipmentcolourid") != null ? item.GetAttributeValue<EntityReference>("rnt_equipmentcolourid").Name : string.Empty,
                            tireSize = item.GetAttributeValue<EntityReference>("rnt_tiresizeid") != null ? item.GetAttributeValue<EntityReference>("rnt_tiresizeid").Name : string.Empty,
                            tireTypeCode = item.GetAttributeValue<OptionSetValue>("rnt_tiretypecode") != null ? item.GetAttributeValue<OptionSetValue>("rnt_tiretypecode").Value : 0,
                            tireTypeName = item.GetAttributeValue<OptionSetValue>("rnt_tiretypecode") != null ? item.FormattedValues["rnt_tiretypecode"] : string.Empty,
                            firstRegistrationDate = item.GetAttributeValue<DateTime>("rnt_firstregistrationdate") != null ? item.GetAttributeValue<DateTime>("rnt_firstregistrationdate").Date : DateTime.MinValue,
                            inspectionExpireDate = item.GetAttributeValue<DateTime>("rnt_inspectionexpiredate") != null ? item.GetAttributeValue<DateTime>("rnt_inspectionexpiredate").Date : DateTime.MinValue,
                            licensePlace = item.GetAttributeValue<string>("rnt_licenseplace"),
                            oldPlate = item.GetAttributeValue<string>("rnt_oldplate"),
                            hgsLabel = item.GetAttributeValue<EntityReference>("rnt_hgslabelid") != null ? item.GetAttributeValue<EntityReference>("rnt_hgslabelid").Name : string.Empty,
                            hgsNumber = item.GetAttributeValue<string>("rnt_hgsnumber"),
                            mongoDbIntegrationTrigger = item.GetAttributeValue<string>("rnt_mongodbintegrationtrigger"),
                            cost = item.GetAttributeValue<Money>("rnt_cost") != null ? item.GetAttributeValue<Money>("rnt_cost").Value : 0,
                            publishDate = publishDate.Ticks,
                        });
                    }
                    catch (Exception ex)
                    {

                        //throw;
                    }
                   
                }

                return fleetReportData;

            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
