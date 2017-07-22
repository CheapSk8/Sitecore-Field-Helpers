using System;
using System.Collections.Generic;
using System.Linq;

using SC = Sitecore;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;

using FH = Specialized.Content.FieldHelpers;
using System.Collections;

namespace Specialized.Content {

    /// <summary>
    /// Extension methods for Sitecore
    /// </summary>
    public static partial class FieldExtensions
    {

        /// <summary>
        /// Set value of Field according to Field type
        /// </summary>
        /// <param name="field">Sitecore Field</param>
        /// <param name="value">Value of Field</param>
        public static void SetValueByType(this Field field, object value)
        {
            switch (field.Type)
            {
                case "Checkbox":
                    FH.SetCheckboxValue(field, value);
                    break;
                case "Date":
                case "Datetime":
                    FH.SetDateTimeValue(field, value);
                    break;
                case "File":
                    FH.SetFileValue(field, value);
                    break;
                case "General Link":
                case "General Link with Search":
                    FH.SetLinkValue(field, value);
                    break;
                case "Internal Link":
                    FH.SetInternalLinkvalue(field, value);
                    break;
                case "Image":
                    FH.SetImageValue(field, value);
                    break;
                case "Droplink":
                case "Droplist":
                case "Droptree":
                case "Grouped Droplink":
                case "Grouped Droplist":
                    FH.SetReferenceValue(field, value);
                    break;
                case "Checklist":
                case "Multilist":
                case "Multilist with Search":
                case "Treelist":
                case "TreelistEx":
                case "Version Link":
                    FH.SetListValue(field, value);
                    break;
                // default handles all fields that just use Value = string;
                default:
                    field.Value = value.ToString();
                    break;
            }
        }

        /// <summary>
        /// Get Sitecore Item by value
        /// </summary>
        /// <param name="db">Sitecore Database</param>
        /// <param name="value">Value as ID, GUID, Path, or Item</param>
        /// <returns>Sitecore Item</returns>
        public static Item GetItemFromValue(this SC.Data.Database db, object value)
        {
            if (value is Item)
            {
                return (Item)value;
            }
            else
            {
                return db.GetItem(value.ToString());
            }
        }

        /// <summary>
        /// Get Sitecore Items by an enumerable of object values
        /// </summary>
        /// <param name="db">Sitecore Database</param>
        /// <param name="values">Values as ID, GUID, Path, or Item</param>
        /// <param name="ignoreNull">Disable to throw exception on null Item</param>
        /// <returns>List of Sitecore Items</returns>
        public static List<Item> GetItems(this SC.Data.Database db, IEnumerable values, bool ignoreNull = true)
        {
            List<Item> items = new List<Item>();
            foreach (object value in values)
            {
                Item item = db.GetItemFromValue(value);
                if (item == null && !ignoreNull)
                {
                    throw new NullReferenceException(string.Format("No item found matching {0}.", value.ToString()));
                }
                if (item != null)
                {
                    items.Add(item);
                }
            }
            return items;
        }

        /// <summary>
        /// Get Sitecore Items by a delimited string of values
        /// </summary>
        /// <param name="db">Sitecore Database</param>
        /// <param name="values">Value as ID, GUID, or Path</param>
        /// <param name="delimiter">Character delimiter to split values</param>
        /// <param name="ignoreNull">Disable to throw exception on null Item</param>
        /// <returns>List of Sitecore Items</returns>
        public static List<Item> GetItems(this SC.Data.Database db, string values, char delimiter = ',', bool ignoreNull = true)
        {
            var valueArray = values.Split(delimiter);
            return db.GetItems(valueArray, ignoreNull);
        }

        /// <summary>
        /// Get children Items from Sitecore Item optionally filtered by a template
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="templateID">TemplateID to filter by</param>
        /// <param name="recursive">Whether or not to return all children or only immediate children</param>
        /// <returns></returns>
        public static List<Item> GetChildren(this Item item, ID templateID = null, bool recursive = false)
        {
            if (recursive)
            {
                return item.Axes.SelectItems(string.Format("descendant::*{0}", (!templateID.Equals(null) ? string.Format("[@@templateid='{0}']", templateID.ToString()) : ""))).ToList();
            }
            else
            {
                return item.Children.ToList();
            }
        }

        /// <summary>
        /// Get Sitecore Fields that are not System Fields for Item 
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <returns>Editable fields</returns>
        public static IEnumerable<Field> GetEditableFields(this Item item)
        {
            item.Fields.ReadAll();
            return item.Fields.Where(f => !f.Name.StartsWith("__"));
        }

        /// <summary>
        /// Render field from current item
        /// </summary>
        /// <param name="item">Sitecore Item</param>
        /// <param name="fieldName">Name of field to render</param>
        /// <param name="parameters">Optional parameters for rendering</param>
        /// <returns>Rendering string</returns>
        public static string RenderField(this Item item, string fieldName, string parameters = "")
        {
            if (item.Fields[fieldName] != null)
            {
                return SC.Web.UI.WebControls.FieldRenderer.Render(item, fieldName, parameters);
            }
            return string.Empty;
        }

        /// <summary>
        /// Render current Field with value from Item the Field is from
        /// </summary>
        /// <param name="field">Sitecore Field</param>
        /// <param name="parameters">Optional parameters for rendering</param>
        /// <returns>Rendering string</returns>
        public static string RenderField(this Field field, string parameters = "")
        {
            if (field.Item != null)
            {
                return SC.Web.UI.WebControls.FieldRenderer.Render(field.Item, field.Name, parameters);
            }
            return string.Empty;
        }

        /// <summary>
        /// Get Sitecore Fields that are not System Fields for TemplateItem
        /// </summary>
        /// <param name="item">Sitecore TemplateItem</param>
        /// <returns>Editable fields</returns>
        public static IEnumerable<TemplateFieldItem> GetEditableFields(this TemplateItem item)
        {
            return item.Fields.Where(f => !f.Name.StartsWith("__"));
        }
    }
}
