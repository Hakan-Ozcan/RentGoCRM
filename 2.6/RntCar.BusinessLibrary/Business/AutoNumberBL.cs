using Microsoft.Xrm.Sdk;
using RntCar.BusinessLibrary.Handlers;
using RntCar.BusinessLibrary.Repository;
using RntCar.SDK.Common;
using System;

namespace RntCar.BusinessLibrary.Business
{
    public class AutoNumberBL : BusinessHandler
    {
        public AutoNumberBL(IOrganizationService orgService) : base(orgService)
        {
        }

        //public void CreateAutoNumber4GivenEntity(Entity entity)
        //{
            
        //    AutoNumberRepository repository = new AutoNumberRepository(this.OrgService);
        //    var autoNumberDefinitions = repository.GetAutoNumberDefinitions4GivenEntityAsIfPublished(entity);

        //    this.TraceWriteLine(autoNumberDefinitions.Entities.Count + "AutoNumber Record Found");

        //    foreach (var autoNumberEntity in autoNumberDefinitions.Entities)
        //    {
        //        var expression = autoNumberEntity.GetAttributeValue<String>("rnt_expression");
        //        var mySplittedSTring = expression.Split('-');
        //        var formattedString = String.Empty;
        //        foreach (var item in mySplittedSTring)
        //        {
        //            #region Template Builder
        //            //means it is an expression !
        //            if (item.Contains("{") && item.Contains("}"))
        //            {
        //                this.TraceWriteLine("expression value" + item);

        //                if (item.Contains("SEQNUM"))
        //                {
        //                    this.TraceWriteLine("SEQNUM" + item);

        //                    var subItem = item.Split(':');
        //                    if (subItem.Length == 2)
        //                    {
        //                        this.TraceWriteLine("Correct format guaranteed");
        //                        this.TraceWriteLine("SEQNUM is splitted with : " + item);

        //                        formattedString += StaticHelper.RandomDigits(Convert.ToInt32(subItem[1].Replace("}", "")));
        //                        this.TraceWriteLine("latestvalue : " + formattedString);
        //                    }

        //                }
        //                else if (item.Contains("RANDSTRING"))
        //                {
        //                    this.TraceWriteLine("RANDSTRING" + item);

        //                    var subItem = item.Split(':');
        //                    if (subItem.Length == 2)
        //                    {
        //                        this.TraceWriteLine("Correct format guaranteed");
        //                        this.TraceWriteLine("RANDSTRING is splitted with : " + item);

        //                        formattedString += StaticHelper.GenerateString(Convert.ToInt32(subItem[1].Replace("}", "")));
        //                        this.TraceWriteLine("latestvalue : " + formattedString);

        //                    }

        //                }

        //                else if (item.Contains("INC"))
        //                {
        //                    this.TraceWriteLine("INC" + item);

        //                    var subItem = item.Split(':');
        //                    if (subItem.Length == 2)
        //                    {
        //                        this.TraceWriteLine("Correct format guaranteed");

        //                        //default inc is one. value in definition entity always override
        //                        var counter = autoNumberEntity.Attributes.Contains("rnt_stepcounter") ? autoNumberEntity.GetAttributeValue<int>("rnt_stepcounter") : 1;
        //                        this.TraceWriteLine("Counter" + counter);

        //                        //start is always zero
        //                        var latestValue = autoNumberEntity.Attributes.Contains("rnt_latestvalue") ? autoNumberEntity.GetAttributeValue<int>("rnt_latestvalue") : 0;
        //                        this.TraceWriteLine("latestValue" + latestValue);

        //                        //new value assigned
        //                        var newVal = latestValue + counter;

        //                        formattedString += newVal.ToString("D" + subItem[1].Replace("}", ""));

        //                        this.TraceWriteLine("latestValue" + formattedString);


        //                        //also need to update related autonumber increment related fields
        //                        Entity updateAutoNumber = new Entity("rnt_autonumber");
        //                        updateAutoNumber.Id = autoNumberEntity.Id;
        //                        updateAutoNumber["rnt_latestvalue"] = newVal;
        //                        this.OrgService.Update(updateAutoNumber);

        //                        this.TraceWriteLine("related record updated with ID:" + autoNumberEntity.Id);

        //                    }
        //                }
        //                if (autoNumberEntity.GetAttributeValue<bool>("rnt_useseparator"))
        //                {
        //                    formattedString += autoNumberEntity.GetAttributeValue<String>("rnt_separatortype");
        //                }

        //            }
        //            else
        //            {
        //                this.TraceWriteLine("Not expression value" + item);

        //                formattedString += item;
        //            }
        //            #endregion
        //        }
        //        if (autoNumberEntity.GetAttributeValue<bool>("rnt_useseparator"))
        //        {
        //            formattedString = formattedString.Remove(formattedString.Length - 1);
        //        }
        //        //need to fill master entity fields
        //        entity[autoNumberEntity.GetAttributeValue<String>("rnt_attributename")] = formattedString;

        //        this.TraceWriteLine("all operations are done");

        //    }
        //}
    }
}
