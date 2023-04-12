using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Validations
{
    public class WorkingHourValidation : ValidationHandler
    {
        public WorkingHourValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public WorkingHourValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public string checkBranchWorkingHour(Guid pickupBranchId,
                                             Guid dropoffBranchId,
                                             DateTime pickupDateTime,
                                             DateTime dropoffDateTime,
                                             int langId,
                                             int channel)
        {
            if(channel != (int)rnt_ReservationChannel.Web && channel != (int)rnt_ReservationChannel.Mobile)
            {
                return string.Empty;
            }
            WorkingHourBL workingHourBL = new WorkingHourBL(this.OrgService);
            this.Trace("param.dropoffDateTime" + dropoffDateTime);
            this.Trace("param.pickupDateTime" + pickupDateTime);
            this.Trace("pickupdate day of week" + (int)pickupDateTime.DayOfWeek);
            this.Trace("dropoff day of week" + (int)dropoffDateTime.DayOfWeek);

            var pickupWorkingHour = workingHourBL.getWorkingHourByBranchIdAndSelectedDay(pickupBranchId, (int)pickupDateTime.DayOfWeek);
            var dropoffWorkingHour = workingHourBL.getWorkingHourByBranchIdAndSelectedDay(dropoffBranchId, (int)dropoffDateTime.DayOfWeek);
            if (pickupWorkingHour == null || dropoffWorkingHour == null)
            {
                this.Trace(pickupWorkingHour == null ? "pickupworkinghour is null" : "pickupworkinghour is not null");
                this.Trace(dropoffWorkingHour == null ? "dropoffWorkingHour is null" : "dropoffWorkingHour is not null");

                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                return xrmHelper.GetXmlTagContentByGivenLangId("NullWorkingHours", langId);
                
            }
            var pickupWorkingHourBegingingTime = pickupDateTime.Date.AddMinutes(pickupWorkingHour.BeginingTime);
            var pickupWorkingHourEndTime = pickupDateTime.Date.AddMinutes(pickupWorkingHour.EndTime);

            if (!pickupDateTime.IsBetween(pickupWorkingHourBegingingTime, pickupWorkingHourEndTime))
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                return xrmHelper.GetXmlTagContentByGivenLangId("InvalidPickupBranchWorkingHour", langId);
            }

            var dropOffWorkingHourBegingingTime = dropoffDateTime.Date.AddMinutes(dropoffWorkingHour.BeginingTime);
            var dropOffWorkingHourEndTime = dropoffDateTime.Date.AddMinutes(dropoffWorkingHour.EndTime);
            if (!dropoffDateTime.IsBetween(dropOffWorkingHourBegingingTime, dropOffWorkingHourEndTime))
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                return xrmHelper.GetXmlTagContentByGivenLangId("InvalidPickupBranchWorkingHour", langId);
            }
            return string.Empty;
        }

        public ValidationResponse checkBranchWorkingHoursForWeekdays(Guid branchId, Guid userId)
        {
            WorkingHourBL workingHourBl = new WorkingHourBL(this.OrgService);
            var workingHours = workingHourBl.getWorkingHourByBranchId(branchId);

            int mondayCount = workingHours.Where(s => s.GetAttributeValue<OptionSetValue>("rnt_daycode").Value == (int)rnt_DayCode.Monday).ToList().Count;
            int tuesdayCount = workingHours.Where(s => s.GetAttributeValue<OptionSetValue>("rnt_daycode").Value == (int)rnt_DayCode.Tuesday).ToList().Count;
            int wednesdayCount = workingHours.Where(s => s.GetAttributeValue<OptionSetValue>("rnt_daycode").Value == (int)rnt_DayCode.Wednesday).ToList().Count;
            int thursdayCount = workingHours.Where(s => s.GetAttributeValue<OptionSetValue>("rnt_daycode").Value == (int)rnt_DayCode.Thursday).ToList().Count;
            int fridayCount = workingHours.Where(s => s.GetAttributeValue<OptionSetValue>("rnt_daycode").Value == (int)rnt_DayCode.Friday).ToList().Count;
            int saturdayCount = workingHours.Where(s => s.GetAttributeValue<OptionSetValue>("rnt_daycode").Value == (int)rnt_DayCode.Saturday).ToList().Count;
            int sundayCount = workingHours.Where(s => s.GetAttributeValue<OptionSetValue>("rnt_daycode").Value == (int)rnt_DayCode.Sunday).ToList().Count;

            if (mondayCount < 1 || tuesdayCount < 1 || wednesdayCount < 1  ||  thursdayCount < 1  || fridayCount < 1 || saturdayCount < 1 || sundayCount < 1)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                var message = xrmHelper.GetXmlTagContent(userId,"WorkingHourValidationWeekdaysCheck", this.branchXmlPath);
                return new ValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            else
            {
                return new ValidationResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
        }
    }
}
