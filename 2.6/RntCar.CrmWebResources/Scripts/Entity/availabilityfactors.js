var availabilityfactorsfunctions = {

    fields: {
        MongoDBTrigger: "rnt_mongodbintegrationtrigger",
        ChannelCode: "rnt_channelcode",
        BranchCode: "rnt_multiselectbranchcode",
        AvailabilityFactorType: "rnt_availabilityfactortypecode",
        GroupCode: "rnt_multiselectgroupcodeinformation"
    },
    init: function (formContext) {
        
        availabilityfactorsfunctions.formContext = formContext.getFormContext();        
        
        if (XrmHelper.GetFormType() == XrmHelper.FormTypes.Create) {
            XrmHelper.SetValue(availabilityfactorsfunctions.fields.MongoDBTrigger, XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        }
        XrmHelper.AddOnChange(availabilityfactorsfunctions.formContext,availabilityfactorsfunctions.fields.ChannelCode, availabilityfactorsfunctions.multiSelectChannelOnChange)
        XrmHelper.AddOnChange(availabilityfactorsfunctions.formContext,availabilityfactorsfunctions.fields.AvailabilityFactorType, availabilityfactorsfunctions.factorTypeOnChange)

        availabilityfactorsfunctions.factorTypeOnChange();
        //availabilityfactorsfunctions.multiSelectChannelOnChange()
    },
    factorTypeOnChange: function () {
     
        var v = availabilityfactorsfunctions.formContext.getAttribute(availabilityfactorsfunctions.fields.AvailabilityFactorType).getValue();
        if (v == 1) {
            availabilityfactorsfunctions.formContext.getAttribute(availabilityfactorsfunctions.fields.GroupCode).setRequiredLevel(XrmHelper.RequiredLevels.None)
            availabilityfactorsfunctions.formContext.getControl(availabilityfactorsfunctions.fields.GroupCode).setVisible(false)
            availabilityfactorsfunctions.formContext.getAttribute(availabilityfactorsfunctions.fields.GroupCode).setValue(null)           

        }
        else {
            availabilityfactorsfunctions.formContext.getAttribute(availabilityfactorsfunctions.fields.GroupCode).setRequiredLevel(XrmHelper.RequiredLevels.Required)
            availabilityfactorsfunctions.formContext.getControl(availabilityfactorsfunctions.fields.GroupCode).setVisible(true)
        }
    },
    multiSelectChannelOnChange: function () {
        
    },
    buttonCallback: function (args) {
    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        
        if (formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {
            formContext.getAttribute(availabilityfactorsfunctions.fields.MongoDBTrigger).setSubmitMode(XrmHelper.SubmitMode.Always);
            formContext.getAttribute(availabilityfactorsfunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
            formContext.getControl(availabilityfactorsfunctions.fields.MongoDBTrigger).setDisabled(true) 
            
        }
    }

};