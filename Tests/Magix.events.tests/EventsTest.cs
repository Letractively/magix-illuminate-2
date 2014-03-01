/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.tests
{
	/**
	 * Contains Unit Tests for "magix.execute" active event
	 */
	public class EventsTest : ActiveController
	{
		/**
		 * Tests to see if create "event", and invoking it later, works
		 */
		[ActiveEvent(Name = "magix.test.execute.invoke-event")]
		public static void magix_test_execute_invoke_event(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["event"].Value = "foo.bar";
			tmp["event"]["code"]["_data"].Value = "howdy";
			tmp["event"]["code"]["set"].Value = "[$][_data].Value";
			tmp["event"]["code"]["set"]["value"].Value = "thomas";
			tmp["foo.bar"].Value = null;
			tmp.Add (new Node("event", "foo.bar"));

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that creating and invoking an 
[event] behaves correctly, and also that event is destroyed when supposed to";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (!tmp["foo.bar"].Contains("_data") || 
			    tmp["foo.bar"]["_data"].Get<string>() != "thomas")
			{
				throw new ApplicationException(
					"Failure of executing event invoke statement");
			}

			foreach (string idx in ActiveEvents.Instance.EventMappingKeys)
			{
				if (idx == "foo.bar")
					throw new ApplicationException(
						"Failure of executing event test, event not destroyed as supposed to");
			}
		}

		/**
		 * Tests to see if create "event", and invoking it later, works
		 */
		[ActiveEvent(Name = "magix.test.execute.invoke-remote")]
		public static void magix_test_execute_invoke_remote(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["event"].Value = "foo.bar";
			tmp["event"]["remotable"].Value = true;
			tmp["event"]["code"]["set"].Value = "[$][Data].Value";
			tmp["event"]["code"]["set"]["value"].Value = "thomas";
			tmp["remote"].Value = "foo.bar";
			tmp["remote"]["url"].Value = "http://localhost:8080";
			tmp.Add (new Node("event", "foo.bar"));

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that creating an [event] which is remotable 
behaves correctly by raising it using [remote] and verify parameters are changed 
as supposed to";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["remote"]["Data"].Get<string>() != "thomas")
			{
				throw new ApplicationException(
					"Failure of executing remote statement");
			}
		}
	}
}

