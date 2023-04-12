using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.BusinessLibrary
{
    public class BonusCalculationBL : BusinessHandler
    {
        public BonusCalculationBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public BonusCalculationBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public BonusCalculationBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public BonusCalculationBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        private string getBranchName(string branchId)
        {
            BranchRepository branchRepository = new BranchRepository(this.OrgService);
            return branchRepository.getBranchById(new Guid(branchId)).GetAttributeValue<string>("rnt_name");
        }

        private List<Entity> getDailyBonusCalculations(List<Entity> bonusCalculations, DateTime invoiceDate)
        {
            return bonusCalculations.Where(p => p.GetAttributeValue<DateTime>("rnt_begindate") <= invoiceDate && p.GetAttributeValue<DateTime>("rnt_enddate") >= invoiceDate).ToList();
        }

        private decimal calculateBonusAmountByProgressRatio(decimal baseBonusAmount, decimal progressRatio, decimal totalAdditionalProductAmount, decimal revenue)
        {
            if (progressRatio < 6)
            {
                return 0;
            }
            else if (6 <= progressRatio && progressRatio < 8)
            {
                return baseBonusAmount * 0.8m;
            }
            else if (8 <= progressRatio && progressRatio < 10)
            {
                return baseBonusAmount;
            }
            else
            {
                return baseBonusAmount + ((revenue - totalAdditionalProductAmount * 0.1m) / totalAdditionalProductAmount) * (baseBonusAmount) * 0.2m;
            }
        }

        public List<BranchRevenue> calculateBranchRevenues(List<PositionalBonusCalculationData> positionalBonusCalculations)
        {
            List<BranchRevenue> branchRevenues = new List<BranchRevenue>();

            var bonusCalculationsGroupedByBranch = positionalBonusCalculations.GroupBy(p => p.PickupBranchId).ToList();
            bonusCalculationsGroupedByBranch.ForEach(groupedBranch =>
            {
                decimal revenueAmount = 0;
                decimal totalAmount = 0;

                var bonusCalculationsGroupedByPnr = groupedBranch.GroupBy(p => p.ContractItemId).ToList();
                bonusCalculationsGroupedByPnr.ForEach(groupedPnr =>
                {
                    var groupedByContact = groupedPnr.GroupBy(p => p.ContractNumber).ToList();
                    groupedByContact.ForEach(i =>
                    {
                        var defaultItem = groupedPnr.FirstOrDefault();
                        totalAmount += defaultItem.Amount;
                        if (defaultItem.IsRevenue)
                            revenueAmount = revenueAmount + defaultItem.Amount;
                    });
                });

                branchRevenues.Add(new BranchRevenue
                {
                    branchId = groupedBranch.FirstOrDefault().PickupBranchId,
                    revenueAmount = revenueAmount,
                    totalAmount = totalAmount
                });
            });

            return branchRevenues;
        }

        public PositionalBonusCalculationResult calculatePositionalNetBonusAmount(List<Entity> contracts)
        {
            List<PositionalBonusCalculationData> result = new List<PositionalBonusCalculationData>();
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            BonusCalculationRepository bonusCalculationRepository = new BonusCalculationRepository(this.OrgService);
            BonusSalesRateRepository bonusSalesRateRepository = new BonusSalesRateRepository(this.OrgService);
            BranchRepository branchRepository = new BranchRepository(this.OrgService);
            ContractItemBL contractItemBL = new ContractItemBL(this.OrgService);

            var additionalProducts = additionalProductRepository.getAdditionalProducts();
            var completedContractItems = contractItemRepository.getAllCompletedContractItems(new string[] { "rnt_contractid", "rnt_totalamount", "rnt_itemtypecode", "rnt_additionalproductid", "rnt_contractitemid", "rnt_name", "rnt_invoicedate", "rnt_netamount" });
            var salesBonusCalculations = bonusCalculationRepository.getSalesBonusCalculations();
            var bonusSalesRates = bonusSalesRateRepository.getBonusSalesRates();
            var branches = branchRepository.getActiveBranchs();

            try
            {
                var priceDifferenceId = StaticHelper.GetConfiguration("PriceDifferenceId");

                contracts.ForEach(contract =>
                {
                    var contractItems = completedContractItems.Where(e => e.GetAttributeValue<EntityReference>("rnt_contractid").Id == contract.Id).ToList();
                    var bonusCalculation = salesBonusCalculations.Where(e => e.GetAttributeValue<DateTime>("rnt_begindate") <= contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime") && e.GetAttributeValue<DateTime>("rnt_enddate") >= contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime")).FirstOrDefault();
                    var pickupBranch = branches.Where(p => p.BranchId == contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id.ToString()).FirstOrDefault();
                    var dropoffBranch = branches.Where(p => p.BranchId == contract.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id.ToString()).FirstOrDefault();
                    var businessRoleOptionSetNames = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_BonusBusinessRole", 1055);
                    var contractItemAmounts = contractItemBL.calculateContractItemAmounts(contractItems);

                    contractItems.ForEach(contractItem =>
                    {
                        var isRevenue = true;
                        if (contractItem.Attributes.Contains("rnt_additionalproductid"))
                        {
                            var additionalProduct = additionalProducts.Where(p => p.Id == contractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id).FirstOrDefault();
                            if (additionalProduct == null || contractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id == new Guid(priceDifferenceId))
                            {
                                return;
                            }

                            isRevenue = additionalProduct.GetAttributeValue<bool>("rnt_revenue");
                        }

                        bonusSalesRates.ForEach(salesRate =>
                        {
                            var businessRoleType = salesRate.GetAttributeValue<OptionSetValue>("rnt_businessrolecode").Value;
                            var businessRoleName = businessRoleOptionSetNames.Where(optionSetItem => optionSetItem.value == businessRoleType).FirstOrDefault().label;

                            result.Add(new PositionalBonusCalculationData
                            {
                                QueryDate = DateTime.Now.Date,
                                BusinessRole = businessRoleName.insertSpaceBetweenWords(),
                                BaseBonusRatio = bonusCalculation != null ? bonusCalculation.GetAttributeValue<decimal>("rnt_bonusratio") : 0,
                                PositionalBonusRatio = salesRate.GetAttributeValue<decimal>("rnt_bonusratio"),
                                BonusCalculationId = bonusCalculation != null ? Convert.ToString(bonusCalculation.GetAttributeValue<Guid>("rnt_bonuscalculationid")) : Convert.ToString(Guid.Empty),
                                ContractNumber = contract.GetAttributeValue<string>("rnt_contractnumber"),
                                ContractItemId = Convert.ToString(contractItem.GetAttributeValue<Guid>("rnt_contractitemid")),
                                ContractItem = contractItem.GetAttributeValue<string>("rnt_name"),
                                PnrNumber = contract.GetAttributeValue<string>("rnt_pnrnumber"),
                                PickupBranch = pickupBranch.BranchName,
                                PickupBranchId = new Guid(pickupBranch.BranchId),
                                DropoffBranch = dropoffBranch.BranchName,
                                DropoffBranchId = new Guid(dropoffBranch.BranchId),
                                PickupDate = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime").Date,
                                DropoffDate = contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime").Date,
                                Amount = contractItem.GetAttributeValue<Money>("rnt_netamount").Value,
                                IsRevenue = isRevenue,
                                InvoiceDate = contractItem.GetAttributeValue<DateTime>("rnt_invoicedate").Date,
                            });
                        });
                    });
                });

                return new PositionalBonusCalculationResult
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new PositionalBonusCalculationResult
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public UserBasedBonusCalculationResult calculateUserBasedNetBonusAmount(List<Entity> contracts)
        {
            List<UserBasedBonusCalculationData> result = new List<UserBasedBonusCalculationData>();
            ReservationItemRepository reservationItemRepository = new ReservationItemRepository(this.OrgService);
            ContractItemRepository contractItemRepository = new ContractItemRepository(this.OrgService);
            ConfigurationRepository configurationRepository = new ConfigurationRepository(this.OrgService);

            var reservationItems = reservationItemRepository.getCompletedReservationItems(new string[] { "createdby" });
            var completedContractItems = contractItemRepository.getAllCompletedContractItems(new string[] { "rnt_contractid", "rnt_additionalproductid", "rnt_itemtypecode", "createdby", "rnt_externalusercreatedbyid", "rnt_reservationitemid", "rnt_netamount", "rnt_invoicedate" });
            var adminId = new Guid(configurationRepository.GetConfigurationByKey("CrmAdminGuid"));

            try
            {
                var priceDifferenceId = StaticHelper.GetConfiguration("PriceDifferenceId");

                contracts.ForEach(contract =>
                {
                    // Get contract's items
                    var contractItems = completedContractItems.Where(e => e.GetAttributeValue<EntityReference>("rnt_contractid").Id == contract.Id).ToList();

                    contractItems.ForEach(contractItem =>
                    {
                        if (contractItem.Attributes.Contains("rnt_additionalproductid"))
                        {
                            // DEV_CONTROL
                            if (contractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id == new Guid(priceDifferenceId))
                            {
                                return;
                            }
                        }

                        string userName = null;
                        int userType = 0;
                        int itemTypeCode = contractItem.GetAttributeValue<OptionSetValue>("rnt_itemtypecode").Value;

                        // Don't calculate if item type is equipment
                        if (itemTypeCode != (int)ContractItemEnums.ItemTypeCode.Equipment)
                        {
                            // Take contract user
                            if (itemTypeCode == (int)ContractItemEnums.ItemTypeCode.Fine)
                            {
                                userName = contractItem.GetAttributeValue<EntityReference>("createdby").Name;
                                userType = (int)ClassLibrary._Enums_1055.rnt_BonusBusinessRole.SatsDansman;
                            }
                            // Take tablet user
                            else if (contractItem.Attributes.Contains("rnt_externalusercreatedbyid"))
                            {
                                userName = contractItem.GetAttributeValue<EntityReference>("rnt_externalusercreatedbyid").Name;
                                userType = (int)ClassLibrary._Enums_1055.rnt_BonusBusinessRole.TeslimatPersoneli;
                            }
                            else
                            {
                                // DEV_CONTROL
                                if (!contractItem.Attributes.Contains("rnt_reservationitemid"))
                                {
                                    return;
                                }

                                var reservationItem = reservationItems.Where(p => p.Id == contractItem.GetAttributeValue<EntityReference>("rnt_reservationitemid").Id).FirstOrDefault();

                                if (reservationItem != null)
                                {
                                    var userId = reservationItem.GetAttributeValue<EntityReference>("createdby").Id;

                                    // Take contract user
                                    if (userId == adminId)
                                    {
                                        userName = contractItem.GetAttributeValue<EntityReference>("createdby").Name;
                                        userType = (int)ClassLibrary._Enums_1055.rnt_BonusBusinessRole.SatsDansman;
                                    }
                                    // Take reservation user
                                    else
                                    {
                                        userName = reservationItem.GetAttributeValue<EntityReference>("createdby").Name;
                                        userType = (int)ClassLibrary._Enums_1055.rnt_BonusBusinessRole.SatsDansman;
                                    }
                                }
                            }

                            result.Add(new UserBasedBonusCalculationData
                            {
                                Amount = contractItem.GetAttributeValue<Money>("rnt_netamount").Value,
                                BranchId = Convert.ToString(contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id),
                                BranchName = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Name,
                                ContractId = Convert.ToString(contract.GetAttributeValue<Guid>("rnt_contractid")),
                                ContractNumber = contract.GetAttributeValue<string>("rnt_contractnumber"),
                                AdditionalProductName = contractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Name,
                                AdditionalProductId = Convert.ToString(contractItem.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id),
                                UserName = userName,
                                UserType = userType,
                                QueryDate = DateTime.Now.Date,
                                InvoiceDate = contractItem.GetAttributeValue<DateTime>("rnt_invoicedate")
                            });
                        }

                    });
                });

                return new UserBasedBonusCalculationResult
                {
                    IsSuccess = true,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new UserBasedBonusCalculationResult
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        public List<AdditionalProductReportData> createAdditionalProductReportData(List<UserBasedBonusCalculationData> contractItems, List<BranchRevenue> branchRevenues)
        {
            List<AdditionalProductReportData> additionalProductReportDatas = new List<AdditionalProductReportData>();
            BonusBusinessRoleRatesRepository bonusBusinessRoleRatesRepository = new BonusBusinessRoleRatesRepository(this.OrgService);
            BonusCalculationRepository bonusCalculationRepository = new BonusCalculationRepository(this.OrgService);

            var bonusCalculations = bonusCalculationRepository.getAdditionalProductBonusCalculations();
            var activeBonusBusinessRoleRates = bonusBusinessRoleRatesRepository.getBonusBusinessRoleRates();

            contractItems.ForEach(contractItem =>
            {
                List<Entity> businessRoleRates = new List<Entity>();
                List<AdditionalProductRoleRate> additionalProductRoleRates = new List<AdditionalProductRoleRate>();

                decimal bonusRatio = 0;
                var itemAdditionalProductId = contractItem.AdditionalProductId;

                var dailyBonusCalculations = getDailyBonusCalculations(bonusCalculations, contractItem.InvoiceDate);
                var bonusCalculation = dailyBonusCalculations.Where(p => p.GetAttributeValue<EntityReference>("rnt_additionalproductlookup").Id == new Guid(itemAdditionalProductId)).FirstOrDefault();

                // Take bonus calculation's bonus ratio
                if (bonusCalculation != null)
                {
                    bonusRatio = bonusCalculation.GetAttributeValue<decimal>("rnt_bonusratio");
                }

                // Add item to list
                additionalProductReportDatas.Add(new AdditionalProductReportData
                {
                    baseBonusAmount = contractItem.Amount * bonusRatio / 100,
                    branchId = contractItem.BranchId,
                    branchName = contractItem.BranchName,
                    additionalProductAmount = contractItem.Amount,
                    additionalProductId = contractItem.AdditionalProductId,
                    userName = contractItem.UserName,
                    userType = contractItem.UserType
                });
            });

            var groupedAdditionalProductData = additionalProductReportDatas.GroupBy(p => p.branchId).ToList();
            List<AdditionalProductReportData> additionalProductReportResult = new List<AdditionalProductReportData>();

            groupedAdditionalProductData.ForEach(grpBranch =>
            {
                var branchAdditionalProductReportData = grpBranch.ToList();

                List<AdditionalProductRoleRate> additionalProductRoleRates = new List<AdditionalProductRoleRate>();
                activeBonusBusinessRoleRates.ForEach(ratio =>
                {
                    additionalProductRoleRates.Add(new AdditionalProductRoleRate
                    {
                        bonusRatio = Decimal.Round(ratio.GetAttributeValue<decimal>("rnt_bonusratio"), 2),
                        businessRole = Enum.GetName(typeof(ClassLibrary._Enums_1055.rnt_BonusBusinessRole), (int)ratio.GetAttributeValue<OptionSetValue>("rnt_businessrole").Value).insertSpaceBetweenWords()
                    });
                });

                // Get branch revenue
                var revenue = branchRevenues.Where(p => p.branchId.ToString() == grpBranch.Key).Select(p => p.revenueAmount).FirstOrDefault();
                // Sum of base bonus amount
                var baseBonusAmount = grpBranch.Sum(p => p.baseBonusAmount);
                // Sum of additional product amounts
                var totalAdditionalProductAmount = grpBranch.Sum(p => p.additionalProductAmount);
                // Calculate progress ratio
                var progressRatio = totalAdditionalProductAmount / revenue * 100;
                // Calculate final bonus amount
                var bonusAmount = this.calculateBonusAmountByProgressRatio(baseBonusAmount, progressRatio, totalAdditionalProductAmount, revenue);

                // Calculate role based bonus
                List<AdditionalProductRoleRate> branchRoleAmounts = new List<AdditionalProductRoleRate>();

                additionalProductRoleRates.ForEach(roleRate =>
                {
                    branchRoleAmounts.Add(new AdditionalProductRoleRate
                    {
                        roleBonusAmount = Decimal.Round(bonusAmount * roleRate.bonusRatio / 100, 2),
                        businessRole = roleRate.businessRole,
                        bonusRatio = roleRate.bonusRatio,
                    });
                });

                // Calculate user based bonus
                List<AdditionalProductUserRate> additionalProductUserRates = new List<AdditionalProductUserRate>();
                Dictionary<int, decimal> roleTotalAmount = new Dictionary<int, decimal>();

                var groupedUserType = grpBranch.GroupBy(p => p.userType).ToList();
                groupedUserType.ForEach(grpUserType =>
                {
                    roleTotalAmount[grpUserType.Key] = grpUserType.Sum(s => s.additionalProductAmount);
                });

                var groupedUserName = grpBranch.GroupBy(p => p.userName).ToList();
                groupedUserName.ForEach(grpUserName =>
                {
                    var defaultItem = grpUserName.FirstOrDefault();
                    var userType = Enum.GetName(typeof(ClassLibrary._Enums_1055.rnt_BonusBusinessRole), defaultItem.userType);
                    var userTotalAmount = grpUserName.Sum(s => s.additionalProductAmount);

                    branchRoleAmounts.ForEach(roleRate =>
                    {
                        if (roleRate.businessRole.removeEmptyCharacters() == userType)
                        {
                            var bonusAmountPerUser = roleRate.roleBonusAmount / (roleTotalAmount[defaultItem.userType] / 100);
                            var userBonusAmount = (userTotalAmount / 100) * bonusAmountPerUser;

                            additionalProductUserRates.Add(new AdditionalProductUserRate
                            {
                                userName = grpUserName.Key,
                                userType = userType.insertSpaceBetweenWords(),
                                userBonusAmount = Decimal.Round(userBonusAmount, 2)
                            });
                        }
                    });
                });

                // Add to result
                additionalProductReportResult.Add(new AdditionalProductReportData
                {
                    revenue = Decimal.Round(revenue, 2),
                    progressRatio = Decimal.Round(progressRatio, 2),
                    baseBonusAmount = Decimal.Round(baseBonusAmount, 2),
                    additionalProductAmount = totalAdditionalProductAmount,
                    branchId = grpBranch.Key,
                    branchName = getBranchName(grpBranch.Key),
                    progressBonusAmount = Decimal.Round(bonusAmount, 2),
                    additionalProductRoleRates = branchRoleAmounts,
                    additionalProductUserRates = additionalProductUserRates
                });
            });

            return additionalProductReportResult;
        }
    }
}
