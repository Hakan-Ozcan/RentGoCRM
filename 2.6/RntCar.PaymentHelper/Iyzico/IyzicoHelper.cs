using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Model.V2;
using Iyzipay.Model.V2.Iyzilink;
using Iyzipay.Model.V2.Transaction;
using Iyzipay.Request;
using Iyzipay.Request.V2;
using RestSharp;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary.Payment;
using RntCar.PaymentHelper.iyzico.Model;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using static RntCar.ClassLibrary.PaymentEnums;

namespace RntCar.PaymentHelper.iyzico
{
    public class IyzicoHelper
    {
        public IyzicoConfiguration iyzicoConfiguration { get; set; }

        public IyzicoHelper(IyzicoConfiguration _iyzicoConfiguration)
        {
            iyzicoConfiguration = _iyzicoConfiguration;
        }

        public CreateCreditCardResponse createCreditCard(CreateCreditCardParameters createCreditCardParameters)
        {

            Options options = this.prepareOptions();

            CreateCardRequest request = new CreateCardRequest();
            if (createCreditCardParameters.langId == (int)GlobalEnums.LangId.English)
            {
                request.Locale = Locale.EN.ToString();

            }
            else if (createCreditCardParameters.langId == (int)GlobalEnums.LangId.Turkish)
            {
                request.Locale = Locale.TR.ToString();
            }

            request.ConversationId = StaticHelper.RandomDigits(10);
            request.Email = string.IsNullOrEmpty(createCreditCardParameters.email) ? StaticHelper.defaultEmail : createCreditCardParameters.email;
            request.ExternalId = createCreditCardParameters.customerExternalId;
            //request.CardUserKey = "439758934758973458734";

            CardInformation cardInformation = new CardInformation();
            cardInformation.CardAlias = createCreditCardParameters.cardAlias;
            cardInformation.CardHolderName = createCreditCardParameters.cardHolderName;
            cardInformation.CardNumber = createCreditCardParameters.creditCardNumber;
            cardInformation.ExpireMonth = Convert.ToString(createCreditCardParameters.expireMonth);
            cardInformation.ExpireYear = Convert.ToString(createCreditCardParameters.expireYear);
            request.Card = cardInformation;

            Card card = Card.Create(request, options);

            if (card.Status == "success")
            {
                return new CreateCreditCardResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    cardToken = card.CardToken,
                    cardUserKey = card.CardUserKey,
                    conversationId = request.ConversationId,
                    externalId = request.ExternalId,
                    binNumber = card.BinNumber,
                    bankName = card.CardBankName,
                    bankCode = card.CardBankCode.HasValue ? (long?)card.CardBankCode.Value : null,
                    cardType = card.CardType,
                    cardFamily = card.CardFamily,
                    cardAssociation = card.CardAssociation
                };
            }

            return new CreateCreditCardResponse
            {
                ResponseResult = ResponseResult.ReturnError(card.ErrorMessage),
                errorCode = card.ErrorCode,
                errorGroup = card.ErrorGroup,
                errorMessage = card.ErrorMessage

            };

        }

