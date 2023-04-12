let availabilityPriceListTable = (function () {
    let that = {};
    that.init = function () {
        that.retrieveTableData();
        $('#tableForAvailabilityListPriceData').bootstrapTable({
            idField: 'name',
            columns: that.prepareTableColumns(),
            data: window.tableDataForAvailabilityPriceList.sort((a, b) => parseFloat(a.rnt_minimumday) - parseFloat(b.rnt_minimumday))
        });

        // editable on save event
        $("#tableForAvailabilityListPriceData").on("editable-save.bs.table", function (field, row, $el, oldValue) {
        
            var index = window.tableDataForAvailabilityPriceList.findIndex(x => x.rnt_availabilitypricelisttemplateId == $el.rnt_availabilitypricelisttemplateId);

            if (index >= 0)
                window.tableDataForAvailabilityPriceList[index] = $el;
            
            var innerThat = this;
            //setTimeout(function () {
            //    if ($(innerThat).find("tr[data-index=" + index + "]").closest('td .editable').length > 0) {
            //        $(innerThat).find("tr[data-index=" + index + "]").closest('td .editable').editable('show')
            //    }
            //    else {
            //        $(innerThat).find("tr[data-index=" + index + "]").next().find('.editable').editable('show')
            //    }
                    
            //}, 200);
        });

        // insert button click event
        $('#addButtonAvailabilityListPrice').click(function () {
            var obj = {
                rnt_availabilitypricelisttemplateId: that.generateGuid(),
                rnt_maximumavailability: '0',
                rnt_minimumavailability: '0',
                rnt_ratio: ''
            }

            $('#tableForAvailabilityListPriceData').bootstrapTable('insertRow', {
                index: 0,
                row: obj
            });

            //// bind hidden event again
            //$(".editable").on('hidden', function (e, reason) {
            //    console.log("hidden");
            //    if (reason === 'save' || reason === 'nochange') {
            //        var that = this;
            //        $(".editable:visible").eq($(".editable:visible").index(that) + 1).editable('show');
            //    }
            //});
        });
    }
    that.prepareTableColumns = function () {
        let editable = {
            type: 'text', mode: 'inline',
            validate: function (value) {
                if ($.trim(value) == '') {
                    return 'This field is required';
                }
                var format = /^[0-9]+([.][0-9]+)?$/;
                if (!value.match(format)) {
                    return 'This field is not valid';
                }
            }
        }
        let columns = [
            { field: "rnt_availabilitypricelisttemplateId", title: "Id", visible: false },
            { field: "rnt_name", title: "Name", visible: false },
            { field: "rnt_minimumavailability", title: "Minimum Availability", editable },
            { field: "rnt_maximumavailability", title: "Maximum Availability", editable },
            { field: "rnt_ratio", title: "Ratio %", editable }]
        return columns;
    }
    that.generateGuid = function () {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    that.retrieveTableData = function () {
        var url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/rnt_availabilitypricelisttemplateSet?$select=rnt_availabilitypricelisttemplateId,rnt_name,rnt_maximumavailability,rnt_minimumavailability,rnt_value&$orderby=rnt_name asc";
        XrmHelper.GetRecordsWithOData(url, false, function (data) {
            if (data.d.results.length > 0) {
                for (var i in data.d.results) {
                    let id = data.d.results[i]["rnt_availabilitypricelisttemplateId"];
                    let name = data.d.results[i]["rnt_name"];
                    let value = data.d.results[i]["rnt_value"];
                    if (value != null)
                        value = parseFloat(value).toFixed(2);
                    window.tableDataForAvailabilityPriceList.push({ rnt_name: name, rnt_availabilitypricelisttemplateId: id, rnt_maximumavailability: data.d.results[i]["rnt_maximumavailability"], rnt_minimumavailability: data.d.results[i]["rnt_minimumavailability"], rnt_ratio: value, });
                }
            }

        },
            function (e) {
                alert(e);
            });
    }
    that.validation = function () {
        var res = window.tableDataForAvailabilityPriceList.some(x => x.rnt_ratio == null || x.rnt_ratio == "");
        if (res) {
            return false;
        }
        return true;
    }
    return that;
})();