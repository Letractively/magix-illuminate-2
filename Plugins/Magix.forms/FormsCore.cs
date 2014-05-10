/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;

namespace Magix.forms
{
	/*
	 * controller for creating web parts, either as control collections, or as ml
	 */
	public class FormsCore : ActiveController
	{
		/*
		 * will create a web part based upon a web control hierarchy
		 */
		[ActiveEvent(Name = "magix.forms.create-web-part")]
		public void magix_forms_create_web_part(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>creates a web part and puts 
it into the [container] viewport container</p><p>[form-id] must be a unique id.
&nbsp;&nbsp;[controls] contains the controls themselves, and is a list of the 
controls the form contains.&nbsp;&nbsp;[class] is the css class(es) you wish to 
use for your form</p><p>you can also associate temporary active events with your 
form, which will only exist as long as your form exists.&nbsp;&nbsp;you do this 
by adding active events directly beneath the [events] node, where the name of the 
active event becomes the first node, and the code to execute is directly beneath 
the active event declaration</p><p>all parameters can be either constants or 
expressions.&nbsp;&nbsp;if you use expressions for the [events] and [controls] 
nodes, then you add the expression as the value of the [event] and/or the 
[controls] node, and whatever node these expressions returns, will become what 
the form uses to load its controls/events</p><p>you can optionally supply a 
[file-controls] node, instead of a [controls] node, which will be assumed to 
be a file containing the declaration of the web controls.&nbsp;&nbsp;in addition, 
you can also optionally supply a [file-events] node, which will be assumed to be 
a file containing the declaration of the events associated with the web part.&nbsp;
&nbsp;both [file-controls] and [file-events] can be either constants or expressions.
&nbsp;&nbsp;you cannot supply both [controls] and [file-controls].&nbsp;&nbsp;neither 
can you supply both [events] and [file-events]</p><p>both [file-events] and [file-controls] 
are useful for separating your code from your ui as an alternative to handcoding in the 
controls and events together in the same piece of code.&nbsp;&nbsp;in addition, you can 
also separate code and ui by supplying the [controls] and [events] as expressions to other 
parts of your execution node tree</p><p>not thread safe</p>";
                e.Params["magix.forms.create-web-part"]["container"].Value = "content3";
                e.Params["magix.forms.create-web-part"]["class"].Value = "css class(es) of your form";
                e.Params["magix.forms.create-web-part"]["form-id"].Value = "unique-id";
                e.Params["magix.forms.create-web-part"]["events"]["magix.forms.control-clicked"]["magix.viewport.show-message"]["message"].Value = "yo";
                e.Params["magix.forms.create-web-part"]["controls"]["button"].Value = "btn";
                e.Params["magix.forms.create-web-part"]["controls"]["button"]["value"].Value = "click me";
                e.Params["magix.forms.create-web-part"]["controls"]["button"]["onclick"]["magix.forms.control-clicked"].Value = null;
                return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Get<Node>();

            if (!ip.Contains("controls") && !ip.Contains("file-controls"))
                throw new ArgumentException("create-web-part needs either a [file-controls] parameter or a [controls] parameter");

            if (ip.Contains("controls") && ip.Contains("file-controls"))
                throw new ArgumentException("either supply [file-controls] or [controls] to create-web-part, not both of them");

            if (ip.Contains("events") && ip.Contains("file-events"))
                throw new ArgumentException("either supply [file-events] or [events] to create-web-part, not both of them");

            if (ip.Contains("file-controls"))
            {
                string file = Expressions.GetExpressionValue(ip["file-controls"].Get<string>(), dp, ip, false) as string;
                Node loadControls = new Node("magix.file.load", file);
                RaiseActiveEvent(
                    "magix.file.load",
                    loadControls);

                Node toNode = new Node();
                toNode["code"].Value = loadControls["value"].Get<string>();
                RaiseActiveEvent(
                    "magix.execute.code-2-node",
                    toNode);

                ip["controls"].Clear();
                ip["controls"].AddRange(toNode["node"]);
            }

            if (ip.Contains("file-events"))
            {
                string file = Expressions.GetExpressionValue(ip["file-events"].Get<string>(), dp, ip, false) as string;
                Node loadControls = new Node("magix.file.load", file);
                RaiseActiveEvent(
                    "magix.file.load",
                    loadControls);

                Node toNode = new Node();
                toNode["code"].Value = loadControls["value"].Get<string>();
                RaiseActiveEvent(
                    "magix.execute.code-2-node",
                    toNode);

                ip["events"].Clear();
                ip["events"].AddRange(toNode["node"]);
            }

            string container = null;
            if (ip.Contains("container"))
                container = Expressions.GetExpressionValue(ip["container"].Get<string>(), dp, ip, false) as string;

			LoadActiveModule(
				"Magix.forms.WebPart",
                container,
                e.Params);
		}
		
		/*
		 * will create a magix markup language web part
		 */
		[ActiveEvent(Name = "magix.forms.create-mml-web-part")]
		public void magix_forms_create_mml_web_part(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>creates a dynamic magix markup language 
web part from a [file] or [mml] value, putting it into the [container] viewport container
</p><p>[form-id] must be a unique id.&nbsp;&nbsp;you can intermix webcontrols into your mml 
by creating a control collection, by typing them out inside of brackets such as {{...controls, 
using hyper lisp syntax and code in event handlers goes here...}}</p><p>internally it uses 
LoadActiveModule, hence all the parameters that goes into your [magix.viewport.load-module] 
active event, can also be passed into this, such as [class], and so on</p><p>the magix markup 
language can either be hardcoded in through the [mml] node, or exist on a file, which you 
de-reference through the [file] node.&nbsp;&nbsp;either supply [file] or [mml], do not supply 
both.&nbsp;&nbsp;[mml] has precedence, which means that [file] will be ignored if there exist 
an [mml] node</p><p>both [mml], [file], [container], [class] and [form-id], can be either 
constants or expressions</p><p>not thread safe</p>";
                e.Params["magix.forms.create-mml-web-part"]["container"].Value = "content3";
                e.Params["magix.forms.create-mml-web-part"]["form-id"].Value = "unique-id";
                e.Params["magix.forms.create-mml-web-part"]["class"].Value = "span-22 clear";
                e.Params["magix.forms.create-mml-web-part"]["mml"].Value = @"
<p>notice how you can combine html with {{
link-button=>btn-hello
  value=>web controls
  onclick
    magix.viewport.show-message
      message=>hello world
}}</p>";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Get<Node>();

			if (!ip.Contains("container"))
				throw new ArgumentException("create-mml-web-part needs a [container] parameter");

			if (!ip.Contains("file") && !ip.Contains("mml"))
				throw new ArgumentException("create-mml-web-part needs either a [file] parameter or an [mml] parameter");

            if (!ip.Contains("mml"))
            {
                string file = Expressions.GetExpressionValue(ip["file"].Get<string>(), dp, ip, false) as string;

                Node loadFile = new Node("magix.file.load", file);
                RaiseActiveEvent(
                    "magix.file.load",
                    loadFile);

                ip["mml"].Value = loadFile["value"].Get<string>();
            }

			LoadActiveModule(
				"Magix.forms.WebPart", 
				Expressions.GetExpressionValue(ip["container"].Get<string>(), dp, ip, false) as string, 
				e.Params);
		}
	}
}

