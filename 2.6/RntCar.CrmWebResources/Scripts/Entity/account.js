var accountFunctions = {
    formContext: {},
    showButtons: function () {
        let c = {}
        if (typeof (accountFunctions.formContext.context) !== "undefined") {
            c = accountFunctions.formContext.context
        } else {
            c = Xrm.Page.context
        }
        //first is admin
        let currentUserId = c.getUserId().replace("{", "").replace("}", "");
        let adminUserId = null;
        let url = c.getClientUrl() + "/xrmservices/2011/OrganizationData.svc/rnt_systemparameterSet?$select=rnt_systemuserid";
        let isAdminUser = false
        window.parent.$.ajax({
            type: "GET",
            contentType: "application/json;charset=utf-8",
            datatype: "json",
            url: url,
            async: false,
            cache: false,
            beforeSend: function (XMLHttpRequest) {
                XMLHttpRequest.setRequestHeader("Accept", "application/json");
            },
            success: function (data) {//On Successfull service call   
                adminUserId = data.d.results[0].rnt_systemuserid.Id.toUpperCase();
                if (currentUserId == adminUserId) {
                    isAdminUser = true
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                isAdminUser = false;
            }
        });


        let userRoles = c.getUserRoles()
        let result = false;
        for (var i in userRoles) {
            if (userRoles[i].toString().replace("{", "").replace("}", "").toUpperCase() == "EAC03252-06E3-E911-A831-000D3A47CF3E" ||
                userRoles[i].toString().replace("{", "").replace("}", "").toUpperCase() == "97FABB79-4E60-E911-A961-000D3A454F67" ||
                isAdminUser) {
                result = true;
                break;
            }
        }
        return result;
    },

    init: function (formContext) {
        accountFunctions.formContext = formContext.getFormContext();
        accountFunctions.event.accountTypeOnChange();
        XrmHelper.AddOnChange(accountFunctions.formContext, accountFunctions.fields.AccountType, accountFunctions.event.accountTypeOnChange);
        XrmHelper.AddOnChange(accountFunctions.formContext, accountFunctions.fields.CorporatePaymentMethod, accountFunctions.event.paymentTypeOnchange);

        XrmHelper.enableKeyUser(accountFunctions.formContext);
        XrmHelper.enableCallCenter(accountFunctions.formContext);
    },
    fields: {
        AccountType: "rnt_accounttypecode",
        CorporatePaymentMethod: "rnt_paymentmethodcode",
        AgencyWorkingType: "rnt_agencyworkingtypecode",
        BrokerWorkingType: "rnt_brokerworkingtypecode",

        AccountTypeValues: {
            Corporate: 10,
            Agency: 20,
            Broker: 30
        }
    },
    paymentMethods: {
        Current: { value: 10, text: "Current"},
        CreditCard: { value: 20, text: "CreditCard" },
        LimitedCredit: { value: 30, text: "LimitedCredit" },
        FullCredit: { value: 40, text: "FullCredit" },
        PayBroker: { value: 50, text: "PayBroker" },
        PayOffice: { value: 60, text: "PayOffice" },
        Individual: { value: 5, text: "Individual" },
        Corporate: { value: 15, text: "Corporate" }
    },
    event: {
        paymentTypeOnchange: function () {
           
        },
        accountTypeOnChange: function () {
            if (XrmHelper.GetValue(accountFunctions.formContext, accountFunctions.fields.AccountType) == accountFunctions.fields.AccountTypeValues.Corporate) {
                XrmHelper.ClearOptions(accountFunctions.fields.CorporatePaymentMethod);
                
                XrmHelper.AddOption(accountFunctions.fields.CorporatePaymentMethod, accountFunctions.paymentMethods.Current)
                XrmHelper.AddOption(accountFunctions.fields.CorporatePaymentMethod, accountFunctions.paymentMethods.CreditCard)
            }
            else if (XrmHelper.GetValue(accountFunctions.formContext, accountFunctions.fields.AccountType) == accountFunctions.fields.AccountTypeValues.Agency) {
                XrmHelper.ClearOptions(accountFunctions.fields.CorporatePaymentMethod);

                XrmHelper.AddOption(accountFunctions.fields.CorporatePaymentMethod,accountFunctions.paymentMethods.CreditCard)
                XrmHelper.AddOption(accountFunctions.fields.CorporatePaymentMethod,accountFunctions.paymentMethods.LimitedCredit)
                XrmHelper.AddOption(accountFunctions.fields.CorporatePaymentMethod, accountFunctions.paymentMethods.FullCredit)
            }
            else if (XrmHelper.GetValue(accountFunctions.formContext, accountFunctions.fields.AccountType) == accountFunctions.fields.AccountTypeValues.Broker) {
                XrmHelper.ClearOptions(accountFunctions.fields.CorporatePaymentMethod);

                XrmHelper.AddOption(accountFunctions.fields.CorporatePaymentMethod, accountFunctions.paymentMethods.PayBroker)
                XrmHelper.AddOption(accountFunctions.fields.CorporatePaymentMethod, accountFunctions.paymentMethods.PayOffice)
            }
        }
    }
}