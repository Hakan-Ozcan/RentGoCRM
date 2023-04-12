let tabName = "{942fd855-7569-4fd1-a927-b5eb7dd54f00}";
let globalCounter = 0;
let options = { id: "loading" };

var priceCalculationFunctions = {
    fields: {
        groupCodeValues: 'rnt_groupcodevalue',
        groupCodeValidation: 'rnt_groupcodevalidation',

        groupCodeListPriceValues: 'rnt_groupcodelistpricevalues',
        groupCodeListPriceValidation: 'rnt_groupcodelistpricevalidation',

        availabilityPriceListValues: 'rnt_availabilitypricelistvalues',
        availabilityPriceListValidation: 'rnt_availabilitypricelistvalidation',

        name: 'rnt_name',
        priceType: 'rnt_pricetype',
        endDate: 'rnt_enddate',
        beginDate: 'rnt_begindate',

        priceInfoValidation: 'rnt_priceinfovalidation',

        dummyField: 'rnt_dummyfield'
    },
    sections: {
        priceInfoSection: '{942fd855-7569-4fd1-a927-b5eb7dd54f00}_priceInformation',
        groupCodeSection: '{942fd855-7569-4fd1-a927-b5eb7dd54f00}_grupCodeSection',
        groupCodeListPriceSection: '{942fd855-7569-4fd1-a927-b5eb7dd54f00}_priceDaysSection',
        availabilityInfoSection: '{942fd855-7569-4fd1-a927-b5eb7dd54f00}_availabilityInformation'
    },
    stages: {
        priceInfoStageId: '001e370d-77b8-4264-8842-1335c1d145e1',
        groupCodeStageId: 'ad0d70c6-3330-4a67-9d17-361b8dade596',
        groupCodeListPriceStageId: 'fc8ccf11-e06e-449e-95a7-f80a984ec02e',
        availabilityStageId: '7f699ec1-a22c-469d-90c3-901e97c37273'
    },
    init: function () {
        if (XrmHelper.GetFormType() == XrmHelper.FormTypes.Create) {
            XrmHelper.SaveForm();
        }

        priceCalculationFunctions.common.hideSectionByStageId();
        priceCalculationFunctions.common.hideFieldsOnProcess();
        priceCalculationFunctions.common.setFieldsRequiredLevel();

        XrmHelper.AddOnChange(priceCalculationFunctions.fields.groupCodeValues, priceCalculationFunctions.common.validations.groupCodeValidations);
        XrmHelper.AddOnChange(priceCalculationFunctions.fields.groupCodeListPriceValues, priceCalculationFunctions.common.validations.groupCodeListPriceValidations);
        XrmHelper.AddOnChange(priceCalculationFunctions.fields.availabilityPriceListValues, priceCalculationFunctions.common.validations.availabilityPriceListValidations);
        XrmHelper.AddOnChange(priceCalculationFunctions.fields.beginDate, priceCalculationFunctions.common.validations.datesValidation);
        XrmHelper.AddOnChange(priceCalculationFunctions.fields.endDate, priceCalculationFunctions.common.validations.datesValidation);

        XrmHelper.AddStateChange(priceCalculationFunctions.events.onStageChange);
        XrmHelper.AddStatusChange(priceCalculationFunctions.events.onStageStatusChange);
    },
    onsave: function () {
    },
    events: {
        onStageChange: function () {
            debugger;
            priceCalculationFunctions.common.hideSectionByStageId();
        },
        onStageStatusChange: function () {
            debugger;
            // bug : bpf on status change trigger twice sometimes
            if (XrmHelper.GetStageStatus() == "finished") {
                if (globalCounter == 0) {

                    new Alert.showLoading();

                    priceCalculationFunctions.common.createProcess();
                    globalCounter = 1;
                }
            }
        }
    },
    common: {
        createProcess: function () {
            debugger;
            var priceListObj = {
                rnt_name: XrmHelper.GetValue("ownerid")[0].name + " - " + XrmHelper.GetText("rnt_pricetype"),
                rnt_pricetype: XrmHelper.GetValue("rnt_pricetype"),
                rnt_begindate: XrmHelper.GetValue("rnt_begindate"),
                rnt_enddate: XrmHelper.GetValue("rnt_enddate")
            }
            var priceListParameters = JSON.stringify(priceListObj);
            var groupCodeParameters = XrmHelper.GetValue("rnt_groupcodevalue");
            var groupCodeListPriceParameters = XrmHelper.GetValue("rnt_groupcodelistpricevalues");
            var availabilityPriceListParameters = XrmHelper.GetValue("rnt_availabilitypricelistvalues");


            Process.callAction("rnt_ExecuteCreatePriceCalculation",
                [{
                    key: "PriceListParameters",
                    type: Process.Type.String,
                    value: priceListParameters
                },
                {
                    key: "GroupCodeParameters",
                    type: Process.Type.String,
                    value: groupCodeParameters
                },
                {
                    key: "GroupCodeLisPriceParameters",
                    type: Process.Type.String,
                    value: groupCodeListPriceParameters
                },
                {
                    key: "AvailabilityPriceListParameters",
                    type: Process.Type.String,
                    value: availabilityPriceListParameters
                }],
                function (data) {
                    debugger;
                    XrmHelper.SetFormNotification("Process successfully finished.", XrmHelper.NotificationTypes.Info);
                    new Alert.hide();
                    //var respose = JSON.parse(data);
                },
                function (e, t) {
                    alert(e);
                    // Write the trace log to the dev console
                    if (window.console && console.error) {
                        console.error(e + "\n" + t);
                    }
                    return false;
                });
        },
        setFieldsRequiredLevel: function () {
            XrmHelper.SetRequiredLevel(priceCalculationFunctions.fields.name, XrmHelper.RequiredLevels.Required);
            XrmHelper.SetRequiredLevel(priceCalculationFunctions.fields.priceType, XrmHelper.RequiredLevels.Required);
            XrmHelper.SetRequiredLevel(priceCalculationFunctions.fields.beginDate, XrmHelper.RequiredLevels.Required);
            XrmHelper.SetRequiredLevel(priceCalculationFunctions.fields.endDate, XrmHelper.RequiredLevels.Required);
        },
        hideSectionByStageId: function () {
            var stageId = XrmHelper.GetActiveStageId();
            var stageName = XrmHelper.GetActiveStageName();
            var selectedStageId = XrmHelper.GetSelectedStageId();

            priceCalculationFunctions.common.hideFieldsOnProcess();

            if (stageId == priceCalculationFunctions.stages.priceInfoStageId) {
                XrmHelper.SectionVisibility(tabName, priceCalculationFunctions.sections.priceInfoSection, true);
            }
            else {
                XrmHelper.SectionVisibility(tabName, priceCalculationFunctions.sections.priceInfoSection, false);
            }

            if (stageId == priceCalculationFunctions.stages.groupCodeStageId) {
                XrmHelper.FireOnChange(priceCalculationFunctions.fields.groupCodeValues);
                XrmHelper.SectionVisibility(tabName, priceCalculationFunctions.sections.groupCodeSection, true);
            }
            else {
                XrmHelper.SectionVisibility(tabName, priceCalculationFunctions.sections.groupCodeSection, false);
            }

            if (stageId == priceCalculationFunctions.stages.groupCodeListPriceStageId) {
                XrmHelper.FireOnChange(priceCalculationFunctions.fields.groupCodeListPriceValues);
                XrmHelper.SectionVisibility(tabName, priceCalculationFunctions.sections.groupCodeListPriceSection, true);
            }
            else {
                XrmHelper.SectionVisibility(tabName, priceCalculationFunctions.sections.groupCodeListPriceSection, false);
            }

            if (stageId == priceCalculationFunctions.stages.availabilityStageId) {
                XrmHelper.FireOnChange(priceCalculationFunctions.fields.availabilityPriceListValues);
                XrmHelper.SectionVisibility(tabName, priceCalculationFunctions.sections.availabilityInfoSection, true);
            }
            else {
                XrmHelper.SectionVisibility(tabName, priceCalculationFunctions.sections.availabilityInfoSection, false);
            }
        },
        hideFieldsOnProcess: function () {
            XrmHelper.ProcessFieldVisibility(priceCalculationFunctions.fields.priceType, false);
            XrmHelper.ProcessFieldVisibility(priceCalculationFunctions.fields.dummyField, false);
            XrmHelper.ProcessFieldVisibility(priceCalculationFunctions.fields.priceInfoValidation, false);
            XrmHelper.ProcessFieldVisibility(priceCalculationFunctions.fields.groupCodeValidation, false);
            XrmHelper.ProcessFieldVisibility(priceCalculationFunctions.fields.groupCodeListPriceValidation, false);
            XrmHelper.ProcessFieldVisibility(priceCalculationFunctions.fields.availabilityPriceListValidation, false);
        },
        validations: {
            groupCodeValidations: function () {
                var jsonString = XrmHelper.GetValue(priceCalculationFunctions.fields.groupCodeValues);
                var data = JSON.parse(jsonString);
                var response = data.filter(x => x.rnt_price == 0 || x.rnt_price == "");
                if (response.length > 0) {
                    XrmHelper.SetFormNotification(response[0].rnt_name + " cannot be null or zero", XrmHelper.NotificationTypes.Error, "customError");
                    XrmHelper.SetValue(priceCalculationFunctions.fields.groupCodeValidation, false);
                }
                else {
                    XrmHelper.ClearNotification("customError");
                    XrmHelper.SetValue(priceCalculationFunctions.fields.groupCodeValidation, true);
                }
            },
            groupCodeListPriceValidations: function () {
                var jsonString = XrmHelper.GetValue(priceCalculationFunctions.fields.groupCodeListPriceValues);
                var data = JSON.parse(jsonString);
                var response = data.some(x => x.rnt_ratio == null || x.rnt_ratio == "");
                if (response) {
                    XrmHelper.SetFormNotification("Ratio value cannot be null", XrmHelper.NotificationTypes.Error, "customError");
                    XrmHelper.SetValue(priceCalculationFunctions.fields.groupCodeListPriceValidation, false);
                }
                else {
                    XrmHelper.ClearNotification("customError");
                    XrmHelper.SetValue(priceCalculationFunctions.fields.groupCodeListPriceValidation, true);
                }
            },
            availabilityPriceListValidations: function () {
                var jsonString = XrmHelper.GetValue(priceCalculationFunctions.fields.availabilityPriceListValues);
                var data = JSON.parse(jsonString);
                var response = data.some(x => x.rnt_ratio == null || x.rnt_ratio == "");
                if (response) {
                    XrmHelper.SetFormNotification("Ratio value cannot be null", XrmHelper.NotificationTypes.Error, "customError");
                    XrmHelper.SetValue(priceCalculationFunctions.fields.availabilityPriceListValidation, false);
                }
                else {
                    XrmHelper.ClearNotification("customError");
                    XrmHelper.SetValue(priceCalculationFunctions.fields.availabilityPriceListValidation, true);
                }
            },
            datesValidation: function () {
                if (XrmHelper.GetValue(priceCalculationFunctions.fields.beginDate) > XrmHelper.GetValue(priceCalculationFunctions.fields.endDate)) {
                    XrmHelper.SetFormNotification("End date cannot be smaller than begin date", XrmHelper.NotificationTypes.Error, "customError");
                    XrmHelper.SetValue(priceCalculationFunctions.fields.priceInfoValidation, false);
                }
                else {
                    XrmHelper.SetValue(priceCalculationFunctions.fields.priceInfoValidation, true);
                    XrmHelper.ClearNotification("customError");
                }
            }
        }
    }
}