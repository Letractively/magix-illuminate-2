/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.Core;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/**
	 * Base class, containging helpers to help dynamically build Web Controls
	 */
	public class DynamicFormBase : ActiveModule
	{
		protected delegate Node FindNode(string path);

		protected string FormID
		{
			get { return ViewState["FormID"] as string; }
			set { ViewState["FormID"] = value; }
		}

		public override void InitialLoading(Node node)
		{
			Load +=
				delegate
				{
					FormID = node["form-id"].Get<string>();
				};
			base.InitialLoading(node);
		}

		// TODO: Change id to Value instead of "id" node
		/**
		 * Returns the given value of the given "id" widget, within the given
		 * "form-id" form, in the "value" node
		 */
        [ActiveEvent(Name = "magix.forms.get-value")]
		protected void magix_forms_get_value(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.get-value"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "formID";
				e.Params["inspect"].Value = @"returns the value of the given 
[id] web control, in the [form-id] form, as [value]";
				return;
			}

			if (!e.Params.Contains ("form-id"))
				throw new ArgumentException("Missing form-id in get-value");

			if (!e.Params.Contains ("id"))
				throw new ArgumentException("Missing id in get-value");

			if (e.Params["form-id"].Get<string>() == FormID)
			{
				Control ctrl = Selector.FindControl<Control>(this, e.Params["id"].Get<string>());
				if (ctrl is BaseWebControlFormElementText)
					e.Params["value"].Value = 
						((BaseWebControlFormElementText)ctrl).Text;
				else if (ctrl is CheckBox)
					e.Params["value"].Value = 
						((CheckBox)ctrl).Checked;
				else if (ctrl is HiddenField)
					e.Params["value"].Value = 
						((HiddenField)ctrl).Value;
				else if (ctrl is RadioButton)
					e.Params["value"].Value = 
						((RadioButton)ctrl).Checked;
				else if (ctrl is SelectList)
					e.Params["value"].Value = 
						((SelectList)ctrl).SelectedItem.Value;
				else if (ctrl is Label)
					e.Params["value"].Value = 
						((Label)ctrl).Text;
			}
		}

		/**
		 * selects all text in a mux textbox or textarea
		 */
        [ActiveEvent(Name = "magix.forms.select-all")]
		protected void magix_forms_select_all(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.select-all"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "formid";
				e.Params["inspect"].Value = @"selects all text in the specific 
[id] textbox or textarea, in the [form-id] form";
				return;
			}

			if (!e.Params.Contains("form-id"))
				throw new ArgumentException("Missing form-id in set-value");

			if (!e.Params.Contains("id"))
				throw new ArgumentException("Missing id in set-value");

			if (e.Params["form-id"].Get<string>() == FormID)
			{
				BaseWebControlFormElementInputText ctrl = 
					Selector.FindControl<BaseWebControlFormElementInputText>(this, e.Params["id"].Get<string>());
				ctrl.Select();
			}
		}

		/**
		 * sets focus to a specific mux web control
		 */
        [ActiveEvent(Name = "magix.forms.set-focus")]
		protected void magix_forms_set_focus(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-focus"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "formid";
				e.Params["inspect"].Value = @"sets focus to the specific 
[id] web control, in the [form-id] form";
				return;
			}

			if (!e.Params.Contains("form-id"))
				throw new ArgumentException("Missing form-id in set-value");

			if (!e.Params.Contains("id"))
				throw new ArgumentException("Missing id in set-value");

			if (e.Params["form-id"].Get<string>() == FormID)
			{
				BaseWebControl ctrl = Selector.FindControl<BaseWebControl>(this, e.Params["id"].Get<string>());
				ctrl.Focus();
			}
		}

		/**
		 * Returns the given value of the given "id" widget, within the given
		 * "form-id" form, in the "value" node
		 */
        [ActiveEvent(Name = "magix.forms.set-value")]
		protected void magix_forms_set_value(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-value"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "formid";
				e.Params["inspect"].Value = @"sets the value of the given 
[id] web control, in the [form-id] form, from [value]";
				return;
			}

			if (!e.Params.Contains ("form-id"))
				throw new ArgumentException("Missing form-id in set-value");

			if (!e.Params.Contains ("id"))
				throw new ArgumentException("Missing id in set-value");

			if (e.Params["form-id"].Get<string>() == FormID)
			{
				string value = e.Params.Contains("value") ? e.Params["value"].Get<string>("") : "";

				Control ctrl = Selector.FindControl<Control>(this, e.Params["id"].Get<string>());
				if (ctrl is BaseWebControlFormElementText)
					((BaseWebControlFormElementText)ctrl).Text = value;
				if (ctrl is Label)
					((Label)ctrl).Text = value;
				else if (ctrl is Button)
					((Button)ctrl).Text = value;
				else if (ctrl is LinkButton)
					((LinkButton)ctrl).Text = value;
				else if (ctrl is Image)
					((Image)ctrl).ImageUrl = value;
				else if (ctrl is HyperLink)
					((HyperLink)ctrl).URL = value;
				else if (ctrl is Button)
					((Button)ctrl).Text = value;
				else if (ctrl is CheckBox)
					((CheckBox)ctrl).Checked = bool.Parse (value);
				else if (ctrl is HiddenField)
					((HiddenField)ctrl).Value = value;
				else if (ctrl is RadioButton)
					((RadioButton)ctrl).Checked = bool.Parse (value);
				else if (ctrl is SelectList)
					((SelectList)ctrl).SetSelectedItemAccordingToValue(value);
				else
					throw new ArgumentException("Don't know how to set the value of that control");
			}
		}

		protected string GetPath(Node idxInner)
		{
			Node idxNode = idxInner;
			string retVal = "";
			while (idxNode.Parent != null)
			{
				string tmp = idxNode.Parent.IndexOf(idxNode).ToString() + "|";
				retVal = tmp + retVal;

				idxNode = idxNode.Parent;
			}
			retVal = retVal.Trim('|');
			return retVal;
		}

		protected Node GetNode(string codePath, Node source)
		{
			string[] splits = codePath.Split('|');
			Node tmp = source;
			foreach (string idx in splits)
			{
				int idxNo = int.Parse(idx);
				tmp = tmp[idxNo];
			}
			return tmp;
		}

		protected void BuildControl(Node idx, Control parent, FindNode del)
		{
			BaseControl ctrl = null;
			switch (idx.Name)
			{
			case "OnFirstLoad":
			{
				string path = GetPath(idx);
				if (ViewState[ClientID + "_FirstLoad"] == null)
				{
					ViewState[ClientID + "_FirstLoad"] = true;
					Node codeNode = del(path);
					
					RaiseEvent(
						"magix.execute", 
						codeNode);
				}
				return;
			}
			case "Button":
				ctrl = new Button();
				break;
			case "CheckBox":
				ctrl = new CheckBox();
				break;
			case "HiddenField":
				ctrl = new HiddenField();
				break;
			case "HyperLink":
				ctrl = new HyperLink();
				break;
			case "Image":
				ctrl = new Image();
				break;
			case "Label":
				ctrl = new Label();
				break;
			case "LinkButton":
				ctrl = new LinkButton();
				break;
			case "Panel":
				ctrl = new Panel();
				break;
			case "RadioButton":
				ctrl = new RadioButton();
				break;
			case "SelectList":
				ctrl = new SelectList();
				break;
			case "TextArea":
				ctrl = new TextArea();
				break;
			case "TextBox":
				ctrl = new TextBox();
				break;
			default:
				bool isHtml = true;
				foreach (char idxC in idx.Name)
				{
					if ("abcdefghijklmnopqrstuvwxyz".IndexOf(idxC) == -1)
					{
						isHtml = false;
					}
				}
				if (isHtml)
				{
					ctrl = new Label();
					((Label)ctrl).Tag = idx.Name;
				}
				else
					throw new ArgumentException("Unknown type of tag in creating form; " + idx.Name);
				break;
			}
			if (idx.Value != null)
				ctrl.ID = idx.Get<string>();

			foreach (Node idxInner in idx)
			{
				switch (idxInner.Name)
				{
				case "Text":
					if (ctrl is Label)
						((Label)ctrl).Text = idxInner.Get<string>();
					else if (ctrl is HyperLink)
						((HyperLink)ctrl).Text = idxInner.Get<string>();
					else
						((BaseWebControlFormElementText)ctrl).Text = idxInner.Get<string>();
					break;
				case "Enabled":
					if (ctrl is BaseWebControlFormElement)
						((BaseWebControlFormElement)ctrl).Enabled = idxInner.Get<bool>();
					else
						throw new ArgumentException("Tried to set Enabled on a control that does not support it; " + ctrl.GetType ().Name);
					break;
				case "SelectedIndex":
					if (ctrl is BaseWebControlListFormElement)
						((BaseWebControlListFormElement)ctrl).SelectedIndex = idxInner.Get<int>();
					else
						throw new ArgumentException("Tried to set SelectedIndex on a control that does not support it; " + ctrl.GetType ().Name);
					break;
				case "CssClass":
					((BaseWebControl)ctrl).CssClass = idxInner.Get<string>();
					break;
				case "ID":
					ctrl.ID = idxInner.Get<string>();
					break;
				case "Dir":
					((BaseWebControl)ctrl).Dir = idxInner.Get<string>();
					break;
				case "ToolTip":
					((BaseWebControl)ctrl).ToolTip = idxInner.Get<string>();
					break;
				case "Visible":
					((BaseWebControl)ctrl).Visible = idxInner.Get<bool>();
					break;
				case "TabIndex":
					((BaseWebControl)ctrl).TabIndex = idxInner.Get<string>();
					break;
				case "AccessKey":
					if (ctrl is BaseWebControlFormElement)
						((BaseWebControlFormElement)ctrl).AccessKey = idxInner.Get<string>();
					break;
				case "Checked":
					if (ctrl is CheckBox)
						((CheckBox)ctrl).Checked = bool.Parse (idxInner.Get<string>());
					else
						((RadioButton)ctrl).Checked = bool.Parse (idxInner.Get<string>());
					break;
				case "Tag":
					if (ctrl is Label)
						((Label)ctrl).Tag = idxInner.Get<string>();
					else
						((Panel)ctrl).Tag = idxInner.Get<string>();
					break;
				case "For":
					((Label)ctrl).For = idxInner.Get<string>();
					break;
				case "Value":
					((HiddenField)ctrl).Value = idxInner.Get<string>();
					break;
				case "URL":
					((HyperLink)ctrl).URL = idxInner.Get<string>();
					break;
				case "Target":
					((HyperLink)ctrl).Target = idxInner.Get<string>();
					break;
				case "ImageUrl":
					((Image)ctrl).ImageUrl = idxInner.Get<string>();
					break;
				case "AlternateText":
					((Image)ctrl).AlternateText = idxInner.Get<string>();
					break;
				case "Items":
					foreach (Node idxChild in idxInner)
					{
						((SelectList)ctrl).Items.Add (new ListItem(idxChild.Name, idxChild.Get<string>()));
					} break;
				case "PlaceHolder":
					if (ctrl is TextBox)
						((TextBox)ctrl).PlaceHolder = idxInner.Get<string>();
					else
						((TextArea)ctrl).PlaceHolder = idxInner.Get<string>();
					break;
				case "DefaultWidget":
					if (ctrl is Panel)
						((Panel)ctrl).DefaultWidget = idxInner.Get<string>();
					else
						throw new ArgumentException("Only Panels can have a Default Widget, you tried to set DefaultWidget on a " + ctrl.GetType().Name);
					break;
				case "Rows":
					if (ctrl is TextArea)
						((TextArea)ctrl).Rows = idxInner.Get<int>();
					else
						throw new ArgumentException("Only textarea can have Rows");
					break;
				case "controls":
					foreach (Node idxChild in idxInner)
					{
						BuildControl(idxChild, (BaseWebControl)ctrl, del);
					} break;
				case "OnSelectedIndexChanged":
				{
					string path = GetPath(idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_SelectedIndexChanged"] = path;
							((SelectList)ctrl).SelectedIndexChanged +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_SelectedIndexChanged"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnEnterPressed":
				{
					string path = GetPath(idxInner);
					((TextBox)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_EnterPressed"] = path;
							((TextBox)ctrl).EnterPressed +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_EnterPressed"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnTextChanged":
				{
					string path = GetPath(idxInner);
					((BaseWebControlFormElementInputText)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_TextChanged"] = path;
							((BaseWebControlFormElementInputText)ctrl).TextChanged +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_TextChanged"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnCheckedChanged":
				{
					string path = GetPath(idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_CheckedChanged"] = path;
							if (ctrl is CheckBox)
							{
								((CheckBox)ctrl).CheckedChanged +=
									delegate
									{
										string codePath = ViewState[((Control)sender).ClientID + "_CheckedChanged"] as string;
										Node codeNode = del(codePath);
										RaiseEvent("magix.execute", codeNode);
									};
							}
							else if (ctrl is RadioButton)
							{
								((RadioButton)ctrl).CheckedChanged +=
									delegate
									{
										string codePath = ViewState[((Control)sender).ClientID + "_CheckedChanged"] as string;
										Node codeNode = del(codePath);
										RaiseEvent("magix.execute", codeNode);
									};
							}
						else
							throw new ArgumentException("OnCheckedChanged on something that's not a RadioButton or a CheckBox ...???");
						};
				} break;
				case "OnClick":
				{
					string path = GetPath(idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_Click"] = path;
							((BaseWebControl)ctrl).Click +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_Click"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnDblClick":
				{
					string path = GetPath(idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_DblClick"] = path;
							((BaseWebControl)ctrl).DblClick +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_DblClick"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnMouseOver":
				{
					string path = GetPath(idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_MouseOver"] = path;
							((BaseWebControl)ctrl).MouseOver +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_MouseOver"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnMouseOut":
				{
					string path = GetPath(idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_MouseOut"] = path;
							((BaseWebControl)ctrl).MouseOut +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_MouseOut"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnMouseDown":
				{
					string path = GetPath(idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_MouseDown"] = path;
							((BaseWebControl)ctrl).MouseDown +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_MouseDown"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnMouseUp":
				{
					string path = GetPath(idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_MouseUp"] = path;
							((BaseWebControl)ctrl).MouseUp +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_MouseUp"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnKeyPress":
				{
					string path = GetPath(idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_KeyPress"] = path;
							((BaseWebControl)ctrl).KeyPress +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_KeyPress"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnEscKey":
				{
					string path = GetPath(idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_EscKey"] = path;
							((BaseWebControl)ctrl).EscKey +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_EscKey"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnFocused":
				{
					string path = GetPath(idxInner);
					((BaseWebControlFormElement)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_OnFocused"] = path;
							((BaseWebControlFormElement)ctrl).Focused +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_EscKey"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnBlur":
				{
					string path = GetPath(idxInner);
					((BaseWebControlFormElement)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_OnBlur"] = path;
							((BaseWebControlFormElement)ctrl).Blur +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_EscKey"] as string;
									Node codeNode = del(codePath);
									RaiseEvent("magix.execute", codeNode);
								};
						};
				} break;
				case "OnFirstLoad":
				{
					string path = GetPath(idxInner);
					((Control)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							if (ViewState[ctrl.ClientID + "_FirstLoad"] == null)
							{
								ViewState[ctrl.ClientID + "_FirstLoad"] = true;
								Node codeNode = del(path);
								
								RaiseEvent(
									"magix.execute", 
									codeNode);
							}
						};
				} break;
				}
			}

			parent.Controls.Add(ctrl);
		}
	}
}
