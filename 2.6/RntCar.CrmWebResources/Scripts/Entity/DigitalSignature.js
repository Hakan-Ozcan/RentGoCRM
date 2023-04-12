var digitalSignature = {
    formContext: {},
    fields: {
        statuscode: "statuscode",
        header_status: "header_statuscode",

    },
    init: function (formContext) {
        digitalSignature.formContext = formContext.getFormContext();
        XrmHelper.enableKeyUser(digitalSignature.formContext);
        digitalSignature.formContext.getControl(digitalSignature.fields.header_status).setDisabled(true)
    }
}