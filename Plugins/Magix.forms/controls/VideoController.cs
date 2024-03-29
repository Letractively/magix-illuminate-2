/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;
using System.Web.UI;
using Magix.UX;

namespace Magix.forms
{
	/*
	 * contains the video control
	 */
    internal sealed class VideoController : BaseWebControlController
	{
		/*
		 * creates button widget
		 */
		[ActiveEvent(Name = "magix.forms.controls.video")]
		private void magix_forms_controls_rating(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

            Node codeNode = ip["_code"].Get<Node>();
            Label ret = new Label();
            FillOutParameters(e.Params, ret);

            ret.Tag = "video";
            string src = "";
            if (codeNode.ContainsValue("src"))
                src = codeNode["src"].Get<string>();
            ret.Attributes.Add(new AttributeControl.Attribute("src", src));
            ret.Attributes.Add(new AttributeControl.Attribute("controls", null));
            ret.Value = "get a decent browser dude!";
            ip["_ctrl"].Value = ret;
		}

		/*
		 * creates an attribute
		 */
        [ActiveEvent(Name = "magix.forms.set-video")]
        private static void magix_forms_set_video(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-video-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-video-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("video"))
                throw new ArgumentException("no [video] value given to [magix.forms.set-video]");
            string video = Expressions.GetExpressionValue<string>(ip["video"].Get<string>(), dp, ip, false);

            AttributeControl ctrl = FindControl<AttributeControl>(e.Params);
            ctrl.SetAttribute("src", video);
        }

		protected override void Inspect(Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.video-dox-start].value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.video-sample]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["video"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.video-dox-end].value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["video"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.video-sample-end]");
        }
	}
}

