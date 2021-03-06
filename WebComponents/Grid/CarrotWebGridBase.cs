﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.WebPages;

/*
* CarrotCake CMS (MVC5)
* http://www.carrotware.com/
*
* Copyright 2015, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: August 2015
*/

namespace Carrotware.Web.UI.Components {

	public abstract class CarrotWebGridBase : IHtmlString {
		protected HtmlHelper _htmlHelper;
		protected SortParm _sortDir;

		protected void StandardInit(HtmlHelper htmlHelper, PagedDataBase dp) {
			_htmlHelper = htmlHelper;

			this.FooterOuterTag = "ul";
			this.FooterTag = "li";

			this.FieldIdPrefix = String.Empty;
			this.FieldNamePrefix = String.Empty;

			this.SortDescIndicator = "&nbsp;&#9660;";
			this.SortAscIndicator = "&nbsp;&#9650;";

			this.HtmlClientId = "tblDataTable";

			this.Columns = new List<ICarrotGridColumn>();

			this.UseDataPage = true;
			this.PageSizeExternal = false;

			this.PagedDataBase = dp;
		}

		protected PagedDataBase PagedDataBase { get; set; }

		public List<ICarrotGridColumn> Columns { get; protected set; }

		public Func<Object, HelperResult> EmptyDataTemplate { get; set; }

		public string HtmlClientId { get; set; }
		public string HtmlFormId { get; set; }
		public string SortDescIndicator { get; set; }
		public string SortAscIndicator { get; set; }
		protected string FieldIdPrefix { get; set; }
		protected string FieldNamePrefix { get; set; }

		public int RowNumber { get; set; }

		public bool UseDataPage { get; set; }
		public bool PageSizeExternal { get; set; }

		public string FooterOuterTag { get; set; }
		public object htmlFootAttrib { get; set; }

		public string FooterTag { get; set; }
		public object htmlFootSel { get; set; }
		public object htmlFootNotSel { get; set; }

		public void ConfigName(IHtmlString name) {
			this.FieldNamePrefix = name.ToString();

			if (String.IsNullOrEmpty(this.FieldNamePrefix)) {
				this.FieldNamePrefix = String.Empty;
			} else {
				this.FieldNamePrefix = String.Format("{0}.", this.FieldNamePrefix);
			}

			this.FieldIdPrefix = this.FieldNamePrefix.Replace(".", "_").Replace("]", "_").Replace("[", "_");
		}

		public object TableAttributes { get; set; }
		public object THeadAttributes { get; set; }
		public object TBodyAttributes { get; set; }

		protected IDictionary<string, object> InitAttrib(object htmlAttribs) {
			IDictionary<string, object> tblAttrib = (IDictionary<string, object>)new RouteValueDictionary();

			if (htmlAttribs != null) {
				tblAttrib = (IDictionary<string, object>)HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttribs);
			}

			return tblAttrib;
		}

		protected void FormHelper(Expression<Func<PagedDataBase, object>> property, StringBuilder sb) {
			PropertyInfo propInfo = this.PagedDataBase.PropInfoFromExpression<PagedDataBase>(property);
			string columnName = ReflectionUtilities.BuildProp(property);

			Object val = propInfo.GetValue(this.PagedDataBase, null);

			string fldName = String.Format("{0}{1}", this.FieldNamePrefix, columnName);
			string str = val == null ? String.Empty : val.ToString();

			sb.AppendLine(_htmlHelper.Hidden(fldName, str).ToString());
		}

		protected StringBuilder BuildHeadScript(StringBuilder sb) {
			string frm = "form:first";
			if (!String.IsNullOrEmpty(this.HtmlFormId)) {
				frm = String.Format("#{0}", this.HtmlFormId);
			}

			if (this.UseDataPage) {
				sb.AppendLine(String.Empty);
				sb.AppendLine("	<script type=\"text/javascript\">");
				sb.AppendLine("	function __clickHead(fld) {");
				sb.AppendLine(String.Format("		$('#{0}SortByNew').val(fld);", this.FieldIdPrefix));
				sb.AppendLine(String.Format("		$('{0}')[0].submit();", frm));
				sb.AppendLine("	}");
				sb.AppendLine(String.Empty);
				sb.AppendLine("	function __clickPage(nbr, fld) {");
				sb.AppendLine("		$('#' + fld).val(nbr);");
				sb.AppendLine("		$('#' + fld).focus();");
				sb.AppendLine(String.Format("		$('{0}')[0].submit();", frm));
				sb.AppendLine("	}");
				sb.AppendLine("	</script>");
				sb.AppendLine(String.Empty);

				FormHelper(x => x.OrderBy, sb);
				FormHelper(x => x.SortByNew, sb);
				if (!this.PageSizeExternal) {
					FormHelper(x => x.PageSize, sb);
				}
				FormHelper(x => x.TotalRecords, sb);
				FormHelper(x => x.MaxPage, sb);
				FormHelper(x => x.PageNumber, sb);
			}

			return sb;
		}