        public BinResponse retrieveBinRequest(string binNumber, string conversationId)
        {
            Options options = this.prepareOptions();
            RetrieveBinNumberRequest retrieveBinNumberRequest = new RetrieveBinNumberRequest
            {
                BinNumber = binNumber,
                ConversationId = conversationId
            };
            var card = BinNumber.Retrieve(retrieveBinNumberRequest, options);

            return new BinResponse
            {
                conversationId = conversationId,
                binNumber = card.Bin,
                bankName = card.BankName,
                bankCode = card.BankCode,
                cardType = card.CardType,
                cardFamily = card.CardFamily,
                cardAssociation = card.CardAssociation,
                status = card.Status,
                errorCode = card.ErrorCode,
                errorMessage = card.ErrorMessage,
                errorGroup = card.ErrorGroup,
            };
        }
        public PaymentResponse makePayment(CreatePaymentParameters createPaymentParameters)
        {
            Options options = this.prepareOptions();
            PaymentResponse paymentResponse = new PaymentResponse();
            CreatePaymentRequest request = new CreatePaymentRequest();
            //iyzico default is turkish
            if (createPaymentParameters.langId == (int)GlobalEnums.LangId.English)
            {
                request.Locale = Locale.EN.ToString();
            }
            else if (createPaymentParameters.langId == (int)GlobalEnums.LangId.English)
            {
                request.Locale = Locale.TR.ToString();
            }
            request.ConversationId = createPaymentParameters.conversationId;
            request.Price = Convert.ToString(createPaymentParameters.paidAmount);
            request.PaidPrice = Convert.ToString(createPaymentParameters.paidAmount);
            //multiple currencies in phase 2
            request.Currency = Currency.TRY.ToString();
            request.Installment = createPaymentParameters.installment;
            request.BasketId = "B" + StaticHelper.RandomDigits(10);
            //map input payment channel to iyzico payment channel
            request.PaymentChannel = decidePaymentChannel((int)createPaymentParameters.paymentChannelCode).ToString();
            request.PaymentGroup = PaymentGroup.PRODUCT.ToString();
            //card info

            PaymentCard paymentCard = new PaymentCard();

            paymentCard.CardUserKey = createPaymentParameters.creditCardData.cardUserKey;
            paymentCard.CardToken = createPaymentParameters.creditCardData.cardToken;
            paymentCard.RegisterCard = 0;
            request.PaymentCard = paymentCard;

            //Buyer Info
            Buyer buyer = new Buyer();
            buyer.Id = "BY" + StaticHelper.RandomDigits(10);
            var _firstName = string.Empty;
            var _lastName = string.Empty;
            var _uniqueId = string.Empty;
            if (createPaymentParameters.invoiceAddressData.invoiceType == (int)ClassLibrary._Enums_1033.rnt_invoiceaddress_rnt_invoicetypecode.Individual)
            {
                _firstName = createPaymentParameters.invoiceAddressData.firstName;
                _lastName = createPaymentParameters.invoiceAddressData.lastName;
                _uniqueId = createPaymentParameters.invoiceAddressData.governmentId;
            }
            else if (createPaymentParameters.invoiceAddressData.invoiceType == (int)ClassLibrary._Enums_1033.rnt_invoiceaddress_rnt_invoicetypecode.Corporate)
            {
                _firstName = createPaymentParameters.invoiceAddressData.companyName;
                _lastName = createPaymentParameters.invoiceAddressData.companyName;
                _uniqueId = createPaymentParameters.invoiceAddressData.taxNumber;
            }
            buyer.Name = _firstName;
            buyer.Surname = _lastName;
            buyer.IdentityNumber = _uniqueId;
            buyer.GsmNumber = createPaymentParameters.invoiceAddressData.mobilePhone;
            buyer.Email = string.IsNullOrEmpty(createPaymentParameters.invoiceAddressData.email) ? "pay@rentgo.com" : createPaymentParameters.invoiceAddressData.email;

            if (string.IsNullOrEmpty(createPaymentParameters.invoiceAddressData.addressDetail))
            {
                createPaymentParameters.invoiceAddressData.addressDetail = "detay";
            }
            buyer.RegistrationAddress = createPaymentParameters.invoiceAddressData.addressDetail;
            buyer.City = string.IsNullOrEmpty(createPaymentParameters.invoiceAddressData.addressCityName) ? "rentgo" : createPaymentParameters.invoiceAddressData.addressCityName;
            buyer.Country = createPaymentParameters.invoiceAddressData.addressCountryName;
            request.Buyer = buyer;

            Address shippingAddress = new Address();
            shippingAddress.ContactName = _firstName + " " + _lastName;
            shippingAddress.City = createPaymentParameters.invoiceAddressData.addressCityName;
            shippingAddress.Country = createPaymentParameters.invoiceAddressData.addressCountryName;
            shippingAddress.Description = string.IsNullOrEmpty(createPaymentParameters.invoiceAddressData.addressDetail) ?
                                          "dummy_description" :
                                          createPaymentParameters.invoiceAddressData.addressDetail;
            shippingAddress.ZipCode = "unknown";
            request.ShippingAddress = shippingAddress;

            Address billingAddress = new Address();
            billingAddress.ContactName = _firstName + " " + _lastName;
            billingAddress.City = createPaymentParameters.invoiceAddressData.addressCityName;
            billingAddress.Country = createPaymentParameters.invoiceAddressData.addressCountryName;
            billingAddress.Description = string.IsNullOrEmpty(createPaymentParameters.invoiceAddressData.addressDetail) ?
                                          "dummy_description" :
                                          createPaymentParameters.invoiceAddressData.addressDetail; ;
            billingAddress.ZipCode = "unknown";
            request.BillingAddress = billingAddress;

            List<BasketItem> basketItems = new List<BasketItem>();
            BasketItem firstBasketItem = new BasketItem();
            firstBasketItem.Id = "BI" + StaticHelper.RandomDigits(10);
            firstBasketItem.Name = "rntacar_fee";

            if (createPaymentParameters.reservationId.HasValue)
            {
                firstBasketItem.Category1 = "reservation";
            }
            else if (createPaymentParameters.contractId.HasValue)
            {
                firstBasketItem.Category1 = "contract";
            }
            else if (createPaymentParameters.reservationId.HasValue && createPaymentParameters.contractId.HasValue)
            {
                firstBasketItem.Category1 = "reservation";
                firstBasketItem.Category2 = "contract";
            }

            firstBasketItem.ItemType = BasketItemType.PHYSICAL.ToString();
            firstBasketItem.Price = Convert.ToString(createPaymentParameters.paidAmount);
            basketItems.Add(firstBasketItem);
            request.BasketItems = new List<BasketItem>();
            request.BasketItems.AddRange(basketItems);
            paymentResponse.use3DSecure = createPaymentParameters.use3DSecure;
            if (createPaymentParameters.use3DSecure)
            {
                request.CallbackUrl = createPaymentParameters.callBackUrl;
                ThreedsInitialize threedsInitialize = ThreedsInitialize.Create(request, options);
                //paymentResponse.paymentId = threedsInitialize.ConversationId;
                /*paymentResponse.paymentTransactionId = threedsInitialize.Status == "success" ?
                                      threedsInitialize.PaymentTransactionId :
                                      string.Empty;*/
                paymentResponse.status = threedsInitialize.Status == "success" ? true : false;
                /*paymentResponse.providerComission = threedsInitialize.Status == "success" ?
                                    (/*Convert.ToDecimal(payment.PaymentItems.FirstOrDefault().IyziCommissionFee) +
                                     Convert.ToDecimal(payment.PaymentItems.FirstOrDefault().IyziCommissionRateAmount)) :
                                    decimal.MinValue;*/
                //registerCard = paymentCard.RegisterCard.Value,
                paymentResponse.errorCode = threedsInitialize.ErrorCode;
                paymentResponse.errorMessage = threedsInitialize.ErrorMessage;
                paymentResponse.errorGroup = threedsInitialize.ErrorGroup;
                paymentResponse.htmlContent = threedsInitialize.HtmlContent;
            }
            else
            {
                Payment payment = Payment.Create(request, options);
                paymentResponse.paymentId = payment.PaymentId;
                paymentResponse.paymentTransactionId = payment.Status == "success" ?
                                      payment.PaymentItems.FirstOrDefault().PaymentTransactionId :
                                      string.Empty;
                paymentResponse.status = payment.Status == "success" ? true : false;
                paymentResponse.providerComission = payment.Status == "success" ?
                                    (/*Convert.ToDecimal(payment.PaymentItems.FirstOrDefault().IyziCommissionFee) +*/
                                     Convert.ToDecimal(payment.PaymentItems.FirstOrDefault().IyziCommissionRateAmount)) :
                                    decimal.MinValue;
                //registerCard = paymentCard.RegisterCard.Value,
                paymentResponse.errorCode = payment.ErrorCode;
                paymentResponse.errorMessage = payment.ErrorMessage;
                paymentResponse.errorGroup = payment.ErrorGroup;
            }
            return paymentResponse;

        }

