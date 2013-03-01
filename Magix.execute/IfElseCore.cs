/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Controller for "magix.execute.if/else-if/else" logic
	 */
	public class IfElseCore : ActiveController
	{
		/**
		 * Checks to see if the current 
		 * statement is returning true, and if so, executes the underlaying nodes
		 * as code, through "magix.execute", expecting them to be keywords to
		 * the execution engine. You can either compare a node expression
		 * with another node expression, a node expression with a
		 * constant value or a single node expression for existence of 
		 * Node itself, Value or Name. Operators you can use are '!=', '==', '>=',
		 * '<=', '>' and '<' when comparing two nodes or one node and 
		 * a constant, and '!' in front of operator if only one 
		 * expression is given to check for existence to negate the value.
		 * Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.if")]
		public static void magix_execute_if(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Executes the child nodes as an execution
block, but only if the ""if"" statement returns True.
Pair together with ""else-if"" and ""else"" to create
branching and control of flow of your program. If an ""if""
statement returns True, then no paired ""else-if"" or ""else""
statements will be executed.";
				e.Params["Data"]["Item1"].Value = "Cache1";
				e.Params["Data"]["Cache"].Value = null;
				e.Params["if"].Value = "[Data][Item1].Value!=[Data][1].Name";
				e.Params["if"]["magix.viewport.show-message"].Value = null;
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "They are NOT the same!";
				return;
			}

			IfImplementation(
				e.Params, 
				"magix.execute.if");
		}

		/**
		 * If no previous "if" statement,
		 * or "else-if" statement has returned true, will check to see if the current 
		 * statement is returning true, and if so, executes the underlaying nodes
		 * as code through "magix.execute", expecting them to be keywords to
		 * the execution engine. You can either compare a node expression
		 * with another node expression, a node expression with a
		 * constant value or a single node expression for existence of 
		 * Node itself, Value or Name. Operators you can use are '!=', '==', '>=',
		 * '<=', '>' and '<' when comparing two nodes or one node and 
		 * a constant, and '!' in front of operator if only one 
		 * expression is given to check for existence to negate the value.
		 * Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.else-if")]
		public static void magix_execute_else_if(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Executes the underlaying code block,
but only if no previous ""if"" or ""else-if"" statement has returned True,
and the statement inside the Value of the ""else-if"" 
returns True.";
				e.Params["Data"]["Node"].Value = null;
				e.Params["if"].Value = "[Data][Node].Value";
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "Darn it...";
				e.Params["else-if"].Value = "[Data][Node]";
				e.Params["else-if"]["magix.viewport.show-message"]["message"].Value = "Works...";
				return;
			}

			IfImplementation(
				e.Params, 
				"magix.execute.else-if");
		}

		/**
		 * If no previous "if" statement,
		 * or "else-if" statement has returned true, execute the underlaying nodes
		 * as code through "magix.execute", expecting them to be keywords to
		 * the execution engine. Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.else")]
		public static void magix_execute_else(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Executes the underlaying code block,
but only if no paired ""if"" or ""else-if"" statement
has returned True.";
				e.Params["if"].Value = "[if].Name==x_if";
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "Ohh crap ...";
				e.Params["else"]["magix.viewport.show-message"]["message"].Value = "Yup, I'm still sane ...";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			// Checking to see if a previous "if" or "else-if" statement has returned true
			if (ip.Parent != null &&
			    ip.Parent["_state"].Get<bool>())
				return;

			Node dp = e.Params;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			Node node = new Node("magix.execute.else");

			node["_ip"].Value = ip;
			node["_dp"].Value = dp;

			RaiseEvent(
				"magix.execute", 
				node);
		}

		// Private helper, implementation for both "if" and "else-if" ...
		private static void IfImplementation (Node pars, string evt)
		{
			Node ip = pars;
			if (pars.Contains("_ip"))
				ip = pars["_ip"].Value as Node;

			Node dp = pars;
			if (pars.Contains("_dp"))
				dp = pars["_dp"].Value as Node;

			if (ip.Parent == null)
				throw new ApplicationException("Cannot have an if statement without a parent node");

			// Defaulting if statement to return "false"
			ip.Parent ["_state"].Value = false;

			string expr = ip.Value as string;

			if (string.IsNullOrEmpty (expr))
				throw new ArgumentException ("You cannot have an empty if/else-if statement");

			if (Expressions.IsTrue(expr, ip, dp))
			{
				// Making sure statement returns "true"
				if (ip.Parent != null)
					ip.Parent["_state"].Value = true;

				Node tmp = new Node(evt);
				tmp["_ip"].Value = ip;
				tmp["_dp"].Value = dp;

				RaiseEvent(
					"magix.execute", 
					tmp);
			}
		}
	}
}
