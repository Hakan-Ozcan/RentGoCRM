using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.PaymentHelper.iyzico;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.CrmPlugins.Payment.actions
{
    public class ExecuteGetMaturityDifferenceDetail : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            string getMaturityDifferenceDetailRequest;
            initializer.PluginContext.GetContextParameter<string>("MaturityDifferenceDetailRequest", out getMaturityDifferenceDetailRequest);

            initializer.TraceMe("MaturityDifferenceDetailRequest : " + getMaturityDifferenceDetailRequest);
            try
            {
                var request = JsonConvert.DeserializeObject<MaturityDifferenceRequest>(getMaturityDifferenceDetailRequest);
                decimal? existingMaturityAmount = null;
                foreach (var item in request.additionalProducts)
                {
                    item.tobePaidAmount = item.actualTotalAmount - item.paidAmount;
                }

                ConfigurationRepository configurationRepository = new ConfigurationRepository(initializer.Service);
                var maturityDifferenceCode = configurationRepository.GetConfigurationByKey("additionalProduct_MaturityDifference");
                if (!request.isUpdate)
                    request.additionalProducts = request.additionalProducts.Where(p => p.productCode != maturityDifferenceCode).ToList();
                else
                {
                    var existingMaturityCode = request.additionalProducts.Where(p => p.productCode == maturityDifferenceCode).FirstOrDefault();
                    if (existingMaturityCode != null)
                    {
                        existingMaturityCode.actualTotalAmount = existingMaturityCode.paidAmount;
                        existingMaturityCode.tobePaidAmount = decimal.Zero;
                    }
                    existingMaturityAmount = existingMaturityCode?.actualTotalAmount;
                }

                ConfigurationBL configurationBL = new ConfigurationBL(initializer.Service);
                var iyzicoInfo = configurationBL.GetConfigurationByName("iyzicoInformation");
                var configs = IyzicoHelper.setConfigurationValues(iyzicoInfo);
                IyzicoHelper iyzicoHelper = new IyzicoHelper(configs);


                var _totalAmount = decimal.Zero;
                if (!request.isUpdate)
                {
                    _totalAmount = request.equipmentPrice + request.additionalProducts.Sum(p => p.actualTotalAmount);
                }
                else
                {
                    _totalAmount = request.equipmentPrice + request.additionalProducts.Sum(p => p.actualTotalAmount - p.paidAmount);
                }
                initializer.TraceMe("_totalAmount: " + _totalAmount);

                var installmentResponse = iyzicoHelper.retrieveInstallmentforGivenCardBin(request.binNumber, _totalAmount);
                var maturityAmount = installmentResponse.installmentData.FirstOrDefault().Where(p => p.installmentNumber == request.installmentNumber).FirstOrDefault().totalAmount;
                initializer.TraceMe("maturityAmount: " + maturityAmount);
                if (maturityAmount - _totalAmount > 0)
                {
                    var addedMaturityAmount = maturityAmount - _totalAmount;
                    if (existingMaturityAmount.HasValue)
                    {
                        initializer.TraceMe("existingMaturityAmount: " + existingMaturityAmount);
                        addedMaturityAmount += existingMaturityAmount.Value;
                    }
                    initializer.TraceMe("addedMaturityAmount: " + addedMaturityAmount);

                    AdditionalProductsBL additionalProductsBL = new AdditionalProductsBL(initializer.Service, initializer.TracingService);
                    var response = additionalProductsBL.getAdditionalProductMaturityDifference(addedMaturityAmount, maturityDifferenceCode);
                    initializer.TraceMe("MaturityDifferenceDetailResponse : " + JsonConvert.SerializeObject(response));
                    if (!request.isUpdate || !existingMaturityAmount.HasValue)
                    {
                        request.additionalProducts.Add(response);
                    }
                    else
                    {
                        request.additionalProducts.Where(p => p.productCode == maturityDifferenceCode).FirstOrDefault().actualTotalAmount = addedMaturityAmount;
                        request.additionalProducts.Where(p => p.productCode == maturityDifferenceCode).FirstOrDefault().actualAmount = addedMaturityAmount;
                        request.additionalProducts.Where(p => p.productCode == maturityDifferenceCode).FirstOrDefault().tobePaidAmount = addedMaturityAmount - existingMaturityAmount.Value;
                    }

                }
                var data = new MaturityDifferenceResponse
                {
                    totaltobePaidAmount = request.additionalProducts.Sum(p => p.tobePaidAmount),
                    totalAmount = request.additionalProducts.Sum(p => p.actualTotalAmount),
                    additionalProducts = request.additionalProducts,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
                
                var r = JsonConvert.SerializeObject(data);
                
                initializer.TraceMe("response object : " + r);
                initializer.PluginContext.OutputParameters["MaturityDifferenceDetailResponse"] = r;

            }
            catch (Exception ex)
            {
                initializer.PluginContext.OutputParameters["MaturityDifferenceDetailResponse"] = JsonConvert.SerializeObject(new MaturityDifferenceResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                });
            }
        }
    }
}
