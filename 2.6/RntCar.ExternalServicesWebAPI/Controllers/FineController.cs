using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Tablet;
using RntCar.ExternalServices.Security;
using RntCar.IntegrationHelper;
using RntCar.Logger;
using RntCar.RedisCacheHelper.CachedItems;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using RntCar.SDK.Routes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http;

namespace RntCar.ExternalServicesWebAPI.Controllers
{

    //[BasicHttpAuthorizeAttribute_Local]
    public class FineController : ApiController
    {
        [HttpGet]
        [HttpPost]
        [Route("api/fine/testme")]
        public string testme()
        {
            return "i am ok";
        }
        [HttpPost]
        [Route(TabletRouteConfig.getHgsTransitList)]
        public GetHGSTransitListResponse getHgsTransitList([FromBody] GetHGSTransitListParameter getHGSTransitListParameter)
        {
            return new GetHGSTransitListResponse
            {
                transits = new List<HGSTransitData>(),
                responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
            };


            #region HGS Close For Tablet 26.09.2022 Tabletten HGS kesimi olmayacaktır.
            /*CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            try
            {
                EquipmentRepository equipmentRepository = new EquipmentRepository(crmServiceHelper.IOrganizationService);
                var equipment = equipmentRepository.getEquipmentByHGSNumber(getHGSTransitListParameter.productId);
                if (equipment != null)
                {
                    loggerHelper.traceInfo("equipment :" + equipment.Id);
                    ContractRepository contractRepository = new ContractRepository(crmServiceHelper.IOrganizationService);
                    var c = contractRepository.getRentalContractByEquipment(equipment.Id, new string[] { "rnt_ismonthly" });
                    if (c != null && c.GetAttributeValue<bool>("rnt_ismonthly"))
                    {
                        loggerHelper.traceInfo("monthly");
                        return new GetHGSTransitListResponse
                        {
                            transits = new List<HGSTransitData>(),
                            responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
                        };
                    }
                }
                loggerHelper.traceInfo("getHgsTransitList start");
                loggerHelper.traceInfo(JsonConvert.SerializeObject(getHGSTransitListParameter));

                var dropoffDateTimestamp = getHGSTransitListParameter.isManuelProcess ?
                                           getHGSTransitListParameter.finishDateTimeStamp :
                                           DateTime.UtcNow.converttoTimeStamp();

                getHGSTransitListParameter.finishDateTimeStamp = dropoffDateTimestamp;

                loggerHelper.traceInfo("hgs start time " + getHGSTransitListParameter.startDateTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset));
                loggerHelper.traceInfo("hgs finish time " + getHGSTransitListParameter.finishDateTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset));
                using (var hgsHelper = new HGSHelper(crmServiceHelper.IOrganizationService))
                {
                    var _s = getHGSTransitListParameter.startDateTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset).converttoTimeStamp();
                    var _e = getHGSTransitListParameter.finishDateTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset).converttoTimeStamp();

                    getHGSTransitListParameter.startDateTimeStamp = getHGSTransitListParameter.startDateTimeStamp.converttoDateTime().AddDays(-15).converttoTimeStamp();
                    getHGSTransitListParameter.finishDateTimeStamp = getHGSTransitListParameter.finishDateTimeStamp.converttoDateTime().AddDays(15).converttoTimeStamp();
                    var result = hgsHelper.getHgsTransitList(getHGSTransitListParameter);

                    //result.transits = result.transits.Where(p => p.entryDateTime.isBetween(_s, _e)).ToList();

                    result.transits = result.transits.Where(p => p.exitDateTime.isBetween(_s, _e)).ToList();

                    loggerHelper.traceInfo("result : " + JsonConvert.SerializeObject(result));
                    loggerHelper.traceInfo("getHgsTransitList end");
                    return result;
                }

            }
            catch (Exception ex)
            {
                loggerHelper.traceError(ex.Message);
                return new GetHGSTransitListResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }*/
            #endregion
        }

