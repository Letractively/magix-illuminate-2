/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/**
	 * hidden field
	 */
	public class HiddenFieldCore : BaseControlCore
	{
		/**
		 * creates hidden field
		 */
		[ActiveEvent(Name = "magix.forms.controls.hidden")]
		public void magix_forms_controls_hidden(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

			HiddenField ret = new HiddenField();

			FillOutParameters(node, ret);

			if (node.Contains("value") && node["value"].Value != null)
				ret.Value = node["value"].Get<string>();

            Ip(e.Params)["_ctrl"].Value = ret;
		}

		/**
		 * sets value
		 */
		[ActiveEvent(Name = "magix.forms.set-value")]
		public void magix_forms_set_value(object sender, ActiveEventArgs e)
		{
            if (ShouldInspectOrHasInspected(e.Params))
			{
				e.Params["inspect"].Value = "sets the value property of the control";
				return;
			}

            if (!Ip(e.Params).Contains("value"))
				throw new ArgumentException("set-value needs [value]");

            HiddenField ctrl = FindControl<HiddenField>(Ip(e.Params));

			if (ctrl != null)
			{
                ctrl.Value = Ip(e.Params)["value"].Get<string>();
			}
		}

		/**
		 * returns value
		 */
		[ActiveEvent(Name = "magix.forms.get-value")]
		public void magix_forms_get_value(object sender, ActiveEventArgs e)
		{
            if (ShouldInspectOrHasInspected(e.Params))
			{
				e.Params["inspect"].Value = "returns the value property of the control";
				return;
			}

            HiddenField ctrl = FindControl<HiddenField>(Ip(e.Params));

			if (ctrl != null)
			{
                Ip(e.Params)["value"].Value = ctrl.Value;
			}
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"creates a hidden field input type of web control.&nbsp;&nbsp;
hidden fields are invisible, but can store any data as test in their [value] node";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["hidden"]["value"].Value = "whatever you wish to put in as value";
			base.Inspect(node["controls"]["hidden"]);
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
			return ((HiddenField)that).Value;
		}
	}
}
