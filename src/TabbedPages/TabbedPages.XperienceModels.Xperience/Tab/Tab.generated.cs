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

namespace Generic
{
	/// <summary>
	/// Represents a page of type <see cref="Tab"/>.
	/// </summary>
	[RegisterContentTypeMapping(CONTENT_TYPE_NAME)]
	public partial class Tab : IWebPageFieldsSource, IXperienceCommunityMemberPermissionConfiguration
	{
		/// <summary>
		/// Code name of the content type.
		/// </summary>
		public const string CONTENT_TYPE_NAME = "Generic.Tab";


		/// <summary>
		/// Represents system properties for a web page item.
		/// </summary>
		[SystemField]
		public WebPageFields SystemFields { get; set; }


		/// <summary>
		/// TabName.
		/// </summary>
		public string TabName { get; set; }


		/// <summary>
		/// MemberPermissionOverride.
		/// </summary>
		public bool MemberPermissionOverride { get; set; }


		/// <summary>
		/// MemberPermissionIsSecure.
		/// </summary>
		public bool MemberPermissionIsSecure { get; set; }


		/// <summary>
		/// MemberPermissionRoleTags.
		/// </summary>
		public IEnumerable<TagReference> MemberPermissionRoleTags { get; set; }
	}
}