var contactFunctions = {
    formContext: {},
    errorMessageValues: {
        InvalidTCKNNumberVal: '1000',
        TCKNNumberValidationVal: '2000',
        LicenseDateVal: '3000',
        BirthDayVal: '4000',
        InvalidTaxNumberVal: '5000'
    },
    configKeys: {
        TurkeyGuid: 'TurkeyGuid',
    },
    globalVariables: {
        turkeyVal: '',
        isNational: '',

    },
    entityNames: {
        country: "rnt_country"
    },

    fields: {
        CitizenshipId: 'rnt_citizenshipid',
        PassportNumber: 'rnt_passportnumber',
        //PassportIssueDate: 'rnt_passportissuedate',
        GovernmentId: 'governmentid',
        LicenseNumber: 'rnt_drivinglicensenumber',
        LicenseDate: 'rnt_drivinglicensedate',
        LicenseClass: 'rnt_drivinglicenseclasscode',
        LicensePlace: 'rnt_drivinglicenseplace',
        BirthDate: 'birthdate',
        ModifiedOn: "modifiedon",
        IsTurkishCitizen: 'rnt_isturkishcitizen',
        DialCode: 'rnt_dialcode',
        header_status: "header_statuscode"

    },
    filePaths: {
        ErrorMessageXML: 'ErrorMessages.xml'
    },
    messageKeys: {
        InvalidTCKNNumber: "InvalidTCKNNumber",
        MinTCKNNumber: "MinTCKNNumber",
        MinLicenseDate: 'LicenseDateValidation',
        MinAge: 'BirthDateValidation',
        InvalidTaxNumber: "InvalidTaxNumber"

    },
    init: function (formContext) {
        contactFunctions.formContext = formContext.getFormContext();
        Alert.hide()
        contactFunctions.globalVariables.turkeyVal = XrmHelper.GetConfigurationByName_Unified(contactFunctions.formContext, contactFunctions.configKeys.TurkeyGuid)

        if (contactFunctions.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            if (contactFunctions.globalVariables.turkeyVal != '') {
                let lookupValue = {
                    Id: contactFunctions.globalVariables.turkeyVal.toString().split(";")[0],
                    LogicalName: contactFunctions.entityNames.country,
                    Name: contactFunctions.globalVariables.turkeyVal.toString().split(";")[1]
                }
                contactFunctions.formContext.getAttribute(contactFunctions.fields.CitizenshipId).setValue([{ id: lookupValue.Id, name: lookupValue.Name, entityType: lookupValue.LogicalName }]);
                contactFunctions.formContext.getAttribute(contactFunctions.fields.IsTurkishCitizen).setValue(true)
                contactFunctions.formContext.getAttribute(contactFunctions.fields.DialCode).setValue("90")
            }
        }

        //bind events
        XrmHelper.AddOnChange(contactFunctions.formContext, contactFunctions.fields.CitizenshipId, contactFunctions.events.countryOnChange)
        XrmHelper.AddOnChange(contactFunctions.formContext, contactFunctions.fields.LicenseDate, contactFunctions.events.licenseDateOnChange)
        XrmHelper.AddOnChange(contactFunctions.formContext, contactFunctions.fields.BirthDate, contactFunctions.events.birthDateOnChange)
        XrmHelper.AddOnChange(contactFunctions.formContext, contactFunctions.fields.GovernmentId, contactFunctions.events.governmentOnChange)
        //to trigger onload after save
        XrmHelper.AddOnChange(contactFunctions.formContext, contactFunctions.fields.LicenseNumber, contactFunctions.events.licenseNumberKeyPress)

        //triggers
        contactFunctions.events.countryOnChange(true)


        if (XrmHelper.isAdminUser(contactFunctions.formContext)) {
            contactFunctions.formContext.getControl("rnt_segmentcode").setDisabled(false)
            contactFunctions.formContext.getControl(contactFunctions.fields.header_status).setDisabled(false)
        }


    },
    actions: {

    },
    events: {
        customOnSave: function () {
            debugger;
            Xrm.Page.data.save().then(function () {
            },
                function (error) {
                    debugger;
                    Xrm.Page.ui.clearFormNotification();
                    let errorCode = error.errorCode;

                    error.errorMessage = "Duplicate record for email.Please enter unique record!";
                    if (errorCode == "2147879058") {
                        alert(error.errorMessage);
                    }
                    else {

                    }
                });
        },
        birthDateOnChange: function () {
            var val = contactFunctions.formContext.getAttribute(contactFunctions.fields.BirthDate).getValue()

            var age = XrmHelper.calculateAge(val);

            if (age < 18) {
                contactFunctions.formContext.ui.setFormNotification(XrmHelper.GetMessages(contactFunctions.messageKeys.MinAge, contactFunctions.filePaths.ErrorMessageXML),
                    XrmHelper.NotificationTypes.Error,
                    contactFunctions.errorMessageValues.TCKNNumberValidationVal);
                contactFunctions.formContext.getAttribute(contactFunctions.fields.BirthDate).setValue(null)                
            }
            else {
                contactFunctions.formContext.ui.clearFormNotification(contactFunctions.errorMessageValues.BirthDayVal);
            }
        },
        licenseDateOnChange: function () {
            var val = contactFunctions.formContext.getAttribute(contactFunctions.fields.LicenseDate).getValue() 

            if (val > new Date()) {
                contactFunctions.formContext.ui.setFormNotification(XrmHelper.GetMessages(contactFunctions.messageKeys.MinAge, contactFunctions.filePaths.ErrorMessageXML),
                    XrmHelper.NotificationTypes.Error,
                    contactFunctions.errorMessageValues.LicenseDateVal);
                contactFunctions.formContext.getAttribute(contactFunctions.fields.LicenseDate).setValue(null)             
            }
            else {
                contactFunctions.formContext.ui.clearFormNotification(contactFunctions.errorMessageValues.LicenseDateVal);
            }
        },
        countryOnChange: function (initiatingFromLoad) {
            //citizenship is changed need to set null driving license
            //set fields
            if (initiatingFromLoad == false || typeof (initiatingFromLoad) == "undefined") {
                contactFunctions.formContext.getAttribute(contactFunctions.fields.LicenseNumber).setValue(null)
                contactFunctions.formContext.getAttribute(contactFunctions.fields.LicenseDate).setValue(null)
                contactFunctions.formContext.getAttribute(contactFunctions.fields.LicensePlace).setValue(null)
                contactFunctions.formContext.getAttribute(contactFunctions.fields.LicenseClass).setValue(null)
            }
            let citizenShipValue = contactFunctions.formContext.getAttribute(contactFunctions.fields.CitizenshipId).getValue();
            if (citizenShipValue != null) {
                //if it is turkey
                if (citizenShipValue[0].id.toString().toLowerCase().replace("{", "").replace("}", "") ==
                    contactFunctions.globalVariables.turkeyVal.toString().split(";")[0].toLowerCase().replace("{", "").replace("}", "")) {
                    contactFunctions.globalVariables.isNational = true;
                    //government - passport required levels check
                    contactFunctions.formContext.getAttribute(contactFunctions.fields.GovernmentId).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                    contactFunctions.formContext.getAttribute(contactFunctions.fields.PassportNumber).setRequiredLevel(XrmHelper.RequiredLevels.None)
                    //value resetting
                    if (contactFunctions.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
                        contactFunctions.formContext.getAttribute(contactFunctions.fields.PassportNumber).setValue(null);
                        contactFunctions.formContext.getAttribute(contactFunctions.fields.IsTurkishCitizen).setValue(true)
                        contactFunctions.formContext.getAttribute(contactFunctions.fields.DialCode).setValue("90")
                    }
                    //hide fields 
                    contactFunctions.formContext.getControl(contactFunctions.fields.PassportNumber).setVisible(false)
                    //show fields
                    contactFunctions.formContext.getControl(contactFunctions.fields.GovernmentId).setVisible(true)

                }
                else {
                    contactFunctions.globalVariables.isNational = false;
                    //government - passport required levels check
                    contactFunctions.formContext.getAttribute(contactFunctions.fields.GovernmentId).setRequiredLevel(XrmHelper.RequiredLevels.None)
                    contactFunctions.formContext.getAttribute(contactFunctions.fields.PassportNumber).setRequiredLevel(XrmHelper.RequiredLevels.Required)

                    //value resetting
                    if (contactFunctions.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
                        contactFunctions.formContext.getAttribute(contactFunctions.fields.GovernmentId).setValue(null);
                        contactFunctions.formContext.getAttribute(contactFunctions.fields.IsTurkishCitizen).setValue(false)

                        //need to retrieve data from country table
                        var url = contactFunctions.formContext.context.getClientUrl() + "/xrmservices/2011/OrganizationData.svc/rnt_countrySet?$select=rnt_telephonecode&$filter=rnt_countryId eq guid'" + citizenShipValue[0].id + "'";
                        XrmHelper.GetRecordsWithOData(url, false,
                            function (data) {
                                contactFunctions.formContext.getAttribute(contactFunctions.fields.DialCode).setValue(data.d.results[0].rnt_telephonecode);
                            },
                            function () { });
                    }
                    //hide fields 
                    contactFunctions.formContext.getControl(contactFunctions.fields.GovernmentId).setVisible(false)
                    //show fields
                    contactFunctions.formContext.getControl(contactFunctions.fields.PassportNumber).setVisible(true)
                    //clear tckn notification just in case
                    contactFunctions.formContext.ui.clearFormNotification(contactFunctions.errorMessageValues.InvalidTCKNNumberVal);
                }
            }

        },
        checkGoverment: function () {
            // Modified by Tolga AYKURT on 06.03.2019.
            var isOK = false;
            var errorMessage = "";
            var errorMessageVal = "";

            let governmentId = contactFunctions.formContext.getAttribute(contactFunctions.fields.GovernmentId).getValue()
            if (governmentId != null) {
                if (governmentId.length == 11) {
                    isOK = XrmHelper.ValidateCitizenshipNumber(governmentId);
                    errorMessage = XrmHelper.GetMessages(contactFunctions.messageKeys.InvalidTCKNNumber, contactFunctions.filePaths.ErrorMessageXML);
                    errorMessageVal = contactFunctions.errorMessageValues.InvalidTCKNNumberVal;
                }
                else if (governmentId.length == 10) {
                    isOK = XrmHelper.ValidateTaxNumber(governmentId);
                    errorMessage = XrmHelper.GetMessages(contactFunctions.messageKeys.InvalidTaxNumber, contactFunctions.filePaths.ErrorMessageXML);
                    errorMessageVal = contactFunctions.errorMessageValues.InvalidTaxNumberVal;
                }
                else {
                    isOK = false;
                    errorMessage = XrmHelper.GetMessages(contactFunctions.messageKeys.InvalidTCKNNumber, contactFunctions.filePaths.ErrorMessageXML);
                    errorMessageVal = contactFunctions.errorMessageValues.InvalidTCKNNumberVal;
                }

                if (isOK == false) {
                    XrmHelper.SetFormNotification(errorMessage, XrmHelper.NotificationTypes.Error, errorMessageVal);
                }
                else {
                    XrmHelper.ClearNotification(errorMessageVal);
                }

                return isOK;
            }
        },
        governmentOnChange: function () {
            XrmHelper.PreventCharacters(contactFunctions.formContext, contactFunctions.fields.GovernmentId)
            let governmentId = contactFunctions.formContext.getAttribute(contactFunctions.fields.GovernmentId).getValue()
            if (governmentId != null) {
                if (governmentId.length == 11 || governmentId.length == 10) {
                    contactFunctions.events.checkGoverment()
                    //XrmHelper.ClearNotification(contactFunctions.errorMessageValues.TCKNNumberValidationVal);
                }
            }
        },
        licenseNumberKeyPress: function (exContent) {
            if (contactFunctions.globalVariables.isNational)
                XrmHelper.PreventCharacters(contactFunctions.formContext, contactFunctions.fields.LicenseNumber)
        },
        onSave: function (context) {
            let formContext = context.getFormContext();
            if (!formContext.data.entity.getIsDirty()) {
                return false;
            }

            //check goverment is only turkish
            if (formContext.getAttribute(contactFunctions.fields.IsTurkishCitizen).getValue()) {
                let governmentId = formContext.getAttribute(contactFunctions.fields.GovernmentId).getValue();
                if (governmentId != null && governmentId.length < 11) {
                    XrmHelper.PreventSave(context)
                    formContext.ui.controls.get(contactFunctions.fields.GovernmentId).setFocus();
                    formContext.ui.setFormNotification(XrmHelper.GetMessages(contactFunctions.messageKeys.MinTCKNNumber, contactFunctions.filePaths.ErrorMessageXML),
                                                       XrmHelper.NotificationTypes.Error,
                                                       contactFunctions.errorMessageValues.TCKNNumberValidationVal);
                  
                    return false;
                }
                else {
                    formContext.ui.clearFormNotification(contactFunctions.errorMessageValues.TCKNNumberValidationVal);
                    
                }
                if (!contactFunctions.events.checkGoverment()) {
                    XrmHelper.PreventSave(context)
                    formContext.ui.controls.get(contactFunctions.fields.GovernmentId).setFocus();
                    
                    return false;
                }
            }
            //Alert.showLoading()
        }
    }

};