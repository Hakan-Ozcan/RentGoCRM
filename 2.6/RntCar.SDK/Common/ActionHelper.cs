using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Tablet;
using RntCar.ClassLibrary.Actions;
using RntCar.ClassLibrary.MongoDB;
using RntCar.ClassLibrary.Reservation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Common
{
    public class ActionHelper
    {
        private CrmServiceClient _crmServiceClient;
        private IOrganizationService _service;

        public IOrganizationService IOrganizationService
        {
            get { return _service; }
        }
        public CrmServiceClient CrmServiceClient
        {
            get { return _crmServiceClient; }
        }
        public ActionHelper(CrmServiceClient crmServiceClient)
        {
            _crmServiceClient = crmServiceClient;
        }
        public ActionHelper(IOrganizationService organizationService)
        {
            _service = organizationService;
        }
        public ActionHelper(CrmServiceClient crmServiceClient, IOrganizationService organizationService)
        {
            _service = organizationService;
            _crmServiceClient = crmServiceClient;
        }
        public ClassLibrary._Tablet.ResponseResult UpdateContractforDelivery(UpdateContractforDeliveryParameters updateContractforDeliveryParameters)
        {
            OrganizationRequest request = new OrganizationRequest("rnt_UpdateContractforDelivery");
            request["updateContractforDeliveryParameters"] = JsonConvert.SerializeObject(updateContractforDeliveryParameters);
            return this.Execute<ClassLibrary._Tablet.ResponseResult>(request, "UpdateContractforDeliveryResponse");            
        }
        public ClassLibrary._Tablet.ResponseResult UpdateContractforRental(UpdateContractforRentalParameters updateContractforRentalParameters)
        {
            OrganizationRequest request = new OrganizationRequest("rnt_updateContractforRental");
            request["updateContractforRentalParameters"] = JsonConvert.SerializeObject(updateContractforRentalParameters);

            return this.Execute< ClassLibrary._Tablet.ResponseResult>(request, "UpdateContractforRentalResponse");
            
        }
        public List<CreditCardData> getCustomerCreditCards(string individualCustomerId)
        {
            OrganizationRequest request = new OrganizationRequest("rnt_GetCustomerCreditCards");
            request["customerId"] = individualCustomerId;

            return this.Execute<List<CreditCardData>>(request,"CreditCardResponse");            
        }

        public AvailabilityResponse calculateAvailability(int langId, AvailabilityParameters availabilityParameters)
        {
            OrganizationRequest request = new OrganizationRequest("rnt_CalculateAvailibility");
            request["LangId"] = langId;
            request["AvailibilityParameters"] = JsonConvert.SerializeObject(availabilityParameters);
            return this.Execute<AvailabilityResponse>(request, "AvailibilityResponse");
        }

        public CalculatePricesForUpdateContractResponse CalculatePricesforUpdateContract(CalculatePricesForUpdateContractParameters calculatePricesForUpdateContractParameters)
        {
            OrganizationRequest request = new OrganizationRequest("rnt_CalculatePricesforUpdateContract");
            request["CalculatePricesforUpdateContractParameters"] = JsonConvert.SerializeObject(calculatePricesForUpdateContractParameters);
            return this.Execute<CalculatePricesForUpdateContractResponse>(request, "CalculatePricesforUpdateContractResponse");
        }
        public CreateDamageResponse createDamages(CreateDamageParameter createDamageParameter)
        {
            OrganizationRequest request = new OrganizationRequest("rnt_CreateDamages");
            request["DamageParameter"] = JsonConvert.SerializeObject(createDamageParameter);
            return this.Execute<CreateDamageResponse>(request, "CreateDamageResponse");
        }
        public ClassLibrary._Tablet.ResponseResult updateTransferForDelivery(UpdateTransferParameter updateTransferParameter)
        {
            OrganizationRequest organizationRequest = new OrganizationRequest("rnt_UpdateTransferForDelivery");
            organizationRequest["UpdateTranferParameter"] = JsonConvert.SerializeObject(updateTransferParameter);
            return this.Execute<ClassLibrary._Tablet.ResponseResult>(organizationRequest, "UpdateTransferResponse");
        }
        public ClassLibrary._Tablet.ResponseResult updateTransferForReturn(UpdateTransferParameter updateTransferParameter)
        {
            OrganizationRequest organizationRequest = new OrganizationRequest("rnt_UpdateTransferForReturn");
            organizationRequest["UpdateTranferParameter"] = JsonConvert.SerializeObject(updateTransferParameter);
            return this.Execute<ClassLibrary._Tablet.ResponseResult>(organizationRequest, "UpdateTransferResponse");
        }
        public ClassLibrary._Tablet.ResponseResult updateEquipmentInformation(UpdateEquipmentInformationParameter updateEquipmentInformationParameter)
        {
            OrganizationRequest organizationRequest = new OrganizationRequest("rnt_UpdateEquipmentInformation");
            organizationRequest["UpdateEquipmentInformationParameter"] = JsonConvert.SerializeObject(updateEquipmentInformationParameter);
            return this.Execute<ClassLibrary._Tablet.ResponseResult>(organizationRequest, "UpdateEquipmentInformationResponse");
        }
        public ContractCreateResponse createQuickContract(CreateQuickContractParameter parameter)
        {
            OrganizationRequest organizationRequest = new OrganizationRequest("rnt_ExecuteCreateQuickContract");
            organizationRequest["SelectedCustomer"] = JsonConvert.SerializeObject(parameter.customerInformation);
            organizationRequest["PriceParameters"] = JsonConvert.SerializeObject(parameter.priceInformation);
            organizationRequest["Currency"] = Convert.ToString(parameter.currency);
            organizationRequest["LangId"] = parameter.langId;
            organizationRequest["ReservationId"] = parameter.reservationId;
            organizationRequest["ReservationPNR"] = parameter.reservationPNR;
            organizationRequest["UserInformation"] = JsonConvert.SerializeObject(parameter.userInformation);
            organizationRequest["channelCode"] = parameter.channelCode;
            return this.Execute<ContractCreateResponse>(organizationRequest, "QuickContractResponse");
        }
        public ReservationCreateResponse createReservation(ReservationCreationParameters parameter)
        {
            ClassLibrary.Actions.rnt_CreateReservationRequest rnt_CreateReservationRequest = new ClassLibrary.Actions.rnt_CreateReservationRequest
            {
                SelectedCustomer = JsonConvert.SerializeObject(parameter.SelectedCustomer),
                SelectedDateAndBranch = JsonConvert.SerializeObject(parameter.SelectedDateAndBranch),
                SelectedEquipment = JsonConvert.SerializeObject(parameter.SelectedEquipment),
                PriceParameters = JsonConvert.SerializeObject(parameter.PriceParameters),
                SelectedAdditionalProducts = JsonConvert.SerializeObject(parameter.SelectedAdditionalProducts),
                ReservationChannel = parameter.ReservationChannel,
                ReservationTypeCode = parameter.ReservationTypeCode,
                Currency = parameter.Currency,
                TotalDuration = parameter.TotalDuration,
                LangId = parameter.LangId,
                TrackingNumber = parameter.TrackingNumber,               
            };
            
            var response = this.CrmServiceClient == null ? 
                           (ClassLibrary.Actions.rnt_CreateReservationResponse)this.IOrganizationService.Execute(rnt_CreateReservationRequest) :
                           (ClassLibrary.Actions.rnt_CreateReservationResponse)this.CrmServiceClient.Execute(rnt_CreateReservationRequest);
            return JsonConvert.DeserializeObject<ReservationCreateResponse>(response.ReservationResponse);            

         
        }
        public ContractValidationResponse checkBeforeContractCreation(CheckBeforeContractCreationParameters parameters)
        {
            OrganizationRequest organizationRequest = new OrganizationRequest("rnt_ExecuteCheckBeforeContractCreation");
            organizationRequest["ValidationParameters"] = JsonConvert.SerializeObject(parameters);
            return this.Execute<ContractValidationResponse>(organizationRequest, "ResponseResult");
        }
        public rnt_CreateIndividualCustomerandAddressResponse createIndividualCustomer()
        {
            ClassLibrary.Actions.rnt_CreateIndividualCustomerandAddressRequest rnt_CreateIndividualCustomerandAddressRequest = new ClassLibrary.Actions.rnt_CreateIndividualCustomerandAddressRequest
            {
                
            };
            var response = this.CrmServiceClient == null ?
                          (rnt_CreateIndividualCustomerandAddressResponse)this.IOrganizationService.Execute(rnt_CreateIndividualCustomerandAddressRequest) :
                          (rnt_CreateIndividualCustomerandAddressResponse)this.CrmServiceClient.Execute(rnt_CreateIndividualCustomerandAddressRequest);
            return response;
        }
        public NotifyCheckoutResponse HopiSales(NotifyCheckoutRequest notifyCheckoutParameters)
        {
            OrganizationRequest request = new OrganizationRequest("rnt_ExecuteHopiSales");
            request["notifyCheckoutParameters"] = JsonConvert.SerializeObject(notifyCheckoutParameters); 

            return this.Execute<NotifyCheckoutResponse>(request, "notifyCheckoutResponse");

        }
        public StartReturnTransactionResponse HopiReturn(StartReturnTransactionRequest startReturnTransactionParameters)
        {
            OrganizationRequest request = new OrganizationRequest("rnt_ExecuteHopiReturn");
            request["startReturnTransactionParameters"] = JsonConvert.SerializeObject(startReturnTransactionParameters);

            return this.Execute<StartReturnTransactionResponse>(request, "startReturnTransactionResponse");

        }
        private T Execute<T>(OrganizationRequest request,string responseKey)
        {
            var response = this.CrmServiceClient == null ? this.IOrganizationService.Execute(request) : this.CrmServiceClient.Execute(request);
            return JsonConvert.DeserializeObject<T>(Convert.ToString(response.Results[responseKey]));
        }
    }
}
