using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.ClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Common
{
    public class LogoHelper : IDisposable
    {
        private static EndpointAddress myEndpointAddress { get; set; }
        private static BasicHttpBinding myBasicHttpBinding { get; set; }
        private LogoService.RentGoServiceSoapClient rentGoServiceSoapClient { get; set; }
        public IOrganizationService Service { get; set; }
        public ITracingService TracingService { get; set; }
        public string[] loginInfo { get; set; }
        private string endpointUrl { get; set; }
        public LogoHelper(IOrganizationService _service)
        {
            Service = _service;
            prepareServiceConfiguration();
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            rentGoServiceSoapClient = new LogoService.RentGoServiceSoapClient(myBasicHttpBinding, myEndpointAddress);
        }
        public LogoHelper(IOrganizationService _service, ITracingService _tracingService, bool useLocalIP = false)
        {
            Service = _service;
            TracingService = _tracingService;
            prepareServiceConfiguration(useLocalIP);
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
            rentGoServiceSoapClient = new LogoService.RentGoServiceSoapClient(myBasicHttpBinding, myEndpointAddress);
        }

        public string getCurrentFirm()
        {
            return rentGoServiceSoapClient.CurrentFirm();
        }

        private void prepareServiceConfiguration(bool useLocalIP = false)
        {
            XrmHelper xrmHelper = new XrmHelper(this.Service);
            endpointUrl = xrmHelper.getConfigurationValueByName("logoServiceEndpointUrl");
            loginInfo = xrmHelper.getConfigurationValueByName("logoServiceLoginInfo").Split(';');
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Name = "RentGoServiceSoap";
            binding.Security.Mode = BasicHttpSecurityMode.None;
            //crm'in timeoutu 120 saniye olduğundan 100'e çekelim logoyu
            binding.OpenTimeout = TimeSpan.FromSeconds(100);
            binding.CloseTimeout = TimeSpan.FromSeconds(100);
            binding.ReceiveTimeout = TimeSpan.FromSeconds(100);
            binding.SendTimeout = TimeSpan.FromSeconds(100);
            binding.MaxReceivedMessageSize = 2147483647;
            myBasicHttpBinding = binding;

            myEndpointAddress = new EndpointAddress(endpointUrl);//todo will be read from xrm helper
        }
        public bool Login()
        {
            var response = rentGoServiceSoapClient.Login(loginInfo[0], loginInfo[1], Convert.ToInt32(loginInfo[2]));
            this.trace(JsonConvert.SerializeObject(response));
            if (response.Equals("1"))
            {
                return true;
            }

            throw new Exception("login hatası");

        }
        public bool Disconnect()
        {
            return rentGoServiceSoapClient.Disconnect();
        }
        public void trace(string text)
        {
            if (this.TracingService != null)
            {
                this.TracingService.Trace(text);
            }
        }
        public void connect()
        {
            if (rentGoServiceSoapClient.Connect() == "1")
            {
                this.trace("connect == 1");
                var currentFirm = rentGoServiceSoapClient.CurrentFirm();

                this.trace("currentFirm : " + currentFirm);
                this.trace("loginInfo : " + loginInfo[2]);

                if (currentFirm != loginInfo[2])
                {
                    this.trace("current firm is not equal to logo");

                    this.Disconnect();
                    this.Login();
                }
            }
            else
            {
                this.trace("connect == 0");
                this.Login();
            }
        }
        public byte[] getPDFContent(string invoiceNumber)
        {
            this.connect();
            return rentGoServiceSoapClient.InvoicePdf(invoiceNumber, string.Empty);

        }
        public double getAccountBalance(string taxNumber)
        {

            return rentGoServiceSoapClient.CurrentAccountBalance("", "", taxNumber);

        }
        public string currentAccountCode(CurrentAccountCodeParameter parameter)
        {
            //parameter.einvoiceEmail = null;
            if (!string.IsNullOrEmpty(parameter.taxNo) && parameter.taxNo.Length == 11)
            {
                parameter.tckn = parameter.taxNo;
                parameter.taxNo = "";
            }
            if (!string.IsNullOrEmpty(parameter.tckn) && parameter.tckn.Length == 10)
            {
                parameter.taxNo = parameter.tckn;
                parameter.tckn = "";

            }


            return rentGoServiceSoapClient.CurrentAccountCode(parameter.tckn,
                                                              parameter.taxNo,
                                                              parameter.customerFirstName,
                                                              parameter.customerLastName,
                                                              parameter.title,
                                                              parameter.address,
                                                              parameter.address2,
                                                              parameter.mobilePhone,
                                                              parameter.email,
                                                              parameter.city,
                                                              parameter.town,
                                                              parameter.country,
                                                              parameter.taxOffice,
                                                              parameter.email,
                                                              "MS",
                                                              parameter.corporateType,
                                                              parameter.paymentMethods?.ElementAtOrDefault(0) != null ? parameter.paymentMethods[0] : string.Empty,
                                                              parameter.paymentMethods?.ElementAtOrDefault(1) != null ? parameter.paymentMethods[1] : string.Empty,
                                                              parameter.paymentMethods?.ElementAtOrDefault(2) != null ? parameter.paymentMethods[2] : string.Empty);
        }
        public SalesInvoiceResponse salesInvoice(SalesInvoiceParameter parameter)
        {
            try
            {
                var auxilCode = parameter.invoiceInformationList.Where(p => !string.IsNullOrEmpty(p.plateNumber)).FirstOrDefault()?.plateNumber;
                var note = parameter.notes != null && parameter.notes.ElementAtOrDefault(0) != null ? parameter.notes[0] : null;
                var note1 = parameter.notes != null && parameter.notes.ElementAtOrDefault(1) != null ? parameter.notes[1] : null;
                var note2 = parameter.notes != null && parameter.notes.ElementAtOrDefault(2) != null ? parameter.notes[2] : null;
                var note3 = parameter.notes != null && parameter.notes.ElementAtOrDefault(3) != null ? parameter.notes[3] : null;
                var convertedInvoiceInfo = parameter.invoiceInformationList.ConvertAll(item => new LogoService.SatirBilgiAlanlar
                {
                    Description = item.description,
                    Metarial_Code = item.metarialCode,
                    Metarial_Description = item.metarialDescription,
                    Quantity = item.quantity,
                    Type = item.type,
                    Unit_Code = item.unitCode,
                    Unit_Price = item.unitPrice,
                    Vat_Rate = item.vatRate,
                    Auxil_Code = auxilCode,
                    Vatexcept_Code = item.vatExceptCode,
                    Vatexcept_Reason = item.vatExceptReason
                });
                string vatExceptCode = string.Empty, vateExceptReason = string.Empty;

                var zeroVatList = convertedInvoiceInfo.Where(p => p.Vat_Rate == 0).ToList();
                if (zeroVatList.Count > 0)
                {
                    vatExceptCode = StaticHelper._350;
                    vateExceptReason = StaticHelper._others;
                }
                var response = rentGoServiceSoapClient.SalesInvoice(parameter.currentAccountCode,
                                                                    parameter.documentInvoiceNo,
                                                                    parameter.documentNumber,
                                                                    parameter.invoiceDate,
                                                                    parameter.warehouse,
                                                                    note,
                                                                    note1,
                                                                    note2,
                                                                    note3,
                                                                    parameter.tckn,
                                                                    parameter.taxNo,
                                                                    parameter.division,
                                                                    parameter.projectCode,
                                                                    convertedInvoiceInfo.ToArray(),
                                                                    parameter.currentAccountCodeShpm,
                                                                    vatExceptCode,
                                                                    vateExceptReason).FirstOrDefault();
                if (response == null)
                {
                    return new SalesInvoiceResponse
                    {
                        ResponseResult = ResponseResult.ReturnError("null response")
                    };
                }

                var convertedResponse = new SalesInvoiceData
                {
                    invoiceRef = response.FaturaRef,
                    invoiceNumber = response.FaturaNumara,
                    errorMessage = response.Hata
                };

                if (!string.IsNullOrEmpty(convertedResponse.errorMessage))
                {
                    return new SalesInvoiceResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(convertedResponse.errorMessage)
                    };
                }
                return new SalesInvoiceResponse
                {
                    salesInvoices = convertedResponse,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new SalesInvoiceResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }

        public CreditCardSlipResponse creditCardSlip(CreditCardSlipParameter parameter)
        {

            //reservation number or contract number
            var response = rentGoServiceSoapClient.CreditCardSlip(parameter.paymentCreatedon.ToString("dd.MM.yyyy"),
                                                                     parameter.division,
                                                                     parameter.pnrNumber,
                                                                     parameter.documentNumber,
                                                                     parameter.currentAccountCode,
                                                                     parameter.bankCode,
                                                                     parameter.projectCode,
                                                                     parameter.approveNumber,
                                                                     parameter.description,
                                                                     Convert.ToDouble(parameter.credit),
                                                                     null,
                                                                     parameter.paymentType).FirstOrDefault();
            if (response == null)
            {
                return new CreditCardSlipResponse
                {
                    ResponseResult = ResponseResult.ReturnError("null response")
                };
            }

            if (!string.IsNullOrEmpty(response.Hata))
            {
                return new CreditCardSlipResponse
                {
                    ResponseResult = ResponseResult.ReturnError(response.Hata)
                };
            }

            return new CreditCardSlipResponse
            {
                plugNumber = response.KrediKartNumara,
                plugReference = response.KrediKartRef,
                ResponseResult = ResponseResult.ReturnSuccess()
            };

        }

        public CreditCardSlipResponse creditCardSlip_Iyzico(CreditCardSlipParameter parameter)
        {

            //reservation number or contract number
            var response = rentGoServiceSoapClient.RemittanceSlip(parameter.description,
                                                                  parameter.paymentCreatedon.ToString("dd.MM.yyyy"),
                                                                  Convert.ToDouble(parameter.credit),
                                                                  parameter.division,
                                                                  "320.4830343157",
                                                                  parameter.currentAccountCode,
                                                                  parameter.projectCode,
                                                                  Convert.ToString(parameter.installment),
                                                                  parameter.pnrNumber + " -" + parameter.documentNumber,
                                                                  parameter.paymentType).FirstOrDefault();


            if (response == null)
            {
                return new CreditCardSlipResponse
                {
                    ResponseResult = ResponseResult.ReturnError("null response")
                };
            }

            if (!string.IsNullOrEmpty(response.Hata))
            {
                return new CreditCardSlipResponse
                {
                    ResponseResult = ResponseResult.ReturnError(response.Hata)
                };
            }

            return new CreditCardSlipResponse
            {
                plugNumber = response.VirmanNumara,
                plugReference = response.VirmanFisRef,
                ResponseResult = ResponseResult.ReturnSuccess()
            };

        }
        ~LogoHelper()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                var res = this.rentGoServiceSoapClient.Disconnect();
            }
        }

        public CreditCardSlipResponse remittanceSlip(RemittanceSlipParameter parameter)
        {
            try
            {
                var response = rentGoServiceSoapClient.RemittanceSlip(parameter.description, parameter.date, parameter.total, parameter.division, parameter.iyzicoArpCode, parameter.custemerArpCode, parameter.projeCode, parameter.specode, parameter.lineDescription, parameter.canceledStatus).FirstOrDefault();

                if (response == null)
                {
                    return new CreditCardSlipResponse
                    {
                        ResponseResult = ResponseResult.ReturnError("null response")
                    };
                }

                if (!string.IsNullOrEmpty(response.Hata))
                {
                    return new CreditCardSlipResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(response.Hata)
                    };
                }

                return new CreditCardSlipResponse
                {
                    plugNumber = response.VirmanNumara,
                    plugReference = response.VirmanFisRef,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };

            }
            catch (Exception ex)
            {
                return new CreditCardSlipResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
    }
}
