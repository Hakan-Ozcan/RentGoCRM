using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.BusinessLibrary.Validations;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary.MarkettingPermission;
using RntCar.SDK.Common;
using RntCar.SDK.Mappers;
using System;
using System.Collections.Generic;

namespace RntCar.BusinessLibrary.Business
{
    public class IndividualCustomerBL : BusinessHandler
    {

        public IndividualCustomerBL(GlobalEnums.ConnectionType enums = GlobalEnums.ConnectionType.default_Service) : base(enums)
        {
        }
        public IndividualCustomerBL(IOrganizationService orgService) : base(orgService)
        {
        }
        public IndividualCustomerBL(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }

        public IndividualCustomerBL(PluginInitializer initializer, IOrganizationService orgService, ITracingService tracingService) : base(initializer, orgService, tracingService)
        {
        }

        public bool checkUserISVIPorStaff(Guid id)
        {
            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
            var segmentEntity = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(id, new string[] { "rnt_segmentcode" });

            if (segmentEntity.Contains("rnt_segmentcode") &&
               (segmentEntity.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value == (int)rnt_CustomerSegment.Staff ||
                segmentEntity.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value == (int)rnt_CustomerSegment.VIP10 ||
                segmentEntity.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value == (int)rnt_CustomerSegment.VIP15 ||
                segmentEntity.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value == (int)rnt_CustomerSegment.PersonnelOfTunalar)
                )
            {
                return true;
            }
            return false;
        }
        public void updateDebitAmount(CustomerDebtResponse debitResponse)
        {
            Entity e = new Entity("contact");
            e.Id = debitResponse.customerId;
            e["rnt_debitamount"] = new Money(debitResponse.debtAmount);
            this.OrgService.Update(e);
        }

        public bool checkDebitAmount(Guid contactId)
        {
            bool checkDebitAmount = true;
            Entity contact = this.OrgService.Retrieve("contact", contactId, new ColumnSet("rnt_debitamount"));
            if (contact.Contains("rnt_debitamount"))
            {
                Money debitAmount = contact.GetAttributeValue<Money>("rnt_debitamount");
                if (debitAmount.Value > 0)
                {
                    checkDebitAmount = false;
                }
            }
            return checkDebitAmount;
        }

        public void buildLogoAccountCode(Entity entity)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid").Split(';')[0];
            this.Trace("turkeyGuid : " + turkeyGuid);
            if (entity.Attributes.Contains("rnt_citizenshipid"))
            {
                this.Trace("rnt_citizenshipid contains");
                if (entity.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id == new Guid(turkeyGuid))
                {
                    this.Trace("turkey : " + entity.GetAttributeValue<string>("governmentid"));
                    entity["rnt_logoaccountcode"] = entity.GetAttributeValue<string>("governmentid");
                }
                else
                {
                    var passportNumber = entity.GetAttributeValue<string>("rnt_passportnumber");
                    this.Trace("passportNumber : " + CommonHelper.buildCharacters(passportNumber, 11));
                    entity["rnt_logoaccountcode"] = CommonHelper.buildCharacters(passportNumber, 11);
                }
            }

        }

        public void updateWebPassword(Guid individualCustomerId, string password)
        {
            Entity e = new Entity("contact");
            e.Id = individualCustomerId;
            e["rnt_webpassword"] = password;
            this.OrgService.Update(e);
        }

