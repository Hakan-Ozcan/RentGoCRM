using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;

namespace RntCar.BusinessLibrary.Business
{
    public class ManualPaymentBL : BusinessHandler
    {
        public ManualPaymentBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public ManualPaymentBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service, string UserId = "") : base(enums, UserId)
        {
        }

        public ManualPaymentBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public ManualPaymentBL(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public ManualPaymentBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public ManualPaymentBL(IOrganizationService orgService, Guid userId, string orgName) : base(orgService, userId, orgName)
        {
        }

        public CreditCardData FillNewCreditCardData(ManualPaymentParameters parameters)
        {
            //CreditCardData creditCardData = new CreditCardData();
            return new CreditCardData
            {
                cardHolderName = parameters.nameOnCard,
                creditCardNumber = parameters.creditCardNumber,
                cvc = parameters.cvc,
                expireMonth = parameters.month,
                expireYear = parameters.year
            };
        }
        public CreditCardData FillExistingCreditCardData(ManualPaymentParameters parameters)
        {
            CreditCardRepository creditCardRepository = new CreditCardRepository(this.OrgService);
            var card = creditCardRepository.getCreditCardByIdWithGivenColumns(new Guid(parameters.creditCardId.ToString()), new string[] { "rnt_cardtoken", "rnt_carduserkey", "rnt_binnumber" });
            return new CreditCardData
            {
                cardToken = card.GetAttributeValue<string>("rnt_cardtoken"),
                cardUserKey = card.GetAttributeValue<string>("rnt_carduserkey"),
                binNumber = card.GetAttributeValue<string>("rnt_binnumber"),
                creditCardId = parameters.creditCardId
            };
        }
        public void CreateReservationItem(Guid reservationId, decimal amount, Guid additionalProductId)
        {
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var reservation = reservationRepository.getReservationById(reservationId);
            this.Trace($@"reservation retrieved.");
            var currencyId = reservation.GetAttributeValue<EntityReference>("transactioncurrencyid").Id;
            int itemTypeCode = (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Fine;
            int paymentType = 100;

            ReservationCustomerParameters reservationCustomerParameters = new ReservationCustomerParameters();
            if (reservation.Attributes.Contains("rnt_customerid"))
            {
                reservationCustomerParameters.contactId = ((EntityReference)reservation.Attributes["rnt_customerid"]).Id;
            }
            else if (reservation.Attributes.Contains("rnt_customerid") && reservation.Attributes.Contains("rnt_corporateid"))
            {
                reservationCustomerParameters.contactId = ((EntityReference)reservation.Attributes["rnt_customerid"]).Id;
                reservationCustomerParameters.corporateCustomerId = ((EntityReference)reservation.Attributes["rnt_corporateid"]).Id;
            }
            else
            {
                reservationCustomerParameters.corporateCustomerId = ((EntityReference)reservation.Attributes["rnt_corporateid"]).Id;
            }

            this.Trace($@"reservation ReservationCustomerParameters filled.");

            ReservationDateandBranchParameters reservationDateandBranchParameter = new ReservationDateandBranchParameters
            {
                pickupBranchId = ((EntityReference)reservation.Attributes["rnt_pickupbranchid"]).Id,
                pickupDate = reservation.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                dropoffDate = reservation.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                dropoffBranchId = reservation.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id
            };
            this.Trace($@"reservation ReservationDateandBranchParameters filled.");
            ReservationEquipmentParameters reservationEquipmentParameter = new ReservationEquipmentParameters
            {
                groupCodeInformationId = ((EntityReference)reservation.Attributes["rnt_groupcodeid"]).Id
            };

            ReservationPriceParameters reservationPriceParameters = new ReservationPriceParameters { };
            this.Trace($@"reservation ReservationEquipmentParameters filled.");
            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var additionalProduct = additionalProductRepository.getAdditionalProductById(additionalProductId, new string[] { "rnt_name" ,
                                                                                                                                             "rnt_pricecalculationtypecode" });
            this.Trace($@"reservation additionalProduct retrieved.");

            ReservationAdditionalProductParameters reservationItemAdditionalProductParameter = new ReservationAdditionalProductParameters
            {
                productName = additionalProduct.GetAttributeValue<string>("rnt_name"),
                productId = additionalProductId,
                actualAmount = amount,
                priceCalculationType = additionalProduct.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value
            };
            this.Trace($@"reservation ReservationAdditionalProductParameters List filled.");
            var totalDuration = Convert.ToInt32(CommonHelper.calculateTotalDurationInDays(reservationDateandBranchParameter.pickupDate.converttoTimeStamp(), reservationDateandBranchParameter.dropoffDate.converttoTimeStamp()));

            ReservationItemBL reservationItemBL = new ReservationItemBL(this.OrgService);
            this.Trace($@"createReservationItemForManualPayment method called.");

            reservationItemBL.createReservationItemForManualPayment(reservationCustomerParameters, reservationDateandBranchParameter, reservationEquipmentParameter,
                                                              reservationItemAdditionalProductParameter, reservationId, currencyId,
                                                               totalDuration,
                                                              paymentType,
                                                              itemTypeCode);
        }
        public Guid CreateContractItem(Guid contractId, decimal amount, Guid additionalProductId, int? channelCode)
        {
            if (!channelCode.HasValue)
            {
                this.Trace("!channelCode.HasValue");

                channelCode = (int)rnt_ReservationChannel.Branch;
            }
            ContractRepository contractRepository = new ContractRepository(this.OrgService);
            var contract = contractRepository.getContractById(contractId);
            var contactId = contract.GetAttributeValue<EntityReference>("rnt_customerid").Id;
            var currencyId = contract.GetAttributeValue<EntityReference>("transactioncurrencyid").Id;
            var contractStatusCode = contract.GetAttributeValue<OptionSetValue>("statuscode").Value;
            int itemTypeCode = (int)ClassLibrary._Enums_1033.rnt_contractitem_rnt_itemtypecode.Fine;
            var groupCodeInformationId = contract.GetAttributeValue<EntityReference>("rnt_groupcodeid").Id;

            this.Trace("before mapping contractStatusCode : " + contractStatusCode);
            this.Trace("after mapping contractStatusCode : " + ContractMapper.getContractItemStatusCodeByContractStatusCode(contractStatusCode));
            var contractItemStatusCode = ContractMapper.getContractItemStatusCodeByContractStatusCode(contractStatusCode);
            ContractDateandBranchParameters contractDateandBranchParameter = new ContractDateandBranchParameters
            {
                pickupBranchId = ((EntityReference)contract.Attributes["rnt_pickupbranchid"]).Id,
                pickupDate = contract.GetAttributeValue<DateTime>("rnt_pickupdatetime"),
                dropoffDate = contract.GetAttributeValue<DateTime>("rnt_dropoffdatetime"),
                dropoffBranchId = contract.GetAttributeValue<EntityReference>("rnt_dropoffbranchid").Id
            };
            this.Trace("additional product retrieve start");

            AdditionalProductRepository additionalProductRepository = new AdditionalProductRepository(this.OrgService);
            var additionalProduct = additionalProductRepository.getAdditionalProductById(additionalProductId, new string[] { "rnt_name" ,
                                                                                                                                             "rnt_pricecalculationtypecode" });
            this.Trace("additional product retrieve end");

            ContractAdditionalProductParameters contractItemAdditionalProductParameter = new ContractAdditionalProductParameters
            {
                productName = additionalProduct.GetAttributeValue<string>("rnt_name"),
                productId = additionalProductId,
                actualAmount = amount,
                priceCalculationType = additionalProduct.GetAttributeValue<OptionSetValue>("rnt_pricecalculationtypecode").Value
            };
            ContractHelper contractHelper = new ContractHelper(this.OrgService);
            var totalDuration = contractHelper.calculateTotalDuration_ContractByPriceHourEffect(contractDateandBranchParameter.pickupDate,
                                                                                                contractDateandBranchParameter.dropoffDate);

            //InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);
            //var invoice = invoiceRepository.getFirstInvoiceByContractId(contractId);

            this.Trace("start createContractItemForManualPayment");
            ContractItemBL contractItemBL = new ContractItemBL(this.OrgService);
            var item = contractItemBL.createContractItemForAdditionalProduct(contractDateandBranchParameter,
                                                                          contractItemAdditionalProductParameter,
                                                                          contactId,
                                                                          groupCodeInformationId,
                                                                          contractId,
                                                                          currencyId,
                                                                          Guid.Empty,
                                                                          totalDuration,
                                                                          contractItemStatusCode,
                                                                          string.Empty,
                                                                          channelCode.Value,
                                                                          Guid.Empty,
                                                                          Guid.Empty,
                                                                          itemTypeCode, false);
            this.Trace("end createContractItemForManualPayment");
            return item;

        }
        public CreatePaymentParameters CreateReservationPaymentParameters(ManualPaymentParameters parameters, CreditCardData creditCardData)
        {
            Guid reservationId = new Guid(parameters.reservationId.ToString());
            ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
            var reservation = reservationRepository.getReservationById(reservationId, new string[] { "transactioncurrencyid", "rnt_customerid", "rnt_pnrnumber" });

            return new CreatePaymentParameters
            {
                reservationId = reservationId,
                transactionCurrencyId = ((EntityReference)reservation.Attributes["transactioncurrencyid"]).Id,
                individualCustomerId = ((EntityReference)reservation.Attributes["rnt_customerid"]).Id,
                conversationId = reservation.Attributes["rnt_pnrnumber"].ToString(),
                langId = parameters.langId,
                paymentTransactionType = (int)PaymentEnums.PaymentTransactionType.SALE,
                creditCardData = creditCardData,
                installment = 1,
                paidAmount = parameters.amount.Value,
                //todo will read from input
                paymentChannelCode = rnt_PaymentChannelCode.BRANCH,
                invoiceAddressData = new InvoiceAddressData { },
            };
        }
        public CreatePaymentParameters CreateContractPaymentParameters(ManualPaymentParameters parameters, CreditCardData creditCardData, Entity contract)
        {
            Guid contractId = new Guid(parameters.contractId.ToString());

            return new CreatePaymentParameters
            {
                contractId = contractId,
                transactionCurrencyId = ((EntityReference)contract.Attributes["transactioncurrencyid"]).Id,
                individualCustomerId = ((EntityReference)contract.Attributes["rnt_customerid"]).Id,
                conversationId = contract.Attributes["rnt_pnrnumber"].ToString(),
                langId = parameters.langId,
                paymentTransactionType = (int)PaymentEnums.PaymentTransactionType.SALE,
                creditCardData = creditCardData,
                installment = 1,
                paidAmount = parameters.amount.Value,
                paymentChannelCode = rnt_PaymentChannelCode.BRANCH,
                invoiceAddressData = new InvoiceAddressData { },
            };
        }

        public void MakeRefundForReservation(CreatePaymentParameters paymentParameters)
        {
            PaymentBL paymentBl = new PaymentBL(this.OrgService);
            paymentBl.createRefund(new CreateRefundParameters
            {
                refundAmount = paymentParameters.paidAmount,
                reservationId = paymentParameters.reservationId,
                langId = paymentParameters.langId
            });
        }
        public void MakeRefundForContract(CreatePaymentParameters paymentParameters)
        {
            PaymentBL paymentBl = new PaymentBL(this.OrgService);
            paymentBl.createRefund(new CreateRefundParameters
            {
                refundAmount = paymentParameters.paidAmount,
                contractId = paymentParameters.contractId,
                langId = paymentParameters.langId
            });
        }
        public ManualPaymentResponse makeManualPayment(ManualPaymentParameters manualPaymentParameters)
        {
            OrganizationRequest request = new OrganizationRequest("rnt_ManualPayment");
            request["ManualPaymentParameters"] = JsonConvert.SerializeObject(manualPaymentParameters);
            var response = this.OrgService.Execute(request);
            return JsonConvert.DeserializeObject<ManualPaymentResponse>(Convert.ToString(response.Results["ManualPaymentResponse"]));
        }
    }
}

