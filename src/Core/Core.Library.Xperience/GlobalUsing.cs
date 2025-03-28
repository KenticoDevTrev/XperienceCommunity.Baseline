// System
global using System;
global using System.Linq;
global using System.Threading;
global using CSharpFunctionalExtensions;

// Core
global using Core.Repositories;
global using Core.Services;
global using Core.Models;
global using Core.Enums;
global using Core.Interfaces;
global using MVCCaching;

global using CMS.ContentEngine;
global using CMS.DataEngine;
global using CMS.Helpers;

global using XperienceCommunity.QueryExtensions.Objects;
global using XperienceCommunity.QueryExtensions.Collections;
global using XperienceCommunity.QueryExtensions.ContentItems;
global using Result = CSharpFunctionalExtensions.Result;
// Kentico's new ICacheDependencyBuilderFactor is currently incompatible with Cache Scope, waiting on Kentico to add hooks to allow replacing.
global using ICacheDependencyBuilderFactory = MVCCaching.ICacheDependencyBuilderFactory;