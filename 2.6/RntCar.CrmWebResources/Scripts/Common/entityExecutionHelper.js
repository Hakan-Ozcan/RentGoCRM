var entityExecutionHelper = {

    onSave: function () {
        setTimeout(entityExecutionHelper.errorMessageCheckInterval, 1000);
     
        Xrm.Page.ui.clearFormNotification("100000");
        Xrm.Page.data.save().then(entityExecutionHelper.successCallBack, entityExecutionHelper.errorCallBack);
    },
    successCallBack: function () {
        debugger
        Xrm.Page.ui.clearFormNotification("100000");

    },
    errorCallBack: function (a, b) {
        debugger
        setTimeout(entityExecutionHelper.errorMessageCheckInterval, 1000);
        
        Xrm.Page.ui.setFormNotification(a.message, 'ERROR', "100000");
    },
    errorMessageCheckInterval: function () {
        if (window.top.$(".ms-crm-InlineDialogBackground").length >= 1) {
            window.top.$(".ms-crm-InlineDialogBackground").css("display", "none");
            window.top.$(".ms-crm-InlineDialogBackground").next().css("display", "none");
        }
        setTimeout(entityExecutionHelper.errorMessageCheckInterval, 1000);

    }

}