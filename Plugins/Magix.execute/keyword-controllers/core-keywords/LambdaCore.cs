/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
    /*
     * hyperlisp lambda keyword
     */
    public class LambdaCore : ActiveController
    {
        /*
         * hyper lisp lambda keyword
         */
        [ActiveEvent(Name = "magix.execute.lambda")]
        public static void magix_execute_lambda(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.lambda-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.lambda-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            // retrieving and cloning code block to execute
            Node lambdaCodeBlock = Expressions.GetExpressionValue<Node>(ip.Get<string>(), dp, ip, false);
            if (lambdaCodeBlock == null)
                throw new HyperlispExecutionErrorException("[lambda] couldn't find a block of code to execute from its expression");
            lambdaCodeBlock = lambdaCodeBlock.Clone();

            // adding parameters to code block
            if (ip.Count > 0)
                lambdaCodeBlock["$"].AddRange(ip);

            // executing lambda block
            RaiseActiveEvent(
                "magix.execute",
                lambdaCodeBlock);

            // adding parameters to ip back up again
            ip.Clear();
            if (lambdaCodeBlock.Contains("$") && lambdaCodeBlock["$"].Count > 0)
                ip.AddRange(lambdaCodeBlock["$"]);
        }
    }
}

