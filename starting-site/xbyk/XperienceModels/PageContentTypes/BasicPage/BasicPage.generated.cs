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
using CMS.ContentEngine;
using CMS.Websites;
using CMS.MediaLibrary;

namespace Generic
{
	/// <summary>
	/// Represents a page of type <see cref="BasicPage"/>.
	/// </summary>
	[RegisterContentTypeMapping(CONTENT_TYPE_NAME)]
	public partial class BasicPage : IWebPageFieldsSource, IBaseMetadata, IBaseRedirect
	{
		/// <summary>
		/// Code name of the content type.
		/// </summary>
		public const string CONTENT_TYPE_NAME = "Generic.BasicPage";


		/// <summary>
		/// Represents system properties for a web page item.
		/// </summary>
		[SystemField]
		public WebPageFields SystemFields { get; set; }


		/// <summary>
		/// PageName.
		/// </summary>
		public string PageName { get; set; }


		/// <summary>
		/// MetaData_Title.
		/// </summary>
		public string MetaData_Title { get; set; }


		/// <summary>
		/// MetaData_Description.
		/// </summary>
		public string MetaData_Description { get; set; }


		/// <summary>
		/// MetaData_Keywords.
		/// </summary>
		public string MetaData_Keywords { get; set; }


		/// <summary>
		/// MetaData_ThumbnailSmall.
		/// </summary>
		public IEnumerable<AssetRelatedItem> MetaData_ThumbnailSmall { get; set; }


		/// <summary>
		/// MetaData_ThumbnailLarge.
		/// </summary>
		public IEnumerable<AssetRelatedItem> MetaData_ThumbnailLarge { get; set; }


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