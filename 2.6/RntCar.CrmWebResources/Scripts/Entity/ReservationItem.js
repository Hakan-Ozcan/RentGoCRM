var myreservationitemfunctions = {
    counter: 1,
    selectedDailyPrices: [],
    fields: {
        MongoDBTrigger: "rnt_mongodbintegrationtrigger",
        basePrice: "rnt_baseprice",
        totalAmount: "rnt_totalamount",
        statusCode: "statuscode"
    },
    views: {
        payNowViewId: {
            entityType: 1039, // SavedQuery
            id: "{CA628208-5A61-E911-A95D-000D3A454E11}",
        },
        payLaterViewId: {
            entityType: 1039, // SavedQuery
            id: "{43F166E9-5C61-E911-A95D-000D3A454E11}",
        }
    },
    dailyPricesSubGridOnload: function () {
        //console.log("subgrid onload trigger")
        //if (myreservationitemfunctions.counter > 1) {
        //    return;
        //}
        //let url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/rnt_reservationSet?$select=rnt_paymentchoicecode&$filter=rnt_reservationId eq guid'" + XrmHelper.GetValue("rnt_reservationid")[0].id + "'";
        //XrmHelper.GetRecordsWithOData(url, false,
        //    function (data) {
        //        let v = data.d.results[0].rnt_paymentchoicecode.Value;
        //        if (v == 10) {
        //            Xrm.Page.getControl("dailypricessubgrid").getViewSelector().setCurrentView(myreservationitemfunctions.views.payNowViewId);

        //        }
        //        else {
        //            Xrm.Page.getControl("dailypricessubgrid").getViewSelector().setCurrentView(myreservationitemfunctions.views.payLaterViewId);
        //        }
        //        myreservationitemfunctions.counter++;
        //    },
        //    function () {
        //        myreservationitemfunctions.counter++;
        //    });

        //return;
    },
    init: function (formContext) {

        myreservationitemfunctions.formContext = formContext.getFormContext();
        myreservationitemfunctions.context = formContext.getContext();
        if (myreservationitemfunctions.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            myreservationitemfunctions.formContext.getAttribute(myreservationitemfunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        }
        let itemTypeCode = myreservationitemfunctions.formContext.getAttribute("rnt_itemtypecode").getValue()
        //if not equipment type, mongodb section visibility is false.
        if (itemTypeCode != 1) {
            myreservationitemfunctions.formContext.ui.tabs.get("generalTab").sections.get("mongodbsection").setVisible(false);
            myreservationitemfunctions.formContext.ui.tabs.get("dailyprices").setVisible(false);
        }
        let userIsAdmin = XrmHelper.CheckUserIsAdminUser(myreservationitemfunctions.formContext.context);
        if (userIsAdmin == true) {
            XrmHelper.ChangeAllTabsDisableStatus(myreservationitemfunctions.formContext, false);
        }
        else {
            XrmHelper.ChangeAllTabsDisableStatus(myreservationitemfunctions.formContext, true);
            let isMongoSyncOperationUser = XrmHelper.MongoSyncOperationUserList(myreservationitemfunctions.formContext.context);
            if (isMongoSyncOperationUser == true) {
                myreservationitemfunctions.formContext.getControl(myreservationitemfunctions.fields.MongoDBTrigger).setDisabled(false);
            }
        }

        var currentUserId = myreservationitemfunctions.context.getUserId().replace("{", "").replace("}", "");
        var currentUserName = XrmHelper.getUserNameById(currentUserId);

        var ReservationItemPricingUserList = XrmHelper.GetConfigurationByName("ItemPricingUserList");
        if (ReservationItemPricingUserList != null && currentUserName != null && ReservationItemPricingUserList.includes(currentUserName)) {
            myreservationitemfunctions.formContext.getControl(myreservationitemfunctions.fields.basePrice).setDisabled(false)
            myreservationitemfunctions.formContext.getControl(myreservationitemfunctions.fields.totalAmount).setDisabled(false)
        }
    },

    onsave: function (context) {
        let formContext = context.getFormContext();
        if (formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {

            formContext.getAttribute(myreservationitemfunctions.fields.MongoDBTrigger).setSubmitMode(XrmHelper.SubmitMode.Always);
            formContext.getAttribute(myreservationitemfunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
            formContext.getControl(myreservationitemfunctions.fields.MongoDBTrigger).setDisabled(true);
        }
    }

};