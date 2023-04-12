var groupCodeListPriceScripts = {
    fields: {
        MongoDBTrigger: "rnt_mongodbintegrationtrigger"
    },
    init: function (formContext) {
        groupCodeListPriceScripts.formContext = formContext.getFormContext();
        if (groupCodeListPriceScripts.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            groupCodeListPriceScripts.formContext.getAttribute(groupCodeListPriceScripts.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        }

    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        if (formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {
            formContext.getAttribute(groupCodeListPriceScripts.fields.MongoDBTrigger).setSubmitMode(XrmHelper.SubmitMode.Always);
            formContext.getAttribute(groupCodeListPriceScripts.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
            formContext.getControl(groupCodeListPriceScripts.fields.MongoDBTrigger).setDisabled(true);
        }
    }

};