/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.Core;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using Magix.UX.Widgets.Core;

namespace Magix.Core
{
    /*
     * base class for viewports
     */
    public abstract class Viewport : ActiveModule
    {
        /*
         * override to return default container
         */
        protected abstract string GetDefaultContainer();

        /*
         * override to return all containers, except the system containers
         */
        protected abstract string[] GetAllDefaultContainers();

        /*
         * contains all css files
         */
        private List<string> CssFiles
        {
            get
            {
                if (ViewState["CssFiles"] == null)
                    ViewState["CssFiles"] = new List<string>();
                return ViewState["CssFiles"] as List<string>;
            }
        }

        /*
         * contains all javascript files
         */
        private List<string> JavaScriptFiles
        {
            get
            {
                if (ViewState["JavaScriptFiles"] == null)
                    ViewState["JavaScriptFiles"] = new List<string>();
                return ViewState["JavaScriptFiles"] as List<string>;
            }
        }

        protected override void OnInit(EventArgs e)
		{
			Load +=
				delegate
				{
					Page_Load_Initializing();
		            if (!Manager.Instance.IsAjaxCallback && IsPostBack)
		            {
		                IncludeAllCssFiles();
		                IncludeAllJsFiles();
		            }
				};
			base.OnInit(e);
            Page_Init_Initializing();
		}

        private void Page_Init_Initializing()
        {
            Node node = new Node();
            node["is-postback"].Value = IsPostBack;

            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                "magix.viewport.page-init",
                node);
        }

		private void Page_Load_Initializing()
		{
			if (!IsPostBack)
			{
				// Checking to see if this is a remotely activated Active Event
				// And if so, raising the "magix.viewport.remote-event" active event
	            if (!Manager.Instance.IsAjaxCallback && 
	                Request.HttpMethod == "POST" &&
	                !string.IsNullOrEmpty(Page.Request["event"]))
	            {
					// We only raise events which are allowed to be remotely invoked
					if (ActiveEvents.Instance.IsAllowedRemotely(Page.Request["event"]))
					{
						Node node = new Node();

                        if (!string.IsNullOrEmpty(Page.Request["params"]))
                            node[Page.Request["event"]].AddRange(Node.FromJSONString(Page.Request["params"]));
                        else
                            node[Page.Request["event"]].Value = null;

                        bool inspect = node[Page.Request["event"]].Contains("inspect");

                        Node exe = new Node();
                        exe["_ip"].Value = node;
                        exe["_dp"].Value = node;

						RaiseActiveEvent(
							"magix.execute",
							exe);

                        if (inspect)
                        {
                            // removing all nodes from result, except inspect
                            Node inspectNode = node[Page.Request["event"]]["inspect"];
                            node[Page.Request["event"]].Clear();
                            node[Page.Request["event"]].Add(inspectNode);
                        }

						Page.Response.Clear();
                        Page.Response.Write("return:" + node[Page.Request["event"]].ToJSONString());
						try
						{
							Page.Response.End();
						}
						catch
						{
							; // Intentionally do nothing ...
						}
					}
				}
				else
				{
					RaiseActiveEvent("magix.viewport.page-load");
				}
			}
		}

        /*
         * helper, re-includes all css files
         */
        private void IncludeAllCssFiles()
        {
            foreach (string idx in CssFiles)
            {
                IncludeCssFile(idx);
            }
        }

        /*
         * helper, re-includes all javascript files
         */
        private void IncludeAllJsFiles()
        {
            foreach (string idx in JavaScriptFiles)
            {
                IncludeJavaScriptFile(idx);
            }
        }

        /*
         * helper for clearing controls from container, such that it gets un-registered
         */
		private void ClearControls(DynamicPanel dynamic)
        {
            foreach (Control idx in dynamic.Controls)
            {
                ActiveEvents.Instance.RemoveListener(idx);
            }
            dynamic.ClearControls();
        }

