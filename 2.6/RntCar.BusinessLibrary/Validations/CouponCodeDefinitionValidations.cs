using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary._Enums_1033;
using System;
using System.Linq;

namespace RntCar.BusinessLibrary.Validations
{
    public class CouponCodeDefinitionValidations : ValidationHandler
    {
        public CouponCodeDefinitionValidations(IOrganizationService orgService) : base(orgService)
        {
        }

        public CouponCodeDefinitionValidations(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public CouponCodeDefinitionValidations(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public bool checkCuponCodeDefinitionGroupCodes(Entity couponCodeDefinition, Guid groupCodeInformationId)
        {
            if (groupCodeInformationId == Guid.Empty)
            {
                return true;
            }
            MultiSelectMappingRepository multiSelectRepository = new MultiSelectMappingRepository(this.OrgService);

            var definitionGroupCodes = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_groupcode",
                couponCodeDefinition.Attributes.Contains("rnt_groupcodeinformations") ? couponCodeDefinition.GetAttributeValue<OptionSetValueCollection>("rnt_groupcodeinformations") : null);
            if (definitionGroupCodes.Count > 0)
            {
                return definitionGroupCodes.Any(p => p == groupCodeInformationId);
            }
            return false;
        }

        public bool checkCuponCodeDefinitionBranchs(Entity couponCodeDefinition, Guid branchId)
        {
            MultiSelectMappingRepository multiSelectRepository = new MultiSelectMappingRepository(this.OrgService);

            var definitionBranchs = multiSelectRepository.getSelectedItemsGuidListByOptionValueandGivenColumn("rnt_branch",
                couponCodeDefinition.Attributes.Contains("rnt_branchcodes") ? couponCodeDefinition.GetAttributeValue<OptionSetValueCollection>("rnt_branchcodes") : null);
            if (definitionBranchs.Count > 0)
            {
                return definitionBranchs.Any(p => p == branchId);
            }
            return false;
        }

        public bool checkCuponCodeDefinitionChannel(Entity couponCodeDefinition, int channelCode)
        {
            MultiSelectMappingRepository multiSelectRepository = new MultiSelectMappingRepository(this.OrgService);
            if (channelCode == 0)
            {
                channelCode = (int)rnt_ReservationChannel.Job;
            }
            var definitionChannels = couponCodeDefinition.Attributes.Contains("rnt_channelcodes") ? couponCodeDefinition.GetAttributeValue<OptionSetValueCollection>("rnt_channelcodes") : null;
            if (definitionChannels != null)
            {
                return definitionChannels.Any(p => p.Value == channelCode);
            }
            return false;
        }

        public bool checkCuponCodeDefinitionReservationType(Entity couponCodeDefinition, int reservationType)
        {
            MultiSelectMappingRepository multiSelectRepository = new MultiSelectMappingRepository(this.OrgService);

            var definitionReservationTypes = couponCodeDefinition.Attributes.Contains("rnt_reservationtype") ? couponCodeDefinition.GetAttributeValue<OptionSetValueCollection>("rnt_reservationtype") : null;
            if (definitionReservationTypes != null)
            {
                return definitionReservationTypes.Any(p => p.Value == reservationType);
            }
            return false;
        }

        public bool checkCuponCodeDefinitionReservationDate(Entity couponCodeDefinition, DateTime reservastionStartDate)
        {
            var beginDate = couponCodeDefinition.GetAttributeValue<DateTime>("rnt_startdate").AddHours(3);
            var endDate = couponCodeDefinition.GetAttributeValue<DateTime>("rnt_enddate").AddHours(3);

            this.Trace("begin date : " + beginDate);
            this.Trace("end date : " + endDate);

            this.Trace("current date : " + this.DateNow);

            if (reservastionStartDate >= beginDate.Date && reservastionStartDate <= endDate.Date)
            {
                this.Trace("coupon code date is valid");
                return true;
            }
            return false;
        }

        public bool checkCuponCodeValidationDate(Entity couponCodeDefinition, out int status)
        {
            var validityStartDate = couponCodeDefinition.GetAttributeValue<DateTime>("rnt_validitystartdate").AddHours(3);
            var validityEndDate = couponCodeDefinition.GetAttributeValue<DateTime>("rnt_validityenddate").AddHours(3);

            this.Trace("validition Start date : " + validityStartDate);
            this.Trace("validition End date : " + validityEndDate);

            this.Trace("current date : " + this.DateNow);

            status = -1;

            if (this.DateNow.Date >= validityStartDate.Date && this.DateNow.Date <= validityEndDate.Date)
            {
                this.Trace("coupon code date is valid");
                status = 0;
                return true;
            }
            else if (this.DateNow.Date >= validityEndDate.Date)
            {
                status = 1;
            }
            else if (this.DateNow.Date <= validityStartDate.Date)
            {
                status = 2;
            }
            return false;
        }
    }
}
