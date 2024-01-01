﻿//--------------------------------------------------------------------------------------------------
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
using CMS;
using CMS.Base;
using CMS.Helpers;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Types.Generic;

namespace CMS.DocumentEngine.Types.Generic
{
	/// <summary>
	/// Represents a content item of type Navigation.
	/// </summary>
	public partial class Navigation : TreeNode
	{
		#region "Constants and variables"

		/// <summary>
		/// The name of the data class.
		/// </summary>
		public const string CLASS_NAME = "Generic.Navigation";


		/// <summary>
		/// The instance of the class that provides extended API for working with Navigation fields.
		/// </summary>
		private readonly NavigationFields mFields;

		#endregion


		#region "Properties"

		/// <summary>
		/// NavigationID.
		/// </summary>
		[DatabaseIDField]
		public int NavigationID
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("NavigationID"), 0);
			}
			set
			{
				SetValue("NavigationID", value);
			}
		}


		/// <summary>
		/// Name of the Node.
		/// </summary>
		[DatabaseField]
		public string NavigationName
		{
			get
			{
				return ValidationHelper.GetString(GetValue("NavigationName"), @"");
			}
			set
			{
				SetValue("NavigationName", value);
			}
		}


		/// <summary>
		/// Whether or not the Navigation Item should automatically build itself off of the selected page, or if you want to manually enter in the information.
		/// </summary>
		[DatabaseField]
		public int NavigationType
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("NavigationType"), 0);
			}
			set
			{
				SetValue("NavigationType", value);
			}
		}


		/// <summary>
		/// Can select a page, the navigation will be based on this page's Title and Url.
		/// </summary>
		[DatabaseField]
		public Guid NavigationPageNodeGuid
		{
			get
			{
				return ValidationHelper.GetGuid(GetValue("NavigationPageNodeGuid"), Guid.Empty);
			}
			set
			{
				SetValue("NavigationPageNodeGuid", value);
			}
		}


		/// <summary>
		/// Can be HTML (icons and stuff), but be sure to make it valid HTML if you do.
		/// </summary>
		[DatabaseField]
		public string NavigationLinkText
		{
			get
			{
				return ValidationHelper.GetString(GetValue("NavigationLinkText"), @"");
			}
			set
			{
				SetValue("NavigationLinkText", value);
			}
		}


		/// <summary>
		/// Nav Link.
		/// </summary>
		[DatabaseField]
		public string NavigationLinkUrl
		{
			get
			{
				return ValidationHelper.GetString(GetValue("NavigationLinkUrl"), @"");
			}
			set
			{
				SetValue("NavigationLinkUrl", value);
			}
		}


		/// <summary>
		/// Link Target.
		/// </summary>
		[DatabaseField]
		public string NavigationLinkTarget
		{
			get
			{
				return ValidationHelper.GetString(GetValue("NavigationLinkTarget"), @"_self");
			}
			set
			{
				SetValue("NavigationLinkTarget", value);
			}
		}


		/// <summary>
		/// If true, then the navigation will render the Page contents within the mega menu.  Typically mega menu pages will not have any child navigation elements.
		/// </summary>
		[DatabaseField]
		public bool NavigationIsMegaMenu
		{
			get
			{
				return ValidationHelper.GetBoolean(GetValue("NavigationIsMegaMenu"), false);
			}
			set
			{
				SetValue("NavigationIsMegaMenu", value);
			}
		}


		/// <summary>
		/// Descriptive text for on hover and for screen readers.
		/// </summary>
		[DatabaseField]
		public string NavigationLinkAlt
		{
			get
			{
				return ValidationHelper.GetString(GetValue("NavigationLinkAlt"), @"");
			}
			set
			{
				SetValue("NavigationLinkAlt", value);
			}
		}


		/// <summary>
		/// Optional On click for the link.
		/// </summary>
		[DatabaseField]
		public string NavigationLinkOnClick
		{
			get
			{
				return ValidationHelper.GetString(GetValue("NavigationLinkOnClick"), @"");
			}
			set
			{
				SetValue("NavigationLinkOnClick", value);
			}
		}


		/// <summary>
		/// Optional CSS Class applied to the navigation item.
		/// </summary>
		[DatabaseField]
		public string NavigationLinkCSS
		{
			get
			{
				return ValidationHelper.GetString(GetValue("NavigationLinkCSS"), @"");
			}
			set
			{
				SetValue("NavigationLinkCSS", value);
			}
		}


		/// <summary>
		/// If checked, allows you to define child items dynamically.
		/// </summary>
		[DatabaseField]
		public bool IsDynamic
		{
			get
			{
				return ValidationHelper.GetBoolean(GetValue("IsDynamic"), false);
			}
			set
			{
				SetValue("IsDynamic", value);
			}
		}


		/// <summary>
		/// Path.
		/// </summary>
		[DatabaseField]
		public string Path
		{
			get
			{
				return ValidationHelper.GetString(GetValue("Path"), @"");
			}
			set
			{
				SetValue("Path", value);
			}
		}


		/// <summary>
		/// Page Types.
		/// </summary>
		[DatabaseField]
		public string PageTypes
		{
			get
			{
				return ValidationHelper.GetString(GetValue("PageTypes"), @"");
			}
			set
			{
				SetValue("PageTypes", value);
			}
		}


		/// <summary>
		/// Choose which you would like your items to be ordered by.
		/// </summary>
		[DatabaseField]
		public string OrderBy
		{
			get
			{
				return ValidationHelper.GetString(GetValue("OrderBy"), @"");
			}
			set
			{
				SetValue("OrderBy", value);
			}
		}


		/// <summary>
		/// Where Condition.
		/// </summary>
		[DatabaseField]
		public string WhereCondition
		{
			get
			{
				return ValidationHelper.GetString(GetValue("WhereCondition"), @"");
			}
			set
			{
				SetValue("WhereCondition", value);
			}
		}


		/// <summary>
		/// The max NodeLevel this should go down.  1 = First children, 2 = Grandchildren, etc.
		/// </summary>
		[DatabaseField]
		public int MaxLevel
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("MaxLevel"), 0);
			}
			set
			{
				SetValue("MaxLevel", value);
			}
		}


		/// <summary>
		/// The top number of items to retrieve and display.
		/// </summary>
		[DatabaseField]
		public int TopNumber
		{
			get
			{
				return ValidationHelper.GetInteger(GetValue("TopNumber"), 0);
			}
			set
			{
				SetValue("TopNumber", value);
			}
		}


		/// <summary>
		/// Gets an object that provides extended API for working with Navigation fields.
		/// </summary>
		[RegisterProperty]
		public NavigationFields Fields
		{
			get
			{
				return mFields;
			}
		}


		/// <summary>
		/// Provides extended API for working with Navigation fields.
		/// </summary>
		[RegisterAllProperties]
		public partial class NavigationFields : AbstractHierarchicalObject<NavigationFields>
		{
			/// <summary>
			/// The content item of type Navigation that is a target of the extended API.
			/// </summary>
			private readonly Navigation mInstance;


			/// <summary>
			/// Initializes a new instance of the <see cref="NavigationFields" /> class with the specified content item of type Navigation.
			/// </summary>
			/// <param name="instance">The content item of type Navigation that is a target of the extended API.</param>
			public NavigationFields(Navigation instance)
			{
				mInstance = instance;
			}


			/// <summary>
			/// NavigationID.
			/// </summary>
			public int ID
			{
				get
				{
					return mInstance.NavigationID;
				}
				set
				{
					mInstance.NavigationID = value;
				}
			}


			/// <summary>
			/// Name of the Node.
			/// </summary>
			public string Name
			{
				get
				{
					return mInstance.NavigationName;
				}
				set
				{
					mInstance.NavigationName = value;
				}
			}


			/// <summary>
			/// Whether or not the Navigation Item should automatically build itself off of the selected page, or if you want to manually enter in the information.
			/// </summary>
			public int Type
			{
				get
				{
					return mInstance.NavigationType;
				}
				set
				{
					mInstance.NavigationType = value;
				}
			}


			/// <summary>
			/// Can select a page, the navigation will be based on this page's Title and Url.
			/// </summary>
			public Guid PageNodeGuid
			{
				get
				{
					return mInstance.NavigationPageNodeGuid;
				}
				set
				{
					mInstance.NavigationPageNodeGuid = value;
				}
			}


			/// <summary>
			/// Can be HTML (icons and stuff), but be sure to make it valid HTML if you do.
			/// </summary>
			public string LinkText
			{
				get
				{
					return mInstance.NavigationLinkText;
				}
				set
				{
					mInstance.NavigationLinkText = value;
				}
			}


			/// <summary>
			/// Nav Link.
			/// </summary>
			public string LinkUrl
			{
				get
				{
					return mInstance.NavigationLinkUrl;
				}
				set
				{
					mInstance.NavigationLinkUrl = value;
				}
			}


			/// <summary>
			/// Link Target.
			/// </summary>
			public string LinkTarget
			{
				get
				{
					return mInstance.NavigationLinkTarget;
				}
				set
				{
					mInstance.NavigationLinkTarget = value;
				}
			}


			/// <summary>
			/// If true, then the navigation will render the Page contents within the mega menu.  Typically mega menu pages will not have any child navigation elements.
			/// </summary>
			public bool IsMegaMenu
			{
				get
				{
					return mInstance.NavigationIsMegaMenu;
				}
				set
				{
					mInstance.NavigationIsMegaMenu = value;
				}
			}


			/// <summary>
			/// Descriptive text for on hover and for screen readers.
			/// </summary>
			public string LinkAlt
			{
				get
				{
					return mInstance.NavigationLinkAlt;
				}
				set
				{
					mInstance.NavigationLinkAlt = value;
				}
			}


			/// <summary>
			/// Optional On click for the link.
			/// </summary>
			public string LinkOnClick
			{
				get
				{
					return mInstance.NavigationLinkOnClick;
				}
				set
				{
					mInstance.NavigationLinkOnClick = value;
				}
			}


			/// <summary>
			/// Optional CSS Class applied to the navigation item.
			/// </summary>
			public string LinkCSS
			{
				get
				{
					return mInstance.NavigationLinkCSS;
				}
				set
				{
					mInstance.NavigationLinkCSS = value;
				}
			}


			/// <summary>
			/// If checked, allows you to define child items dynamically.
			/// </summary>
			public bool IsDynamic
			{
				get
				{
					return mInstance.IsDynamic;
				}
				set
				{
					mInstance.IsDynamic = value;
				}
			}


			/// <summary>
			/// Path.
			/// </summary>
			public string Path
			{
				get
				{
					return mInstance.Path;
				}
				set
				{
					mInstance.Path = value;
				}
			}


			/// <summary>
			/// Page Types.
			/// </summary>
			public string PageTypes
			{
				get
				{
					return mInstance.PageTypes;
				}
				set
				{
					mInstance.PageTypes = value;
				}
			}


			/// <summary>
			/// Choose which you would like your items to be ordered by.
			/// </summary>
			public string OrderBy
			{
				get
				{
					return mInstance.OrderBy;
				}
				set
				{
					mInstance.OrderBy = value;
				}
			}


			/// <summary>
			/// Where Condition.
			/// </summary>
			public string WhereCondition
			{
				get
				{
					return mInstance.WhereCondition;
				}
				set
				{
					mInstance.WhereCondition = value;
				}
			}


			/// <summary>
			/// The max NodeLevel this should go down.  1 = First children, 2 = Grandchildren, etc.
			/// </summary>
			public int MaxLevel
			{
				get
				{
					return mInstance.MaxLevel;
				}
				set
				{
					mInstance.MaxLevel = value;
				}
			}


			/// <summary>
			/// The top number of items to retrieve and display.
			/// </summary>
			public int TopNumber
			{
				get
				{
					return mInstance.TopNumber;
				}
				set
				{
					mInstance.TopNumber = value;
				}
			}
		}

		#endregion


		#region "Constructors"

		/// <summary>
		/// Initializes a new instance of the <see cref="Navigation" /> class.
		/// </summary>
		public Navigation() : base(CLASS_NAME)
		{
			mFields = new NavigationFields(this);
		}

		#endregion
	}
}