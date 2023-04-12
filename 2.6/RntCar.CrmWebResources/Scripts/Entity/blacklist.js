var blackListfunctions = {
    formContext: {},
    init: function (formContext) {
        blackListfunctions.formContext = formContext.getFormContext();
        XrmHelper.AddOnChange(blackListfunctions.formContext, blackListfunctions.fields.CustomerId, blackListfunctions.events.customerOnChange)
    },
    fields: {
        CustomerId: "rnt_customerid",
        IdentityKey: "rnt_identitykey"
    },
    entityTypes: {
        Contact: "contact",
        Account: "account"
    },
    events: {
        customerOnChange: function () {
            let customerId = blackListfunctions.formContext.getAttribute(blackListfunctions.fields.CustomerId).getValue()
            if (customerId == null) {
                blackListfunctions.formContext.getAttribute(blackListfunctions.fields.CustomerId).setValue(blackListfunctions.fields.IdentityKey, null)
            }
            else {
                //individuals
                let v = blackListfunctions.formContext.getAttribute(blackListfunctions.fields.CustomerId).getValue()

                if (v[0].entityType == blackListfunctions.entityTypes.Contact) {
                    var url = blackListfunctions.formContext.context.getClientUrl() + "/xrmservices/2011/OrganizationData.svc/ContactSet?$select=GovernmentId,rnt_passportnumber&$filter=ContactId eq guid'" + v[0].id + "'";
                    XrmHelper.GetRecordsWithOData(url, false, function (data) {
                        if (data.d.results.length > 0) {
                            if (data.d.results[0].GovernmentId == null) {
                                blackListfunctions.formContext.getAttribute(blackListfunctions.fields.IdentityKey).setValue(data.d.results[0].rnt_passportnumber)
                            }
                            else {
                                blackListfunctions.formContext.getAttribute(blackListfunctions.fields.IdentityKey).setValue(data.d.results[0].GovernmentId)
                            }
                        }
                    }, function () {

                    });
                }
                else {
                    let accountUrl = blackListfunctions.formContext.context.getClientUrl() + "/xrmservices/2011/OrganizationData.svc/AccountSet?$select=rnt_taxnumber&$filter=AccountId eq guid'" + v[0].id + "'";

                    XrmHelper.GetRecordsWithOData(accountUrl, false, function (data) {
                        if (data.d.results.length > 0 && data.d.results[0].rnt_taxnumber != null) {
                            blackListfunctions.formContext.getAttribute(blackListfunctions.fields.IdentityKey).setValue(data.d.results[0].rnt_taxnumber)
                        }
                    }, function () {

                    });
                }
            }
        },
        onSave: function (context) {
            let identityKey = blackListfunctions.formContext.getAttribute(blackListfunctions.fields.IdentityKey).getValue()
            if (identityKey == null || identityKey == "") {
                XrmHelper.PreventSave(context);
                //todo will add in error message xml
                blackListfunctions.formContext.ui.setFormNotification("identity key can not be empty", XrmHelper.NotificationTypes.Error,"1000")
                
                return false;
            }
            else {
                blackListfunctions.formContext.ui.clearFormNotification("1000");
            }

        }
    }
}