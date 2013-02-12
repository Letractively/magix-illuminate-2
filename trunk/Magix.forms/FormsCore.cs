/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Level3: Controller that makes it possible to create new 
	 * dynamically created forms
	 */
	[ActiveController]
	public class FormsCore : ActiveController
	{
		/**
		 * Will create and instantiate a newly created dynamic form
		 */
		[ActiveEvent(Name = "magix.forms.create-form")]
		public void magix_forms_create_form (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Forms"]["Form1"]["controls"]["Button"].Value = "btn";
				e.Params["Forms"]["Form1"]["controls"]["Button"]["Text"].Value = "Hello World2!!";
				e.Params["container"].Value = "content2";
				e.Params["form-id"].Value = "unique-identification-of-your-form";
				e.Params["controls"]["Button"].Value = "btn";
				e.Params["controls"]["Button"]["Text"].Value = "Hello World!";
				e.Params["context"].Value = "[Forms][Form1]";
				e.Params["inspect"].Value = @"Creates a dynamic form
and loading it into the ""container"" viewport container. ""form-id""
must be a uniquely identifiable id for later use. Either ""controls""
contains the controls themselves, such as ""Button"" nodes, etc, 
or the ""context"" path contains a Value expression that points to 
a place in the data hierarchy where the code for the form is.";
				return;
			}
			Node formContent = e.Params;
			if (e.Params.Contains ("context"))
				formContent = Expressions.GetExpressionValue (
					e.Params["context"].Get<string>(), 
					e.Params, 
					e.Params) as Node;
			LoadModule (
				"Magix.forms.DynamicForm", 
				e.Params["container"].Get<string>(), 
				formContent.Clone ());
		}
	}
}

