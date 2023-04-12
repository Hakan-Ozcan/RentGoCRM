using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using RntCar.BusinessLibrary.Business;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.ClassLibrary;
using RntCar.ClassLibrary._Enums_1033;
using RntCar.SDK.Common;
using System;
using System.Globalization;
using System.ServiceModel;
using System.Text.RegularExpressions;

namespace RntCar.BusinessLibrary.Validations
{
    public class IndividualCustomerValidation : ValidationHandler
    {
        #region CONSTRUCTORS
        public IndividualCustomerValidation(IOrganizationService orgService) : base(orgService)
        {
        }

        public IndividualCustomerValidation(IOrganizationService orgService, Guid userId) : base(orgService, userId)
        {
        }

        public IndividualCustomerValidation(IOrganizationService orgService, ITracingService tracingService) : base(orgService, tracingService)
        {
        }
        #endregion

        #region METHODS
        public bool CheckIndividualCustomerInput(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return false;
            }
            return true;
        }

        public bool checkCitizenshipNumber(string firstName, string lastName, int BirthDateYear, long citizenshipNumber)
        {
            var result = false;
            try
            {

                var formattedFirstName = CommonHelper.removeWhitespacesForTCKNValidation(firstName.ToUpper(new CultureInfo("tr-TR", false)));
                var formattedLastName = CommonHelper.removeWhitespacesForTCKNValidation(lastName.ToUpper(new CultureInfo("tr-TR", false)));

                this.Trace("formattedFirstName : " + formattedFirstName);
                this.Trace("formattedLastName : " + formattedLastName);

                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls12;
                CitizenshipService.KPSPublicSoapClient citizenShipClient = new CitizenshipService.KPSPublicSoapClient(GetBasicHttpBinding(), GetEndPointAddress());
                result = citizenShipClient.TCKimlikNoDogrula(citizenshipNumber,
                                                             formattedFirstName,
                                                             formattedLastName,
                                                             BirthDateYear);

            }
            catch
            {


                return false;
            }
            return result;
        }

        public void checkIndividualCustomerDuplicateFieldAndClear(string fieldName, string fieldValue)
        {
            QueryExpression query = new QueryExpression("contact");
            query.ColumnSet = new ColumnSet(fieldName);
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition(fieldName, ConditionOperator.Equal, fieldValue);

            var collection = this.OrgService.RetrieveMultiple(query);
            foreach (var item in collection.Entities)
            {
                item.Attributes[fieldName] = string.Empty;
                this.OrgService.Update(item);
            }
        }

        public void checkIndividualCustomerDuplicateFieldAndClearOthers(string fieldName, string fieldValue, Guid existingRecordId)
        {
            QueryExpression query = new QueryExpression("contact");
            query.ColumnSet = new ColumnSet(fieldName);
            query.Criteria = new FilterExpression(LogicalOperator.And);
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, (int)GlobalEnums.StateCode.Active);
            query.Criteria.AddCondition(fieldName, ConditionOperator.Equal, fieldValue);

