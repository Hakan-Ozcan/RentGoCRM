var customerCreditCardFunctions = {
    messageKeys: {
        CannotCreateCreditCard: "CannotCreateCreditCard",

    },
    filePaths: {
        ErrorMessageXML: 'PaymentErrorMessages.xml'
    },
    onSave: function (context) {

        if (XrmHelper.GetFormType() === XrmHelper.FormTypes.Create) {
            XrmHelper.SetFormNotification(XrmHelper.GetMessages(customerCreditCardFunctions.messageKeys.CannotCreateCreditCard, customerCreditCardFunctions.filePaths.ErrorMessageXML),
                                          XrmHelper.NotificationTypes.Error,
                                          "1000")
            XrmHelper.PreventSave(context)
            return 
        }
       
    }
}