using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using RntCar.ClassLibrary.MongoDB;
using System.Collections.Generic;
using System;
using RntCar.SDK.Common;

namespace RntCar.SDK.Mappers
{
    public class AvailabilityMapper
    {
        public ClassLibrary.MongoDB.AvailabilityParameters createAvailabilityParameter_Web(AvailabilityParameters_Web availabilityParameters_Web,
                                                                                           int channelCode,
                                                                                           int customerType,
                                                                                           int shifDuration)
        {
            return new ClassLibrary.MongoDB.AvailabilityParameters
            {
                dropOffBranchId = availabilityParameters_Web.queryParameters.dropoffBranchId,
                dropoffDateTime = availabilityParameters_Web.queryParameters.dropoffDateTime,
                pickupBranchId = availabilityParameters_Web.queryParameters.pickupBranchId,
                pickupDateTime = availabilityParameters_Web.queryParameters.pickupDateTime,
                reservationId = availabilityParameters_Web.reservationId.HasValue ? Convert.ToString(availabilityParameters_Web.reservationId.Value) : null,
                individualCustomerId = availabilityParameters_Web.individualCustomerId.HasValue ? Convert.ToString(availabilityParameters_Web.individualCustomerId.Value) : null,
                shiftDuration = shifDuration,
                channel = channelCode,
                customerType = customerType,
                campaignId = availabilityParameters_Web.campaignId,
                segment = availabilityParameters_Web.segmentCode,
                priceCodeId = availabilityParameters_Web.priceCodeId,
                corporateCustomerId = availabilityParameters_Web.corporateCustomerId.HasValue ? Convert.ToString(availabilityParameters_Web.corporateCustomerId) : null,
                isMonthly = availabilityParameters_Web.isMonthly.HasValue ? availabilityParameters_Web.isMonthly.Value : false,
                month_pickupdatetime= availabilityParameters_Web.isMonthly.HasValue && availabilityParameters_Web.isMonthly.Value ? availabilityParameters_Web.queryParameters.pickupDateTime : new DateTime(),
                month_dropoffdatetime = availabilityParameters_Web.isMonthly.HasValue && availabilityParameters_Web.isMonthly.Value ? availabilityParameters_Web.queryParameters.pickupDateTime : new DateTime(),
                monthValue = availabilityParameters_Web.monthValue,
            };
        }

        public ClassLibrary.MongoDB.AvailabilityParameters createAvailabilityParameter_Mobile(AvailabilityParameters_Mobile availabilityParameters_Mobile,
                                                                                           int channelCode,
                                                                                           int customerType,
                                                                                           int shifDuration)
        {
            CommonHelper commonHelper = new CommonHelper();

            return new ClassLibrary.MongoDB.AvailabilityParameters
            {
                dropOffBranchId = availabilityParameters_Mobile.queryParameters.dropoffBranchId,
                dropoffDateTime = availabilityParameters_Mobile.queryParameters.dropoffDateTime,
                pickupBranchId = availabilityParameters_Mobile.queryParameters.pickupBranchId,
                pickupDateTime = availabilityParameters_Mobile.queryParameters.pickupDateTime,
                reservationId = availabilityParameters_Mobile.reservationId.HasValue ? Convert.ToString(availabilityParameters_Mobile.reservationId.Value) : null,
                individualCustomerId = availabilityParameters_Mobile.individualCustomerId.HasValue ? Convert.ToString(availabilityParameters_Mobile.individualCustomerId.Value) : null,
                shiftDuration = shifDuration,
                channel = channelCode,
                customerType = customerType,
                campaignId = availabilityParameters_Mobile.campaignId,
                segment = availabilityParameters_Mobile.segmentCode,
                priceCodeId = availabilityParameters_Mobile.priceCodeId,
                corporateCustomerId = availabilityParameters_Mobile.corporateCustomerId.HasValue ? Convert.ToString(availabilityParameters_Mobile.corporateCustomerId) : null
            };
        }

        public ClassLibrary.MongoDB.AvailabilityParameters createAvailabilityParameter_Broker(AvailabilityParameters_Broker availabilityParameters_Broker,
                                                                                           int channelCode,
                                                                                           int customerType,
                                                                                           int shifDuration,
                                                                                           Guid priceCodeId,
                                                                                           Guid corporateCustomerId,
                                                                                           bool processIndividualPrices)
        {
            return new ClassLibrary.MongoDB.AvailabilityParameters
            {
                dropOffBranchId = availabilityParameters_Broker.queryParameters.dropoffBranchId,
                dropoffDateTime = availabilityParameters_Broker.queryParameters.dropoffDateTime,
                pickupBranchId = availabilityParameters_Broker.queryParameters.pickupBranchId,
                pickupDateTime = availabilityParameters_Broker.queryParameters.pickupDateTime,
                corporateCustomerId = Convert.ToString(corporateCustomerId),
                shiftDuration = shifDuration,
                channel = channelCode,
                customerType = customerType,
                priceCodeId = priceCodeId,
                processIndividualPrices_broker = processIndividualPrices
            };
        }

        public List<AvailabilityData_Web> createWebAvailabilityList(List<AvailabilityData> availabilityDatas)
        {
            var convertedData = availabilityDatas.ConvertAll(item => new AvailabilityData_Web
            {
                //amounttobePaid = item.totalPrice -  item.documentEquipmentPrice,
                groupCodeId = item.groupCodeInformationId,
                groupCodeName = item.groupCodeInformationName,
                //paidAmount = item.documentEquipmentPrice,
                ratio = item.ratio,
                //totalAmount = item.totalPrice,
                payLaterTotalAmount = item.payLaterTotalPrice,
                payNowTotalAmount = item.payNowTotalPrice
            });
            return convertedData;
        }

        public List<AvailabilityData_Mobile> createMobileAvailabilityList(List<AvailabilityData> availabilityDatas)
        {
            var convertedData = availabilityDatas.ConvertAll(item => new AvailabilityData_Mobile
            {
                groupCodeId = item.groupCodeInformationId,
                groupCodeName = item.groupCodeInformationName,
                ratio = item.ratio,
                payLaterTotalAmount = item.payLaterTotalPrice,
                payNowTotalAmount = item.payNowTotalPrice
            });
            return convertedData;
        }

        public List<AvailabilityData_Broker> createBrokerAvailabilityList(List<AvailabilityData> availabilityDatas)
        {
            var convertedData = availabilityDatas.ConvertAll(item => new AvailabilityData_Broker
            {
                //amounttobePaid = item.totalPrice -  item.documentEquipmentPrice,
                groupCodeId = item.groupCodeInformationId,
                groupCodeName = item.groupCodeInformationName,
                //paidAmount = item.documentEquipmentPrice,
                ratio = item.ratio,
                //totalAmount = item.totalPrice,
                payAmount = item.payLaterTotalPrice
            });
            return convertedData;
        }
    }
}
