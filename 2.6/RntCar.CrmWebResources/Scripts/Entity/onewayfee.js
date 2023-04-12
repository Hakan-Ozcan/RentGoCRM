var onewayfeeScripts = {
    fields: {
        MongoDBTrigger: "rnt_mongodbintegrationtrigger"
    },
    init: function (formContext) {
        onewayfeeScripts.formContext = formContext.getFormContext();
        if (onewayfeeScripts.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            onewayfeeScripts.formContext.getAttribute(onewayfeeScripts.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        }

    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        if (onewayfeeScripts.formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {
            XrmHelper.SetSubmitMode(onewayfeeScripts.fields.MongoDBTrigger, XrmHelper.SubmitMode.Always);
            XrmHelper.SetValue(onewayfeeScripts.fields.MongoDBTrigger, XrmHelper.getRandomNumberForGivenForGivenLength(10).toString());
            XrmHelper.FieldDisable(onewayfeeScripts.fields.MongoDBTrigger, true)
        }
    }

};