		protected StringBuilder BuildTableHeadRow(StringBuilder sb) {
			using (new WrappedItem(sb, "thead", this.THeadAttributes)) {
				using (new WrappedItem(sb, "tr", null)) {
					foreach (var col in this.Columns) {
						using (new WrappedItem(sb, "th", col.HeadAttributes)) {
							if (col is ICarrotGridColumnExt) {
								var colExt = (ICarrotGridColumnExt)col;
								if (colExt.Sortable && this.UseDataPage) {
									string js = String.Format("javascript:__clickHead('{0}')", colExt.ColumnName);

									IDictionary<string, object> tagAttrib = InitAttrib(colExt.HeadLinkAttributes);

									tagAttrib.Add("href", js);

									using (new WrappedItem(sb, "a", tagAttrib)) {
										sb.Append(col.HeaderText);

										if (_sortDir.SortField.ToUpper() == colExt.ColumnName.ToUpper()) {
											if (_sortDir.SortDirection.ToUpper() == "ASC") {
												sb.Append(this.SortAscIndicator);
											} else {
												sb.Append(this.SortDescIndicator);
											}
										}
									}
								} else {
									sb.Append(col.HeaderText);
								}
							} else {
								sb.Append(col.HeaderText);
							}
						}
					}
				}
			}

			return sb;
		}

		protected virtual IHtmlString CreateBody() {
			return new HtmlString(String.Empty);
		}

		public virtual void SetupFooter(string outer, object outerAttrib, string inner, object selAttrib, object noselAttrib) {
			this.FooterOuterTag = String.IsNullOrEmpty(outer) ? "ul" : outer;

			this.htmlFootAttrib = outerAttrib;

			this.FooterTag = String.IsNullOrEmpty(inner) ? "li" : inner;

			this.htmlFootSel = selAttrib;
			this.htmlFootNotSel = noselAttrib;
		}

		public virtual IHtmlString OutputFooter() {
			StringBuilder sb = new StringBuilder();

			if (this.PagedDataBase.TotalPages > 1) {
				using (new WrappedItem(sb, this.FooterOuterTag, this.htmlFootAttrib)) {
					foreach (var i in this.PagedDataBase.PageNumbers) {
						string clickFn = String.Format("javascript:__clickPage('{0}','{1}PageNumber')", i, this.FieldIdPrefix);

						using (new WrappedItem(_htmlHelper, sb, this.FooterTag, i, this.PagedDataBase.PageNumber, this.htmlFootSel, this.htmlFootNotSel)) {
							using (new WrappedItem(sb, "a", new { @href = clickFn })) {
								sb.Append(String.Format(" {0} ", i));
							}
						}
					}
				}
			}

			return new HtmlString(sb.ToString());
		}

		protected virtual IHtmlString EmptyTable() {
			this.PagedDataBase.TotalRecords = 0;
			this.PagedDataBase.PageNumber = 1;

			string cellContents = String.Empty;

			StringBuilder sb = new StringBuilder();

			FormHelper(x => x.OrderBy, sb);
			FormHelper(x => x.SortByNew, sb);
			if (!this.PageSizeExternal) {
				FormHelper(x => x.PageSize, sb);
			}
			FormHelper(x => x.TotalRecords, sb);
			FormHelper(x => x.MaxPage, sb);
			FormHelper(x => x.PageNumber, sb);

			if ((!this.PagedDataBase.HasData) && this.EmptyDataTemplate != null) {
				cellContents = (new HelperResult(writer => {
					this.EmptyDataTemplate(new Object()).WriteTo(writer);
				})).ToHtmlString();
			}

			sb.AppendLine(cellContents);

			return new HtmlString(sb.ToString());
		}

		public virtual IHtmlString OutputHtmlBody() {
			if (this.PagedDataBase.HasData) {
				return CreateBody();
			} else {
				return EmptyTable();
			}
		}

		public string ToHtmlString() {
			StringBuilder sb = new StringBuilder();
			//sb.AppendLine("<div>");
			sb.AppendLine(this.OutputHtmlBody().ToString());
			sb.AppendLine(this.OutputFooter().ToString());
			//sb.AppendLine("</div>");
			return sb.ToString();
		}

		public IHtmlString Write() {
			return new HtmlString(ToHtmlString());
		}
	}
}