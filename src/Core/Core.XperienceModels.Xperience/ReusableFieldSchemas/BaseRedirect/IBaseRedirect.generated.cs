//--------------------------------------------------------------------------------------------------
// <auto-generated>
//
//     This code was generated by code generator tool.
//
//     To customize the code use your own partial class. For more info about how to use and customize
//     the generated code see the documentation at https://docs.xperience.io/.
//
// </auto-generated>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using CMS.Websites;

namespace Generic
{
	/// <summary>
	/// Defines a contract for content types with the <see cref="IBaseRedirect"/> reusable schema assigned.
	/// </summary>
	public interface IBaseRedirect : IWebPageFieldsSource
	{
		/// <summary>
		/// Code name of the reusable field schema.
		/// </summary>
		public const string REUSABLE_FIELD_SCHEMA_NAME = "BaseRedirect";


		/// <summary>
		/// PageRedirectionType.
		/// </summary>
		public string PageRedirectionType { get; set; }


		/// <summary>
		/// PageInternalRedirectPage.
		/// </summary>
		public IEnumerable<WebPageRelatedItem> PageInternalRedirectPage { get; set; }


		/// <summary>
		/// PageExternalRedirectURL.
		/// </summary>
		public string PageExternalRedirectURL { get; set; }


		/// <summary>
		/// PageFirstChildClassName.
		/// </summary>
		public string PageFirstChildClassName { get; set; }


		/// <summary>
		/// PageUsePermanentRedirects.
		/// </summary>
		public bool PageUsePermanentRedirects { get; set; }
	}
}