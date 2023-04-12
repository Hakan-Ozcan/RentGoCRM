var priceFactorFunctions = {
    fields: {
        PriceFactorType: "rnt_pricefactortypecode",
        PayMethod: "rnt_paymethodcode",
        ReservationChannel: "rnt_reservationchannelcode",
        WeekDays: "rnt_weekdayscode",
        MongoDBTrigger: "rnt_mongodbintegrationtrigger",
        BeginDate: "rnt_begindate",
        EndDate: "rnt_enddate",
        DummyReservationChannel: "rnt_dummy_reservationchannelcode",
        DummyWeekDaysCode: "rnt_dummy_weekdayscode",
        DummyBranchCode: "rnt_dummy_branchcode",
        DummyGroupCodeInformationCode: "rnt_dummy_groupcodeinformation",
        DummySegmentCode: "rnt_dummy_segmentcode",
        SegmentCode: "rnt_segmentcode",
        Branch: "rnt_branchcode",
        GroupCodeInformationCode: "rnt_groupcodeinformationcode"
    },
    init: function (formContext) {
        priceFactorFunctions.formContext = formContext.getFormContext();
        //remove pay later option
        priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.PayMethod).removeOption(2)

        priceFactorFunctions.events.priceFactorTypeOnChange();

        priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.PriceFactorType).addOnChange(priceFactorFunctions.events.priceFactorTypeOnChange);
        priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.ReservationChannel).addOnChange(priceFactorFunctions.events.reservationChannelCodeOnChange);
        priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.WeekDays).addOnChange(priceFactorFunctions.events.weekDaysCodeOnChange);
        priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.Branch).addOnChange(priceFactorFunctions.events.branchCodesOnChange);
        priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.GroupCodeInformationCode).addOnChange(priceFactorFunctions.events.groupCodesOnChange);
        priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.SegmentCode).addOnChange(priceFactorFunctions.events.segmentCodesOnChange);

        if (priceFactorFunctions.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        }
        //XrmHelper.RemoveOption(priceFactorFunctions.fields.PriceFactorType, 2)
    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        if (formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {
            formContext.getAttribute(priceFactorFunctions.fields.MongoDBTrigger).setSubmitMode(XrmHelper.SubmitMode.Always);
            formContext.getAttribute(priceFactorFunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
            formContext.getControl(priceFactorFunctions.fields.MongoDBTrigger).setDisabled(true);
        }
    },
    events: {
        priceFactorTypeOnChange: function () {

            let priceFactorType = priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.PriceFactorType).getValue()
            if (priceFactorType == 1 || priceFactorType == 9) {
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.PayMethod).setVisible(true);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.PayMethod).setRequiredLevel(XrmHelper.RequiredLevels.Required)

                //date related
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.BeginDate).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.EndDate).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.BeginDate).setVisible(false);
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.EndDate).setVisible(false);

                //other fields
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.SegmentCode).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.SegmentCode).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.WeekDays).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.WeekDays).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.ReservationChannel).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.ReservationChannel).setRequiredLevel(XrmHelper.RequiredLevels.None)
            }
            //reservation channel
            else if (priceFactorType == 2) {
                //reservation channel
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.ReservationChannel).setVisible(true);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.ReservationChannel).setRequiredLevel(XrmHelper.RequiredLevels.Required)

                //date related
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.BeginDate).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.EndDate).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.BeginDate).setVisible(true);
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.EndDate).setVisible(true);

                //other fields
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.SegmentCode).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.SegmentCode).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.WeekDays).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.WeekDays).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.PayMethod).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.PayMethod).setRequiredLevel(XrmHelper.RequiredLevels.None)
            }
            //weekdays
            else if (priceFactorType == 3) {
                //weekdays
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.WeekDays).setVisible(true);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.WeekDays).setRequiredLevel(XrmHelper.RequiredLevels.Required)

                //date related
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.BeginDate).setVisible(true);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.BeginDate).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.EndDate).setVisible(true);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.EndDate).setRequiredLevel(XrmHelper.RequiredLevels.Required)

                //other fields
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.SegmentCode).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.SegmentCode).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.ReservationChannel).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.ReservationChannel).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.PayMethod).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.PayMethod).setRequiredLevel(XrmHelper.RequiredLevels.None)
            }
            // special days
            else if (priceFactorType == 4) {
                //special days
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.BeginDate).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.EndDate).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.BeginDate).setVisible(true);
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.EndDate).setVisible(true);
                //other fields
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.SegmentCode).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.SegmentCode).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.ReservationChannel).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.ReservationChannel).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.PayMethod).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.PayMethod).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.WeekDays).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.WeekDays).setRequiredLevel(XrmHelper.RequiredLevels.None)
            }
            //customer
            else if (priceFactorType == 5) {
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.SegmentCode).setVisible(true);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.SegmentCode).setRequiredLevel(XrmHelper.RequiredLevels.Required)

                //date related
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.BeginDate).setVisible(true);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.BeginDate).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.EndDate).setVisible(true);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.EndDate).setRequiredLevel(XrmHelper.RequiredLevels.Required)


                //other fields    
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.ReservationChannel).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.ReservationChannel).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.PayMethod).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.PayMethod).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.WeekDays).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.WeekDays).setRequiredLevel(XrmHelper.RequiredLevels.None)

            }
            //branch
            else if (priceFactorType == 6 || priceFactorType == 7 || priceFactorType == 8) {
                //date related
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.BeginDate).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.EndDate).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.BeginDate).setVisible(true);
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.EndDate).setVisible(true);

                //other fields
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.SegmentCode).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.SegmentCode).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.ReservationChannel).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.ReservationChannel).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.PayMethod).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.PayMethod).setRequiredLevel(XrmHelper.RequiredLevels.None)
                priceFactorFunctions.formContext.getControl(priceFactorFunctions.fields.WeekDays).setVisible(false);
                priceFactorFunctions.formContext.getAttribute(priceFactorFunctions.fields.WeekDays).setRequiredLevel(XrmHelper.RequiredLevels.None)
            }
        },
        // Tolga AYKURT - 14.03.2019
        reservationChannelCodeOnChange: function () {
            priceFactorFunctions.methods.getmultiselectvalues(priceFactorFunctions.fields.ReservationChannel, priceFactorFunctions.fields.DummyReservationChannel);
        },
        // Tolga AYKURT - 14.03.2019
        weekDaysCodeOnChange: function () {
            priceFactorFunctions.methods.getmultiselectvalues(priceFactorFunctions.fields.WeekDays, priceFactorFunctions.fields.DummyWeekDaysCode);
        },
        groupCodesOnChange: function () {
            priceFactorFunctions.methods.getmultiselectvalues(priceFactorFunctions.fields.GroupCodeInformationCode, priceFactorFunctions.fields.DummyGroupCodeInformationCode);
        },
        branchCodesOnChange: function () {
            priceFactorFunctions.methods.getmultiselectvalues(priceFactorFunctions.fields.Branch, priceFactorFunctions.fields.DummyBranchCode);
        },
        segmentCodesOnChange: function () {
            priceFactorFunctions.methods.getmultiselectvalues(priceFactorFunctions.fields.SegmentCode, priceFactorFunctions.fields.DummySegmentCode);
        },
    },
    methods: {
        // Tolga AYKURT - 14.03.2019
        getmultiselectvalues(attributename, dummyattributename) {
            let attributeValues = priceFactorFunctions.formContext.getAttribute(attributename).getValue();
            let dummyStr = "";

            for (let index = 0; index < attributeValues.length; index++) {
                if (dummyStr != "") {
                    dummyStr += ",|" + attributeValues[index] + "|";
                }
                else {
                    dummyStr += "|" + attributeValues[index] + "|";
                }
            }
            priceFactorFunctions.formContext.getAttribute(dummyattributename).setValue(dummyStr)
        }
    }
}