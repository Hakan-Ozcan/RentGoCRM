var firstCreate;

var hgsLabelFunctions = {
    formContext: {},

    init: function (formContext) {
        hgsLabelFunctions.formContext = formContext.getFormContext();
        var formType = hgsLabelFunctions.formContext.ui.getFormType();
        if (formType == hgsLabelFunctions.formType.Create) {
            hgsLabelFunctions.formContext.getControl(hgsLabelFunctions.fields.equipmentId).setDisabled(false);
            hgsLabelFunctions.formContext.getControl(hgsLabelFunctions.fields.label).setDisabled(false);
            hgsLabelFunctions.formContext.getControl(hgsLabelFunctions.fields.hgsPaymentCardId).setVisible(false);
            hgsLabelFunctions.formContext.getControl(hgsLabelFunctions.fields.loadingAmount).setVisible(false);
            hgsLabelFunctions.formContext.getControl(hgsLabelFunctions.fields.loadingLowerLimit).setVisible(false);
            firstCreate=true;
        }
        else
        {
            if (firstCreate) {
                alert(hgsLabelFunctions.message.createsuccess);
                firstCreate = false;
            }
        }
    },
    formType: {
        Undefined: 0,
        Create: 1,
        Update: 2,
        ReadOnly: 3,
        Disabled: 4,
        BulkEdit: 6,
    },
    fields: {
        hgsPaymentCardId: "rnt_hgspaymentcardid",
        loadingLowerLimit: "rnt_loadinglowerlimit",
        loadingAmount: "rnt_loadingamount",
        equipmentId: "rnt_equipmentid",
        label: "rnt_label"
    },
    message: {
        createsuccess: "HGS İşlemi Başarılıdır"
    }
}