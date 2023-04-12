var equipmentScripts = {
    formContext: {},
    showButtons: function () {
        let c = {}
        if (typeof (equipmentScripts.formContext.context) !== "undefined") {
            c = equipmentScripts.formContext.context;
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

    fields: {
        MongoDBTrigger: "rnt_mongodbintegrationtrigger",
        header_status: "header_statuscode",
        currentBranch: "rnt_currentbranchid",
        tireType: "rnt_tiretypecode",
        lastInspection: "rnt_lastvehicleinspection"
    },
    init: function (formContext) {
        equipmentScripts.formContext = formContext.getFormContext();

        if (equipmentScripts.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            equipmentScripts.formContext.getAttribute(equipmentScripts.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        }
        let userIsAdmin = XrmHelper.CheckUserIsAdminUser(equipmentScripts.formContext.context);
        if (userIsAdmin == true) {
            XrmHelper.ChangeAllTabsDisableStatus(equipmentScripts.formContext, false);
        }
        else {
            XrmHelper.ChangeAllTabsDisableStatus(equipmentScripts.formContext, true);
        }
        let userRoles = equipmentScripts.formContext.context.getUserRoles()
        var branchManagerRoleName = XrmHelper.GetConfigurationByName("RentGoBranchManagerRoleName");
        var isCheckBranchManager = XrmHelper.checkUserRoleByName(branchManagerRoleName);

        for (var i in userRoles) {
            if (userRoles[i].toString().replace("{", "").replace("}", "").toUpperCase() == "EAC03252-06E3-E911-A831-000D3A47CF3E") {
                XrmHelper.ChangeAllTabsDisableStatus(equipmentScripts.formContext, false);
                break;
            }
        }
        if (isCheckBranchManager) {
            equipmentScripts.formContext.getControl(equipmentScripts.fields.tireType).setDisabled(false);
        }
        equipmentScripts.formContext.getControl(equipmentScripts.fields.header_status).setDisabled(true);
        XrmHelper.FieldDisable(equipmentScripts.fields.lastInspection, true);

    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        if (formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {
            formContext.getAttribute(equipmentScripts.fields.MongoDBTrigger).setSubmitMode(XrmHelper.SubmitMode.Always);
            formContext.getAttribute(equipmentScripts.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
            formContext.getControl(equipmentScripts.fields.MongoDBTrigger).setDisabled(true);
        }
    },
    equalbutton: function (formContext) {
        formContext.getAttribute(equipmentScripts.fields.MongoDBTrigger).setSubmitMode(XrmHelper.SubmitMode.Always);
        formContext.getAttribute(equipmentScripts.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        formContext.getControl(equipmentScripts.fields.MongoDBTrigger).setDisabled(true);
        XrmHelper.SaveForm();
    }

};