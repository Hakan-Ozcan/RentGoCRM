var campaignFunctions = {
    init: function (formContext) {
        campaignFunctions.formContext = formContext.getFormContext();
        campaignFunctions.events.productAndCampaingTypeOnChange();

        campaignFunctions.formContext.getAttribute(campaignFunctions.fields.ProductType).addOnChange(campaignFunctions.events.productAndCampaingTypeOnChange);
        campaignFunctions.formContext.getAttribute(campaignFunctions.fields.CampaignType).addOnChange(campaignFunctions.events.productAndCampaingTypeOnChange);
        campaignFunctions.formContext.getAttribute(campaignFunctions.fields.CampaignType).addOnChange(campaignFunctions.checkCampaignPrices);

        if (campaignFunctions.formContext.ui.getFormType() == XrmHelper.FormTypes.Create) {
            campaignFunctions.formContext.getAttribute(campaignFunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
        }
        campaignFunctions.checkCampaignPrices()
    },
    checkCampaignPrices: function () {
        debugger
        let v = campaignFunctions.formContext.getAttribute(campaignFunctions.fields.CampaignType).getValue()
      
        if (v == 4) {
            campaignFunctions.formContext.ui.tabs.get("prices").setVisible(true);
            campaignFunctions.formContext.ui.tabs.get("{69e9b688-9a1b-440f-b118-39c46dcd6daa}").sections.get("{69e9b688-9a1b-440f-b118-39c46dcd6daa}_section_5").setVisible(false);
        } else {
            campaignFunctions.formContext.ui.tabs.get("prices").setVisible(false);
            campaignFunctions.formContext.ui.tabs.get("{69e9b688-9a1b-440f-b118-39c46dcd6daa}").sections.get("{69e9b688-9a1b-440f-b118-39c46dcd6daa}_section_5").setVisible(true);
        }
    },
    fields: {
        MongoDBTrigger: "rnt_mongodbintegrationtrigger",

        //control fields
        ProductType: "rnt_producttypecode",
        CampaignType: "rnt_campaigntypecode",

        //action fields
        GroupCode: "rnt_groupcode",
        PayNowDiscountRatio: "rnt_paynowdiscountratio",
        PayLaterDiscountRatio: "rnt_paylaterdiscountratio",
        PayNowDailyPrice: "rnt_paynowdailyprice",
        PayLaterDailyPrice: "rnt_paylaterdailyprice",
        AdditionalProductCode: "rnt_additionalproductcode",
        AdditionalProductDiscount: "rnt_additionalproductdiscount",
        AdditionalProductDailyPrice: "rnt_additionalproductdailyprice"
    },
    fieldTypeCodes: {
        ProductTypeCodes: {
            Car: 1,
            AdditionalProduct: 2,
            CarAndAdditionalProduct: 3
        },
        CampaignTypeCodes: {
            Discount: 1,
            FixPrice: 2
        }
    },
    events: {
        productAndCampaingTypeOnChange: function () {
            debugger
            let productType = campaignFunctions.formContext.getAttribute(campaignFunctions.fields.ProductType).getValue()
            let campaignType = campaignFunctions.formContext.getAttribute(campaignFunctions.fields.CampaignType).getValue()

            if (productType == campaignFunctions.fieldTypeCodes.ProductTypeCodes.Car) {
                if (campaignType == campaignFunctions.fieldTypeCodes.CampaignTypeCodes.FixPrice) {
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.GroupCode).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDailyPrice).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDailyPrice).setVisible(true);

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDiscountRatio).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayNowDiscountRatio).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDiscountRatio).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayLaterDiscountRatio).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductCode).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.AdditionalProductCode).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDiscount).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.AdditionalProductDiscount).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.AdditionalProductDailyPrice).setValue(null)

                }
                else if (campaignType == campaignFunctions.fieldTypeCodes.CampaignTypeCodes.Discount) {
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.GroupCode).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDiscountRatio).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDiscountRatio).setVisible(true);

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayNowDailyPrice).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayLaterDailyPrice).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductCode).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.AdditionalProductCode).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDiscount).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.AdditionalProductDiscount).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.AdditionalProductDailyPrice).setValue(null)
                }
            }
            else if (productType == campaignFunctions.fieldTypeCodes.ProductTypeCodes.AdditionalProduct) {
                if (campaignType == campaignFunctions.fieldTypeCodes.CampaignTypeCodes.FixPrice) {
                    //to do show/hide
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductCode).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDailyPrice).setVisible(true);

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.GroupCode).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.GroupCode).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayNowDailyPrice).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayLaterDailyPrice).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDiscountRatio).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayNowDiscountRatio).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDiscountRatio).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayLaterDiscountRatio).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDiscount).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.AdditionalProductDiscount).setValue(null)

                }
                else if (campaignType == campaignFunctions.fieldTypeCodes.CampaignTypeCodes.Discount) {

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductCode).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDiscount).setVisible(true);

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.GroupCode).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.GroupCode).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayNowDailyPrice).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayLaterDailyPrice).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDiscountRatio).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayNowDiscountRatio).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDiscountRatio).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayLaterDiscountRatio).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.AdditionalProductDailyPrice).setValue(null)
                }
            }
            else if (productType == campaignFunctions.fieldTypeCodes.ProductTypeCodes.CarAndAdditionalProduct) {
                if (campaignType == campaignFunctions.fieldTypeCodes.CampaignTypeCodes.FixPrice) {

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductCode).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDailyPrice).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.GroupCode).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDailyPrice).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDailyPrice).setVisible(true);

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDiscountRatio).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayNowDiscountRatio).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDiscountRatio).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayLaterDiscountRatio).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDiscount).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.AdditionalProductDiscount).setValue(null)

                }
                else if (campaignType == campaignFunctions.fieldTypeCodes.CampaignTypeCodes.Discount) {
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductCode).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDiscount).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDiscountRatio).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDiscountRatio).setVisible(true);
                    campaignFunctions.formContext.getControl(campaignFunctions.fields.GroupCode).setVisible(true);

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayNowDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayNowDailyPrice).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.PayLaterDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.PayLaterDailyPrice).setValue(null)

                    campaignFunctions.formContext.getControl(campaignFunctions.fields.AdditionalProductDailyPrice).setVisible(false);
                    campaignFunctions.formContext.getAttribute(campaignFunctions.fields.AdditionalProductDailyPrice).setValue(null)
                }
            }


        }
    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        if (formContext.ui.getFormType() == XrmHelper.FormTypes.Update && formContext.data.entity.getIsDirty()) {
            formContext.getAttribute(campaignFunctions.fields.MongoDBTrigger).setSubmitMode(XrmHelper.SubmitMode.Always);
            formContext.getAttribute(campaignFunctions.fields.MongoDBTrigger).setValue(XrmHelper.getRandomNumberForGivenForGivenLength(10).toString())
            formContext.getControl(campaignFunctions.fields.MongoDBTrigger).setDisabled(true);
        }
    }
}