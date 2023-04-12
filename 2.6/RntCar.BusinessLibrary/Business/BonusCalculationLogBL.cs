using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class BonusCalculationLogBL : BusinessHandler
    {
        private int? currentUserBusinessRoleCode { get; set; }
        private bool isItemCreatedByServiceUser { get; set; } = false;
        private Guid currentUserBranchId { get; set; }
        private Guid contractCreatedBy { get; set; }
        private Guid? contractCreatedByWebServiceUser { get; set; }
        private Entity contractItemAdditionalProduct { get; set; }
        private decimal calculatedBaseBonusAmount { get; set; }
        private List<Entity> additionalProductBonusRates { get; set; }
        public BonusCalculationLogBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public BonusCalculationLogBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public BonusCalculationLogBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public BonusCalculationLogBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        private bool checkIsBonusCalculateAndGetAdditionalProductData(Entity contractItem)
        {
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);

            var itemTypeCode = contractItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;
            // no need to calculate bonus for equipment
            if (itemTypeCode != (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Equipment)
            {
                var additionalProductId = contractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id;
                this.contractItemAdditionalProduct = additionalProductRepository.getAdditionalProductById(additionalProductId, new string[] { "rnt_bonuscheckcode" });
                return this.contractItemAdditionalProduct.GetAttributeValue<bool>("rnt_bonuscheckcode");
            }
            return false;
        }
        private void checkContractItemRelationWithReservationItem(Entity contractItem)
        {
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);
            var webServiceUserId = configurationRepository.GetConfigurationByKey("WebServiceUserId");

            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);

            var reservationItemId = contractItem.Attributes.Contains("rnt_reservationitemid") ?
                                        contractItem.GetAttributeValue<EntityReference>("rnt_reservationitemid").Id
                                        : (Guid?)null;
            if (reservationItemId.HasValue)
            {
                var resevationItem = reservationItemRepository.getReservationItemByIdByGivenColumns(reservationItemId.Value, new string[] { "createdby", "rnt_channelcode" });
                var itemChannelCode = resevationItem.GetAttributeValue<OptionSetValue>("rnt_channelcode").Value;
                var reservationCreatedBy = resevationItem.GetAttributeValue<EntityReference>("createdby");
                // if reservation item does not created by web service user set reservation item created by to contract item
                if (itemChannelCode == (int)ClassLibrary._Enums_1033.rnt_ReservationChannel.Branch ||
                        itemChannelCode == (int)ClassLibrary._Enums_1033.rnt_ReservationChannel.CallCenter)
                    this.contractCreatedBy = reservationCreatedBy.Id;
            }
        }
        private void getUserInfoByContractItemCreatedBy()
        {
            if (isItemCreatedByServiceUser)
            {
                ServiceUserRepository serviceUserRepository = new ServiceUserRepository(this.OrgService);
                var serviceUser = serviceUserRepository.getServiceUserById(this.contractCreatedByWebServiceUser.Value);
                this.currentUserBusinessRoleCode = serviceUser.GetAttributeValue<OptionSetValue>("rnt_businessrolecode").Value;
                this.currentUserBranchId = serviceUser.GetAttributeValue<EntityReference>("rnt_branchid").Id;
            }
            else
            {
                SystemUserRepository systemUserRepository = new SystemUserRepository(this.OrgService);
                var systemUser = systemUserRepository.getSystemUserById(this.contractCreatedBy);
                this.currentUserBusinessRoleCode = systemUser.GetAttributeValue<OptionSetValue>("rnt_businessrolecode").Value;
                var currentUserBusinessUnitId = systemUser.GetAttributeValue<EntityReference>("businessunitid").Id;
                BranchRepository branchRepository = new BranchRepository(this.OrgService);
                this.currentUserBranchId = branchRepository.getBranchByBusinessUnitId(currentUserBusinessUnitId).FirstOrDefault().Id;
            }
        }
        private void getContractItemCreationInfo(Entity contractItem)
        {
            var itemChannelCode = contractItem.GetAttributeValue<OptionSetValue>("rnt_channelcode").Value;
            if (itemChannelCode != (int)ClassLibrary._Enums_1033.rnt_ReservationChannel.Branch &&
                itemChannelCode != (int)ClassLibrary._Enums_1033.rnt_ReservationChannel.CallCenter)
                this.isItemCreatedByServiceUser = true;

            this.contractCreatedByWebServiceUser = contractItem.Attributes.Contains("rnt_externalusercreatedbyid") ?
                            contractItem.GetAttributeValue<EntityReference>("rnt_externalusercreatedbyid").Id
                            : (Guid?)null;
            this.contractCreatedBy = contractItem.GetAttributeValue<EntityReference>("createdby").Id;
        }
        private Entity getBonusCalculationByContractPickupDate(Guid contractId)
        {
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var contract = contractRepository.getContractById(contractId, new string[] { "rnt_pickupdatetime" });

            BonusCalculationRepository bonusCalculationRepository = new BonusCalculationRepository(this.OrgService);
            return bonusCalculationRepository.getBonusCalculationByDate(contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"));
        }
        private void calculateBaseBonusAmount(Entity bonusCalculation, decimal itemNetAmount)
        {
            var bonusCalculationRate = bonusCalculation.GetAttributeValue<decimal>("rnt_bonusratio");

            // calculate base bonus amount by bonus calculation rate
            this.calculatedBaseBonusAmount = itemNetAmount * bonusCalculationRate / 100;
        }
        private void calculateBonusandCreateForCurrentUser(Guid contractId, Guid contractItemId, decimal itemNetAmount)
        {
            // first create bonus record for current user
            var currentUserBonusRateRecord = additionalProductBonusRates.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_businessrolecode").Value == this.currentUserBusinessRoleCode).FirstOrDefault();
            var currentUserBonusRate = currentUserBonusRateRecord.GetAttributeValue<decimal>("rnt_bonusratio");
            var latestBonusAmount = calculatedBaseBonusAmount * currentUserBonusRate / 100;

            var parameters = new CreateBonusCalculationLogParameters
            {
                netAmount = itemNetAmount,
                contractId = contractId,
                contractItemId = contractItemId,
                userId = this.isItemCreatedByServiceUser ? (Guid?)null : this.contractCreatedBy,
                externalUserId = this.isItemCreatedByServiceUser ? this.contractCreatedByWebServiceUser.Value : (Guid?)null,
                bonusAmount = latestBonusAmount,
                businessRoleCode = this.currentUserBusinessRoleCode
            };

            this.createCalculateBonus(parameters);
            // remove current user from list after create
            additionalProductBonusRates.Remove(currentUserBonusRateRecord);
        }
        private void calculateBonusandCreateForOtherUsers(Guid contractId, Guid contractItemId, decimal itemNetAmount)
        {
            SystemUserRepository systemUserRepository = new SystemUserRepository(this.OrgService);

            foreach (var additionalProductBonusRateItem in this.additionalProductBonusRates)
            {
                var additionalProductBusinessRoleCode = additionalProductBonusRateItem.GetAttributeValue<OptionSetValue>("rnt_businessrolecode").Value;
                // if additional products bonus rate grather then current user bonus rate calculate bonus for role code users 
                if (additionalProductBusinessRoleCode > this.currentUserBusinessRoleCode)
                {
                    // get system user by additional product role code and calculate bonus
                    var systemUsersByBusinessRoleCode = systemUserRepository.getSystemUsersByBusinessRoleCodeandBranchWithGivenColumns(additionalProductBusinessRoleCode,
                                                                                                                                       Convert.ToString(this.currentUserBranchId),
                                                                                                                                       new string[] { "rnt_businessrolecode" });
                    var additionalProductBonusRate = additionalProductBonusRateItem.GetAttributeValue<decimal>("rnt_bonusratio");
                    // calculate bonus by all users count
                    var latestBonusAmount = (this.calculatedBaseBonusAmount * additionalProductBonusRate / 100) / systemUsersByBusinessRoleCode.Count;
                    // create bonus record for each user
                    foreach (var systemUserItem in systemUsersByBusinessRoleCode)
                    {
                        var parameters = new CreateBonusCalculationLogParameters
                        {
                            netAmount = itemNetAmount,
                            contractId = contractId,
                            contractItemId = contractItemId,
                            userId = systemUserItem.Id,
                            bonusAmount = latestBonusAmount,
                            businessRoleCode = additionalProductBusinessRoleCode
                        };

                        this.createCalculateBonus(parameters);
                    }
                }
            }
        }
        public void calculateBonusByUser(EntityReference contractReference)
        {

            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            var contractItems = contractItemRepository.getCompletedContractItemsByContractIdWithGivenColmuns(contractReference.Id, new string[] { "rnt_reservationitemid",
                                                                                                                                                  "rnt_externalusercreatedbyid",
                                                                                                                                                  "rnt_externalusermodifiedbyid",
                                                                                                                                                  "createdby",
                                                                                                                                                  "rnt_additionalproductid",
                                                                                                                                                  "rnt_itemtypecode",
                                                                                                                                                  "rnt_netamount",
                                                                                                                                                  "rnt_channelcode"});

            foreach (var item in contractItems)
            {
                if (this.checkIsBonusCalculateAndGetAdditionalProductData(item))
                {
                    // if there is record for contract item deactivate all 
                    // after deactivation recalculate bonus
                    // deactivation for re-triggering of the workflow
                    this.deactiveBonusCalculationLogsByContractItemId(item.Id);

                    // check contract item created by service user and get creation info
                    this.getContractItemCreationInfo(item);

                    // check contrat item relation with reservation item
                    // if it has relation with reservation item check reservation item created by 
                    // if created by is not service user set contract created by as reservation created by
                    this.checkContractItemRelationWithReservationItem(item);

                    // get contract item business role information by created by
                    this.getUserInfoByContractItemCreatedBy();

                    // bonus will be calculate from item net amount
                    var itemNetAmount = item.GetAttributeValue<Money>("rnt_netamount").Value;

                    var bonusCalculation = this.getBonusCalculationByContractPickupDate(contractReference.Id);

                    if (bonusCalculation != null)
                    {
                        // calculate base bonus amount from item net amount
                        this.calculateBaseBonusAmount(bonusCalculation, itemNetAmount);

                        // get additional products bonus rates
                        BonusAdditionalProductRateRepository bonusAdditionalProductRateRepository = new BonusAdditionalProductRateRepository(this.OrgService);
                        this.additionalProductBonusRates = bonusAdditionalProductRateRepository.getBonusAdditionalProductRateByBonusCalcuationandAdditionalProduct(bonusCalculation.Id, this.contractItemAdditionalProduct.Id);

                        if (additionalProductBonusRates.Count > 0)
                        {
                            // calculate bonus for current user
                            // check is service user before creation
                            this.calculateBonusandCreateForCurrentUser(contractReference.Id, item.Id, itemNetAmount);
                            // create bonus record for other users
                            this.calculateBonusandCreateForOtherUsers(contractReference.Id, item.Id, itemNetAmount);
                        }
                    }
                }
            }
        }
        public Guid createCalculateBonus(CreateBonusCalculationLogParameters parameters)
        {
            Entity entity = new Entity("rnt_bonuscalculationlog");
            entity["rnt_bonusamount"] = new Money(parameters.bonusAmount);
            entity["rnt_businessrolecode"] = new OptionSetValue(parameters.businessRoleCode.Value);
            entity["rnt_contractid"] = new EntityReference("rnt_contract", parameters.contractId);
            entity["rnt_contractitemid"] = new EntityReference("rnt_contractitem", parameters.contractItemId);
            entity["rnt_netamount"] = new Money(parameters.netAmount);
            if (parameters.externalUserId.HasValue)
                entity["rnt_serviceuserid"] = new EntityReference("rnt_serviceuser", parameters.externalUserId.Value);
            if (parameters.userId.HasValue)
                entity["rnt_userid"] = new EntityReference("systemuser", parameters.userId.Value);

            return this.createEntity(entity);
        }
        public void deactiveBonusCalculationLogsByContractItemId(Guid contractItemId)
        {
            BonusCalculationLogRepository bonusCalculationLogRepository = new BonusCalculationLogRepository(this.OrgService);
            var bonusCalculationLogs = bonusCalculationLogRepository.getBonusCalculationLogsByContractItem(contractItemId);
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);
            foreach (var item in bonusCalculationLogs)
            {
                xrmHelper.setState("rnt_bonuscalculationlog", item.Id, (int)GlobalEnums.StateCode.Passive, (int)ClassLibrary._Enums_1033.rnt_bonuscalculationlog_StatusCode.Inactive);
            }
        }
    }
}
