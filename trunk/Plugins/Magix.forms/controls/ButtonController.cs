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
	/*
	 * contains the button control
	 */
    public class ButtonController : BaseWebControlFormElementTextController
	{
		/*
		 * creates button widget
		 */
		[ActiveEvent(Name = "magix.forms.controls.button")]
		public void magix_forms_controls_button(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			Button ret = new Button();
            FillOutParameters(e.Params, ret);
            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect(Node node)
		{
            AppendInspect(node["inspect"], @"creates a button input type of web control

the button is probably one of the most commonly used web control, both in magix, and on 
the web in general.  it renders as &lt;input type='button' ... /&gt;

buttons are clickable objects, and logically similar to [link-button], though rendered 
differently.  although virtually anything can be a clickable object in magix, it is 
considered more polite to use buttons and link-buttons as your clickable objects.  first 
of all, since these web controls are recognized by screen readers and such.  secondly, 
because it keeeps the semantic parts of your website more correct according to the html 
standard");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["button"]);
		}
	}
}

