var couponCodeDefinitionScripts = {
    fields: {
        MongoDBTrigger: "rnt_mongodbintegrationtrigger",
        StartDate: "rnt_startdate",
        EndDate: "rnt_enddate",
        ValidityStartDate: "rnt_validitystartdate",
        ValidityEndDate: "rnt_validityenddate",
        NumberofCoupons:"rnt_numberofcoupons",
        AdditionalNumberofCoupons: "rnt_additionalnumberofcoupons",
        PayLaterDiscountValue: "rnt_paylaterdiscountvalue",
        PayNowDiscountValue: "rnt_paynowdiscountvalue",
        CouponCode: "rnt_couponcode",
        IsUnique:"rnt_isunique",
        EnterManuelCouponCode: "rnt_entermanuelcouponcode",
        Type: "rnt_type"

    },
    init: function (formContext) {
        couponCodeDefinitionScripts.formContext = formContext.getFormContext();
        if (couponCodeDefinitionScripts.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            couponCodeDefinitionScripts.formContext.getAttribute(couponCodeDefinitionScripts.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString());

            couponCodeDefinitionScripts.formContext.ui.controls.get(couponCodeDefinitionScripts.fields.NumberofCoupons).setDisabled(false);
            couponCodeDefinitionScripts.formContext.ui.controls.get(couponCodeDefinitionScripts.fields.PayLaterDiscountValue).setDisabled(false);
            couponCodeDefinitionScripts.formContext.ui.controls.get(couponCodeDefinitionScripts.fields.PayNowDiscountValue).setDisabled(false);
            couponCodeDefinitionScripts.formContext.ui.controls.get(couponCodeDefinitionScripts.fields.Value).setDisabled(false);
            couponCodeDefinitionScripts.formContext.ui.controls.get(couponCodeDefinitionScripts.fields.CouponCode).setDisabled(false);
            couponCodeDefinitionScripts.formContext.ui.controls.get(couponCodeDefinitionScripts.fields.IsUnique).setDisabled(false);
            couponCodeDefinitionScripts.formContext.ui.controls.get(couponCodeDefinitionScripts.fields.EnterManuelCouponCode).setDisabled(false);
            couponCodeDefinitionScripts.formContext.ui.controls.get(couponCodeDefinitionScripts.fields.Type).setDisabled(false);
            couponCodeDefinitionScripts.formContext.ui.controls.get(couponCodeDefinitionScripts.fields.AdditionalNumberofCoupons).setDisabled(true);
        }

    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        if (couponCodeDefinitionScripts.formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {
            XrmHelper.SetSubmitMode(couponCodeDefinitionScripts.fields.MongoDBTrigger, XrmHelper.SubmitMode.Always);
            XrmHelper.SetValue(couponCodeDefinitionScripts.fields.MongoDBTrigger, XrmHelper.getRandomNumberForGivenForGivenLength(10).toString());
            XrmHelper.FieldDisable(couponCodeDefinitionScripts.fields.MongoDBTrigger, true)
        }
    }

};