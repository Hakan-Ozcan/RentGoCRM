using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using System;
using System.Collections.Generic;

namespace RntCar.SDK.Mappers
{
    public class IndividualAddressMapper
    {
        public List<IndividualAddressData_Web> createWebIndividualAddressList(List<IndividualAddressData> individualAddressDatas)
        {
            var convertedData = individualAddressDatas.ConvertAll(item => new IndividualAddressData_Web
            {
                addressDetail = item.addressDetail,
                cityId = item.cityId == Guid.Empty ? null  : (Guid?)item.cityId,
                cityName = item.cityName,
                countryId = item.countryId == Guid.Empty ? null : (Guid?)item.countryId,
                countryName = item.countryName,
                districtId = item.districtId == Guid.Empty ? null : (Guid?)item.districtId,
                districtName = item.districtName,
                individualAddressId = item.individualAddressId,
                isDefaultAddress = item.isDefaultAddress,
                name = item.name
            });
            return convertedData;
        }

        public List<IndividualAddressData> buildIndividualAddressData(List<Entity> entities)
        {
            List<IndividualAddressData> result = new List<IndividualAddressData>();

            foreach (var item in entities)
            {
                result.Add(new IndividualAddressData
                {
                    individualAddressId = item.Id,
                    isDefaultAddress = item.Attributes.Contains("rnt_isdefaultaddress") ? item.GetAttributeValue<bool>("rnt_isdefaultaddress") : false,
                    name = item.Attributes.Contains("rnt_name") ? item.GetAttributeValue<string>("rnt_name") : string.Empty,
                    countryId = item.Attributes.Contains("rnt_countryid") ? item.GetAttributeValue<EntityReference>("rnt_countryid").Id : Guid.Empty,
                    countryName = item.Attributes.Contains("rnt_countryid") ? item.GetAttributeValue<EntityReference>("rnt_countryid").Name : string.Empty,
                    cityId = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Id : Guid.Empty,
                    cityName = item.Attributes.Contains("rnt_cityid") ? item.GetAttributeValue<EntityReference>("rnt_cityid").Name : string.Empty,
                    districtId = item.Attributes.Contains("rnt_districtid") ? item.GetAttributeValue<EntityReference>("rnt_districtid").Id : Guid.Empty,
                    districtName = item.Attributes.Contains("rnt_districtid") ? item.GetAttributeValue<EntityReference>("rnt_districtid").Name : string.Empty,
                    addressDetail = item.Attributes.Contains("rnt_addressdetail") ? item.GetAttributeValue<string>("rnt_addressdetail") : string.Empty,
                });
            }

            return result;
        }

        public IndividualAddressData_Web createWebDefaultIndividualAddress(Entity individualAddress)
        {
            var convertedData = new IndividualAddressData_Web
            {
                addressDetail = individualAddress.GetAttributeValue<string>("rnt_addressdetail"),
                cityId = individualAddress.GetAttributeValue<EntityReference>("rnt_cityid").Id,
                cityName = individualAddress.GetAttributeValue<EntityReference>("rnt_cityid").Name,
                countryId = individualAddress.GetAttributeValue<EntityReference>("rnt_countryid").Id,
                countryName = individualAddress.GetAttributeValue<EntityReference>("rnt_countryid").Name,
                districtId = individualAddress.GetAttributeValue<EntityReference>("rnt_districtid").Id,
                districtName = individualAddress.GetAttributeValue<EntityReference>("rnt_districtid").Name,
                individualAddressId = individualAddress.GetAttributeValue<Guid>("rnt_individualaddressid"),
                isDefaultAddress = individualAddress.GetAttributeValue<bool>("rnt_isdefaultaddress"),
                name = individualAddress.GetAttributeValue<string>("name")
            };
            return convertedData;
        }

        public List<IndividualAddressData_Mobile> createMobileIndividualAddressList(List<IndividualAddressData> individualAddressDatas)
        {
            var convertData = individualAddressDatas.ConvertAll(item => new IndividualAddressData_Mobile
            {
                addressDetail = item.addressDetail,
                cityId = item.cityId == Guid.Empty ? null : (Guid?)item.cityId,
                cityName = item.cityName,
                countryId = item.countryId == Guid.Empty ? null : (Guid?)item.countryId,
                countryName = item.countryName,
                districtId = item.districtId == Guid.Empty ? null : (Guid?)item.districtId,
                districtName = item.districtName,
                individualAddressId = item.individualAddressId,
                isDefaultAddress = item.isDefaultAddress,
                name = item.name
            });

            return convertData;
        }
    }
}
