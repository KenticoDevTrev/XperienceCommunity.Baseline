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

namespace Testing
{
	/// <summary>
	/// Represents a page of type <see cref="WebPage"/>.
	/// </summary>
	[RegisterContentTypeMapping(CONTENT_TYPE_NAME)]
	public partial class WebPage : IWebPageFieldsSource
	{
		/// <summary>
		/// Code name of the content type.
		/// </summary>
		public const string CONTENT_TYPE_NAME = "Testing.WebPage";


		/// <summary>
		/// Represents system properties for a web page item.
		/// </summary>
		[SystemField]
		public WebPageFields SystemFields { get; set; }


		/// <summary>
		/// TestingContentItem.
		/// </summary>
		public IEnumerable<WebPageContentItem> TestingContentItem { get; set; }


		/// <summary>
		/// RelatedPages.
		/// </summary>
		public IEnumerable<WebPageRelatedItem> RelatedPages { get; set; }


		/// <summary>
		/// TaxonomyTest.
		/// </summary>
		public IEnumerable<TagReference> TaxonomyTest { get; set; }


		/// <summary>
		/// TestObjectNames.
		/// </summary>
		public IEnumerable<string> TestObjectNames { get; set; }


		/// <summary>
		/// TestGlobalIdentifiers.
		/// </summary>
		public IEnumerable<Guid> TestGlobalIdentifiers { get; set; }


		/// <summary>
		/// TestLanguageAgnosticValue.
		/// </summary>
		public string TestLanguageAgnosticValue { get; set; }
	}
}