        public void updateMobilePassword(Guid individualCustomerId, string password)
        {
            Entity e = new Entity("contact");
            e.Id = individualCustomerId;
            e["rnt_webpassword"] = password;
            this.OrgService.Update(e);
        }
        public IndividualCustomerCreateResponse callCreateCustomerAction(IndividualCustomerCreateParameter _individualCustomerParameters)
        {

            OrganizationRequest request = new OrganizationRequest("rnt_CreateIndividualCustomerandAddress");
            request["CustomerInformation"] = JsonConvert.SerializeObject(_individualCustomerParameters);
            var response = this.OrgService.Execute(request);

            return new IndividualCustomerCreateResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess(),
                CustomerId = Convert.ToString(response.Results["IndividualCustomerId"]),
                IndividualAddressId = Convert.ToString(response.Results["IndividualAddressId"]),
                InvoiceAddressId = Convert.ToString(response.Results["InvoiceAddressId"]),
            };
        }
        public IndividualCustomerUpdateResponse callUpdateCustomerAction(IndividualCustomerUpdateParameters individualCustomerUpdateParameters)
        {
            OrganizationRequest request = new OrganizationRequest("rnt_UpdateIndividualCustomerandAddress");
            request["CustomerInformation"] = JsonConvert.SerializeObject(individualCustomerUpdateParameters);
            var response = this.OrgService.Execute(request);

            return new IndividualCustomerUpdateResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public IndividualCustomerCreateResponse createCustomer(string _individualCustomerParameters, int langId)
        {
            IndividualCustomerCreateParameter individualCustomerParameters = JsonConvert.DeserializeObject<IndividualCustomerCreateParameter>(_individualCustomerParameters,
                                                                                                                 new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

            this.Trace("param" + _individualCustomerParameters);
            this.Trace("langId" + langId);

            IndividualCustomerValidation individualCustomerValidation = new IndividualCustomerValidation(this.OrgService, this.TracingService);
            this.Trace("checkIndividualCustomerMandatoryFields start" + DateTime.Now);
            var result = individualCustomerValidation.checkIndividualCustomerMandatoryFields(individualCustomerParameters); //null control for all fields
            this.Trace("checkIndividualCustomerMandatoryFields end" + result);
            if (!result)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("IndividualCustomerMandatoryFields", langId);
                return new IndividualCustomerCreateResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            };
            this.Trace("checkIndividualCustomerMandatoryFields end" + DateTime.Now);
            this.Trace("checkIndividualCustomerPassportInputs start" + DateTime.Now);

            if (!individualCustomerParameters.isTurkishCitizen) // passport fields validation
            {
                var passportInputsValidation = individualCustomerValidation.checkIndividualCustomerPassportInputs(individualCustomerParameters);
                if (!passportInputsValidation)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                    var message = xrmHelper.GetXmlTagContentByGivenLangId("IndividualCustomerMandatoryFields", langId);
                    return new IndividualCustomerCreateResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

            }
            this.Trace("checkIndividualCustomerPassportInputs end" + DateTime.Now);
            //if turkish citizen must be validated for local rules
            if (individualCustomerParameters.isTurkishCitizen)
            {
                this.Trace("checkIndividualCustomerGovermentIdInput start" + DateTime.Now);
                var govermentIdInputValidationResult = individualCustomerValidation.checkIndividualCustomerGovermentIdInput(individualCustomerParameters);
                if (!govermentIdInputValidationResult)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                    var message = xrmHelper.GetXmlTagContentByGivenLangId("IndividualCustomerMandatoryFields", langId);
                    return new IndividualCustomerCreateResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

                this.Trace("checkIndividualCustomerGovermentIdInput end" + DateTime.Now);
                this.Trace("checkIndividualCustomerGovermentIdInputLength start" + DateTime.Now);
                var govermentIdLenghtValidationResult = individualCustomerValidation.checkIndividualCustomerGovermentIdInputLength(individualCustomerParameters); // check goverment id input length
                if (!govermentIdLenghtValidationResult)
                {
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                    var message = xrmHelper.GetXmlTagContentByGivenLangId("MinTCKNNumber", langId);
                    return new IndividualCustomerCreateResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }
                this.Trace("checkIndividualCustomerGovermentIdInputLength end" + DateTime.Now);
                this.Trace("checkIndividualCustomerGovermentIdValidity start" + DateTime.Now);

                var govermentIdValidityResult = individualCustomerValidation.checkIndividualCustomerGovermentIdValidity(individualCustomerParameters); //check for valid tcnk
                if (!govermentIdValidityResult)
                {
                    this.Trace("govermentIdValidityResult" + govermentIdValidityResult);
                    XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                    var message = xrmHelper.GetXmlTagContentByGivenLangId("InvalidTCKNNumber", langId);

                    return new IndividualCustomerCreateResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }
                this.Trace("checkIndividualCustomerGovermentIdValidity end" + DateTime.Now);
            }
            this.Trace("checkIndividualCustomerDrivingLicenseDate start" + DateTime.Now);

            var districtValidationResult = individualCustomerValidation.checkIndividualCustomerDistrict(individualCustomerParameters);
            if (!districtValidationResult)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("NullDistrict", langId);
                return new IndividualCustomerCreateResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }

            var drivingLicenseDateValidationResult = individualCustomerValidation.checkIndividualCustomerDrivingLicenseDate(individualCustomerParameters); // date cannot be greater then today
            if (!drivingLicenseDateValidationResult)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("LicenseDateValidation", langId);
                return new IndividualCustomerCreateResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            this.Trace("checkIndividualCustomerDrivingLicenseDate end" + DateTime.Now);
            this.Trace("checkIndividualCustomerBirthDate start" + DateTime.Now);

            var birthDateValidationResult = individualCustomerValidation.checkIndividualCustomerBirthDate(individualCustomerParameters); // date cannot be smaller than 17
            if (!birthDateValidationResult)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("IndividualCustomerBirthDateValidation", langId);
                return new IndividualCustomerCreateResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            this.Trace("checkIndividualCustomerBirthDate end" + DateTime.Now);
            this.Trace("checkIndividualCustomerEmail start" + DateTime.Now);

            var emailValidationResult = individualCustomerValidation.checkIndividualCustomerEmail(individualCustomerParameters); // email regex control
            if (!emailValidationResult)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("EmailValidation", langId);
                return new IndividualCustomerCreateResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            this.Trace("checkIndividualCustomerEmail end" + DateTime.Now);
            this.Trace("checkIndividualCustomerMobilePhone start" + DateTime.Now);

            var mobilePhoneValidationResult = individualCustomerValidation.checkIndividualCustomerMobilePhone(individualCustomerParameters); // phone number regex control
            if (!mobilePhoneValidationResult)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                var message = xrmHelper.GetXmlTagContentByGivenLangId("MobilePhoneValidation", langId);
                return new IndividualCustomerCreateResponse
                {
                    ResponseResult = ResponseResult.ReturnError(message)
                };
            }
            this.Trace("checkIndividualCustomerMobilePhone end" + DateTime.Now);

            //if there is number in the system then clear the field
            //individualCustomerValidation.checkIndividualCustomerDuplicateFieldAndClear("mobilephone", individualCustomerParameters.mobilePhone);

            if (!individualCustomerParameters.isCallCenter)
            {
                individualCustomerValidation.checkIndividualCustomerDuplicateFieldAndClear("rnt_drivinglicensenumber", individualCustomerParameters.drivingLicenseNumber);
                if (individualCustomerParameters.findeksPoint.HasValue)
                {
                    int maximumFindeks;
                    var findeksValidationResponse = individualCustomerValidation.checkCustomerFinkdexPoint(individualCustomerParameters.findeksPoint.Value, out maximumFindeks);
                    if (!findeksValidationResponse)
                    {
                        XrmHelper xrmHelper = new XrmHelper(this.OrgService, this.PluginInitializer.InitiatingUserId);

                        var message = string.Format(xrmHelper.GetXmlTagContentByGivenLangId("FindeksPointValidation", langId), maximumFindeks);
                        this.Trace("findeks validation error" + message);
                        return new IndividualCustomerCreateResponse
                        {
                            ResponseResult = ResponseResult.ReturnError(message)
                        };
                    }
                }
            }
            this.Trace("contact creation start" + DateTime.Now);
            var entityId = this.createCustomer(individualCustomerParameters);
            this.Trace("contact creation end" + DateTime.Now);

            //now create addresses
            this.Trace("individual address creation start" + DateTime.Now);
            var individualAddressId = Guid.Empty;
            this.Trace("!individualCustomerParameters.isCallCenter" + !individualCustomerParameters.isCallCenter);
            this.Trace("individualCustomerParameters.distributionChannelCode" + individualCustomerParameters.distributionChannelCode);
            this.Trace("individualCustomerParameters.isAlsoInvoiceAddress" + individualCustomerParameters.isAlsoInvoiceAddress);

            if (!individualCustomerParameters.isCallCenter && individualCustomerParameters.distributionChannelCode != (int)rnt_DistributionChannelCode.Mobile)
            {
                IndividualAddressBL individualAddressBL = new IndividualAddressBL(this.OrgService);
                individualAddressId = individualAddressBL.createDefaultIndividualAddress(new IndividualAddressCreateParameters
                {
                    addressCountryId = Guid.Parse(individualCustomerParameters.addressCountry.value),
                    addressCityId = individualCustomerParameters.addressCity != null &&
                                    individualCustomerParameters.addressCity.value != null ? (Guid?)Guid.Parse(individualCustomerParameters.addressCity.value) : null,
                    addressDistrictId = individualCustomerParameters.addressDistrict != null &&
                                        individualCustomerParameters.addressDistrict.value != null ? (Guid?)Guid.Parse(individualCustomerParameters.addressDistrict.value) : null,
                    individualCustomerId = entityId,
                    addressDetail = individualCustomerParameters.addressDetail
                });

                this.Trace("individual address created" + DateTime.Now);
            }
            this.Trace("individual address creation end" + DateTime.Now);
            this.Trace("invoice address creation start" + DateTime.Now);
            Guid invoiceAddressId = Guid.Empty;
            if (!individualCustomerParameters.isCallCenter && individualCustomerParameters.distributionChannelCode != (int)rnt_DistributionChannelCode.Mobile)
            {
                if (individualCustomerParameters.isAlsoInvoiceAddress)
                {

                    InvoiceAddressBL invoiceAddressBL = new InvoiceAddressBL(this.OrgService);
                    invoiceAddressId = invoiceAddressBL.createInvoiceAddress(new InvoiceAddressCreateParameters
                    {
                        individualCustomerId = entityId,
                        addressDetail = individualCustomerParameters.addressDetail,
                        firstName = individualCustomerParameters.firstName,
                        lastName = individualCustomerParameters.lastName,
                        governmentId = individualCustomerParameters.isTurkishCitizen ? individualCustomerParameters.governmentId : individualCustomerParameters.passportNumber,
                        //means individual type of invoice address
                        invoiceType = 10,
                        addressCountryId = Guid.Parse(individualCustomerParameters.addressCountry.value),
                        addressCityId = individualCustomerParameters.addressCity != null &&
                                        individualCustomerParameters.addressCity.value != null ? (Guid?)Guid.Parse(individualCustomerParameters.addressCity.value) : null,
                        addressDistrictId = individualCustomerParameters.addressDistrict != null &&
                                            individualCustomerParameters.addressDistrict.value != null ? (Guid?)Guid.Parse(individualCustomerParameters.addressDistrict.value) : null,
                    });

                }
                else
                {
                    var invoiceAddress = individualCustomerParameters.invoiceAddress;
                    this.Trace("invoiceAddress in else" + JsonConvert.SerializeObject(invoiceAddress));

                    InvoiceAddressBL invoiceAddressBL = new InvoiceAddressBL(this.OrgService);
                    invoiceAddressId = invoiceAddressBL.createInvoiceAddress(new InvoiceAddressCreateParameters
                    {
                        individualCustomerId = entityId,
                        invoiceType = invoiceAddress.invoiceType.HasValue ? invoiceAddress.invoiceType.Value : 10,
                        firstName = string.IsNullOrEmpty(invoiceAddress.firstName) ? string.Empty : invoiceAddress.firstName,
                        lastName = string.IsNullOrEmpty(invoiceAddress.lastName) ? string.Empty : invoiceAddress.lastName,
                        companyName = string.IsNullOrEmpty(invoiceAddress.companyName) ? string.Empty : invoiceAddress.companyName,
                        addressCityId = string.IsNullOrEmpty(invoiceAddress.addressCityId) ? null : (Guid?)Guid.Parse(invoiceAddress.addressCityId),
                        addressCountryId = string.IsNullOrEmpty(invoiceAddress.addressCountryId) ? Guid.Empty : Guid.Parse(invoiceAddress.addressCountryId),
                        addressDistrictId = string.IsNullOrEmpty(invoiceAddress.addressDistrictId) ? null : (Guid?)Guid.Parse(invoiceAddress.addressDistrictId),
                        taxOfficeId = string.IsNullOrEmpty(invoiceAddress.taxOfficeId) ? null : (Guid?)Guid.Parse(invoiceAddress.taxOfficeId),
                        addressDetail = string.IsNullOrEmpty(invoiceAddress.addressDetail) ? string.Empty : invoiceAddress.addressDetail,
                        governmentId = string.IsNullOrEmpty(invoiceAddress.governmentId) ? string.Empty : invoiceAddress.governmentId,
                        taxNumber = string.IsNullOrEmpty(invoiceAddress.taxNumber) ? string.Empty : invoiceAddress.taxNumber
                    }); ;
                }

            }

            this.Trace("invoice address creation end" + DateTime.Now);

            if (individualCustomerParameters.markettingPermissions.HasValue)
            {
                MarkettingPermissionsBL markettingPermissionsBL = new MarkettingPermissionsBL(this.OrgService);

                var marketingPermissionData = new MarketingPermission()
                {
                    contactId = entityId,
                    smsPermission = individualCustomerParameters.markettingPermissions.Value,
                    notificationPermission = individualCustomerParameters.markettingPermissions.Value,
                    kvkkPermission = individualCustomerParameters.markettingPermissions.Value,
                    etkPermission = individualCustomerParameters.markettingPermissions.Value,
                    emailPermission = individualCustomerParameters.markettingPermissions.Value,
                    callPermission = individualCustomerParameters.markettingPermissions.Value,
                    channelCode = individualCustomerParameters.distributionChannelCode,
                    operationType = (int)rnt_marketingpermissions_rnt_operationtype.CustomerCreation
                };
                var markettingPermissionId = markettingPermissionsBL.createMarkettingPermissions(marketingPermissionData);
            }

            return new IndividualCustomerCreateResponse
            {
                CustomerId = Convert.ToString(entityId),
                IndividualAddressId = Convert.ToString(individualAddressId),
                InvoiceAddressId = Convert.ToString(invoiceAddressId),
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public IndividualCustomerUpdateResponse updateCustomer(string _individualCustomerParameters)
        {
            List<string> errors = new List<string>();

            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorEventArgs)
                {
                    errors.Add(errorEventArgs.ErrorContext.Error.Message);
                    errorEventArgs.ErrorContext.Handled = true;
                },
                Converters = { new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" } }
            };

            IndividualCustomerUpdateParameters individualCustomerParameters = JsonConvert.DeserializeObject<IndividualCustomerUpdateParameters>(_individualCustomerParameters, settings);
            this.Trace("passport issue date : " + individualCustomerParameters.passportIssueDate);
            this.Trace("parameters deserialized");
            this.Trace("parameters : " + _individualCustomerParameters);
            Entity e = new Entity("contact");
            e.Attributes.Add("firstname", CommonHelper.removeWhitespacesForTCKNValidation(individualCustomerParameters.firstName));
            e.Attributes.Add("lastname", CommonHelper.removeWhitespacesForTCKNValidation(individualCustomerParameters.lastName));
            e.Attributes.Add("rnt_citizenshipid", new EntityReference("rnt_country", new Guid(individualCustomerParameters.nationality.value)));
            if (individualCustomerParameters.drivingLicenseCountry != null && !string.IsNullOrWhiteSpace(individualCustomerParameters.drivingLicenseCountry.value))
            {
                e.Attributes.Add("rnt_drivinglicensecountryid", new EntityReference("rnt_country", new Guid(individualCustomerParameters.drivingLicenseCountry.value)));
            }
            e.Attributes.Add("rnt_drivinglicensedate", individualCustomerParameters.drivingLicenseDate);


            if (!string.IsNullOrEmpty(individualCustomerParameters.mobilePhone))
            {
                e.Attributes.Add("mobilephone", individualCustomerParameters.mobilePhone.removeEmptyCharacters());
            }

            if (individualCustomerParameters.isTurkishCitizen)
            {
                //e.Attributes.Add("rnt_passportissuedate",null);
                e.Attributes.Add("rnt_passportnumber", null);
                e.Attributes.Add("governmentid", individualCustomerParameters.governmentId.removeEmptyCharacters());
            }
            else
            {
                e.Attributes.Add("governmentid", null);
                e.Attributes.Add("rnt_passportnumber", individualCustomerParameters.passportNumber.removeEmptyCharacters());
                //if (!individualCustomerParameters.isCallCenter)
                //    e.Attributes.Add("rnt_passportissuedate", individualCustomerParameters.passportIssueDate);

            }

            this.Trace("rnt_dialcode : " + individualCustomerParameters.dialCode == null ? "null" : "not null");
            if (!string.IsNullOrEmpty(individualCustomerParameters.dialCode.value))
            {
                e.Attributes.Add("rnt_dialcode", individualCustomerParameters.dialCode.value);
            }
            e.Attributes.Add("rnt_isturkishcitizen", individualCustomerParameters.isTurkishCitizen);
            e.Attributes.Add("birthdate", individualCustomerParameters.birthDate);
            e.Id = individualCustomerParameters.individualCustomerId.Value;
            if (!individualCustomerParameters.isCallCenter)
            {
                this.Trace("is not call center 2");
                if (!string.IsNullOrEmpty(individualCustomerParameters.email))
                {
                    e.Attributes.Add("emailaddress1", individualCustomerParameters.email.removeEmptyCharacters());
                }

                if ((int)rnt_DistributionChannelCode.Mobile != individualCustomerParameters.distributionChannelCode)
                {
                    this.Trace("not mobile");
                    e.Attributes.Add("rnt_drivinglicensenumber", individualCustomerParameters.drivingLicenseNumber);
                    e.Attributes.Add("rnt_drivinglicenseclasscode", new OptionSetValue(Convert.ToInt32(individualCustomerParameters.drivingLicenseClass.value)));
                    e.Attributes.Add("rnt_drivinglicenseplace", individualCustomerParameters.drivingLicensePlace);
                }
                else
                {
                    this.Trace("for mobile");
                    this.Trace("drivingLicenseNumber: " + individualCustomerParameters.drivingLicenseNumber);
                    this.Trace("rnt_drivinglicenseclasscode: " + individualCustomerParameters.drivingLicenseClass.value);
                    this.Trace("rnt_drivinglicenseplace: " + individualCustomerParameters.drivingLicensePlace);

                    if (!string.IsNullOrEmpty(individualCustomerParameters.drivingLicenseNumber))
                        e.Attributes.Add("rnt_drivinglicensenumber", individualCustomerParameters.drivingLicenseNumber);

                    if (!string.IsNullOrEmpty(individualCustomerParameters.drivingLicenseClass.value))
                        e.Attributes.Add("rnt_drivinglicenseclasscode", new OptionSetValue(Convert.ToInt32(individualCustomerParameters.drivingLicenseClass.value)));

                    if (!string.IsNullOrEmpty(individualCustomerParameters.drivingLicensePlace))
                        e.Attributes.Add("rnt_drivinglicenseplace", individualCustomerParameters.drivingLicensePlace);
                }

                if ((int)rnt_DistributionChannelCode.Web != individualCustomerParameters.distributionChannelCode)
                {
                    e.Attributes.Add("rnt_findekspoint", individualCustomerParameters.findeksPoint);
                }

                if (individualCustomerParameters.gender != null && !string.IsNullOrEmpty(individualCustomerParameters.gender.value)) // this condition is for the update from the create contract page
                {
                    e.Attributes.Add("gendercode", new OptionSetValue(Convert.ToInt32(individualCustomerParameters.gender.value)));
                }
            }
            if ((int)rnt_DistributionChannelCode.Web == individualCustomerParameters.distributionChannelCode || (int)rnt_DistributionChannelCode.Mobile == individualCustomerParameters.distributionChannelCode)
            {
                e["rnt_webemailaddress"] = individualCustomerParameters.email;
                this.Trace("individualCustomerParameters.password : " + individualCustomerParameters.password);
                if (!string.IsNullOrEmpty(individualCustomerParameters.password))
                {
                    e["rnt_webpassword"] = individualCustomerParameters.password;
                }
            }
            this.Trace("before update");
            this.OrgService.Update(e);
            this.Trace("after update");

            //now update addresses
            if (individualCustomerParameters.distributionChannelCode != (int)rnt_DistributionChannelCode.Mobile && individualCustomerParameters.individualAddressId.HasValue && !individualCustomerParameters.isCallCenter)
            {
                if (individualCustomerParameters.addressCountry.value != null)
                {
                    IndividualAddressBL individualAddressBL = new IndividualAddressBL(this.OrgService);
                    individualAddressBL.updateDefaultIndividualAddress(new IndividualAddresUpdateParameters
                    {
                        addressCountryId = Guid.Parse(individualCustomerParameters.addressCountry.value),
                        addressCityId = individualCustomerParameters.addressCity != null ? Guid.Parse(individualCustomerParameters.addressCity.value) : (Guid?)(null),
                        addressDistrictId = individualCustomerParameters.addressDistrict != null ? Guid.Parse(individualCustomerParameters.addressDistrict.value) : (Guid?)null,
                        individualAddressId = individualCustomerParameters.individualAddressId.Value,
                        addressDetail = individualCustomerParameters.addressDetail
                    });
                }

            }
            else if (individualCustomerParameters.distributionChannelCode != (int)rnt_DistributionChannelCode.Mobile && !individualCustomerParameters.individualAddressId.HasValue && !individualCustomerParameters.isCallCenter)
            {
                this.Trace("individualAddressId is null");
                IndividualAddressBL individualAddressBL = new IndividualAddressBL(this.OrgService);
                IndividualAddressRepository individualAddressRepository = new IndividualAddressRepository(this.OrgService);
                var address = individualAddressRepository.getDefaultIndividualAddressByCustomerIdByGivenColumns(individualCustomerParameters.individualCustomerId.Value, new string[] { });
                this.Trace("address " + address == null ? "null" : " not null");
                if (address != null && individualCustomerParameters.addressCountry != null)
                {
                    if (individualCustomerParameters.addressCountry.value != null)
                    {
                        individualAddressBL.updateDefaultIndividualAddress(new IndividualAddresUpdateParameters
                        {
                            addressCountryId = Guid.Parse(individualCustomerParameters.addressCountry.value),
                            addressCityId = individualCustomerParameters.addressCity != null ? Guid.Parse(individualCustomerParameters.addressCity.value) : (Guid?)(null),
                            addressDistrictId = individualCustomerParameters.addressDistrict != null ? Guid.Parse(individualCustomerParameters.addressDistrict.value) : (Guid?)null,
                            individualAddressId = address.Id,
                            addressDetail = individualCustomerParameters.addressDetail
                        });
                    }

                }

                else if (address == null && individualCustomerParameters.addressCountry != null)
                {
                    if (individualCustomerParameters.addressCountry.value != null)
                    {
                        var individualAddressId = individualAddressBL.createDefaultIndividualAddress(new IndividualAddressCreateParameters
                        {
                            addressCountryId = Guid.Parse(individualCustomerParameters.addressCountry.value),
                            addressCityId = individualCustomerParameters.addressCity != null ? (Guid?)Guid.Parse(individualCustomerParameters.addressCity.value) : null,
                            addressDistrictId = individualCustomerParameters.addressDistrict != null ? (Guid?)Guid.Parse(individualCustomerParameters.addressDistrict.value) : null,
                            individualCustomerId = individualCustomerParameters.individualCustomerId.Value,
                            addressDetail = individualCustomerParameters.addressDetail
                        });
                    }

                }
            }

            if (individualCustomerParameters.markettingPermissions.HasValue)
            {
                MarkettingPermissionsRepository markettingPermissionsRepository = new MarkettingPermissionsRepository(this.OrgService);
                var markettingPermission = markettingPermissionsRepository.getMarkettingPermissionByContactId(individualCustomerParameters.individualCustomerId.Value);
                var marketingPermissionData = new MarketingPermissionMapper().createMarketingPermissionData(markettingPermission);
                marketingPermissionData.contactId = individualCustomerParameters.individualCustomerId;
                marketingPermissionData.channelCode = individualCustomerParameters.distributionChannelCode;
                marketingPermissionData.operationType = (int)rnt_marketingpermissions_rnt_operationtype.CustomerUpdate;
                marketingPermissionData.marketingPermissionId = null;
                MarkettingPermissionsBL markettingPermissionsBL = new MarkettingPermissionsBL(this.OrgService);
                if (markettingPermission != null)
                {
                    // get any field for value check
                    //var kvkPermission = marketingPermissionData.kvkkPermission;
                    //if (individualCustomerParameters.markettingPermissions.Value != kvkPermission)
                    //{
                    markettingPermissionsBL.deactiveMarkettingPermissions(markettingPermission.Id);
                    markettingPermissionsBL.createMarkettingPermissions(marketingPermissionData);
                    //}
                }
                else
                {
                    markettingPermissionsBL.createMarkettingPermissions(marketingPermissionData);
                }
            }

            this.Trace("after individual address");

            return new IndividualCustomerUpdateResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }
        public IndividualCustomerSearchResponse SearchCustomer(String text, String[] columns)
        {
            IndividualCustomerRepository repository = new IndividualCustomerRepository(this.OrgService);

            return new IndividualCustomerSearchResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess(),
                IndividualCustomerData = repository.CustomerSearchByFetchXML(text, columns)
            };

        }
        public List<IndividualCustomerData> getCorporateEmployeesInformation(string corporateCustomerId)
        {
            IndividualCustomerRepository repository = new IndividualCustomerRepository(this.OrgService);
            var res = repository.getCorporateEmployeesInformationByFetchXML(corporateCustomerId);

            List<IndividualCustomerData> data = new List<IndividualCustomerData>();
            foreach (var item in res.Entities)
            {
                data.Add(new IndividualCustomerData
                {
                    FirstName = item.GetAttributeValue<String>("firstname"),
                    LastName = item.GetAttributeValue<String>("lastname"),
                    GovernmentId = item.GetAttributeValue<String>("governmentid"),
                    MobilePhone = item.GetAttributeValue<String>("mobilephone"),
                    Email = item.GetAttributeValue<String>("emailaddress1"),
                    IndividualCustomerId = item.Id
                });
            }
            return data;
        }
        public IndividualCustomerDetailInformationResponse getIndividualCustomerDetail(IndividualCustomerDetailInformationParameters parameters)
        {
            try
            {
                IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);

                var columns = new string[] { "firstname",
                                         "lastname",
                                         "rnt_citizenshipid",
                                         "birthdate",
                                         "governmentid",
                                         "rnt_passportnumber",
                                         "rnt_isturkishcitizen",
                                         "emailaddress1",
                                         "mobilephone",
                                         "rnt_drivinglicenseclasscode",
                                         "rnt_drivinglicensedate",
                                         "rnt_drivinglicensenumber",
                                         "rnt_drivinglicenseplace",
                                         "rnt_drivinglicensecountryid",
                                         "gendercode",
                                         "rnt_dialcode",
                                         "rnt_findekspoint"
                                       };
                //todo check result parameter.
                var result = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(parameters.individualCustomerId, columns);

                IndividualAddressRepository individualAddressRepository = new IndividualAddressRepository(this.OrgService);
                //todo default address check
                var defaultAdress = individualAddressRepository.getDefaultIndividualAddressByCustomerId(parameters.individualCustomerId);
                var customerCorporateRelation = this.getCustomerCorporateRelation(parameters);

                IndividualCustomerDetailData data = new IndividualCustomerDetailData
                {
                    individualCustomerId = result.Id,
                    firstName = result.GetAttributeValue<string>("firstname"),
                    lastName = result.GetAttributeValue<string>("lastname"),
                    nationalityId = result.Attributes.Contains("rnt_citizenshipid") ?
                                    (Guid?)result.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id :
                                    null,
                    nationalityIdName = result.Attributes.Contains("rnt_citizenshipid") ?
                                      result.GetAttributeValue<EntityReference>("rnt_citizenshipid").Name :
                                      null,
                    birthDate = result.GetAttributeValue<DateTime>("birthdate"),
                    governmentId = result.GetAttributeValue<string>("governmentid"),
                    isTurkishCitizen = result.GetAttributeValue<bool>("rnt_isturkishcitizen"),
                    //passportIssueDate = result.GetAttributeValue<DateTime>("rnt_passportissuedate"),
                    passportNumber = result.GetAttributeValue<string>("rnt_passportnumber"),
                    email = result.GetAttributeValue<string>("emailaddress1"),
                    mobilePhone = result.GetAttributeValue<string>("mobilephone"),
                    dialCode = result.GetAttributeValue<string>("rnt_dialcode"),
                    drivingLicenseClass = result.Attributes.Contains("rnt_drivinglicenseclasscode") ?
                                      (int?)result.GetAttributeValue<OptionSetValue>("rnt_drivinglicenseclasscode").Value :
                                      null,
                    drivingLicenseDate = result.GetAttributeValue<DateTime>("rnt_drivinglicensedate"),
                    drivingLicenseNumber = result.GetAttributeValue<string>("rnt_drivinglicensenumber"),
                    drivingLicensePlace = result.GetAttributeValue<string>("rnt_drivinglicenseplace"),
                    drivingLicenseCountry = result.Attributes.Contains("rnt_drivinglicensecountryid") ?
                                    (Guid?)result.GetAttributeValue<EntityReference>("rnt_drivinglicensecountryid").Id :
                                    null,
                    findeksPoint = result.Attributes.Contains("rnt_findekspoint") ? result.GetAttributeValue<int>("rnt_findekspoint") : 0,
                    gender = result.Attributes.Contains("gendercode") ?
                                      (int?)result.GetAttributeValue<OptionSetValue>("gendercode").Value :
                                      null,
                    addressCityId = defaultAdress != null && defaultAdress.Attributes.Contains("rnt_cityid") ?
                                    Convert.ToString(defaultAdress.GetAttributeValue<EntityReference>("rnt_cityid").Id) :
                                    null,
                    addressCity = defaultAdress != null && defaultAdress.Attributes.Contains("rnt_cityid") ?
                                   defaultAdress.GetAttributeValue<EntityReference>("rnt_cityid").Name :
                                    null,
                    addressCountryId = defaultAdress != null && defaultAdress.Attributes.Contains("rnt_countryid") ?
                                    Convert.ToString(defaultAdress.GetAttributeValue<EntityReference>("rnt_countryid").Id) :
                                    null,
                    addressCountry = defaultAdress != null && defaultAdress.Attributes.Contains("rnt_countryid") ?
                                     defaultAdress.GetAttributeValue<EntityReference>("rnt_countryid").Name :
                                    null,
                    addressDistrictId = defaultAdress != null && defaultAdress.Attributes.Contains("rnt_districtid") ?
                                    Convert.ToString(defaultAdress.GetAttributeValue<EntityReference>("rnt_districtid").Id) :
                                    null,
                    addressDistrict = defaultAdress != null && defaultAdress.Attributes.Contains("rnt_districtid") ?
                                    defaultAdress.GetAttributeValue<EntityReference>("rnt_districtid").Name :
                                    null,
                    addressDetail = defaultAdress != null && defaultAdress.Attributes.Contains("rnt_addressdetail") ?
                                    defaultAdress.GetAttributeValue<string>("rnt_addressdetail") :
                                    null,
                    individualAddressId = defaultAdress != null ? (Guid?)defaultAdress.Id : null,

                };

                return new IndividualCustomerDetailInformationResponse
                {
                    IndividualCustomerDetailData = data,
                    IndividualCustomerCorporateRelationData = customerCorporateRelation,
                    ResponseResult = ResponseResult.ReturnSuccess()
                };
            }
            catch (Exception ex)
            {
                return new IndividualCustomerDetailInformationResponse
                {
                    ResponseResult = ResponseResult.ReturnError(ex.Message)
                };
            }
        }
        public IndividualCustomerContractRelationData getIndividualCustomerDetailContractRelationDataById(Guid individualCustomerId)
        {
            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);

            var columns = new string[] { "firstname",
                                         "lastname",
                                         "rnt_citizenshipid",
                                         "birthdate",
                                         "governmentid",
                                         "rnt_passportnumber",
                                         "rnt_isturkishcitizen",
                                         "emailaddress1",
                                         "mobilephone",
                                         "rnt_drivinglicenseclasscode",
                                         "rnt_drivinglicensedate",
                                         "rnt_drivinglicensenumber",
                                         "rnt_drivinglicenseplace",
                                         "gendercode",
                                         "rnt_dialcode",
                                         "rnt_findekspoint"
                                       };
            //todo check result parameter.
            var result = individualCustomerRepository.retrieveById("contact", individualCustomerId, columns);

            IndividualCustomerContractRelationData data = new IndividualCustomerContractRelationData
            {
                individualCustomerId = result.Id,
                firstName = result.GetAttributeValue<string>("firstname"),
                lastName = result.GetAttributeValue<string>("lastname"),
                nationalityId = result.Attributes.Contains("rnt_citizenshipid") ?
                                (Guid?)result.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id :
                                null,
                birthDate = result.GetAttributeValue<DateTime>("birthdate"),
                governmentId = result.GetAttributeValue<string>("governmentid"),
                isTurkishCitizen = result.GetAttributeValue<bool>("rnt_isturkishcitizen"),
                //passportIssueDate = result.GetAttributeValue<DateTime>("rnt_passportissuedate"),
                passportNumber = result.GetAttributeValue<string>("rnt_passportnumber"),
                email = result.GetAttributeValue<string>("emailaddress1"),
                mobilePhone = result.GetAttributeValue<string>("mobilephone"),
                dialCode = result.GetAttributeValue<string>("rnt_dialcode"),
                drivingLicenseClass = result.Attributes.Contains("rnt_drivinglicenseclasscode") ?
                                  (int?)result.GetAttributeValue<OptionSetValue>("rnt_drivinglicenseclasscode").Value :
                                  null,
                drivingLicenseDate = result.GetAttributeValue<DateTime>("rnt_drivinglicensedate"),
                drivingLicenseNumber = result.GetAttributeValue<string>("rnt_drivinglicensenumber"),
                drivingLicensePlace = result.GetAttributeValue<string>("rnt_drivinglicenseplace"),
                findeksPoint = result.Attributes.Contains("rnt_findekspoint") ? result.GetAttributeValue<int>("rnt_findekspoint") : 0
            };

            return data;
        }
        public IndividualCustomerDetailData getIndividualCustomerInformationForValidation(Guid individualCustomerId)
        {
            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);

            var columns = new string[] { "governmentid", "birthdate", "rnt_drivinglicensedate", "rnt_passportnumber", "rnt_isturkishcitizen", "rnt_segmentcode" };

            var result = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(individualCustomerId, columns);
            var isTurkishCitizen = result.GetAttributeValue<bool>("rnt_isturkishcitizen");
            IndividualCustomerDetailData data = new IndividualCustomerDetailData
            {
                individualCustomerId = result.Id,
                governmentId = isTurkishCitizen ? result.GetAttributeValue<string>("governmentid") : result.GetAttributeValue<string>("rnt_passportnumber"),
                birthDate = result.GetAttributeValue<DateTime>("birthdate"),
                drivingLicenseDate = result.GetAttributeValue<DateTime>("rnt_drivinglicensedate"),
                customerSegment = result.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value
            };
            return data;
        }

        public ValidationResponse checkContractStatus(Guid customerId, int langId)
        {
            string[] columns = new string[] { "statuscode" };

            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
            var contacts = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(customerId, columns);
            OptionSetValue statusCode = contacts.GetAttributeValue<OptionSetValue>("statuscode");
            if (statusCode != null)
            {
                XrmHelper xrmHelper = new XrmHelper(this.OrgService);
                if (statusCode.Value == (int)Contact_StatusCode.FraudForCreditCard)
                {
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("FraudCustomerForCreditCard", langId, this.reservationXmlPath);
                    return new ValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }
                else if (statusCode.Value == (int)Contact_StatusCode.WaitingForAnonymous)
                {
                    var message = xrmHelper.GetXmlTagContentByGivenLangId("WaitingForAnonymous", langId, this.reservationXmlPath);
                    return new ValidationResponse
                    {
                        ResponseResult = ResponseResult.ReturnError(message)
                    };
                }

            }
            return new ValidationResponse
            {
                ResponseResult = ResponseResult.ReturnSuccess()
            };
        }

        public List<IndividualCustomerCorporateRelationData> getCustomerCorporateRelation(IndividualCustomerDetailInformationParameters parameters)
        {
            ConnectionRepository connectionRepository = new ConnectionRepository(this.OrgService);
            var result = connectionRepository.getConnectionsByIndividualCustomerId(parameters.individualCustomerId);

            List<IndividualCustomerCorporateRelationData> data = new List<IndividualCustomerCorporateRelationData>();
            foreach (var item in result)
            {
                IndividualCustomerCorporateRelationData d = new IndividualCustomerCorporateRelationData
                {
                    companyName = item.GetAttributeValue<EntityReference>("rnt_accountid").Name,
                    corporateCustomerId = item.GetAttributeValue<EntityReference>("rnt_accountid").Id,
                    relationType = item.Attributes.Contains("rnt_relationcode") ? item.GetAttributeValue<OptionSetValue>("rnt_relationcode").Value : 0,
                    roleType = GlobalEnums.RoleType.C.ToString()
                };
                data.Add(d);
            }
            //add individual record itself
            IndividualCustomerCorporateRelationData itself = new IndividualCustomerCorporateRelationData
            {
                individualCustomerId = parameters.individualCustomerId,
                individualCustomerName = parameters.individualCustomerName,
                roleType = GlobalEnums.RoleType.I.ToString()
            };
            data.Add(itself);

            return data;
        }
        public Guid? getExistingCustomerIdByGovernmentIdOrPassportNumber(string _id)
        {
            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
            return individualCustomerRepository.getExistingCustomerIdByGovernmentIdOrPassportNumber(_id, new string[] { });
        }
        public void createFindeksTransactionHitory(Entity contact, int oldValue, int newValue)
        {
            Entity e = new Entity("rnt_findekstransactionhistory");
            e["rnt_newfindeskvalue"] = newValue;
            e["rnt_oldfindeskvalue"] = oldValue;
            e["rnt_contactid"] = new EntityReference("contact", contact.Id);
            var name = contact.GetAttributeValue<string>("firstname") + " - " + contact.GetAttributeValue<string>("lastname");
            name += " - " + e.GetAttributeValue<DateTime>("modifiedon");
            e["rnt_name"] = name;
            this.OrgService.Create(e);
        }

        public Guid createCustomer(IndividualCustomerCreateParameter individualCustomerParameters)
        {
            Entity e = new Entity("contact");
            e.Attributes.Add("firstname", CommonHelper.removeWhitespacesForTCKNValidation(individualCustomerParameters.firstName).upperCaseTurkish());
            e.Attributes.Add("lastname", CommonHelper.removeWhitespacesForTCKNValidation(individualCustomerParameters.lastName).upperCaseTurkish());
            if (!string.IsNullOrEmpty(individualCustomerParameters.email))
            {
                e.Attributes.Add("emailaddress1", individualCustomerParameters.email.removeEmptyCharacters());
            }
            e.Attributes.Add("rnt_citizenshipid", new EntityReference("rnt_country", new Guid(individualCustomerParameters.nationality.value)));
            if (individualCustomerParameters.drivingLicenseCountry != null && !string.IsNullOrWhiteSpace(individualCustomerParameters.drivingLicenseCountry.value))
            {
                e.Attributes.Add("rnt_drivinglicensecountryid", new EntityReference("rnt_country", new Guid(individualCustomerParameters.drivingLicenseCountry.value)));
            }
            e.Attributes.Add("rnt_drivinglicensedate", individualCustomerParameters.drivingLicenseDate);
            if (individualCustomerParameters.distributionChannelCode != (int)rnt_DistributionChannelCode.Mobile)
            {
                if (individualCustomerParameters.gender != null)
                    e.Attributes.Add("gendercode", new OptionSetValue(Convert.ToInt32(individualCustomerParameters.gender.value)));

                e.Attributes.Add("rnt_drivinglicensenumber", individualCustomerParameters.drivingLicenseNumber);
                if (individualCustomerParameters.drivingLicenseClass != null)
                    e.Attributes.Add("rnt_drivinglicenseclasscode", new OptionSetValue(Convert.ToInt32(individualCustomerParameters.drivingLicenseClass.value)));

                if (!string.IsNullOrEmpty(individualCustomerParameters.drivingLicensePlace))
                {
                    e.Attributes.Add("rnt_drivinglicenseplace", individualCustomerParameters.drivingLicensePlace.removeEmptyCharacters());
                }
            }
            e.Attributes.Add("rnt_distributionchannelcode", new OptionSetValue(individualCustomerParameters.distributionChannelCode));
            e.Attributes.Add("mobilephone", individualCustomerParameters.mobilePhone.removeEmptyCharacters());
            this.Trace("turkishcitizen" + individualCustomerParameters.isTurkishCitizen);
            if (individualCustomerParameters.isTurkishCitizen)
            {
                this.Trace("governmentid" + individualCustomerParameters.governmentId);
                e.Attributes.Add("governmentid", individualCustomerParameters.governmentId.removeEmptyCharacters());
                //e.Attributes.Add("rnt_passportissuedate", null);
                e.Attributes.Add("rnt_passportnumber", null);
            }
            else
            {
                e.Attributes.Add("governmentid", null);
                e.Attributes.Add("rnt_passportnumber", individualCustomerParameters.passportNumber.removeEmptyCharacters());
                //e.Attributes.Add("rnt_passportissuedate", individualCustomerParameters.passportIssueDate);
            }

            e.Attributes.Add("rnt_dialcode", individualCustomerParameters.dialCode.value);
            e.Attributes.Add("rnt_isturkishcitizen", individualCustomerParameters.isTurkishCitizen);
            e.Attributes.Add("birthdate", individualCustomerParameters.birthDate);
            e.Attributes.Add("rnt_findekspoint", individualCustomerParameters.findeksPoint != null ? individualCustomerParameters.findeksPoint : 0);

            //todo: change DistributionChannelCode branch to mobile
            if ((int)rnt_DistributionChannelCode.Web == individualCustomerParameters.distributionChannelCode || (int)rnt_DistributionChannelCode.Mobile == individualCustomerParameters.distributionChannelCode)
            {
                this.Trace("set mail and password");
                e["rnt_webemailaddress"] = individualCustomerParameters.email;
                e["rnt_webpassword"] = individualCustomerParameters.password;
            }
            return this.OrgService.Create(e);
        }

        public Guid createIndividualCustomerForAdditionalDrivers(IndividualCustomerAdditionalDriverParameters parameters)
        {
            Entity e = new Entity("contact");
            e.Attributes.Add("firstname", parameters.firstName);
            e.Attributes.Add("lastname", parameters.lastName);
            e.Attributes.Add("rnt_drivinglicensedate", parameters.drivingLicenseDate.AddMinutes(StaticHelper.offset));
            e.Attributes.Add("rnt_dialcode", parameters.dialCode);
            e.Attributes.Add("mobilephone", parameters.mobilePhone);
            e.Attributes.Add("birthdate", parameters.birthDate.AddMinutes(StaticHelper.offset));

            if (parameters.isTurkishCitizen)
            {
                this.Trace("governmentid" + parameters.governmentId);
                e.Attributes.Add("governmentid", parameters.governmentId);
            }
            else
            {
                e.Attributes.Add("rnt_passportnumber", parameters.passportNumber);
            }
            e["rnt_citizenshipid"] = new EntityReference("rnt_country", parameters.nationalityId);
            e["rnt_distributionchannelcode"] = new OptionSetValue((int)rnt_DistributionChannelCode.Branch);

            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid");

            if (parameters.nationalityId == new Guid(turkeyGuid.Split(';')[0]))
            {
                e["rnt_isturkishcitizen"] = true;
            }
            else
            {
                e["rnt_isturkishcitizen"] = false;
            }
            return this.OrgService.Create(e);
        }
        public void updateIndividualCustomerForAdditionalDrivers(IndividualCustomerAdditionalDriverParameters parameters)
        {
            Entity e = new Entity("contact");

            e.Id = parameters.contactId.Value;
            e.Attributes.Add("firstname", parameters.firstName);
            e.Attributes.Add("lastname", parameters.lastName);
            e.Attributes.Add("rnt_drivinglicensedate", parameters.drivingLicenseDate.AddMinutes(StaticHelper.offset));
            e.Attributes.Add("rnt_dialcode", parameters.dialCode);
            e.Attributes.Add("mobilephone", parameters.mobilePhone);
            e.Attributes.Add("birthdate", parameters.birthDate.AddMinutes(StaticHelper.offset));

            if (parameters.isTurkishCitizen)
            {
                this.Trace("governmentid" + parameters.governmentId);
                e.Attributes.Add("governmentid", parameters.governmentId);
                ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
                var turkishCitizenId = configurationBL.GetConfigurationByName("TurkeyGuid").Split(';')[0];
                e.Attributes.Add("rnt_citizenshipid", new EntityReference("rnt_country", Guid.Parse(turkishCitizenId)));
                e["rnt_isturkishcitizen"] = true;
            }
            else
            {
                e.Attributes.Add("rnt_passportnumber", parameters.passportNumber);
                e["rnt_isturkishcitizen"] = false;
            }

            this.OrgService.Update(e);
        }



        public DateTime RequestIndividualCustomerAnonymization(Guid contactId, DateTime expireRequestDate, int langId)
        {
            XrmHelper xrmHelper = new XrmHelper(this.OrgService);

            IndividualCustomerRepository individualCustomerRepository = new IndividualCustomerRepository(this.OrgService);
            Entity contact = individualCustomerRepository.getIndividualCustomerByIdWithGivenColumns(contactId, new string[] { "fullname" });
            string fullName = contact.GetAttributeValue<string>("fullname");
            ReservationBL reservationBL = new ReservationBL(this.OrgService);
            bool reservationCheck = reservationBL.checkActiveReservationForCustomer(contactId);
            if (!reservationCheck)
            {
                throw new Exception(string.Format(xrmHelper.GetXmlTagContentByGivenLangId("ReservationCheckForAnonymization", langId, this.mobileXmlPath), fullName));
            }


            bool contractDebitCheck = checkDebitAmount(contactId);
            if (!contractDebitCheck)
            {
                throw new Exception(string.Format(xrmHelper.GetXmlTagContentByGivenLangId("IndiviualCustomerDebitForAnonymization", langId, this.mobileXmlPath), fullName));
            }


            ContractBL contractBL = new ContractBL(this.OrgService);
            bool contractCheck = contractBL.checkActiveContractForCustomer(contactId);
            if (!contractCheck)
            {
                throw new Exception(string.Format(xrmHelper.GetXmlTagContentByGivenLangId("ContractCheckForAnonymization", langId, this.mobileXmlPath), fullName));
            }

            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            int expireDay = systemParameterBL.getCustomerExpireDay();
            if (expireDay <= 0)
            {
                expireDay = 1;
            }
            DateTime expireDate = expireRequestDate.AddDays(expireDay);
            Entity updateContact = new Entity("contact", contactId);
            updateContact.Attributes["rnt_expirerequest"] = expireRequestDate;
            updateContact.Attributes["rnt_expiredate"] = expireDate;

            updateContact.Attributes["statuscode"] = new OptionSetValue((int)Contact_StatusCode.WaitingForAnonymous);
            this.OrgService.Update(contact);
            return expireDate.Date;
        }

        public void CancelIndividualCustomerAnonymization(Guid contactId)
        {
            Entity contact = new Entity("contact", contactId);
            contact.Attributes["rnt_expirerequest"] = null;
            contact.Attributes["rnt_expiredate"] = null;
            contact.Attributes["statuscode"] = new OptionSetValue((int)Contact_StatusCode.Active);
            this.OrgService.Update(contact);
        }
    }
}
