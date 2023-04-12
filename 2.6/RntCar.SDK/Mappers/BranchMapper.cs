using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using System;
using System.Collections.Generic;

namespace RntCar.SDK.Mappers
{
    public class BranchMapper
    {
        public List<Branch_Web> createWebBranchList(List<BranchData> branchDatas)
        {
            var convertedData = branchDatas.ConvertAll(item => new Branch_Web
            {
                addressDetail = item.addressDetail,
                branchId = new Guid(item.BranchId),
                branchName = item.BranchName,
                cityId = !string.IsNullOrEmpty(item.CityId) ? (Guid?)new Guid(item.CityId) : null,
                cityName = item.CityName,
                emailaddress = item.emailaddress,
                latitude = item.latitude,
                longitude = item.longitude,
                telephone = item.telephone,
                seoDescription = item.seoDescription,
                seoKeyword = item.seoKeyword,
                seoTitle = item.seoTitle,
                earlistPickupTime = item.earlistPickupTime,
                webRank = item.webRank,
                postalCode = item.postalCode
            });
            return convertedData;
        }

        public List<Branch_Mobile> createMobileBranchList(List<BranchData> branchDatas)
        {
            var convertedData = branchDatas.ConvertAll(item => new Branch_Mobile
            {
                addressDetail = item.addressDetail,
                branchId = new Guid(item.BranchId),
                branchName = item.BranchName,
                cityId = !string.IsNullOrEmpty(item.CityId) ? (Guid?)new Guid(item.CityId) : null,
                cityName = item.CityName,
                emailaddress = item.emailaddress,
                latitude = item.latitude,
                longitude = item.longitude,
                telephone = item.telephone,
                seoDescription = item.seoDescription,
                seoKeyword = item.seoKeyword,
                seoTitle = item.seoTitle,
                branchZone = item.branchZone.Value == 10 ? "Havalimanı" : "Şehir",
                webRank = item.webRank,
                earlistPickupTime = item.earlistPickupTime
            });
            return convertedData;
        }
        public List<Branch_Broker> createBrokerBranchList(List<BranchData> branchData)
        {
            var convertedData = branchData.ConvertAll(item => new Branch_Broker
            {
                addressDetail = item.addressDetail,
                branchId = new Guid(item.BranchId),
                branchName = item.BranchName,
                cityId = !string.IsNullOrEmpty(item.CityId) ? (Guid?)new Guid(item.CityId) : null,
                cityName = item.CityName,
                emailaddress = item.emailaddress,
                latitude = item.latitude,
                longitude = item.longitude,
                telephone = item.telephone,
                seoDescription = item.seoDescription,
                seoKeyword = item.seoKeyword,
                seoTitle = item.seoTitle
            });
            return convertedData;
        }
    }
}
