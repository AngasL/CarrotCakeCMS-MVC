﻿using Carrotware.CMS.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.CMS.UI.Components {

	public class ControlUtilities {

		private static Page WebPage {
			get {
				if (_Page == null) {
					_Page = new Page();
					_Page.AppRelativeVirtualPath = "~/";
				}
				return _Page;
			}
		}

		private static Page _Page;

		public static string GetWebResourceUrl(Type type, string resource) {
			string sPath = String.Empty;

			try {
				sPath = WebPage.ClientScript.GetWebResourceUrl(type, resource);
				sPath = HttpUtility.HtmlEncode(sPath);
			} catch { }

			return sPath;
		}

		public static string GetWebResourceUrl(string resource) {
			return GetWebResourceUrl(typeof(ControlUtilities), resource);
		}

		public static string GetManifestResourceStream(string sResouceName) {
			string sReturn = null;

			Assembly _assembly = Assembly.GetExecutingAssembly();
			using (StreamReader oTextStream = new StreamReader(_assembly.GetManifestResourceStream(sResouceName))) {
				sReturn = oTextStream.ReadToEnd();
			}

			return sReturn;
		}

		public static SiteNav IdentifyLinkAsInactive(SiteNav nav) {
			return CMSConfigHelper.IdentifyLinkAsInactive(nav);
		}

		public static List<SiteNav> GetPageNavTree() {
			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				return navHelper.GetPageCrumbNavigation(SiteData.CurrentSiteID, SiteData.AlternateCurrentScriptName, !SecurityData.IsAuthEditor);
			}
		}

		public static SiteNav GetParentPage() {
			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				SiteNav pageNav = navHelper.GetParentPageNavigation(SiteData.CurrentSiteID, SiteData.AlternateCurrentScriptName);

				//assign bogus page name for comp purposes
				if (pageNav == null || pageNav.SiteID == Guid.Empty) {
					pageNav = new SiteNav();
					pageNav.Root_ContentID = Guid.Empty;
					pageNav.FileName = "/##/";
					pageNav.TemplateFile = "/##/";
				}

				return pageNav;
			}
		}

		public static SiteNav GetCurrentPage() {
			using (ISiteNavHelper navHelper = SiteNavFactory.GetSiteNavHelper()) {
				SiteNav pageNav = navHelper.FindByFilename(SiteData.CurrentSiteID, SiteData.AlternateCurrentScriptName);
				//assign bogus page name for comp purposes
				if (pageNav == null) {
					pageNav = new SiteNav();
					pageNav.Root_ContentID = Guid.Empty;
					pageNav.FileName = "/##/##/";
					pageNav.TemplateFile = "/##/##/";
				}

				pageNav.SiteID = SiteData.CurrentSiteID;

				return pageNav;
			}
		}

		public static string GetParentPageName() {
			SiteNav nav = ControlUtilities.GetParentPage();

			return nav.FileName.ToLower();
		}

		public static bool AreFilenamesSame(string sParm1, string sParm2) {
			if (sParm1 == null || sParm2 == null) {
				return false;
			}

			return (sParm1.ToLower() == sParm2.ToLower()) ? true : false;
		}
	}

	//===========================

	public class WrapperForHtmlHelper<T, U> : IViewDataContainer {
		public ViewDataDictionary ViewData { get; set; }

		public WrapperForHtmlHelper(T type, ViewDataDictionary<U> viewdata) {
			this.ViewData = new ViewDataDictionary<T>(type);

			foreach (var item in viewdata.ModelState) {
				if (!this.ViewData.ModelState.Keys.Contains(item.Key)) {
					this.ViewData.ModelState.Add(item);
				}
			}
		}
	}

	public class WrapperForHtmlHelper<T> : IViewDataContainer {
		public ViewDataDictionary ViewData { get; set; }

		public WrapperForHtmlHelper(T type) {
			this.ViewData = new ViewDataDictionary<T>(type);
		}

		public WrapperForHtmlHelper(T type, ViewDataDictionary viewdata) {
			this.ViewData = new ViewDataDictionary<T>(type);

			foreach (var item in viewdata.ModelState) {
				if (!this.ViewData.ModelState.Keys.Contains(item.Key)) {
					this.ViewData.ModelState.Add(item);
				}
			}
		}
	}
}