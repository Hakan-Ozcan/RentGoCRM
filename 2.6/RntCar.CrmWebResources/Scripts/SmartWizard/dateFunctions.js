let dateFunctions = (function () {
    let that = {}
    let nowTemp = new Date();
    let now = new Date(nowTemp.getFullYear(), nowTemp.getMonth(), nowTemp.getDate(), 0, 0, 0, 0);

    that.init = function () {
        let checkin = $('#beginDate').fdatepicker({
            initialDate: now,
            onRender: function (date) {
                return date.valueOf() < now.valueOf() ? 'disabled' : '';
            }
        }).on('changeDate', function (ev) {
            if (ev.date.valueOf() > checkout.date.valueOf()) {
                let newDate = new Date(ev.date)
                newDate.setDate(newDate.getDate() + 1);
                checkout.update(newDate);
            }
            checkin.hide();
            $('#endDate')[0].focus();
        }).data('datepicker');
        now.setDate(now.getDate() + 1);
        let checkout = $('#endDate').fdatepicker({
            initialDate: now,
            onRender: function (date) {
                return date.valueOf() <= checkin.date.valueOf() ? 'disabled' : '';
            }
        }).on('changeDate', function (ev) {
            checkout.hide();
        }).data('datepicker');
    }
    that.validation = function () {
        let beginDateTemp = new Date($('#beginDate')[0].value);
        let beginDate = new Date(beginDateTemp.getFullYear(), beginDateTemp.getMonth(), beginDateTemp.getDate(), 0, 0, 0, 0);

        let endDateTemp = new Date($('#endDate')[0].value);
        let endDate = new Date(endDateTemp.getFullYear(), endDateTemp.getMonth(), endDateTemp.getDate(), 0, 0, 0, 0);
        if (beginDate > endDate)
        {
            return false;
        }
        return true;
    }
    return that;
})();