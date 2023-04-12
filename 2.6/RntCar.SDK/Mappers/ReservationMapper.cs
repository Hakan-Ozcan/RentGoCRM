using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Mobile.Reservation;
using RntCar.ClassLibrary._Web.Reservation;
using RntCar.ClassLibrary.MongoDB;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.SDK.Mappers
{
    public class ReservationMapper
    {
        public List<ReservationData_Broker> buildBrokerReservationData(List<ReservationItemData> reservationDatas, int langId)
        {
            var statusCodes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_reservation_StatusCode", langId);

            return reservationDatas.ConvertAll(p => new ReservationData_Broker
            {
                reservationId = new Guid(p.ReservationId),
                StatusName = statusCodes.Where(z => z.value.Equals(p.StatusCode)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                PnrNumber = p.PnrNumber,
                PickupTime = p.PickupTime,
                DropoffTime = p.DropoffTime,
                totalAmount = p.ReservationTotalAmount,
                PickupBranchId = p.PickupBranchId,
                DropoffBranchId = p.DropoffBranchId,
                StatusCode = p.StatusCode,
                DropoffBranchName = p.DropoffBranchName,
                PickupBranchName = p.PickupBranchName,
                corporateCustomerName = p.corporateCustomerName,
                corporateCustomerId = p.corporateCustomerId,
                customerId = new Guid(p.CustomerId),
                customerName = p.CustomerName,
                reservationType = p.ReservationType,
                groupCodeId = new Guid(p.GroupCodeInformationId),
                groupCodeName = p.GroupCodeInformationName,
                dummyContactInformation = p.dummyContactInformation !=null ? JsonConvert.DeserializeObject<DummyContactData>(p.dummyContactInformation) : new DummyContactData()
            });
        }
        public List<ReservationData_Web> buildWebReservationData(List<ReservationItemData> reservationDatas, int langId)
        {
            var statusCodes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_reservation_StatusCode", langId);

            return reservationDatas.ConvertAll(p => new ReservationData_Web
            {
                reservationId = new Guid(p.ReservationId),
                StatusName = statusCodes.Where(z => z.value.Equals(p.StatusCode)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                PnrNumber = p.PnrNumber,
                PickupTime = p.PickupTime,
                DropoffTime = p.DropoffTime,
                PickupBranchId = p.PickupBranchId,
                DropoffBranchId = p.DropoffBranchId,
                StatusCode = p.StatusCode,
                DropoffBranchName = p.DropoffBranchName,
                PickupBranchName = p.PickupBranchName,
                corporateCustomerName = p.corporateCustomerName,
                corporateCustomerId = p.corporateCustomerId,
                customerId = new Guid(p.CustomerId),
                customerName = p.CustomerName,
                reservationType = p.ReservationType,
                groupCodeId = new Guid(p.GroupCodeInformationId),
                groupCodeName = p.GroupCodeInformationName
            });
        }
        public List<ReservationData_Mobile> buildMobileReservationData(List<ReservationItemData> reservationDatas, int langId)
        {
            var statusCodes = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_reservation_StatusCode", langId);

            return reservationDatas.ConvertAll(p => new ReservationData_Mobile
            {
                reservationId = new Guid(p.ReservationId),
                StatusName = statusCodes.Where(z => z.value.Equals(p.StatusCode)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                PnrNumber = p.PnrNumber,
                PickupTime = p.PickupTime,
                DropoffTime = p.DropoffTime,
                PickupBranchId = p.PickupBranchId,
                DropoffBranchId = p.DropoffBranchId,
                StatusCode = p.StatusCode,
                DropoffBranchName = p.DropoffBranchName,
                PickupBranchName = p.PickupBranchName,
                corporateCustomerName = p.corporateCustomerName,
                corporateCustomerId = p.corporateCustomerId,
                customerId = new Guid(p.CustomerId),
                customerName = p.CustomerName,
                reservationType = p.ReservationType,
                groupCodeId = new Guid(p.GroupCodeInformationId),
                groupCodeName = p.GroupCodeInformationName
            });
        }

        public ReservationDetailData_Mobile buildMobileReservationDetail(Entity reservation, List<Entity> reservationItems)
        {
            var response = new ReservationDetailData_Mobile
            {
                depositAmount = reservation.Attributes.Contains("rnt_depositamount") ? reservation.GetAttributeValue<Money>("rnt_depositamount").Value : decimal.Zero,
                totalAmount = reservation.Attributes.Contains("rnt_totalamount") ? reservation.GetAttributeValue<Money>("rnt_totalamount").Value : decimal.Zero,
                reservationItems = buildMobileReservationItems(reservationItems)
            };

            return response;
        }

        private List<ReservationItemData_Mobile> buildMobileReservationItems(List<Entity> reservationItems)
        {
            List<ReservationItemData_Mobile> result = new List<ReservationItemData_Mobile>();
            foreach (var item in reservationItems)
            {
                    var index = result.FindIndex(x => x.additionalProductId == item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id);

                    if (index > -1)
                    {
                        result[index].value += 1;
                        result[index].totalAmount = result[index].totalAmount * result[index].value; 
                    }
                    else
                    {
                        var reservationItem = new ReservationItemData_Mobile
                        {
                            itemId = item.Id,
                            reservationId = item.Attributes.Contains("rnt_reservationid") ? item.GetAttributeValue<EntityReference>("rnt_reservationid").Id : Guid.Empty,
                            itemNo = item.Attributes.Contains("rnt_itemno") ? item.GetAttributeValue<string>("rnt_itemno") : null,
                            itemType = item.Attributes.Contains("rnt_itemtypecode") ? item.GetAttributeValue<OptionSetValue>("rnt_itemtypecode")?.Value : null,
                            itemName = item.Attributes.Contains("rnt_name") ? item.GetAttributeValue<string>("rnt_name") : string.Empty,
                            basePrice = item.Attributes.Contains("rnt_baseprice") ? item.GetAttributeValue<Money>("rnt_baseprice")?.Value : null,
                            netAmount = item.Attributes.Contains("rnt_netamount") ? item.GetAttributeValue<Money>("rnt_netamount")?.Value : null,
                            totalAmount = item.Attributes.Contains("rnt_totalamount") ? item.GetAttributeValue<Money>("rnt_totalamount")?.Value : null,
                            additionalProductId = item.Attributes.Contains("rnt_additionalproductid") ? item.GetAttributeValue<EntityReference>("rnt_additionalproductid").Id : Guid.Empty,
                            productCode = item.Attributes.Contains("additionalProducts.rnt_additionalproductcode") ? ((string)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_additionalproductcode").Value) : string.Empty,
                            productDescription = item.Attributes.Contains("additionalProducts.rnt_productdescription") ? ((string)item.GetAttributeValue<AliasedValue>("additionalProducts.rnt_productdescription").Value) : string.Empty,
                            value = 1
                        };

                        result.Add(reservationItem);
                    }
            }

            return result;
        }
    }
}
