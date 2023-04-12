var manualPaymentScripts = {
    contractCompletedStatus: 100000001,
    contractStatusCode: 0,
    contractType: 0,
    contractPaymentType: 0,
    contractUrl: "/xrmservices/2011/OrganizationData.svc/rnt_contractSet?$select=statuscode,rnt_contracttypecode,rnt_paymentmethodcode&$filter=rnt_contractId eq guid'",
    reservationUrl: "/xrmservices/2011/OrganizationData.svc/rnt_reservationSet?$select=statuscode,rnt_reservationtypecode,rnt_paymentmethodcode&$filter=rnt_reservationId eq guid'",
    entityType: 0,//contract

    filterCustomerAddress: function (customerId) {
        let fetchXml = "<filter type='and'>"
        fetchXml += "<condition attribute='rnt_contactid' uitype='contact' operator='eq' value='" + customerId + "' />"
        fetchXml += "</filter>";
        manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.invoiceAddress).addCustomFilter(fetchXml);
    },
    invoiceAddressOperations: function () {
        
        if (manualPaymentScripts.statusCode == manualPaymentScripts.contractCompletedStatus) {
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.manualPaymentType).getValue() == 30) {
                manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.invoiceAddress).setVisible(true);
                manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.invoiceAddress).setRequiredLevel(XrmHelper.RequiredLevels.Required)
            }
            else {
                manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.invoiceAddress).setVisible(false);
                manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.invoiceAddress).setRequiredLevel(XrmHelper.RequiredLevels.None)
            }

        }
        else {
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.invoiceAddress).setVisible(false);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.invoiceAddress).setRequiredLevel(XrmHelper.RequiredLevels.None)
        }
    },
    onLoad: function (formContext) {       
        manualPaymentScripts.formContext = formContext.getFormContext();
        manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("existingCreditCardSection").setVisible(false);
        manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("newCardCreateSection").setVisible(false);

        let statuscode = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.statusCode).getValue()

        if (statuscode == 100000000) {
            XrmHelper.ChangeAllTabsDisableStatus(manualPaymentScripts.formContext, true);
        }
        else {
            var xrmObject = manualPaymentScripts.formContext.context.getQueryStringParameters();
            var lookupValue = new Array();
            lookupValue[0] = new Object();
            var xrmObject = manualPaymentScripts.formContext.context.getQueryStringParameters();
            console.log("xrmObject", xrmObject)
            if (typeof xrmObject["reservationid_id"] === 'undefined' && typeof xrmObject["contractid_id"] === 'undefined') {
                manualPaymentScripts.formContext.ui.setFormNotification("Manual payment'ı , sözleşme detayındaki buton üzerinden açmanız gerekmektedir", XrmHelper.NotificationTypes.Error, "1000");
                return;
            }
            else {
                manualPaymentScripts.formContext.ui.clearFormNotification("1000");
            }

            manualPaymentScripts.entityType = typeof xrmObject["reservationid_id"] === 'undefined' ? 0 : 10;
            console.log("manualPaymentScripts.entityType", manualPaymentScripts.entityType)
            //console.log("manualPaymentScripts.reservationid_id", xrmObject["reservationid_id"].toString())
            //console.log("manualPaymentScripts.reservationid_name", xrmObject["reservationid_name"].toString())
            //console.log("manualPaymentScripts.reservationid_type", xrmObject["reservationid_type"].toString())

            lookupValue[0].id = manualPaymentScripts.entityType == 0 ? xrmObject["contractid_id"].toString() : xrmObject["reservationid_id"].toString();
            lookupValue[0].name = manualPaymentScripts.entityType == 0 ? xrmObject["contractid_name"].toString() : xrmObject["reservationid_name"].toString();
            lookupValue[0].entityType = manualPaymentScripts.entityType == 0 ? xrmObject["contractid_type"].toString() : xrmObject["reservationid_type"].toString();

            console.log("lookupValue", lookupValue)
            debugger
            if (lookupValue[0].id != null) {
                let entity = manualPaymentScripts.entityType == 0 ? manualPaymentScripts.fields.contract : manualPaymentScripts.fields.reservation;
                manualPaymentScripts.formContext.getAttribute(entity).setValue(lookupValue);
            }
            console.log("test1");
            manualPaymentScripts.formContext.getAttribute("rnt_isdebt").setValue(true)
            manualPaymentScripts.formContext.getAttribute("rnt_isdebt").setSubmitMode("always")

            let customerId = xrmObject["customerid_id"].toString();
            let clientUrl = manualPaymentScripts.entityType == 0 ? manualPaymentScripts.contractUrl : manualPaymentScripts.reservationUrl;
            let url = manualPaymentScripts.formContext.context.getClientUrl() + clientUrl + lookupValue[0].id + "'";
            console.log("test2");

            XrmHelper.GetRecordsWithOData(url, false,
                function (data) {
                    console.log("test3");

                    manualPaymentScripts.contractPaymentType = data.d.results[0].rnt_paymentmethodcode.Value
                    manualPaymentScripts.contractType = manualPaymentScripts.entityType == 0 ? data.d.results[0].rnt_contracttypecode.Value : data.d.results[0].rnt_reservationtypecode.Value

                    if (manualPaymentScripts.contractType == 20) {
                        if (manualPaymentScripts.contractPaymentType == 10) {
                            manualPaymentScripts.formContext.ui.setFormNotification("Sözleşme kurumsal , Ödeme Tipi Cari'dir.", XrmHelper.NotificationTypes.Warning, "8000");
                        }
                        if (manualPaymentScripts.contractPaymentType == 20) {
                            manualPaymentScripts.formContext.ui.setFormNotification("Sözleşme kurumsal , Ödeme Tipi Kredi Kartı'dır.", XrmHelper.NotificationTypes.Warning, "8000");
                        }
                    }

                    if (manualPaymentScripts.contractPaymentType == 40 ||
                        manualPaymentScripts.contractPaymentType == 10) {
                        manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.manualPaymentType).removeOption(10);
                        manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.manualPaymentType).removeOption(20);
                        manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.creditCardType).setVisible(false)
                        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditCardType).setRequiredLevel(XrmHelper.RequiredLevels.None)
                    }
                    console.log("test4");

                    if (manualPaymentScripts.entityType == 10) {
                        manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.manualPaymentType).removeOption(10);
                        manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.manualPaymentType).removeOption(30);
                    }
                    console.log("test5");

                    manualPaymentScripts.statusCode = data.d.results[0].statuscode.Value;
                    manualPaymentScripts.invoiceAddressOperations();
                },
                function () { });

            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.invoiceAddress).addPreSearch(function () {

                manualPaymentScripts.filterCustomerAddress(customerId);

            });

            XrmHelper.AddOnChange(manualPaymentScripts.formContext, manualPaymentScripts.fields.manualPaymentType, manualPaymentScripts.manualPaymentTypeOnChange)
        }
    },
    isClickedMakePaymentButton: false,
    fields: {
        creditCardType: "rnt_creditcardtype",
        statusCode: "statuscode",
        reservation: "rnt_reservationid",
        contract: "rnt_contractid",
        nameonCard: "rnt_nameoncard",
        cardNumber: "rnt_cardnumber",
        cvc: "rnt_cvcstring",
        year: "rnt_year",
        month: "rnt_month",
        amount: "rnt_amount",
        creditcardId: "rnt_creditcardid",
        additionalProduct: "rnt_additionalproductid",
        invoiceAddress: "rnt_invoiceaddressid",
        manualPaymentType: "rnt_manualpaymenttypecode",
        paymentAmount: "rnt_paymentamount",
        additionalProduct2: "rnt_additionalproduct2id",
        additionalProduct3: "rnt_additionalproduct3id",
        amount2: "rnt_amount2",
        amount3: "rnt_amount3",
        isdebt: "rnt_isdebt",
        description: "rnt_description"
    },
    tabs: {
        general: "general"
    },
    onsave: function (context) {
        if (!manualPaymentScripts.isClickedMakePaymentButton) {
            XrmHelper.PreventSave(context);
            alert("Please, click Make Payment button to pay.");
        }
    },
    manualPaymentTypeOnChange: function () {
        //bireysel sözleşme ve broker // limited credit ve kredi kartı  acenta
        if (manualPaymentScripts.contractType == 10 || manualPaymentScripts.contractType == 40 || manualPaymentScripts.contractPaymentType == 30 ||
            (manualPaymentScripts.contractType == 30 && manualPaymentScripts.contractPaymentType == 20)) {
            manualPaymentScripts.invoiceAddressOperations();
        }
        manualPaymentScripts.manualPaymentTypeOperations()
        //cari ödeme tipli sözleşme
        if (manualPaymentScripts.contractPaymentType == 10 || manualPaymentScripts.contractPaymentType == 40) {
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.creditCardType).setVisible(false);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditCardType).setRequiredLevel(XrmHelper.RequiredLevels.None);
        }
    },
    manualPaymentTypeOperations: function () {
        let text = "";
        manualPaymentScripts.formContext.ui.clearFormNotification("5555");
        //only payment
        if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.manualPaymentType).getValue() == 10 && manualPaymentScripts.entityType == 0) {
            text = "Müşteri'nin Rentgo'ya borçlu olduğu durumlarda kullanılması gerekmektedir(Sözleşme üzerindeki borç tutarı alanının 0'dan büyük olması müşteri'nin Rentgo'ya borçlu olduğu anlamına gelir.)\n Borçlu olmayan sözleşmeler için bu değer seçilip çekim yapılırsa , sistem daha sonra iadesini yapacaktır.";
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.isdebt).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.additionalProduct).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.additionalProduct2).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.additionalProduct3).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.amount).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.amount2).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.amount3).setVisible(false);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.isdebt).setValue(false)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct).setValue(null)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct2).setValue(null)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct3).setValue(null)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount).setValue(null)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount2).setValue(null)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount3).setValue(null)
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.paymentAmount).setVisible(true);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.creditCardType).setVisible(true);

            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct).setRequiredLevel(XrmHelper.RequiredLevels.None);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount).setRequiredLevel(XrmHelper.RequiredLevels.None);

        }
        //only refund
        else if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.manualPaymentType).getValue() == 20) {
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.isdebt).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.additionalProduct).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.additionalProduct2).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.additionalProduct3).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.amount).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.amount2).setVisible(false);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.amount3).setVisible(false);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.isdebt).setValue(false)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct).setValue(null)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct2).setValue(null)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct3).setValue(null)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount).setValue(null)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount2).setValue(null)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount3).setValue(null)

            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.paymentAmount).setVisible(true);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.creditCardType).setVisible(false);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditCardType).setValue(null)

            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct).setRequiredLevel(XrmHelper.RequiredLevels.None);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount).setRequiredLevel(XrmHelper.RequiredLevels.None);
        }
        //Add Additional Product With Payment
        else if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.manualPaymentType).getValue() == 30 && manualPaymentScripts.entityType == 0) {
            text = "Sözleşmeye kalem ekleyerek çekim yapacaktır. Çekim yapılamaması durumunda müşteriye borç kaydı atılacaktır.";

            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.isdebt).setVisible(true);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.additionalProduct).setVisible(true);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.additionalProduct2).setVisible(true);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.additionalProduct3).setVisible(true);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.amount).setVisible(true);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.amount2).setVisible(true);
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.amount3).setVisible(true);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.isdebt).setValue(true)
            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.paymentAmount).setVisible(false);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.paymentAmount).setValue(null)

            manualPaymentScripts.formContext.getControl(manualPaymentScripts.fields.creditCardType).setVisible(true);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditCardType).setRequiredLevel(XrmHelper.RequiredLevels.Required);

            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct).setRequiredLevel(XrmHelper.RequiredLevels.Required);
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount).setRequiredLevel(XrmHelper.RequiredLevels.Required);
        }

        manualPaymentScripts.formContext.ui.setFormNotification(text, XrmHelper.NotificationTypes.Info, "5555");
    },
    creditCardTypeFieldonchange: function () {

        let key = "19051993";
        let creditCardType = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditCardType).getValue()
        sessionStorage.setItem("creditCardType1905", JSON.stringify(creditCardType));
        let reservationId = manualPaymentScripts.entityType == 0 ? null : manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.reservation).getValue()[0].id;
        let contractId = manualPaymentScripts.entityType == 0 ? manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.contract).getValue()[0].id : null;

        let customerId = ""
        let clientUrl = manualPaymentScripts.entityType == 0 ? "/xrmservices/2011/OrganizationData.svc/rnt_contractSet?$select=rnt_customerid&$filter=rnt_contractId eq guid'" + manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.contract).getValue()[0].id + "'"
            : "/xrmservices/2011/OrganizationData.svc/rnt_reservationSet?$select=rnt_customerid&$filter=rnt_reservationId eq guid'" + manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.reservation).getValue()[0].id + "'";
        let url = manualPaymentScripts.formContext.context.getClientUrl() + clientUrl
        debugger

        XrmHelper.GetRecordsWithOData(url, false,
            function (data) {
                customerId = data.d.results[0].rnt_customerid.Id
            },
            function () { });
        //let customerId = xrmObject["customerid_id"].toString();
        //type 1: new 
        //type 2: existing
        if (creditCardType == 1) {
            manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("existingCreditCardSection").setVisible(false);
            manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("newCardCreateSection").setVisible(true);

            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.nameonCard).setRequiredLevel(XrmHelper.RequiredLevels.Required)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cardNumber).setRequiredLevel(XrmHelper.RequiredLevels.Required)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cvc).setRequiredLevel(XrmHelper.RequiredLevels.Required)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.year).setRequiredLevel(XrmHelper.RequiredLevels.Required)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.month).setRequiredLevel(XrmHelper.RequiredLevels.Required)
            manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditcardId).setValue(null)
        }
        else if (creditCardType == 2) {
            manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("newCardCreateSection").setVisible(false);
            manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("existingCreditCardSection").setVisible(true);

            Process.callAction("rnt_GetCustomerCreditCards",
                [{
                    key: "customerId",
                    type: Process.Type.String,
                    value: customerId
                },
                {
                    key: "reservationId",
                    type: Process.Type.String,
                    value: reservationId
                },
                {
                    key: "contractId",
                    type: Process.Type.String,
                    value: contractId
                }],
                function (data) {
                    let parsed = JSON.parse(data.CreditCardResponse)

                    if (parsed.ResponseResult.Result) {
                        sessionStorage.removeItem(key)
                        sessionStorage.setItem(key, JSON.stringify(parsed.creditCards));

                        let randomCode = Math.random().toString(36).substring(7);
                        let querystring = "?code=" + randomCode;

                        let FilterControl = manualPaymentScripts.formContext.ui.controls.get("WebResource_manualPaymenthtml");
                        var tempSrc = FilterControl.getSrc();
                        var aboutBlank = "about:blank";
                        FilterControl.setSrc(aboutBlank);

                        setTimeout(function () {
                            FilterControl.setSrc(tempSrc)
                        }, 1000);


                    }
                    else {
                        alert(parsed.ResponseResult.ExceptionDetail);
                    }
                },
                function (e, t) {

                    let message = e.split(":")[1];
                    alert(message);
                });
        }
        else {
            manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("existingCreditCardSection").setVisible(false);
            manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("newCardCreateSection").setVisible(false);
        }
    },
    amountFieldonchange: function () {
        let amount = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount).getValue();

        //no need to show credit cards for refund
        if (amount < 0) {
            manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("existingCreditCardSection").setVisible(false);
            manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("newCardCreateSection").setVisible(false);
        }
        else {
            let creditCardType = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditCardType).getValue()
            if (creditCardType == 1) {
                manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("existingCreditCardSection").setVisible(false);
                manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("newCardCreateSection").setVisible(true);

                manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.nameonCard).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cardNumber).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cvc).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.year).setRequiredLevel(XrmHelper.RequiredLevels.Required)
                manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.month).setRequiredLevel(XrmHelper.RequiredLevels.Required)
            }
            else if (creditCardType == 2) {
                manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("newCardCreateSection").setVisible(false);
                manualPaymentScripts.formContext.ui.tabs.get(manualPaymentScripts.tabs.general).sections.get("existingCreditCardSection").setVisible(true);

            }
        }
    },
    yesCloseCallback: function () {
        var xrmObject = manualPaymentScripts.formContext.context.getQueryStringParameters();
        if (typeof xrmObject["reservationid_id"] === 'undefined' && typeof xrmObject["contractid_id"] === 'undefined') {
            manualPaymentScripts.formContext.ui.setFormNotification("Manual payment'ı , sözleşme detayındaki buton üzerinden açmanız gerekmektedir", XrmHelper.NotificationTypes.Error, "1000");
            return;
        }
        else {
            manualPaymentScripts.formContext.ui.clearFormNotification("1000");
        }

        if (manualPaymentScripts.entityType == 0 && manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.contract).getValue() == null) {
            alert("Sözleşme zorunludur.")
            return
        }
        if (manualPaymentScripts.entityType == 10 && manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.reservation).getValue() == null) {
            alert("Sözleşme zorunludur.")
            return
        }
        let manualPaymentType = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.manualPaymentType).getValue()
        //only refund
        //only payment
        if (manualPaymentType == 10 || manualPaymentType == 20) {
            let paymentAmount = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.paymentAmount).getValue()
            if (paymentAmount == null) {
                alert("Miktar alanı zorunludur.")
                return
            }
        }
        //Add Additional Product With Payment
        else if (manualPaymentType == 30) {
            let amount = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount).getValue();
            if (amount == null) {
                alert("Miktar alanı zorunludur.")
                return;
            }
            let add = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct).getValue();
            if (add == null) {
                alert("Ek ürün alanı zorunludur.")
                return;
            }
            let add2 = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct2).getValue();
            if (add2 != null) {
                if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount2).getValue() == null) {
                    alert("miktar 2 zorunludur.")
                    return;
                }
            }
            let add3 = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct3).getValue();
            if (add3 != null) {
                if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount3).getValue() == null) {
                    alert("miktar 3 zorunludur.")
                    return;
                }
            }

            let amount3 = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount3).getValue();
            if (amount3 != null) {
                if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct3).getValue() == null) {
                    alert("Ek ürün 3 zorunludur.")
                    return;
                }
            }
            let amount2 = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount2).getValue();
            if (amount2 != null) {
                if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct2).getValue() == null) {
                    alert("Ek ürün 2 zorunludur.")
                    return;
                }
            }

            if (manualPaymentScripts.contractCompletedStatus === manualPaymentScripts.statusCode && manualPaymentScripts.contractType == 10) {
                let invoice = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.invoiceAddress).getValue();
                if (invoice == null) {
                    alert("Fatura alanı zorunludur.")
                    return;
                }
            }
        }


        manualPaymentScripts.isClickedMakePaymentButton = true;
        let statuscode = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.statusCode).getValue();

        if (statuscode == 100000000) {
            XrmHelper.PreventSave(context);
            manualPaymentScripts.isClickedMakePaymentButton = false;
            alert("İşlenmiş olan manuel ödemeler , tekrar işleme alınamaz.");
        }
        else {
            Xrm.Utility.showProgressIndicator("Manuel ödeme işleniyor , lütfen bekleyiniz.");
            let parameters = {
                creditCardId: null, creditCardNumber: null, nameOnCard: null, month: null,
                year: null, cvc: null, amount: null, reservationId: null, contractId: null,
                langId: null, additionalProductId: null, description: null, entityId: null
            };
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct2).getValue() != null) {
                parameters.additionalProductId2 = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct2).getValue()[0].id
            }
            else {
                parameters.additionalProductId2 = null
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct3).getValue() != null) {
                parameters.additionalProductId3 = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct3).getValue()[0].id
            }
            else {
                parameters.additionalProductId3 = null
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount2).getValue() != null) {
                parameters.amount2 = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount2).getValue()
            } else {
                parameters.amount2 = null
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount3).getValue() != null) {
                parameters.amount3 = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount3).getValue()
            }
            else {
                parameters.amount3 = null
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.manualPaymentType).getValue() != null) {
                parameters.manualPaymentType = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.manualPaymentType).getValue()
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.paymentAmount).getValue() != null) {
                parameters.paymentAmount = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.paymentAmount).getValue()
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.isdebt).getValue() != null) {
                parameters.isDebt = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.isdebt).getValue()
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditcardId).getValue() != null) {
                parameters.creditCardId = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditcardId).getValue()
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount).getValue() != null) {
                parameters.amount = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.amount).getValue();
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.month).getValue() != null) {
                parameters.month = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.month).getValue()
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.year).getValue() != null) {
                parameters.year = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.year).getValue()
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cvc).getValue() != null) {
                parameters.cvc = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cvc).getValue()
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cardNumber).getValue() != null) {
                parameters.creditCardNumber = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cardNumber).getValue()
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.nameonCard).getValue() != null) {
                parameters.nameOnCard = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.nameonCard).getValue();
            }
            if (manualPaymentScripts.entityType == 0 && manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.contract).getValue() != null) {
                parameters.contractId = manualPaymentScripts.entityType == 0 ? manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.contract).getValue()[0].id.replace(/{|"|}/gi, "") : null;
            }
            if (manualPaymentScripts.entityType == 10 && manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.reservation).getValue() != null) {
                parameters.reservationId = manualPaymentScripts.entityType == 0 ? null : manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.reservation).getValue()[0].id.replace(/{|"|}/gi, "");
            }
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct).getValue() != null) {
                parameters.additionalProductId = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.additionalProduct).getValue()[0].id.replace(/{|"|}/gi, "");
            }
            parameters.langId = manualPaymentScripts.formContext.context.getUserLcid()
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.invoiceAddress).getValue() != null)
                parameters.invoiceAddressId = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.invoiceAddress).getValue()[0].id;
            if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.description).getValue() != null)
                parameters.description = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.description).getValue()

            parameters.entityId = manualPaymentScripts.formContext.data.entity.getId();

            Process.callAction("rnt_ManualPayment",
                [{
                    key: "ManualPaymentParameters",
                    type: Process.Type.String,
                    value: JSON.stringify(parameters)
                }],
                function (data) {
                    let parsed = JSON.parse(data.ManualPaymentResponse)

                    if (parsed.ResponseResult.Result) {
                        //100000000 : processed
                        manualPaymentScripts.finishTransaction();
                        Xrm.Utility.closeProgressIndicator();

                        alert("Manuel ödeme başarılı");
                        //save the form.
                        manualPaymentScripts.formContext.data.entity.save();
                    }
                    else {

                        if (parameters.isDebt == true) {
                            manualPaymentScripts.finishTransaction();
                            Xrm.Utility.closeProgressIndicator();
                            alert("Ödeme başarısız. Müşteriye borç kaydı atanmıştır.");
                            manualPaymentScripts.formContext.data.entity.save();
                        }
                        else {
                            Xrm.Utility.closeProgressIndicator();
                            alert(parsed.ResponseResult.ExceptionDetail);
                        }
                    }
                },
                function (e, t) {
                    Xrm.Utility.closeProgressIndicator();
                    manualPaymentScripts.isClickedMakePaymentButton = false;

                    let message = e.split(":")[1];
                    alert(message);
                },
                null,
                true);
        }
    },
    noCloseCallback: function () {

    },
    makeManualPayment: function () {
        let creditCardType = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditCardType).getValue();

        if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.creditcardId).getValue() == null && creditCardType == 2) {
            manualPaymentScripts.formContext.ui.setFormNotification("Lütfen bir kredi kartını seçin!", XrmHelper.NotificationTypes.Error, "1000");
            return;
        }
        if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.nameonCard).getValue() == null && creditCardType == 1) {
            manualPaymentScripts.formContext.ui.setFormNotification("Kredi kartı üzerindeki isim bilgisini doğru giriniz!", XrmHelper.NotificationTypes.Error, "1000");
            return;
        }
        if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cardNumber).getValue() == null && creditCardType == 1) {
            manualPaymentScripts.formContext.ui.setFormNotification("Kredi kartı numarasını doğru giriniz!", XrmHelper.NotificationTypes.Error, "1000");
            return;
        }
        if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.month).getValue() == null && creditCardType == 1) {
            manualPaymentScripts.formContext.ui.setFormNotification("Ay bilgisini doğru giriniz!", XrmHelper.NotificationTypes.Error, "1000");
            return;
        }
        if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.year).getValue() == null && creditCardType == 1) {
            manualPaymentScripts.formContext.ui.setFormNotification("Yıl bilgisini doğru giriniz!", XrmHelper.NotificationTypes.Error, "1000");
            return;
        }
        if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cvc).getValue() == null && creditCardType == 1) {
            manualPaymentScripts.formContext.ui.setFormNotification("CVC bilgisini doğru giriniz!", XrmHelper.NotificationTypes.Error, "1000");
            return;
        }

        if (manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.manualPaymentType).getValue() == null) {
            alert("Manual ödeme türü zorunludur")
            return;
        }
        let manualPaymentType = manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.manualPaymentType).getValue()
        let type = manualPaymentType == 20 ? "Iade" : "Ödeme"
        Xrm.Utility.confirmDialog(type + " işlemi gerçekleştirilecektir.\nDevam etmek istiyor musunuz?", manualPaymentScripts.yesCloseCallback, manualPaymentScripts.noCloseCallback)

    },
    finishTransaction: function () {
        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.statusCode).setValue(100000000)
        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.month).setValue(null)
        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.year).setValue(null)
        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cvc).setValue(null)
        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cardNumber).setValue(null)
        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.nameonCard).setValue(null)

        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.nameonCard).setRequiredLevel(XrmHelper.RequiredLevels.None)
        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cardNumber).setRequiredLevel(XrmHelper.RequiredLevels.None)
        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.cvc).setRequiredLevel(XrmHelper.RequiredLevels.None)
        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.year).setRequiredLevel(XrmHelper.RequiredLevels.None)
        manualPaymentScripts.formContext.getAttribute(manualPaymentScripts.fields.month).setRequiredLevel(XrmHelper.RequiredLevels.None)
    }
};