            var collection = this.OrgService.RetrieveMultiple(query);
            foreach (var item in collection.Entities)
            {
                if (item.Id != existingRecordId)
                {
                    item.Attributes[fieldName] = string.Empty;
                    this.OrgService.Update(item);
                }
            }
        }

        public bool checkIndividualCustomerMandatoryFields(IndividualCustomerCreateParameter parameter)
        {
            if (parameter.isCallCenter)
                return true;
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid").Split(';')[0];

            foreach (var item in parameter.GetType().GetProperties())
            {
                this.Trace("item : " + item.Name);

                if (parameter.distributionChannelCode == (int)rnt_DistributionChannelCode.Mobile || parameter.isAlsoInvoiceAddress)
                {
                    if (item.Name == "invoiceAddress")
                    {
                        continue;
                    }
                }
                //if it is foreign customer
                if (!parameter.isTurkishCitizen)
                {
                    if (parameter.distributionChannelCode != (int)rnt_DistributionChannelCode.Mobile) //todo :change branch to mobile
                    {
                        if (item.Name != "governmentId" &&
                            item.Name != "findeksPoint" &&
                            item.Name != "addressDistrict" &&
                            item.Name != "addressCity" &&
                            item.Name != "password")
                        {
                            if (item.Name == "passportNumber" && item.GetValue(parameter) == "")
                            {
                                this.Trace("item name null" + item.Name);
                                return false;
                            }
                            if (item.GetValue(parameter) == null)
                            {
                                this.Trace("item name null" + item.Name);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        if (item.Name != "governmentId" &&
                                item.Name != "findeksPoint" &&
                                item.Name != "password" &&
                                item.Name != "drivingLicenseClass" &&
                                item.Name != "drivingLicensePlace" &&
                                item.Name != "drivingLicenseNumber" &&
                                item.Name != "drivingLicenseCountry" &&
                                item.Name != "addressCountry" &&
                                item.Name != "addressCity" &&
                                item.Name != "addressDistrict" &&
                                item.Name != "addressDetail" &&
                                item.Name != "markettingPermissions" &&
                                item.Name != "gender")
                        {
                            if (item.Name == "passportNumber" && item.GetValue(parameter) == "")
                            {
                                this.Trace("item name null" + item.Name);
                                return false;
                            }
                            if (item.GetValue(parameter) == null)
                            {
                                this.Trace("item name null" + item.Name);
                                return false;
                            }
                        }
                    }
                }
                //if turkish
                else
                {
                    if (parameter.distributionChannelCode != (int)rnt_DistributionChannelCode.Mobile)//todo :change branch to mobile
                    {
                        if (string.IsNullOrEmpty(parameter.addressCountry.value))
                        {
                            return false;
                        }
                        if (new Guid(parameter.addressCountry.value) == new Guid(turkeyGuid))
                        {
                            if (item.Name != "passportNumber" &&
                                item.Name != "findeksPoint" &&
                                item.Name != "password")
                            {
                                if (item.GetValue(parameter) == null)
                                {
                                    this.Trace("item name null" + item.Name);
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (item.Name != "passportNumber" &&
                                item.Name != "addressDistrict" &&
                                item.Name != "addressCity" &&
                                item.Name != "findeksPoint" &&
                                item.Name != "password")
                            {
                                if (item.GetValue(parameter) == null)
                                {
                                    this.Trace("item name null" + item.Name);
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (item.Name != "passportNumber" &&
                            item.Name != "findeksPoint" &&
                            item.Name != "password" &&
                            item.Name != "drivingLicenseClass" &&
                            item.Name != "drivingLicensePlace" &&
                            item.Name != "drivingLicenseNumber" &&
                            item.Name != "addressCountry" &&
                            item.Name != "addressCity" &&
                            item.Name != "addressDistrict" &&
                            item.Name != "addressDetail" &&
                            item.Name != "markettingPermissions" &&
                            item.Name != "gender")
                        {
                            if (item.GetValue(parameter) == null)
                            {
                                this.Trace("item name null" + item.Name);
                                return false;
                            }
                        }
                    }



                }
            }
            return true;
        } // null control for all fields
        public bool checkIndividualCustomerFieldsForQuickContract(IndividualCustomerContractRelationData parameter)
        {
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid");
            foreach (var item in parameter.GetType().GetProperties())
            {
                //if it is foreign customer
                if (!parameter.isTurkishCitizen)
                {
                    if (item.Name != "governmentId" &&
                        item.Name != "findeksPoint")
                    {
                        if (item.GetValue(parameter) == null)
                        {
                            return false;
                        }
                    }
                }
                //if turkish
                else
                {
                    if (item.Name != "passportNumber" &&
                        item.Name != "findeksPoint")
                    {
                        if (item.GetValue(parameter) == null)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        } // null control for all fields
        public bool checkIndividualCustomerGovermentIdInput(IndividualCustomerCreateParameter parameter) // goverment id null control
        {
            if (string.IsNullOrEmpty(parameter.governmentId))
                return false;

            return true;
        }

        public bool checkIndividualCustomerGovermentIdInputLength(IndividualCustomerCreateParameter parameter)
        {
            if (parameter.governmentId.Length < 11)
                return false;

            return true;
        } //goverment id length control

        public bool checkIndividualCustomerGovermentIdValidity(IndividualCustomerCreateParameter parameter) // goverment id valid control
        {
            Int64 ATCNO, BTCNO, TCKN;
            long C1, C2, C3, C4, C5, C6, C7, C8, C9, Q1, Q2;

            TCKN = Int64.Parse(parameter.governmentId);

            ATCNO = TCKN / 100;
            BTCNO = TCKN / 100;

            C1 = ATCNO % 10; ATCNO = ATCNO / 10;
            C2 = ATCNO % 10; ATCNO = ATCNO / 10;
            C3 = ATCNO % 10; ATCNO = ATCNO / 10;
            C4 = ATCNO % 10; ATCNO = ATCNO / 10;
            C5 = ATCNO % 10; ATCNO = ATCNO / 10;
            C6 = ATCNO % 10; ATCNO = ATCNO / 10;
            C7 = ATCNO % 10; ATCNO = ATCNO / 10;
            C8 = ATCNO % 10; ATCNO = ATCNO / 10;
            C9 = ATCNO % 10; ATCNO = ATCNO / 10;
            Q1 = ((10 - ((((C1 + C3 + C5 + C7 + C9) * 3) + (C2 + C4 + C6 + C8)) % 10)) % 10);
            Q2 = ((10 - (((((C2 + C4 + C6 + C8) + Q1) * 3) + (C1 + C3 + C5 + C7 + C9)) % 10)) % 10);

            return ((BTCNO * 100) + (Q1 * 10) + Q2 == TCKN);
        }

        public bool checkIndividualCustomerPassportInputs(IndividualCustomerCreateParameter parameter) // null control for passport fields
        {
            if (parameter.passportNumber == null)
                return false;
            return true;
        }

        public bool checkIndividualCustomerDrivingLicenseDate(IndividualCustomerCreateParameter parameter)
        {
            if (parameter.drivingLicenseDate > DateTime.UtcNow)
                return false;

            return true;
        } // driving license date cannot be grater then today

        public bool checkIndividualCustomerMobilePhone(IndividualCustomerCreateParameter parameter)
        {
            string regex = @"((\d{10}))$";
            Match match = Regex.Match(parameter.mobilePhone, regex, RegexOptions.IgnoreCase);
            return match.Success; // bool
        } // check mobile phone validity

        public bool checkIndividualCustomerBirthDate(IndividualCustomerCreateParameter parameter)
        {
            if (parameter.birthDate.Value.CalculateAge() < 17)
                return false;

            return true;
        } //birthdate cannot be smaller than 17
        public bool checkCustomerFinkdexPoint(int findeksPoint, out int maximumFindeksPoint)
        {
            SystemParameterBL systemParameterBL = new SystemParameterBL(this.OrgService);
            var systemParameter = systemParameterBL.GetSystemParameters();
            maximumFindeksPoint = systemParameter.maximumFindeksPoint;
            if (findeksPoint > maximumFindeksPoint)
                return false;

            return true;
        }
        public bool checkIndividualCustomerEmail(IndividualCustomerCreateParameter parameter) // check email validity
        {
            if (parameter.isCallCenter)
                return true;
            string MatchEmailPattern = @"^(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
                                                     + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				                                                [0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
                                                     + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?
				                                                [0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
                                                     + @"([a-zA-Z0-9]+[\w-]+\.)+[a-zA-Z]{1}[a-zA-Z0-9-]{1,23})$";

            return Regex.IsMatch(parameter.email, MatchEmailPattern);

        }

        public bool checkIndividualCustomerDistrict(IndividualCustomerCreateParameter parameter)
        {
            if (parameter.isCallCenter)
                return true;
            ConfigurationBL configurationBL = new ConfigurationBL(this.OrgService);
            var turkeyGuid = configurationBL.GetConfigurationByName("TurkeyGuid").Split(';')[0];
            if (parameter.addressCountry.value == turkeyGuid.ToLower())
            {
                if (parameter.addressDistrict == null)
                {
                    return false;
                }
            }
            return true;
        }

        // Tolga AYKURT - 06.03.2019
        public bool checkDrivingLicenseDateAndBirthDate(string citizenshipId, DateTime birthDate, DateTime drivingLicenseDate, out bool isAlienCustomer)
        {
            /*
             Yabancı uyrukluların ehliyet tarihleri doğum tarihlerinden en az 16 yıl sonra olmalıdır.
             Türkiye uyrukluların ehliyet tarihleri doğum tarihlerinden en az 18 yıl sonra olmalıdır.
             */
            var configurationBL = new ConfigurationBL(this.OrgService);
            var turkeyGuid = new Guid(configurationBL.GetConfigurationByName("TurkeyGuid").Split(';')[0]);
            var isOK = false;

            if (new Guid(citizenshipId).Equals(turkeyGuid) == true)
            {
                isAlienCustomer = false;
                isOK = checkDrivingLicenseDateAndBirthDateForTurks(birthDate, drivingLicenseDate);
            }
            else
            {
                isAlienCustomer = true;
                isOK = checkDrivingLicenseDateAndBirthDateForAliens(birthDate, drivingLicenseDate);
            }

            return isOK;
        }

        // Tolga AYKURT - 06.03.2019
        public bool checkBirthDate(DateTime birthDate, string citizenshipId, out bool isAlienCustomer)
        {
            /*
             Yabancı uyruklular en az 16 yaşında olmalı.
             Türkiye uyruklular en az 18 yaşında olmalı.
             */
            var configurationBL = new ConfigurationBL(this.OrgService);
            var turkeyGuid = new Guid(configurationBL.GetConfigurationByName("TurkeyGuid").Split(';')[0]);
            var isOK = false;

            if (new Guid(citizenshipId).Equals(turkeyGuid) == true)
            {
                isAlienCustomer = false;
                isOK = checkBirthDateForTurkishCitizen(birthDate);
            }
            else
            {
                isAlienCustomer = true;
                isOK = checkBirthDateForAliens(birthDate);
            }

            return isOK;
        }

        private static EndpointAddress GetEndPointAddress()
        {
            var myEndpointAddress = new EndpointAddress("https://tckimlik.nvi.gov.tr/service/kpspublic.asmx");
            return myEndpointAddress;
        }

        private static BasicHttpBinding GetBasicHttpBinding()
        {
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.Name = "KPSPublicSoap";
            binding.Security.Mode = BasicHttpSecurityMode.Transport;

            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.MaxReceivedMessageSize = 2147483647;
            return binding;
        }
        #endregion

        #region HELPER METHODS
        // Tolga AYKURT - 06.03.2019
        private bool checkDrivingLicenseDateAndBirthDateForTurks(DateTime birthDate, DateTime drivingLicenseDate)
        {
            var birthDateYear = birthDate.Year;
            var drivingLicenseDateYear = drivingLicenseDate.Year;

            return (drivingLicenseDateYear - birthDateYear) >= 18;
        }

        // Tolga AYKURT - 06.03.2019
        private bool checkDrivingLicenseDateAndBirthDateForAliens(DateTime birthDate, DateTime drivingLicenseDate)
        {
            var birthDateYear = birthDate.Year;
            var drivingLicenseDateYear = drivingLicenseDate.Year;

            return (drivingLicenseDateYear - birthDateYear) >= 16;
        }

        // Tolga AYKURT - 06.03.2019
        public bool checkBirthDateForTurkishCitizen(DateTime birthDate)
        {
            return (DateTime.Now.Year - birthDate.Year) >= 18;
        }

        // Tolga AYKURT - 06.03.2019
        public bool checkBirthDateForAliens(DateTime birthDate)
        {
            return (DateTime.Now.Year - birthDate.Year) >= 16;
        }

        // Tola AYKURT - 21.0.2019
        public bool CheckDrivingLicenseDate(DateTime drivingLicenceDate)
        {
            return drivingLicenceDate <= DateTime.Now;
        }
        #endregion
    }
}
