var additionalProductsFunctions = {
    fields: {
        ProductCode: "rnt_additionalproductcode",
        Price: "rnt_price"
    },
    init: function (formContext) {
        additionalProductsFunctions.formContext = formContext.getFormContext();
        additionalProductsFunctions.events.productCodeOnChange();
        additionalProductsFunctions.formContext.getAttribute(additionalProductsFunctions.fields.ProductCode).addOnChange(additionalProductsFunctions.events.productCodeOnChange);

    },
    events: {
        productCodeOnChange: function () {
            debugger
            let additionalProductCode = additionalProductsFunctions.formContext.getAttribute(additionalProductsFunctions.fields.ProductCode).getValue()
            if (additionalProductCode == XrmHelper.GetConfigurationByName("OneWayFeeCode")) {

                additionalProductsFunctions.formContext.getControl(additionalProductsFunctions.fields.Price).setVisible(false);
                additionalProductsFunctions.formContext.getAttribute(additionalProductsFunctions.fields.Price).setRequiredLevel(XrmHelper.RequiredLevels.None)
            }
            else {
                additionalProductsFunctions.formContext.getControl(additionalProductsFunctions.fields.Price).setVisible(true);
                additionalProductsFunctions.formContext.getAttribute(additionalProductsFunctions.fields.Price).setRequiredLevel(XrmHelper.RequiredLevels.Required)
            }
        }
    }
}