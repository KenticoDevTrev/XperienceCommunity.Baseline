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
	/// Represents a content item of type <see cref="Video"/>.
	/// </summary>
	[RegisterContentTypeMapping(CONTENT_TYPE_NAME)]
	public partial class Video : IContentItemFieldsSource
	{
		/// <summary>
		/// Code name of the content type.
		/// </summary>
		public const string CONTENT_TYPE_NAME = "Generic.Video";


		/// <summary>
		/// Represents system properties for a content item.
		/// </summary>
		[SystemField]
		public ContentItemFields SystemFields { get; set; }


		/// <summary>
		/// VideoTitle.
		/// </summary>
		public string VideoTitle { get; set; }


		/// <summary>
		/// VideoDescription.
		/// </summary>
		public string VideoDescription { get; set; }


		/// <summary>
		/// VideoFile.
		/// </summary>
		public ContentItemAsset VideoFile { get; set; }


        /// <summary>
        /// VideoTranscript.
        /// </summary>
        public string VideoTranscript { get; set; }
    }
}