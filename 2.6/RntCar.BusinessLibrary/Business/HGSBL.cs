using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RestSharp;
using RntCar.BusinessLibrary.Handlers;
using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class HGSBL : BusinessHandler
    {
        public HGSBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public HGSBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public bool IsExistHgsLabel(Entity entity)
        {
            bool checkValue = false;
            Entity hgsLabel = entity;

            string labelNumber = entity.GetAttributeValue<string>("rnt_label");
            QueryExpression getHGSLabel = new QueryExpression(entity.LogicalName);
            getHGSLabel.Criteria.AddCondition("rnt_label", ConditionOperator.Equal, labelNumber);
            getHGSLabel.Criteria.AddCondition("rnt_hgslabelid", ConditionOperator.NotEqual, entity.Id);

            EntityCollection hgsLabelList = this.OrgService.RetrieveMultiple(getHGSLabel);
            checkValue = hgsLabelList.Entities.Count > 0;
            return checkValue;
        }

        
        public ResponseResult saleHGS(Entity entity)
        {
            Entity hgsLabel = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("hgsApiBaseUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "saleProduct", Method.POST);

            var saleProductParameter = this.BuildSaleHgsLabelParameter(hgsLabel);
            restSharpHelper.AddJsonParameter<SaleProductParameter>(saleProductParameter);

            var responseSaleHGS = restSharpHelper.Execute<SaleProductResponse>();

            if (!responseSaleHGS.ResponseResult.Result)
            {
                return ResponseResult.ReturnError(responseSaleHGS.ResponseResult.ExceptionDetail);
            }

            return (ResponseResult)responseSaleHGS.ResponseResult;
        }

        public ResponseResult cancelHGS(Entity entity)
        {
            Entity hgsLabel = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("hgsApiBaseUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "cancelProduct", Method.POST);

            var cancelProductParameter = this.BuildCancelHgsLabelParameter(hgsLabel);
            restSharpHelper.AddJsonParameter<CancelProductParameter>(cancelProductParameter);

            var responseCancelHGS = restSharpHelper.Execute<CancelProductResponse>();

            if (!responseCancelHGS.ResponseResult.Result)
            {
                return ResponseResult.ReturnError(responseCancelHGS.ResponseResult.ExceptionDetail);
            }

            return (ResponseResult)responseCancelHGS.ResponseResult;
        }

        public ResponseResult updateVehicle(Entity entity)
        {
            Entity hgsLabel = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("hgsApiBaseUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "updateVehicleInfo", Method.POST);

            var updateVehicleInfoParameter = this.BuildUpdatePlateNoHgsLabelParameter(hgsLabel);
            restSharpHelper.AddJsonParameter<UpdateVehicleInfoParameter>(updateVehicleInfoParameter);

            var responseUpdateVehicle = restSharpHelper.Execute<UpdateVehicleInfoResponse>();

            if (!responseUpdateVehicle.ResponseResult.Result)
            {
                return ResponseResult.ReturnError(responseUpdateVehicle.ResponseResult.ExceptionDetail);
            }

            return (ResponseResult)responseUpdateVehicle.ResponseResult;
        }

        public ResponseResult updateLimitHGS(Entity entity)
        {
            Entity hgsLabel = entity;

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);

            var responseUrl = configurationBL.GetConfigurationByName("hgsApiBaseUrl");

            RestSharpHelper restSharpHelper = new RestSharpHelper(responseUrl, "updateDirectiveAmounts", Method.POST);

            var updateDirectiveAmountsParameter = this.BuildUpdateLimitHgsLabelParameter(hgsLabel);
            restSharpHelper.AddJsonParameter<UpdateDirectiveAmountsParameter>(updateDirectiveAmountsParameter);

            var responseUpdateLimitHGS = restSharpHelper.Execute<UpdateDirectiveAmountsResponse>();

            if (!responseUpdateLimitHGS.ResponseResult.Result)
            {
                return ResponseResult.ReturnError(responseUpdateLimitHGS.ResponseResult.ExceptionDetail);
            }

            return (ResponseResult)responseUpdateLimitHGS.ResponseResult;
        }

        private SaleProductParameter BuildSaleHgsLabelParameter(Entity hgsLabel)
        {
            EntityReference equipmentRef = hgsLabel.GetAttributeValue<EntityReference>("rnt_equipmentid");
            Entity equipment = this.OrgService.Retrieve(equipmentRef.LogicalName, equipmentRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_platenumber", "rnt_licensenumber"));
            OptionSetValue productTypeValue = hgsLabel.GetAttributeValue<OptionSetValue>("rnt_producttype");
            OptionSetValue vehicleClassValue = hgsLabel.GetAttributeValue<OptionSetValue>("rnt_vehicleclass");
            string productType = "E";
            if (productTypeValue.Value != 1)
            {
                productType = "K";
            }

            SaleProductParameter saleProductParameter = new SaleProductParameter
            {
                productId = hgsLabel.GetAttributeValue<string>("rnt_label"),
                licenseNo = equipment.GetAttributeValue<string>("rnt_licensenumber"),
                plateNo = equipment.GetAttributeValue<string>("rnt_platenumber"),
                productType = productType,
                vehicleClass = vehicleClassValue.Value,
            };
            return saleProductParameter;
        }

        private CancelProductParameter BuildCancelHgsLabelParameter(Entity hgsLabel)
        {
            CancelProductParameter cancelProductParameter = new CancelProductParameter
            {
                productId = hgsLabel.GetAttributeValue<string>("rnt_label"),
                cancelReason = CancelReasonMap(hgsLabel.GetAttributeValue<OptionSetValue>("statuscode")),
            };
            return cancelProductParameter;
        }

        private UpdateVehicleInfoParameter BuildUpdatePlateNoHgsLabelParameter(Entity hgsLabel)
        {
            EntityReference equipmentRef = hgsLabel.GetAttributeValue<EntityReference>("rnt_equipmentid");
            Entity equipment = this.OrgService.Retrieve(equipmentRef.LogicalName, equipmentRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_platenumber", "rnt_licensenumber"));

            UpdateVehicleInfoParameter updateVehicleInfoParameter = new UpdateVehicleInfoParameter
            {
                productId = hgsLabel.GetAttributeValue<string>("rnt_label"),
                licenseNo = equipment.GetAttributeValue<string>("rnt_licensenumber"),
                plateNo = equipment.GetAttributeValue<string>("rnt_platenumber")
            };
            return updateVehicleInfoParameter;
        }

        private UpdateDirectiveAmountsParameter BuildUpdateLimitHgsLabelParameter(Entity hgsLabel)
        {
            EntityReference paymentCardRef = hgsLabel.GetAttributeValue<EntityReference>("rnt_hgspaymentcardid");
            Entity paymentCard = this.OrgService.Retrieve(paymentCardRef.LogicalName, paymentCardRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("rnt_creditcardnumber", "rnt_accountnumber"));

            UpdateDirectiveAmountsParameter updateDirectiveAmountsParameter = new UpdateDirectiveAmountsParameter
            {
                productId = hgsLabel.GetAttributeValue<string>("rnt_label"),
                accountNumber = paymentCard.GetAttributeValue<string>("rnt_accountnumber"),
                creditCardNumber = paymentCard.GetAttributeValue<string>("rnt_creditcardnumber"),
                loadingAmount = hgsLabel.GetAttributeValue<Money>("rnt_loadingamount").Value,
                loadingLowerLimit = hgsLabel.GetAttributeValue<Money>("rnt_loadinglowerlimit").Value,
            };
            return updateDirectiveAmountsParameter;
        }

        private int CancelReasonMap(OptionSetValue statuscode)
        {
            int returnMappingValue = 0;
            if ((int)ClassLibrary._Enums_1033.rnt_hgslabel_StatusCode.Other == statuscode.Value)
            {
                returnMappingValue = 5;
            }
            else if ((int)ClassLibrary._Enums_1033.rnt_hgslabel_StatusCode.Other == statuscode.Value)
            {
                returnMappingValue = 1;
            }
            else if ((int)ClassLibrary._Enums_1033.rnt_hgslabel_StatusCode.Other == statuscode.Value)
            {
                returnMappingValue = 2;
            }
            else if ((int)ClassLibrary._Enums_1033.rnt_hgslabel_StatusCode.Other == statuscode.Value)
            {
                returnMappingValue = 3;
            }
            else if ((int)ClassLibrary._Enums_1033.rnt_hgslabel_StatusCode.Other == statuscode.Value)
            {
                returnMappingValue = 4;
            }
            return returnMappingValue;
        }
    }
}
