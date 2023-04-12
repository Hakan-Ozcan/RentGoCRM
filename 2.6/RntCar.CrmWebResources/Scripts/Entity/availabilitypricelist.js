var availabilitypricelistfunctions = {
    fields: {
        MongoDBTrigger: "rnt_mongodbintegrationtrigger"
    },
    init: function (formContext) {
        availabilitypricelistfunctions.formContext = formContext.getFormContext();

        if (availabilitypricelistfunctions.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            availabilitypricelistfunctions.formContext.getAttribute(availabilitypricelistfunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        }

    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        if (formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {
            formContext.getAttribute(availabilitypricelistfunctions.fields.MongoDBTrigger).setSubmitMode(XrmHelper.SubmitMode.Always);
            formContext.getAttribute(availabilitypricelistfunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
            formContext.getControl(availabilitypricelistfunctions.fields.MongoDBTrigger).setDisabled(true);
        }
    }

};