/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * hyperlisp event support
	 */
	public class EventCore : ActiveController
	{
        private static Node _events = new Node();
        private static Node _inspect = new Node();

        /*
         * creates the associations between existing events in the database, and active event references
         */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.application-startup-dox].value");
				return;
			}

			Node tmp = new Node();
			tmp["prototype"]["type"].Value = "magix.execute.event";

			RaiseActiveEvent(
				"magix.data.load",
				tmp);

			if (tmp.Contains("objects"))
			{
				foreach (Node idx in tmp["objects"])
				{
					ActiveEvents.Instance.CreateEventMapping(
                        idx["value"]["event"].Get<string>(), 
						"magix.execute._active-event-2-code-callback");

                    if (idx["value"].Contains("remotable") && idx["value"]["remotable"].Get<bool>())
                        ActiveEvents.Instance.MakeRemotable(idx["value"]["event"].Get<string>());

                    _events[idx["value"]["event"].Get<string>()].Clear();
                    _events[idx["value"]["event"].Get<string>()].AddRange(idx["value"]["code"]);
                    if (idx["value"].ContainsValue("inspect"))
                        AppendInspect(
                            true,
                            _inspect[idx["value"]["event"].Get<string>()],
                            idx["value"]["inspect"].Get<string>());
				}
			}
		}

        /*
         * event hyperlisp keyword
         */
        [ActiveEvent(Name = "magix.execute.event")]
        public static void magix_execute_event(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip) && !ip.Contains("code"))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.event-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.event-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("name"))
                throw new ArgumentException("you cannot create an event without a [name]");
            string activeEvent = Expressions.GetExpressionValue<string>(ip["name"].Get<string>(), dp, ip, false);

			if (ip.Contains("code"))
			{
                CreateActiveEvent(ip, activeEvent, e.Params);
			}
			else
			{
                RemoveActiveEvent(ip, activeEvent, e.Params);
			}
		}

        /*
         * creates an active event
         */
        private static void CreateActiveEvent(Node ip, string activeEvent, Node pars)
        {
            Node dp = Dp(pars);

            bool remotable = ip.Contains("remotable") && ip["remotable"].Get<bool>();

            string inspect = Expressions.GetFormattedExpression("inspect", pars, "");

            if (!ip.Contains("persist") || ip["persist"].Get<bool>())
            {
                // removing any previous similar events
                DataBaseRemoval.Remove(activeEvent, "magix.execute.event", pars);

                Node saveNode = new Node("magix.data.save");
                saveNode["id"].Value = Guid.NewGuid().ToString();
                saveNode["value"]["event"].Value = activeEvent;
                saveNode["value"]["type"].Value = "magix.execute.event";
                saveNode["value"]["remotable"].Value = remotable;
                if (ip.Contains("inspect"))
                    saveNode["value"]["inspect"].Value = inspect;
                saveNode["value"]["code"].AddRange(ip["code"].Clone());

                BypassExecuteActiveEvent(saveNode, pars);
            }

            ActiveEvents.Instance.CreateEventMapping(
                activeEvent,
                "magix.execute._active-event-2-code-callback");

            if (ip.Contains("remotable"))
            {
                if (remotable)
                    ActiveEvents.Instance.MakeRemotable(activeEvent);
                else
                    ActiveEvents.Instance.RemoveRemotable(activeEvent);
            }

            _events[activeEvent].Clear();
            _events[activeEvent].AddRange(ip["code"].Clone());

            _inspect[activeEvent].Clear();
            if (ip.Contains("inspect"))
                _inspect[activeEvent].Value = inspect;
        }

        /*
         * removes an active event
         */
        private static void RemoveActiveEvent(Node ip, string activeEvent, Node pars)
        {
            if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                DataBaseRemoval.Remove(activeEvent, "magix.execute.event", pars);

            ActiveEvents.Instance.RemoveMapping(activeEvent);
            ActiveEvents.Instance.RemoveRemotable(activeEvent);

            _events[activeEvent].UnTie();
            _inspect[activeEvent].UnTie();
        }

        /*
         * entry point for hyperlisp created active event overrides
         */
        [ActiveEvent(Name = "magix.execute._active-event-2-code-callback")]
        public static void magix_execute__active_event_2_code_callback(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                if (e.Name == "magix.execute._active-event-2-code-callback")
                {
                    AppendInspectFromResource(
                        ip["inspect"],
                        "Magix.execute",
                        "Magix.execute.hyperlisp.inspect.hl",
                        "[magix.execute._active-event-2-code-callback-dox].value");
                }
                else if (_events.Contains(e.Name))
                {
                    AppendInspectFromResource(
                        ip["inspect"],
                        "Magix.execute",
                        "Magix.execute.hyperlisp.inspect.hl",
                        "[magix.execute._active-event-2-code-callback-dynamic-dox].value");

                    if (_inspect.ContainsValue(e.Name))
                    {
                        ip["inspect"].Value = null; // dropping the default documentation
                        AppendInspect(ip["inspect"], _inspect[e.Name].Get<string>());
                    }
                    foreach (Node idx in _events[e.Name])
                    {
                        ip.Add(idx.Clone().UnTie());
                    }
                }
                return;
            }

            Node dp = Dp(e.Params);

            Node code = GetEventCode(e.Name);
            if (code != null)
            {
                e.Params["_ip"].Value = code;
                if (!e.Params.Contains("_dp") || (ip.Name != null && ip.Name.Contains(".")))
                    e.Params["_dp"].Value = code;

                Node nDp = Dp(e.Params);
                if (ip.Count > 0)
                    nDp["$"].AddRange(ip);

                // making sure we're starting with correct namespace
                e.Params["_namespaces"].Add(new Node("item", "magix.execute"));
                e.Params["_root-only-execution"].UnTie();
                try
                {
                    if (e.Params.Contains("_whitelist"))
                        e.Params["_whitelist"].Name = "_whitelist-old";
                    RaiseActiveEvent(
                        "magix.execute",
                        e.Params);
                    ip.Clear();
                    if (nDp.Contains("$") && nDp["$"].Count > 0)
                        ip.AddRange(nDp["$"]);
                    if (nDp.ContainsValue("$"))
                        ip.Value = nDp["$"].Value;
                    nDp["$"].UnTie();
                    e.Params["_dp"].Value = dp;
                    e.Params["_ip"].Value = ip;
                }
                catch (Exception err)
                {
                    while (err.InnerException != null)
                        err = err.InnerException;

                    if (!(err is HyperlispStopException))
                        throw;
                }
                finally
                {
                    if (e.Params.Contains("_whitelist-old"))
                        e.Params["_whitelist-old"].Name = "_whitelist";
                    if (ip != e.Params)
                    {
                        e.Params["_namespaces"][e.Params["_namespaces"].Count - 1].UnTie();
                        if (e.Params["_namespaces"].Count == 0)
                            e.Params["_namespaces"].UnTie();
                        e.Params["_ip"].Value = ip;
                        e.Params["_dp"].Value = dp;
                    }
                }
            }
        }

        /*
         * get active events active event
         */
        [ActiveEvent(Name = "magix.execute.list-events")]
        public static void magix_execute_list_events(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.list-events-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.list-events-sample]");
                return;
            }

            bool open = false;
            if (ip.Contains("open"))
                open = ip["open"].Get<bool>();

            bool all = true;
            if (ip.Contains("all"))
                all = ip["all"].Get<bool>();

            bool remoted = false;
            if (ip.Contains("remoted"))
                remoted = ip["remoted"].Get<bool>();

            bool overridden = false;
            if (ip.Contains("overridden"))
                overridden = ip["overridden"].Get<bool>();

            string beginsWith = null;
            if (ip.Contains("begins-with"))
                beginsWith = ip["begins-with"].Get<string>();

            foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
            {
                if (open && !ActiveEvents.Instance.IsAllowedRemotely(idx))
                    continue;
                if (remoted && string.IsNullOrEmpty(ActiveEvents.Instance.RemotelyOverriddenURL(idx)))
                    continue;
                if (overridden && !ActiveEvents.Instance.IsOverride(idx))
                    continue;

                if (!all && !string.IsNullOrEmpty(beginsWith) && idx.Replace(beginsWith, "").Contains("_"))
                    continue;

                if (!all && idx.StartsWith("magix.test."))
                    continue;

                if (!string.IsNullOrEmpty(beginsWith) && !idx.StartsWith(beginsWith))
                    continue;

                if (string.IsNullOrEmpty(idx))
                    continue;

                ip["events"][idx].Value = null;

                string remoteOverride = ActiveEvents.Instance.RemotelyOverriddenURL(idx);
                if (!string.IsNullOrEmpty(remoteOverride))
                    ip["events"][idx]["url"].Value = remoteOverride;
            }

            ip["events"].Sort(
                delegate(Node left, Node right)
                {
                    return left.Name.CompareTo(right.Name);
                });
        }

        /*
         * returns stored active event
         */
        private static Node GetEventCode(string name)
		{
			if (_events.Contains(name) && _events[name].Count > 0)
				return _events[name].Clone();
			else if (_events.Contains(name))
				return null;

			lock (typeof(EventCore))
			{
				if (_events.Contains(name) && _events[name].Count > 0)
					return _events[name].Clone();
				else if (_events.Contains(name))
					return null;
				else
				{
					Node evtNode = new Node();

					evtNode["prototype"]["type"].Value = "magix.execute.event";
					evtNode["prototype"]["event"].Value = name;

					RaiseActiveEvent(
						"magix.data.load",
						evtNode);

					if (evtNode.Contains("objects"))
					{
                        _events[name].AddRange(evtNode["objects"][0]["value"]["code"].UnTie());
                        return _events[name].Clone();
					}
					_events[name].Value = null;
					return null;
				}
			}
		}
	}
}

