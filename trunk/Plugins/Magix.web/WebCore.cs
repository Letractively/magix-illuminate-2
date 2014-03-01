/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Configuration;
using System.Web;
using Magix.Core;
using Magix.UX.Builder;

namespace Magix.execute
{
	/**
	 * web helper core
	 */
	public class WebCore : ActiveController
	{
		/**
		 * return the given http get parameter
		 */
		[ActiveEvent(Name = "magix.web.get")]
		public void magix_web_get(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will return the given
get http parameter as [value], if existing.&nbsp;&nbsp;not thread safe";
				e.Params["magix.web.get"].Value = "some-get-parameter";
				return;
			}

			string par = e.Params.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("[magix.web.get] needs a value defining which get parameter to fetch");

			if (HttpContext.Current.Request.Params[par] != null)
				e.Params["value"].Value = HttpContext.Current.Request.Params[par];
		}

		/**
		 * sets a session object
		 */
		[ActiveEvent(Name = "magix.web.set-session")]
		public void magix_web_set_session(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will create or overwrite
an existing session variable with name of value of [magix.web.set-session] with node 
hierarchy from [value].&nbsp;&nbsp;
if you pass in no [value], any existing session object with given key will be removed.&nbsp;&nbsp;
not thread safe";
				e.Params["magix.web.set-session"].Value = "some_key";
				e.Params["magix.web.set-session"]["value"].Value = "something to store into session";
				return;
			}

			string id = e.Params.Get<string>();

			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("[magix.web.set-session] needs a value to know which session object to fetch");

			if (!e.Params.Contains("value"))
			{
				// removal of existing session object
				Page.Session.Remove(id);
			}
			else
			{
				// adding or overwiting existing value
				Node value = e.Params["value"].Clone();
				Page.Session[id] = value;
			}
		}

		/**
		 * returns an existing session object
		 */
		[ActiveEvent(Name = "magix.web.get-session")]
		public void magix_web_get_session(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will return
an existing session variable named through the value of [magix.web.get-session] 
as node hierarchy as child nodes, if existing.&nbsp;&nbsp;
not thread safe";
				e.Params["magix.web.get-session"].Value = "some_key";
				return;
			}

			string id = e.Params.Get<string>();

			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("need a value for [magix.web.get-session]");

			if (Page.Session[id] != null &&
			    Page.Session[id] is Node)
				e.Params.Add(Page.Session[id] as Node);
		}

		/**
		 * redirects the client/browser
		 */
		[ActiveEvent(Name = "magix.web.redirect")]
		public void magix_web_redirect(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will redirect the browser 
to the value of [magix.web.redirect] node.&nbsp;&nbsp;
you can use the ~ to reference the base url of the website, and in 
such a way create relative redirecting urls.&nbsp;&nbsp;
not thread safe";
				e.Params["magix.web.redirect"].Value = "http://google.com";
				return;
			}

			string url = e.Params.Get<string>();

			if (string.IsNullOrEmpty(url))
				throw new ArgumentException("need url as value for [magix.web.redirect] to function");

			if (url.Contains("~"))
			{
				url = url.Replace("~", GetApplicationBaseUrl());
			}

			Magix.UX.AjaxManager.Instance.Redirect(url);
		}

		/**
		 * sets a cookie
		 */
		[ActiveEvent(Name = "magix.web.set-cookie")]
		public void magix_web_set_cookie(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will create or overwrite
and existing http cookie.&nbsp;&nbsp;if no expiration date 
is used, a default of three years from now will be used.&nbsp;&nbsp;
if no [value] is passed in, any existing cookies with given name will 
be removed.&nbsp;&nbsp;not thread safe";
				e.Params["magix.web.set-cookie"].Value = "some-cookie-name";
				e.Params["magix.web.set-cookie"]["value"].Value = "something to store into cookie";
				e.Params["magix.web.set-cookie"]["expires"].Value = DateTime.Now.AddYears (3);
				return;
			}

			string par = e.Params.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("[magix.web.set-cookie] needs a value to know which cookie to set");

			string value = null;

			if (e.Params.Contains("value"))
				value = e.Params["value"].Get<string>();

			if (value == null)
			{
				HttpContext.Current.Response.Cookies.Remove(par);
			}
			else
			{
				HttpCookie cookie = new HttpCookie(par, value);
				cookie.HttpOnly = true;
				DateTime expires = DateTime.Now.AddYears(3);
				if (e.Params.Contains("expires"))
					expires = e.Params["expires"].Get<DateTime>();
				cookie.Expires = expires;

				HttpContext.Current.Response.SetCookie(cookie);
			}
		}

		/**
		 * returns an existing cookie
		 */
		[ActiveEvent(Name = "magix.web.get-cookie")]
		public void magix_web_get_cookie(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"returns the given
http cookie parameter as [value] node.&nbsp;&nbsp;not thread safe";
				e.Params["magix.web.get-cookie"].Value = "some-cookie-name";
				return;
			}

			string par = e.Params.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("[magix.web.get-cookie] nneds a value to know which cookie to fetch");

			if (HttpContext.Current.Request.Cookies.Get(par) != null)
				e.Params["value"].Value = HttpContext.Current.Request.Cookies[par].Value;
		}
		
		/**
		 * returns a web.config setting
		 */
		[ActiveEvent(Name = "magix.web.get-config-setting")]
		public static void magix_web_get_config_setting(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"returns the given
web.config setting as [value] node.&nbsp;&nbsp;thread safe";
				e.Params["magix.web.get-config-setting"].Value = "some-setting-name";
				return;
			}

			string par = e.Params.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("you must tell me which web.config setting you wish to retrieve as value to [magix.web.get-config-setting]");

			string val = ConfigurationManager.AppSettings[par];

			if (!string.IsNullOrEmpty(val))
				e.Params["value"].Value = val;
		}
	}
}

