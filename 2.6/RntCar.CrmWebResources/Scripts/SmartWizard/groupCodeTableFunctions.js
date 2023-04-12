let groupCodeTable = (function () {
    let that = {};
    that.init = function () {
        $(".fixed-table-loading").hide();

        that.retrieveTableData();

        $('#tableForGroupCodeData').bootstrapTable({
            idField: 'name',
            columns: that.prepareTableColumns(),
            data: window.tableDataForGroupCode
        });

        // editable on save event
        $("#tableForGroupCodeData").on("editable-save.bs.table", function (field, row, $el, oldValue) {
            var index = window.tableDataForGroupCode.findIndex(x => x.rnt_groupcodeinformationsId == $el.rnt_groupcodeinformationsId);
            if (index >= 0)
                window.tableDataForGroupCode[index] = $el;

            //var innerThat = this;
            //setTimeout(function () {
            //    $(innerThat).find("tr[data-index=" + index + "]").next().find('.editable').editable('show')
            //}, 200);
            // XrmHelper.SetValue("rnt_groupcodevalue", null);
            // XrmHelper.SetValue("rnt_groupcodevalue", JSON.stringify(tableData));
            // XrmHelper.FireOnChange("rnt_groupcodevalue");
        });
        //$("#tableForGroupCodeData .editable").on('hidden', function (e, reason) {
        //    console.log("hidden");
        //    if (reason === 'save' || reason === 'nochange') {
        //        var that = this;
        //        $(".editable:visible").eq($(".editable:visible").index(that) + 1).editable('show');
        //    }
        //});
    }
    that.prepareTableColumns = function () {
        let editable = {
            type: 'number', mode: 'inline',
            validate: function (value) {
                //if ($.trim(value) == '') {
                //    return 'This field is required';
                //}
                if ($.trim(value) == '0') {
                    return 'Value cannot be zero';
                }
            }
        }
        let columns = [
            { field: "rnt_groupcodeinformationsId", title: "Id", visible: false },
            { field: "rnt_name", title: "Group Code" },
            { field: "rnt_price", title: "Price", editable }]
        return columns;
    }
    that.retrieveTableData = function () {
        var url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/rnt_groupcodeinformationsSet?$select=rnt_name,rnt_groupcodeinformationsId";
        XrmHelper.GetRecordsWithOData(url, false, function (data) {
            if (data.d.results.length > 0) {
                for (var i in data.d.results) {
                    let name = data.d.results[i]["rnt_name"];
                    let id = data.d.results[i]["rnt_groupcodeinformationsId"];
                    window.tableDataForGroupCode.push({ rnt_groupcodeinformationsId: id, rnt_name: name, rnt_price: '', });
                }
                //XrmHelper.SetValue("rnt_groupcodevalue", JSON.stringify(tableData));
            }

        },
            function (e) {
                alert(e)
            });
    }
    that.validation = function () {
        var nullCheck = window.tableDataForGroupCode.filter(x => x.rnt_price != "");
        if (nullCheck.length > 0)
            return true;
        return false;
    }
    return that;
})();