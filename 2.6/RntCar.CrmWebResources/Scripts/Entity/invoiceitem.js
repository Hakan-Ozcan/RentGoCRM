var invoiceItemScripts = {

    init: function (formContext) {
        invoiceItemScripts.formContext = formContext.getFormContext();
        XrmHelper.AddOnChange(invoiceItemScripts.formContext, "rnt_totalamount", invoiceItemScripts.totalAmountChange)
    },
    totalAmountChange: function () {
        if (invoiceItemScripts.formContext.getAttribute("rnt_contractitemid").getValue() == null) {
            alert("Sözleşme Kalemi boş olduğundan , hesaplama yapılamadı")
            return
        }
        let clientUrl = "/xrmservices/2011/OrganizationData.svc/rnt_contractitemSet?$select=rnt_taxratio&$filter=rnt_contractitemId eq guid'" + invoiceItemScripts.formContext.getAttribute("rnt_contractitemid").getValue()[0].id + "'"
        let url = invoiceItemScripts.formContext.context.getClientUrl() + clientUrl
        

        XrmHelper.GetRecordsWithOData(url, false,
            function (data) {
                let amount = (invoiceItemScripts.formContext.getAttribute("rnt_totalamount").getValue() * 100) / (100 + parseFloat(data.d.results[0].rnt_taxratio))
                invoiceItemScripts.formContext.getAttribute("rnt_netamount").setValue(amount)
              
            },
            function () { });
    }

}