        /*
         * changes the title of the web page
         */
        [ActiveEvent(Name = "magix.viewport.lock-to-device-width")]
        protected void magix_viewport_lock_to_device_width(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.lock-to-device-width-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.lock-to-device-width-sample]");
                return;
            }

            LiteralControl lit = new LiteralControl();
            lit.Text = @"
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
";
            Page.Header.Controls.Add(lit);
        }

        /*
         * changes the title of the web page
         */
        [ActiveEvent(Name = "magix.viewport.set-title")]
        protected void magix_viewport_set_title(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.set-title-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.set-title-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("title"))
                throw new ArgumentException("no [title] given to [magix.viewport.set-title]");
            string title = Expressions.GetExpressionValue<string>(ip["title"].Get<string>(), dp, ip, false);

            Page.Title = title;
        }

		/*
		 * clears the given container
         */
        [ActiveEvent(Name = "magix.viewport.clear-controls")]
		protected virtual void magix_viewport_clear_controls(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.clear-controls-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.clear-controls-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string container = Expressions.GetExpressionValue<string>(ip.GetValue("container", ""), dp, ip, false);
            bool resetClass = ip.ContainsValue("reset-class") ?
                Expressions.GetExpressionValue<bool>(ip["reset-class"].Get<string>(), dp, ip, false) :
                false;

            string resetClassNewClass = null;
            if (ip.Contains("reset-class") && ip["reset-class"].Contains("new-class"))
                resetClassNewClass = ip["reset-class"]["new-class"].Get<string>("");

            if (ip.ContainsValue("all") && ip["all"].Get<bool>())
            {
                foreach (string idx in GetAllDefaultContainers())
                {
                    DynamicPanel dyn = Selector.FindControl<DynamicPanel>(
                        this,
                        idx);
                    ClearControls(dyn);
                    if (resetClass || !string.IsNullOrEmpty(resetClassNewClass))
                    {
                        if (!string.IsNullOrEmpty(resetClassNewClass))
                            dyn.Class = resetClassNewClass;
                        else
                            dyn.Class = "";
                    }
                }
            }
            else
            {
                DynamicPanel dyn = Selector.FindControl<DynamicPanel>(
                    this,
                    container);

                if (dyn == null)
                    return;
                ClearControls(dyn);
                if (resetClass || !string.IsNullOrEmpty(resetClassNewClass))
                {
                    if (!string.IsNullOrEmpty(resetClassNewClass))
                        dyn.Class = resetClassNewClass;
                    else
                        dyn.Class = "";
                }
            }
		}

		/*
		 * executes the given javascript
		 */
		[ActiveEvent(Name = "magix.viewport.execute-javascript")]
		protected void magix_viewport_execute_javascript(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.execute-javascript-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.execute-javascript-sample]");
                return;
			}

            if (!ip.ContainsValue("script"))
				throw new ArgumentException("no [script] given to [magix.viewport.execute-script]");

            Node dp = Dp(e.Params);

            string script = Expressions.GetFormattedExpression("script", e.Params, "");
			Manager.Instance.JavaScriptWriter.Write(script);
		}

        /*
         * helper for re-including css files
         */
        private void IncludeCssFile (string cssFile)
		{
			if (!string.IsNullOrEmpty (cssFile))
			{
				if (cssFile.Contains("~"))
				{
					string appPath = HttpContext.Current.Request.Url.ToString ();
					appPath = appPath.Substring (0, appPath.LastIndexOf ('/'));
					cssFile = cssFile.Replace ("~", appPath);
				}
				if (Manager.Instance.IsAjaxCallback)
				{
					Manager.Instance.JavaScriptWriter.Write (
                        @"MUX.Element.prototype.includeCSS('<link href=""{0}"" rel=""stylesheet"" type=""text/css"" />');", cssFile);
				}
				else
				{
					LiteralControl lit = new LiteralControl ();
					lit.Text = string.Format (@"
<link href=""{0}"" rel=""stylesheet"" type=""text/css"" />
",
                        cssFile);
					Page.Header.Controls.Add (lit);
				}
			}
		}

        /*
         * helper for including javascript file
         */
        private void IncludeJavaScriptFile(string javaScriptFile)
        {
            javaScriptFile = javaScriptFile.Replace("~/", GetApplicationBaseUrl());
            Manager.Instance.IncludeFileScript(javaScriptFile);
        }

        /*
         * includes a file on the client
         */
        [ActiveEvent(Name = "magix.viewport.include-client-file")]
		protected void magix_viewport_include_client_file(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.include-client-file-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.include-client-file-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("type"))
                throw new ArgumentException("no [type] given to [magix.viewport.include-client-file]");
            string type = Expressions.GetExpressionValue<string>(ip["type"].Get<string>(), dp, ip, false);

            if (!ip.ContainsValue("file"))
                throw new ArgumentException("no [file] given to [magix.viewport.include-client-file]");
            string file = Expressions.GetExpressionValue<string>(ip["file"].Get<string>(), dp, ip, false);

            if (type == "css")
			{
                if (!CssFiles.Contains(file))
                {
                    CssFiles.Add(file);
                    IncludeCssFile(file);
                }
			}
            else if (type == "javascript")
			{
                if (!JavaScriptFiles.Contains(file))
                {
                    JavaScriptFiles.Add(file);
                    IncludeJavaScriptFile(file);
                }
			}
			else
                throw new ArgumentException("only 'css' and 'javascript' are legal types in [magix.viewport.include-client-file]");
		}

		/*
		 * sets a viewstate object
		 */
		[ActiveEvent(Name = "magix.viewstate.set")]
		protected virtual void magix_viewstate_set(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewstate.set-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewstate.set-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("id"))
                throw new ArgumentException("no [id] given to [magix.viewstate.set]");
            string id = Expressions.GetFormattedExpression("id", e.Params, "");

            if (!ip.Contains("value"))
			{
				if (ViewState[id] != null)
					ViewState.Remove(id);
			}
			else
			{
                Node value = null;
                if (ip.ContainsValue("value") && ip["value"].Get<string>().StartsWith("["))
                    value = Expressions.GetExpressionValue<Node>(ip["value"].Get<string>(), dp, ip, false).Clone();
                else
                    value = ip["value"].Clone();
				ViewState[id] = value;
			}
		}

		/*
		 * retrieves a viewstate object
		 */
		[ActiveEvent(Name = "magix.viewstate.get")]
		protected virtual void magix_viewstate_get(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewstate.get-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewstate.get-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("id"))
                throw new ArgumentException("no [id] given to [magix.viewstate.get]");
            string id = Expressions.GetFormattedExpression("id", e.Params, "");

            if (ViewState[id] != null && ViewState[id] is Node)
            {
                ip["value"].UnTie();
                ip.Add((ViewState[id] as Node).Clone());
            }
		}

		/*
         * loads an active module
         */
        [ActiveEvent(Name = "magix.viewport.load-module")]
		protected virtual void magix_viewport_load_module(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.load-module-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.load-module-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            string container = GetDefaultContainer();
            if (ip.ContainsValue("container"))
                container = Expressions.GetExpressionValue<string>(ip["container"].Get<string>(), dp, ip, false);

			DynamicPanel dyn = Selector.FindControl<DynamicPanel>(
            	this,
                container);

            if (dyn == null && ip.ContainsValue("container"))
				return;

            string moduleName = Expressions.GetExpressionValue<string>(ip["name"].Get<string>(), dp, ip, false);

            ClearControls(dyn);

            if (ip.ContainsValue("class"))
                dyn.Class = Expressions.GetExpressionValue<string>(ip["class"].Get<string>(), dp, ip, false);

            if (ip.ContainsValue("style"))
            {
                // clearing old styles
                foreach (string idx in dyn.Style.Keys)
                {
                    dyn.Style[idx] = "";
                }

                // setting new styles
                string[] styles =
                    ip["style"].Get<string>().Replace("\n", "").Replace("\r", "").Split(
                        new char[] { ';' },
                        StringSplitOptions.RemoveEmptyEntries);
                foreach (string idxStyle in styles)
                {
                    dyn.Style[idxStyle.Split(':')[0]] = idxStyle.Split(':')[1];
                }
            }

            dyn.LoadControl(moduleName, e.Params);
        }

		/*
         * reloading controls upon postbacks
		 */
        protected void dynamic_LoadControls(object sender, DynamicPanel.ReloadEventArgs e)
        {
            DynamicPanel dynamic = sender as DynamicPanel;
            Control ctrl = ModuleControllerLoader.Instance.LoadActiveModule(e.Key);
            if (e.FirstReload)
            {
                ActiveModule module = ctrl as ActiveModule;
                if (module != null)
                {
                    Node nn = e.Extra as Node;
                    ctrl.Init +=
                        delegate
                        {
                            module.InitialLoading(nn);
                        };
                }
            }
            dynamic.Controls.Add(ctrl);
        }
    }
}
