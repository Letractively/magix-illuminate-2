/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
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
		 * Takes an Expression, which it compares for true or false. This expression
		 * can take a single argument, e.g. [Data][Tmp], at which case it will check
		 * for the existence of that Node, and return true if exists. It can prefix
		 * a single argument with a '!', to return true if NOT exists. It can have two 
		 * arguments, where the right-hand side might be either another Expression
		 * path, or a constant, such as e.g. if=>[Data][Tmp].Value = thomas - or
		 * if=>[Data][Tmp].Name != [Data].Value. Comparison operators it implements
		 * are '=', '<', '>', '<=', '>=' and '!='. You can use either one '=' or '=='
		 * for comparing two values
		 */
		public static bool IsTrue (string expr, Node ip, Node dp)
		{
			List<string> tokens = TokenizeExpression(expr);

			if (tokens.Count == 1)
			{
				return ExpressionExist (tokens[0], dp, ip);
			}
			else if (tokens.Count == 2)
			{
				// Only ![Something] ...
				if (tokens[0] != "!")
					throw new ArgumentException("Didn't understand '" + expr + "'");
				return !ExpressionExist (tokens[1], dp, ip);
			}
			else if (tokens.Count == 3)
			{
				object lhs = GetExpressionValue(tokens[0], dp, ip);
				string comparer = tokens[1];
				object rhs = GetExpressionValue(tokens[2], dp, ip);

				if (lhs == null || rhs == null)
					return false;

				if (lhs.GetType () != rhs.GetType ())
				{
					switch (lhs.GetType ().FullName)
					{
					case "System.Int32":
						rhs = int.Parse (rhs.ToString ());
						break;
					case "System.Boolean":
						rhs = bool.Parse (rhs.ToString ());
						break;
					case "System.DateTime":
						rhs = DateTime.ParseExact(rhs.ToString (), "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
						break;
					case "System.Decimal":
						rhs = decimal.Parse (rhs.ToString (), CultureInfo.InvariantCulture);
						break;
					default:
						throw new ArgumentException("Don't know how to compare '" + expr + "' since the types of the expressions are incompatible");
					}
				}

				// Actual comparison, now our types are hopefully identical, and we can perform actual comparison
				switch (comparer)
				{
				case "!=":
					return !lhs.Equals (rhs);
				case "<=":
					switch (lhs.GetType ().FullName)
					{
					case "System.Boolean":
						return ((bool)lhs) == false;
					case "System.DateTime":
						return ((DateTime)lhs) <= ((DateTime)rhs);
					case "System.Decimal":
						return ((decimal)lhs) <= ((decimal)rhs);
					case "System.Int32":
						return ((int)lhs) <= ((int)rhs);
					case "System.String":
						return ((string)lhs).CompareTo(rhs) <= 0;
					default:
						throw new ArgumentException("Don't know how to compare '" + expr + "' since types don't match");
					}
				case ">=":
					switch (lhs.GetType ().FullName)
					{
					case "System.Boolean":
						return ((bool)lhs) == true;
					case "System.DateTime":
						return ((DateTime)lhs) >= ((DateTime)rhs);
					case "System.Decimal":
						return ((decimal)lhs) >= ((decimal)rhs);
					case "System.Int32":
						return ((int)lhs) >= ((int)rhs);
					case "System.String":
						return ((string)lhs).CompareTo(rhs) >= 0;
					default:
						throw new ArgumentException("Don't know how to compare '" + expr + "' since types don't match");
					}
				case "<":
					switch (lhs.GetType ().FullName)
					{
					case "System.Boolean":
						return ((bool)lhs) == false && ((bool)rhs) == true;
					case "System.DateTime":
						return ((DateTime)lhs) < ((DateTime)rhs);
					case "System.Decimal":
						return ((decimal)lhs) < ((decimal)rhs);
					case "System.Int32":
						return ((int)lhs) < ((int)rhs);
					case "System.String":
						return ((string)lhs).CompareTo(rhs) < 0;
					default:
						throw new ArgumentException("Don't know how to compare '" + expr + "' since types don't match");
					}
				case ">":
					switch (lhs.GetType ().FullName)
					{
					case "System.Boolean":
						return ((bool)lhs) == true && ((bool)rhs) == false;
					case "System.DateTime":
						return ((DateTime)lhs) > ((DateTime)rhs);
					case "System.Decimal":
						return ((decimal)lhs) > ((decimal)rhs);
					case "System.Int32":
						return ((int)lhs) > ((int)rhs);
					case "System.String":
						return ((string)lhs).CompareTo(rhs) > 0;
					default:
						throw new ArgumentException("Don't know how to compare '" + expr + "' since types don't match");
					}
				case "=":
					switch (lhs.GetType ().FullName)
					{
					case "System.Boolean":
						return ((bool)lhs) == ((bool)rhs);
					case "System.DateTime":
						return ((DateTime)lhs) == ((DateTime)rhs);
					case "System.Decimal":
						return ((decimal)lhs) == ((decimal)rhs);
					case "System.Int32":
						return ((int)lhs) == ((int)rhs);
					case "System.String":
						return ((string)lhs) == ((string)rhs);
					default:
						throw new ArgumentException("Don't know how to compare '" + expr + "' since types don't match");
					}
				default:
					throw new ArgumentException("Don't understand the expression '" + expr + "'");
				}
			}
			else
				throw new ArgumentException("Didn't understand '" + expr + "'");
		}

		// TODO: Implement "strings" parsing for complex strings, such that e.g. "[Data].Value"
		// becomes a string literal, and not an expression
		/**
		 * Sets the given exprDestination to the valuer of exprSource. If
		 * exprSource starts with a '[', it is expected to be a reference to another
		 * expression, else it will be assumed to be a static value
		 */
		public static void SetNodeValue (
			string exprDestination, 
			string exprSource, 
			Node source, 
			Node ip)
		{
			object valueToSet = GetExpressionValue (exprSource, source, ip);

			if (valueToSet == null)
			{
				Remove (exprDestination, source, ip);
				return;
			}

			string lastEntity = "";
			Node x = GetNode (exprDestination, source, ip, ref lastEntity, true);

            if (lastEntity == ".Value")
			{
               x.Value = valueToSet;
			}
            else if (lastEntity == ".Name")
			{
				if (!(valueToSet is string))
					throw new ArgumentException("Cannot set the Name of a node to something which is not a string literal");
                x.Name = valueToSet.ToString ();
			}
            else if (lastEntity == "")
			{
				Node clone = (valueToSet as Node).Clone ();
				x.ReplaceChildren(clone);
				x.Name = clone.Name;
				x.Value = clone.Value;
			}
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		/**
		 * Returns the value of the given expression, which might return a string, 
		 * list of nodes, or any other object your node tree might contain
		 */
        public static object GetExpressionValue(string expression, Node source, Node ip)
        {
			if (expression == null)
				return null;

			if (!expression.TrimStart ().StartsWith ("["))
				return expression;

			string lastEntity = "";
			Node x = GetNode(expression, source, ip, ref lastEntity, false);

			if (x == null)
				return null;

            if (lastEntity == ".Value")
                return x.Value;
            else if (lastEntity == ".Name")
                return x.Name;
            else if (lastEntity == ".Count")
                return x.Count;
            else if (lastEntity == "")
                return x;
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
        }

		// Helper for above method ...
		private static List<string> TokenizeExpression (string expr)
		{
			List<string> ret = new List<string>();

			string buffer = "";
			int insides = 0;
			for (int idx = 0; idx < expr.Length; idx++)
			{
				if (expr[idx] == '[')
				{
					buffer += expr[idx];
					insides += 1;
				}
				else if (expr[idx] == ']')
				{
					buffer += expr[idx];
					insides -= 1;
				}
				else if (insides == 0 && expr[idx] == '!')
				{
					if (!string.IsNullOrEmpty (buffer))
					{
						ret.Add (buffer);
						buffer = "";
					}
					if (expr[idx + 1] == '=')
					{
						idx += 1;
						ret.Add ("!=");
					}
					else
					{
						ret.Add ("!");
					}
				}
				else if (insides == 0 && expr[idx] == '<')
				{
					if (!string.IsNullOrEmpty (buffer))
					{
						ret.Add (buffer);
						buffer = "";
					}
					if (expr[idx + 1] == '=')
					{
						idx += 1;
						ret.Add ("<=");
					}
					else
					{
						ret.Add ("<");
					}
				}
				else if (insides == 0 && expr[idx] == '>')
				{
					if (!string.IsNullOrEmpty (buffer))
					{
						ret.Add (buffer);
						buffer = "";
					}
					if (expr[idx + 1] == '=')
					{
						idx += 1;
						ret.Add (">=");
					}
					else
					{
						ret.Add (">");
					}
				}
				else if (insides == 0 && expr[idx] == '=')
				{
					if (!string.IsNullOrEmpty (buffer))
					{
						ret.Add (buffer);
						buffer = "";
					}
					if (expr[idx + 1] == '=')
					{
						idx += 1;
					}
					ret.Add ("=");
				}
				else
				{
					buffer += expr[idx];
				}
			}
			if (!string.IsNullOrEmpty (buffer))
				ret.Add (buffer);
			return ret;
		}

		// Helper for finding nodes
		private static Node GetNode (
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

						string subValue = null;
						string lastSubEntity = "";
						Node subNode = GetNode (entireSubStatement, source, ip, ref lastSubEntity, forcePath);
						if (lastSubEntity == ".Value")
							subValue = subNode.Value.ToString ();
						else if (lastSubEntity == ".Name")
							subValue = subNode.Name;
						else if (lastSubEntity == ".Count")
							subValue = subNode.Count.ToString ();
						else if (lastSubEntity == "")
							throw new ArgumentException("Sub expressions cannot return node lists, but only Value and Name");
						else
							throw new ArgumentException("Don't know how to parse: " + lastSubEntity);
						bufferNodeName = subValue;
						bool allNumber = true;
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
									x.Add (new Node("item"));
								}
								x = x[intIdx];
							}
							else
								return null;
                        }
                        else
                        {
							if (x.Contains (bufferNodeName))
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
                        else if (bufferNodeName == ".ip")
                        {
                            x = ip;
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                        else if (bufferNodeName == ".")
                        {
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
								else
									return null;
                            }
                            else
                            {
								if (x.Contains (bufferNodeName))
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

		private static void Remove (string expression, Node source, Node ip)
		{
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity, false);

			if (x == null)
				throw new ArgumentException("Cannot remove a none existing Node");

            if (lastEntity == ".Value")
				x.Value = null;
            else if (lastEntity == ".Name")
				throw new ArgumentException("Cannot remove a Name of a node");
            else if (lastEntity == "")
                x.UnTie ();
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		private static bool ExpressionExist (string expression, Node source, Node ip)
		{
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity, false);

			if (x == null)
				return false;

            if (lastEntity == ".Value")
                return x.Value != null;
            else if (lastEntity == "")
                return true;
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}
	}
}

