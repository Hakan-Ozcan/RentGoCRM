var invoiceScripts = {
    formContext: {},
    formType:
    {
        createForm: 1,
        updateForm: 2
    },
    fields: {
        InvoiceType: "rnt_invoicetypecode",
        TaxNumber: "rnt_taxnumber",
        Goverment: "rnt_govermentid",
        logoInvoiceNumber: "rnt_logoinvoicenumber",
        contract: "rnt_contractid",
        firstName: "rnt_firstname",
        lastName: "rnt_lastname",
        email: "rnt_email",
        govermentid: "rnt_govermentid",
        companyName: "rnt_companyname",
        taxNumber: "rnt_taxnumber",
        taxOffice: "rnt_taxofficeid",
        country: "rnt_countryid",
        city: "rnt_cityid",
        district: "rnt_districtid",
        addressDetail: "rnt_addressdetail",
        invoiceTypeCode: "rnt_invoicetypecode",
        statusReasonCode:"statuscode"
    },
    createInvoice: function () {
        let statusCode = Xrm.Page.getAttribute("statuscode").getValue()
        if (statusCode == 100000002 || statusCode == 100000007) {
            alert("Bu fatura daha önce logoya gönderilmiştir")
            return;
        }
        Xrm.Utility.confirmDialog("Fatura Logoya gönderilecektir? Onaylıyor musunuz?", invoiceScripts.yesCloseCallback, invoiceScripts.noCloseCallback)


    },
    yesCloseCallback: function () {
        Xrm.Utility.showProgressIndicator("Fatura logoya gönderiliyor.. , lütfen bekleyiniz.");
        Process.callWorkflow("1C8E21D1-DEA0-4C59-BF76-8D386EBEA9D6",
            Xrm.Page.data.entity.getId(),
            function () {
                Xrm.Utility.closeProgressIndicator();
                Xrm.Utility.openEntityForm("rnt_invoice", Xrm.Page.data.entity.getId());
            },
            function () {
                Xrm.Utility.closeProgressIndicator();
            }, null, true);
    },
    noCloseCallback: function () {

    },
    getContractStatus: function () {
        var returnVal = false;
        var contract = invoiceScripts.formContext.getAttribute(invoiceScripts.fields.contract).getValue();
        if (contract == null || contract[0] == null || contract[0].id == null) {
            return false;
        }
        var fetchXml =
            "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='rnt_contract'>" +
            "<attribute name='statuscode' />" +
            "<filter type='and'>" +
            "<condition attribute='rnt_contractid' operator='eq' value='" + contract[0].id + "'/>" +
            "<condition attribute='rnt_contracttypecode' operator='eq' value='10'/>" +
            "</filter>" +
            "</entity>" +
            "</fetch>";

        XrmHelper.retrieveData("rnt_contracts", fetchXml,
            function (data) {
                if (data.length > 0)
                    if (data[0].statuscode == 100000000 || data[0].statuscode == 1) {
                        returnVal = true;
                    } else {
                        returnVal = false;
                    }
            },
            function (data) {
                returnVal = false;
            },
            false)
        return returnVal;
    },
    init: function (formContext) {

        invoiceScripts.formContext = formContext.getFormContext();
        invoiceScripts.formContext.ui.setFormNotification("Şahıs şirketleri için bireysel fatura girişi yapmanız gerekmektedir", XrmHelper.NotificationTypes.Info, "1000");

        var logoInvoiceNumber = invoiceScripts.formContext.getAttribute(invoiceScripts.fields.logoInvoiceNumber).getValue();
        var statusCode = invoiceScripts.formContext.getAttribute("statuscode").getValue();
        if (logoInvoiceNumber != null && (statusCode == 100000002 || statusCode == 100000007)) {
            invoiceScripts.formContext.ui.tabs.get("pdf").setVisible(true);
        }
        else {
            invoiceScripts.formContext.ui.tabs.get("pdf").setVisible(false);
        }
        XrmHelper.enableKeyUser(invoiceScripts.formContext);
        var controlContractValue = invoiceScripts.getContractStatus();
        if (controlContractValue) {
            var roleName = XrmHelper.GetConfigurationByName("RentGoSalesRole");
            var userCheckRole = XrmHelper.checkUserRoleByName(roleName);
            if (userCheckRole) {
                XrmHelper.FieldDisable(invoiceScripts.fields.firstName, false);
                XrmHelper.FieldDisable(invoiceScripts.fields.lastName, false);
                XrmHelper.FieldDisable(invoiceScripts.fields.email, false);
                XrmHelper.FieldDisable(invoiceScripts.fields.govermentid, false);
                XrmHelper.FieldDisable(invoiceScripts.fields.companyName, false);
                XrmHelper.FieldDisable(invoiceScripts.fields.taxNumber, false);
                XrmHelper.FieldDisable(invoiceScripts.fields.taxOffice, false);
                XrmHelper.FieldDisable(invoiceScripts.fields.country, false);
                XrmHelper.FieldDisable(invoiceScripts.fields.city, false);
                XrmHelper.FieldDisable(invoiceScripts.fields.district, false);
                XrmHelper.FieldDisable(invoiceScripts.fields.addressDetail, false);
            }
        }
        XrmHelper.FieldDisable(invoiceScripts.fields.statusReasonCode, false);
    },
    
    handleInvoiceType: function () {
        var statusCode = invoiceScripts.formContext.getAttribute("statuscode").getValue();
        var formType = invoiceScripts.formContext.ui.getFormType();
        if (formType == invoiceScripts.formType.updateForm && statusCode != 100000008) {
            XrmHelper.FieldDisable(invoiceScripts.fields.invoiceTypeCode, true);
        }
        else {
            XrmHelper.FieldDisable(invoiceScripts.fields.invoiceTypeCode, false);
        }
    },
    showNewItemButton: function () {
        debugger;
        let userRoles = Xrm.Page.context.getUserRoles();
        let userIsAdmin = XrmHelper.CheckUserIsAdminUser(Xrm.Page.context);

        for (var i in userRoles) {
            if (userRoles[i].toString().replace("{", "").replace("}", "").toUpperCase() == "EAC03252-06E3-E911-A831-000D3A47CF3E" || userIsAdmin) {
                return true;
            }
            else {
                return false;
            }
        }
    },

    CheckUserIsAdminUser: function (context) {
        let currentUserId = Xrm.Page.context.getUserId().replace("{", "").replace("}", "");
        let adminUserId = null;
        let url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/rnt_systemparameterSet?$select=rnt_systemuserid";
        XrmHelper.GetRecordsWithOData(url, false,
            function (data) {
                adminUserId = data.d.results[0].rnt_systemuserid.Id.toUpperCase();
            },
            function () {

            });

        if (currentUserId == adminUserId) {
            return true;
        }
        return false;
    },

    onsave: function (context) {
        var saveEvent = context.getEventArgs();
        invoiceScripts.formContext = context.getFormContext();
        var invoiceType = invoiceScripts.formContext.data.entity.attributes.get(invoiceScripts.fields.InvoiceType).getValue();
        var taxNumber = invoiceScripts.formContext.data.entity.attributes.get(invoiceScripts.fields.TaxNumber).getValue();
        var goverment = invoiceScripts.formContext.data.entity.attributes.get(invoiceScripts.fields.Goverment).getValue();

        console.log("invoice type = ", invoiceType);
        console.log("tax = ", taxNumber);
        console.log("goverment = ", goverment);

        if (invoiceType == 10) { //Individual
            var rgx = /^[1-9]{1}[0-9]{10}$/g
            if (goverment != null && goverment.length != 11) {
                Xrm.Page.ui.setFormNotification("TC Kimlik Numarası 11 Haneli Olmalıdır!", 'ERROR', "100000");
                saveEvent.preventDefault();
            }
            else {
                Xrm.Page.ui.clearFormNotification("100000");
            }
        }
        else if (invoiceType == 20) { //Corporate
            if (taxNumber != null && taxNumber.length != 10) {
                Xrm.Page.ui.setFormNotification("Vergi Numarası 10 Haneli Olmalıdır!", 'ERROR', "100000");
                saveEvent.preventDefault();
            }
            else {
                Xrm.Page.ui.clearFormNotification("100000");
            }

        }

        function checkInput(saveEvent, str, regex) {
            var result = checkRegex(str, regex);
            if (str != result) {
                if (invoiceType == 10)//Individual
                    Xrm.Page.ui.setFormNotification("TC Kimlik Numarası 11 Haneli Olmalıdır!", 'ERROR', "100000");
                else//Corporate
                    Xrm.Page.ui.setFormNotification("Vergi Numarası 10 Haneli Olmalıdır!", 'ERROR', "100000");

                saveEvent.preventDefault();
            }
            else {
                Xrm.Page.ui.clearFormNotification("100000");
            }
        }

        function checkRegex(str, rgx) {
            if (str)
                return str.match(rgx);

            return "undefined or empty";
        }
    }
};