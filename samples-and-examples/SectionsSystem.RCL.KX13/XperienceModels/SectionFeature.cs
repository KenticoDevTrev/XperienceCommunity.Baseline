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
using CMS.DocumentEngine.Types.SectionsSystem;

[assembly: RegisterDocumentType(SectionFeature.CLASS_NAME, typeof(SectionFeature))]

namespace CMS.DocumentEngine.Types.SectionsSystem
{
    /// <summary>
    /// Represents a content item of type SectionFeature.
    /// </summary>
    public partial class SectionFeature : TreeNode
    {
        #region "Constants and variables"

        /// <summary>
        /// The name of the data class.
        /// </summary>
        public const string CLASS_NAME = "SectionsSystem.SectionFeature";


        /// <summary>
        /// The instance of the class that provides extended API for working with SectionFeature fields.
        /// </summary>
        private readonly SectionFeatureFields mFields;

        #endregion


        #region "Properties"

        /// <summary>
        /// SectionFeatureID.
        /// </summary>
        [DatabaseIDField]
        public int SectionFeatureID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("SectionFeatureID"), 0);
            }
            set
            {
                SetValue("SectionFeatureID", value);
            }
        }


        /// <summary>
        /// Displayed above the section.
        /// </summary>
        [DatabaseField]
        public string FeatureHeading
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FeatureHeading"), @"");
            }
            set
            {
                SetValue("FeatureHeading", value);
            }
        }


        /// <summary>
        /// Displayed above the entire section.
        /// </summary>
        [DatabaseField]
        public string FeatureSubHeading
        {
            get
            {
                return ValidationHelper.GetString(GetValue("FeatureSubHeading"), @"");
            }
            set
            {
                SetValue("FeatureSubHeading", value);
            }
        }


        /// <summary>
        /// Is Section.
        /// </summary>
        [DatabaseField]
        public bool IsSection
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("IsSection"), true);
            }
            set
            {
                SetValue("IsSection", value);
            }
        }


        /// <summary>
        /// How the section should render.
        /// 
        /// Default = No styling.
        /// 
        /// Color = Solid color (based on Theme)
        /// 
        /// Image = Image background
        /// 
        /// Parallax Image = Image background with parallax effect
        /// 
        /// Video = Video background.
        /// </summary>
        public string SectionStyleType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionStyleType"), @"Default");
            }
            set
            {
                SetValue("SectionStyleType", value);
            }
        }


        /// <summary>
        /// Classes to add to the section.
        /// </summary>
        [DatabaseField]
        public string SectionStyleAdditionalCSS
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionStyleAdditionalCSS"), @"");
            }
            set
            {
                SetValue("SectionStyleAdditionalCSS", value);
            }
        }


        /// <summary>
        /// Section Theme, controls font and background coloring.
        /// 
        /// The color is usually the background, and the font color is white/black depending on the theme.
        /// </summary>
        public string SectionTheme
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionTheme"), @"None");
            }
            set
            {
                SetValue("SectionTheme", value);
            }
        }


        /// <summary>
        /// Can add a contrast box over the individual text elements or the entire background to make text easier to read when the background may have light/dark areas.
        /// </summary>
        public string SectionThemeContrastBoxMode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionThemeContrastBoxMode"), @"None");
            }
            set
            {
                SetValue("SectionThemeContrastBoxMode", value);
            }
        }


        /// <summary>
        /// If checked, will show a divider between the sections.
        /// </summary>
        public bool SectionShowDivider
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("SectionShowDivider"), false);
            }
            set
            {
                SetValue("SectionShowDivider", value);
            }
        }


        /// <summary>
        /// Image.
        /// </summary>
        public string SectionImageUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionImageUrl"), @"");
            }
            set
            {
                SetValue("SectionImageUrl", value);
            }
        }


        /// <summary>
        /// VIdeo source.
        /// 
        /// Html5Video allows setting of mp4, webm, and/or ogg with a thumbnail.
        /// 
        /// Youtube you should put the media ID (ex for https://www.youtube.com/watch?v=xCnxsoXtlmY, the media ID is xCnxsoXtlmY)
        /// 
        /// For JWPlayer use the Html5Video type and get the values by: 
        /// 1: Log into JW PLayer
        /// 2: Select Media Library and select your video
        /// 3: Select "View Assets" on the right
        /// 4: Hit the down arrow on the Video Rendition you wish to use (smaller size better)
        /// 5: Copy the Thumbnail URL and mp4 stream into the form.
        /// </summary>
        public string SectionVideoType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionVideoType"), @"Html5Video");
            }
            set
            {
                SetValue("SectionVideoType", value);
            }
        }


        /// <summary>
        /// The video ID of the video to use in the background.
        /// </summary>
        public string SectionVideoYoutubeCode
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionVideoYoutubeCode"), @"");
            }
            set
            {
                SetValue("SectionVideoYoutubeCode", value);
            }
        }


        /// <summary>
        /// Video Thumbnail Url.
        /// </summary>
        public string SectionVideoThumbnailUrl
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionVideoThumbnailUrl"), @"");
            }
            set
            {
                SetValue("SectionVideoThumbnailUrl", value);
            }
        }


        /// <summary>
        /// MP4 Stream Url.
        /// </summary>
        public string SectionVideoUrlMp4
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionVideoUrlMp4"), @"");
            }
            set
            {
                SetValue("SectionVideoUrlMp4", value);
            }
        }


        /// <summary>
        /// WebM Stream Url.
        /// </summary>
        public string SectionVideoUrlWebM
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionVideoUrlWebM"), @"");
            }
            set
            {
                SetValue("SectionVideoUrlWebM", value);
            }
        }


        /// <summary>
        /// Ogg Stream Url.
        /// </summary>
        public string SectionVideoUrlOgg
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SectionVideoUrlOgg"), @"");
            }
            set
            {
                SetValue("SectionVideoUrlOgg", value);
            }
        }


        /// <summary>
        /// Gets an object that provides extended API for working with SectionFeature fields.
        /// </summary>
        [RegisterProperty]
        public SectionFeatureFields Fields
        {
            get
            {
                return mFields;
            }
        }


        /// <summary>
        /// Provides extended API for working with SectionFeature fields.
        /// </summary>
        [RegisterAllProperties]
        public partial class SectionFeatureFields : AbstractHierarchicalObject<SectionFeatureFields>
        {
            /// <summary>
            /// The content item of type SectionFeature that is a target of the extended API.
            /// </summary>
            private readonly SectionFeature mInstance;


            /// <summary>
            /// Initializes a new instance of the <see cref="SectionFeatureFields" /> class with the specified content item of type SectionFeature.
            /// </summary>
            /// <param name="instance">The content item of type SectionFeature that is a target of the extended API.</param>
            public SectionFeatureFields(SectionFeature instance)
            {
                mInstance = instance;
            }


            /// <summary>
            /// SectionFeatureID.
            /// </summary>
            public int ID
            {
                get
                {
                    return mInstance.SectionFeatureID;
                }
                set
                {
                    mInstance.SectionFeatureID = value;
                }
            }


            /// <summary>
            /// Displayed above the section.
            /// </summary>
            public string FeatureHeading
            {
                get
                {
                    return mInstance.FeatureHeading;
                }
                set
                {
                    mInstance.FeatureHeading = value;
                }
            }


            /// <summary>
            /// Displayed above the entire section.
            /// </summary>
            public string FeatureSubHeading
            {
                get
                {
                    return mInstance.FeatureSubHeading;
                }
                set
                {
                    mInstance.FeatureSubHeading = value;
                }
            }


            /// <summary>
            /// Is Section.
            /// </summary>
            public bool IsSection
            {
                get
                {
                    return mInstance.IsSection;
                }
                set
                {
                    mInstance.IsSection = value;
                }
            }


            /// <summary>
            /// How the section should render.
            /// 
            /// Default = No styling.
            /// 
            /// Color = Solid color (based on Theme)
            /// 
            /// Image = Image background
            /// 
            /// Parallax Image = Image background with parallax effect
            /// 
            /// Video = Video background.
            /// </summary>
            public string SectionStyleType
            {
                get
                {
                    return mInstance.SectionStyleType;
                }
                set
                {
                    mInstance.SectionStyleType = value;
                }
            }


            /// <summary>
            /// Classes to add to the section.
            /// </summary>
            public string SectionStyleAdditionalCSS
            {
                get
                {
                    return mInstance.SectionStyleAdditionalCSS;
                }
                set
                {
                    mInstance.SectionStyleAdditionalCSS = value;
                }
            }


            /// <summary>
            /// Section Theme, controls font and background coloring.
            /// 
            /// The color is usually the background, and the font color is white/black depending on the theme.
            /// </summary>
            public string SectionTheme
            {
                get
                {
                    return mInstance.SectionTheme;
                }
                set
                {
                    mInstance.SectionTheme = value;
                }
            }


            /// <summary>
            /// Can add a contrast box over the individual text elements or the entire background to make text easier to read when the background may have light/dark areas.
            /// </summary>
            public string SectionThemeContrastBoxMode
            {
                get
                {
                    return mInstance.SectionThemeContrastBoxMode;
                }
                set
                {
                    mInstance.SectionThemeContrastBoxMode = value;
                }
            }


            /// <summary>
            /// If checked, will show a divider between the sections.
            /// </summary>
            public bool SectionShowDivider
            {
                get
                {
                    return mInstance.SectionShowDivider;
                }
                set
                {
                    mInstance.SectionShowDivider = value;
                }
            }


            /// <summary>
            /// Image.
            /// </summary>
            public string SectionImageUrl
            {
                get
                {
                    return mInstance.SectionImageUrl;
                }
                set
                {
                    mInstance.SectionImageUrl = value;
                }
            }


            /// <summary>
            /// VIdeo source.
            /// 
            /// Html5Video allows setting of mp4, webm, and/or ogg with a thumbnail.
            /// 
            /// Youtube you should put the media ID (ex for https://www.youtube.com/watch?v=xCnxsoXtlmY, the media ID is xCnxsoXtlmY)
            /// 
            /// For JWPlayer use the Html5Video type and get the values by: 
            /// 1: Log into JW PLayer
            /// 2: Select Media Library and select your video
            /// 3: Select "View Assets" on the right
            /// 4: Hit the down arrow on the Video Rendition you wish to use (smaller size better)
            /// 5: Copy the Thumbnail URL and mp4 stream into the form.
            /// </summary>
            public string SectionVideoType
            {
                get
                {
                    return mInstance.SectionVideoType;
                }
                set
                {
                    mInstance.SectionVideoType = value;
                }
            }


            /// <summary>
            /// The video ID of the video to use in the background.
            /// </summary>
            public string SectionVideoYoutubeCode
            {
                get
                {
                    return mInstance.SectionVideoYoutubeCode;
                }
                set
                {
                    mInstance.SectionVideoYoutubeCode = value;
                }
            }


            /// <summary>
            /// Video Thumbnail Url.
            /// </summary>
            public string SectionVideoThumbnailUrl
            {
                get
                {
                    return mInstance.SectionVideoThumbnailUrl;
                }
                set
                {
                    mInstance.SectionVideoThumbnailUrl = value;
                }
            }


            /// <summary>
            /// MP4 Stream Url.
            /// </summary>
            public string SectionVideoUrlMp4
            {
                get
                {
                    return mInstance.SectionVideoUrlMp4;
                }
                set
                {
                    mInstance.SectionVideoUrlMp4 = value;
                }
            }


            /// <summary>
            /// WebM Stream Url.
            /// </summary>
            public string SectionVideoUrlWebM
            {
                get
                {
                    return mInstance.SectionVideoUrlWebM;
                }
                set
                {
                    mInstance.SectionVideoUrlWebM = value;
                }
            }


            /// <summary>
            /// Ogg Stream Url.
            /// </summary>
            public string SectionVideoUrlOgg
            {
                get
                {
                    return mInstance.SectionVideoUrlOgg;
                }
                set
                {
                    mInstance.SectionVideoUrlOgg = value;
                }
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the <see cref="SectionFeature" /> class.
        /// </summary>
        public SectionFeature() : base(CLASS_NAME)
        {
            mFields = new SectionFeatureFields(this);
        }

        #endregion
    }
}