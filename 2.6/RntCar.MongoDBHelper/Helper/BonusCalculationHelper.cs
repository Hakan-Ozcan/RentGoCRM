using RntCar.ClassLibrary;
using RntCar.MongoDBHelper.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.MongoDBHelper.Helper
{
    public class BonusCalculationHelper
    {
        public List<PositionalBonusCalculationData> createPositionalBonusCalculationData(List<PositionalBonusCalculationDataMongoDB> positionalBonusCalculationDataMongoDb)
        {
            var bonusCalculations = new List<PositionalBonusCalculationData>();

            positionalBonusCalculationDataMongoDb.ForEach(bonusCalculation =>
            {
                bonusCalculations.Add(new PositionalBonusCalculationData
                {
                    InvoiceDate = bonusCalculation.InvoiceDate,
                    Amount = bonusCalculation.Amount,
                    BaseBonusRatio = bonusCalculation.BaseBonusRatio,
                    BonusCalculationId = bonusCalculation.BonusCalculationId,
                    BusinessRole = bonusCalculation.BusinessRole,
                    ContractItem = bonusCalculation.ContractItem,
                    ContractItemId = bonusCalculation.ContractItemId,
                    ContractNumber = bonusCalculation.ContractNumber,
                    DropoffBranch = bonusCalculation.DropoffBranch,
                    DropoffDate = bonusCalculation.DropoffDate,
                    IsRevenue = bonusCalculation.IsRevenue,
                    PickupBranch = bonusCalculation.PickupBranch,
                    PickupBranchId = bonusCalculation.PickupBranchId,
                    PickupDate = bonusCalculation.PickupDate,
                    PnrNumber = bonusCalculation.PnrNumber,
                    PositionalBonusRatio = bonusCalculation.PositionalBonusRatio,
                    QueryDate = bonusCalculation.QueryDate
                });
            });

            return bonusCalculations;
        }

        public List<UserBasedBonusCalculationData> createUserBasedBonusCalculationData(List<UserBasedBonusCalculationDataMongoDB> userBasedBonusCalculationDataMongoDb)
        {
            List<UserBasedBonusCalculationData> userBasedBonusCalculationDatas = new List<UserBasedBonusCalculationData>();
            userBasedBonusCalculationDataMongoDb.ForEach(item =>
            {
                userBasedBonusCalculationDatas.Add(new UserBasedBonusCalculationData
                {
                    AdditionalProductId = item.AdditionalProductId,
                    AdditionalProductName = item.AdditionalProductName,
                    Amount = item.Amount,
                    BranchId = item.BranchId,
                    BranchName = item.BranchName,
                    ContractId = item.ContractId,
                    ContractNumber = item.ContractNumber,
                    InvoiceDate = item.InvoiceDate,
                    QueryDate = item.QueryDate,
                    UserName = item.UserName,
                    UserType = item.UserType
                });
            });

            return userBasedBonusCalculationDatas;
        }
    }
}
