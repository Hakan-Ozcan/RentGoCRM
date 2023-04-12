using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Helpers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Linq;

namespace RntCar.BusinessLibrary.Business
{
    public class CreditCardSlipBL : BusinessHandler
    {
        private DateTime createdon { get; set; }
        private string paymentResultId { get; set; }
        private string paymentTransactionId { get; set; }
        private decimal totalAmount { get; set; }
        private int paymentType { get; set; }
        private string virtualPosId { get; set; }
        private string pnrNumber { get; set; }
        private string documentNumber { get; set; }
        private string corporateId { get; set; }
        private string customerId { get; set; }
        private Guid pickupBranchId { get; set; }
        private int paymentTypeCode { get; set; }
        private string currentAccountCode { get; set; }
        private Guid? invoiceId { get; set; }
        private bool isNetTahsilat { get; set; }
        private int transferTypeCode { get; set; }
        public int installment { get; set; }
        public int paymentProvider { get; set; }

        public CreditCardSlipBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public CreditCardSlipBL(CrmServiceClient crmServiceClient) : base(crmServiceClient)
        {
        }

        public CreditCardSlipBL(IOrganizationService orgService, CrmServiceClient crmServiceClient) : base(orgService, crmServiceClient)
        {
        }

        public CreditCardSlipBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public void updateCreditCardSlipContract(Entity creditCardSlip)
        {
            this.Trace("creditCardSlip.Contains(rnt_reservationid) : " + creditCardSlip.Contains("rnt_reservationid"));
            this.Trace("creditCardSlip.Contains(rnt_contractid) : " + creditCardSlip.Contains("rnt_contractid"));
            if (creditCardSlip.Contains("rnt_reservationid") && !creditCardSlip.Contains("rnt_contractid"))
            {
                this.Trace("Contains rnt_reservationid , not contains rnt_contractid");
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                var reservation = reservationRepository.getReservationById(creditCardSlip.GetAttributeValue<EntityReference>("rnt_reservationid").Id, new string[] { "rnt_contractnumber" });

                if (reservation.Contains("rnt_contractnumber"))
                {
                    this.Trace("reservation contains rnt_contractnumber");
                    Entity creditCardSlipUpdate = new Entity(creditCardSlip.LogicalName);
                    creditCardSlipUpdate.Id = creditCardSlip.Id;
                    creditCardSlipUpdate["rnt_contractid"] = reservation.GetAttributeValue<EntityReference>("rnt_contractnumber");
                    this.OrgService.Update(creditCardSlipUpdate);
                }

            }
        }
        public void updatePaymentWithCreditCardSlip(Guid creditCardSlipId, Guid paymentId)
        {
            this.Trace(string.Format("Payment entity updated by Id : {0} , credicardslipId : {1}", paymentId, creditCardSlipId));
            Entity e = new Entity("rnt_payment");
            e["rnt_creditcardslipid"] = new EntityReference("rnt_creditcardslip", creditCardSlipId);
            e.Id = paymentId;
            this.OrgService.Update(e);
        }
        public void setName(Entity entity)
        {
            string name = string.Empty;

            if (entity.Contains("rnt_paymentid"))
            {
                this.Trace("entity contains paymentid");
                PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
                var payment = paymentRepository.retrieveById(entity.GetAttributeValue<EntityReference>("rnt_paymentid").LogicalName,
                                                             entity.GetAttributeValue<EntityReference>("rnt_paymentid").Id,
                                                             new string[] { "rnt_paymenttransactionid", "rnt_transactiontypecode" });

                name += payment.GetAttributeValue<string>("rnt_paymenttransactionid");

                var v = payment.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode").Value;

                if ((int)RntCar.ClassLibrary._Enums_1033.rnt_TransactionTypeCode.Deposit == v)
                {
                    name += " - Teminat";
                }
                else if ((int)RntCar.ClassLibrary._Enums_1033.rnt_TransactionTypeCode.Sale == v)
                {
                    name += " - Satış";
                }
                else if ((int)RntCar.ClassLibrary._Enums_1033.rnt_TransactionTypeCode.Refund == v)
                {
                    name += " - Iade";
                }

                name += " - Slipi";
            }

            entity["rnt_name"] = name;
        }

