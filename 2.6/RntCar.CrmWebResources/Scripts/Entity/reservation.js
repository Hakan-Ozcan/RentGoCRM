var reservationScripts = {
    formContext: {},
    constData: {
        WORKFLOWID: "D55F24AA-9AFF-4A2A-8BDE-874F0C91E369",
        ACTION_NAME: "rnt_CreateInvoiceWithLogo_Action",
        IntegrationErrorValue: "100000003"
    },
    fields: {

    },
    init: function (formContext) {
        reservationScripts.formContext = formContext.getFormContext();
        let userIsAdmin = XrmHelper.CheckUserIsAdminUser(reservationScripts.formContext.context);
        if (userIsAdmin == true) {
            XrmHelper.ChangeAllTabsDisableStatus(reservationScripts.formContext, false);
        }
        else {
            XrmHelper.ChangeAllTabsDisableStatus(reservationScripts.formContext, true);
        }
    },
    onsave: function () {

    },
    events: {
        callCreateInvoiceWorkflow: function () {
            var buttons = [
                new Alert.Button("OK", null, true),
            ];
            Alert.showLoading()
            let recordId = XrmHelper.GetEntityId(reservationScripts.formContext)
            let url = reservationScripts.formContext.context.getClientUrl()
            Process.callAction(reservationScripts.constData.ACTION_NAME,
                [
                    {
                        key: "Reservation",
                        type: Process.Type.EntityReference,
                        value: new Process.EntityReference("rnt_reservation", recordId)
                    },
                    {
                        key: "LangId",
                        type: Process.Type.Int,
                        value: reservationScripts.formContext.context.getUserLcid()
                    }
                ],
                function (response) {
                    Alert.hide();
                    var res = JSON.parse(response.ExecutionResult);
                    if (!res.ResponseResult.Result) {
                        Alert.show(res.ResponseResult.ExceptionDetail, "", buttons, "ERROR", 500, 250, url, true, 30);
                    }
                    else {
                        Alert.show("Success", "", buttons, "SUCCESS", 500, 250, url, true, 30);
                    }
                },
                function (err) {
                    var message = err.split("CustomErrorMessagefinder: ")[1];
                    Alert.hide();
                    Alert.show(message, "", buttons, "ERROR", 500, 250, url, true, 30);
                })
        },
        openManualPaymentForm: function () {

            var windowOptions = {
                openInNewWindow: false,
                entityName: "rnt_manualpayment"
            };
            var formParameters = {};
            formParameters["reservationid_id"] = XrmHelper.GetEntityId(reservationScripts.formContext);
            formParameters["reservationid_name"] = XrmHelper.GetValue(reservationScripts.formContext, "rnt_name");
            formParameters["reservationid_type"] = XrmHelper.GetEntityName(reservationScripts.formContext);
            if (reservationScripts.formContext.data.entity.attributes.get("rnt_customerid").getValue() != null) {
                formParameters["customerid_id"] = reservationScripts.formContext.data.entity.attributes.get("rnt_customerid").getValue()[0].id;
            }
            Xrm.Navigation.openForm(windowOptions, formParameters);
        }
    }
}