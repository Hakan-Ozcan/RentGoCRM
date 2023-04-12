var paymentScripts = {
    formContext: {},
    onLoad: function (formContext) {
        paymentScripts.formContext = formContext.getFormContext();
        XrmHelper.enableKeyUser(paymentScripts.formContext);
    }
}