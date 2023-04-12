using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Tablet;
using RntCar.ClassLibrary._Web;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.SDK.Mappers
{
    public class GroupCodeInformationMapper
    {
        public List<GroupCodeInformation> createTabletModelList(List<GroupCodeInformationDetailData> groupCodeData)
        {
            var convertedData = groupCodeData.ConvertAll(item => new GroupCodeInformation
            {
                groupCodeId = item.groupCodeInformationId,
                groupCodeName = item.groupCodeInformationName,
                groupCodeDescription = item.showRoomModelName,
                transmissionName = item.gearboxcodeName,
                fuelTypeName = item.fueltypecodeName,
                groupCodeImage = item.image,
                isDoubleCard = item.isDoubleCard,
                depositAmount = item.deposit
            });
            return convertedData;
        }
        public List<GroupCodeInformation_Web> createWebGroupCodeList(List<GroupCodeInformationDetailData> groupCodeData, List<OptionSetModel> segmentNameData, int langId)
        {
            var segments = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_SegmentCode", langId);
            var convertedData = groupCodeData.ConvertAll(item => new GroupCodeInformation_Web
            {
                groupCodeId = item.groupCodeInformationId,
                groupCodeName = item.groupCodeInformationName,
                groupCodeDescription = item.groupCodeDescription,
                transmissionName = item.gearboxcodeName,
                fuelTypeName = item.fueltypecodeName,
                isDoubleCard = item.isDoubleCard,
                depositAmount = item.deposit,
                findeksPoint = item.findeks.Value,
                minimumAge = item.minimumAge.Value,
                youngDriverAge = item.youngDriverAge.Value,
                minimumDriverLicense = item.minimumDriverLicense.Value,
                youngDriverMinimumLicense = item.youngDriverMinimumLicense.Value,
                fuelType = item.fueltypecode,
                transmission = item.gearboxcode,
                segment = item.segment.HasValue ? item.segment.Value : 0,
                segmentName = item.segment.HasValue ? segmentNameData.Where(p => p.value == item.segment.Value).Select(i => i.label).FirstOrDefault() : "",
                showRoomBrandName = item.showRoomBrandName,
                showRoomModelName = item.showRoomModelName,
                webImageURL = item.webImageURL,
                colorName = item.colorName,
                engineVolumeId = item.engineVolume,
                SIPPCode = item.SIPPCode
            });
            return convertedData;
        }
        public List<GroupCodeInformation_Mobile> createMobileGroupCodeList(List<GroupCodeInformationDetailData> groupCodeData, List<OptionSetModel> segmentNameData, int langid)
        {
            var segments = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_SegmentCode", langid);
            var convertedData = groupCodeData.ConvertAll(item => new GroupCodeInformation_Mobile
            {
                groupCodeId = item.groupCodeInformationId,
                groupCodeName = item.groupCodeInformationName,
                groupCodeDescription = item.groupCodeDescription,
                transmissionName = item.gearboxcodeName,
                fuelTypeName = item.fueltypecodeName,
                isDoubleCard = item.isDoubleCard,
                depositAmount = item.deposit,
                findeksPoint = item.findeks.Value,
                minimumAge = item.minimumAge.Value,
                youngDriverAge = item.youngDriverAge.Value,
                minimumDriverLicense = item.minimumDriverLicense.Value,
                youngDriverMinimumLicense = item.youngDriverMinimumLicense.Value,
                fuelType = item.fueltypecode,
                transmission = item.gearboxcode,
                segment = item.segment.HasValue ? item.segment.Value : 0,
                segmentName = item.segment.HasValue ? segmentNameData.Where(p => p.value == item.segment.Value).Select(i => i.label).FirstOrDefault() : "",
                showRoomBrandName = item.showRoomBrandName,
                showRoomModelName = item.showRoomModelName,
                SIPPCode = item.SIPPCode
            });

            return convertedData;
        }

        public List<GroupCodeInformation_Broker> createBrokerGroupCodeList(List<GroupCodeInformationDetailData> groupCodeData, List<Entity> brokerMappings, int langId)
        {
            List<GroupCodeInformation_Broker> brokerCodeGroupList = new List<GroupCodeInformation_Broker>();

            groupCodeData.ForEach(item =>
            {
                var mapping = brokerMappings.Where(p => p.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value == item.segment.Value).FirstOrDefault();
                brokerCodeGroupList.Add(new GroupCodeInformation_Broker
                {
                    groupCodeId = item.groupCodeInformationId,
                    groupCodeName = item.groupCodeInformationName,
                    groupCodeDescription = item.groupCodeDescription,
                    transmissionName = item.gearboxcodeName,
                    fuelTypeName = item.fueltypecodeName,
                    isDoubleCard = item.isDoubleCard,
                    depositAmount = item.deposit,
                    findeksPoint = item.findeks.Value,
                    minimumAge = item.minimumAge.Value,
                    youngDriverAge = item.youngDriverAge.Value,
                    minimumDriverLicense = item.minimumDriverLicense.Value,
                    youngDriverMinimumLicense = item.youngDriverMinimumLicense.Value,
                    fuelType = item.fueltypecode,
                    transmission = item.gearboxcode,
                    segment = mapping != null ? mapping.GetAttributeValue<int>("rnt_optionvalue") : 0,
                    segmentName = mapping != null ? mapping.GetAttributeValue<string>("rnt_segmentdefinition").removeAlphaNumericCharactersFromString() : "",
                    showRoomBrandName = item.showRoomBrandName,
                    showRoomModelName = item.showRoomModelName,
                    webImageURL =  item.webImageURL,
                    SIPPCode = item.SIPPCode
                });
            });

            return brokerCodeGroupList;
        }

        public ReservationEquipmentParameters buildReservationEquipmentParameter(GroupCodeInformation_Web groupCodeInformation_Web)
        {
            return new ReservationEquipmentParameters
            {
                depositAmount = groupCodeInformation_Web.depositAmount.Value,
                groupCodeInformationId = groupCodeInformation_Web.groupCodeId,
                groupCodeInformationName = groupCodeInformation_Web.groupCodeName,
                itemName = groupCodeInformation_Web.groupCodeDescription,
                segment = groupCodeInformation_Web.segment
            };
        }

        public ReservationEquipmentParameters buildReservationEquipmentParameter(GroupCodeInformation_Mobile groupCodeInformation_Mobile)
        {
            return new ReservationEquipmentParameters
            {
                depositAmount = groupCodeInformation_Mobile.depositAmount.Value,
                groupCodeInformationId = groupCodeInformation_Mobile.groupCodeId,
                groupCodeInformationName = groupCodeInformation_Mobile.groupCodeName,
                itemName = groupCodeInformation_Mobile.groupCodeDescription,
                segment = groupCodeInformation_Mobile.segment
            };
        }

        public ReservationEquipmentParameters buildReservationEquipmentParameter(Entity groupCodeInformation, ReservationPriceParameter_Broker priceParameters, ReservationEquimentParameters_Broker equipmentParameters)
        {
            return new ReservationEquipmentParameters
            {
                groupCodeInformationId = groupCodeInformation.Id,
                depositAmount = groupCodeInformation.GetAttributeValue<decimal>("rnt_deposit"),
                groupCodeInformationName = groupCodeInformation.GetAttributeValue<string>("rnt_name"),
                segment = groupCodeInformation.GetAttributeValue<OptionSetValue>("rnt_segment").Value,
                itemName = groupCodeInformation.GetAttributeValue<string>("rnt_groupcodedefinition"),
                billingType = CommonHelper.decideBillingType((int)rnt_reservationitem_rnt_itemtypecode.Equipment, Convert.ToString(priceParameters.paymentMethodCode), equipmentParameters.billingType).Value
            };
        }

        public List<GroupCodeImage_Mobile> createMobileGroupCodeImageList(List<GroupCodeImageData> groupCodeImageData)
        {
            var convertedData = groupCodeImageData.ConvertAll(item => new GroupCodeImage_Mobile
            {
                groupCodeInformationId = item.groupCodeInformationId.ToString(),
                groupCodeInformationName = item.groupCodeInformationName,
                imageUrl = string.IsNullOrEmpty(item.imageUrl) ? "default_url" : item.imageUrl
            });

            return convertedData;
        }
    }
}
