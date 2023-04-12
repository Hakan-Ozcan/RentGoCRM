using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.Logger;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RntCar.UpdateCreditCardSlip
{
    class Program
    {
        static void Main(string[] args)
        {
            LoggerHelper logger = new LoggerHelper();
            CrmServiceHelper crmServiceHelper = new CrmServiceHelper();


            PaymentRepository paymentRepository = new PaymentRepository(crmServiceHelper.IOrganizationService);
            InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(crmServiceHelper.IOrganizationService);
            var payments = paymentRepository.getPaymentsCreditCardSlipsNotIntegratedWithLogo();
            logger.traceInfo("items found : " + payments.Count);

            //payments = payments.Where(p => p.Id == new Guid("9ea099fc-0760-ec11-8f8f-000d3a29caa1")).ToList();
            foreach (var item in payments)
            {
                try
                {
                    if (!item.Contains("rnt_contractid"))
                    {
                        var address = invoiceAddressRepository.getInvoiceAddressByCustomerIdByGivenColumns(item.GetAttributeValue<EntityReference>("rnt_contactid").Id);

                        var invoiceAddress = address.OrderByDescending(p => p.GetAttributeValue<DateTime>("createdon")).FirstOrDefault();
                        var res = sendCreditCardSlipReservation(crmServiceHelper.IOrganizationService, item.GetAttributeValue<EntityReference>("rnt_reservationid").Id, invoiceAddress.Id);

                        logger.traceInfo("item id : " + item.Id);
                        logger.traceInfo(JsonConvert.SerializeObject(res));
                    }
                    else
                    {
                        var res = sendCreditCardSlip(crmServiceHelper.IOrganizationService, item.Id, item.GetAttributeValue<EntityReference>("rnt_contractid").Id, item.GetAttributeValue<EntityReference>("rnt_creditcardslipid").Id);

                        logger.traceInfo("item id : " + item.Id);
                        logger.traceInfo(JsonConvert.SerializeObject(res));
                    }
                }
                catch (Exception ex)
                {
                    logger.traceInfo(string.Format("exception record : {0} , detail : {1}", item.Id, ex.Message));
                }

            }
        }

        public static List<CreditCardSlipResponse> sendCreditCardSlipReservation(IOrganizationService service, Guid reservationId, Guid invoiceAddress)
        {
            List<CreditCardSlipResponse> creditCardSlipResponses = new List<CreditCardSlipResponse>();
            var invoiceAddressId = invoiceAddress;
            var reservationID = reservationId;

            InvoiceRepository invoiceRepository = new InvoiceRepository(service);
            var invoices = invoiceRepository.getDraftInvoicesByReservationId(reservationId);
            foreach (var item in invoices)
            {
                service.Delete("rnt_invoice", item.Id);
            }
            InvoiceHelper invoiceHelper = new InvoiceHelper(service);
            var invoiceID = invoiceHelper.createInvoiceFromInvoiceAddress_Reservation(reservationID, invoiceAddressId);

            CreditCardSlipRepository creditCardSlipRepository = new CreditCardSlipRepository(service);
            var slips = creditCardSlipRepository.getFailedIntegrationCreditCardSlips_Reservation(reservationID);
            foreach (var item in slips)
            {
                CreditCardSlipBL creditCardSlipBL = new CreditCardSlipBL(service);

                var resRef =
                new EntityReference
                {
                    LogicalName = "rnt_reservation",
                    Id = reservationID
                };
                var paymentRef = new EntityReference
                {
                    Id = item.GetAttributeValue<EntityReference>("rnt_paymentid").Id,
                    LogicalName = "rnt_payment"
                };
                creditCardSlipResponses.Add(creditCardSlipBL.handleCreateCreditCardSlipWithLogo(null, resRef, paymentRef, item.Id));
            }
            return creditCardSlipResponses;
        }

        public static CreditCardSlipResponse sendCreditCardSlip(IOrganizationService service, Guid _paymentId, Guid _contractId, Guid _slipId)
        {
            var paymentId = _paymentId;
            var contractId = _contractId;
            var slipId = _slipId;

            CreditCardSlipRepository creditCardSlipRepository = new CreditCardSlipRepository(service);

            CreditCardSlipBL creditCardSlipBL = new CreditCardSlipBL(service);

            var contractRef =
                new EntityReference
                {
                    LogicalName = "rnt_contract",
                    Id = contractId
                };

            var paymentRef = new EntityReference
            {
                Id = paymentId,
                LogicalName = "rnt_payment"
            };

            return creditCardSlipBL.handleCreateCreditCardSlipWithLogo(contractRef, null, paymentRef, slipId);
        }
    }
}
