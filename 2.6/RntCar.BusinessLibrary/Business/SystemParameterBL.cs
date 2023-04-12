using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;

namespace RntCar.BusinessLibrary.Business
{
    public class SystemParameterBL : BusinessHandler
    {
        public SystemParameterBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }
        public SystemParameterBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public SystemParameterData GetSystemParameters()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            var res = systemParameterRepository.getSystemParameters();
            if(res != null)
            {
                return new SystemParameterData
                {
                    isMernisEnabled = res.GetAttributeValue<bool>("rnt_ismernisenabled"),
                    isCustomerDuplicateCheckEnabled = res.GetAttributeValue<bool>("rnt_iscustomerduplicatecheckenabled"),
                    isTaxnoValidationEnabled = res.GetAttributeValue<bool>("rnt_istaxnovalidationenabled"),
                    isReservationandContractCheckEnabled = res.GetAttributeValue<bool>("rnt_isexistingreservationcontractcheckenabled"),
                    maximumFindeksPoint = res.GetAttributeValue<int>("rnt_maximumfindekspoint")
                };
            }

            //return default values
            return new SystemParameterData
            {
                isMernisEnabled = false,
                isCustomerDuplicateCheckEnabled = false,
                isTaxnoValidationEnabled = false,
                isReservationandContractCheckEnabled = false
            };
        }

        public decimal getSystemTaxRatio()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            return systemParameterRepository.getTaxRatio();
           
        }
        public int getProvider()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            return systemParameterRepository.getProvider();

        }
        public int getReservationShiftDuration()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            return systemParameterRepository.getReservationShiftDuration();

        }
        public ReservationRelatedSystemParameters getReservationRelatedParameters()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            var reservationRelatedParameters = systemParameterRepository.getReservationRelatedSystemParameters();

            return new ReservationRelatedSystemParameters
            {
                reservationCancellationDuration = reservationRelatedParameters.GetAttributeValue<int>("rnt_reservationcancellationduration"),
                reservationCancellationFineDuration = reservationRelatedParameters.GetAttributeValue<int>("rnt_reservationfineduration"),
                reservationNoShowDuration = reservationRelatedParameters.GetAttributeValue<int>("rnt_reservationnoshowduration"),
            };
        }
        public bool getInstallmentEnabled()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            var systemParameter = systemParameterRepository.getIsInstallmentEnabled();

            return systemParameter.Contains("rnt_isinstallmentenabled") ? systemParameter.GetAttributeValue<bool>("rnt_isinstallmentenabled") : false;
        }
        public bool getPegasusCampaignEnabled()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            var systemParameter = systemParameterRepository.getCampaignPegasusEnabled();

            return systemParameter.Contains("rnt_pegasuscampaingnenabled") ? systemParameter.GetAttributeValue<bool>("rnt_pegasuscampaingnenabled") : false;
        }
        public ContractRelatedSystemParameters getContractRelatedSystemParameters()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            var contractRelatedParameters = systemParameterRepository.getContractRelatedSystemParameters();

            return new ContractRelatedSystemParameters
            {
                contractCancellationFineDuration = contractRelatedParameters.GetAttributeValue<int>("rnt_contractcancellationduration"),
                quickContractMinimumDuration = contractRelatedParameters.GetAttributeValue<string>("rnt_quickcontractminduration"),
                contractMinimumDuration = contractRelatedParameters.GetAttributeValue<int>("rnt_contractminduration"),
                checkUserBranch = contractRelatedParameters.GetAttributeValue<bool>("rnt_checkuserbranchforcontractcreate")
            };
        }
        public int getCustomerExpireDay()
        {

            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            return systemParameterRepository.getCustomerExpireDay();
        }

        public MaintenanceRelatedSystemParameters getMaintenanceRelatedSystemParameters()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            var maintenanceRelatedSystemParameters = systemParameterRepository.getMaintenanceRelatedSystemParameters();

            return new MaintenanceRelatedSystemParameters
            {
                maintenanceInformKm = maintenanceRelatedSystemParameters.GetAttributeValue<int>("rnt_maintenanceinformkm"),
                maintenanceLimitKm = maintenanceRelatedSystemParameters.GetAttributeValue<int>("rnt_maintenancelimitkm"),
                maintenanceInformDay = maintenanceRelatedSystemParameters.GetAttributeValue<int>("rnt_maintenanceinformday")
            };
        }
        public int getMonthlyKilometerLimit()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            return systemParameterRepository.getMonthlyKilometerLimit();
        }
        public int getKilometerLimitTolerance()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            return systemParameterRepository.getKilometerLimitTolerance();
        }

        public int getCustomerCreditCardLimit()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            return systemParameterRepository.getCustomerCreditCardLimit();

        }


        public EntityReference getAdminId()
        {
            SystemParameterRepository systemParameterRepository = new SystemParameterRepository(this.OrgService);
            return systemParameterRepository.getAdminId();

        }
    }
}
