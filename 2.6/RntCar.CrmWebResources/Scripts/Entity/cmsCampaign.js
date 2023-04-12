var campaignFunctions = {
    init: function (formContext) {
        campaignFunctions.formContext = formContext.getFormContext();
        campaignFunctions.events.CampaingOnChange();

        campaignFunctions.formContext.getAttribute(campaignFunctions.fields.Campaign).addOnChange(campaignFunctions.events.CampaingOnChange);
    },
    fields: {
        Campaign: "rnt_campaignid",
        MobileImageURL: "rnt_campaignmobileimageurl",
        BannerUrl:"rnt_capmaignbannerurl"
    },
    events: {
        CampaingOnChange: function () {
            let lookupValue = campaignFunctions.formContext.getAttribute(campaignFunctions.fields.Campaign).getValue()
            var url = campaignFunctions.formContext.context.getClientUrl() + "/xrmservices/2011/OrganizationData.svc/rnt_campaignSet(guid'" + lookupValue[0].id + "')?$select=rnt_reservationchannelcode";
            XrmHelper.GetRecordsWithOData(url, false,
                function (data) {
                    console.log("channel", data.d.results[0].rnt_reservationchannelcode.Value)
                },
                function (err) {
                    console.log("err",err)
                });
        }
    },
    onsave: function (context) {
        let formContext = context.getFormContext();
        
    }
}