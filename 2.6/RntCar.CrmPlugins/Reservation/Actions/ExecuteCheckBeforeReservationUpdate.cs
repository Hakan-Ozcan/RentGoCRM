using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.CrmPlugins.Reservation.Actions
{
    public class ExecuteCheckBeforeReservationUpdate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            PluginInitializer initializer = new PluginInitializer(serviceProvider);
            try
            {
                string reservationId;
                initializer.PluginContext.GetContextParameter<string>("ReservationId", out reservationId);

                int langId;
                initializer.PluginContext.GetContextParameter<int>("LangId", out langId);

                ReservationRepository reservationRepository = new ReservationRepository(initializer.Service);
                var reservation = reservationRepository.getReservationById(Guid.Parse(reservationId), new string[] { "statuscode", "statecode", "rnt_customerid", "rnt_reservationtypecode", "rnt_paymentmethodcode" });
               
                initializer.TraceMe("rnt_reservationtypecode : " + (reservation.Contains("rnt_reservationtypecode") ? "true" : "false"));
                initializer.TraceMe("ReservationId" + reservationId);
                if (reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayBroker ||
                   reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.PayOffice ||
                   reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.FullCredit ||
                   (reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.CreditCard &&
                    reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value == (int)rnt_ReservationTypeCode.Acente) ||
                   reservation.GetAttributeValue<OptionSetValue>("rnt_paymentmethodcode").Value == (int)rnt_PaymentMethodCode.LimitedCredit)
                {
                    //todo will read from xml
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>("Broker ve acente rezervasyonları güncellenemez. Lütfen iptal edip , yenisini oluşturun.");
                }

                #region Retrieve Individual Customer Detail Data
                var contactId = reservation.GetAttributeValue<EntityReference>("rnt_customerid").Id;
                initializer.TraceMe("individualCustomer retrieve start");
                IndividualCustomerBL individualCustomerBL = new IndividualCustomerBL(initializer.Service);
                var individualCustomer = individualCustomerBL.getIndividualCustomerInformationForValidation(contactId);
                initializer.TraceMe("individualCustomer retrieve end");
                #endregion

                #region Black List Validation
                initializer.TraceMe("Black List Validation start");
                BlackListBL blackListBL = new BlackListBL(initializer.Service);
                initializer.TraceMe("black list governmentId : " + individualCustomer.governmentId);
                var blackListValidation = blackListBL.BlackListValidation(individualCustomer.governmentId);
                initializer.TraceMe("IsInBlackList : " + blackListValidation.BlackList.IsInBlackList);
                if (blackListValidation.BlackList.IsInBlackList)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("BlackListValidation", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
                initializer.TraceMe("Black List Validation end");
                #endregion

                ReservationUpdateValidation reservationUpdateValidation = new ReservationUpdateValidation(initializer.Service);

                var response = reservationUpdateValidation.checkReservationStatus(reservation);
                if (!response)
                {
                    XrmHelper xrmHelper = new XrmHelper(initializer.Service);
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("ReservationUpdateStatus", langId);
                    initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(message);
                }
            }
            catch (Exception ex)
            {
                initializer.PluginContext.ThrowException<InvalidPluginExecutionException>(ex.Message);
            }
        }
    }
}
