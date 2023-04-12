let groupCodeListPriceTable = (function () {
    let that = {};
    that.init = function () {
        that.retrieveTableData();

        $('#tableForGroupCodeListPriceData').bootstrapTable({
            idField: 'name',
            columns: that.prepareTableColumns(),
            data: window.tableDataForGroupCodeListPrice.sort((a, b) => parseFloat(a.rnt_minimumday) - parseFloat(b.rnt_minimumday))
        });

        // editable on save event
        $("#tableForGroupCodeListPriceData").on("editable-save.bs.table", function (field, row, $el, oldValue) {
            var index = window.tableDataForGroupCodeListPrice.findIndex(x => x.rnt_groupcodepricelisttemplateId == $el.rnt_groupcodepricelisttemplateId);
            if (index >= 0) {
                window.tableDataForGroupCodeListPrice[index] = $el;
            }
            else {
                window.tableDataForGroupCodeListPrice.push($el);
            }

            //var innerThat = this;
            //setTimeout(function () {
            //    $(innerThat).find("tr[data-index=" + index + "]").next().find('.editable').editable('show')
            //}, 200);
        });

        // insert button click event
        $('#addButtonListPrice').click(function () {
            var obj = {
                rnt_groupcodepricelisttemplateId: that.generateGuid(),
                rnt_minimumday: '0',
                rnt_maximumday: '0',
                rnt_ratio: ''
            }

            $('#tableForGroupCodeListPriceData').bootstrapTable('insertRow', {
                index: 0,
                row: obj
            });
            
            // bind hidden event again
            //$(".editable").on('hidden', function (e, reason) {
            //    console.log("hidden");
            //    if (reason === 'save' || reason === 'nochange') {
            //        var that = this;
            //        $(".editable:visible").eq($(".editable:visible").index(that) + 1).editable('show');
            //    }
            //});
        });

        //automatically show next editable
        //$("#tableForGroupCodeListPriceData .editable").on('hidden', function (e, reason) {
        //    console.log("hidden");
        //    if (reason === 'save' || reason === 'nochange') {
        //        var that = this;
        //        $(".editable:visible").eq($(".editable:visible").index(that) + 1).editable('show');
        //    }
        //});
    }
    that.generateGuid = function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    that.prepareTableColumns = function () {
        let editable = {
            type: 'text', mode: 'inline',
            validate: function (value, d) {
                if ($.trim(value) == '') {
                    return 'This field is required';
                }
                var format = /^[0-9]+([.][0-9]+)?$/;
                if (!value.match(format)) {
                    return 'This field is not valid';
                }
                //else if ($.trim(value) == '0') {
                //    return 'Value cannot be 0';
                //}
            }
        }
        let columns = [
            { field: "rnt_groupcodepricelisttemplateId", title: "Id", visible: false },
            { field: "rnt_minimumday", title: "Min Day", editable },
            { field: "rnt_maximumday", title: "Max Day", editable },
            { field: "rnt_ratio", title: "Ratio %", editable }]
        return columns;
    }
    that.retrieveTableData = function () {
        var url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/rnt_groupcodepricelisttemplateSet?$select=rnt_groupcodepricelisttemplateId,rnt_maximumday,rnt_minimumday,rnt_value";
        XrmHelper.GetRecordsWithOData(url, false, function (data) {
            if (data.d.results.length > 0) {
                for (var i in data.d.results) {
                    let id = data.d.results[i]["rnt_groupcodepricelisttemplateId"];
                    let maxDay = data.d.results[i]["rnt_maximumday"]
                    let minDay = data.d.results[i]["rnt_minimumday"];
                    let value = data.d.results[i]["rnt_value"];
                    if (value != null)
                        value = parseFloat(value).toFixed(2);
                    window.tableDataForGroupCodeListPrice.push({ rnt_groupcodepricelisttemplateId: id, rnt_minimumday: minDay, rnt_maximumday: maxDay, rnt_ratio: value, });
                }
            }

        },
            function (e) {
                alert(e)
            });
    }
    that.validation = function () {
        var res = window.tableDataForGroupCodeListPrice.some(x => x.rnt_ratio == null || x.rnt_ratio == "");
        if (res) {
            return false;
        }
        return true;
    }
    return that;
})();