var contractScripts = {
    formContext: {},
    constData: {
        WORKFLOWID: "AC45A087-6A39-41C3-B794-0E60537A164C",
        ACTION_NAME: "rnt_CreateInvoiceWithLogo_Action",
        IntegrationErrorValue: "100000003",

    },
    fields: {
        depositBlockage: "rnt_depositblockage",
        digitalSignature: "rnt_digitalsignatureurl",
        statuscode: "statuscode",
        kabis: "rnt_kabiscode",
        equipment: "rnt_equipmentid",
        header_status: "header_statuscode",
        headerCustomer: "header_rnt_customerid",
        dateOfReturn: "rnt_dateofreturn",
        milessMilesCode: "rnt_milessmilescode",
        depositAmount: "rnt_depositamount",
        cancelReasonCode: "rnt_cancelreasoncode"
    },
    showButtons: function () {
        let c = {}
        if (typeof (contractScripts.formContext.context) !== "undefined") {
            c = contractScripts.formContext.context;
            console.log("formContext");
        } else {
            c = Xrm.Page.context;
            console.log("pageContext");
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
                isAdminUser) {
                result = true;
                break;
            }
        }
        return result;
    },
    init: function (formContext) {
        contractScripts.formContext = formContext.getFormContext();
        contractScripts.context = formContext.getContext();
        if (contractScripts.formContext.getAttribute(contractScripts.fields.digitalSignature).getValue() == null) {

            contractScripts.formContext.ui.tabs.get("digitalsignature").setVisible(false)
        } else {
            contractScripts.formContext.ui.tabs.get("digitalsignature").setVisible(true)
        }
        if (contractScripts.formContext.getAttribute(contractScripts.fields.statuscode).getValue() == 1) {
            contractScripts.formContext.getAttribute(contractScripts.fields.equipment).setRequiredLevel(XrmHelper.RequiredLevels.None)
        }
        //for key users
        XrmHelper.enableKeyUser(contractScripts.formContext);

        let userIsAdmin = XrmHelper.CheckUserIsAdminUser(contractScripts.formContext.context);
        if (!userIsAdmin) {
            contractScripts.formContext.getControl(contractScripts.fields.depositBlockage).setDisabled(false)
            contractScripts.formContext.getControl(contractScripts.fields.kabis).setDisabled(false)
            contractScripts.formContext.getControl(contractScripts.fields.statuscode).setDisabled(false)
            contractScripts.formContext.getControl(contractScripts.fields.cancelReasonCode).setDisabled(false)
        }
        contractScripts.formContext.getControl(contractScripts.fields.header_status).setDisabled(true)
        contractScripts.formContext.getControl(contractScripts.fields.headerCustomer).setDisabled(true)
        contractScripts.formContext.getControl(contractScripts.fields.dateOfReturn).setDisabled(false)
        contractScripts.formContext.getControl(contractScripts.fields.milessMilesCode).setDisabled(false)

        var currentUserId = contractScripts.context.getUserId().replace("{", "").replace("}", "");
        var currentUserName = XrmHelper.getUserNameById(currentUserId);

        var DepositUpdateUserList = XrmHelper.GetConfigurationByName("DepositUpdateUserList");
        if (DepositUpdateUserList != null && currentUserName != null && DepositUpdateUserList.includes(currentUserName)) {
            contractScripts.formContext.getControl(contractScripts.fields.depositAmount).setDisabled(false)
        }
    },
    events: {
        callCreateInvoiceWorkflow: function () {
            var buttons = [
                new Alert.Button("OK", null, true),
            ];
            Alert.showLoading()
            let recordId = XrmHelper.GetEntityId(contractScripts.formContext)
            let url = contractScripts.formContext.context.getClientUrl()
            Process.callAction(contractScripts.constData.ACTION_NAME,
                [
                    {
                        key: "Contract",
                        type: Process.Type.EntityReference,
                        value: new Process.EntityReference("rnt_contract", recordId)
                    },
                    {
                        key: "LangId",
                        type: Process.Type.Int,
                        value: contractScripts.formContext.context.getUserLcid()
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
            //let parameters = {};
            //parameters["contractid_id"] = XrmHelper.GetEntityId(contractScripts.formContext);
            //parameters["contractid_name"] = XrmHelper.GetValue(contractScripts.formContext, "rnt_name");
            //parameters["contractid_type"] = XrmHelper.GetEntityName(contractScripts.formContext);

            //if (contractScripts.formContext.data.entity.attributes.get("rnt_customerid").getValue() != null) {
            //    parameters["customerid_id"] = contractScripts.formContext.data.entity.attributes.get("rnt_customerid").getValue()[0].id;
            //}

            var options = { openInNewWindow: true };
            var windowOptions = {
                openInNewWindow: false,
                entityName: "rnt_manualpayment"
            };
            var formParameters = {};
            formParameters["contractid_id"] = XrmHelper.GetEntityId(contractScripts.formContext);
            formParameters["contractid_name"] = XrmHelper.GetValue(contractScripts.formContext, "rnt_name");
            formParameters["contractid_type"] = XrmHelper.GetEntityName(contractScripts.formContext);
            if (contractScripts.formContext.data.entity.attributes.get("rnt_customerid").getValue() != null) {
                formParameters["customerid_id"] = contractScripts.formContext.data.entity.attributes.get("rnt_customerid").getValue()[0].id;
            }
            Xrm.Navigation.openForm(windowOptions, formParameters);


            // Xrm.Utility.openEntityForm("rnt_manualpayment", null, parameters, options);
        },
        digitalSignature: function () {
            let alertStrings = { text: "Sadece teslimat bekliyor statüsündeki sözleşmeler için dijital imza çalıştırılabilir." };
            let alertOptions = { height: 120, width: 260 };

            if (contractScripts.formContext.getAttribute(contractScripts.fields.statuscode).getValue() != 1) {
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(function () { });
                return
            }
            Xrm.Utility.showProgressIndicator("Döküman tablete gönderiliyor.Lütfen bekleyiniz.");
            Process.callWorkflow("1B974C30-FA4E-4691-92AA-A850147DDC21",
                contractScripts.formContext.data.entity.getId(),
                function (data) {
                    Xrm.Utility.closeProgressIndicator();
                    alertStrings = { text: "Döküman tablete gönderilmiştir." };
                    Xrm.Navigation.openAlertDialog(alertStrings, alertOptions).then(function () { });

                },
                function (data) {
                    Xrm.Utility.closeProgressIndicator();
                },
                null);
        }
    }
}