        [HttpPost]
        [Route(TabletRouteConfig.getHgsAdditionalProducts)]
        public GetHgsAdditionalProductsResponse getHgsAdditionalProducts([FromBody] GetHgsAdditionalProductsRequest getHgsAdditionalProductsRequest)
        {
            string contractId = Convert.ToString(getHgsAdditionalProductsRequest.contractId);
            LoggerHelper loggerHelper;
            if (!string.IsNullOrWhiteSpace(contractId))
            {
                loggerHelper = new LoggerHelper(contractId + "GHAP");
            }
            else
            {
                loggerHelper = new LoggerHelper();
            }
            try
            {
                loggerHelper.traceInfo("getHgsAdditionalProducts start");
                loggerHelper.traceInputsInfo<GetHgsAdditionalProductsRequest>(getHgsAdditionalProductsRequest);

                List<AdditonalProductDataTablet> additionalProductData = new List<AdditonalProductDataTablet>();
                CrmServiceHelper crmServiceHelper = new CrmServiceHelper();


                if (getHgsAdditionalProductsRequest.totalAmount > 0)
                {
                    ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
                    var hgsCode = configurationRepository.GetConfigurationByKey("additionalProduct_HGS");

                    AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(crmServiceHelper.IOrganizationService);
                    var hgsAdditionalProduct = additionalProductRepository.getAdditionalProductByProductCode(hgsCode);
                    var hgsadditionalProductData = new AdditionalProductMapper().buildAdditionalProductData(hgsAdditionalProduct, getHgsAdditionalProductsRequest.totalAmount, 0);
                    hgsadditionalProductData.actualAmount = getHgsAdditionalProductsRequest.totalAmount;
                    hgsadditionalProductData.tobePaidAmount = getHgsAdditionalProductsRequest.totalAmount;
                    hgsadditionalProductData.value = 1;

                    additionalProductData.Add(new AdditionalProductMapper().createTabletModel(hgsadditionalProductData));


                    AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(crmServiceHelper.IOrganizationService);
                    var serviceContractItem = additionalProductHelper.getAdditionalProductService_Contract(hgsAdditionalProduct.Id, getHgsAdditionalProductsRequest.contractId);
                    //and add hgs service item
                    if (serviceContractItem.subProduct != null && serviceContractItem.serviceItem == null)
                    {
                        var serviceAmount = additionalProductHelper.calculateFineProductServicePrice(null, hgsAdditionalProduct.Id, getHgsAdditionalProductsRequest.totalAmount, serviceContractItem.subProduct.GetAttributeValue<Money>("rnt_price").Value);

                        var hgsServiceadditionalProductData = new AdditionalProductMapper().buildAdditionalProductData(serviceContractItem.subProduct, serviceAmount, 0);
                        hgsServiceadditionalProductData.actualAmount = serviceAmount;
                        hgsServiceadditionalProductData.tobePaidAmount = serviceAmount;
                        hgsServiceadditionalProductData.value = 1;
                        //hgsServiceadditionalProductData.isServiceFee = true;
                        var item = new AdditionalProductMapper().createTabletModel(hgsServiceadditionalProductData);
                        item.isServiceFee = true;
                        additionalProductData.Add(item);
                    }


                }
                return new GetHgsAdditionalProductsResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess(),
                    hgsAdditionalProductData = additionalProductData
                };
            }
            catch (Exception ex)
            {
                return new GetHgsAdditionalProductsResponse
                {
                    responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        [HttpPost]
        [Route(TabletRouteConfig.getEihlalFineList)]
        public GetEIhlalFineResponse getEIhlalFinesList([FromBody] GetEIhlalFineParameters getEIhlalFineParameters)
        {
            return new GetEIhlalFineResponse
            {
                fineList = new List<EIhlalFineData>(),
                fineAdditionalProducts = new List<AdditonalProductDataTablet>(),
                responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
            };

            #region Traffic Fines Close For Tablet 26.09.2022 Tabletten HGS kesimi olmayacaktır.
            //try
            //{
            //    loggerHelper.traceInfo("getEIhlalFinesList start");
            //    loggerHelper.traceInfo(JsonConvert.SerializeObject(getEIhlalFineParameters));

            //    List<EIhlalFineData> eIhlalFines = new List<EIhlalFineData>();
            //    CrmServiceHelper crmServiceHelper = new CrmServiceHelper();
            //    CultureInfo culture = CultureInfo.InvariantCulture;
            //    var eihlalHelper = new EIhlalHelper(crmServiceHelper.IOrganizationService);

            //    var response = eihlalHelper.getTrafficFine(getEIhlalFineParameters.plateNumber);
            //    if (response.result)
            //    {
            //        long dropoffDateTimestamp = getEIhlalFineParameters.isManuelProcess ?
            //                                    getEIhlalFineParameters.dropoffDatetimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset).converttoTimeStamp() :
            //                                    DateTime.UtcNow.AddMinutes(StaticHelper.offset).converttoTimeStamp();

            //        var pickupDateTime = getEIhlalFineParameters.pickupDateTimeStamp.converttoDateTime().AddMinutes(StaticHelper.offset);

            //        loggerHelper.traceInfo("eihlal pickupDateTime " + pickupDateTime);
            //        loggerHelper.traceInfo("eihlal dropoffdatetime " + dropoffDateTimestamp.converttoDateTime());
            //        var e = response.trafficFineData.Where(item => DateTime.ParseExact(item.fineDate + " " + item.fineTime + ":00", "dd.MM.yyyy HH:mm:ss", culture) >= pickupDateTime &&
            //                                                       DateTime.ParseExact(item.fineDate + " " + item.fineTime + ":00", "dd.MM.yyyy HH:mm:ss", culture) <= dropoffDateTimestamp.converttoDateTime()).ToList();
            //        e.ForEach((item) =>
            //        {
            //            eIhlalFines.Add(new EIhlalFineData
            //            {
            //                ceza_maddesi = item.fineClause,
            //                duzenleyen_birim = item.organizingUnit,
            //                il_ilce = item.addressInfo,
            //                teblig_tarihi = item.communiqueDate,
            //                tutanak_seri = item.reportInfo,
            //                tutanak_sira_no = item.reportNo,
            //                displayText = item.finePlace,
            //                fineAmount = decimal.Parse(item.fineAmount, new NumberFormatInfo() { NumberDecimalSeparator = "," }),
            //                fineDate = DateTime.ParseExact(item.fineDate + " " + item.fineTime + ":00", "dd.MM.yyyy HH:mm:ss", culture).converttoTimeStamp()
            //            });
            //        });
            //    }
            //    loggerHelper.traceInfo("eIhlalFines " + JsonConvert.SerializeObject(eIhlalFines));

            //    List<AdditonalProductDataTablet> additonalProductDataTablets = new List<AdditonalProductDataTablet>();

            //    ConfigurationRepository configurationRepository = new ConfigurationRepository(crmServiceHelper.IOrganizationService);
            //    var cacheKey = configurationRepository.GetConfigurationByKey("additionalProduct_cacheKey");
            //    var productCode = configurationRepository.GetConfigurationByKey("additionalProduct_trafficFineProductCode");
            //    AdditionalProductCacheClient additionalProductCacheClient = new AdditionalProductCacheClient(crmServiceHelper.IOrganizationService, cacheKey);
            //    var fineProduct = additionalProductCacheClient.getAdditionalProductCache(productCode);
            //    var fineProductData = new AdditionalProductMapper().createTabletModel(fineProduct);
            //    var totalFineAmount = eIhlalFines.Sum(item => item.fineAmount);
            //    totalFineAmount = totalFineAmount * 0.75M;
            //    if (totalFineAmount > 0)
            //    {
            //        fineProductData.actualAmount = totalFineAmount;
            //        fineProductData.actualTotalAmount = totalFineAmount;
            //        fineProductData.tobePaidAmount = totalFineAmount;
            //        fineProductData.value = 1;
            //        additonalProductDataTablets.Add(fineProductData);


            //        AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(crmServiceHelper.IOrganizationService);
            //        var fineProductService = additionalProductRepository.getAdditonalProductByParentAdditionalProductId(fineProduct.productId);
            //        AdditionalProductHelper additionalProductHelper = new AdditionalProductHelper(crmServiceHelper.IOrganizationService);
            //        var fineProductAmount = additionalProductHelper.calculateFineProductServicePrice(fineProductService.Id,
            //                                                                                         fineProductService.Id,
            //                                                                                         totalFineAmount,
            //                                                                                         decimal.Zero);
            //        var fineProductServiceData = new AdditionalProductMapper().buildAdditionalProductData(fineProductService, fineProductAmount, fineProductAmount);
            //        var fineProductTabletData = new AdditionalProductMapper().createTabletModel(fineProductServiceData);
            //        fineProductTabletData.actualTotalAmount = fineProductAmount;
            //        fineProductTabletData.tobePaidAmount = fineProductAmount;
            //        fineProductTabletData.isServiceFee = true;
            //        fineProductTabletData.value = 1;

            //        additonalProductDataTablets.Add(fineProductTabletData);
            //    }
            //    loggerHelper.traceInfo("additonalProductDataTablets " + JsonConvert.SerializeObject(additonalProductDataTablets));

            //    return new GetEIhlalFineResponse
            //    {
            //        fineList = eIhlalFines,
            //        fineAdditionalProducts = additonalProductDataTablets,
            //        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnSuccess()
            //    };
            //}
            //catch (Exception ex)
            //{
            //    loggerHelper.traceError("eIhlalFines " + ex.Message);

            //    return new GetEIhlalFineResponse
            //    {
            //        responseResult = ClassLibrary._Tablet.ResponseResult.ReturnError(ex.Message)
            //    };
            //}
            #endregion
        }
    }
}