        public void updateCreditCardSlipWithServiceParameter(Guid creditCardSlipId,
                                                             string plugNumber,
                                                             string plugReference,
                                                             int statusCode,
                                                             string currentAccountCodeInput,
                                                             string currentAccountCodeOutput,
                                                             string creditCardSlipInput,
                                                             string creditCardSlipOutput,
                                                             string firm)
        {
            Entity entity = new Entity("rnt_creditcardslip");
            entity.Id = creditCardSlipId;

            entity["rnt_plugnumber"] = plugNumber;
            entity["rnt_plugreference"] = plugReference;
            entity["statuscode"] = new OptionSetValue(statusCode);
            entity["rnt_currentaccountcodeinput"] = currentAccountCodeInput;
            entity["rnt_currentaccountcodeoutput"] = currentAccountCodeOutput;
            entity["rnt_creditcardslipinput"] = creditCardSlipInput;
            entity["rnt_creditcardslipoutput"] = creditCardSlipOutput + "-" + firm;
            this.OrgService.Update(entity);
        }

        public void updateCreditCardSlipWithInternalError(Guid creditCardSlipId,
                                                          string errorMessage)
        {
            Entity entity = new Entity("rnt_creditcardslip");
            entity.Id = creditCardSlipId;

            entity["statuscode"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_creditcardslip_StatusCode.InternalError);

            entity["rnt_internalerrormessage"] = errorMessage;
            this.OrgService.Update(entity);
        }
        private void updateCreditCardSlipWithIntegratedWithLogo(Guid creditCardSlipId)
        {
            Entity entity = new Entity("rnt_creditcardslip");
            entity.Id = creditCardSlipId;
            entity["statuscode"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_creditcardslip_StatusCode.IntegratedWithLogo);
            this.OrgService.Update(entity);
        }
        public Guid createCreditCarSlip(Guid? reservationId,
                                        Guid? contractId,
                                        Guid paymentId)
        {
            Entity entity = new Entity("rnt_creditcardslip");
            if (contractId.HasValue)
                entity["rnt_contractid"] = new EntityReference("rnt_contract", contractId.Value);

            if (reservationId.HasValue)
                entity["rnt_reservationid"] = new EntityReference("rnt_reservation", reservationId.Value);

            entity["rnt_paymentid"] = new EntityReference("rnt_payment", paymentId);

            PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
            var payment = paymentRepository.getPaymentByIdByGivenColumns(paymentId, new string[] { "rnt_amount", "rnt_transactiontypecode" });

            decimal totalAmount = 0;
            if (payment.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode").Value == (int)RntCar.ClassLibrary._Enums_1033.rnt_TransactionTypeCode.Refund)
            {
                totalAmount = payment.GetAttributeValue<Money>("rnt_amount").Value * -1;
            }
            else
            {
                totalAmount = payment.GetAttributeValue<Money>("rnt_amount").Value;
            }
            entity["rnt_totalamount"] = new Money(totalAmount);
            entity["statuscode"] = new OptionSetValue((int)ClassLibrary._Enums_1033.rnt_creditcardslip_StatusCode.Draft);
            return this.OrgService.Create(entity);
        }

        public CreditCardSlipResponse handleCreateCreditCardSlipWithLogo(EntityReference contract, EntityReference reservation, EntityReference payment, Guid creditCardSlipId)
        {
            CreditCardSlipRepository creditCardSlipRepository = new CreditCardSlipRepository(this.OrgService);
            var creditCardSlipEntity = creditCardSlipRepository.getCreditCardSlipById(creditCardSlipId);
            OptionSetValue statusCode = creditCardSlipEntity.GetAttributeValue<OptionSetValue>("statuscode");
            string taxNumber = creditCardSlipEntity.GetAttributeValue<string>("rnt_taxnumber");
            string goverment = creditCardSlipEntity.GetAttributeValue<string>("rnt_goverment");
            if (statusCode.Value == (int)rnt_creditcardslip_StatusCode.IntegratedWithLogo)
            {
                this.Trace("credit card slip is already send to logo");
                return new CreditCardSlipResponse
                {
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }

            this.getPaymentInformation(payment.Id);
            this.Trace("this.transferTypeCode : " + this.transferTypeCode);
            this.Trace("this.paymentTransactionId : " + this.paymentTransactionId);
            this.Trace("this.paymentResultId  : " + this.paymentResultId);

            if (this.transferTypeCode == (int)rnt_payment_rnt_transfertypecode.MoneyOrder)
            {
                this.updateCreditCardSlipWithIntegratedWithLogo(creditCardSlipId);
                return new CreditCardSlipResponse { ResponseResult = ResponseResult.ReturnSuccess() };
            }

            LogoHelper logoHelper = new LogoHelper(this.OrgService, this.TracingService);
            this.Trace("getting login info : " + (logoHelper.loginInfo.Length > 2 ? logoHelper.loginInfo[2].ToString() : ""));

            logoHelper.connect();

            this.getDocumentInformation(contract, reservation);


            InvoiceRepository invoiceRepository = new InvoiceRepository(this.OrgService);

            this.Trace("paymentTypeCode : " + this.paymentTypeCode);
            this.Trace("corporateId : " + this.corporateId);
            CurrentAccountCodeParameter currentAccountCodeParameter = null;
            BranchHelper branchHelper = new BranchHelper(this.OrgService);
            var branchInfo = branchHelper.getBranchType(reservation?.Id, contract?.Id);
            if (branchInfo.branchType != (int)rnt_BranchType.Office)
            {
                this.currentAccountCode = branchInfo.logoAccountNumber;
                currentAccountCodeParameter = new CurrentAccountCodeParameter();
                currentAccountCodeParameter.tckn = this.currentAccountCode;
            }

            var creditCardSlipResponse = new CreditCardSlipResponse();

            var statusCodeValue = (int)ClassLibrary._Enums_1033.rnt_creditcardslip_StatusCode.IntegrationError;

            object parameter = null;
            if (string.IsNullOrWhiteSpace(taxNumber) && string.IsNullOrWhiteSpace(goverment))
            {
                parameter = new CreditCardSlipParameter();

                if (currentAccountCodeParameter == null)
                {
                    currentAccountCodeParameter = new CurrentAccountCodeParameter();

                    Entity invoice = null;
                    if (contract != null)
                    {
                        if (this.invoiceId.HasValue)
                        {
                            this.Trace("invoice id is not null");
                            invoice = invoiceRepository.getInvoiceById(this.invoiceId.Value);
                        }
                        else
                        {
                            this.Trace("invoice id is null");
                            invoice = invoiceRepository.getFirstActiveInvoiceByContractId(contract.Id).FirstOrDefault();

                        }
                        if (invoice == null)
                        {
                            ContractRepository contractRepository = new ContractRepository(this.OrgService);
                            var c = contractRepository.getContractById(contract.Id, new string[] { "rnt_contracttypecode", "rnt_paymentmethodcode", "rnt_customerid" });
                            if (c.GetAttributeValue<OptionSetValue>("rnt_contracttypecode").Value == (int)rnt_ReservationTypeCode.Kurumsal &&
                               c.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.CreditCard)
                            {
                                InvoiceHelper invoiceHelper = new InvoiceHelper(this.OrgService);
                                invoice = invoiceHelper.createInvoiceFromCorporateCustomer(new Guid(this.corporateId), contract.Id);
                            }
                            else
                            {
                                InvoiceHelper invoiceHelper = new InvoiceHelper(this.OrgService);
                                invoice = invoiceHelper.createInvoiceFromIndividualCustomer(c.GetAttributeValue<EntityReference>("rnt_customerid").Id, contract.Id);
                            }

                        }
                        if (!string.IsNullOrEmpty(this.corporateId) &&
                            this.paymentTypeCode == (int)rnt_PaymentMethodCode.PayBroker)
                        {
                            this.Trace("invoice:  " + invoice == null ? "invoice is null" : "invoice is not null : " + invoice?.Id);
                            this.Trace("rnt_taxnumber :  " + (invoice == null ? "invoice is null" : invoice.GetAttributeValue<string>("rnt_taxnumber")));
                            if (invoice != null && !string.IsNullOrEmpty(invoice.GetAttributeValue<string>("rnt_taxnumber")))
                            {
                                CorporateCustomerRepository corporateCustomerRepository = new CorporateCustomerRepository(this.OrgService);
                                var corp = corporateCustomerRepository.getCorporateCustomerById(new Guid(this.corporateId), new string[] { "rnt_taxnumber" });
                                //pay brokerda slip her zaman bireysel
                                this.Trace("corp rnt_taxnumber " + corp.GetAttributeValue<string>("rnt_taxnumber"));
                                this.Trace("invoice rnt_taxnumber " + invoice.GetAttributeValue<string>("rnt_taxnumber"));

                                if (corp.GetAttributeValue<string>("rnt_taxnumber") == invoice.GetAttributeValue<string>("rnt_taxnumber") && contract != null)
                                {
                                    var invoices = invoiceRepository.getInvoicesByContractIdExceptByGivenTaxNumber(contract.Id, invoice.GetAttributeValue<string>("rnt_taxnumber"));
                                    this.Trace("invoices.count " + invoices.Count);
                                    if (invoices.Count > 0)
                                    {
                                        invoice = invoices.FirstOrDefault();
                                    }
                                    else
                                    {
                                        InvoiceAddressRepository invoiceAddressRepository = new InvoiceAddressRepository(this.OrgService);
                                        var invoiceAddress = invoiceAddressRepository.getInvoiceAddressByCustomerIdByGivenColumns(new Guid(this.customerId));
                                        InvoiceHelper invoiceHelper = new InvoiceHelper(this.OrgService);
                                        var id = invoiceHelper.createInvoiceFromInvoiceAddress(contract.Id, invoiceAddress.FirstOrDefault().Id);
                                        invoice = invoiceRepository.getInvoiceById(id.Value);
                                    }


                                }
                            }
                        }
                    }
                    else if (reservation != null)
                        invoice = invoiceRepository.getFirstActiveInvoiceByReservationId(reservation.Id).FirstOrDefault();

                    this.Trace("final invoice : " + invoice.Id);

                    currentAccountCodeParameter = this.prepareCurrentAccountCodeParameter(invoice);
                    this.currentAccountCode = logoHelper.currentAccountCode(currentAccountCodeParameter);
                }

                if (!string.IsNullOrEmpty(currentAccountCode))
                {
                    parameter = this.prepareCreditCardSlipParameter();
                    this.Trace("creditCardSlipParameter : " + JsonConvert.SerializeObject((CreditCardSlipParameter)parameter));
                    this.Trace("this.isNetTahsilat : " + this.isNetTahsilat);
                    if (this.isNetTahsilat)
                    {
                        creditCardSlipResponse = logoHelper.creditCardSlip((CreditCardSlipParameter)parameter);
                    }
                    else
                    {
                        creditCardSlipResponse = logoHelper.creditCardSlip_Iyzico((CreditCardSlipParameter)parameter);
                    }


                    statusCodeValue = creditCardSlipResponse.ResponseResult.Result ? (int)ClassLibrary._Enums_1033.rnt_creditcardslip_StatusCode.IntegratedWithLogo :
                                                                                        (int)ClassLibrary._Enums_1033.rnt_creditcardslip_StatusCode.IntegrationError;

                    this.Trace("creditCardSlipResponse : " + JsonConvert.SerializeObject(creditCardSlipResponse));
                }

            }
            else
            {

                parameter = new RemittanceSlipParameter();


                if (currentAccountCodeParameter == null)
                {
                    currentAccountCodeParameter = new CurrentAccountCodeParameter();
                    currentAccountCodeParameter = this.prepareCurrentAccountCodeParameterWithCreditCardSlip(creditCardSlipEntity);
                    this.currentAccountCode = logoHelper.currentAccountCode(currentAccountCodeParameter);
                }

                if (!string.IsNullOrEmpty(currentAccountCode))
                {
                    parameter = this.prepareRemittanceSlipParameter();
                    this.Trace("remittanceSlipParameter : " + JsonConvert.SerializeObject((RemittanceSlipParameter)parameter));

                    creditCardSlipResponse = logoHelper.remittanceSlip((RemittanceSlipParameter)parameter);

                    statusCodeValue = creditCardSlipResponse.ResponseResult.Result ? (int)ClassLibrary._Enums_1033.rnt_creditcardslip_StatusCode.IntegratedWithLogo :
                                                                                        (int)ClassLibrary._Enums_1033.rnt_creditcardslip_StatusCode.IntegrationError;

                    this.Trace("remittanceSlipResponse : " + JsonConvert.SerializeObject(creditCardSlipResponse));


                }

            }

            this.Trace("currentAccountCodeParameter : " + JsonConvert.SerializeObject(currentAccountCodeParameter));
            // if currentAccountCode is null, update record with IntegrationError
            this.Trace("update crm start");
            this.updateCreditCardSlipWithServiceParameter(creditCardSlipId,
                                              creditCardSlipResponse.plugNumber,
                                              Convert.ToString(creditCardSlipResponse?.plugReference),
                                              statusCodeValue,
                                              JsonConvert.SerializeObject(currentAccountCodeParameter),
                                              JsonConvert.SerializeObject(currentAccountCode),
                                              JsonConvert.SerializeObject(parameter),
                                              JsonConvert.SerializeObject(creditCardSlipResponse),
                                              logoHelper.getCurrentFirm());
            this.Trace("update crm end");
            this.Trace("creditCardSlipResponse : " + JsonConvert.SerializeObject(creditCardSlipResponse));

            return creditCardSlipResponse;
        }

        private void getPaymentInformation(Guid paymentId)
        {
            PaymentRepository paymentRepository = new PaymentRepository(this.OrgService);
            var payment = paymentRepository.getPaymentById(paymentId);

            this.createdon = payment.GetAttributeValue<DateTime>("createdon");
            this.paymentResultId = payment.GetAttributeValue<string>("rnt_paymentresultid");
            this.paymentTransactionId = payment.GetAttributeValue<string>("rnt_paymenttransactionid");
            this.totalAmount = payment.GetAttributeValue<Money>("rnt_amount").Value;
            this.paymentType = payment.GetAttributeValue<OptionSetValue>("rnt_transactiontypecode").Value;
            this.virtualPosId = payment.GetAttributeValue<string>("rnt_virtualposid");
            this.invoiceId = payment.Contains("rnt_invoiceid") ? (Guid?)payment.GetAttributeValue<EntityReference>("rnt_invoiceid").Id : null;
            this.isNetTahsilat = !string.IsNullOrEmpty(this.paymentTransactionId) && !this.paymentTransactionId.StartsWith("NT") ? false : true;
            this.installment = payment.GetAttributeValue<int>("rnt_installment") == 0 ? 1 : payment.GetAttributeValue<int>("rnt_installment");
            this.transferTypeCode = payment.Attributes.Contains("rnt_transfertypecode") ? payment.GetAttributeValue<OptionSetValue>("rnt_transfertypecode").Value : 0;
            this.paymentProvider = payment.Attributes.Contains("rnt_paymentprovider") ? payment.GetAttributeValue<OptionSetValue>("rnt_paymentprovider").Value : 0;
        }
        private CurrentAccountCodeParameter prepareCurrentAccountCodeParameter(Entity invoice)
        {
            var firstName = string.Empty;
            var lastName = string.Empty;
            var companyName = string.Empty;
            var accountCodeTitle = string.Empty;
            var invoiceType = invoice.GetAttributeValue<OptionSetValue>("rnt_invoicetypecode").Value;
            if (invoiceType == (int)ClassLibrary._Enums_1033.rnt_invoice_rnt_invoicetypecode.Individual)
            {
                firstName = invoice.Attributes.Contains("rnt_firstname") ? invoice.GetAttributeValue<string>("rnt_firstname") : string.Empty;
                lastName = invoice.Attributes.Contains("rnt_lastname") ? invoice.GetAttributeValue<string>("rnt_lastname") : string.Empty;

                firstName = CommonHelper.removeWhitespacesForTCKNValidation(firstName);
                lastName = CommonHelper.removeWhitespacesForTCKNValidation(lastName);
                accountCodeTitle = firstName + " " + lastName;
            }
            else
            {
                if (invoice.GetAttributeValue<string>("rnt_taxnumber").Length == 11)
                {
                    if (invoice.Contains("rnt_contractid"))
                    {
                        //şahıs firmaları
                        Entity e = new Entity(invoice.LogicalName);
                        e.Id = invoice.Id;
                        e["rnt_invoicetypecode"] = new OptionSetValue(10);
                        e["rnt_govermentid"] = invoice.GetAttributeValue<string>("rnt_taxnumber");
                        e["rnt_taxnumber"] = null;
                        e["rnt_companyname"] = null;


                        ContractRepository contractRepository = new ContractRepository(this.OrgService);
                        var c = contractRepository.getContractById(invoice.GetAttributeValue<EntityReference>("rnt_contractid").Id);

                        IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
                        var customer = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(c.GetAttributeValue<EntityReference>("rnt_customerid").Id, new string[] { "firstname", "lastname" });

                        e["rnt_firstname"] = customer.GetAttributeValue<string>("firstname");
                        e["rnt_lastname"] = customer.GetAttributeValue<string>("lastname");
                        this.OrgService.Update(e);


                        firstName = customer.GetAttributeValue<string>("firstname");
                        lastName = customer.GetAttributeValue<string>("lastname");

                        firstName = CommonHelper.removeWhitespacesForTCKNValidation(firstName);
                        lastName = CommonHelper.removeWhitespacesForTCKNValidation(lastName);
                        accountCodeTitle = firstName + " " + lastName;
                    }

                }
                else
                {
                    companyName = invoice.Attributes.Contains("rnt_companyname") ? invoice.GetAttributeValue<string>("rnt_companyname") : string.Empty;
                    accountCodeTitle = companyName;
                }

            }
            var emailAddress = invoice.Attributes.Contains("rnt_email") ? invoice.GetAttributeValue<string>("rnt_email") : string.Empty;
            // get current account code from logo
            return new CurrentAccountCodeParameter
            {
                tckn = invoice.Attributes.Contains("rnt_govermentid") ? invoice.GetAttributeValue<string>("rnt_govermentid") : string.Empty,
                taxNo = invoice.Attributes.Contains("rnt_taxnumber") ? invoice.GetAttributeValue<string>("rnt_taxnumber") : string.Empty,
                customerFirstName = firstName,
                customerLastName = lastName,
                title = accountCodeTitle,
                address = invoice.GetAttributeValue<string>("rnt_addressdetail"),
                address2 = string.Empty,
                mobilePhone = invoice.Attributes.Contains("rnt_mobilephone") ? invoice.GetAttributeValue<string>("rnt_mobilephone") : string.Empty,
                email = emailAddress,
                city = invoice.Attributes.Contains("rnt_cityid") ? invoice.GetAttributeValue<EntityReference>("rnt_cityid").Name : string.Empty,
                town = invoice.Attributes.Contains("rnt_districtid") ? invoice.GetAttributeValue<EntityReference>("rnt_districtid").Name : string.Empty,
                country = invoice.Attributes.Contains("rnt_countryid") ? invoice.GetAttributeValue<EntityReference>("rnt_countryid").Name : string.Empty,
                taxOffice = invoice.Attributes.Contains("rnt_taxofficeid") ? invoice.GetAttributeValue<EntityReference>("rnt_taxofficeid").Name : string.Empty,
                einvoiceEmail = emailAddress
            };
        }

        private CurrentAccountCodeParameter prepareCurrentAccountCodeParameterWithCreditCardSlip(Entity creditCardSlip)
        {
            var emailAddress = creditCardSlip.Attributes.Contains("rnt_email") ? creditCardSlip.GetAttributeValue<string>("rnt_email") : string.Empty;
            // get current account code from logo
            return new CurrentAccountCodeParameter
            {
                tckn = creditCardSlip.Attributes.Contains("rnt_goverment") ? creditCardSlip.GetAttributeValue<string>("rnt_goverment") : string.Empty,
                taxNo = creditCardSlip.Attributes.Contains("rnt_taxnumber") ? creditCardSlip.GetAttributeValue<string>("rnt_taxnumber") : string.Empty,
                customerFirstName = creditCardSlip.Attributes.Contains("rnt_firstname") ? creditCardSlip.GetAttributeValue<string>("rnt_firstname") : string.Empty,
                customerLastName = creditCardSlip.Attributes.Contains("rnt_lastname") ? creditCardSlip.GetAttributeValue<string>("rnt_lastname") : string.Empty,
                title = creditCardSlip.Attributes.Contains("rnt_companyname") ? creditCardSlip.GetAttributeValue<string>("rnt_companyname") : string.Empty,
                address = creditCardSlip.GetAttributeValue<string>("rnt_addressdetail"),
                address2 = string.Empty,
                mobilePhone = creditCardSlip.Attributes.Contains("rnt_mobilephone") ? creditCardSlip.GetAttributeValue<string>("rnt_mobilephone") : string.Empty,
                email = emailAddress,
                city = creditCardSlip.Attributes.Contains("rnt_cityid") ? creditCardSlip.GetAttributeValue<EntityReference>("rnt_cityid").Name : string.Empty,
                town = creditCardSlip.Attributes.Contains("rnt_districtid") ? creditCardSlip.GetAttributeValue<EntityReference>("rnt_districtid").Name : string.Empty,
                country = creditCardSlip.Attributes.Contains("rnt_countryid") ? creditCardSlip.GetAttributeValue<EntityReference>("rnt_countryid").Name : string.Empty,
                taxOffice = creditCardSlip.Attributes.Contains("rnt_taxofficeid") ? creditCardSlip.GetAttributeValue<EntityReference>("rnt_taxofficeid").Name : string.Empty,
                einvoiceEmail = emailAddress
            };
        }

        private void getDocumentInformation(EntityReference contractRef, EntityReference reservationRef)
        {
            if (contractRef != null)
            {
                ContractRepository contractRepository = new ContractRepository(this.OrgService);
                var contract = contractRepository.getContractById(contractRef.Id, new string[] { "rnt_paymentmethodcode", "rnt_customerid", "rnt_corporateid", "rnt_pickupbranchid", "rnt_contractnumber", "rnt_pnrnumber" });

                this.documentNumber = contract.GetAttributeValue<string>("rnt_contractnumber");
                this.pnrNumber = contract.GetAttributeValue<string>("rnt_pnrnumber");
                this.paymentTypeCode = contract.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
                this.pickupBranchId = contract.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id;
                this.corporateId = contract.Contains("rnt_corporateid") ? contract.GetAttributeValue<EntityReference>("rnt_corporateid").Id.ToString() : null;
                this.customerId = contract.Contains("rnt_customerid") ? contract.GetAttributeValue<EntityReference>("rnt_customerid").Id.ToString() : null;
            }
            else if (reservationRef != null)
            {
                ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                var reservation = reservationRepository.getReservationById(reservationRef.Id, new string[] { "rnt_customerid", "rnt_paymentmethodcode", "rnt_corporateid", "rnt_pickupbranchid", "rnt_reservationnumber", "rnt_pnrnumber" });
                this.documentNumber = reservation.GetAttributeValue<string>("rnt_reservationnumber");
                this.pnrNumber = reservation.GetAttributeValue<string>("rnt_pnrnumber");
                this.paymentTypeCode = reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value;
                this.pickupBranchId = reservation.GetAttributeValue<EntityReference>("rnt_pickupbranchid").Id;
                this.corporateId = reservation.Contains("rnt_corporateid") ? reservation.GetAttributeValue<EntityReference>("rnt_corporateid").Id.ToString() : null;
                this.customerId = reservation.Contains("rnt_customerid") ? reservation.GetAttributeValue<EntityReference>("rnt_customerid").Id.ToString() : null;
            }
        }
        private CreditCardSlipParameter prepareCreditCardSlipParameter()
        {
            BranchRepository branchRepository = new BranchRepository(this.OrgService);
            var branch = branchRepository.getBranchById(this.pickupBranchId);
            var branchProjectCode = branch.GetAttributeValue<string>("rnt_logoprojectcode");
            var branchDivision = Convert.ToInt32(branch.GetAttributeValue<string>("rnt_logodivision"));
            LogoBankCodeMappingRepository logoBankCodeMappingRepository = new LogoBankCodeMappingRepository(this.OrgService);
            string logoBankCode = "";
            Entity logoBankCodeEntity=new Entity();
            if (this.paymentProvider == (int)GlobalEnums.PaymentProvider.iyzico)
            {
                logoBankCodeEntity = logoBankCodeMappingRepository.getIyzicoVirtualPosId();
               
            }
            else
            {
                logoBankCodeEntity = logoBankCodeMappingRepository.getNetTahsilatVirtualPosId();
            }

            logoBankCode = logoBankCodeEntity.GetAttributeValue<string>("rnt_logobankcode");
            this.virtualPosId = logoBankCodeEntity.GetAttributeValue<string>("rnt_name");

            this.Trace("currentBankName : " + this.virtualPosId);

            string type = string.Empty;
            string pnrDescription = string.Empty;

            if (this.paymentType == (int)PaymentEnums.PaymentTransactionType.SALE)
            {
                type = "Dökümanın Tahsilatı";
                pnrDescription = string.Format("PNR :{0} ,{1} Nolu Dökümanın Sanal Pos Tahsilatı", pnrNumber, this.documentNumber);
            }
            else if (this.paymentType == (int)PaymentEnums.PaymentTransactionType.DEPOSIT)
            {
                type = "Dökümanın Teminatı";
                pnrDescription = string.Format("PNR :{0} ,{1} Nolu Dökümanın Sanal Pos Teminat Çekimi", pnrNumber, this.documentNumber);

            }
            else if (this.paymentType == (int)PaymentEnums.PaymentTransactionType.REFUND)
            {
                type = "Dökümanın Iadesi";
                pnrDescription = string.Format("PNR :{0} ,{1} Nolu Dökümanın Sanal Pos İadesi", pnrNumber, this.documentNumber);
            }

            var param = new CreditCardSlipParameter
            {
                description = string.Format("PNR Numarası :{0} , Döküman Numarası {1} , {2}", pnrNumber, this.documentNumber, type),
                paymentCreatedon = createdon,
                division = branchDivision,
                pnrNumber = pnrDescription,
                documentNumber = this.documentNumber,
                currentAccountCode = currentAccountCode,
                bankCode = logoBankCode,
                projectCode = branchProjectCode,
                documentPaymentResultId = paymentTransactionId,
                approveNumber = paymentTransactionId,
                credit = totalAmount,
                paymentType = this.paymentType == (int)PaymentEnums.PaymentTransactionType.REFUND ? true : false,
                installment = this.installment
            };

            //this.Trace("param : " + JsonConvert.SerializeObject(param));
            return param;
        }

        private RemittanceSlipParameter prepareRemittanceSlipParameter()
        {
            BranchRepository branchRepository = new BranchRepository(this.OrgService);
            var branch = branchRepository.getBranchById(this.pickupBranchId);
            var branchProjectCode = branch.GetAttributeValue<string>("rnt_logoprojectcode");
            var branchDivision = Convert.ToInt32(branch.GetAttributeValue<string>("rnt_logodivision"));
            LogoBankCodeMappingRepository logoBankCodeMappingRepository = new LogoBankCodeMappingRepository(this.OrgService);
            string logoBankCode = "";
            Entity logoBankCodeEntity = new Entity();
            if (this.paymentProvider == (int)GlobalEnums.PaymentProvider.iyzico)
            {
                logoBankCodeEntity = logoBankCodeMappingRepository.getIyzicoVirtualPosId();

            }
            else
            {
                logoBankCodeEntity = logoBankCodeMappingRepository.getNetTahsilatVirtualPosId();
            }

            logoBankCode = logoBankCodeEntity.GetAttributeValue<string>("rnt_logobankcode");
            this.virtualPosId = logoBankCodeEntity.GetAttributeValue<string>("rnt_name");

            string type = string.Empty;
            string pnrDescription = string.Empty;

            if (this.paymentType == (int)PaymentEnums.PaymentTransactionType.SALE)
            {
                type = "Dökümanın Tahsilatı";
                pnrDescription = string.Format("PNR :{0} ,{1} Nolu Dökümanın Sanal Pos Tahsilatı", pnrNumber, this.documentNumber);
            }
            else if (this.paymentType == (int)PaymentEnums.PaymentTransactionType.DEPOSIT)
            {
                type = "Dökümanın Teminatı";
                pnrDescription = string.Format("PNR :{0} ,{1} Nolu Dökümanın Sanal Pos Teminat Çekimi", pnrNumber, this.documentNumber);

            }
            else if (this.paymentType == (int)PaymentEnums.PaymentTransactionType.REFUND)
            {
                type = "Dökümanın Iadesi";
                pnrDescription = string.Format("PNR :{0} ,{1} Nolu Dökümanın Sanal Pos İadesi", pnrNumber, this.documentNumber);
            }

            var param = new RemittanceSlipParameter
            {
                description = pnrDescription,
                date = createdon.ToString("dd.MM.yyyy"),
                division = branchDivision,
                custemerArpCode = currentAccountCode,
                iyzicoArpCode = logoBankCode,
                lineDescription = null,
                projeCode = branchProjectCode,
                specode = null,
                canceledStatus = this.paymentType == (int)PaymentEnums.PaymentTransactionType.REFUND ? true : false,
                total = (double)totalAmount
            };

            //this.Trace("param : " + JsonConvert.SerializeObject(param));
            return param;
        }

    }


}
