using System;
using System.Collections.Generic;
using System.Linq;

using SC = Sitecore;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;

namespace Specialized.Content{

	/// <summary>
	/// Helper methods specific to fields
	/// </summary>
	public static class FieldHelpers {

		/// <summary>
		/// Set URL for General Link field with default link type set as external
		/// </summary>
		/// <param name="field">Sitecore Field</param>
		/// <param name="value">URL, Item Path, or Item ID</param>
		/// <param name="linkType">Type of link as string</param>
		public static void SetLinkValue(Field field, object value, string linkType = "external") {
			LinkField thisField = (LinkField) field;
			if (linkType == "media" || linkType == "internal") {
				Item item = field.Database.GetItemFromValue(value);
				if (item != null) {
					if (linkType == "media") {
						thisField.Url = item.Paths.MediaPath;
					}
					else {
						thisField.Url = SC.Links.LinkManager.GetItemUrl(item, SC.Links.LinkManager.GetDefaultUrlOptions());
					}
					thisField.TargetID = item.ID;
				}
				else { thisField.Url = value.ToString(); }
			}
			else {
				thisField.Url = value.ToString();
			}
			thisField.LinkType = linkType;
		}

		/// <summary>
		/// Set URL for Internal Link field
		/// </summary>
		/// <param name="field">Sitecore Field</param>
		/// <param name="value">Path or ID of existing Sitecore Item</param>
		public static void SetInternalLinkvalue(Field field, object value) {
			InternalLinkField thisField = (InternalLinkField) field;
			Item item = field.Database.GetItemFromValue(value);
			if (item != null) {
				thisField.Value = item.Paths.Path;
			}
		}

		/// <summary>
		/// Set value for Date and DateTime field
		/// </summary>
		/// <param name="field">Sitecore Field</param>
		/// <param name="value">Date string or DateTime object</param>
		public static void SetDateTimeValue(Field field, object value) {
			DateField thisField = (DateField) field;
			DateTime dateVal = new DateTime();
			if (DateTime.TryParse(value.ToString(), out dateVal)) {
				thisField.Value = SC.DateUtil.ToIsoDate(dateVal);
			}
		}

		/// <summary>
		/// Set checked state on Checkbox field
		/// </summary>
		/// <param name="field">Sitecore Field</param>
		/// <param name="value">Boolean, True/False string, or Integer</param>
		public static void SetCheckboxValue(Field field, object value) {
			CheckboxField thisField = (CheckboxField) field;
			try {
				int boolInt = 0;
				bool boolVal;
				if (value.ToString().Equals("yes", StringComparison.OrdinalIgnoreCase) || value.ToString().Equals("no", StringComparison.OrdinalIgnoreCase)) {
					boolVal = (value.ToString().Equals("yes", StringComparison.OrdinalIgnoreCase) ? true : false);
				}
				else if (int.TryParse(value.ToString(), out boolInt)) {
					boolVal = Convert.ToBoolean(boolInt);
				}
				else { boolVal = Convert.ToBoolean(value); }
				thisField.Checked = boolVal;
			}
			catch (Exception ex) { /* do nothing for now */ }
		}

		/// <summary>
		/// Set value for File field
		/// </summary>
		/// <param name="field">Sitecore Field</param>
		/// <param name="value">Path or ID of existing Sitecore Item</param>
		public static void SetFileValue(Field field, object value) {
			FileField thisField = (FileField) field;
			Item item = field.Database.GetItemFromValue(value);

			if (item != null) {
				thisField.MediaID = item.ID;
				thisField.Src = SC.Resources.Media.MediaManager.GetMediaUrl(item);
			}
		}

		/// <summary>
		/// Set value for Image field
		/// </summary>
		/// <param name="field">Sitecore Field</param>
		/// <param name="value">Path or ID of existing Sitecore Item</param>
		public static void SetImageValue(Field field, object value) {
			ImageField thisField = (ImageField) field;
			Item item = field.Database.GetItemFromValue(value);

			if (item != null) {
				MediaItem mediaItem = new MediaItem(item);
				thisField.MediaID = mediaItem.ID;
				if (!string.IsNullOrWhiteSpace(mediaItem.Alt)) {
					thisField.Alt = mediaItem.Alt;
				}
				else {
					thisField.Alt = mediaItem.DisplayName;
				}
			}
		}

		/// <summary>
		/// Set value for Droplist, Droplink, and Droptree field types
		/// </summary>
		/// <param name="field">Sitecore Field</param>
		/// <param name="value">Path or ID of existing Sitecore Item</param>
		public static void SetReferenceValue(Field field, object value) {
			ReferenceField thisField = (ReferenceField) field;
			Item item = field.Database.GetItemFromValue(value);

			if (item != null) {
				if (field.Type == "Droplist") {
					thisField.Value = item.Name;
				}
				else {
					thisField.Value = item.ID.ToString();
				}
			}
		}

		/// <summary>
		/// Set value for Checklist, Multilist, Treelist, and Version Link field types
		/// </summary>
		/// <param name="field">Sitecore Field</param>
		/// <param name="value">Sitecore Item, IEnumerable of Item, IEnumerable of Path or IDs as string, ID as GUID or Sitecore.Data.ID, Path or ID as string</param>
		public static void SetListValue(Field field, object value) {
			MultilistField thisField = (MultilistField) field;

			if (value is Item) {
				thisField.Value = ((Item) value).ID.ToString();
			}
			else if (value is IEnumerable<Item>) {
				thisField.Value = string.Join("|", ((IEnumerable<Item>) value).Select(i => i.ID.ToString()));
			}
			else if (value is IEnumerable<string>) {	// assume string is path or guid
				thisField.Value = string.Empty;	// clear value for add
				foreach (var val in (IEnumerable<string>) value) {
					Item item = field.Database.GetItemFromValue(val);
					if (item != null) {
						thisField.Add(item.ID.ToString());
					}
				}
				thisField.Value = string.Join("|", (IEnumerable<string>) value);
			}
			else if (value is Guid || value is SC.Data.ID) {
				thisField.Value = value.ToString();
			}
			else if (value is string) {
				string val = value.ToString().ToUpper();	// fixes "Item not in selection" mismatch due to lowercase
				if (val.Contains(',')) {
					thisField.Value = val.Replace(',', '|');
				}
				else if (val.Contains('|')) {
					thisField.Value = val;
				}
				else {
					Item item = field.Database.GetItemFromValue(value);
					if (item != null) {
						thisField.Value = item.ID.ToString();
					}
				}
			}
		}
	}
}