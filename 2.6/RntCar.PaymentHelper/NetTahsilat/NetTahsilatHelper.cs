using RntCar.ClassLibrary;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace RntCar.PaymentHelper.NetTahsilat
{
    public class NetTahsilatHelper
    {
        public NetTahsilatConfiguration netTahsilatConfiguration { get; set; }
        public NetTahsilatHelper(NetTahsilatConfiguration _netTahsilatConfiguration)
        {
            netTahsilatConfiguration = _netTahsilatConfiguration;
        }
        public CreateCreditCardResponse createCreditCard(CreateCreditCardParameters createCreditCardParameters)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            NetTahsilatService.PaymentWebServiceClient paymentWebServiceClient = new NetTahsilatService.PaymentWebServiceClient(GetBasicHttpsBinding(), GetEndPointAddress(netTahsilatConfiguration.nettahsilat_url));
            NetTahsilatService.SaveCreditCardRequest saveCreditCardRequest = new NetTahsilatService.SaveCreditCardRequest()
            {
                CardHolderName = createCreditCardParameters.cardHolderName,
                CardNumber = createCreditCardParameters.creditCardNumber,
                Cvv = createCreditCardParameters.cvc,
                Email = createCreditCardParameters.email,
                ExpMonth = createCreditCardParameters.expireMonth.Value,
                ExpYear = createCreditCardParameters.expireYear.Value
            };
            var tokenProcessResult = paymentWebServiceClient.SaveCreditCard(new NetTahsilatService.AuthenticationInfo
            {
                Password = this.netTahsilatConfiguration.nettahsilat_password,
                UserName = this.netTahsilatConfiguration.nettahsilat_username
            }, saveCreditCardRequest, this.netTahsilatConfiguration.nettahsilat_vendorcode, false, false, null);

            if (tokenProcessResult.IsSuccess)
            {
                return new CreateCreditCardResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    cardToken = tokenProcessResult.SaveCreditCardDetail[0].MerchantSafeKey,
                    cardUserKey = tokenProcessResult.SaveCreditCardDetail[0].MerchantSafeKey
                };
            }

            return new CreateCreditCardResponse
            {
                ResponseResult = ResponseResult.ReturnError(tokenProcessResult.ErrorMessage),
                errorCode = tokenProcessResult.ErrorCode,
                errorMessage = tokenProcessResult.ErrorMessage

            };
        }

        public RefundResponse makeRefund(CreateRefundParameters createRefundParameters)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            NetTahsilatService.PaymentWebServiceClient paymentWebServiceClient = new NetTahsilatService.PaymentWebServiceClient(GetBasicHttpsBinding(), GetEndPointAddress(netTahsilatConfiguration.nettahsilat_url));
            var result = paymentWebServiceClient.RefundByReferenceCode(new NetTahsilatService.AuthenticationInfo
            {
                Password = this.netTahsilatConfiguration.nettahsilat_password,
                UserName = this.netTahsilatConfiguration.nettahsilat_username
            },
            createRefundParameters.paymentTransactionId,
            NetTahsilatService.RatePolicy.TransactionRate,
            createRefundParameters.refundAmount,
            true, new NetTahsilatService.InvoicDynamiceData[] { });

            var _errorMessage = string.Empty;
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                _errorMessage = result.ErrorMessage;
            }
            else if (!string.IsNullOrEmpty(result.BankMessage))
            {
                _errorMessage = result.BankMessage;
            }
            else if (!string.IsNullOrEmpty(result.InternalMessage))
            {
                _errorMessage = result.InternalMessage;
            }
            var _errorCode = string.Empty;
            if (!string.IsNullOrEmpty(result.ErrorCode))
            {
                _errorCode = result.ErrorCode;
            }
            else if (!string.IsNullOrEmpty(result.BankErrorCode))
            {
                _errorCode = result.BankErrorCode;
            }
            else if (!string.IsNullOrEmpty(result.InternalErrorCode))
            {
                _errorCode = result.InternalErrorCode;
            }
            return new RefundResponse
            {
                status = result.IsSuccess,
                errorCode = _errorCode,
                errorMessage = _errorMessage
            };
        }
        public PaymentResponse makePayment(CreatePaymentParameters createPaymentParameters)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            NetTahsilatService.PaymentWebServiceClient paymentWebServiceClient = new NetTahsilatService.PaymentWebServiceClient(GetBasicHttpsBinding(), GetEndPointAddress(netTahsilatConfiguration.nettahsilat_url));

            string clientReferenceCode = StaticHelper.RandomDigits(15);
            var saveCreditCard = string.IsNullOrEmpty(createPaymentParameters.creditCardData.cardToken) ? true : false;
            var creditCard = new NetTahsilatService.CreditCard1();
            var virtualPosId = StaticHelper.defaultVirtualPosId;
            if (createPaymentParameters.virtualPosId.HasValue && createPaymentParameters.virtualPosId.Value != 0)
            {
                virtualPosId = createPaymentParameters.virtualPosId.Value;
            }

            //set the credit card info
            if (string.IsNullOrEmpty(createPaymentParameters.creditCardData.cardToken))
            {
                creditCard.SaveCreditCard = true;
                creditCard.CardHolderName = createPaymentParameters.creditCardData.cardHolderName;
                creditCard.CardNumber = createPaymentParameters.creditCardData.creditCardNumber.removeEmptyCharacters();
                creditCard.ExpMonth = createPaymentParameters.creditCardData.expireMonth.Value;
                creditCard.CvcNumber = createPaymentParameters.creditCardData.cvc;

                if (createPaymentParameters.creditCardData.expireYear.ToString().Length == 2)
                {
                    creditCard.ExpYear = Convert.ToInt32("20" + createPaymentParameters.creditCardData.expireYear);
                }
                else
                {
                    creditCard.ExpYear = Convert.ToInt32(createPaymentParameters.creditCardData.expireYear);
                }

                creditCard.MerchantSafeKey = saveCreditCard ? null : createPaymentParameters.creditCardData.cardToken;
            }
            else
            {
                creditCard.SaveCreditCard = false;
                creditCard.CardHolderName = string.Empty;
                creditCard.CardNumber = string.Empty;
                creditCard.ExpMonth = 0;
                creditCard.CvcNumber = string.Empty;//createPaymentParameters.creditCardData.cvc.Value.ToString();
                creditCard.ExpYear = 0;
                creditCard.MerchantSafeKey = createPaymentParameters.creditCardData.cardToken;
            }
            var result = paymentWebServiceClient.ProcessPayment(
            new NetTahsilatService.AuthenticationInfo
            {
                Password = this.netTahsilatConfiguration.nettahsilat_password,
                UserName = this.netTahsilatConfiguration.nettahsilat_username
            },
            new NetTahsilatService.ProcessSaleParameters
            {
                Amount = createPaymentParameters.paidAmount,
                //CustomData = customDynamicData,
                Use3d = createPaymentParameters.use3DSecure,
                UseSafeKey = !saveCreditCard,
                Installment = createPaymentParameters.installment == 0 ? 1 : createPaymentParameters.installment,
                ReturnUrl = createPaymentParameters.use3DSecure ? createPaymentParameters.callBackUrl : string.Empty,
                OrderReferenceType = NetTahsilatService.OrderReferenceType.Auto,
                ClientReferenceCode = clientReferenceCode,
                TransactionType = NetTahsilatService.TransactionType.Sale,
                VendorId = createPaymentParameters.vendorId,
                VirtualPosId = virtualPosId,
                CreditCard = creditCard,
            });

            return new PaymentResponse
            {
                clientReferenceCode = clientReferenceCode,
                paymentTransactionId = result.ReferenceCode,
                paymentId = Convert.ToString(result.TransactionId),
                status = result.IsSuccess,
                errorCode = result.BankErrorCode,
                errorMessage = string.IsNullOrEmpty(result.BankMessage) ? result.InternalMessage : result.BankMessage,
                cardOrganizationType = result.CardBin?.CardOrgranizationType,
                cardGroup = result.CardBin?.Group,
                cardProgram = result.CardBin?.CardProgram,
                cardType = result.CardBin?.CardType,
                cardBin = result.CardBin?.Bin,
                creditCardSaveSafely = result.TokenResult?.IsSuccess,
                merchantSafeKey = result.TokenResult?.TokenString,
                htmlContent = result.ThreeDUrl
            };
        }
        public InstallmentResponse retrieveInstallmentforGivenCardBin(string cardBin, decimal amount)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            NetTahsilatService.PaymentWebServiceClient paymentWebServiceClient = new NetTahsilatService.PaymentWebServiceClient(GetBasicHttpsBinding(), GetEndPointAddress(netTahsilatConfiguration.nettahsilat_url));
            var result = paymentWebServiceClient.GetPaymentSetListBinNumber(new NetTahsilatService.AuthenticationInfo
            {
                Password = this.netTahsilatConfiguration.nettahsilat_password,
                UserName = this.netTahsilatConfiguration.nettahsilat_username
            },
            cardBin);

            if (result.IsSuccess)
            {
                var paymentSet = result.PaymentSets.Where(p => p.PaymentSetIsActive == true && p.PaymentSetIsDefault == true).DistinctBy(p => p.PaymentSetId).ToList();
                if (paymentSet.Count == 0)
                {
                    return new InstallmentResponse
                    {
                        //todo this is a key and will replace with rich text from calling 
                        ResponseResult = ResponseResult.ReturnError("nettahsilatpaymentsetnotfound")
                    };
                }
                ////checking virtualPos 
                //if (paymentSet.VirtualPosList.Length == 0)
                //{
                //    return new InstallmentResponse
                //    {
                //        //todo
                //        ResponseResult = ResponseResult.ReturnError("nettahsilatvirtualposnotfound")
                //    };
                //}
                List<List<InstallmentData>> installmentDatas = new List<List<InstallmentData>>();
                foreach (var paymentSetItem in paymentSet)
                {
                    foreach (var item in paymentSetItem.VirtualPosList)
                    {
                        List<InstallmentData> _internalData = new List<InstallmentData>();
                        foreach (var commRates in item.CommRates)
                        {
                            decimal _calcAmount = decimal.Zero;

                            if (commRates.Instalment != 1)
                            {
                                if (paymentSetItem.PaymentSetComApplyTypeId == 10)
                                {
                                    _calcAmount = ((100 + commRates.CommRateIn) * amount) / 100;
                                }
                                else if (paymentSetItem.PaymentSetComApplyTypeId == 20)
                                {
                                    _calcAmount = ((100 + commRates.CommRateOut) * amount) / 100;
                                }
                            }

                            InstallmentData installmentData = new InstallmentData
                            {
                                bankName = item.Name,
                                virtualPosId = item.VPosId,
                                installmentAmount = commRates.Instalment == 1 ? amount : decimal.Round(_calcAmount / commRates.Instalment, 2, MidpointRounding.AwayFromZero),
                                totalAmount = commRates.Instalment == 1 ? amount : decimal.Round(_calcAmount, 2, MidpointRounding.AwayFromZero),
                                installmentNumber = commRates.Instalment
                            };
                            _internalData.Add(installmentData);
                        }
                        if (_internalData.Count > 0)
                            installmentDatas.Add(_internalData);
                    }
                }

                return new InstallmentResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    installmentData = installmentDatas
                };
            }


            return new InstallmentResponse
            {
                ResponseResult = ResponseResult.ReturnError(result.ErrorMessage)
            };
        }
        public VirtualPosResponse retrieveVPosIdforGivenCard(RetrieveVirtualPosIdParameters retrieveVirtualPosIdParameters)
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            NetTahsilatService.PaymentWebServiceClient paymentWebServiceClient = new NetTahsilatService.PaymentWebServiceClient(GetBasicHttpsBinding(), GetEndPointAddress(netTahsilatConfiguration.nettahsilat_url));
            var result = paymentWebServiceClient.GetPaymentSetListBinNumber(new NetTahsilatService.AuthenticationInfo
            {
                Password = this.netTahsilatConfiguration.nettahsilat_password,
                UserName = this.netTahsilatConfiguration.nettahsilat_username
            },
            retrieveVirtualPosIdParameters.cardBin);

            if (result.IsSuccess)
            {
                var paymentSet = result.PaymentSets.Where(p => p.PaymentSetIsActive == true).DistinctBy(p => p.PaymentSetId).ToList();
                if (paymentSet.Count == 0)
                {
                    return new VirtualPosResponse
                    {
                        //todo this is a key and will replace with rich text from calling 
                        ResponseResult = ResponseResult.ReturnError("virtualposid doesnt exists")
                    };
                }

                return new VirtualPosResponse
                {
                    virtualPosId = paymentSet.FirstOrDefault().VirtualPosList.FirstOrDefault() != null ?
                                   paymentSet.FirstOrDefault().VirtualPosList.FirstOrDefault().VPosId :
                                   StaticHelper.defaultVirtualPosId,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }

            return new VirtualPosResponse
            {
                ResponseResult = ResponseResult.ReturnError(result.ErrorMessage)
            };
        }

        public static NetTahsilatConfiguration setConfigurationValues(string unformattedConfigurationValues)
        {
            var splitted = unformattedConfigurationValues.Split(';');
            return new NetTahsilatConfiguration
            {
                nettahsilat_username = splitted[0],
                nettahsilat_password = splitted[1],
                nettahsilat_url = splitted[2],
                nettahsilat_vendorcode = splitted[3],
                nettahsilat_vendorid = Convert.ToInt32(splitted[4])
            };
        }

        private static BasicHttpBinding GetBasicHttpBinding()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Name = "netTahsilatBinding";
            binding.Security.Mode = BasicHttpSecurityMode.None;

            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.MaxReceivedMessageSize = 2147483647;
            return binding;
        }

        private static BasicHttpsBinding GetBasicHttpsBinding()
        {
            BasicHttpsBinding binding = new BasicHttpsBinding();
            binding.Name = "netTahsilatBinding";
            binding.Security.Mode = BasicHttpsSecurityMode.Transport;

            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.MaxReceivedMessageSize = 2147483647;
            return binding;
        }

        private static EndpointAddress GetEndPointAddress(string url)
        {
            //todo will read from config
            var myEndpointAddress = new EndpointAddress(url);
            return myEndpointAddress;
        }
    }
}