        public RefundResponse makeRefund(CreateRefundParameters createRefundParameters)
        {
            Options options = this.prepareOptions();

            CreateRefundRequest request = new CreateRefundRequest();
            request.ConversationId = createRefundParameters.conversationId;
            if (createRefundParameters.langId == (int)GlobalEnums.LangId.English)
                request.Locale = Locale.EN.ToString();
            else if (createRefundParameters.langId == (int)GlobalEnums.LangId.Turkish)
                request.Locale = Locale.TR.ToString();

            request.PaymentTransactionId = createRefundParameters.paymentTransactionId;
            request.Price = createRefundParameters.refundAmount.ToString().Replace(",", ".");
            request.Currency = Currency.TRY.ToString();

            var res = Refund.Create(request, options);

            return new RefundResponse
            {
                status = res.Status == "success" ? true : false,
                errorCode = res.ErrorCode,
                errorMessage = res.ErrorMessage,
                errorGroup = res.ErrorGroup
            };

        }

        public InstallmentResponse retrieveInstallmentforGivenCardBin(string cardBin, decimal amount)
        {
            Options options = this.prepareOptions();
            CultureInfo culture = CultureInfo.InvariantCulture;
            RetrieveInstallmentInfoRequest request = new RetrieveInstallmentInfoRequest();
            request.Locale = Locale.TR.ToString();
            request.BinNumber = cardBin;
            request.Price = Convert.ToDecimal(amount, culture).ToString(CultureInfo.InvariantCulture);

            InstallmentInfo installmentInfo = InstallmentInfo.Retrieve(request, options);

            List<InstallmentData> installmentDatas = new List<InstallmentData>();

            if (installmentInfo.Status == PaymentEnums.IyzicoResponse.success.ToString())
            {
                var prices = installmentInfo.InstallmentDetails.FirstOrDefault().InstallmentPrices;

                foreach (var item in prices)
                {
                    InstallmentData installmentData = new InstallmentData();
                    installmentData.installmentAmount = Convert.ToDecimal(item.Price, culture);
                    installmentData.installmentNumber = item.InstallmentNumber.Value;
                    installmentData.totalAmount = Convert.ToDecimal(item.TotalPrice, culture);
                    installmentDatas.Add(installmentData);
                }
                List<List<InstallmentData>> general = new List<List<InstallmentData>>();
                general.Add(installmentDatas);
                return new InstallmentResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess(),
                    installmentData = general
                };
            }

