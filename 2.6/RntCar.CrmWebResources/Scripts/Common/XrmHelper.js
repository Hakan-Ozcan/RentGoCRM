var XrmObject = typeof (Xrm) == "undefined" ?
    window.parent.Xrm ?
        window.parent.Xrm.Page :
        {} :
    Xrm.Page;
var pureXrm = typeof (Xrm) == "undefined" ?
    window.parent.Xrm ?
        window.parent.Xrm :
        {} :
    Xrm;

var XrmHelper = {
    apiKey: "/api/data/v9.1/",
    // apiKey: "/api/data/v8.2/",
    isTurboFormsEnabled: true,// pureXrm.Internal.isTurboForm(),
    NotificationTypes: {
        Info: "INFO",
        Warning: "WARNING",
        Error: "ERROR",
    },
    TabState: {
        Expanded: "expanded",
        Collapsed: "collapsed",
    },
    FormTypes: {
        Create: 1,
        Update: 2,
        ReadOnly: 3,
        Disabled: 4,
        BulkEdit: 6

    },
    RequiredLevels: {
        Required: "required",
        Recommended: "recommended",
        None: "none"

    },
    EnvironmentType: {
        OnPremise: "onpremise",
        OnPremiseWithADFS: "onpremisewithadfs",
        Online: "online",
    },
    SubmitMode: {
        Always: "always",
        Never: "never",
        Dirty: "dirty",
    },
    Client: {
        Browser: "Web",
        Outlook: "Outlook",
        Mobile: "Mobile"
    },
    StageResults: {

        CrossEntity: "crossEntity",
        Invalid: "invalid",
        Unreachable: "unreachable",
        DirtyForm: "dirtyForm"
    },
    AttributeTypes: {
        Boolean: "boolean",
        DateTime: "datetime",
        Decimal: "decimal",
        Double: "double",
        Integer: "integer",
        Lookup: "lookup",
        Memo: "memo",
        Money: "money",
        OptionSet: "optionset",
        String: "string",
    },
    isAdminUser: function (formContext) {
        let returnval = false;
        let userRoles = formContext.context.getUserRoles()

        let userIsAdmin = this.CheckUserIsAdminUser(formContext.context);
        for (var i in userRoles) {
            if (userRoles[i].toString().replace("{", "").replace("}", "").toUpperCase() == "97FABB79-4E60-E911-A961-000D3A454F67" ||
                userIsAdmin) {
                returnval = true;
                break;
            }
        }
        return returnval;
    },
    enableCallCenter: function (formContext) {
        let userRoles = formContext.context.getUserRoles()

        let userIsAdmin = this.CheckUserIsAdminUser(formContext.context);
        for (var i in userRoles) {
            if (userRoles[i].toString().replace("{", "").replace("}", "").toUpperCase() == "97FABB79-4E60-E911-A961-000D3A454F67" ||
                userIsAdmin) {
                this.ChangeAllTabsDisableStatus(formContext, false);
                break;
            }
            else {
                this.ChangeAllTabsDisableStatus(formContext, true)
            }
        }
    },
    //Variables
    enableKeyUser: function (formContext) {
        let userRoles = formContext.context.getUserRoles()

        let userIsAdmin = this.CheckUserIsAdminUser(formContext.context);
        for (var i in userRoles) {
            if (userRoles[i].toString().replace("{", "").replace("}", "").toUpperCase() == "EAC03252-06E3-E911-A831-000D3A47CF3E" ||
                userIsAdmin) {
                this.ChangeAllTabsDisableStatus(formContext, false);
                break;
            }
            else {
                this.ChangeAllTabsDisableStatus(formContext, true)
            }
        }
    },
    retrieveData: function (entityName, fetchXml, successcallback, errorcallback, isAsync) {
        if (typeof (isAsync) == "undefined") {
            isAsync = true;
        }
        let encodedFetchXml = encodeURI(fetchXml)
        let url = this.GetClientURL() + this.apiKey + entityName + "?fetchXml=" + encodedFetchXml;
        let req = new XMLHttpRequest();
        req.open("GET", url, isAsync);
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");

        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                this.onreadystatechange = null;
                if (this.status === 200) {
                    var returned = JSON.parse(this.responseText);
                    successcallback(returned.value)
                }
                else {
                    let m = JSON.parse(this.responseText).error.message
                    errorcallback(m);
                }
            }
        };
        req.send();
    },
    IsXrmNan: function () {

    },
    CloseForm: function (submitchanges) {
        if (submitchanges == false) {
            XrmObject.ui.close();
        }
        else {
            var attributes = XrmObject.data.entity.attributes.get();

            for (var i in attributes) { attributes[i].setSubmitMode("never"); }

            XrmObject.ui.close();
        }
    },
    GetDisableStatus: function (fieldname) {
        return XrmObject.getControl(fieldname).getDisabled();
    },
    GetEntityName: function (formContext) {
        return formContext.data.entity.getEntityName();
    },
    GetClient: function () {
        return XrmObject.context.client.getClient();
    },
    GetEntityId: function (formContext) {
        return formContext.data.entity.getId();
    },
    GetFormType: function () {
        return XrmObject.ui.getFormType();
    },
    GetText: function (fieldname) {
        return XrmObject.getAttribute(fieldname).getText();
    },
    GetMessages: function (key, filename) {
        var message = null;
        if (this.GetEnvironmentTypeOfCRM() == XrmHelper.EnvironmentType.OnPremise)
            xmlurl = this.GetOrganizationName() + "/WebResources/rnt_/Data/Xml/" + filename;

        else if (this.GetEnvironmentTypeOfCRM() == XrmHelper.EnvironmentType.OnPremiseWithADFS)
            xmlurl = "/WebResources/rnt_/Data/Xml/" + filename

        else if (this.GetEnvironmentTypeOfCRM() == XrmHelper.EnvironmentType.Online)
            xmlurl = "../WebResources/rnt_/Data/Xml/" + filename

        $.ajax({
            type: "GET",
            url: xmlurl,
            dataType: "xml",
            async: false,
            success: function (xml) {
                var xml = XrmHelper.xmlToJson(xml);

                $.each(xml, function (index, value) {
                    $.each(value, function (tags, messages) {
                        $.each(messages, function (a, b) {
                            if (b["@attributes"] != undefined) {
                                if (b["@attributes"].id == XrmHelper.GetUserLanguage()) {
                                    message = b[key]["#text"];
                                }
                            }
                        });
                    });
                });
            },
            error: function (XMLHttpRequest, textStatus, errorThrow) {

                throw new Error(errorThrow);
            }

        });
        return message;

    },
    GetValue: function (formContext, fieldname) {
        if (XrmHelper.CheckFieldIsOnForm(formContext, fieldname))
            return formContext.getAttribute(fieldname).getValue()
        else
            return null;
    },
    GetUserLanguage: function () {
        return XrmObject.context.getUserLcid();
    },
    GetCurrentUserId: function () {
        var userid = XrmObject.context.getUserId();
        userid = userid.replace("{", "");
        userid = userid.replace("}", "");

        return userid;
    },
    GetUserLanguage: function () {
        return XrmObject.context.getUserLcid();
    },
    GetLabel: function (fieldname) {
        return XrmObject.getControl(fieldname).getLabel()
    },
    GetFieldType: function (fieldname) {
        if (XrmObject.getAttribute(fieldname) != null)
            return XrmObject.getAttribute(fieldname).getAttributeType();
    },
    GetClientURL: function () {
        return XrmObject.context.getClientUrl();
    },
    GetOrganizationName: function () {
        return XrmObject.context.getOrgUniqueName();
    },
    GetFixedOrganizationName: function () {
        if (XrmObject.context.getOrgUniqueName().toString().toUpperCase() == "SITSFA2")
            return "SITSFAFixed2";
        else if (XrmObject.context.getOrgUniqueName().toString().toUpperCase() == "CRMSFA")
            return "SFAFIXED";
        else if (XrmObject.context.getOrgUniqueName().toString().toUpperCase() == "PLANETSFA")
            return "VDFFIXEDKURUMSALTEST";
    },
    GetStageStatus: function () {
        return XrmObject.data.process.getStatus();
    },
    GetActiveStageId: function () {
        return XrmObject.data.process.getActiveStage().getId();
    },
    GetSelectedStageId: function () {
        return XrmObject.data.process.getSelectedStage().getId();
    },
    GetActiveStageName: function () {
        return XrmObject.data.process.getActiveStage().getName();
    },
    GetSelectedStageName: function () {
        return XrmObject.data.process.getSelectedStage().getName();
    },
    GetEnvironmentTypeOfCRM: function () {
        var URLGeneral = XrmHelper.GetClientURL().split("//");
        if (URLGeneral[1].split("/").length > 1 && URLGeneral[1].split("/")[1].toLowerCase() == XrmHelper.GetOrganizationName().toLowerCase()) {
            return XrmHelper.EnvironmentType.OnPremise;
        }
        else {
            //means adfs
            //means adfs
            if (URLGeneral[1].split(".").length > 1 &&
                (URLGeneral[1].split(".")[0].indexOf(XrmObject.context.getOrgUniqueName().toLowerCase()) != -1 || URLGeneral[1].split(".")[0].indexOf(XrmObject.context.getOrgUniqueName()) != -1)) {
                return XrmHelper.EnvironmentType.OnPremiseWithADFS;
            }
            //online
            else if (XrmHelper.GetClientURL().indexOf("dynamics.com") != -1) {
                return XrmHelper.EnvironmentType.Online;
            }
        }
    },
    GetCurrentFormId: function () {
        if (this.GetClient() != this.Client.Mobile) {
            if (XrmObject.ui.formSelector != null && XrmObject.ui.formSelector.getCurrentItem() != null)
                return XrmObject.ui.formSelector.getCurrentItem().getId();
            else return null;
        }
        else {
            return null;
        }
    },
    GetRecordsWithOData: function (url, isasync, successfunction, errorfunction) {
        $.ajax({
            type: "GET",
            contentType: "application/json;charset=utf-8",
            datatype: "json",
            url: url,
            async: isasync,
            cache: false,
            beforeSend: function (XMLHttpRequest) {
                XMLHttpRequest.setRequestHeader("Accept", "application/json");
            },
            success: function (data) {//On Successfull service call   
                successfunction(data);

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                errorfunction(XMLHttpRequest);
            }
        });
    },
    SetValue: function (fieldname, value) {
        return XrmObject.getAttribute(fieldname).setValue(value)
    },
    SetLookupValue: function (fieldname, value) {
        XrmObject.getAttribute(fieldname).setValue([{ id: value.Id, name: value.Name, entityType: value.LogicalName }]);
    },
    SetFormNotification: function (Message, NotificationType, Id) {
        XrmObject.ui.setFormNotification(Message, NotificationType, Id == undefined ? "1000" : Id);
    },
    ClearNotification: function (Id) {
        XrmObject.ui.clearFormNotification(Id);
    },
    SetFieldNotification: function (fieldname, Message) {
        XrmObject.getControl(fieldname).setNotification(Message);

    },
    ClearFieldNotification: function (fieldname) {
        XrmObject.getControl(fieldname).clearNotification();
    },
    ParseQueryString: function (url) {
        var vars = [], hash;
        var hashes = url.slice(url.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    },
    GetWebResourceUrl: function (fieldname) {
        FilterControl = XrmObject.ui.controls.get(fieldname);
        return FilterControl.getSrc();
    },
    SetActiveStage: function (StageId, CallbackFunction) {
        XrmObject.data.process.setActiveStage(StageId, CallbackFunction);
    },
    SetTabDisplayState: function (tabname, tabstate) {
        XrmObject.ui.tabs.get(tabname).setDisplayState(tabstate);
    },
    SetWebResourceUrl: function (fieldname, url) {
        FilterControl = XrmObject.ui.controls.get(fieldname);
        FilterControl.setSrc(url);
    },
    SetWebResourceQueryString: function (fieldname, querystring) {
        FilterControl = XrmObject.ui.controls.get(fieldname);
        var encodedString = encodeURIComponent(querystring);
        FilterControl.setSrc(FilterControl.getSrc() + encodedString);
    },
    AddLookupFilter: function (logicalname, filters) {
        if (filters == null || typeof (filters) !== "object") {
            return;
        }

        var filterXml = "<filter type='and'>";

        for (var key in filters) {
            if (XrmObject.getAttribute(key).getValue() != null) {
                var value = filters[key];
                if (filters[key] == null) {
                    value = XrmObject.getAttribute(key).getValue();
                    if (typeof (value) === "object") {
                        value = value[0].Id;
                    }
                }

                filterXml += "<condition attribute='" + key + "' operator='eq' value='" + value + "'/>";
            }
        }
        filterXml += "</filter>";
        XrmObject.getControl(logicalname).addCustomFilter(filterXml);

    },
    OpenEntityForm: function (entityname, id, parameters) {
        Xrm.Utility.openEntityForm(entityname, id, parameters);
    },
    SetRequiredLevel: function (fieldname, value) {
        XrmObject.getAttribute(fieldname).setRequiredLevel(value);
    },
    GetRequiredLevel: function (fieldname) {
        if (XrmObject.getAttribute(fieldname) != null)
            return XrmObject.getAttribute(fieldname).getRequiredLevel();
    },
    ChangeAllFieldsRequiredLevel: function (RequiredLevel) {
        var tabs = XrmObject.ui.tabs.get();
        for (var i in tabs) {
            var tabsections = tabs[i].sections.get();
            for (var j in tabsections) {
                var secname = tabsections[j].getName();

                var ctrlName = XrmObject.ui.controls.get();
                for (var i in ctrlName) {
                    var ctrl = ctrlName[i];
                    if (ctrl.getParent() != null) {
                        var ctrlSection = ctrl.getParent().getName();
                        if (ctrlSection == secname && ctrl.getControlType() != "subgrid" && ctrl.getControlType() != "webresource" && ctrl.getControlType() != "iframe") {
                            XrmHelper.SetRequiredLevel(ctrl.getName(), RequiredLevel);
                        }
                    }
                }
            }
        }
    },
    GetRequiredFieldsOfCurrentFormForGivenValue: function (RequiredLevel) {
        var returnArr = []
        var tabs = XrmObject.ui.tabs.get();
        for (var i in tabs) {
            var tabsections = tabs[i].sections.get();
            for (var j in tabsections) {
                var secname = tabsections[j].getName();

                var ctrlName = XrmObject.ui.controls.get();
                for (var i in ctrlName) {
                    var ctrl = ctrlName[i];
                    if (ctrl.getParent() != null) {
                        var ctrlSection = ctrl.getParent().getName();
                        if (ctrlSection == secname && ctrl.getControlType() != "subgrid" && ctrl.getControlType() != "webresource" && ctrl.getControlType() != "iframe") {
                            if (XrmHelper.GetRequiredLevel(ctrl.getName()) == RequiredLevel) {
                                var obj = {};
                                obj.FieldName = ctrl.getName();
                                obj.RequiredLevel = XrmHelper.GetRequiredLevel(ctrl.getName());
                                returnArr.push(obj);
                            }
                        }
                    }
                }
            }
        }
        return returnArr;
    },
    GetDisabledFieldsOfCurrentForm: function () {
        var returnArr = []
        var tabs = XrmObject.ui.tabs.get();
        for (var i in tabs) {
            var tabsections = tabs[i].sections.get();
            for (var j in tabsections) {
                var secname = tabsections[j].getName();

                var ctrlName = XrmObject.ui.controls.get();
                for (var i in ctrlName) {
                    var ctrl = ctrlName[i];
                    if (ctrl.getParent() != null) {
                        var ctrlSection = ctrl.getParent().getName();
                        if (ctrlSection == secname && ctrl.getControlType() != "subgrid" && ctrl.getControlType() != "webresource" && ctrl.getControlType() != "iframe") {
                            if (XrmHelper.GetDisableStatus(ctrl.getName())) {
                                var obj = {};
                                obj.FieldName = ctrl.getName();
                                obj.DisableStatus = true;
                                returnArr.push(obj);
                            }
                        }
                    }
                }
            }
        }
        return returnArr;
    },
    GetAllAttributeOfCurrentFormForGivenValue: function (AttributeTypes) {
        var returnArr = []
        var tabs = XrmObject.ui.tabs.get();
        for (var i in tabs) {
            var tabsections = tabs[i].sections.get();
            for (var j in tabsections) {
                var secname = tabsections[j].getName();

                var ctrlName = XrmObject.ui.controls.get();
                for (var t in ctrlName) {
                    var ctrl = ctrlName[t];
                    if (ctrl.getParent() != null) {
                        var ctrlSection = ctrl.getParent().getName();
                        if (ctrlSection == secname && ctrl.getControlType() != "subgrid" && ctrl.getControlType() != "webresource" && ctrl.getControlType() != "iframe") {
                            if ($.inArray(XrmHelper.GetFieldType(ctrl.getName()), AttributeTypes) != -1) {
                                var obj = {};
                                obj.FieldName = ctrl.getName();
                                obj.FieldType = XrmHelper.GetFieldType(ctrl.getName());
                                returnArr.push(obj);
                            }
                        }
                    }
                }
            }
        }
        return returnArr;
    },
    GetAllAttributeOfCurrentForm: function () {
        var returnArr = []
        var tabs = XrmObject.ui.tabs.get();
        for (var i in tabs) {
            var tabsections = tabs[i].sections.get();
            for (var j in tabsections) {
                var secname = tabsections[j].getName();

                var ctrlName = XrmObject.ui.controls.get();
                for (var t in ctrlName) {
                    var ctrl = ctrlName[t];
                    if (ctrl.getParent() != null) {
                        var ctrlSection = ctrl.getParent().getName();
                        if (ctrlSection == secname && ctrl.getControlType() != "subgrid" && ctrl.getControlType() != "webresource" && ctrl.getControlType() != "iframe") {

                            var obj = {};
                            obj.FieldName = ctrl.getName();
                            obj.FieldType = XrmHelper.GetFieldType(ctrl.getName());
                            returnArr.push(obj);

                        }
                    }
                }
            }
        }
        return returnArr;
    },
    FieldDisable: function (fieldname, value) {
        if (XrmObject.getControl(fieldname) != null)
            XrmObject.getControl(fieldname).setDisabled(value);
    },
    ControlDisable: function (controlname, value) {
        if (XrmObject.getControl(controlname) != null)
            XrmObject.getControl(controlname).setDisabled(value);
    },
    ControlVisible: function (controlname, value) {
        if (XrmObject.getControl(controlname) != null)
            XrmObject.getControl(controlname).setVisible(value);
    },
    TabDisable: function (tabname, disablestatus) {
        var tab = XrmObject.ui.tabs.get(tabname);
        if (tab == null) alert("Error: The tab: " + tabname + " is not on the form");
        else {
            var tabsections = tab.sections.get();
            for (var i in tabsections) {
                var secname = tabsections[i].getName();
                this.SectionDisable(secname, disablestatus);
            }
        }
    },
    SectionDisable: function (formContext, sectionname, disablestatus) {
        var ctrlName = formContext.ui.controls.get();
        for (var i in ctrlName) {
            var ctrl = ctrlName[i];
            if (ctrl.getParent() != null) {
                var ctrlSection = ctrl.getParent().getName();
                if (ctrlSection == sectionname && ctrl.getControlType() != "subgrid" && ctrl.getControlType() != "webresource" && ctrl.getControlType() != "iframe") {
                    ctrl.setDisabled(disablestatus);
                }
            }
        }
    },
    ProcessFieldVisibility: function (fieldname, visibility) {
        XrmObject.getControl("header_process_" + fieldname).setVisible(visibility);
    },
    FieldVisibility: function (fieldname, visibility) {
        XrmObject.getControl(fieldname).setVisible(visibility);
    },
    TabVisibility: function (tabname, visibility) {
        XrmObject.ui.tabs.get(tabname).setVisible(visibility);
    },
    BusinessProcessFlowVisiblity: function (value) {
        XrmObject.ui.process.setVisible(value);
    },
    SectionVisibility: function (tabname, sectionname, value) {
        XrmObject.ui.tabs.get(tabname).sections.get(sectionname).setVisible(value);
    },
    ChangeAllTabsVisibility: function (value) {
        var tabs = XrmObject.ui.tabs.get();
        for (var i in tabs) {
            var tab = tabs[i];
            this.TabVisibility(tab.getName(), value);
        }
    },
    ChangeAllTabsDisableStatus: function (formContext, disablestatus) {
        var tabs = formContext.ui.tabs.get();
        for (var i in tabs) {
            var tabsections = tabs[i].sections.get();
            for (var j in tabsections) {
                var secname = tabsections[j].getName();
                this.SectionDisable(formContext, secname, disablestatus);
            }
        }
    },
    NavigateForm: function (formid) {

        var items = XrmObject.ui.formSelector.items.get();
        for (var i in items) {
            var form = items[i];
            if (form.getId().toString().toUpperCase() == formid) {
                form.navigate();
            }
        }
    },
    SetSubmitMode: function (fieldname, submitmode) {
        XrmObject.getAttribute(fieldname).setSubmitMode(submitmode);
    },
    AddOnChange: function (formContext, fieldname, functionname, parameters) {
        formContext.getAttribute(fieldname).addOnChange(function () { functionname.apply(null, parameters); });
    },
    FireOnChange: function (fieldname) {
        XrmObject.getAttribute(fieldname).fireOnChange()
    },
    AddKeyPress: function (formContext, fieldname, functionname) {
        formContext.getControl(fieldname).addOnKeyPress(functionname);
    },
    RemoveKeyPress: function (fieldname, functionname) {
        XrmObject.getControl(fieldname).removeOnKeyPress(functionname)
    },
    FireKeyPress: function (fieldname) {
        XrmObject.getControl(fieldname).fireOnKeyPress()
    },
    AddStateChange: function (functionname) {
        XrmObject.data.process.addOnStageChange(functionname);
    },
    AddStatusChange: function (functionname) {
        XrmObject.data.process.addOnProcessStatusChange(functionname);
    },
    AddStateSelected: function (functionname) {
        XrmObject.data.process.addOnStageSelected(functionname);
    },
    SetFocus: function (fieldname) {
        XrmObject.ui.controls.get(fieldname).setFocus();
    },
    RemoveOption: function (fieldname, value) {
        XrmObject.getControl(fieldname).removeOption(value);
    },
    ClearOptions: function (fieldname) {
        XrmObject.getControl(fieldname).clearOptions();
    },
    AddOption: function (fieldname, optionsettext, optionsetvalue) {
        XrmObject.getControl(fieldname).addOption(optionsettext, optionsetvalue);
    },
    CheckFieldIsOnForm: function (formContext, fieldname) {
        if (formContext.getAttribute(fieldname) != null) {
            return true;
        }
        return false;
    },
    CheckUserIsAdminUser: function (context) {
        let currentUserId = context.getUserId().replace("{", "").replace("}", "");
        let adminUserId = null;
        let url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/rnt_systemparameterSet?$select=rnt_systemuserid";
        XrmHelper.GetRecordsWithOData(url, false,
            function (data) {
                adminUserId = data.d.results[0].rnt_systemuserid.Id.toUpperCase();
            },
            function () {

            });


        if (currentUserId == adminUserId) {
            return true;
        }
        var otherUserList = XrmHelper.GetConfigurationByName("otherAdminList");
        if (otherUserList != null && otherUserList.includes(currentUserId.toLowerCase())) {
            return true;
        }
        return false;
    },
    MongoSyncOperationUserList: function (context) {
        let currentUserId = context.getUserId().replace("{", "").replace("}", "").toLowerCase();
        var mongoSyncOperationUserList = XrmHelper.GetConfigurationByName("MongoSyncOperationUserList");
        if (mongoSyncOperationUserList != null && mongoSyncOperationUserList.includes(currentUserId.toLowerCase())) {
            return true;
        }
        return false;
    },
    RefreshRibbon: function () {
        XrmObject.ui.refreshRibbon()
    },
    SaveForm: function () {
        XrmObject.data.entity.save();
    },
    CallWorkflow: function (workflowId, recordId, successCallback, errorCallback, isAsync) {

        url = this.GetClientURL();
        var request = "<s:Envelope xmlns:s='http://schemas.xmlsoap.org/soap/envelope/'>" +
            "<s:Body>" +
            "<Execute xmlns='http://schemas.microsoft.com/xrm/2011/Contracts/Services' xmlns:i='http://www.w3.org/2001/XMLSchema-instance'>" +
            "<request i:type='b:ExecuteWorkflowRequest' xmlns:a='http://schemas.microsoft.com/xrm/2011/Contracts' xmlns:b='http://schemas.microsoft.com/crm/2011/Contracts'>" +
            "<a:Parameters xmlns:c='http://schemas.datacontract.org/2004/07/System.Collections.Generic'>" +
            "<a:KeyValuePairOfstringanyType>" +
            "<c:key>EntityId</c:key>" +
            "<c:value i:type='d:guid' xmlns:d='http://schemas.microsoft.com/2003/10/Serialization/'>" + recordId + "</c:value>" +
            "</a:KeyValuePairOfstringanyType>" +
            "<a:KeyValuePairOfstringanyType>" +
            "<c:key>WorkflowId</c:key>" +
            "<c:value i:type='d:guid' xmlns:d='http://schemas.microsoft.com/2003/10/Serialization/'>" + workflowId + "</c:value>" +
            "</a:KeyValuePairOfstringanyType>" +
            "</a:Parameters>" +
            "<a:RequestId i:nil='true' />" +
            "<a:RequestName>ExecuteWorkflow</a:RequestName>" +
            "</request>" +
            "</Execute>" +
            "</s:Body>" +
            "</s:Envelope>";

        var req = new XMLHttpRequest();
        req.open("POST", url + "/XRMServices/2011/Organization.svc/web", isAsync);

        req.setRequestHeader("Accept", "application/xml, text/xml, */*");
        req.setRequestHeader("Content-Type", "text/xml; charset=utf-8");
        req.setRequestHeader("SOAPAction", "http://schemas.microsoft.com/xrm/2011/Contracts/Services/IOrganizationService/Execute");
        req.onreadystatechange = function () {
            if (req.readyState == 4) {
                if (req.status == 200) {
                    if (successCallback) {
                        successCallback();
                    }
                }
                else {
                    if (errorCallback) {
                        errorCallback();
                    }
                }
            }
        };

        req.send(request);
    },
    IsDirty: function (fieldname) {
        if (XrmObject.getAttribute(fieldname) == null)
            return false;
        return XrmObject.getAttribute(fieldname).getIsDirty();
    },
    isFormDirty: function () {
        return XrmObject.data.entity.getIsDirty()
    },
    PreventSave: function (context) {
        let saveEvent = context.getEventArgs();
        saveEvent.preventDefault();
    },
    PreventCharacters: function (context, fieldName) {
        let val = context.getControl(fieldName).getValue()
        if (val != null)
            val = val.replace(/\D/g, '');
        context.getAttribute(fieldName).setValue(val)
    },
    GetConfigurationByName: function (name) {
        var configValue = "";
        let url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/rnt_configurationSet?$select=rnt_value&$filter=rnt_name eq '" + name + "'";
        XrmHelper.GetRecordsWithOData(url, false,
            function (data) {
                configValue = data.d.results[0].rnt_value
            },
            function () {

            });
        return configValue;
    },
    GetConfigurationByName_Unified: function (formContext, name) {
        var configValue = "";
        let url = formContext.context.getClientUrl() + "/xrmservices/2011/OrganizationData.svc/rnt_configurationSet?$select=rnt_value&$filter=rnt_name eq '" + name + "'";
        XrmHelper.GetRecordsWithOData(url, false,
            function (data) {
                configValue = data.d.results[0].rnt_value
            },
            function () {

            });
        return configValue;
    },
    GetCarModelPreviewHeightandWidth: function () {
        let obj = {};
        let url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/rnt_systemparameterSet?$select=rnt_carmodelimageheight,rnt_carmodelimagewidth";
        XrmHelper.GetRecordsWithOData(url, false,
            function (data) {
                obj.ModelHeight = data.d.results[0].rnt_carmodelimageheight;
                obj.ModelWidth = data.d.results[0].rnt_carmodelimagewidth

            },
            function () {

            });
        return obj;
    },
    CheckIsCouponCodeProcessEnable: function () {
        let result = false;
        let url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/rnt_systemparameterSet?$select=rnt_iscouponcodeprocessenable";
        XrmHelper.GetRecordsWithOData(url, false,
            function (data) {
                result = data.d.results[0].rnt_iscouponcodeprocessenable;
            },
            function () {

            });
        return result;
    },
    //subgrid functions 
    GetSelectedSubGridRows: function (subgridname) {
        return XrmObject.getControl(subgridname).getGrid().getSelectedRows();
    },
    /////////  Not Crm Functions   /////////
    xmlToJson: function (xml) {
        var obj = {};
        if (xml.nodeType == 1) {
            if (xml.attributes.length > 0) {
                obj["@attributes"] = {};
                for (var j = 0; j < xml.attributes.length; j++) {
                    var attribute = xml.attributes.item(j);
                    obj["@attributes"][attribute.nodeName] = attribute.nodeValue;
                }
            }
        } else if (xml.nodeType == 3) {
            obj = xml.nodeValue;
        }
        if (xml.hasChildNodes()) {
            for (var i = 0; i < xml.childNodes.length; i++) {
                var item = xml.childNodes.item(i);
                var nodeName = item.nodeName;
                if (typeof (obj[nodeName]) == "undefined") {
                    obj[nodeName] = this.xmlToJson(item);
                } else {
                    if (typeof (obj[nodeName].push) == "undefined") {
                        var old = obj[nodeName];
                        obj[nodeName] = [];
                        obj[nodeName].push(old);
                    }
                    obj[nodeName].push(this.xmlToJson(item));
                }
            }
        }
        return obj;
    },
    StringFormat: function () {
        var s = arguments[0];
        for (var i = 0; i < arguments.length - 1; i++) {
            var reg = new RegExp("\\{" + i + "\\}", "gm");
            s = s.replace(reg, arguments[i + 1]);
        }
        return s;
    },
    // Convert /Date(1212154)/ type dates to Js Date Object or Formatted String
    FixAspNetDatesOnJsObject: function (obj, format) {
        var result = {};

        for (var key in obj) {
            var dateISO = /\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(?:[.,]\d+)?Z/i;
            var dateNet = /Date\(([^)]+)\)/;

            if (typeof (obj[key]) === "string") {
                var value;

                if (dateISO.test(obj[key])) {
                    value = new Date(obj[key]);
                    value = (typeof (format) !== "undefined") ? XrmHelper.FormatDate(value, format) : value;
                }
                if (dateNet.test(obj[key])) {
                    value = new Date(parseInt(dateNet.exec(obj[key])[1], 10));
                    value = (typeof (format) !== "undefined") ? XrmHelper.FormatDate(value, format) : value;
                }
                else {
                    value = obj[key];
                }
            }
            else {
                value = obj[key];
            }

            result[key] = value;
        }

        return result;
    },
    FormatDate: function (date, format) {
        var d = new Date();
        d.setTime(date.getTime());

        var retVal = format
            .replace('yyyy', d.getFullYear())
            .replace('MM', (d.getMonth() < 9 ? '0' : '') + (d.getMonth() + 1))
            .replace('dd', (d.getDate() < 10 ? '0' : '') + d.getDate())
            .replace('HH', (d.getHours() < 10 ? '0' : '') + d.getHours())
            .replace('mm', (d.getMinutes() < 10 ? '0' : '') + d.getMinutes())
            .replace('ss', (d.getSeconds() < 10 ? '0' : '') + d.getSeconds());

        return retVal;

    },
    ValidateCitizenshipNumber: function (value) {

        var KimlikNo = value
        if (KimlikNo == null || KimlikNo == "")
            return true;
        KimlikNo = String(KimlikNo);
        if (!KimlikNo.match(/^[0-9]{11}$/)) {

            return false;
        }

        pr1 = parseInt(KimlikNo.substr(0, 1));
        pr2 = parseInt(KimlikNo.substr(1, 1));
        pr3 = parseInt(KimlikNo.substr(2, 1));
        pr4 = parseInt(KimlikNo.substr(3, 1));
        pr5 = parseInt(KimlikNo.substr(4, 1));
        pr6 = parseInt(KimlikNo.substr(5, 1));
        pr7 = parseInt(KimlikNo.substr(6, 1));
        pr8 = parseInt(KimlikNo.substr(7, 1));
        pr9 = parseInt(KimlikNo.substr(8, 1));
        pr10 = parseInt(KimlikNo.substr(9, 1));
        pr11 = parseInt(KimlikNo.substr(10, 1));

        if ((pr1 + pr3 + pr5 + pr7 + pr9 + pr2 + pr4 + pr6 + pr8 + pr10) % 10 != pr11) {
            return false;
        }
        if (((pr1 + pr3 + pr5 + pr7 + pr9) * 7 + (pr2 + pr4 + pr6 + pr8) * 9) % 10 != pr10) {
            return false;
        }
        if (((pr1 + pr3 + pr5 + pr7 + pr9) * 8) % 10 != pr11) {
            return false;
        }

        return true;
    },
    ValidateTaxNumber: function (value) {
        if (value == null || value == "")
            return false;

        var v1 = 0, v2 = 0, v3 = 0, v4 = 0, v5 = 0, v6 = 0, v7 = 0, v8 = 0, v9 = 0,
            v11 = 0, v22 = 0, v33 = 0, v44 = 0, v55 = 0, v66 = 0, v77 = 0, v88 = 0, v99 = 0,
            lastDigit = 0, sum = 0;
        if (value.length == 10) {
            v1 = (Number(value.charAt(0)) + 9) % 10;
            v2 = (Number(value.charAt(1)) + 8) % 10;
            v3 = (Number(value.charAt(2)) + 7) % 10;
            v4 = (Number(value.charAt(3)) + 6) % 10;
            v5 = (Number(value.charAt(4)) + 5) % 10;
            v6 = (Number(value.charAt(5)) + 4) % 10;
            v7 = (Number(value.charAt(6)) + 3) % 10;
            v8 = (Number(value.charAt(7)) + 2) % 10;
            v9 = (Number(value.charAt(8)) + 1) % 10;
            lastDigit = Number(value.charAt(9));
            v11 = (v1 * 512) % 9;
            v22 = (v2 * 256) % 9;
            v33 = (v3 * 128) % 9;
            v44 = (v4 * 64) % 9;
            v55 = (v5 * 32) % 9;
            v66 = (v6 * 16) % 9;
            v77 = (v7 * 8) % 9;
            v88 = (v8 * 4) % 9;
            v99 = (v9 * 2) % 9;
            if (v1 != 0 && v11 == 0) { v11 = 9; }
            if (v2 != 0 && v22 == 0) { v22 = 9; }
            if (v3 != 0 && v33 == 0) { v33 = 9; }
            if (v4 != 0 && v44 == 0) { v44 = 9; }
            if (v5 != 0 && v55 == 0) { v55 = 9; }
            if (v6 != 0 && v66 == 0) { v66 = 9; }
            if (v7 != 0 && v77 == 0) { v77 = 9; }
            if (v8 != 0 && v88 == 0) { v88 = 9; }
            if (v9 != 0 && v99 == 0) { v99 = 9; }
            sum = v11 + v22 + v33 + v44 + v55 + v66 + v77 + v88 + v99;
            sum = (sum % 10 == 0) ? 0 : (10 - (sum % 10));
            if (sum == lastDigit)
                return true;
            else {
                return false;
            }
        }

        return false;
    },
    FormatPhoneFields: function (fieldname, notsetvalue) {
        if (this.GetValue(fieldname) == null || this.GetValue(fieldname) == "") {
            return true;
        }

        var scrubbed = this.GetValue(fieldname).toString().replace(/[^0-9]/g, "");

        var elevenDigitFormat = /^\(?([0-9]{4})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{2})[-. ]?([0-9]{2})$/;
        var tenDigitFormat = /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{2})[-. ]?([0-9]{2})$/;

        var digitedformat = null;
        if (elevenDigitFormat.test(scrubbed)) {
            if (notsetvalue == undefined) {
                digitedformat = scrubbed.replace(elevenDigitFormat, "($1) $2 $3 $4");
                this.SetValue(fieldname, digitedformat);
            }
            return true;
        }
        else if (tenDigitFormat.test(scrubbed)) {
            if (notsetvalue == undefined) {
                digitedformat = scrubbed.replace(tenDigitFormat, "($1) $2 $3 $4");
                this.SetValue(fieldname, digitedformat);
            }
            return true;
        }
        else {
            return false;
        }

    },
    FormatNumericFields: function (value) {
        return value.toString().replace(/[^0-9\.]+/g, "");
    },
    //-1 if a < b
    //0 if a = b
    //1 if a > b
    //NaN if a or b is an illegal date
    compareDates: function (a, b) {
        return (
            isFinite(a = this.convert(a).valueOf()) &&
                isFinite(b = this.convert(b).valueOf()) ?
                (a > b) - (a < b) :
                NaN
        );
    },
    convert: function (d) {
        // Converts the date in d to a date-object. The input can be:
        //   a date object: returned without modification
        //  an array      : Interpreted as [year,month,day]. NOTE: month is 0-11.
        //   a number     : Interpreted as number of milliseconds
        //                  since 1 Jan 1970 (a timestamp) 
        //   a string     : Any format supported by the javascript engine, like
        //                  "YYYY/MM/DD", "MM/DD/YYYY", "Jan 31 2009" etc.
        //  an object     : Interpreted as an object with year, month and date
        //                  attributes.  **NOTE** month is 0-11.
        return (
            d.constructor === Date ? d :
                d.constructor === Array ? new Date(d[0], d[1], d[2]) :
                    d.constructor === Number ? new Date(d) :
                        d.constructor === String ? new Date(d) :
                            typeof d === "object" ? new Date(d.year, d.month, d.date) :
                                NaN
        );
    },
    getRandomNumberForGivenForGivenLength: function (length) {
        var zeros = "";
        for (var i = 0; i < length; i++) {
            zeros += "0";
        }
        return Math.floor(Math.random() * parseInt("1" + zeros));
    },
    calculateAge: function (dateInput) {
        if (dateInput != null) {
            var ageDifMs = Date.now() - dateInput.getTime();
            var ageDate = new Date(ageDifMs); // miliseconds from epoch
            return Math.abs(ageDate.getUTCFullYear() - 1970);
        }
    },
    removeAlert: function () {
        setTimeout(XrmHelper.errorMessageCheckInterval, 1000);
    },
    errorMessageCheckInterval: function () {
        if (window.top.$(".ms-crm-InlineDialogBackground").length >= 1) {
            Alert.hide()
        }
        setTimeout(XrmHelper.errorMessageCheckInterval, 200);
    },
    getUserRoles: function () {
        //return XrmObject.context.getUserRoles();

        return pureXrm.Utility.getGlobalContext().userSettings.securityRoles;
    },
    getRoleNameById: function (userRoleId) {
        var roleName = "";
        let url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/RoleSet?$top=1&$filter=RoleId eq guid'" + userRoleId + "'&$select=Name";
        XrmHelper.GetRecordsWithOData(url, false,
            function (data) {
                roleName = data.d.results[0].Name
            },
            function () {

            });
        return roleName;
    },
    checkUserRoleByName: function (roleName) {
        var currentUserRoles = this.getUserRoles();
        for (var i = 0; i < currentUserRoles.length; i++) {
            var userRoleId = currentUserRoles[i];
            var userRoleName = this.getRoleNameById(userRoleId);
            if (userRoleName == roleName) {
                return true;
            }
        }
        return false;
    },
    checkUserRoleByNameForCallCenter: function () {
        let returnVal = false
        var roleName = XrmHelper.GetConfigurationByName("callCenterRoleName");
        var currentUserRoles = this.getUserRoles();
        var fetchXml =
            "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>" +
            "<entity name='systemuser'>" +
            "<attribute name='fullname' />" +
            "<filter type='and'>" +
            "<condition attribute='systemuserid' operator='eq-userid' />" +
            "</filter>" +
            "<link-entity name='systemuserroles' from='systemuserid' to='systemuserid' visible='false' intersect='true'>" +
            "<link-entity name='role' from='roleid' to='roleid' alias='r'>" +
            "<filter type='and'>" +
            "<condition attribute='name' operator='like' value='%" + roleName + "%' />" +
            "</filter>" +
            "</link-entity>" +
            "</link-entity>" +
            "</entity>" +
            "</fetch>";

        XrmHelper.retrieveData("systemusers", fetchXml,
            function (data) {
                if (data.length > 0)
                    returnVal = true;
            },
            function (data) {
                returnVal = false;
            },
            false)
        return returnVal;
    },
    getUserNameById: function (userId) {
        var userName = null;
        let url = XrmHelper.GetClientURL() + "/xrmservices/2011/OrganizationData.svc/SystemUserSet(guid'" + userId + "')?&$select=FullName";
        XrmHelper.GetRecordsWithOData(url, false,
            function (data) {
                userName = data.d.FullName;
            },
            function () {

            });
        return userName;
    }
};

window.XrmHelper = XrmHelper;