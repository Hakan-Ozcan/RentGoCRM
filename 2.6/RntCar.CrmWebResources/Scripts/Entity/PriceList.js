var priceListScripts = {
    fields: {
        MongoDBTrigger: "rnt_mongodbintegrationtrigger"
    },
    init: function (formContext) {
        priceListScripts.formContext = formContext.getFormContext();

        if (priceListScripts.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            priceListScripts.formContext.getAttribute(priceListScripts.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        }

    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        if (formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {
            formContext.getAttribute(priceListScripts.fields.MongoDBTrigger).setSubmitMode(XrmHelper.SubmitMode.Always);
            formContext.getAttribute(priceListScripts.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
            formContext.getControl(priceListScripts.fields.MongoDBTrigger).setDisabled(true);
        }
    },
    common: {
        createNewPriceListFromPriceCalculation: function () {
            debugger;
            XrmHelper.OpenEntityForm("rnt_pricecalculation");
        }
    }
}