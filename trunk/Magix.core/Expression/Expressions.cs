/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Globalization;
using System.Collections.Generic;

namespace Magix.Core
{
    /**
     * Implementer of Expression logic, such that nodes can be retrieved
     * and manipulated using expressions, such as e.g. [Data][Name].Value, which will
     * traverse the Data node's Name node, and return its Value
     */
	public class Expressions
	{
        /**
         * Returns the value of the given expression, which might return a string, 
         * list of nodes, or any other object your node tree might contain
         */
        public static object GetExpressionValue(string expression, Node source, Node ip, bool createPath)
        {
            if (expression == null)
                return null;

            // Checking to see if this is an escaped expression
            if (expression.StartsWith("\\") && expression.Length > 1 && expression[1] == '[')
                return expression.Substring(1);

            if (!expression.TrimStart().StartsWith("["))
                return expression;

            string lastEntity = "";
            Node x = GetNode(expression, source, ip, ref lastEntity, createPath);

            if (x == null)
                return null;

            object retVal = null;

            if (lastEntity.StartsWith(".Value"))
                retVal = x.Value;
            else if (lastEntity.StartsWith(".Name"))
                retVal = x.Name;
            else if (lastEntity.StartsWith(".Count"))
                retVal = x.Count;
            else if (lastEntity == "")
                retVal = x;

            return retVal;
        }

