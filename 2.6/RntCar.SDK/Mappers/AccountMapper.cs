using Microsoft.Xrm.Sdk;
using RntCar.ClassLibrary._Broker;
using RntCar.ClassLibrary._Mobile;
using RntCar.ClassLibrary._Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RntCar.SDK.Mappers
{
    public class AccountMapper
    {
        public CustomerAccountData_Web createWebAccountData(Entity account, string relation)

        {
            var paymentMethodCodeList = ((OptionSetValueCollection)account["rnt_paymentmethodcode"]).Select(p => p.Value).ToList();

            CustomerAccountData_Web customerAccountData_Web = new CustomerAccountData_Web
            {
                relation = relation,
                accountName = account.GetAttributeValue<string>("name"),
                accountId = account.GetAttributeValue<Guid>("accountid"),
                accountType = Enum.GetName(typeof(ClassLibrary._Enums_1033.rnt_AccountTypeCode), account.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value),
                addressDetail = account.GetAttributeValue<string>("rnt_adressdetail"),
                city = account.GetAttributeValue<EntityReference>("rnt_cityid").Name,
                country = account.GetAttributeValue<EntityReference>("rnt_countryid").Name,
                district = account.GetAttributeValue<EntityReference>("rnt_districtid").Name,
                paymentMethodCode = this.buildPaymentMethodCode(paymentMethodCodeList),
                priceCode = account.GetAttributeValue<EntityReference>("rnt_pricecodeid").Name,
                priceCodeId = account.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id,
                taxNumber = account.GetAttributeValue<string>("rnt_taxnumber"),
                taxOffice = account.GetAttributeValue<EntityReference>("rnt_taxoffice").Name,
                emailAddress = account.GetAttributeValue<string>("emailaddress1"),
                phoneNumber = account.GetAttributeValue<string>("telephone1")
            };
            return customerAccountData_Web;
        }
        public CustomerAccountData_Broker createBrokerAccountData(Entity account, double balance)

        {
            var paymentMethodCodeList = ((OptionSetValueCollection)account["rnt_paymentmethodcode"]).Select(p => p.Value).ToList();

            CustomerAccountData_Broker customerAccountData_Web = new CustomerAccountData_Broker
            {
                brokerCode = account.GetAttributeValue<string>("rnt_brokercode"),
                accountName = account.GetAttributeValue<string>("name"),
                accountId = account.GetAttributeValue<Guid>("accountid"),
                accountType = Enum.GetName(typeof(ClassLibrary._Enums_1033.rnt_AccountTypeCode), account.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value),
                addressDetail = account.GetAttributeValue<string>("rnt_adressdetail"),
                city = account.GetAttributeValue<EntityReference>("rnt_cityid").Name,
                country = account.GetAttributeValue<EntityReference>("rnt_countryid").Name,
                district = account.GetAttributeValue<EntityReference>("rnt_districtid").Name,
                paymentMethodCode = paymentMethodCodeList,
                priceCode = account.GetAttributeValue<EntityReference>("rnt_pricecodeid").Name,
                priceCodeId = account.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id,
                taxNumber = account.GetAttributeValue<string>("rnt_taxnumber"),
                taxOffice = account.GetAttributeValue<EntityReference>("rnt_taxoffice").Name,
                emailAddress = account.GetAttributeValue<string>("emailaddress1"),
                phoneNumber = account.GetAttributeValue<string>("telephone1"),
                balance = balance
            };
            return customerAccountData_Web;
        }
        public CustomerAccountData_Mobile createMobileAccountData(Entity account, string relation)
        {
            var paymentMethodCodeList = ((OptionSetValueCollection)account["rnt_paymentmethodcode"]).Select(x => x.Value).ToList();
            CustomerAccountData_Mobile customerAccountData_Mobile = new CustomerAccountData_Mobile
            {
                relation = relation,
                accountName = account.GetAttributeValue<string>("name"),
                accountId = account.GetAttributeValue<Guid>("accountid"),
                accountType = Enum.GetName(typeof(ClassLibrary._Enums_1033.rnt_AccountTypeCode), account.GetAttributeValue<OptionSetValue>("rnt_accounttypecode").Value),
                addressDetail = account.GetAttributeValue<string>("rnt_adressdetail"),
                city = account.GetAttributeValue<EntityReference>("rnt_cityid").Name,
                country = account.GetAttributeValue<EntityReference>("rnt_countryid").Name,
                district = account.GetAttributeValue<EntityReference>("rnt_districtid").Name,
                paymentMethodCode = paymentMethodCodeList,
                priceCode = account.GetAttributeValue<EntityReference>("rnt_pricecodeid").Name,
                priceCodeId = account.GetAttributeValue<EntityReference>("rnt_pricecodeid").Id,
                taxNumber = account.GetAttributeValue<string>("rnt_taxnumber"),
                taxOffice = account.GetAttributeValue<EntityReference>("rnt_taxoffice").Name,
                emailAddress = account.GetAttributeValue<string>("emailaddress1"),
                phoneNumber = account.GetAttributeValue<string>("telephone1")
            };

            return customerAccountData_Mobile;
        }

        public List<string> buildPaymentMethodCode(List<int> paymentMethodCodes)
        {
            List<string> result = new List<string>();

            paymentMethodCodes.ForEach(code =>
            {
                result.Add(Enum.GetName(typeof(ClassLibrary._Enums_1033.rnt_PaymentMethodCode), code));
            });

            return result;
        }
    }
    
}
