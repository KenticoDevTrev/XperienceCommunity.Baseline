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

namespace Generic
{
	/// <summary>
	/// Represents a content item of type <see cref="Audio"/>.
	/// </summary>
	[RegisterContentTypeMapping(CONTENT_TYPE_NAME)]
	public partial class Audio : IContentItemFieldsSource
	{
		/// <summary>
		/// Code name of the content type.
		/// </summary>
		public const string CONTENT_TYPE_NAME = "Generic.Audio";


		/// <summary>
		/// Represents system properties for a content item.
		/// </summary>
		[SystemField]
		public ContentItemFields SystemFields { get; set; }


		/// <summary>
		/// AudioTitle.
		/// </summary>
		public string AudioTitle { get; set; }


		/// <summary>
		/// AudioDescription.
		/// </summary>
		public string AudioDescription { get; set; }


		/// <summary>
		/// AudioFile.
		/// </summary>
		public ContentItemAsset AudioFile { get; set; }


		/// <summary>
		/// AudioTranscript.
		/// </summary>
		public string AudioTranscript { get; set; }
	}
}