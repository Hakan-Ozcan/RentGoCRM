let smartWizardFunctions = (function () {
    let that = {};
    let btnFinish = $('<button></button>').text('Finish')
        .addClass('btn btn-info btn-finish disabled')
        .on('click', function () {
            if (!$(this).hasClass('disabled')) {

                $.blockUI();

                typeText = $('#typeSelection')[0].value == 1 ? "Bireysel" : "Kurumsal"
                let priceListObj = {
                    rnt_name: typeText,
                    rnt_pricecodeid: $('#codeSelection')[0].value,
                    rnt_pricetype: $('#typeSelection')[0].value,
                    rnt_begindate: $('#beginDate')[0].value,
                    rnt_enddate: $('#endDate')[0].value
                }

                let priceListParameters = JSON.stringify(priceListObj);
                let groupCodeParameters = JSON.stringify(window.tableDataForGroupCode.filter(item => item.rnt_price != ""));
                let groupCodeListPriceParameters = JSON.stringify(window.tableDataForGroupCodeListPrice);
                let availabilityPriceListParameters = JSON.stringify(window.tableDataForAvailabilityPriceList);

                Process.callAction("rnt_ExecuteCreatePriceCalculation",
                    [{
                        key: "PriceListParameters",
                        type: Process.Type.String,
                        value: priceListParameters
                    },
                    {
                        key: "GroupCodeParameters",
                        type: Process.Type.String,
                        value: groupCodeParameters
                    },
                    {
                        key: "GroupCodeLisPriceParameters",
                        type: Process.Type.String,
                        value: groupCodeListPriceParameters
                    },
                    {
                        key: "AvailabilityPriceListParameters",
                        type: Process.Type.String,
                        value: availabilityPriceListParameters
                    }],
                    function (data) {
                        debugger;
                        let response = JSON.parse(data.ResponseResult)
                        $("#smartwizard li").each(function () {
                            $(this).removeClass("done")
                            $(this).removeClass("active")
                        });
                        $('.btn-finish').hide();
                        $('.sw-btn-next').hide()
                        $('.sw-btn-prev').hide()
                        $('#info').hide();
                        let url = XrmHelper.GetClientURL() + "/main.aspx?etn=rnt_pricelist&id=%7b" + response.priceListId + "%7d&newWindow=true&pagetype=entityrecord"
                        $('#success').append("<strong>Success!</strong> <p>Price list successfully created</p><p>If you want to see record <a target='_blank' href=" + url + ">click here</a></p>");
                        $('#success').show();
                        $.unblockUI()
                        //let respose = JSON.parse(data);
                    },
                    function (e, t) {
                        let message = e.split("CustomErrorMessagefinder:")
                        $("#customError").empty();
                        $("#customError").append("<strong>Error !</strong> " + message[1]);
                        $("#info").hide();
                        $("#customError").show();
                        $.unblockUI()
                        // Write the trace log to the dev console
                        if (window.console && console.error) {
                            console.error(e + "\n" + t);
                        }
                        return false;
                    });
            }
        });
    that.init = function () {
        $('#smartwizard').smartWizard({
            selected: 0,
            theme: 'arrows',
            transitionEffect: 'fade',
            showStepURLhash: true,
            toolbarSettings: {
                toolbarPosition: 'both',
                toolbarExtraButtons: [btnFinish]
            },
            anchorSettings: {
                markDoneStep: true, // add done css
                markAllPreviousStepsAsDone: true, // When a step selected by url hash, all previous steps are marked done
                removeDoneStepOnNavigateBack: true, // While navigate back done step after active step will be cleared
                enableAnchorOnDoneStep: true // Enable/Disable the done steps navigation
            }
        });

        $("#smartwizard").on("leaveStep", function (e, anchorObject, stepNumber, stepDirection) {
            debugger;
            $(".fixed-table-loading").hide();
            let elmForm = $("#form-step-" + stepNumber);
            // stepDirection === 'forward' :- this condition allows to do the form validation 
            // only on forward navigation, that makes easy navigation on backwards still do the validation when going next 
            if (stepDirection === 'forward' && elmForm) {
                if (stepNumber == 0) {
                    if (!dateFunctions.validation()) {
                        $("#error").html("<strong>Hata !</strong> Başlangıç tarihi bitiş tarihinden büyük olamaz.");
                        $("#error").show();
                        return false;
                    };
                }
                else if (stepNumber == 1) {
                    if (!groupCodeTable.validation()) {
                        $("#error").html("<strong>Hata !</strong> En az bir adet fiyat girilmelidir.");
                        $("#error").show();
                        return false;
                    };
                }
                else if (stepNumber == 2) {
                    if (!groupCodeListPriceTable.validation()) {
                        $("#error").html("<strong>Hata !</strong> Lütfen tüm alanları doldurun.");
                        $("#error").show();
                        return false;
                    }
                }
                else if (stepNumber == 3) {
                    if (!availabilityPriceListTable.validation()) {
                        $("#error").html("<strong>Hata !</strong> Lütfen tüm alanları doldurun.");
                        $("#error").show();
                        return false;
                    }
                }
                $("#info").show();
                $("#customError").hide();
            }
            $("#error").hide();
            return true;
        });
        $("#smartwizard").on("showStep", function (e, anchorObject, stepNumber, stepDirection) {
            // Enable finish button only on last step
            if (stepNumber == 4) {
                $('.btn-finish').removeClass('disabled');
            } else {
                $('.btn-finish').addClass('disabled');
            }
        });

        $("#typeSelection").on("change", function () {
            if (this.value == 2) {
                $('#smartwizard').smartWizard("stepState", [3], "hide");
            }
            else {
                $('#smartwizard').smartWizard("stepState", [3], "show");
            }
        })
    }
    return that;
})();