using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using RntCar.ClassLibrary.MarkettingPermission;
using System;

namespace RntCar.SDK.Mappers
{
    public class MarketingPermissionMapper
    {
        public MarketingPermission_Web createWebMarketingPermissionData(Entity marketingPermission)
        {
            MarketingPermission_Web marketingPermission_Web = new MarketingPermission_Web
            {
                emailPermission = marketingPermission.GetAttributeValue<bool>("rnt_allowemails"),
                etkPermission = marketingPermission.GetAttributeValue<bool>("rnt_etkpermission"),
                kvkkPermission = marketingPermission.GetAttributeValue<bool>("rnt_kvkkpermission"),
                notificationPermission = marketingPermission.GetAttributeValue<bool>("rnt_allownotification"),
                smsPermission = marketingPermission.GetAttributeValue<bool>("rnt_allowsms"),
                marketingPermissionId = marketingPermission.Id
            };
            return marketingPermission_Web;
        }

        public MarketingPermission_Mobile createMobileMarketingPermissionData(Entity marketingPermission)
        {
            MarketingPermission_Mobile marketingPermission_Mobile = new MarketingPermission_Mobile
            {
                emailPermission = marketingPermission.GetAttributeValue<bool>("rnt_allowemails"),
                etkPermission = marketingPermission.GetAttributeValue<bool>("rnt_etkpermission"),
                kvkkPermission = marketingPermission.GetAttributeValue<bool>("rnt_kvkkpermission"),
                notificationPermission = marketingPermission.GetAttributeValue<bool>("rnt_allownotification"),
                smsPermission = marketingPermission.GetAttributeValue<bool>("rnt_allowsms"),
                marketingPermissionId = marketingPermission.Id
            };
            return marketingPermission_Mobile;
        }

        public MarketingPermission createMarketingPermissionData(Entity marketingPermission)
        {
            if(marketingPermission == null)
            {
                return new MarketingPermission();
            }

            return new MarketingPermission() {
                marketingPermissionId = marketingPermission.Id,
                operationType = marketingPermission.Attributes.Contains("rnt_operationtype") ? marketingPermission.GetAttributeValue<OptionSetValue>("rnt_operationtype").Value : (int?)null,
                channelCode = marketingPermission.Attributes.Contains("rnt_permissionchannelcode") ? marketingPermission.GetAttributeValue<OptionSetValue>("rnt_permissionchannelcode").Value : (int?)null,
                callPermission = marketingPermission.Attributes.Contains("rnt_allowcall") ? marketingPermission.GetAttributeValue<bool>("rnt_allowcall") : (bool?)null,
                emailPermission = marketingPermission.Attributes.Contains("rnt_allowemails") ? marketingPermission.GetAttributeValue<bool>("rnt_allowemails") : (bool?)null,
                etkPermission = marketingPermission.Attributes.Contains("rnt_etkpermission") ? marketingPermission.GetAttributeValue<bool>("rnt_etkpermission") : (bool?)null,
                kvkkPermission = marketingPermission.Attributes.Contains("rnt_kvkkpermission") ? marketingPermission.GetAttributeValue<bool>("rnt_kvkkpermission") : (bool?)null,
                notificationPermission = marketingPermission.Attributes.Contains("rnt_allownotification") ? marketingPermission.GetAttributeValue<bool>("rnt_allownotification") : (bool?)null,
                smsPermission = marketingPermission.Attributes.Contains("rnt_allowsms") ? marketingPermission.GetAttributeValue<bool>("rnt_allowsms") : (bool?)null,
                contactId = marketingPermission.Attributes.Contains("rnt_contactid") ? marketingPermission.GetAttributeValue<EntityReference>("rnt_contactid").Id : Guid.Empty
            };
        }
    }
}
