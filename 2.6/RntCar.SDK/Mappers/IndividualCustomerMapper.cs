using Microsoft.Xrm.Sdk;
using Newtonsoft.Json;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using RntCar.ClassLibrary._Web.IndividualCustomer;
using RntCar.ClassLibrary.Reservation;
using RntCar.SDK.Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RntCar.SDK.Mappers
{
    public class IndividualCustomerMapper
    {
        public List<IndividualCustomerMarketingPermissionData_Web> buildCustomerMarketingPermissionData(List<Entity> datas)
        {
            var i = 1;
            List<IndividualCustomerMarketingPermissionData_Web> returnData = new List<IndividualCustomerMarketingPermissionData_Web>();
            datas.ForEach(ind =>
            {

                returnData.Add(new IndividualCustomerMarketingPermissionData_Web
                {
                    permissionDate = ind.GetAttributeValue<DateTime>("createdon"),
                    allowEmail = ind.GetAttributeValue<bool>("rnt_allowemails"),
                    allowNotification = ind.GetAttributeValue<bool>("rnt_allownotification"),
                    allowSMS = ind.GetAttributeValue<bool>("rnt_allowsms"),
                    permissionChannel = ind.Contains("rnt_permissionchannelcode") ? (int?)ind.GetAttributeValue<OptionSetValue>("rnt_permissionchannelcode").Value : null,
                    birthDate = ind.Contains("ac.birthdate") ? (DateTime?)ind.GetAttributeValue<AliasedValue>("ac.birthdate").Value : null,
                    customerNumber = (string)ind.GetAttributeValue<AliasedValue>("ac.rnt_customernumber").Value,
                    emailaddress = ind.Contains("ac.emailaddress1") ? (string)ind.GetAttributeValue<AliasedValue>("ac.emailaddress1").Value : null,
                    findeksPoint = (int)ind.GetAttributeValue<AliasedValue>("ac.rnt_findekspoint").Value,
                    firstName = (string)ind.GetAttributeValue<AliasedValue>("ac.firstname").Value,
                    lastName = (string)ind.GetAttributeValue<AliasedValue>("ac.lastname").Value,
                    genderCode = ind.Contains("ac.gendercode") ? (int?)((OptionSetValue)(ind.GetAttributeValue<AliasedValue>("ac.gendercode").Value)).Value : null,
                    isTurkishCitizen = (bool)ind.GetAttributeValue<AliasedValue>("ac.rnt_isturkishcitizen").Value,
                    licenseDate = ind.Contains("ac.rnt_drivinglicensedate") ? (DateTime?)ind.GetAttributeValue<AliasedValue>("ac.rnt_drivinglicensedate").Value : null,
                    mobilePhone = ind.Contains("ac.mobilephone") ? (string)ind.GetAttributeValue<AliasedValue>("ac.mobilephone").Value : null,
                    segmentCode = ind.Contains("ac.rnt_segmentcode") ? (int?)(((OptionSetValue)(ind.GetAttributeValue<AliasedValue>("ac.rnt_segmentcode").Value)).Value) : null,
                    identityKey = ind.Contains("ac.governmentid") ? (string)ind.GetAttributeValue<AliasedValue>("ac.governmentid").Value : (string)ind.GetAttributeValue<AliasedValue>("ac.rnt_passportnumber").Value,
                    createdon = (DateTime)ind.GetAttributeValue<AliasedValue>("ac.createdon").Value,
                    lastRentalOffice = ind.Contains("ac.rnt_lastrentalofficeid") ? ((EntityReference)(ind.GetAttributeValue<AliasedValue>("ac.rnt_lastrentalofficeid").Value)).Name : string.Empty,
                });
                i++;
            });

            return returnData;
        }
        public IndividualCustomerData_Web createWebIndividualCustomerData(Entity individualCustomer, int langId)
        {
            var licenseCodes = XrmHelper.getEnumsAsOptionSetModelByLangId("Contact_rnt_drivinglicenseclasscode", langId);
            var segments = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_CustomerSegment", langId);
            var drivingLicense = individualCustomer.GetAttributeValue<OptionSetValue>("rnt_drivinglicenseclasscode") == null ?
                                  (int)Contact_rnt_drivinglicenseclasscode.B :
                                  individualCustomer.GetAttributeValue<OptionSetValue>("rnt_drivinglicenseclasscode").Value;

            string email = null;

            if (!string.IsNullOrEmpty(individualCustomer.GetAttributeValue<string>("rnt_webemailaddress")))
            {
                email = individualCustomer.GetAttributeValue<string>("rnt_webemailaddress");
            }
            else if (!string.IsNullOrEmpty(individualCustomer.GetAttributeValue<string>("emailaddress1")))
            {
                email = individualCustomer.GetAttributeValue<string>("emailaddress1");
            }
            IndividualCustomerData_Web individualCustomerData_Web = new IndividualCustomerData_Web
            {
                individualCustomerId = individualCustomer.Id,
                birthDate = individualCustomer.GetAttributeValue<DateTime>("birthdate"),
                customerId = individualCustomer.GetAttributeValue<string>("rnt_customerexternalid"),
                emailAddress = email,
                firstName = individualCustomer.GetAttributeValue<string>("firstname"),
                governmentId = individualCustomer.Attributes.Contains("governmentid") ?
                               individualCustomer.GetAttributeValue<string>("governmentid") :
                               individualCustomer.GetAttributeValue<string>("rnt_passportnumber"),
                lastName = individualCustomer.GetAttributeValue<string>("lastname"),
                licenseClass = drivingLicense,
                licenseClassName = licenseCodes.Where(p => p.value.Equals(drivingLicense)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                licenseDate = individualCustomer.GetAttributeValue<DateTime>("rnt_drivinglicensedate"),
                licenseNumber = individualCustomer.GetAttributeValue<string>("rnt_drivinglicensenumber"),
                licensePlace = individualCustomer.GetAttributeValue<string>("rnt_drivinglicenseplace"),
                mobilePhone = individualCustomer.GetAttributeValue<string>("mobilephone"),
                dialCode = individualCustomer.GetAttributeValue<string>("rnt_dialcode"),
                genderCode = individualCustomer.GetAttributeValue<OptionSetValue>("gendercode") == null ?
               (int)Contact_GenderCode.Male :
                individualCustomer.GetAttributeValue<OptionSetValue>("gendercode").Value,
                citizenShipId = individualCustomer.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id,
                citizenShipName = individualCustomer.GetAttributeValue<EntityReference>("rnt_citizenshipid").Name,
                segmentCode = individualCustomer.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value,
                segmentCodeName = segments.Where(p => p.value.Equals(individualCustomer.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
            };
            return individualCustomerData_Web;
        }

        public IndividualCustomerData_Mobile createMobileIndividualCustomerData(Entity individualCustomer, int langId)
        {
            var licenseCodes = XrmHelper.getEnumsAsOptionSetModelByLangId("Contact_rnt_drivinglicenseclasscode", langId);
            var segments = XrmHelper.getEnumsAsOptionSetModelByLangId("rnt_CustomerSegment", langId);
            var isNotMobileUser = individualCustomer.Attributes.Contains("rnt_distributionchannelcode") ? individualCustomer.GetAttributeValue<OptionSetValue>("rnt_distributionchannelcode").Value != (int)rnt_DistributionChannelCode.Mobile : true;

            IndividualCustomerData_Mobile individualCustomerData_Mobile = new IndividualCustomerData_Mobile
            {
                individualCustomerId = individualCustomer.Id,
                birthDate = individualCustomer.GetAttributeValue<DateTime>("birthdate"),
                customerId = individualCustomer.GetAttributeValue<string>("rnt_customerexternalid"),
                emailAddress = individualCustomer.GetAttributeValue<string>("rnt_webemailaddress"),
                firstName = individualCustomer.GetAttributeValue<string>("firstname"),
                governmentId = individualCustomer.Attributes.Contains("governmentid") ?
                               individualCustomer.GetAttributeValue<string>("governmentid") :
                               individualCustomer.GetAttributeValue<string>("rnt_passportnumber"),
                lastName = individualCustomer.GetAttributeValue<string>("lastname"),
                licenseClass = isNotMobileUser ? individualCustomer.GetAttributeValue<OptionSetValue>("rnt_drivinglicenseclasscode")?.Value : null,
                licenseClassName = isNotMobileUser ? licenseCodes.Where(p => p.value.Equals(individualCustomer.GetAttributeValue<OptionSetValue>("rnt_drivinglicenseclasscode").Value)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString() : null,
                licenseDate = individualCustomer.GetAttributeValue<DateTime>("rnt_drivinglicensedate"),
                licenseNumber = isNotMobileUser ? individualCustomer.GetAttributeValue<string>("rnt_drivinglicensenumber") : null,
                licensePlace = isNotMobileUser ? individualCustomer.GetAttributeValue<string>("rnt_drivinglicenseplace") : null,
                mobilePhone = individualCustomer.GetAttributeValue<string>("mobilephone"),
                genderCode = isNotMobileUser ? individualCustomer.GetAttributeValue<OptionSetValue>("gendercode")?.Value : null,
                citizenShipId = individualCustomer.GetAttributeValue<EntityReference>("rnt_citizenshipid").Id,
                citizenShipName = individualCustomer.GetAttributeValue<EntityReference>("rnt_citizenshipid").Name,
                segmentCode = individualCustomer.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value,
                segmentCodeName = segments.Where(p => p.value.Equals(individualCustomer.GetAttributeValue<OptionSetValue>("rnt_segmentcode").Value)).FirstOrDefault()?.label.removeAlphaNumericCharactersFromString(),
                statusCode = individualCustomer.GetAttributeValue<OptionSetValue>("statuscode").Value
            };

            return individualCustomerData_Mobile;
        }

        public ReservationCustomerParameters buildReservationCustomerParameters(ReservationCustomerParameters_Web reservationCustomerParameters_Web)
        {
            var c = new ReservationCustomerParameters
            {

                corporateCustomerId = reservationCustomerParameters_Web.corporateCustomerId,
                contactId = reservationCustomerParameters_Web.individualCustomerId,
                customerType = reservationCustomerParameters_Web.corporateCustomerId.HasValue && reservationCustomerParameters_Web.corporateCustomerId.Value != Guid.Empty ?
                              (int)GlobalEnums.CustomerType.Corporate :
                              (int)GlobalEnums.CustomerType.Individual,
            };
            if (reservationCustomerParameters_Web.invoiceAddress != null)
            {
                c.invoiceAddress = new InvoiceAddressMapper().buildInvoiceAddressData(reservationCustomerParameters_Web.invoiceAddress, reservationCustomerParameters_Web.individualCustomerId);
            }
            return c;
        }
        public ReservationCustomerParameters buildReservationCustomerParameters(ReservationCustomerParameters_Mobile reservationCustomerParameters_mobile)
        {
            var c = new ReservationCustomerParameters
            {
                corporateCustomerId = reservationCustomerParameters_mobile.corporateCustomerId,
                contactId = reservationCustomerParameters_mobile.individualCustomerId,
                customerType = reservationCustomerParameters_mobile.corporateCustomerId.HasValue && reservationCustomerParameters_mobile.corporateCustomerId.Value != Guid.Empty ?
                              (int)GlobalEnums.CustomerType.Corporate :
                              (int)GlobalEnums.CustomerType.Individual,
            };
            if (reservationCustomerParameters_mobile.invoiceAddress != null)
            {
                c.invoiceAddress = new InvoiceAddressMapper().buildInvoiceAddressData(reservationCustomerParameters_mobile.invoiceAddress, reservationCustomerParameters_mobile.individualCustomerId);
            }
            return c;
        }

        public ReservationCustomerParameters buildReservationCustomerParameters(ReservationCustomerParameters_Broker reservationCustomerParameters_Broker, Guid corporateCustomerId, ReservationPriceParameter_Broker priceParameters)
        {
            int customerType = (priceParameters.paymentMethodCode == (int)rnt_PaymentMethodCode.PayBroker)
                ? (int)GlobalEnums.CustomerType.Broker
                : (int)GlobalEnums.CustomerType.Agency;

            return new ReservationCustomerParameters
            {
                corporateCustomerId = corporateCustomerId,
                customerType = customerType,
                dummyContactId = JsonConvert.SerializeObject(reservationCustomerParameters_Broker.dummyContactData)
            };
        }

        public IndividualCustomerCreateParameter buildCreateIndividualCustomerParameter(IndividualCustomerCreateParameter_Web individualCustomerCreateParameter_Web, bool isTurkishCitizen)
        {

            SelectModel _addressCity = null;
            if (individualCustomerCreateParameter_Web.individualAddressInformation.cityId.HasValue)
            {
                _addressCity = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Web.individualAddressInformation.cityId.Value),
                    label = individualCustomerCreateParameter_Web.individualAddressInformation.cityName
                };
            }
            SelectModel _addressDistrict = null;
            if (individualCustomerCreateParameter_Web.individualAddressInformation.districtId.HasValue)
            {
                _addressDistrict = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Web.individualAddressInformation.districtId.Value),
                    label = individualCustomerCreateParameter_Web.individualAddressInformation.districtName
                };
            }

            SelectModel _licenseCountry = null;
            _licenseCountry = new SelectModel
            {
                value = string.Empty,
                label = string.Empty
            };

            var _birthDate = DateTime.ParseExact(individualCustomerCreateParameter_Web.birthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var _licenseDate = DateTime.ParseExact(individualCustomerCreateParameter_Web.licenseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            return new IndividualCustomerCreateParameter
            {
                addressCity = _addressCity,
                addressDistrict = _addressDistrict,
                addressCountry = new SelectModel
                {
                    label = individualCustomerCreateParameter_Web.individualAddressInformation.countryName,
                    value = Convert.ToString(individualCustomerCreateParameter_Web.individualAddressInformation.countryId.Value)
                },
                addressDetail = individualCustomerCreateParameter_Web.individualAddressInformation.addressDetail,
                birthDate = _birthDate,
                distributionChannelCode = (int)rnt_DistributionChannelCode.Web,
                firstName = individualCustomerCreateParameter_Web.firstName,
                lastName = individualCustomerCreateParameter_Web.lastName,
                email = individualCustomerCreateParameter_Web.emailAddress,
                password = individualCustomerCreateParameter_Web.password,
                findeksPoint = 0,
                drivingLicenseClass = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Web.licenseClass)
                },
                gender = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Web.genderCode)
                },
                drivingLicenseDate = _licenseDate,
                drivingLicensePlace = individualCustomerCreateParameter_Web.licensePlace,
                drivingLicenseNumber = individualCustomerCreateParameter_Web.licenseNumber,
                drivingLicenseCountry = _licenseCountry,
                mobilePhone = individualCustomerCreateParameter_Web.mobilePhone,
                nationality = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Web.citizenShipId)
                },
                isCallCenter = false,
                isAlsoInvoiceAddress = individualCustomerCreateParameter_Web.isAlsoInvoiceAddress,
                governmentId = isTurkishCitizen ? individualCustomerCreateParameter_Web.governmentId : null,
                passportNumber = isTurkishCitizen ? null : individualCustomerCreateParameter_Web.governmentId,
                isTurkishCitizen = isTurkishCitizen,
                dialCode = new SelectModel
                {
                    value = individualCustomerCreateParameter_Web.dialCode
                },
                markettingPermissions = individualCustomerCreateParameter_Web.allowMarketingPermissions
            };
        }

        public IndividualCustomerCreateParameter buildCreateIndividualCustomerParameter(IndividualCustomerCreateParameter_Mobile individualCustomerCreateParameter_Mobile, bool isTurkishCitizen)
        {
            SelectModel _addressCity = null,
                _addressDistrict = null,
                _addressCountry = null,
                _drivingLicenseClass = null,
                _gender = null,
                _nationality = null,
                _dialCode = null;

            string _addressDetail = string.Empty,
                _drivingLicensePlace = string.Empty,
                _drivingLicenseNumber = string.Empty;

            bool existIndividualAddressInformation = individualCustomerCreateParameter_Mobile.individualAddressInformation != null;

            if (existIndividualAddressInformation && individualCustomerCreateParameter_Mobile.individualAddressInformation.cityId.HasValue)
            {
                _addressCity = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.individualAddressInformation.cityId.Value),
                    label = individualCustomerCreateParameter_Mobile.individualAddressInformation.cityName
                };
            }
            if (existIndividualAddressInformation && individualCustomerCreateParameter_Mobile.individualAddressInformation.districtId.HasValue)
            {
                _addressDistrict = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.individualAddressInformation.districtId.Value),
                    label = individualCustomerCreateParameter_Mobile.individualAddressInformation.districtName
                };
            }
            if (existIndividualAddressInformation && individualCustomerCreateParameter_Mobile.individualAddressInformation.countryId.HasValue)
            {
                _addressCountry = new SelectModel
                {
                    label = individualCustomerCreateParameter_Mobile.individualAddressInformation.countryName,
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.individualAddressInformation.countryId.Value)
                };
            }
            if (individualCustomerCreateParameter_Mobile.licenseClass.HasValue)
            {
                _drivingLicenseClass = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.licenseClass)
                };
            }
            if (individualCustomerCreateParameter_Mobile.genderCode.HasValue)
            {
                _gender = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.genderCode)
                };
            }
            if (individualCustomerCreateParameter_Mobile.citizenShipId.HasValue)
            {
                _nationality = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.citizenShipId.Value)
                };
            }
            if (!string.IsNullOrEmpty(individualCustomerCreateParameter_Mobile.dialCode))
            {
                _dialCode = new SelectModel
                {
                    value = individualCustomerCreateParameter_Mobile.dialCode
                };
            }
            if (existIndividualAddressInformation && !string.IsNullOrEmpty(individualCustomerCreateParameter_Mobile.individualAddressInformation.addressDetail))
            {
                _addressDetail = individualCustomerCreateParameter_Mobile.individualAddressInformation.addressDetail;
            }
            if (!string.IsNullOrEmpty(individualCustomerCreateParameter_Mobile.licensePlace))
            {
                _drivingLicensePlace = individualCustomerCreateParameter_Mobile.licensePlace;
            }
            if (!string.IsNullOrEmpty(individualCustomerCreateParameter_Mobile.licenseNumber))
            {
                _drivingLicenseNumber = individualCustomerCreateParameter_Mobile.licenseNumber;
            }

            SelectModel _licenseCountry = null;
            _licenseCountry = new SelectModel
            {
                value = string.Empty,
                label = string.Empty
            };

            var _birthDate = DateTime.ParseExact(individualCustomerCreateParameter_Mobile.birthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var _licenseDate = DateTime.ParseExact(individualCustomerCreateParameter_Mobile.licenseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            return new IndividualCustomerCreateParameter
            {
                addressCity = _addressCity == null ? new SelectModel() : _addressCity,
                addressDistrict = _addressDistrict == null ? new SelectModel() : _addressDistrict,
                addressCountry = _addressCountry == null ? new SelectModel() : _addressCountry,
                addressDetail = _addressDetail,
                birthDate = _birthDate,
                distributionChannelCode = (int)rnt_DistributionChannelCode.Mobile,
                firstName = individualCustomerCreateParameter_Mobile.firstName,
                lastName = individualCustomerCreateParameter_Mobile.lastName,
                email = individualCustomerCreateParameter_Mobile.emailAddress,
                password = individualCustomerCreateParameter_Mobile.password,
                findeksPoint = 0,
                drivingLicenseClass = _drivingLicenseClass == null ? new SelectModel() : _drivingLicenseClass,
                gender = _gender == null ? new SelectModel() : _gender,
                drivingLicenseDate = _licenseDate,
                drivingLicensePlace = _drivingLicensePlace,
                drivingLicenseNumber = _drivingLicenseNumber,
                drivingLicenseCountry = _licenseCountry,
                mobilePhone = individualCustomerCreateParameter_Mobile.mobilePhone,
                nationality = _nationality == null ? new SelectModel() : _nationality,
                isCallCenter = false,
                isAlsoInvoiceAddress = individualCustomerCreateParameter_Mobile.isAlsoInvoiceAddress,
                governmentId = isTurkishCitizen ? individualCustomerCreateParameter_Mobile.governmentId : null,
                passportNumber = isTurkishCitizen ? null : individualCustomerCreateParameter_Mobile.governmentId,
                isTurkishCitizen = isTurkishCitizen,
                dialCode = _dialCode == null ? new SelectModel() : _dialCode,
                markettingPermissions = individualCustomerCreateParameter_Mobile.allowMarketingPermissions
            };
        }

        public IndividualCustomerUpdateParameters buildUpdatendividualCustomerParameter(IndividualCustomerUpdateParameter_Web individualCustomerUpdateParameter_Web, bool isTurkishCitizen)
        {
            var _birthDate = DateTime.ParseExact(individualCustomerUpdateParameter_Web.birthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var _licenseDate = DateTime.ParseExact(individualCustomerUpdateParameter_Web.licenseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            SelectModel _addressCity = null;
            if (individualCustomerUpdateParameter_Web.individualAddressInformation.cityId.HasValue)
            {
                _addressCity = new SelectModel
                {
                    value = Convert.ToString(individualCustomerUpdateParameter_Web.individualAddressInformation.cityId.Value),
                    label = individualCustomerUpdateParameter_Web.individualAddressInformation.cityName
                };
            }
            SelectModel _addressDistrict = null;
            if (individualCustomerUpdateParameter_Web.individualAddressInformation.districtId.HasValue)
            {
                _addressDistrict = new SelectModel
                {
                    value = Convert.ToString(individualCustomerUpdateParameter_Web.individualAddressInformation.districtId.Value),
                    label = individualCustomerUpdateParameter_Web.individualAddressInformation.districtName
                };
            }
            return new IndividualCustomerUpdateParameters
            {
                addressCity = _addressCity,
                addressDistrict = _addressDistrict,
                addressCountry = new SelectModel
                {
                    label = individualCustomerUpdateParameter_Web.individualAddressInformation.countryName,
                    value = Convert.ToString(individualCustomerUpdateParameter_Web.individualAddressInformation.countryId.Value)
                },
                addressDetail = individualCustomerUpdateParameter_Web.individualAddressInformation.addressDetail,
                birthDate = _birthDate,
                distributionChannelCode = (int)rnt_DistributionChannelCode.Web,
                firstName = individualCustomerUpdateParameter_Web.firstName,
                lastName = individualCustomerUpdateParameter_Web.lastName,
                email = individualCustomerUpdateParameter_Web.emailAddress,
                findeksPoint = 0,
                drivingLicenseClass = new SelectModel
                {
                    value = Convert.ToString(individualCustomerUpdateParameter_Web.licenseClass)
                },
                gender = new SelectModel
                {
                    value = Convert.ToString(individualCustomerUpdateParameter_Web.genderCode)
                },
                drivingLicenseDate = _licenseDate,
                drivingLicensePlace = individualCustomerUpdateParameter_Web.licensePlace,
                drivingLicenseNumber = individualCustomerUpdateParameter_Web.licenseNumber,
                mobilePhone = individualCustomerUpdateParameter_Web.mobilePhone,
                nationality = new SelectModel
                {
                    value = Convert.ToString(individualCustomerUpdateParameter_Web.citizenShipId)
                },
                isCallCenter = false,
                governmentId = isTurkishCitizen ? individualCustomerUpdateParameter_Web.governmentId : null,
                passportNumber = isTurkishCitizen ? null : individualCustomerUpdateParameter_Web.governmentId,
                isTurkishCitizen = isTurkishCitizen,
                dialCode = new SelectModel
                {
                    value = individualCustomerUpdateParameter_Web.dialCode
                },
                markettingPermissions = null,
                individualCustomerId = individualCustomerUpdateParameter_Web.individualCustomerId,
                individualAddressId = individualCustomerUpdateParameter_Web.individualAddressInformation.individualAddressId
            };
        }

        public IndividualCustomerUpdateParameters buildUpdatendividualCustomerParameter(IndividualCustomerCreateParameter_Web individualCustomerCreateParameter_Web, bool isTurkishCitizen)
        {
            var _birthDate = DateTime.ParseExact(individualCustomerCreateParameter_Web.birthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var _licenseDate = DateTime.ParseExact(individualCustomerCreateParameter_Web.licenseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            SelectModel _addressCity = null;
            if (individualCustomerCreateParameter_Web.individualAddressInformation.cityId.HasValue)
            {
                _addressCity = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Web.individualAddressInformation.cityId.Value),
                    label = individualCustomerCreateParameter_Web.individualAddressInformation.cityName
                };
            }
            SelectModel _addressDistrict = null;
            if (individualCustomerCreateParameter_Web.individualAddressInformation.districtId.HasValue)
            {
                _addressDistrict = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Web.individualAddressInformation.districtId.Value),
                    label = individualCustomerCreateParameter_Web.individualAddressInformation.districtName
                };
            }
            return new IndividualCustomerUpdateParameters
            {
                addressCity = _addressCity,
                addressDistrict = _addressDistrict,
                addressCountry = new SelectModel
                {
                    label = individualCustomerCreateParameter_Web.individualAddressInformation.countryName,
                    value = Convert.ToString(individualCustomerCreateParameter_Web.individualAddressInformation.countryId.Value)
                },
                addressDetail = individualCustomerCreateParameter_Web.individualAddressInformation.addressDetail,
                birthDate = _birthDate,
                distributionChannelCode = (int)rnt_DistributionChannelCode.Web,
                firstName = individualCustomerCreateParameter_Web.firstName,
                lastName = individualCustomerCreateParameter_Web.lastName,
                email = individualCustomerCreateParameter_Web.emailAddress,
                password = individualCustomerCreateParameter_Web.password,
                findeksPoint = 0,
                drivingLicenseClass = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Web.licenseClass)
                },
                gender = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Web.genderCode)
                },
                drivingLicenseDate = _licenseDate,
                drivingLicensePlace = individualCustomerCreateParameter_Web.licensePlace,
                drivingLicenseNumber = individualCustomerCreateParameter_Web.licenseNumber,
                mobilePhone = individualCustomerCreateParameter_Web.mobilePhone,
                nationality = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Web.citizenShipId)
                },
                isCallCenter = false,
                governmentId = isTurkishCitizen ? individualCustomerCreateParameter_Web.governmentId : null,
                passportNumber = isTurkishCitizen ? null : individualCustomerCreateParameter_Web.governmentId,
                isTurkishCitizen = isTurkishCitizen,
                dialCode = new SelectModel
                {
                    value = individualCustomerCreateParameter_Web.dialCode
                },
                markettingPermissions = individualCustomerCreateParameter_Web.allowMarketingPermissions,
                individualAddressId = individualCustomerCreateParameter_Web.individualAddressInformation.individualAddressId
            };
        }

        public IndividualCustomerUpdateParameters buildUpdatendividualCustomerParameter(IndividualCustomerUpdateParameter_Mobile individualCustomerUpdateParameter_Mobile, bool isTurkishCitizen)
        {
            var _birthDate = DateTime.ParseExact(individualCustomerUpdateParameter_Mobile.birthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var _licenseDate = DateTime.ParseExact(individualCustomerUpdateParameter_Mobile.licenseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            SelectModel _addressCity = null,
                _addressDistrict = null,
                _addressCountry = null,
                _drivingLicenseClass = null,
                _gender = null,
                _nationality = null,
                _dialCode = null;

            string _addressDetail = string.Empty,
                _drivingLicensePlace = string.Empty,
                _drivingLicenseNumber = string.Empty;

            bool existIndividualAddressInformation = individualCustomerUpdateParameter_Mobile.individualAddressInformation != null;

            if (existIndividualAddressInformation && individualCustomerUpdateParameter_Mobile.individualAddressInformation.cityId.HasValue)
            {
                _addressCity = new SelectModel
                {
                    value = Convert.ToString(individualCustomerUpdateParameter_Mobile.individualAddressInformation.cityId.Value),
                    label = individualCustomerUpdateParameter_Mobile.individualAddressInformation.cityName
                };
            }
            if (existIndividualAddressInformation && individualCustomerUpdateParameter_Mobile.individualAddressInformation.districtId.HasValue)
            {
                _addressDistrict = new SelectModel
                {
                    value = Convert.ToString(individualCustomerUpdateParameter_Mobile.individualAddressInformation.districtId.Value),
                    label = individualCustomerUpdateParameter_Mobile.individualAddressInformation.districtName
                };
            }
            if (existIndividualAddressInformation && individualCustomerUpdateParameter_Mobile.individualAddressInformation.countryId.HasValue)
            {
                _addressCountry = new SelectModel
                {
                    label = individualCustomerUpdateParameter_Mobile.individualAddressInformation.countryName,
                    value = Convert.ToString(individualCustomerUpdateParameter_Mobile.individualAddressInformation.countryId.Value)
                };
            }
            if (individualCustomerUpdateParameter_Mobile.licenseClass.HasValue)
            {
                _drivingLicenseClass = new SelectModel
                {
                    value = Convert.ToString(individualCustomerUpdateParameter_Mobile.licenseClass)
                };
            }
            if (individualCustomerUpdateParameter_Mobile.genderCode.HasValue)
            {
                _gender = new SelectModel
                {
                    value = Convert.ToString(individualCustomerUpdateParameter_Mobile.genderCode)
                };
            }
            if (individualCustomerUpdateParameter_Mobile.citizenShipId.HasValue)
            {
                _nationality = new SelectModel
                {
                    value = Convert.ToString(individualCustomerUpdateParameter_Mobile.citizenShipId.Value)
                };
            }
            if (!string.IsNullOrEmpty(individualCustomerUpdateParameter_Mobile.dialCode))
            {
                _dialCode = new SelectModel
                {
                    value = individualCustomerUpdateParameter_Mobile.dialCode
                };
            }
            if (existIndividualAddressInformation && !string.IsNullOrEmpty(individualCustomerUpdateParameter_Mobile.individualAddressInformation.addressDetail))
            {
                _addressDetail = individualCustomerUpdateParameter_Mobile.individualAddressInformation.addressDetail;
            }
            if (!string.IsNullOrEmpty(individualCustomerUpdateParameter_Mobile.licensePlace))
            {
                _drivingLicensePlace = individualCustomerUpdateParameter_Mobile.licensePlace;
            }
            if (!string.IsNullOrEmpty(individualCustomerUpdateParameter_Mobile.licenseNumber))
            {
                _drivingLicenseNumber = individualCustomerUpdateParameter_Mobile.licenseNumber;
            }


            return new IndividualCustomerUpdateParameters
            {
                addressCity = _addressCity == null ? new SelectModel() : _addressCity,
                addressDistrict = _addressDistrict == null ? new SelectModel() : _addressDistrict,
                addressCountry = _addressCountry == null ? new SelectModel() : _addressCountry,
                addressDetail = _addressDetail,
                birthDate = _birthDate,
                distributionChannelCode = (int)rnt_DistributionChannelCode.Mobile,
                firstName = individualCustomerUpdateParameter_Mobile.firstName,
                lastName = individualCustomerUpdateParameter_Mobile.lastName,
                email = individualCustomerUpdateParameter_Mobile.emailAddress,
                password = individualCustomerUpdateParameter_Mobile.password,
                findeksPoint = 0,
                drivingLicenseClass = _drivingLicenseClass == null ? new SelectModel() : _drivingLicenseClass,
                gender = _gender == null ? new SelectModel() : _gender,
                drivingLicenseDate = _licenseDate,
                drivingLicensePlace = _drivingLicensePlace,
                drivingLicenseNumber = _drivingLicenseNumber,
                mobilePhone = individualCustomerUpdateParameter_Mobile.mobilePhone,
                nationality = _nationality == null ? new SelectModel() : _nationality,
                isCallCenter = false,
                governmentId = isTurkishCitizen ? individualCustomerUpdateParameter_Mobile.governmentId : null,
                passportNumber = isTurkishCitizen ? null : individualCustomerUpdateParameter_Mobile.governmentId,
                isTurkishCitizen = isTurkishCitizen,
                dialCode = _dialCode == null ? new SelectModel() : _dialCode,
                markettingPermissions = individualCustomerUpdateParameter_Mobile.allowMarketingPermissions,
                individualCustomerId = individualCustomerUpdateParameter_Mobile.individualCustomerId,
                individualAddressId = existIndividualAddressInformation ? individualCustomerUpdateParameter_Mobile.individualAddressInformation.individualAddressId : null
            };
        }

        public IndividualCustomerUpdateParameters buildUpdateIndividualCustomerParameter(IndividualCustomerCreateParameter_Mobile individualCustomerCreateParameter_Mobile, bool isTurkishCitizen)
        {
            var _birthDate = DateTime.ParseExact(individualCustomerCreateParameter_Mobile.birthDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
            var _licenseDate = DateTime.ParseExact(individualCustomerCreateParameter_Mobile.licenseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

            SelectModel _addressCity = null,
                _addressDistrict = null,
                _addressCountry = null,
                _drivingLicenseClass = null,
                _gender = null,
                _nationality = null,
                _dialCode = null,
            _licenseCountry = null;

            _licenseCountry = new SelectModel
            {
                value = string.Empty,
                label = string.Empty
            };

            string _addressDetail = string.Empty,
                _drivingLicensePlace = string.Empty,
                _drivingLicenseNumber = string.Empty;

            bool existIndividualAddressInformation = individualCustomerCreateParameter_Mobile.individualAddressInformation != null;

            if (existIndividualAddressInformation && individualCustomerCreateParameter_Mobile.individualAddressInformation.cityId.HasValue)
            {
                _addressCity = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.individualAddressInformation.cityId.Value),
                    label = individualCustomerCreateParameter_Mobile.individualAddressInformation.cityName
                };
            }
            if (existIndividualAddressInformation && individualCustomerCreateParameter_Mobile.individualAddressInformation.districtId.HasValue)
            {
                _addressDistrict = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.individualAddressInformation.districtId.Value),
                    label = individualCustomerCreateParameter_Mobile.individualAddressInformation.districtName
                };
            }
            if (existIndividualAddressInformation && individualCustomerCreateParameter_Mobile.individualAddressInformation.countryId.HasValue)
            {
                _addressCountry = new SelectModel
                {
                    label = individualCustomerCreateParameter_Mobile.individualAddressInformation.countryName,
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.individualAddressInformation.countryId.Value)
                };
            }
            if (individualCustomerCreateParameter_Mobile.licenseClass.HasValue)
            {
                _drivingLicenseClass = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.licenseClass)
                };
            }
            if (individualCustomerCreateParameter_Mobile.genderCode.HasValue)
            {
                _gender = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.genderCode)
                };
            }
            if (individualCustomerCreateParameter_Mobile.citizenShipId.HasValue)
            {
                _nationality = new SelectModel
                {
                    value = Convert.ToString(individualCustomerCreateParameter_Mobile.citizenShipId.Value)
                };
            }
            if (!string.IsNullOrEmpty(individualCustomerCreateParameter_Mobile.dialCode))
            {
                _dialCode = new SelectModel
                {
                    value = individualCustomerCreateParameter_Mobile.dialCode
                };
            }
            if (existIndividualAddressInformation && !string.IsNullOrEmpty(individualCustomerCreateParameter_Mobile.individualAddressInformation.addressDetail))
            {
                _addressDetail = individualCustomerCreateParameter_Mobile.individualAddressInformation.addressDetail;
            }
            if (!string.IsNullOrEmpty(individualCustomerCreateParameter_Mobile.licensePlace))
            {
                _drivingLicensePlace = individualCustomerCreateParameter_Mobile.licensePlace;
            }
            if (!string.IsNullOrEmpty(individualCustomerCreateParameter_Mobile.licenseNumber))
            {
                _drivingLicenseNumber = individualCustomerCreateParameter_Mobile.licenseNumber;
            }


            return new IndividualCustomerUpdateParameters
            {
                addressCity = _addressCity == null ? new SelectModel() : _addressCity,
                addressDistrict = _addressDistrict == null ? new SelectModel() : _addressDistrict,
                addressCountry = _addressCountry == null ? new SelectModel() : _addressCountry,
                addressDetail = _addressDetail,
                birthDate = _birthDate,
                distributionChannelCode = (int)rnt_DistributionChannelCode.Mobile,
                firstName = individualCustomerCreateParameter_Mobile.firstName,
                lastName = individualCustomerCreateParameter_Mobile.lastName,
                email = individualCustomerCreateParameter_Mobile.emailAddress,
                password = individualCustomerCreateParameter_Mobile.password,
                findeksPoint = 0,
                drivingLicenseClass = _drivingLicenseClass == null ? new SelectModel() : _drivingLicenseClass,
                gender = _gender == null ? new SelectModel() : _gender,
                drivingLicenseDate = _licenseDate,
                //drivingLicensePlace = _drivingLicensePlace,
                //drivingLicenseNumber = _drivingLicenseNumber,
                drivingLicenseCountry = _licenseCountry,
                mobilePhone = individualCustomerCreateParameter_Mobile.mobilePhone,
                nationality = _nationality,
                isCallCenter = false,
                governmentId = isTurkishCitizen ? individualCustomerCreateParameter_Mobile.governmentId : null,
                passportNumber = isTurkishCitizen ? null : individualCustomerCreateParameter_Mobile.governmentId,
                isTurkishCitizen = isTurkishCitizen,
                dialCode = _dialCode == null ? new SelectModel() : _dialCode,
                markettingPermissions = individualCustomerCreateParameter_Mobile.allowMarketingPermissions,
                individualAddressId = existIndividualAddressInformation ? individualCustomerCreateParameter_Mobile.individualAddressInformation.individualAddressId : null
            };
        }
    }
}