            return new InstallmentResponse
            {
                ResponseResult = ResponseResult.ReturnError(installmentInfo.ErrorMessage)
            };
        }

        public static IyzicoConfiguration setConfigurationValues(string unformattedConfigurationValues)
        {
            var splitted = unformattedConfigurationValues.Split(';');
            return new IyzicoConfiguration
            {
                iyzico_baseUrl = splitted[0],
                iyzico_apiKey = splitted[1],
                iyzico_secretKey = splitted[2]
            };
        }

        public FastLinkResponse CreateFastLink(FastLinkRequest fastLinkRequest)
        {
            Options options = this.prepareOptions();


            IyziLinkSaveRequest request = new IyziLinkSaveRequest();
            request.Locale = Locale.TR.ToString();
            request.ConversationId = fastLinkRequest.ConversationId;
            request.Name = fastLinkRequest.Name;
            request.Description = fastLinkRequest.Description;
            request.Base64EncodedImage = fastLinkRequest.Base64EncodedImage;
            request.Price = fastLinkRequest.Price;
            request.Currency = Currency.TRY.ToString();
            request.AddressIgnorable = false;
            request.SoldLimit = 1;
            request.InstallmentRequested = false;

            ResponseData<IyziLinkSave> response = IyziLink.Create(request, options);

            FastLinkResponse fastLinkResponse = new FastLinkResponse();
            fastLinkResponse.status = response.Status;
            fastLinkResponse.statusCode = response.StatusCode;
            fastLinkResponse.errorMessage = response.ErrorMessage;
            fastLinkResponse.url = response.Data != null ? response.Data.Url : string.Empty;

            return fastLinkResponse;
        }

        private Options prepareOptions()
        {
            return new Options
            {
                ApiKey = this.iyzicoConfiguration.iyzico_apiKey,
                SecretKey = this.iyzicoConfiguration.iyzico_secretKey,
                BaseUrl = this.iyzicoConfiguration.iyzico_baseUrl
            };
        }

        private PaymentChannel decidePaymentChannel(int _paymentChannel)
        {
            PaymentChannel paymentChannel = PaymentChannel.WEB;
            if (_paymentChannel == (int)PaymentEnums.PaymentChannelCode.WEB)
            {
                paymentChannel = PaymentChannel.WEB;
            }
            else if (_paymentChannel == (int)PaymentEnums.PaymentChannelCode.BRANCH)
            {
                paymentChannel = PaymentChannel.WEB;
            }
            else if (_paymentChannel == (int)PaymentEnums.PaymentChannelCode.MOBILE)
            {
                paymentChannel = PaymentChannel.MOBILE;
            }/*
            else if (_paymentChannel == (int)PaymentEnums.PaymentChannelCode.MOBILE_IOS)
            {
                paymentChannel = PaymentChannel.MOBILE_IOS;
            }*/

            else if (_paymentChannel == (int)PaymentEnums.PaymentChannelCode.TABLET)
            {
                paymentChannel = PaymentChannel.MOBILE_TABLET;
            }

            return paymentChannel;
        }

        public List<TransactionReportItem> GetTransactionListByDate(DateTime transactionDate)
        {
            Options options = this.prepareOptions();
            List<TransactionReportItem> transactionReportItemList = new List<TransactionReportItem>();
            int pageCount = 1;
            while (true)
            {
                RetrieveTransactionReportRequest request = new RetrieveTransactionReportRequest()
                {
                    ConversationId = "",
                    TransactionDate = transactionDate.ToString("yyyy-MM-dd"),
                    Page = pageCount
                };

                TransactionReport tempTransactionReport = TransactionReport.Retrieve(request, options);
                if (tempTransactionReport.StatusCode == 200 && tempTransactionReport.Transactions != null)
                {
                    transactionReportItemList.AddRange(tempTransactionReport.Transactions);
                    if (tempTransactionReport.TotalPageCount == null || tempTransactionReport.TotalPageCount == pageCount)
                    {
                        break;
                    }
                    pageCount++;
                }
                else
                {
                    break;
                }

            }
            return transactionReportItemList;
        }

        public Reservation3dPaymentReturnResponse Payment3DReturn(Iyzico3DReturnParameter iyzico3DReturnParameter)
        {
            try
            {
                CreateThreedsPaymentRequest request = new CreateThreedsPaymentRequest()
                {
                    ConversationData = iyzico3DReturnParameter.ConversationData,
                    ConversationId = iyzico3DReturnParameter.ConversationId,
                    Locale = Locale.TR.ToString(),
                    PaymentId = iyzico3DReturnParameter.PaymentId
                };
                ThreedsPayment threedsPayment = ThreedsPayment.Create(request, prepareOptions());
                Reservation3dPaymentReturnResponse reservation3DPaymentReturn = new Reservation3dPaymentReturnResponse()
                {
                    clientReferenceCode = iyzico3DReturnParameter.ConversationId,
                    paymentId = threedsPayment.PaymentId,
                    paymentTransactionId = threedsPayment.Status == "success" ?
                                      threedsPayment.PaymentItems.FirstOrDefault().PaymentTransactionId :
                                      string.Empty,
                    providerComission = threedsPayment.Status == "success" ?
                                    (/*Convert.ToDecimal(payment.PaymentItems.FirstOrDefault().IyziCommissionFee) +*/
                                     Convert.ToDecimal(threedsPayment.PaymentItems.FirstOrDefault().IyziCommissionRateAmount)) :
                                    decimal.MinValue,
                    errorCode = threedsPayment.ErrorCode,
                    errorGroup = threedsPayment.ErrorGroup,
                    errorMessage = threedsPayment.ErrorMessage,
                    status = threedsPayment.Status == IyzicoResponse.success.ToString() ? true : false

                };
                return reservation3DPaymentReturn;
            }
            catch (Exception)
            {
                return new Reservation3dPaymentReturnResponse() { status = false };
            }
        }
    }
}

