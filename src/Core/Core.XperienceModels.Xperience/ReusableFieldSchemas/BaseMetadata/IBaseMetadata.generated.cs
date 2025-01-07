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

using CMS.Websites;
using System;
using System.Collections.Generic;

namespace Generic
{
	/// <summary>
	/// Defines a contract for content types with the <see cref="IBaseMetadata"/> reusable schema assigned.
	/// </summary>
	public interface IBaseMetadata : IWebPageFieldsSource
	{
		/// <summary>
		/// Code name of the reusable field schema.
		/// </summary>
		public const string REUSABLE_FIELD_SCHEMA_NAME = "Base.Metadata";


		/// <summary>
		/// MetaData_PageName.
		/// </summary>
		public string MetaData_PageName { get; set; }


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
		/// MetaData_NoIndex.
		/// </summary>
		public bool MetaData_NoIndex { get; set; }


		/// <summary>
		/// MetaData_OGImage.
		/// </summary>
		public IEnumerable<IGenericHasImage> MetaData_OGImage { get; set; }
	}
}