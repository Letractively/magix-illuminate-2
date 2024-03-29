/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * raise hyper lisp keyword
	 */
	public class RaiseCore : ActiveController
	{
		/*
		 * raise hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.raise")]
		public static void magix_execute_raise(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will raise the active event
given in value, with the parameters found as child 
nodes underneath the [raise] keyword itself.&nbsp;&nbsp;note, 
if your active event contains '.', then you can
raise the active event directly, without using
the raise keyword, by creating a node who's
name is the name of your active event.&nbsp;&nbsp;add up
[no-override] with a value of true if you wish
to go directly to the active event in question,
and not rely upon any overrides.&nbsp;&nbsp;this is useful
for having the possibility of calling base
functionality from overridden active events.&nbsp;&nbsp;thread safe";
				e.Params["raise"].Value = "magix.viewport.show-message";
				e.Params["raise"]["message"].Value = "hi there world";
				e.Params["raise"]["no-override"].Value = "False";
				e.Params["magix.viewport.show-message"].Value = null;
				e.Params["magix.viewport.show-message"]["message"].Value = "hi there world 2.0, hyper lisp world";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			bool forceNoOverride = false;
			if (ip.Contains("no-override") && ip["no-override"].Get<bool>())
				forceNoOverride = true;

			Node tmpParent = ip.Parent;
			ip.SetParent(null); // to avoid that methods have access to parent nodes it shouldn't have access to ...
			try
			{
				if (ip.Name == "raise")
				{
					RaiseActiveEvent(ip.Get<string>(), ip, forceNoOverride);
				}
				else
				{
					RaiseActiveEvent(ip.Name, ip, forceNoOverride);
				}
			}
			finally
			{
				ip.SetParent(tmpParent);
			}
		}
	}
}

