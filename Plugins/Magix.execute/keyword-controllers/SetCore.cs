/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * hyperlisp set keyword
	 */
	public class SetCore : ActiveController
	{
		/*
		 * hyepr lisp set keyword
		 */
		[ActiveEvent(Name = "magix.execute.set")]
		public static void magix_execute_set(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.set-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.set-sample]");
                return;
			}

			string destinationExpression = ip.Get<string>();
			string sourceExpression = null;

            if (ip.Contains("value"))
                sourceExpression = ip["value"].Get<string>();

            Node dp = Dp(e.Params);
			Expressions.SetNodeValue(destinationExpression, sourceExpression, dp, ip, ip.Contains("value"));
		}
	}
}