        // TODO: Implement "strings" parsing for complex strings, such that e.g. "[Data].Value"
		// becomes a string literal, and not an expression
		/**
		 * Sets the given exprDestination to the valuer of exprSource. If
		 * exprSource starts with a '[', it is expected to be a reference to another
		 * expression, else it will be assumed to be a static value
		 */
		public static void SetNodeValue(
			string exprDestination, 
			string exprSource, 
			Node source, 
			Node ip,
			bool noRemove)
		{
			object valueToSet = GetExpressionValue(exprSource, source, ip, false);

			// checking to see if this is a string.Format expression
			if (ip.Contains("value") && ip["value"].Count > 0)
			{
				object[] arrs = new object[ip["value"].Count];
				int idxNo = 0;
				foreach (Node idx in ip["value"])
				{
					arrs[idxNo++] = Expressions.GetExpressionValue(idx.Get<string>(), source, ip, false);
				}
				valueToSet = string.Format(valueToSet.ToString(), arrs);
			}

			if (valueToSet == null && !noRemove)
			{
				Remove(exprDestination, source, ip);
				return;
			}

			string lastEntity = "";
			Node x = GetNode(exprDestination, source, ip, ref lastEntity, true);

			if (lastEntity.StartsWith(".Value"))
			{
               x.Value = valueToSet;
			}
            else if (lastEntity.StartsWith(".Name"))
			{
				if (!(valueToSet is string))
					throw new ArgumentException("Cannot set the Name of a node to something which is not a string literal");
                x.Name = valueToSet.ToString();
			}
            else if (lastEntity == "")
			{
				Node clone = (valueToSet as Node).Clone();
				x.ReplaceChildren(clone);
				x.Name = clone.Name;
				x.Value = clone.Value;
			}
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		// Helper for finding nodes
		private static Node GetNode(
			string expr, 
			Node source, 
			Node ip, 
			ref string lastEntity, 
			bool forcePath)
		{
			Node x = source;

            bool isInside = false;
            string bufferNodeName = null;
            lastEntity = null;

            for (int idx = 0; idx < expr.Length; idx++)
            {
                char tmp = expr[idx];
                if (isInside)
                {
					if (tmp == '[')
					{
						// Nested statement
						if (!string.IsNullOrEmpty (bufferNodeName))
							throw new ArgumentException("Don't understand: " + bufferNodeName);

						// Spooling forward to end of nested statement
						string entireSubStatement = "";
						int braceIndex = 0;
						for (; idx < expr.Length; idx++)
						{
							if (expr[idx] == '[')
								braceIndex += 1;
							else if (expr[idx] == ']')
								braceIndex -= 1;
							if (braceIndex == -1)
								break;
							entireSubStatement += expr[idx];
						}

						object innerVal = GetExpressionValue(entireSubStatement, source, ip, false);

						if (innerVal == null)
							throw new ArgumentException("subexpression failed, expression was; " + entireSubStatement);

						tmp = ']'; // to sucker into ending of logic
						bufferNodeName = innerVal.ToString();
					}
                    if (tmp == ']')
                    {
                        if (string.IsNullOrEmpty(bufferNodeName))
							throw new ArgumentException("Opps, empty node name/index ...");

                        lastEntity = "";

                        bool allNumber = true;
                        if (bufferNodeName == "..")
                        {
							// One up!
                            if (x.Parent == null)
								return null;
                            x = x.Parent;
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                        else if (bufferNodeName == "/")
                        {
                            x = x.RootNode();
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
						else if (bufferNodeName == "$")
						{
							x = ip.RootNode()["$"];
							isInside = false;
							continue;
						}
						else if (bufferNodeName == ".ip")
						{
							x = ip;
							bufferNodeName = "";
							isInside = false;
							continue;
						}
						else if (bufferNodeName == "@")
						{
							x = ip.Parent;
							bufferNodeName = "";
							isInside = false;
							continue;
						}
                        else if (bufferNodeName == ".")
                        {
							x = source;
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
						else if (bufferNodeName.Contains(":"))
						{
							int idxNo = int.Parse(bufferNodeName.Split(':')[1].TrimStart());
							int curNo = 0;
							int totIdx = 0;
							bool found = false;
							bufferNodeName = bufferNodeName.Split(':')[0].TrimEnd();

							foreach (Node idxNode in x)
							{
								if (idxNode.Name == bufferNodeName)
								{
									if (curNo++ == idxNo)
									{
										found = true;
										x = x[totIdx];
										break;
									}
								}
								totIdx += 1;
							}
							if (!found)
							{
								if (forcePath)
								{
									while (idxNo >= curNo++)
									{
										x.Add(new Node(bufferNodeName));
									}
									x = x[x.Count - 1];
								}
								else
									return null;
							}
							bufferNodeName = "";
							isInside = false;
							continue;
						}
                        else
                        {
                            foreach (char idxC in bufferNodeName)
                            {
                                if (("0123456789").IndexOf(idxC) == -1)
                                {
                                    allNumber = false;
                                    break;
                                }
                            }
                            if (allNumber)
                            {
                                int intIdx = int.Parse(bufferNodeName);
								if (x.Count > intIdx)
									x = x[intIdx];
								else if (forcePath)
								{
									while (x.Count <= intIdx)
									{
										x.Add(new Node("item"));
									}
									x = x[intIdx];
								}
								else
									return null;
                            }
                            else
                            {
								if (x.Contains(bufferNodeName))
									x = x[bufferNodeName];
								else if (forcePath)
								{
									x = x[bufferNodeName];
								}
								else
								{
									return null;
								}
                            }
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                    }
                    bufferNodeName += tmp;
                }
                else
                {
                    if (tmp == '[')
                    {
                        bufferNodeName = "";
                        isInside = true;
                        continue;
                    }
                    lastEntity += tmp;
                }
            }
			return x;
		}

		private static void Remove(string expression, Node source, Node ip)
		{
			string lastEntity = "";
			Node x = GetNode(expression, source, ip, ref lastEntity, false);

			if (x == null)
				return;

            if (lastEntity == ".Value")
				x.Value = null;
            else if (lastEntity == ".Name")
				throw new ArgumentException("cannot remove a name of a node");
            else if (lastEntity == "")
                x.UnTie ();
            else
                throw new ArgumentException("couldn't understand the last parts of your expression '" + lastEntity + "'");
		}
	}
}

