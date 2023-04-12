using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using System;
using System.Collections.Generic;

namespace RntCar.SDK.Mappers
{
    public class WorkingHourMapper
    {
        public List<WorkingHour_Web> createWebWorkingHourList(List<WorkingHourData> workingHours)
        {
            var convertedData = workingHours.ConvertAll(item => new WorkingHour_Web
            {
                beginingTime = item.BeginingTime,
                branchId = item.BranchId,
                branchName = item.BranchName,
                dayCode = item.DayCode,
                endTime = item.EndTime,
                workingHourId = item.WorkingHourId
            });
            return convertedData;
        }

        public List<WorkingHour_Mobile> createMobileWorkingHourList(List<WorkingHourData> workingHourDatas)
        {
            var convertedData = workingHourDatas.ConvertAll(item => new WorkingHour_Mobile
            {
                beginingTime = item.BeginingTime,
                branchId = item.BranchId,
                branchName = item.BranchName,
                dayCode = item.DayCode,
                endTime = item.EndTime,
                workingHourId = item.WorkingHourId
            });
            return convertedData;
        }

        public List<WorkingHour_Broker> createBrokerWorkingHourList(List<WorkingHourData> workingHours)
        {
            var convertedData = workingHours.ConvertAll(item => new WorkingHour_Broker
            {
                beginingTime = item.BeginingTime,
                branchId = item.BranchId,
                branchName = item.BranchName,
                dayCode = item.DayCode,
                endTime = item.EndTime,
                workingHourId = item.WorkingHourId
            });
            return convertedData;
        }
    }
}
