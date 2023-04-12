var contractItemFunctions = {
    fields: {
        MongoDBTrigger: "rnt_mongodbintegrationtrigger",
        header_status: "header_statuscode",
        basePrice: "rnt_baseprice",
        totalAmount: "rnt_totalamount",
        statusCode:"statuscode"
    },
    init: function (formContext) {
        contractItemFunctions.formContext = formContext.getFormContext();
        contractItemFunctions.context = formContext.getContext();
        if (contractItemFunctions.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            contractItemFunctions.formContext.getAttribute(contractItemFunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        }
        let itemTypeCode = Xrm.Page.getAttribute("rnt_itemtypecode").getValue();
        //if not equipment type, mongodb section visibility is false.
        if (itemTypeCode != 1) {
            contractItemFunctions.formContext.ui.tabs.get("generalTab").sections.get("mongoSection").setVisible(false);
            contractItemFunctions.formContext.ui.tabs.get("dailyprices").setVisible(false);
        }
        let userIsAdmin = XrmHelper.CheckUserIsAdminUser(contractItemFunctions.formContext.context);
        if (userIsAdmin == true) {
            XrmHelper.ChangeAllTabsDisableStatus(contractItemFunctions.formContext, false);
        }
        else {
            XrmHelper.ChangeAllTabsDisableStatus(contractItemFunctions.formContext, true);
            let isMongoSyncOperationUser = XrmHelper.MongoSyncOperationUserList(contractItemFunctions.formContext.context);
            if (isMongoSyncOperationUser == true) {
                contractItemFunctions.formContext.getControl(contractItemFunctions.fields.MongoDBTrigger).setDisabled(false);
            }
        }
        XrmHelper.AddOnChange(contractItemFunctions.formContext, "rnt_itemtypecode", contractItemFunctions.onStatusChange)
        contractItemFunctions.formContext.getControl(contractItemFunctions.fields.header_status).setDisabled(true)
        contractItemFunctions.onStatusChange()

        var currentUserId = contractItemFunctions.context.getUserId().replace("{", "").replace("}", "");
        var currentUserName = XrmHelper.getUserNameById(currentUserId);

        var ContractItemPricingUserList = XrmHelper.GetConfigurationByName("ItemPricingUserList");
        if (ContractItemPricingUserList != null && currentUserName != null && ContractItemPricingUserList.includes(currentUserName)) {
            contractItemFunctions.formContext.getControl(contractItemFunctions.fields.basePrice).setDisabled(false)
            contractItemFunctions.formContext.getControl(contractItemFunctions.fields.totalAmount).setDisabled(false)
            contractItemFunctions.formContext.getControl(contractItemFunctions.fields.statusCode).setDisabled(false)
        }
    },
    onStatusChange: function () {
        debugger
        let a = contractItemFunctions.formContext.getAttribute("rnt_itemtypecode").getValue()
        if (a === 1 || a === 5) {
            contractItemFunctions.formContext.getAttribute("rnt_equipment").setRequiredLevel(XrmHelper.RequiredLevels.Required)
        } else {
            contractItemFunctions.formContext.getAttribute("rnt_equipment").setRequiredLevel(XrmHelper.RequiredLevels.None)

        }
    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        if (formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {
            formContext.getAttribute(contractItemFunctions.fields.MongoDBTrigger).setSubmitMode(XrmHelper.SubmitMode.Always);
            formContext.getAttribute(contractItemFunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
            formContext.getControl(contractItemFunctions.fields.MongoDBTrigger).setDisabled(true);
        }
    }
}