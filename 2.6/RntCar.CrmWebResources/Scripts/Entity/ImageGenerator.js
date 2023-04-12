var imageGenerator = {
    formContext: {},
    init: function (formContext) {
        imageGenerator.formContext = formContext.getFormContext();
    },
    generate: function () {

        if (sessionStorage.getItem("rnt_binaryImage") == null) {
            alert("Lütfen resim yükleyiniz");
            return;
        }
        if (imageGenerator.formContext.getAttribute("rnt_url").getValue() != null) {
            imageGenerator.formContext.ui.setFormNotification("URL'i oluşturulmuş , bir kayıt tekrar güncellenemez", XrmHelper.NotificationTypes.Error, "1000");
            return;
        }


        imageGenerator.formContext.data.save().then(
            function () {
                let id = imageGenerator.formContext.data.entity.getId()
                Xrm.Utility.showProgressIndicator("Resim URL'i oluşturuluyor.");
                Process.callAction("rnt_ImageGenerator",
                    [
                        {
                            key: "Image",
                            type: Process.Type.String,
                            value: sessionStorage.getItem("rnt_binaryImage")
                        },
                        {
                            key: "Type",
                            type: Process.Type.String,
                            value: window.parent.Xrm.Page.getAttribute("rnt_type").getValue()
                        },
                        {
                            key: "ImageGeneratorId",
                            type: Process.Type.String,
                            value: id
                        },
                        {
                            key: "Target",
                            type: Process.Type.EntityReference,
                            value: new Process.EntityReference("rnt_imagegenerator", Xrm.Page.data.entity.getId())
                        }
                    ],
                    function (data) {
                        Xrm.Utility.closeProgressIndicator();
                        let parsed = JSON.parse(data.Response)
                        if (parsed.ResponseResult.Result) {
                            sessionStorage.removeItem("rnt_binaryImage")
                            imageGenerator.formContext.data.refresh();
                        }
                        else {
                            alert(parsed.ResponseResult.ExceptionDetail);
                        }
                    },
                    function (e, t) {
                        Xrm.Utility.closeProgressIndicator();
                        let message = e.split(":")[1];
                        alert(message);
                        debugger
                    }
                );
            },
            function () {

            })
    }
}