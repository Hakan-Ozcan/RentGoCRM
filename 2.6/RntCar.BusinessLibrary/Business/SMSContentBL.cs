using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.BusinessLibrary.Business
{
    public class SMSContentBL : BusinessHandler
    {
        public SMSContentBL(IOrganizationService orgService) : base(orgService)
        {
        }

        public SMSContentBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public SMSContentBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }
        public string getSMSContentByCodeandLangId(int contentCode, int langId, string mobilePhone, string pnrNumber = "", string verificationCode = "",
                                                   string firstName = "", string lastName = "", string amount = "", string additionalProductName = "")
        {
            try
            {
                this.Trace("getSMSContentByCodeandLangId method started.");
                Entity customer = null;
                Entity additionalProduct = null;
                Entity reservation = null;
                DummyContactData dummyContact = null;

                string email = string.Empty;
                string message = string.Empty;
                string generatedMessage = string.Empty;
                int reservationType = 0;
                this.Trace("here i am");
                if (!string.IsNullOrEmpty(pnrNumber))
                {
                    ReservationRepository reservationRepository = new ReservationRepository(this.OrgService);
                    this.Trace("here i am 1 ");
                    string[] columns = new string[] { "rnt_dummycontactinformation", "rnt_reservationtypecode" };
                    this.Trace("here i am 2");
                    reservation = reservationRepository.getReservationByPnrNumberByGivenColumns(pnrNumber, columns);
                    this.Trace("here i am 3");
                    reservationType = reservation != null && reservation.Contains("rnt_reservationtypecode") ? reservation.GetAttributeValue<OptionSetValue>("rnt_reservationtypecode").Value : 0;
                    this.Trace("here i am 4");
                }

                this.Trace("lets go");
                //if reservation from broker or agency get virtual customer information for message
                if (reservation != null && (reservationType == (int)rnt_ReservationTypeCode.Broker || reservationType == (int)rnt_ReservationTypeCode.Acente))
                {
                    var dummy = reservation.Attributes.Contains("rnt_dummycontactinformation") ? reservation.GetAttributeValue<string>("rnt_dummycontactinformation") : string.Empty;
                    this.Trace("dummy = " + dummy);
                    dummyContact = string.IsNullOrEmpty(dummy) ? null : JsonConvert.DeserializeObject<DummyContactData>(dummy);

                    mobilePhone = dummyContact.phoneNumber;
                    this.Trace("dummyContact.phoneNumber " + mobilePhone);
                }
                else // if customer is true customer get customer information for message
                {
                    IndividualCustomerRepository customerRepository = new IndividualCustomerRepository(this.OrgService);
                    string[] columns = new string[] { "firstname", "lastname", "mobilephone", "emailaddress1" };
                    customer = customerRepository.getCustomerByMobilePhoneWithGivenColumns(mobilePhone, columns);

                }

                SMSContentRepository repository = new SMSContentRepository(this.OrgService);
                var smsEntity = repository.getSMSContentByCodeandLangId(contentCode, langId);

                if (smsEntity != null)
                {
                    message = smsEntity.GetAttributeValue<string>("rnt_message");
                    #region set firstname, lastname and email
                    if (customer != null)
                    {
                        this.Trace("Customer retrieved.");


                        if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                        {
                            firstName = customer.GetAttributeValue<string>("firstname");
                            lastName = customer.GetAttributeValue<string>("lastname");
                        }
                        email = customer.GetAttributeValue<string>("emailaddress1");
                    }
                    else if (dummyContact != null)
                    {
                        if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
                        {
                            firstName = dummyContact.name;
                            lastName = dummyContact.surname;
                        }
                        email = dummyContact.email;
                    }

                    #endregion

                    #region Creating message
                    generatedMessage = message.Replace("@firstName", firstName);
                    generatedMessage = generatedMessage.Replace("@lastName", lastName);

                    this.Trace("mobilePhone " + mobilePhone);
                    //if sms for reservation add pnrNumber
                    if (contentCode == (int)GlobalEnums.SmsContentCode.ReservationCreateSuccessSms ||
                        contentCode == (int)GlobalEnums.SmsContentCode.ReservationCreateMissingKnowledgeSms ||
                        contentCode == (int)GlobalEnums.SmsContentCode.ReservationUpdateSms)
                    {

                        generatedMessage = generatedMessage.Replace("@pnrNumber", pnrNumber);

                    } // if sms for contact add verificationCode
                    else if (contentCode == (int)GlobalEnums.SmsContentCode.ContactCreateSms ||
                        contentCode == (int)GlobalEnums.SmsContentCode.ContactUpdateSms)
                    {
                        generatedMessage = generatedMessage.Replace("@verificationCode", verificationCode);
                    }// if sms for contract add email
                    else if (contentCode == (int)GlobalEnums.SmsContentCode.ContractCloseSms)
                    {
                        generatedMessage = generatedMessage.Replace("@email", email);
                    }
                    else if (contentCode == (int)GlobalEnums.SmsContentCode.DepositRefund)
                    {
                        generatedMessage = generatedMessage.Replace("@pnrNumber", pnrNumber);
                        generatedMessage = generatedMessage.Replace("@amount ", amount);
                    }
                    else if (contentCode == (int)GlobalEnums.SmsContentCode.ManuelPaymentRefund)
                    {
                        generatedMessage = generatedMessage.Replace("@pnrNumber", pnrNumber);
                        generatedMessage = generatedMessage.Replace("@amount ", amount);
                    }
                    else if (contentCode == (int)GlobalEnums.SmsContentCode.ManuelPaymentPayment)
                    {
                        generatedMessage = generatedMessage.Replace("@pnrNumber", pnrNumber);
                        generatedMessage = generatedMessage.Replace("@amount ", amount);
                    }
                    else if (contentCode == (int)GlobalEnums.SmsContentCode.ManuelPaymentAdditionalProduct)
                    {
                        generatedMessage = generatedMessage.Replace("@pnrNumber", pnrNumber);
                        generatedMessage = generatedMessage.Replace("@amount ", amount);
                        generatedMessage = generatedMessage.Replace("@additionalProductName", additionalProductName);
                    }


                    #endregion

                    return generatedMessage;
                }
                else
                {
                    this.Trace("message is null");
                    return string.Empty;
                }
                this.Trace("finised sucessfully");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string getSMSContentByCodeandLangId(int contentCode, int langId, Dictionary<string, string> requirementList)
        {
            try
            {          
                this.Trace("getSMSContentByCodeandLangId method started.");
                SMSContentRepository repository = new SMSContentRepository(this.OrgService);
                var smsEntity = repository.getSMSContentByCodeandLangId(contentCode, langId);

                string generatedMessage = string.Empty;
                this.Trace("lets go");
                if (smsEntity != null)
                {
                    generatedMessage = smsEntity.GetAttributeValue<string>("rnt_message");
                    foreach (var item in requirementList)
                    {
                        generatedMessage = generatedMessage.Replace(item.Key, item.Value);
                    }
                }
                else
                {
                    this.Trace("message is null");
                }
                this.Trace("finised sucessfully");
                return generatedMessage;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
