/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Net;
using Magix.Core;
using Magix.UX.Builder;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;

namespace Magix.execute
{
	/**
	 * Contains the logic for invoking remote events
	 */
	public class RemotingCore : ActiveController
	{
		private static string _dbFile = "store.db4o";

		public class RemoteOverrides
		{
			private string _key;
			private string _url;

			public string Key
			{
				get { return _key; }
				set { _key = value; }
			}

			public string Url
			{
				get { return _url; }
				set { _url = value; }
			}
		}

		public class OpenEvents
		{
			private string _key;

			public string Key
			{
				get { return _key; }
				set { _key = value; }
			}
		}

		/**
		 * Handled to make sure we map our overridden magix.execute events during
		 * app startup
		 */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["initial-startup-of-process"].Value = null;
				e.Params["inspect"].Value = @"Called during startup
of application to make sure our Active Events, 
which are dynamically remotely overridden
are being correctly re-mapped. 
""initial-startup-of-process"" must exists to run event.";
				return;
			}
			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext ().Configure ().UpdateDepth (1000);
					db.Ext ().Configure ().ActivationDepth (1000);

					foreach (RemoteOverrides idx in db.QueryByExample (new RemoteOverrides()))
					{
						ActiveEvents.Instance.OverrideRemotely (idx.Key, idx.Url);
					}

					foreach (OpenEvents idx in db.QueryByExample (new OpenEvents()))
					{
						ActiveEvents.Instance.MakeRemotable (idx.Key);
					}
				}
			}
		}

		/**
		 * Creates a remote override for any active event in your system
		 */
		[ActiveEvent(Name = "magix.execute.tunnel")]
		public static void magix_execute_tunnel (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect") &&
			    e.Params["inspect"].Get<string>("") == "")
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Creates an external override towards the given 
""URL"". The event overridden is found in ""event"". If you pass
in a null value as a URL, or no URL node, the override is removed.
Please make sure the other side has marked the active event as
remotable.";
				e.Params["override-remotely"].Value = "magix.namespace.foo";
				e.Params["override-remotely"]["URL"].Value = "http://127.0.0.1:8080";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (ip.Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.override-remotely needs event parameter to know which event to raise externally");

			string url = ip.Contains ("URL") ? ip["URL"].Get<string>() : null;
			string evt = ip.Get<string>("");

			if (url == null)
			{
				ActiveEvents.Instance.RemoveRemoteOverride(evt);
				lock (typeof(Node))
				{
					using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
					{
						db.Ext ().Configure ().UpdateDepth (1000);
						db.Ext ().Configure ().ActivationDepth (1000);

						bool found = false;
						foreach (RemoteOverrides idx in db.QueryByExample (new RemoteOverrides()))
						{
							if (evt == idx.Key)
							{
								idx.Url = url;
								db.Store (idx);
								found = true;
								break;
							}
						}
						if (!found)
						{
							RemoteOverrides nEvt = new RemoteOverrides();
							nEvt.Key = evt;
							db.Store (nEvt);
						}
						db.Commit ();
						ActiveEvents.Instance.OverrideRemotely(evt, url);
					}
				}
			}
			else
			{
				lock (typeof(Node))
				{
					using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
					{
						db.Ext ().Configure ().UpdateDepth (1000);
						db.Ext ().Configure ().ActivationDepth (1000);

						bool found = false;
						foreach (RemoteOverrides idx in db.QueryByExample (new RemoteOverrides()))
						{
							if (evt == idx.Key)
							{
								found = true;
								break;
							}
						}
						if (!found)
						{
							RemoteOverrides nEvt = new RemoteOverrides();
							nEvt.Key = evt;
							db.Store (nEvt);
						}
						db.Commit ();
						ActiveEvents.Instance.OverrideRemotely(evt, url);
					}
				}
			}
		}

		/**
		 * Allows an event to be remotly invoked within your system
		 */
		[ActiveEvent(Name = "magix.execute.open")]
		public static void magix_execute_open (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect") &&
			    e.Params["inspect"].Get<string>("") == "")
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Allows the given Value event
to be remotely invoked.";
				e.Params["allow-remotely"].Value = "magix.namespace.foo";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (ip.Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.override-remotely needs event parameter to know which event to raise externally");

			string evt = ip.Get<string>();

			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);

				bool found = false;
				foreach (OpenEvents idx in db.QueryByExample (new OpenEvents()))
				{
					if (evt == idx.Key)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					RemoteOverrides nEvt = new RemoteOverrides();
					nEvt.Key = evt;
					db.Store (nEvt);
				}
				db.Commit ();
				ActiveEvents.Instance.MakeRemotable (evt);
			}
		}

		/**
		 * Raises a new event remotely, use "URL" and "event" to instruct which
		 * event you wish to raise at which URL end-point. Whatever you have 
		 * as "params" parameters, will be passed into the end-point server
		 * as the parameters to the active event. "params" will also
		 * contain the return value Nodes from the server, after the 
		 * active event is finished executing.
		 */
		[ActiveEvent(Name = "magix.execute.remote")]
		public static void magix_execute_remote (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect") &&
			    e.Params["inspect"].Get<string>("") == "")
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Remotely invokes an active event on the given 
""URL"". The event raised is found in Value.";
				e.Params["remote"].Value = "magix.namespace.foo";
				e.Params["remote"]["URL"].Value = "http://127.0.0.1:8080";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (!ip.Contains ("URL") || ip["URL"].Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.remote needs URL parameter to know which endpoint to go towards");

			if (ip.Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.remote needs event parameter to know which event to raise externally");

			string url = ip["URL"].Get<string>();
			string evt = ip.Get<string>();

			HttpWebRequest req = WebRequest.Create(url) as System.Net.HttpWebRequest;
            req.Method = "POST";
            req.Referer = GetApplicationBaseUrl();
            req.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write("event=" + System.Web.HttpUtility.UrlEncode(evt));
                writer.Write("&params=" + System.Web.HttpUtility.UrlEncode(ip.ToJSONString()));
            }

            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300)
                    {
                        string val = reader.ReadToEnd();
                        if (!val.StartsWith("return:"))
                            throw new Exception(
                                "Something went wrong when connecting to '" +
                                url +
                                "'. Server responded with: " + val);

                        if (val.Length > 7)
						{
							Node tmp = Node.FromJSONString(val.Substring(7));
                            ip.ReplaceChildren (tmp);
							ip.Name = tmp.Name;
							ip.Value = tmp.Value;
						}
                    }
					else
						throw new ArgumentException("Couldn't find event '" + evt + "' on " + url);
                }
            }
		}
	}
}
