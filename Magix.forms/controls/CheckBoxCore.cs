/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/**
	 * check box
	 */
	public class CheckBoxCore : FormElementCore
	{
		/**
		 * creates check-box
		 */
		[ActiveEvent(Name = "magix.forms.controls.check")]
		public void magix_forms_controls_check(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			CheckBox ret = new CheckBox();

			FillOutParameters(node, ret);

			if (node.Contains("checked") && node["checked"].Value != null)
				ret.Checked = node["checked"].Get<bool>();

			if (node.Contains("key") && 
			    !string.IsNullOrEmpty(node["key"].Get<string>()))
				ret.AccessKey = node["key"].Get<string>();

			if (node.Contains("enabled") && 
			    node["enabled"].Value != null)
				ret.Enabled = node["enabled"].Get<bool>();

			if (node.Contains("oncheckedchanged"))
			{
				Node codeNode = node["oncheckedchanged"].Clone();

				ret.CheckedChanged += delegate(object sender2, EventArgs e2)
				{
					CheckBox that2 = sender2 as CheckBox;
					if (!string.IsNullOrEmpty(that2.Info))
						codeNode["$"]["info"].Value = that2.Info;

					object val = GetValue(that2);
					if (val != null)
						codeNode["$"]["value"].Value = val;

					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			e.Params["_ctrl"].Value = ret;
		}

		/**
		 * sets checked value
		 */
		[ActiveEvent(Name = "magix.forms.set-value")]
		public void magix_forms_set_value(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = "sets the value property of the control";
				return;
			}

			if (!e.Params.Contains("value"))
				throw new ArgumentException("set-value needs [value]");

			CheckBox ctrl = FindControl<CheckBox>(e.Params);

			if (ctrl != null)
			{
				ctrl.Checked = e.Params["value"].Get<bool>();
			}
		}

		/**
		 * returns value
		 */
		[ActiveEvent(Name = "magix.forms.get-value")]
		public void magix_forms_get_value(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = "returns the value property of the control";
				return;
			}

			CheckBox ctrl = FindControl<CheckBox>(e.Params);

			if (ctrl != null)
			{
				e.Params["value"].Value = ctrl.Checked;
			}
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"creates a checkbox input type of web control.&nbsp;&nbsp;
useful for representing choices like yes or no.&nbsp;&nbsp;[checked] determines its 
state, true or false.&nbsp;&nbsp;key is keyboard shortcut.&nbsp;&nbsp;[enabled] can be 
true or false, and determines if it is possible to change its state, or not.&nbsp;&nbsp;
[oncheckedchanged] is raised when checked state of control changes";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["check"]["checked"].Value = true;
			node["controls"]["check"]["key"].Value = "C";
			node["controls"]["check"]["enabled"].Value = true;
			base.Inspect(node["controls"]["check"]);
			node["controls"]["check"]["oncheckedchanged"].Value = "hyper lisp code";
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
			return ((CheckBox)that).Checked;
		}
	}
}

