/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 * uploader control
	 */
	public class UploaderCore : BaseControlCore
	{
		/**
		 * creates uploader control
		 */
		[ActiveEvent(Name = "magix.forms.controls.uploader")]
		public void magix_forms_controls_button(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			Uploader ret = new Uploader();

			if (node.Contains("css") && !string.IsNullOrEmpty(node["css"].Get<string>()))
				ret.CssClass = node["css"].Get<string>();

			string folder = "";

			if (node.Contains("folder") && !string.IsNullOrEmpty(node["folder"].Get<string>()))
				folder = node["folder"].Get<string>();

			if (node.Contains("onuploaded"))
			{
				Node codeNode = node["onuploaded"].Clone();

				ret.Uploaded += delegate(object sender2, EventArgs e2)
				{
					Uploader that = sender2 as Uploader;

					string fileName = Page.Server.MapPath(folder.Trim('/') + "/" + that.GetFileName());

					if (File.Exists(fileName))
						File.Delete(fileName);

					byte[] content = Convert.FromBase64String(that.GetFileRawBASE64());
					using(FileStream stream = File.Create(fileName))
					{
						stream.Write(content, 0, content.Length);

					}

					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}
			else
			{
				ret.Uploaded += delegate(object sender2, EventArgs e2)
				{
					Uploader that = sender2 as Uploader;

					string fileName = Page.Server.MapPath(folder.Trim('/') + "/" + that.GetFileName());

					if (File.Exists(fileName))
						File.Delete(fileName);

					byte[] content = Convert.FromBase64String(that.GetFileRawBASE64());
					using(FileStream stream = File.Create(fileName))
					{
						stream.Write(content, 0, content.Length);
					}
				};
			}
			e.Params["_ctrl"].Value = ret;
		}

		/**
		 * has more data
		 */
		[ActiveEvent(Name = "magix.forms.has-more-data")]
		protected void magix_forms_has_more_data(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.has-more-data"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = true;
				e.Params["inspect"].Value = @"returns true in [value] if there is more data in the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
				return;
			}

			Uploader ctrl = FindControl<Uploader>(e.Params);

			if (ctrl != null)
			{
				e.Params["value"].Value = ctrl.SizeOfBatch > ctrl.CurrentNo + 1;
			}
		}
		protected override void Inspect(Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"creates a uploader input type of web control.&nbsp;&nbsp;
[onuploaded] is the event handler";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["uploader"]["folder"].Value = "system42";
			node["controls"]["uploader"]["css"].Value = "mux-file-uploader";
			base.Inspect(node["controls"]["uploader"]);
			node["controls"]["uploader"]["visible"].UnTie(); // makes no sense
			node["controls"]["uploader"]["info"].UnTie(); // makes no sense
			node["controls"]["uploader"]["onuploaded"].Value = "hyper lisp code";
		}
	}
}

