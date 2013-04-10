/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.SampleController
{
	/**
	 * Please notice, by removing this assembly, and replacing it with
	 * another Assembly, containing a controller, which handles the
	 * "magix.viewport.page-load" event, you can create your 
	 * own Application Startup logic, which effectively would become the 
	 * starting point of your application, serving up whatever Module
	 * you wish to use
	 */
	public class ControllerHelp : ActiveController
	{
		/**
		 * Starts the Magix Illuminate Help System [AKA; Marvin ...]
		 */
		[ActiveEvent(Name = "magix.help.start-help")]
		public void magix_help_start_help(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.help.start-help"].Value = null;
				e.Params["inspect"].Value = @"opens the help files in modal container.
&nbsp;&nbsp;not thread safe";
				return;
			}

			Node tmp = new Node();

			tmp["form-id"].Value = "help";
			tmp["container"].Value = "modal";
			tmp["css"].Value = "modal-fixed-body modal-body";
			tmp["header"].Value = "help from marvin";

			using (TextReader reader = File.OpenText(Page.Server.MapPath("Help/index.mml")))
			{
				tmp["html"].Value = reader.ReadToEnd();
			}

			RaiseActiveEvent(
				"magix.forms.create-web-page",
				tmp);

			if (Pages.Count > 0)
			{
				// We have help history here, open last seen page instead
				tmp = new Node();

				tmp["freeze-help-stack"].Value = true;
				tmp["file"].Value = Pages[CurrentIndex];

				RaiseActiveEvent(
					"magix.help.open-file",
					tmp);
			}
			else
			{
				tmp = new Node();
				tmp["reset"].Value = true;
				tmp["page"].Value = "Help/index.mml";

				RaiseActiveEvent(
					"magix.help.add-page",
					tmp);
			}

			tmp = new Node();

			tmp["form-id"].Value = "help-navigation";
			tmp["container"].Value = "modalFtr";
			tmp["html"].Value = @"
<div class=""btn-group"">
{{
link-button=>back
  text=><<
  css=>btn btn-primary
  onclick
    magix.help.move-backwards
link-button=>index
  text=>index
  css=>btn btn-primary
  onclick
    magix.help.open-file
      file=>Help/index.mml
link-button=>tools
  text=>tools
  css=>btn btn-primary
  onclick
    magix.help.open-file
      file=>Help/tools.mml
link-button=>executor
  text=>executor
  css=>btn btn-primary
  onclick
    magix.admin.load-start
    magix.viewport.clear-controls
      container=>modal
link-button=>next
  text=>>>
  css=>btn btn-primary
  onclick
    magix.help.move-next
}}
</div>
";
			RaiseActiveEvent(
				"magix.forms.create-web-page",
				tmp);
		}

		[ActiveEvent(Name = "magix.help.set-next")]
		public void magix_help_set_next(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.help.set-next"].Value = null;
				e.Params["inspect"].Value = @"set the next page for the help system.
&nbsp;&nbsp;not thread safe";
				e.Params["next"].Value = "Help/index.mml";
				return;
			}
			if (!e.Params.Contains("next") || e.Params["next"].Get<string>("") == "")
				throw new ArgumentException("cannot set-next without a next value");

			Next = e.Params["next"].Get<string>();
		}

		[ActiveEvent(Name = "magix.help.move-next")]
		public void magix_help_move_next(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.help.move-next"].Value = null;
				e.Params["inspect"].Value = @"moves to the next page for the help system.
&nbsp;&nbsp;not thread safe";
				return;
			}

			// Defaulting to "next"
			string next = Next;

			Node node = new Node();

			if (CurrentIndex < Pages.Count - 1)
			{
				// Mowing "forward" since we've got "forward history"
				CurrentIndex += 1;
				next = Pages[CurrentIndex];
				node["freeze-help-stack"].Value = true;
			}

			if (string.IsNullOrEmpty(next))
				throw new ArgumentException("no next page in help system");

			Next = null;

			node["file"].Value = next;

			RaiseActiveEvent(
				"magix.help.open-file",
				node);
		}

		/**
		 * Opens a specific page on disc, as if it was html
		 */
		[ActiveEvent(Name = "magix.help.open-file")]
		public void magix_help_open_file(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.help.open-file"].Value = null;
				e.Params["inspect"].Value = @"opens the given [file] file as html.
&nbsp;&nbsp;thread safe";
				e.Params["file"].Value = "Help/index.html";
				return;
			}

			Node tmp = new Node();
			tmp["form-id"].Value = "help";

			using (TextReader reader = File.OpenText(Page.Server.MapPath(e.Params["file"].Get<string>())))
			{
				tmp["html"].Value = reader.ReadToEnd();
			}

			RaiseActiveEvent(
				"magix.forms.change-html",
				tmp);

			if (!(e.Params.Contains("freeze-help-stack") && 
			    e.Params["freeze-help-stack"].Get<bool>()))
			{
				tmp = new Node();
				tmp["page"].Value = e.Params["file"].Get<string>();

				RaiseActiveEvent(
					"magix.help.add-page",
					tmp);
			}
		}

		private List<string> Pages
		{
			get
			{
				if (Page.Session["ControllerHelper.Pages"] == null)
					Page.Session["ControllerHelper.Pages"] = new List<string>();
				return Page.Session["ControllerHelper.Pages"] as List<string>;
			}
		}

		private string Next
		{
			get
			{
				return Page.Session["ControllerHelper.Next"] as string;
			}
			set
			{
				Page.Session["ControllerHelper.Next"] = value;
			}
		}

		private int CurrentIndex
		{
			get
			{
				if (Page.Session["ControllerHelper.CurrentIndex"] == null)
					Page.Session["ControllerHelper.CurrentIndex"] = -1;
				return (int)Page.Session["ControllerHelper.CurrentIndex"];
			}
			set { Page.Session["ControllerHelper.CurrentIndex"] = value; }
		}

		/**
		 * Adds a page to the stack of forward/backward stored pages
		 */
		[ActiveEvent(Name = "magix.help.add-page")]
		public void magix_help_add_page(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.help.add-page"].Value = null;
				e.Params["page"].Value = "Help/path-to-page.mml";
				e.Params["reset"].Value = false;
				e.Params["inspect"].Value = @"adds the given [page] to the history 
of the help system.&nbsp;&nbsp;set [reset] to true to reset entire history stack
of help system.&nbsp;&nbsp;not thread safe";
				return;
			}

			if (e.Params.Contains("reset") && e.Params["reset"].Get<bool>())
			{
				CurrentIndex = -1;
				Pages.Clear();
			}

			if (!e.Params.Contains("page"))
				throw new ArgumentException("need to know which page to add");

			while (Pages.Count - 1 > CurrentIndex)
				Pages.RemoveAt(Pages.Count - 1);

			CurrentIndex += 1;
			Pages.Add(e.Params["page"].Get<string>());
		}

		/**
		 * Moves backkwards to previous help page
		 */
		[ActiveEvent(Name = "magix.help.move-backwards")]
		public void magix_help_move_backwards(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.help.move-backwards"].Value = null;
				e.Params["inspect"].Value = @"opens the previously opened help file.
&nbsp;&nbsp;not thread safe";
				return;
			}

			if (CurrentIndex <= 0)
			{
				Node ms = new Node();
				ms["message"].Value = "No more previous screens";

				RaiseActiveEvent(
					"magix.viewport.show-message",
					ms);

				return;
			}

			CurrentIndex -= 1;

			Node tmp = new Node();

			tmp["freeze-help-stack"].Value = true;
			tmp["file"].Value = Pages[CurrentIndex];

			RaiseActiveEvent(
				"magix.help.open-file",
				tmp);

			e.Params["success"].Value = true;
		}

		/**
		 * Moves forwards to next help page
		 */
		[ActiveEvent(Name = "magix.help.move-forwards")]
		public void magix_help_move_forwards(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.help.move-backwards"].Value = null;
				e.Params["inspect"].Value = @"moves forward in the history of 
opened help pages.&nbsp;&nbsp;not thread safe";
				return;
			}

			if (CurrentIndex >= Pages.Count - 1)
			{
				Node ms = new Node();
				ms["message"].Value = "No more forward screens";

				RaiseActiveEvent(
					"magix.viewport.show-message",
					ms);
				return;
			}

			CurrentIndex += 1;

			Node tmp = new Node();

			tmp["freeze-help-stack"].Value = true;
			tmp["file"].Value = Pages[CurrentIndex];

			RaiseActiveEvent(
				"magix.help.open-file",
				tmp);
		}
	}
}
