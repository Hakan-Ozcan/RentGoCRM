var invoiceAddressScripts = {
    formContext: {},
    fields: {
        InvoiceType: "rnt_invoicetypecode",
        TaxNumber: "rnt_taxnumber",
        Goverment: "rnt_government",
        logoInvoiceNumber: "rnt_logoinvoicenumber"
    },
    init: function (formContext) { 
        invoiceAddressScripts.formContext = formContext.getFormContext();
        invoiceAddressScripts.formContext.ui.setFormNotification("Şahıs şirketleri için bireysel fatura adresi girişi yapmanız gerekmektedir", XrmHelper.NotificationTypes.Info, "1000");

        //XrmHelper.enableKeyUser(invoiceAddressScripts.formContext);
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
        invoiceAddressScripts.formContext = context.getFormContext();
        var invoiceType = invoiceAddressScripts.formContext.data.entity.attributes.get(invoiceAddressScripts.fields.InvoiceType).getValue();
        var taxNumber = invoiceAddressScripts.formContext.data.entity.attributes.get(invoiceAddressScripts.fields.TaxNumber).getValue();
        var goverment = invoiceAddressScripts.formContext.data.entity.attributes.get(invoiceAddressScripts.fields.Goverment).getValue();

        console.log("invoice type = ", invoiceType);
        console.log("tax = ", taxNumber);
        console.log("goverment = ", goverment);

        if (invoiceType == 10) { //Individual
            var rgx = /^[1-9]{1}[0-9]{10}$/g
            if (goverment.length != 11) {
                Xrm.Page.ui.setFormNotification("TC Kimlik Numarası 11 Haneli Olmalıdır!", 'ERROR', "100000");
                saveEvent.preventDefault();
            }
            else {
                Xrm.Page.ui.clearFormNotification("100000");
            }
        }
        else if (invoiceType == 20) { //Corporate
            if (taxNumber.length != 10) {
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